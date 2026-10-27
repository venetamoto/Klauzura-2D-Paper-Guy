using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;

// Greybox version of the plan: every item is a colored rectangle (like the original osnova).
public static class Rects
{
    public record R(string Name, string Kind, float X, float Y, float W, float H, string Color);

    const string Pink = "1f, 0.4962f, 0.4962f";
    const string Dark = "0.1396f, 0.1188f, 0.1188f";
    const string Blue1 = "0f, 0.7316f, 0.9774f";
    const string Blue2 = "0.1791f, 0.3772f, 0.6830f";
    const string Green = "0f, 1f, 0.191f";

    public static List<R> Build(string dir)
    {
        var list = new List<R>();
        int edible = 0;
        foreach (var it in Plan.Items())
        {
            string n = it.Name;
            switch (it.Kind)
            {
                case "T":
                {
                    var s = Plan.Top(dir, it);
                    list.Add(new R("text_platform_" + n.Replace("Text_", ""), "T", (s.X1 + s.X2) / 2, (s.Y + s.Bottom) / 2, s.X2 - s.X1, s.Y - s.Bottom, Pink));
                    break;
                }
                case "D": case "C": case "TRI":
                {
                    var sz = Plan.Size(dir, it);
                    float w = sz.Width, h = sz.Height;
                    if (it.Kind == "C") { var s = Plan.Top(dir, it); float r = s.Y - it.Y; w = h = 2 * r; }
                    list.Add(new R("Drawing_platforms_" + n.Replace("Kresba_", "").Replace("Shape_", ""), "D", it.X, it.Y, w, h, Dark));
                    break;
                }
                case "DECO":
                {
                    var sz = Plan.Size(dir, it);
                    if (it.Name == "Osa_Y") list.Add(new R("Osa_Y", "DECO", Plan.Graph.X, Plan.Graph.Y - 2.4f, 0.12f, 6.4f, Dark));
                    else list.Add(new R("Drawing_" + n.Replace("Kresba_", ""), "DECO", it.X, it.Y, sz.Width, sz.Height, Dark));
                    break;
                }
                case "AXX":
                {
                    var s = Plan.Top(dir, it);
                    list.Add(new R("Osa_X", "D", (s.X1 + s.X2) / 2 + 0.1f, (s.Y + s.Bottom) / 2, s.X2 - s.X1 + 0.2f, 0.15f, Dark));
                    break;
                }
                case "E":
                    edible++;
                    list.Add(new R("Edible_" + edible, "E", it.X, it.Y, 0.8f, 0.8f, edible % 2 == 0 ? Blue2 : Blue1));
                    break;
                case "GREEN":
                    list.Add(new R("Zeleny_Pruh", "GREEN", it.X, it.Y, 5.1f, 0.5f, Green));
                    break;
                case "WALL": case "DOOR":
                    list.Add(new R(n, it.Kind, it.X, it.Y, it.Vertical ? 0.2f : it.Len, it.Vertical ? it.Len : 0.2f, Dark));
                    break;
                case "HAND":
                    list.Add(new R(n, "HAND", it.X, it.Y, 1.6f, 1.5f, Blue1));
                    break;
            }
        }

        // vic textu (radky jako v sesite)
        void T(string name, float x, float y, float w, float h = 0.5f) => list.Add(new R("text_platform_" + name, "T", x, y, w, h, Pink));
        void E(float x, float y) { edible++; list.Add(new R("Edible_" + edible, "E", x, y, 0.8f, 0.8f, edible % 2 == 0 ? Blue2 : Blue1)); }

        T("Nahore1", 22.5f, 20.4f, 6f);
        T("Nahore2", 26.5f, 22.6f, 6f);
        T("Nahore3", 24.0f, 18.0f, 7f);
        T("Vpravo1", 42.5f, 10.5f, 5f);
        T("Vpravo2", 44.0f, 13.0f, 5f);
        T("Dole1", 16.0f, -2.2f, 6f);
        T("Dole2", 12.0f, 0.1f, 6f);
        E(22.0f, 21.6f);
        E(27.5f, 23.9f);
        E(25.5f, 19.3f);
        E(43.0f, 14.4f);
        E(41.5f, 11.8f);
        E(16.0f, -0.9f);
        E(12.0f, 1.4f);
        return list;
    }

    // ---------- reachability (rectangles) ----------
    public static void Check(string dir)
    {
        var rects = Build(dir);
        var surfs = rects.Where(r => r.Kind is "T" or "D" or "GREEN" or "WALL")
            .Where(r => !r.Name.StartsWith("Stena"))
            .Select(r => new Plan.Surf(r.Name, r.X - r.W / 2, r.X + r.W / 2, r.Y + r.H / 2, r.Y - r.H / 2)).ToList();
        surfs.Add(new Plan.Surf("FLOOR", -16.5f, 49.5f, Plan.FloorTop, -6f));

        var reached = new HashSet<string>();
        var queue = new Queue<Plan.Surf>();
        var start = surfs.First(s => s.Name == "text_platform_Ukol");
        reached.Add(start.Name); queue.Enqueue(start);
        var graphStart = new Plan.Surf("GRAPH", Plan.Graph.X, Plan.Graph.X + 0.6f, Plan.Graph.Y, Plan.Graph.Y - 0.2f);
        void Visit(Plan.Surf s) { if (reached.Add(s.Name)) queue.Enqueue(s); }

        while (queue.Count > 0)
        {
            var a = queue.Dequeue();
            if (a.Name == "GRAPH") { Visit(surfs.First(s => s.Name == "Osa_X")); continue; }
            if (a.Name == "Zeleny_Pruh") Visit(surfs.First(s => s.Name == "Podlaha_Mistnosti"));
            if (a.Name == "Podlaha_Mistnosti") { Visit(surfs.First(s => s.Name == "Rimsa")); continue; }
            foreach (var b in surfs.Append(graphStart))
            {
                if (b == a || b.Name == "Podlaha_Mistnosti") continue;
                float rise = b.Y - a.Y;
                float gap = Math.Max(0, Math.Max(b.X1 - a.X2, a.X1 - b.X2));
                bool ok = rise <= 0 ? gap <= 3.2f + 0.7f * -rise : rise <= 2.45f && gap <= 3.2f - rise * 0.35f;
                if (ok) Visit(b);
            }
        }
        foreach (var s in surfs.Where(s => !reached.Contains(s.Name))) Console.WriteLine($"NEDOSAZITELNE: {s.Name}");
        foreach (var a in surfs) foreach (var b in surfs)
        {
            if (a == b || a.Name == "FLOOR") continue;
            if (!(a.X1 < b.X2 && b.X1 < a.X2) || b.Bottom <= a.Y) continue;
            if (b.Bottom - a.Y < 1.3f) Console.WriteLine($"MALO MISTA: nad {a.Name} je {b.Name} jen {b.Bottom - a.Y:F2}");
        }
        foreach (var e in rects.Where(r => r.Kind == "E"))
        {
            bool ok = surfs.Where(s => reached.Contains(s.Name)).Any(s => e.X >= s.X1 - 1.2f && e.X <= s.X2 + 1.2f && e.Y - s.Y >= -0.2f && e.Y - s.Y <= 3.2f);
            for (float u = 0; u <= 3f && !ok; u += 0.05f)
            {
                float cx = Plan.Graph.X + u * Plan.GraphScale.X, cy = Plan.Graph.Y - u * u * Plan.GraphScale.Y + 0.5f;
                if (Math.Abs(cx - e.X) < 0.7f && Math.Abs(cy - e.Y) < 0.9f) ok = true;
            }
            if (!ok) Console.WriteLine($"JIDLO NEDOSAZITELNE: {e.Name} ({e.X},{e.Y})");
        }
        Console.WriteLine($"plochy: {reached.Count - 1}/{surfs.Count}, jidla: {rects.Count(r => r.Kind == "E")}, texty: {rects.Count(r => r.Kind == "T")}");
    }

    public static void Preview(string dir, string outPath)
    {
        float ppu = 26f, minX = -17.5f, maxX = 50.5f, minY = -5.6f, maxY = 25.4f;
        int W = (int)((maxX - minX) * ppu), H = (int)((maxY - minY) * ppu);
        using var bmp = new Bitmap(W, H);
        using var g = Graphics.FromImage(bmp);
        g.Clear(System.Drawing.Color.White);
        PointF S(float x, float y) => new((x - minX) * ppu, (maxY - y) * ppu);
        System.Drawing.Color Parse(string c)
        {
            var p = c.Replace("f", "").Split(',').Select(v => float.Parse(v.Trim(), CultureInfo.InvariantCulture)).ToArray();
            return System.Drawing.Color.FromArgb(220, (int)(p[0] * 255), (int)(p[1] * 255), (int)(p[2] * 255));
        }
        using (var floor = new SolidBrush(System.Drawing.Color.FromArgb(90, 0, 0, 0))) { var a = S(minX, Plan.FloorTop); g.FillRectangle(floor, a.X, a.Y, W, 0.6f * ppu); }
        foreach (var r in Build(dir))
        {
            var c = S(r.X - r.W / 2, r.Y + r.H / 2);
            using var b = new SolidBrush(r.Kind == "DOOR" ? System.Drawing.Color.Orange : Parse(r.Color));
            if (r.Kind == "HAND") { using var p = new Pen(b, 3f); g.DrawEllipse(p, c.X, c.Y, r.W * ppu, r.H * ppu); continue; }
            g.FillRectangle(b, c.X, c.Y, Math.Max(2f, r.W * ppu), Math.Max(2f, r.H * ppu));
        }
        var pts = new List<PointF>();
        for (int i = 0; i <= 40; i++) { float u = 3f * i / 40f; pts.Add(S(Plan.Graph.X + u * Plan.GraphScale.X, Plan.Graph.Y - u * u * Plan.GraphScale.Y)); }
        using (var pen = new Pen(System.Drawing.Color.Black, 3f)) g.DrawLines(pen, pts.ToArray());
        var ps = S(Plan.PlayerStart.X - 0.4f, Plan.PlayerStart.Y + 0.5f);
        g.FillRectangle(Brushes.Black, ps.X, ps.Y, 0.8f * ppu, ppu);
        bmp.Save(outPath);
    }

    public static string Export(string dir)
    {
        var inv = CultureInfo.InvariantCulture;
        var sb = new StringBuilder();
        foreach (var r in Build(dir))
            sb.AppendLine(string.Format(inv, "        new Item(\"{0}\", \"{1}\", {2}f, {3}f, {4}f, {5}f, new Color({6})),",
                r.Name, r.Kind, Math.Round(r.X, 2), Math.Round(r.Y, 2), Math.Round(r.W, 2), Math.Round(r.H, 2), r.Color));
        return sb.ToString();
    }
}
