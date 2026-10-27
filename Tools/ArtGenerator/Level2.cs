using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;

// Varvara v2: wider level, bigger graph and room, smaller text bars, hand-drawn arrows, exit to VarvaraKonec.
public static class Level2
{
    public record R(string Name, string Kind, float X, float Y, float W, float H);
    public record Arrow(string Name, float X1, float Y1, float X2, float Y2, float ZoneX, float ZoneY, float ZoneW, float ZoneH);

    public const float FloorTop = -5f, Ceiling = 24.84f, LeftX = -16.54f, RightX = 63.5f;
    public static readonly (float X, float Y) Graph = (8.0f, 15.5f);
    public static readonly (float X, float Y) GraphScale = (3.4f, 1.0f);
    public static readonly (float X, float Y) PlayerStart = (-15.0f, -4.4f);

    const float TH = 0.35f; // vyska textoveho pruhu

    public static List<R> Build()
    {
        var l = new List<R>();
        void T(string n, float x, float top, float w) => l.Add(new R("text_platform_" + n, "T", x, top - TH / 2, w, TH));
        void D(string n, float x, float y, float w, float h) => l.Add(new R("Drawing_platforms_" + n, "D", x, y, w, h));
        int e = 0;
        void E(float x, float y) => l.Add(new R("Edible_" + (++e), "E", x, y, 0.7f, 0.7f));

        // ---------- leva stranka ----------
        T("DU", -12.0f, -2.7f, 5f);
        T("Reseni", -6.0f, -0.4f, 5f);
        T("Odpoved", -12.0f, 1.9f, 5f);
        T("Rovnice", -6.0f, 4.2f, 4.5f);
        T("Kruh", -12.0f, 6.5f, 4.5f);
        T("Vypocitej", -6.5f, 8.8f, 3f);
        T("Ukol", -2.5f, 11.15f, 3f);
        T("Zadani", 3.5f, 13.45f, 5f);
        T("Pozor", 0.0f, 15.75f, 4.5f);
        T("a4", -5.0f, 18.05f, 2.5f);
        T("Ukol2", -11.0f, 20.35f, 5f);
        T("Dole2", 1.0f, 1.0f, 3.5f);
        T("Dole3", 3.5f, 6.6f, 3f);
        l.Add(new R("Drawing_Dum", "DECO", -14.3f, 11.2f, 3.42f, 3.87f));
        D("Krychle", -14.6f, 16.8f, 2.93f, 2.93f);
        D("Pravitko", -3.0f, 21.9f, 8f, 1.42f);
        D("Slunce", 4.8f, 20.3f, 3.08f, 3.08f);
        D("Mrak", 10.5f, 21.3f, 3.87f, 1.58f);

        // ---------- graf ----------
        float axisY = Graph.Y - 9f * GraphScale.Y;
        l.Add(new R("Osa_Y", "DECO", Graph.X, (axisY + Graph.Y + 1.5f) / 2f, 0.12f, Graph.Y + 1.5f - axisY));
        l.Add(new R("Osa_X", "D", Graph.X + 5.5f, axisY - 0.075f, 11f, 0.15f));

        // ---------- nahore uprostred ----------
        T("Nahore3", 16.5f, 19.8f, 4f);
        T("Nahore1", 21.5f, 22.1f, 4f);
        T("Nahore2", 27.0f, 23.2f, 4f);
        T("Pi", 32.5f, 21.05f, 3f);

        // ---------- stred po sjezdu ----------
        T("Pyth", 22.5f, 5.2f, 5f);
        T("x4", 27.5f, 7.5f, 2f);
        T("Ukol3", 32.0f, 9.8f, 5f);
        T("r2", 37.0f, 12.1f, 2.5f);
        D("Mrak (2)", 41.5f, 13.4f, 4.35f, 1.78f);
        T("Vpravo2", 46.5f, 16.55f, 4f);
        T("Hotovo", 51.5f, 18.85f, 2.75f);
        T("Trida", 32.5f, 5.2f, 4.5f);
        T("Schody1", 37.5f, 4.35f, 3f);
        T("Schody2", 41.0f, 5.15f, 2.5f);

        // ---------- mistnost (vetsi ctverec) ----------
        l.Add(new R("Zeleny_Pruh", "GREEN", 48.05f, 4.75f, 9.9f, 0.5f));
        l.Add(new R("Stena_Leva", "WALL", 43.0f, -0.25f, 0.2f, 9.5f));
        l.Add(new R("Stena_Prava", "WALL", 53.1f, (Ceiling - 2.8f) / 2f, 0.2f, Ceiling + 2.8f));
        l.Add(new R("Dvere_Stena", "DOOR", 53.1f, -3.9f, 0.2f, 2.2f));

        // ---------- dole ----------
        D("CiselnaOsa", 12.0f, -4.6f, 10.23f, 0.75f);
        T("Dole1", 16.0f, -2.0f, 4f);
        D("Ctverec", 24.0f, -3.82f, 2.36f, 2.35f);
        D("Trojuhelnik", 28.0f, -3.75f, 3.11f, 2.51f);
        D("Kruh", 31.5f, -1.27f, 1.94f, 1.94f);
        D("Sestiuhelnik", 35.0f, 1.05f, 2.16f, 1.89f);
        D("Lichobeznik", 39.0f, 1.6f, 2.89f, 1.32f);
        l.Add(new R("Hand_Drawer", "HAND", 19.0f, -3.99f, 1.6f, 1.5f));

        // ---------- jidlo ----------
        E(-13.5f, -1.6f); E(-4.5f, 0.7f); E(-13.0f, 3.0f); E(-4.8f, 5.3f); E(-10.8f, 7.6f);
        E(-7.0f, 9.9f); E(1.0f, 2.2f); E(3.5f, 7.7f); E(5.0f, 14.6f); E(-1.5f, 16.9f);
        E(-12.5f, 21.5f); E(-14.6f, 18.9f); E(-5.0f, 23.4f); E(10.5f, 23.0f);
        float[] us = { 0.8f, 1.6f, 2.3f, 2.8f };
        foreach (float u in us) E(Graph.X + u * GraphScale.X, Graph.Y - u * u * GraphScale.Y + 0.6f);
        E(18.0f, 21.0f); E(27.0f, 24.2f); E(21.0f, 6.4f); E(27.5f, 8.7f); E(33.5f, 11.0f);
        E(41.5f, 15.4f); E(51.5f, 20.1f); E(35.0f, 6.4f); E(41.0f, 6.4f);
        E(46.0f, -4.2f); E(50.0f, -4.2f); E(57.0f, -4.2f);
        E(12.0f, -3.4f); E(16.0f, -0.8f); E(24.0f, -1.6f); E(31.5f, 0.6f); E(39.0f, 3.0f);

        // ---------- prechod do posledni urovne ----------
        l.Add(new R("Prechod_Konec", "EXIT", 54.0f, -3.9f, 1.2f, 2.2f));
        return l;
    }

    public static List<Arrow> Arrows() => new()
    {
        new("Sipka_Start", -15.3f, -3.3f, -13.4f, -2.2f, -14.5f, -3.8f, 4f, 2.5f),
        new("Sipka_Schody", -9.0f, 2.7f, -7.2f, 3.7f, -11.0f, 2.6f, 5f, 2f),
        new("Sipka_Nahoru", -1.0f, 12.4f, 0.9f, 13.4f, -2.5f, 12.3f, 5f, 3f),
        new("Sipka_Graf", 6.0f, 17.9f, 7.8f, 16.5f, 4.0f, 15.0f, 6f, 4f),
        new("Sipka_Doprava", 23.0f, 6.3f, 25.6f, 7.3f, 22.0f, 6.2f, 6f, 3f),
        new("Sipka_Dolu", 34.0f, 6.3f, 36.2f, 5.5f, 32.5f, 6.2f, 5f, 3f),
        new("Sipka_Pruh", 46.5f, 7.6f, 46.5f, 5.7f, 43.0f, 6.6f, 5f, 3f),
    };

    // ---------- reachability ----------
    public static void Check()
    {
        var rects = Build();
        var surfs = rects.Where(r => r.Kind is "T" or "D" or "GREEN")
            .Select(r => new Plan.Surf(r.Name, r.X - r.W / 2, r.X + r.W / 2, r.Y + r.H / 2, r.Y - r.H / 2)).ToList();
        surfs.Add(new Plan.Surf("FLOOR_L", LeftX, 42.9f, FloorTop, -6f));
        surfs.Add(new Plan.Surf("FLOOR_ROOM", 43.1f, 53.0f, FloorTop, -6f));
        surfs.Add(new Plan.Surf("FLOOR_R", 53.2f, RightX, FloorTop, -6f));

        var reached = new HashSet<string>();
        var q = new Queue<Plan.Surf>();
        var start = surfs.First(s => s.Name == "FLOOR_L");
        reached.Add(start.Name); q.Enqueue(start);
        var g = new Plan.Surf("GRAPH", Graph.X, Graph.X + 0.6f, Graph.Y, Graph.Y - 0.2f);
        void V(Plan.Surf s) { if (reached.Add(s.Name)) q.Enqueue(s); }

        while (q.Count > 0)
        {
            var a = q.Dequeue();
            if (a.Name == "GRAPH") { V(surfs.First(s => s.Name == "Drawing_platforms_Osa_X" || s.Name == "Osa_X")); continue; }
            if (a.Name == "Zeleny_Pruh") V(surfs.First(s => s.Name == "FLOOR_ROOM"));
            if (a.Name == "FLOOR_ROOM") { V(surfs.First(s => s.Name == "FLOOR_R")); continue; }
            foreach (var b in surfs.Append(g))
            {
                if (b == a || b.Name is "FLOOR_ROOM" or "FLOOR_R") continue;
                bool insideRoomX = b.X1 >= 43f && b.X2 <= 53.1f && b.Y < 4.5f;
                if (insideRoomX) continue;
                if (a.Name == "FLOOR_R" || a.Name == "FLOOR_ROOM") continue;
                float rise = b.Y - a.Y;
                float gap = Math.Max(0, Math.Max(b.X1 - a.X2, a.X1 - b.X2));
                // steny mistnosti: nelze skocit pres pravou stenu (x 53.1) a do mistnosti zboku
                bool crossesRight = (a.X2 < 53.1f) != (b.X1 < 53.1f) && Math.Max(a.Y, b.Y) < Ceiling;
                if (crossesRight) continue;
                bool ok = rise <= 0 ? gap <= 3.2f + 0.7f * -rise : rise <= 2.4f && gap <= 3.2f - rise * 0.35f;
                if (ok) V(b);
            }
        }
        foreach (var s in surfs.Where(s => !reached.Contains(s.Name))) Console.WriteLine($"NEDOSAZITELNE: {s.Name}");
        foreach (var e in rects.Where(r => r.Kind == "E"))
        {
            bool ok = surfs.Where(s => reached.Contains(s.Name)).Any(s => e.X >= s.X1 - 1.2f && e.X <= s.X2 + 1.2f && e.Y - s.Y >= -0.2f && e.Y - s.Y <= 3.0f);
            for (float u = 0; u <= 3f && !ok; u += 0.05f)
            {
                float cx = Graph.X + u * GraphScale.X, cy = Graph.Y - u * u * GraphScale.Y + 0.5f;
                if (Math.Abs(cx - e.X) < 0.7f && Math.Abs(cy - e.Y) < 0.9f) ok = true;
            }
            if (!ok) Console.WriteLine($"JIDLO NEDOSAZITELNE: {e.Name} ({e.X},{e.Y})");
            if (e.Y + 0.35f > Ceiling) Console.WriteLine($"JIDLO VE STROPE: {e.Name}");
        }
        Console.WriteLine($"plochy {reached.Count - 1}/{surfs.Count}, jidla {rects.Count(r => r.Kind == "E")}, texty {rects.Count(r => r.Kind == "T")}");
    }

    public static void Preview(string outPath)
    {
        float ppu = 22f, minX = LeftX - 1f, maxX = RightX + 1f, minY = -5.6f, maxY = 25.4f;
        int W = (int)((maxX - minX) * ppu), H = (int)((maxY - minY) * ppu);
        using var bmp = new Bitmap(W, H);
        using var gr = Graphics.FromImage(bmp);
        gr.Clear(Color.White);
        gr.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
        PointF S(float x, float y) => new((x - minX) * ppu, (maxY - y) * ppu);
        using (var f = new SolidBrush(Color.FromArgb(90, 0, 0, 0))) { var a = S(minX, FloorTop); gr.FillRectangle(f, a.X, a.Y, W, 0.6f * ppu); }
        int ei = 0;
        foreach (var r in Build())
        {
            Color c = r.Kind switch
            {
                "T" => Color.FromArgb(255, 127, 127), "E" => (++ei % 2 == 0) ? Color.FromArgb(46, 96, 174) : Color.FromArgb(0, 187, 249),
                "GREEN" => Color.FromArgb(0, 255, 49), "DOOR" => Color.Orange, "EXIT" => Color.FromArgb(80, 160, 0, 200),
                _ => Color.FromArgb(36, 30, 30)
            };
            var p = S(r.X - r.W / 2, r.Y + r.H / 2);
            using var b = new SolidBrush(c);
            if (r.Kind == "HAND") { using var pen = new Pen(Color.FromArgb(0, 187, 249), 3); gr.DrawEllipse(pen, p.X, p.Y, r.W * ppu, r.H * ppu); continue; }
            gr.FillRectangle(b, p.X, p.Y, Math.Max(2, r.W * ppu), Math.Max(2, r.H * ppu));
        }
        var pts = new List<PointF>();
        for (int i = 0; i <= 40; i++) { float u = 3f * i / 40f; pts.Add(S(Graph.X + u * GraphScale.X, Graph.Y - u * u * GraphScale.Y)); }
        using (var pen = new Pen(Color.Blue, 3)) gr.DrawLines(pen, pts.ToArray());
        foreach (var a in Arrows())
            foreach (var poly in ArrowPolyline(a))
            {
                using var pen = new Pen(Color.FromArgb(60, 60, 70), 2.5f);
                gr.DrawLines(pen, poly.Select(v => S(v.X, v.Y)).ToArray());
            }
        var ps = S(PlayerStart.X - 0.4f, PlayerStart.Y + 0.5f);
        gr.FillRectangle(Brushes.Black, ps.X, ps.Y, 0.8f * ppu, ppu);
        // camera view (ortho 7.5) at start
        using (var cam = new Pen(Color.FromArgb(120, 255, 0, 0), 2)) { var c0 = S(PlayerStart.X - 13.3f, PlayerStart.Y + 1 + 7.5f); gr.DrawRectangle(cam, c0.X, c0.Y, 26.6f * ppu, 15f * ppu); }
        bmp.Save(outPath);
    }

    // hand-drawn arrow: wobbly shaft + two head strokes, as one polyline (head drawn by going back over the tip)
    public static List<List<(float X, float Y)>> ArrowPolyline(Arrow a)
    {
        var rnd = new Random(a.Name.Length * 31 + (int)(a.X1 * 7));
        float dx = a.X2 - a.X1, dy = a.Y2 - a.Y1, len = MathF.Sqrt(dx * dx + dy * dy);
        float ux = dx / len, uy = dy / len, nx = -uy, ny = ux;
        var shaft = new List<(float, float)>();
        int n = 8;
        for (int i = 0; i <= n; i++)
        {
            float t = i / (float)n;
            float off = (i == 0 || i == n) ? 0f : (MathF.Sin(t * 5.5f + a.X1) * 0.06f + ((float)rnd.NextDouble() - 0.5f) * 0.05f);
            shaft.Add((a.X1 + dx * t + nx * off, a.Y1 + dy * t + ny * off));
        }
        float hl = 0.5f;
        (float, float) H(float ang)
        {
            float c = MathF.Cos(ang), s = MathF.Sin(ang);
            float bx = -ux * c + -uy * -s, by = -ux * s + -uy * c;
            return (a.X2 + bx * hl + ((float)rnd.NextDouble() - 0.5f) * 0.05f, a.Y2 + by * hl + ((float)rnd.NextDouble() - 0.5f) * 0.05f);
        }
        var all = new List<(float, float)>(shaft) { H(0.55f), (a.X2, a.Y2), H(-0.5f) };
        return new List<List<(float X, float Y)>> { all };
    }

    public static string Export()
    {
        var inv = CultureInfo.InvariantCulture;
        var sb = new StringBuilder();
        sb.AppendLine("    static readonly Item[] Items =");
        sb.AppendLine("    {");
        foreach (var r in Build())
            sb.AppendLine(string.Format(inv, "        new Item(\"{0}\", \"{1}\", {2}f, {3}f, {4}f, {5}f),", r.Name, r.Kind, r.X, r.Y, r.W, r.H));
        sb.AppendLine("    };");
        sb.AppendLine();
        sb.AppendLine("    static readonly ArrowData[] ArrowList =");
        sb.AppendLine("    {");
        foreach (var a in Arrows())
        {
            var pts = ArrowPolyline(a)[0];
            string p = string.Join(", ", pts.Select(v => string.Format(inv, "new Vector2({0:0.###}f, {1:0.###}f)", v.X - a.X1, v.Y - a.Y1)));
            sb.AppendLine(string.Format(inv, "        new ArrowData(\"{0}\", new Vector2({1}f, {2}f), new Vector2({3}f, {4}f), new Vector2({5}f, {6}f), new[] {{ {7} }}),",
                a.Name, a.X1, a.Y1, a.ZoneX, a.ZoneY, a.ZoneW, a.ZoneH, p));
        }
        sb.AppendLine("    };");
        return sb.ToString();
    }
}
