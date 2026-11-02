using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.IO;

// Styl sesitu: linkovany papir, modra propiska, olivove zvyraznovani.
public static class Sesit
{
    const int PPU = 200;
    const float TextHeight = 1.2f;
    const float TextPad = 0.25f;

    static readonly Color Ink = Color.FromArgb(28, 62, 140);
    static readonly Color Marker = Color.FromArgb(120, 150, 128, 60);
    static readonly Color Pale = Color.FromArgb(120, 28, 62, 140);
    static Random rnd = new Random(7);

    public static readonly (string name, string text, float w, float x, float y, bool mark)[] Texts =
    {
        ("Ukol", "Úkol", 3f, -2.5f, 10.97f, true),
        ("Zadani", "Graf ceny", 5f, 3.5f, 13.28f, false),
        ("Pozor", "Pozor!", 4.5f, 0f, 15.58f, false),
        ("Ukol2", "Doplň tabulku:", 5f, -11f, 20.18f, true),
        ("a4", "a = 4 cm", 2.5f, -5f, 17.88f, false),
        ("Vypocitej", "Vypočítej:", 3f, -6.5f, 8.63f, false),
        ("Reseni", "S = 16 cm²", 5f, -6f, -0.58f, false),
        ("Odpoved", "odpověď: 16", 5f, -12f, 1.73f, false),
        ("Rovnice", "2x + 6 = 14", 4.5f, -6f, 4.02f, false),
        ("DU", "Domácí úkol", 5f, -12f, -2.88f, false),
        ("Kruh", "o = 2 · π · r", 4.5f, -12f, 6.33f, false),
        ("Pyth", "a² + b² = c²", 5f, 22.5f, 5.02f, false),
        ("Trida", "Matematika", 4.5f, 32.5f, 5.02f, true),
        ("x4", "x = 4", 2f, 27.5f, 7.33f, false),
        ("Ukol3", "Kolik stojí 4?", 5f, 32f, 9.63f, false),
        ("r2", "r = 2 cm", 2.5f, 37f, 11.93f, false),
        ("Pi", "π ≈ 3,14", 3f, 32.5f, 20.88f, false),
        ("Schody1", "3 + 4 = 7", 3f, 37.5f, 4.17f, false),
        ("Schody2", "9 − 5 = 4", 2.5f, 41f, 4.98f, false),
        ("Hotovo", "Hotovo!", 2.75f, 51.5f, 18.68f, true),
        ("Dole1", "y = 25 − 5x", 4f, 16f, -2.18f, false),
        ("Dole2", "1 jablko", 3.5f, 1f, 0.83f, false),
        ("Dole3", "= −5 Kč", 3f, 3.5f, 6.42f, false),
        ("Nahore1", "Je to lineární", 4f, 21.5f, 21.93f, false),
        ("Nahore2", "Kolik stojí 6?", 4f, 27f, 23.03f, false),
        ("Nahore3", "cena klesá o 5", 4f, 16.5f, 19.63f, false),
        ("Vpravo2", "množství", 4f, 46.5f, 16.38f, false),
    };

    public static readonly (string name, float w, float h, float x, float y)[] Drawings =
    {
        ("Kruh", 1.94f, 1.94f, 31.5f, -1.27f),
        ("Lichobeznik", 2.89f, 1.32f, 39f, 1.6f),
        ("Pravitko", 8f, 1.42f, -3f, 21.9f),
        ("CiselnaOsa", 10.23f, 0.75f, 12f, -4.6f),
        ("Ctverec", 2.36f, 2.35f, 24f, -3.82f),
        ("Mrak", 4.35f, 1.78f, 41.5f, 13.4f),
        ("Slunce", 3.08f, 3.08f, 4.8f, 20.3f),
        ("Sestiuhelnik", 2.16f, 1.89f, 35f, 1.05f),
        ("Krychle", 2.93f, 2.93f, -14.6f, 16.8f),
        ("Trojuhelnik", 3.11f, 2.51f, 28f, -3.75f),
        ("Dum", 3.42f, 3.87f, -14.3f, 11.2f),
    };

    public static void Render(string dir, string preview)
    {
        Directory.CreateDirectory(dir);
        using (var paper = Paper()) paper.Save(Path.Combine(dir, "Papir_Sesit.png"));
        foreach (var t in Texts) using (var b = Text(t.text, t.w, t.mark)) b.Save(Path.Combine(dir, "T_" + t.name + ".png"));
        foreach (var d in Drawings) using (var b = Drawing(d.name, d.w, d.h)) b.Save(Path.Combine(dir, "D_" + d.name + ".png"));

        string[] food = { "Jablko", "Hvezda", "Kvet", "Trojuhelnik", "Pet", "Pi", "Kostka", "Plus" };
        foreach (var f in food)
        {
            using (var b = Food(f, Color.FromArgb(25, 85, 205))) b.Save(Path.Combine(dir, "J_" + f + "_M.png"));
            using (var b = Food(f, Color.FromArgb(20, 165, 235))) b.Save(Path.Combine(dir, "J_" + f + "_S.png"));
        }
        using (var g = Green()) g.Save(Path.Combine(dir, "Zeleny.png"));
        Preview(dir, preview);
    }

    static Bitmap New(int w, int h, out Graphics g)
    {
        var b = new Bitmap(w, h);
        g = Graphics.FromImage(b);
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
        return b;
    }

    // ruka se trochu klepe: kazdy usek se rozdeli a posune o sum
    static void Stroke(Graphics g, Color c, float width, float wobble, bool closed, params PointF[] pts)
    {
        var list = new List<PointF>();
        int n = closed ? pts.Length : pts.Length - 1;
        for (int i = 0; i < n; i++)
        {
            PointF a = pts[i], b = pts[(i + 1) % pts.Length];
            int steps = Math.Max(1, (int)(Math.Sqrt((a.X - b.X) * (a.X - b.X) + (a.Y - b.Y) * (a.Y - b.Y)) / 30f));
            for (int s = 0; s < steps; s++)
            {
                float k = s / (float)steps;
                list.Add(new PointF(a.X + (b.X - a.X) * k + (float)(rnd.NextDouble() - 0.5) * wobble,
                                    a.Y + (b.Y - a.Y) * k + (float)(rnd.NextDouble() - 0.5) * wobble));
            }
        }
        list.Add(closed ? list[0] : pts[pts.Length - 1]);
        using var pen = new Pen(c, width) { StartCap = LineCap.Round, EndCap = LineCap.Round, LineJoin = LineJoin.Round };
        g.DrawCurve(pen, list.ToArray(), 0.5f);
    }

    static void Ink2(Graphics g, float width, float wobble, bool closed, params PointF[] pts)
    {
        Stroke(g, Ink, width, wobble, closed, pts);
        Stroke(g, Ink, width * 0.7f, wobble, closed, pts);
    }

    static PointF[] Ring(float cx, float cy, float rx, float ry, int n = 24)
    {
        var p = new PointF[n];
        for (int i = 0; i < n; i++)
        {
            double a = Math.PI * 2 * i / n;
            p[i] = new PointF(cx + (float)Math.Cos(a) * rx, cy + (float)Math.Sin(a) * ry);
        }
        return p;
    }

    static void Hatch(Graphics g, RectangleF r, Color c, float step, float width)
    {
        for (float x = r.Left - r.Height; x < r.Right; x += step)
            Stroke(g, c, width, 3f, false, new PointF(x, r.Bottom), new PointF(x + r.Height, r.Top));
    }

    static void Write(Graphics g, string text, float emPx, float x, float baseline, Color c, float fit = 0f)
    {
        using var font = new Font("Ink Free", emPx, FontStyle.Bold, GraphicsUnit.Pixel);
        float ascent = emPx * font.FontFamily.GetCellAscent(FontStyle.Bold) / font.FontFamily.GetEmHeight(FontStyle.Bold);
        var fmt = StringFormat.GenericTypographic;
        fmt.FormatFlags |= StringFormatFlags.MeasureTrailingSpaces;
        using var brush = new SolidBrush(c);

        float[] sizes = new float[text.Length];
        float sum = 0f;
        for (int i = 0; i < text.Length; i++) { sizes[i] = g.MeasureString(text[i].ToString(), font, 9999, fmt).Width; sum += sizes[i]; }
        float gap = 0f;
        if (fit > 0f && text.Length > 1) gap = Math.Min((fit - sum) / (text.Length - 1), emPx * 0.22f);

        float px = x;
        for (int i = 0; i < text.Length; i++)
        {
            var state = g.Save();
            float wave = (float)Math.Sin(i * 1.3 + x) * emPx * 0.025f;
            g.TranslateTransform(px, baseline + wave);
            g.RotateTransform((float)(rnd.NextDouble() - 0.5) * 6f);
            using (var letter = new GraphicsPath())
            {
                letter.AddString(text[i].ToString(), font.FontFamily, (int)FontStyle.Bold, emPx, new PointF(0, -ascent), fmt);
                g.FillPath(brush, letter);
            }
            g.Restore(state);
            px += sizes[i] + gap;
        }
    }

    static float Natural(Graphics g, string text, float em)
    {
        using var font = new Font("Ink Free", em, FontStyle.Bold, GraphicsUnit.Pixel);
        var fmt = StringFormat.GenericTypographic;
        fmt.FormatFlags |= StringFormatFlags.MeasureTrailingSpaces;
        float s = 0f;
        foreach (char ch in text) s += g.MeasureString(ch.ToString(), font, 9999, fmt).Width;
        return s;
    }

    // napis je presne tak siroky jako platforma, horni hrana pismen = horni hrana kolideru
    static Bitmap Text(string text, float w, bool mark)
    {
        int W = (int)(w * PPU), H = (int)(TextHeight * PPU);
        var b = New(W, H, out Graphics g);
        float em = 170f;
        while (em > 40f && Natural(g, text, em) > W * 0.97f) em -= 2f;
        float natural = Natural(g, text, em);
        float fit = W * 0.97f;
        float maxWidth = natural + (text.Length - 1) * em * 0.22f;
        float start = maxWidth < fit ? (W - maxWidth) / 2f : W * 0.015f;
        float baseline = TextPad * PPU + em * 0.68f;
        if (mark)
            for (int i = 0; i < 3; i++)
                Stroke(g, Marker, 26f, 8f, false, new PointF(W * 0.04f, baseline + 2 + i * 12), new PointF(W * 0.96f, baseline - 8 + i * 12));
        Write(g, text, em, start, baseline, Ink, fit);
        g.Dispose();
        return b;
    }

    static Bitmap Drawing(string name, float wu, float hu)
    {
        int W = (int)(wu * PPU), H = (int)(hu * PPU);
        var b = New(W, H, out Graphics g);
        float m = 8f, w = W - m * 2, h = H - m * 2, cx = W / 2f, cy = H / 2f;
        PointF P(float x, float y) => new PointF(m + w * x, m + h * y);
        var area = new RectangleF(m, m, w, h);

        void Shape(params PointF[] pts)
        {
            using var clip = new GraphicsPath();
            clip.AddPolygon(pts);
            var state = g.Save();
            g.SetClip(clip);
            Hatch(g, area, Pale, 26f, 4f);
            Hatch(g, new RectangleF(m, m + h * 0.55f, w, h * 0.45f), Marker, 30f, 18f);
            g.Restore(state);
            Ink2(g, 7f, 3f, true, pts);
        }

        switch (name)
        {
            case "Kruh": Shape(Ring(cx, cy, w / 2, h / 2)); break;
            case "Slunce":
                Shape(Ring(cx, cy, w * 0.3f, h * 0.3f));
                for (int i = 0; i < 12; i++)
                {
                    double a = Math.PI * 2 * i / 12;
                    Ink2(g, 6f, 2f, false, new PointF(cx + (float)Math.Cos(a) * w * 0.37f, cy + (float)Math.Sin(a) * h * 0.37f),
                                           new PointF(cx + (float)Math.Cos(a) * w * 0.49f, cy + (float)Math.Sin(a) * h * 0.49f));
                }
                break;
            case "Ctverec": Shape(P(0, 0), P(1, 0), P(1, 1), P(0, 1)); break;
            case "Trojuhelnik": Shape(P(0.5f, 0), P(1, 1), P(0, 1)); break;
            case "Lichobeznik": Shape(P(0.2f, 0), P(0.8f, 0), P(1, 1), P(0, 1)); break;
            case "Sestiuhelnik": Shape(P(0.25f, 0), P(0.75f, 0), P(1, 0.5f), P(0.75f, 1), P(0.25f, 1), P(0, 0.5f)); break;
            case "Krychle":
                Shape(P(0, 0.3f), P(0.7f, 0.3f), P(0.7f, 1), P(0, 1));
                Ink2(g, 7f, 3f, false, P(0, 0.3f), P(0.3f, 0), P(1, 0), P(1, 0.7f), P(0.7f, 1));
                Ink2(g, 7f, 3f, false, P(0.7f, 0.3f), P(1, 0));
                Stroke(g, Pale, 4f, 2f, false, P(0.3f, 0), P(0.3f, 0.7f), P(1, 0.7f));
                Stroke(g, Pale, 4f, 2f, false, P(0.3f, 0.7f), P(0, 1));
                break;
            case "Mrak":
                Hatch(g, new RectangleF(m, m + h * 0.6f, w, h * 0.35f), Marker, 30f, 18f);
                Ink2(g, 7f, 3f, true, P(0.1f, 0.95f), P(0.02f, 0.65f), P(0.15f, 0.42f), P(0.28f, 0.42f), P(0.36f, 0.08f), P(0.6f, 0.02f), P(0.72f, 0.35f), P(0.9f, 0.38f), P(1, 0.68f), P(0.88f, 0.95f));
                break;
            case "Dum":
                Shape(P(0.12f, 0.42f), P(0.88f, 0.42f), P(0.88f, 1), P(0.12f, 1));
                Shape(P(0.5f, 0), P(1, 0.42f), P(0, 0.42f));
                Ink2(g, 6f, 2f, true, P(0.42f, 0.68f), P(0.62f, 0.68f), P(0.62f, 1), P(0.42f, 1));
                break;
            case "Pravitko":
                Hatch(g, new RectangleF(m, m + h * 0.6f, w, h * 0.4f), Marker, 30f, 18f);
                Ink2(g, 7f, 2f, true, P(0, 0), P(1, 0), P(1, 1), P(0, 1));
                for (int i = 1; i < 16; i++)
                    Ink2(g, 4f, 1f, false, P(i / 16f, 0), P(i / 16f, i % 2 == 0 ? 0.45f : 0.25f));
                for (int i = 2; i < 16; i += 4) Write(g, (i / 2).ToString(), 70f, m + w * i / 16f - 12f, m + h * 0.92f, Ink);
                break;
            case "CiselnaOsa":
                Ink2(g, 7f, 2f, false, P(0, 0.3f), P(1, 0.3f));
                Ink2(g, 7f, 2f, false, P(0.98f, 0.08f), P(1, 0.3f), P(0.98f, 0.52f));
                for (int i = 0; i <= 10; i++)
                {
                    Ink2(g, 5f, 1f, false, P(0.02f + i * 0.092f, 0.08f), P(0.02f + i * 0.092f, 0.52f));
                    Write(g, i.ToString(), 58f, m + w * (0.02f + i * 0.092f) - 12f, m + h * 1f, Ink);
                }
                break;
        }
        g.Dispose();
        return b;
    }

    static Bitmap Food(string name, Color c)
    {
        const int S = 200;
        var b = New(S, S, out Graphics g);
        Color fill = Color.FromArgb(70, c);
        void Shape(params PointF[] pts)
        {
            using var br = new SolidBrush(fill);
            g.FillPolygon(br, pts);
            Stroke(g, c, 9f, 3f, true, pts);
            Stroke(g, c, 6f, 3f, true, pts);
        }
        PointF P(float x, float y) => new PointF(x, y);
        switch (name)
        {
            case "Jablko":
                Shape(P(100, 52), P(135, 40), P(170, 62), P(178, 110), P(150, 170), P(115, 180), P(100, 170), P(85, 180), P(50, 170), P(22, 110), P(30, 62), P(65, 40));
                Stroke(g, c, 8f, 2f, false, P(100, 52), P(108, 18));
                Stroke(g, c, 7f, 2f, true, P(108, 34), P(135, 12), P(150, 28), P(125, 40));
                break;
            case "Hvezda":
                var star = new PointF[10];
                for (int i = 0; i < 10; i++)
                {
                    double a = -Math.PI / 2 + Math.PI * i / 5;
                    float r = i % 2 == 0 ? 90f : 38f;
                    star[i] = P(100 + (float)Math.Cos(a) * r, 105 + (float)Math.Sin(a) * r);
                }
                Shape(star);
                break;
            case "Kvet":
                for (int i = 0; i < 5; i++)
                {
                    double a = Math.PI * 2 * i / 5 - Math.PI / 2;
                    Shape(Ring(100 + (float)Math.Cos(a) * 48, 100 + (float)Math.Sin(a) * 48, 36, 36, 12));
                }
                Shape(Ring(100, 100, 24, 24, 10));
                break;
            case "Trojuhelnik": Shape(P(100, 20), P(182, 170), P(18, 170)); break;
            case "Kostka":
                Shape(P(25, 65), P(130, 65), P(130, 175), P(25, 175));
                Shape(P(25, 65), P(70, 25), P(175, 25), P(130, 65));
                Shape(P(130, 65), P(175, 25), P(175, 130), P(130, 175));
                break;
            case "Pet": Write(g, "5", 190f, 48f, 170f, c); break;
            case "Pi": Write(g, "π", 190f, 42f, 165f, c); break;
            case "Plus":
                Stroke(g, c, 26f, 3f, false, P(100, 25), P(100, 175));
                Stroke(g, c, 26f, 3f, false, P(25, 100), P(175, 100));
                break;
        }
        g.Dispose();
        return b;
    }

    // zelena fixa - tahy jsou vodorovne, proto vydrzi roztazeni na dlouhy pruh
    static Bitmap Green()
    {
        var b = New(512, 512, out Graphics g);
        using (var br = new SolidBrush(Color.FromArgb(150, 40, 200, 80))) g.FillRectangle(br, 0, 40, 512, 432);
        for (int y = 50; y < 470; y += 34)
            Stroke(g, Color.FromArgb(170, 20, 170, 60), 30f, 10f, false, new PointF(0, y), new PointF(512, y + 6));
        g.Dispose();
        return b;
    }

    const float PaperPPU = 50f;
    public const float SheetLeft = -17.54f, SheetBottom = -5.6f, SheetW = 82.04f, SheetH = 30.99f;

    static PointF W2P(float x, float y) => new PointF((x - SheetLeft) * PaperPPU, (SheetBottom + SheetH - y) * PaperPPU);

    static Bitmap Paper()
    {
        int W = (int)(SheetW * PaperPPU), H = (int)(SheetH * PaperPPU);
        var b = New(W, H, out Graphics g);
        g.Clear(Color.FromArgb(248, 249, 254));

        for (float y = -5f; y < 26f; y += 2.4f)
            Stroke(g, Color.FromArgb(255, 76, 110, 170), 3.6f, 1.2f, false, W2P(SheetLeft, y), W2P(SheetLeft + SheetW, y));
        Stroke(g, Color.FromArgb(235, 225, 75, 80), 5f, 1f, false, W2P(-15.9f, 26f), W2P(-15.9f, -6f));

        // poznamky v pozadi jsou slabe, aby se nepletly s platformami
        Color n = Color.FromArgb(95, 28, 62, 140);
        Note(g, "Úkol", -10f, 15f, 85f, n);
        Stroke(g, n, 3f, 5f, true, Ring(W2P(-8.3f, 15.8f).X, W2P(-8.3f, 15.8f).Y, 120, 62));
        Note(g, "Je to lineární", 56f, 22f, 55f, n);
        Stroke(g, n, 3f, 5f, true, Ring(W2P(59f, 22.4f).X, W2P(59f, 22.4f).Y, 215, 62));
        Note(g, "y = 25 − 5x", 54.5f, 3f, 70f, n);
        Stroke(g, Marker, 16f, 6f, false, W2P(54.5f, 2.2f), W2P(62.5f, 2.6f));
        Note(g, "každé 1 jablko = −5 Kč", 12f, 2f, 48f, n);
        Note(g, "kolik stojí 6 jablek?", 44.5f, 9f, 48f, n);
        Note(g, "o kolik se cena sníží?", 19f, 15f, 48f, n);

        // tabulka
        float tx = 56f, ty = 17f;
        for (int r = 0; r <= 6; r++) Stroke(g, n, 3f, 3f, false, W2P(tx, ty - r * 1.1f), W2P(tx + 5f, ty - r * 1.1f));
        for (int c = 0; c <= 2; c++) Stroke(g, n, 3f, 3f, false, W2P(tx + c * 2.5f, ty), W2P(tx + c * 2.5f, ty - 6.6f));
        string[,] rows = { { "ks", "cena" }, { "0", "25" }, { "1", "20" }, { "2", "15" }, { "3", "10" }, { "4", "5" } };
        for (int r = 0; r < 6; r++)
        {
            Note(g, rows[r, 0], tx + 0.6f, ty - 1f - r * 1.1f, 38f, n);
            Note(g, rows[r, 1], tx + 3f, ty - 1f - r * 1.1f, 38f, n);
        }
        g.Dispose();
        return b;
    }

    static void Note(Graphics g, string text, float x, float y, float em, Color c)
    {
        PointF p = W2P(x, y);
        Write(g, text, em, p.X, p.Y, c);
    }

    static void Preview(string dir, string outFile)
    {
        using var paper = new Bitmap(Path.Combine(dir, "Papir_Sesit.png"));
        const float S = 24f;
        using var all = new Bitmap((int)(SheetW * S), (int)(SheetH * S));
        using var g = Graphics.FromImage(all);
        g.InterpolationMode = InterpolationMode.HighQualityBicubic;
        g.DrawImage(paper, 0, 0, all.Width, all.Height);
        RectangleF R(float cx, float cyTop, float w, float h) => new RectangleF((cx - w / 2 - SheetLeft) * S, (SheetBottom + SheetH - cyTop) * S, w * S, h * S);

        foreach (var d in Drawings)
            using (var b = new Bitmap(Path.Combine(dir, "D_" + d.name + ".png"))) g.DrawImage(b, R(d.x, d.y + d.h / 2, d.w, d.h));
        foreach (var t in Texts)
            using (var b = new Bitmap(Path.Combine(dir, "T_" + t.name + ".png"))) g.DrawImage(b, R(t.x, t.y + 0.175f + TextPad, t.w, TextHeight));
        all.Save(outFile);

        int cw = (int)(26f * S), ch = (int)(15f * S);
        using var crop = all.Clone(new Rectangle((int)(0.5f * S), (int)(14f * S), cw, ch), all.PixelFormat);
        using var big = new Bitmap(cw * 2, ch * 2);
        using (var gb = Graphics.FromImage(big)) { gb.InterpolationMode = InterpolationMode.HighQualityBicubic; gb.DrawImage(crop, 0, 0, big.Width, big.Height); }
        big.Save(Path.ChangeExtension(outFile, ".crop.png"));
    }
}
