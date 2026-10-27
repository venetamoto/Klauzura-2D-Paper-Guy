using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

// Level plan for scene Varvara: item list, preview render, reachability check, Unity data export.
public static class Plan
{
    // Kind: T text platform, D drawing (polygon), C circle collider drawing, TRI narrow-top shape,
    //       E edible, DECO no collider, AXX x-axis platform, WALL pencil line with box collider,
    //       GREEN erasable bar, DOOR door segment of the room wall, ROOMFLOOR, HAND star drawer
    public record Item(string Name, string Sprite, string Kind, float X, float Y, float Scale = 1f, float Len = 0f, bool Vertical = false);

    public const float FloorTop = -5f;
    public static readonly Vector Graph = new(10.0f, 15.0f);   // pivot (start of y = -x^2)
    public static readonly Vector GraphScale = new(2.2f, 0.6f);
    public static readonly Vector PlayerStart = new(-6.4f, 11.7f);

    public record struct Vector(float X, float Y);

    const float TS = 1.35f; // meritko textu

    public static List<Item> Items() => new()
    {
        // ---------- vlevo dole: stranka s vetami (schody nahoru ke startu) ----------
        new("Text_DU", "Text_DU", "T", -10.5f, -3.0f, TS),
        new("Text_Reseni", "Text_Reseni", "T", 1.0f, -0.74f, TS),
        new("Text_Odpoved", "Text_Odpoved", "T", -10.0f, 1.53f, TS),
        new("Text_Rovnice", "Text_Rovnice", "T", 0.5f, 3.92f, TS),
        new("Text_Kruh", "Text_Kruh", "T", -8.5f, 6.22f, TS),
        new("Text_Vypocitej", "Text_Vypocitej", "T", -1.5f, 8.42f, TS),
        new("Kresba_Dum", "Kresba_Dum", "DECO", -14.3f, 11.2f, 1.1f),

        // ---------- start a hlavni radek ke grafu ----------
        new("Text_Ukol", "Text_Ukol", "T", -6.4f, 10.56f, TS),
        new("Text_Zadani", "Text_Zadani", "T", 2.0f, 13.0f, TS),
        new("Text_Pozor", "Text_Pozor", "T", -1.0f, 15.31f, TS),

        // ---------- graf ----------
        new("Osa_Y", "Kresba_OsaY", "DECO", 9.7f, 12.9f),
        new("Osa_X", "Kresba_OsaX", "AXX", 13.67f, 9.46f),

        // ---------- nahore vlevo ----------
        new("Text_a4", "Text_a4", "T", -7.0f, 17.75f, TS),
        new("Text_Ukol2", "Text_Ukol2", "T", -11.7f, 19.88f, TS),
        new("Shape_Krychle", "Shape_Krychle", "D", -14.6f, 16.8f, 1.1f),
        new("Kresba_Pravitko", "Kresba_Pravitko", "D", 0.0f, 21.89f),
        new("Kresba_Slunce", "Kresba_Slunce", "C", 10.8f, 19.9f, 1.4f),
        new("Kresba_Mrak", "Kresba_Mrak", "D", 16.0f, 21.0f, 0.8f),

        // ---------- stred ----------
        new("Text_Pyth", "Text_Pyth", "T", 23.0f, 8.25f, TS),
        new("Text_x4", "Text_x4", "T", 29.0f, 10.65f, TS),
        new("Text_Trida", "Text_Trida", "T", 33.5f, 8.2f, TS),
        new("Text_Ukol3", "Text_Ukol3", "T", 25.0f, 12.8f, TS),
        new("Text_r2", "Text_r2", "T", 31.5f, 15.27f, TS),
        new("Kresba_Mrak (2)", "Kresba_Mrak", "D", 35.8f, 16.91f, 0.9f),
        new("Text_Pi", "Text_Pi", "T", 40.5f, 19.83f, TS),
        new("Text_Hotovo", "Text_Hotovo", "T", 45.3f, 22.05f, TS),

        // ---------- dolu k zelenemu pruhu ----------
        new("Text_Schody1", "Text_Schody1", "T", 40.0f, 6.95f, TS),

        // ---------- mistnost (ctverec) ----------
        new("Zeleny_Pruh", "Zeleny_Pruh", "GREEN", 44.9f, 4.75f),
        new("Stena_Leva", "Kresba_Cara", "WALL", 42.3f, 1.135f, 1f, 6.7f, true),
        new("Stena_Prava", "Kresba_Cara", "WALL", 47.5f, 5.9f, 1f, 12.2f, true),
        new("Dvere_Stena", "Kresba_Cara", "DOOR", 47.5f, -1.2f, 1f, 2.0f, true),
        new("Podlaha_Mistnosti", "Kresba_Cara", "WALL", 44.9f, -2.2f, 1f, 5.4f, false),
        new("Rimsa", "Kresba_Cara", "WALL", 48.55f, -2.2f, 1f, 1.9f, false),

        // ---------- dole ----------
        new("Kresba_CiselnaOsa", "Kresba_CiselnaOsa", "D", 12.0f, -4.6f),
        new("Shape_Ctverec", "Shape_Ctverec", "D", 24.0f, -3.82f, 0.9f),
        new("Shape_Trojuhelnik", "Shape_Trojuhelnik", "TRI", 28.0f, -3.75f),
        new("Shape_Kruh", "Shape_Kruh", "C", 31.5f, -1.27f, 0.8f),
        new("Shape_Sestiuhelnik", "Shape_Sestiuhelnik", "D", 35.0f, 1.05f, 0.8f),
        new("Shape_Lichobeznik", "Shape_Lichobeznik", "D", 38.5f, 3.65f),
        new("Hand_Drawer", "", "HAND", 19.0f, -3.99f),

        // ---------- jidlo ----------
        new("Jidlo_4", "Jidlo_4", "E", -13.0f, -1.4f),
        new("Jidlo_Kruh", "Jidlo_Kruh", "E", 5.0f, 0.8f),
        new("Jidlo_5", "Jidlo_5", "E", -13.5f, 3.2f),
        new("Jidlo_Ctverec", "Jidlo_Ctverec", "E", 3.0f, 5.5f),
        new("Jidlo_Hvezda", "Jidlo_Hvezda", "E", -11.0f, 7.8f),
        new("Jidlo_Pi", "Jidlo_Pi", "E", -3.5f, 14.6f),
        new("Jidlo_Plus", "Jidlo_Plus", "E", 12.2f, 15.0f),
        new("Jidlo_Rovna", "Jidlo_Rovna", "E", 14.4f, 13.2f),
        new("Jidlo_Kapka", "Jidlo_Kapka", "E", -14.0f, 21.6f),
        new("Jidlo_9", "Jidlo_9", "E", 2.0f, 23.8f),
        new("Jidlo_Hvezda (2)", "Jidlo_Hvezda", "E", 16.0f, 23.0f),
        new("Jidlo_Kapka (2)", "Jidlo_Kapka", "E", -14.6f, 18.9f),
        new("Jidlo_Kosoctverec", "Jidlo_Kosoctverec", "E", 21.0f, 10.0f),
        new("Jidlo_7", "Jidlo_7", "E", 35.5f, 10.0f),
        new("Jidlo_Ctverec (2)", "Jidlo_Ctverec", "E", 23.0f, 14.5f),
        new("Jidlo_Kapka (3)", "Jidlo_Kapka", "E", 35.8f, 19.3f),
        new("Jidlo_Srdce", "Jidlo_Srdce", "E", 47.8f, 23.6f),
        new("Jidlo_Plus (2)", "Jidlo_Plus", "E", 29.0f, 12.0f),
        new("Jidlo_2", "Jidlo_2", "E", 44.9f, -1.4f),
        new("Jidlo_Kruh (2)", "Jidlo_Kruh", "E", 12.0f, -2.9f),
        new("Jidlo_7 (2)", "Jidlo_7", "E", 24.0f, -1.4f),
        new("Jidlo_Trojuhelnik", "Jidlo_Trojuhelnik", "E", 31.5f, 0.6f),
        new("Jidlo_Kosoctverec (2)", "Jidlo_Kosoctverec", "E", 38.5f, 5.6f),
        new("Jidlo_Rovna (2)", "Jidlo_Rovna", "E", 40.0f, 8.4f),
    };

    // ---------- sizes ----------
    static readonly Dictionary<string, SizeF> _sizes = new();

    public static SizeF Size(string dir, Item it)
    {
        if (it.Kind is "WALL" or "DOOR")
            return it.Vertical ? new SizeF(0.2f, it.Len) : new SizeF(it.Len, 0.2f);
        if (it.Kind == "HAND") return new SizeF(1.6f, 1.5f);
        if (!_sizes.TryGetValue(it.Sprite, out var s))
        {
            using var img = Image.FromFile(Path.Combine(dir, it.Sprite + ".png"));
            s = new SizeF(img.Width / 200f, img.Height / 200f);
            _sizes[it.Sprite] = s;
        }
        return new SizeF(s.Width * it.Scale, s.Height * it.Scale);
    }

    // walkable top surface (x1, x2, y)
    public record Surf(string Name, float X1, float X2, float Y, float Bottom);

    public static Surf Top(string dir, Item it)
    {
        SizeF s = Size(dir, it);
        float hw = s.Width / 2, hh = s.Height / 2;
        switch (it.Kind)
        {
            case "T": return new Surf(it.Name, it.X - hw * 0.95f, it.X + hw * 0.95f, it.Y + hh * 0.9f, it.Y - hh * 0.5f);
            case "C":
                float r = it.Sprite == "Kresba_Slunce" ? 1.1f * it.Scale : hw * 0.97f;
                return new Surf(it.Name, it.X - r * 0.6f, it.X + r * 0.6f, it.Y + r, it.Y - r);
            case "TRI": return new Surf(it.Name, it.X - hw * 0.2f, it.X + hw * 0.2f, it.Y + hh, it.Y - hh);
            case "AXX": return new Surf(it.Name, it.X - hw + 0.1f, it.X + hw - 0.3f, it.Y + 0.14f + 0.075f, it.Y + 0.14f - 0.075f);
            case "D": case "GREEN": case "WALL": case "DOOR":
                return new Surf(it.Name, it.X - hw, it.X + hw, it.Y + hh, it.Y - hh);
        }
        return null;
    }

    // =====================================================================
    //  reachability
    // =====================================================================

    public static void Check(string dir)
    {
        var items = Items();
        var surfs = items.Select(i => Top(dir, i)).Where(s => s != null)
            .Where(s => !s.Name.StartsWith("Stena") && s.Name != "Dvere_Stena").ToList();
        surfs.Add(new Surf("FLOOR", -16.5f, 49.5f, FloorTop, -6f));

        var room = new[] { "Podlaha_Mistnosti" };
        var reached = new HashSet<string>();
        var queue = new Queue<Surf>();
        Surf start = surfs.First(s => s.Name == "Text_Ukol");
        reached.Add(start.Name); queue.Enqueue(start);

        float gx = Graph.X, gy = Graph.Y;
        var graphStart = new Surf("GRAPH", gx, gx + 0.6f, gy, gy - 0.2f);

        void Visit(Surf s) { if (reached.Add(s.Name)) queue.Enqueue(s); }

        while (queue.Count > 0)
        {
            Surf a = queue.Dequeue();
            if (a.Name == "GRAPH") { Visit(surfs.First(s => s.Name == "Osa_X")); continue; }
            if (a.Name == "Zeleny_Pruh") Visit(surfs.First(s => s.Name == "Podlaha_Mistnosti"));
            if (a.Name == "Podlaha_Mistnosti") { Visit(surfs.First(s => s.Name == "Rimsa")); continue; }

            foreach (Surf b in surfs.Append(graphStart))
            {
                if (b == a || room.Contains(b.Name)) continue;
                float rise = b.Y - a.Y;
                float gap = Math.Max(0, Math.Max(b.X1 - a.X2, a.X1 - b.X2));
                bool ok = rise <= 0 ? gap <= 3.2f + 0.7f * -rise : rise <= 2.45f && gap <= 3.2f - rise * 0.35f;
                if (ok) Visit(b);
            }
        }

        foreach (Surf s in surfs.Where(s => !reached.Contains(s.Name)))
            Console.WriteLine($"NEDOSAZITELNE: {s.Name}");

        // headroom between overlapping surfaces
        foreach (Surf a in surfs)
        foreach (Surf b in surfs)
        {
            if (a == b || a.Name == "FLOOR") continue;
            bool overlap = a.X1 < b.X2 && b.X1 < a.X2;
            if (!overlap || b.Bottom <= a.Y) continue;
            float clear = b.Bottom - a.Y;
            if (clear < 1.3f) Console.WriteLine($"MALO MISTA: nad {a.Name} je {b.Name} jen {clear:F2}");
        }

        // edibles
        foreach (Item e in items.Where(i => i.Kind == "E"))
        {
            bool ok = surfs.Where(s => reached.Contains(s.Name))
                .Any(s => e.X >= s.X1 - 1.2f && e.X <= s.X2 + 1.2f && e.Y - s.Y >= -0.2f && e.Y - s.Y <= 3.2f);
            for (float u = 0; u <= 3f && !ok; u += 0.05f)
            {
                float cx = gx + u * GraphScale.X, cy = gy - u * u * GraphScale.Y + 0.5f;
                if (Math.Abs(cx - e.X) < 0.7f && Math.Abs(cy - e.Y) < 0.9f) ok = true;
            }
            if (!ok) Console.WriteLine($"JIDLO NEDOSAZITELNE: {e.Name}");
        }
        Console.WriteLine($"dosazeno ploch: {reached.Count}/{surfs.Count + 1}");
    }

    // =====================================================================
    //  preview
    // =====================================================================

    public static void Preview(string dir, string outPath)
    {
        float ppu = 26f, minX = -17.5f, maxX = 50.5f, minY = -5.6f, maxY = 25.4f;
        int W = (int)((maxX - minX) * ppu), H = (int)((maxY - minY) * ppu);
        using var bmp = new Bitmap(W, H);
        using var g = Graphics.FromImage(bmp);
        g.Clear(Color.White);
        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
        g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
        PointF S(float x, float y) => new((x - minX) * ppu, (maxY - y) * ppu);

        using (var wall = new SolidBrush(Color.FromArgb(70, 0, 0, 0)))
        {
            var a = S(minX, FloorTop); g.FillRectangle(wall, a.X, a.Y, W, 0.6f * ppu);
        }

        foreach (Item it in Items())
        {
            SizeF s = Size(dir, it);
            var c = S(it.X, it.Y);
            if (it.Kind is "WALL" or "DOOR")
            {
                using var pen = new Pen(it.Kind == "DOOR" ? Color.Orange : Color.FromArgb(60, 60, 70), 0.2f * ppu);
                if (it.Vertical) g.DrawLine(pen, c.X, c.Y - it.Len / 2 * ppu, c.X, c.Y + it.Len / 2 * ppu);
                else g.DrawLine(pen, c.X - it.Len / 2 * ppu, c.Y, c.X + it.Len / 2 * ppu, c.Y);
                continue;
            }
            if (it.Kind == "HAND")
            {
                using var p = new Pen(Color.FromArgb(70, 175, 240), 3f);
                g.DrawEllipse(p, c.X - 0.8f * ppu, c.Y - 0.8f * ppu, 1.6f * ppu, 1.6f * ppu);
                continue;
            }
            using var img = Image.FromFile(Path.Combine(dir, it.Sprite + ".png"));
            g.DrawImage(img, c.X - s.Width * ppu / 2, c.Y - s.Height * ppu / 2, s.Width * ppu, s.Height * ppu);
        }

        var pts = new List<PointF>();
        for (int i = 0; i <= 40; i++) { float u = 3f * i / 40f; pts.Add(S(Graph.X + u * GraphScale.X, Graph.Y - u * u * GraphScale.Y)); }
        using (var pen = new Pen(Color.FromArgb(45, 45, 55), 3f)) g.DrawLines(pen, pts.ToArray());

        var ps = S(PlayerStart.X - 0.4f, PlayerStart.Y + 0.5f);
        g.FillRectangle(Brushes.Black, ps.X, ps.Y, 0.8f * ppu, 1f * ppu);
        bmp.Save(outPath);
    }

    // =====================================================================
    //  export for the Unity one-shot script
    // =====================================================================

    public static string ExportCSharp()
    {
        var inv = CultureInfo.InvariantCulture;
        var sb = new StringBuilder();
        foreach (Item it in Items())
            sb.AppendLine(string.Format(inv, "        new Item(\"{0}\", \"{1}\", \"{2}\", {3}f, {4}f, {5}f, {6}f, {7}),",
                it.Name, it.Sprite, it.Kind, it.X, it.Y, it.Scale, it.Len, it.Vertical ? "true" : "false"));
        return sb.ToString();
    }
}
