using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.IO;

// Notebook art at 200 px per Unity unit: handwritten words, colored-pencil doodles, ruler-drawn shapes.
public static class Doodles
{
    const int PPU = 200;
    static string _dir;

    static readonly Color Ink = Color.FromArgb(45, 45, 55);          // tuzka/propiska na platformy
    static readonly Color Graphite = Color.FromArgb(58, 58, 68);
    static readonly Color Blue = Color.FromArgb(25, 85, 205);        // jidlo: modra
    static readonly Color LightBlue = Color.FromArgb(70, 175, 240);  // jidlo: svetle modra

    public static void RenderAll(string dir)
    {
        _dir = dir;

        // vety a slova (platformy), seda tuzka
        Text("Text_Ukol", "Úkol 1", 120, Ink, -2f, underline: true);
        Text("Text_Zadani", "Nakresli graf funkce y = −x² a sjeď po něm.", 90, Ink, 0.5f);
        Text("Text_Pozor", "Pozor! Nezapomeň na jednotky.", 90, Ink, -0.8f);
        Text("Text_Ukol2", "Úkol 2: Narýsuj čtverec a kruh.", 90, Ink, 0.6f);
        Text("Text_a4", "a = 4 cm", 90, Ink, -1.5f);
        Text("Text_Vypocitej", "Vypočítej:", 95, Ink, 1.2f);
        Text("Text_Reseni", "Řešení: S = a · a = 4 · 4 = 16 cm²", 90, Ink, -0.5f);
        Text("Text_Odpoved", "Odpověď: Obsah čtverce je 16 cm².", 90, Ink, 0.4f);
        Text("Text_Rovnice", "2x + 6 = 14   →   x = 4", 90, Ink, -0.7f);
        Text("Text_DU", "Domácí úkol: str. 42, cvičení 3 a 5", 90, Ink, 0.5f);
        Text("Text_Kruh", "Obvod kruhu: o = 2 · π · r", 90, Ink, -0.4f);
        Text("Text_Pyth", "Pythagorova věta: a² + b² = c²", 90, Ink, 0.3f);
        Text("Text_Trida", "Matematika – 6. třída", 90, Ink, -0.6f, underline: true);
        Text("Text_x4", "x = 4", 95, Ink, 1.5f);
        Text("Text_Ukol3", "Úkol 3: Vypočítej obvod kruhu.", 90, Ink, -0.5f);
        Text("Text_r2", "r = 2 cm", 90, Ink, 1f);
        Text("Text_Pi", "π ≈ 3,14", 90, Ink, -1f);
        Text("Text_Schody1", "1) 3 + 4 = 7", 90, Ink, 0.8f);
        Text("Text_Schody2", "2) 9 − 5 = 4", 90, Ink, -0.8f);
        Text("Text_Schody3", "3) 2 · 6 = 12", 90, Ink, 0.6f);
        Text("Text_Hotovo", "Hotovo!", 110, Ink, -2.5f);

        // jidlo: cisla a znaky (modra / svetle modra)
        Text("Jidlo_2", "2", 150, Blue, -4f);
        Text("Jidlo_4", "4", 150, LightBlue, 3f);
        Text("Jidlo_5", "5", 150, Blue, 5f);
        Text("Jidlo_7", "7", 150, LightBlue, -3f);
        Text("Jidlo_9", "9", 150, Blue, 2f);
        Text("Jidlo_Pi", "π", 160, LightBlue, 0f);
        Text("Jidlo_Plus", "+", 160, Blue, 0f);
        Text("Jidlo_Rovna", "=", 160, LightBlue, 0f);

        // jidlo: tvary a obrazky
        Circle("Jidlo_Kruh", 140, Color.FromArgb(200, 228, 255), Color.FromArgb(150, 200, 245), LightBlue);
        Square("Jidlo_Ctverec", 130, Color.FromArgb(195, 215, 250), Color.FromArgb(140, 175, 235), Blue);
        Polygon("Jidlo_Trojuhelnik", new[] { P(0, 0), P(160, 0), P(80, 140) }, Color.FromArgb(200, 228, 255), Color.FromArgb(150, 200, 245), LightBlue);
        Polygon("Jidlo_Kosoctverec", new[] { P(80, 0), P(160, 110), P(80, 220), P(0, 110) }, Color.FromArgb(195, 215, 250), Color.FromArgb(140, 175, 235), Blue);
        Polygon("Jidlo_Hvezda", Star(5, 90, 38), Color.FromArgb(200, 228, 255), Color.FromArgb(150, 200, 245), LightBlue);
        Drop("Jidlo_Kapka", Color.FromArgb(150, 210, 250), LightBlue);
        Heart("Jidlo_Srdce", Color.FromArgb(70, 120, 230), Blue);

        // geometricke tvary (platformy) - zadna modra, aby se nepletly s jidlem
        Square("Shape_Ctverec", 500, Color.FromArgb(255, 214, 214), Color.FromArgb(240, 170, 170), Graphite);
        Polygon("Shape_Trojuhelnik", new[] { P(0, 0), P(600, 0), P(300, 480) }, Color.FromArgb(255, 242, 185), Color.FromArgb(240, 205, 120), Graphite);
        Circle("Shape_Kruh", 480, Color.FromArgb(208, 240, 206), Color.FromArgb(150, 205, 150), Graphite);
        Polygon("Shape_Sestiuhelnik", Regular(6, 260, 0f), Color.FromArgb(228, 212, 246), Color.FromArgb(185, 160, 225), Graphite);
        Polygon("Shape_Lichobeznik", new[] { P(0, 0), P(560, 0), P(480, 240), P(80, 240) }, Color.FromArgb(255, 222, 192), Color.FromArgb(240, 175, 130), Graphite);
        Cube("Shape_Krychle", 400, 110);

        // kresby
        Cloud("Kresba_Mrak");
        Sun("Kresba_Slunce");
        House("Kresba_Dum");
        Ruler("Kresba_Pravitko", 2600, 260);
        NumberLine("Kresba_CiselnaOsa", 2000);
        Axis("Kresba_OsaX", 1440, true, "x");
        Axis("Kresba_OsaY", 1300, false, "y");

        // mistnost a zeleny pruh
        Line("Kresba_Cara", 800, 40);
        Bar("Zeleny_Pruh", 1000, 90, Color.FromArgb(120, 205, 105), Color.FromArgb(55, 140, 50));
    }

    static PointF[] Star(int n, float outer, float inner)
    {
        var r = new PointF[n * 2];
        for (int i = 0; i < n * 2; i++)
        {
            float rad = i % 2 == 0 ? outer : inner;
            float a = (-90f + 180f * i / n) * MathF.PI / 180f;
            r[i] = new PointF(outer + rad * MathF.Cos(a), outer + rad * MathF.Sin(a));
        }
        return r;
    }

    static void Drop(string name, Color fill, Color line)
    {
        Bitmap bmp = NewCanvas(200, 260, out Graphics g);
        using (g)
        {
            using var path = new GraphicsPath();
            path.AddBezier(100, 20, 120, 70, 175, 120, 175, 165);
            path.AddBezier(175, 165, 175, 215, 140, 245, 100, 245);
            path.AddBezier(100, 245, 60, 245, 25, 215, 25, 165);
            path.AddBezier(25, 165, 25, 120, 80, 70, 100, 20);
            path.CloseFigure();
            using (var b = new SolidBrush(fill)) g.FillPath(b, path);
            using (var p = new Pen(line, 8f) { LineJoin = LineJoin.Round }) g.DrawPath(p, path);
            using (var hl = new SolidBrush(Color.FromArgb(170, 255, 255, 255))) g.FillEllipse(hl, 60, 140, 26, 40);
        }
        Save(bmp, name);
    }

    static void Ruler(string name, int w, int h)
    {
        Bitmap bmp = NewCanvas(w + 40, h + 40, out Graphics g);
        using (g)
        {
            var body = new[] { P(20, 20), P(w + 20, 20), P(w + 20, h + 20), P(20, h + 20) };
            using (var b = new SolidBrush(Color.FromArgb(245, 230, 190))) g.FillPolygon(b, body);
            using var tick = new Pen(Graphite, 5f);
            using var font = new Font("Segoe Print", 44, FontStyle.Bold, GraphicsUnit.Pixel);
            for (int i = 0; i <= 25; i++)
            {
                float x = 70 + i * (w - 100) / 25f;
                g.DrawLine(tick, x, 20, x, 20 + (i % 5 == 0 ? 110 : 60));
                if (i % 5 == 0) g.DrawString((i / 5 * 5).ToString(), font, new SolidBrush(Graphite), x - 20, 135);
            }
            PencilStroke(g, body, true, Color.FromArgb(150, 110, 50), 9f, 1.2f, 4);
        }
        Save(bmp, name);
    }

    static void NumberLine(string name, int w)
    {
        Bitmap bmp = NewCanvas(w + 80, 260, out Graphics g);
        using (g)
        {
            float y = 80;
            PencilStroke(g, new[] { P(20, y), P(w + 40, y) }, false, Graphite, 9f, 1.2f, 1);
            PencilStroke(g, new[] { P(w + 5, y - 30), P(w + 45, y), P(w + 5, y + 30) }, false, Graphite, 9f, 1f, 2);
            using var font = new Font("Segoe Print", 54, FontStyle.Bold, GraphicsUnit.Pixel);
            using var brush = new SolidBrush(Graphite);
            for (int i = 0; i <= 10; i++)
            {
                float x = 60 + i * (w - 120) / 10f;
                PencilStroke(g, new[] { P(x, y - 25), P(x, y + 25) }, false, Graphite, 7f, 0.5f, i + 10);
                g.DrawString(i.ToString(), font, brush, x - 18, y + 35);
            }
        }
        Save(bmp, name);
    }

    static void Axis(string name, int length, bool horizontal, string label)
    {
        int w = horizontal ? length + 140 : 220, h = horizontal ? 220 : length + 140;
        Bitmap bmp = NewCanvas(w, h, out Graphics g);
        using (g)
        {
            using var font = new Font("Segoe Print", 80, FontStyle.Bold, GraphicsUnit.Pixel);
            using var brush = new SolidBrush(Graphite);
            if (horizontal)
            {
                float y = 60;
                PencilStroke(g, new[] { P(20, y), P(length + 60, y) }, false, Graphite, 10f, 1f, 1);
                PencilStroke(g, new[] { P(length + 20, y - 34), P(length + 64, y), P(length + 20, y + 34) }, false, Graphite, 10f, 0.8f, 2);
                g.DrawString(label, font, brush, length - 20, y + 20);
            }
            else
            {
                float x = 160;
                PencilStroke(g, new[] { P(x, h - 20), P(x, 70) }, false, Graphite, 10f, 1f, 3);
                PencilStroke(g, new[] { P(x - 34, 110), P(x, 66), P(x + 34, 110) }, false, Graphite, 10f, 0.8f, 4);
                g.DrawString(label, font, brush, 10, 40);
            }
        }
        Save(bmp, name);
    }

    static void Line(string name, int w, int h)
    {
        Bitmap bmp = NewCanvas(w, h, out Graphics g);
        using (g) PencilStroke(g, new[] { P(10, h / 2f), P(w - 10, h / 2f) }, false, Graphite, h * 0.45f, 1.5f, 9);
        Bitmap trimmed = bmp; // tloustka musi zustat presna
        trimmed.Save(Path.Combine(_dir, name + ".png"), ImageFormat.Png);
        bmp.Dispose();
    }

    static void Bar(string name, int w, int h, Color fill, Color line)
    {
        Bitmap bmp = NewCanvas(w + 20, h + 20, out Graphics g);
        using (g)
        {
            var r = new[] { P(10, 10), P(w + 10, 10), P(w + 10, h + 10), P(10, h + 10) };
            using (var b = new SolidBrush(fill)) g.FillPolygon(b, r);
            using (var path = new GraphicsPath()) { path.AddPolygon(r); Hatch(g, path, path.GetBounds(), Color.FromArgb(90, 175, 80), 18f, -35f); }
            PencilStroke(g, r, true, line, 7f, 1f, 5);
        }
        Save(bmp, name);
    }

    // =====================================================================
    //  helpers
    // =====================================================================

    static PointF P(float x, float y) => new PointF(x, y);

    static Bitmap NewCanvas(int w, int h, out Graphics g)
    {
        var bmp = new Bitmap(w, h, PixelFormat.Format32bppArgb);
        g = Graphics.FromImage(bmp);
        g.SmoothingMode = SmoothingMode.HighQuality;
        g.PixelOffsetMode = PixelOffsetMode.HighQuality;
        g.TextRenderingHint = TextRenderingHint.AntiAlias;
        g.Clear(Color.Transparent);
        return bmp;
    }

    static void Save(Bitmap bmp, string name)
    {
        Bitmap trimmed = Trim(bmp, 6);
        trimmed.Save(Path.Combine(_dir, name + ".png"), ImageFormat.Png);
        trimmed.Dispose();
        bmp.Dispose();
    }

    static Bitmap Trim(Bitmap src, int pad)
    {
        int minX = src.Width, minY = src.Height, maxX = -1, maxY = -1;
        var data = src.LockBits(new Rectangle(0, 0, src.Width, src.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
        var px = new int[src.Width * src.Height];
        System.Runtime.InteropServices.Marshal.Copy(data.Scan0, px, 0, px.Length);
        src.UnlockBits(data);
        for (int y = 0; y < src.Height; y++)
            for (int x = 0; x < src.Width; x++)
                if (((px[y * src.Width + x] >> 24) & 0xFF) > 8)
                {
                    if (x < minX) minX = x; if (x > maxX) maxX = x;
                    if (y < minY) minY = y; if (y > maxY) maxY = y;
                }
        if (maxX < 0) return new Bitmap(src);
        var rect = Rectangle.FromLTRB(Math.Max(0, minX - pad), Math.Max(0, minY - pad),
            Math.Min(src.Width, maxX + pad + 1), Math.Min(src.Height, maxY + pad + 1));
        return src.Clone(rect, PixelFormat.Format32bppArgb);
    }

    // Hand-drawn line: subdivide and jitter perpendicular to the segment.
    static PointF[] Wobble(IList<PointF> pts, bool closed, float amp, int seed, float step = 10f)
    {
        var rnd = new Random(seed);
        var result = new List<PointF>();
        int count = closed ? pts.Count : pts.Count - 1;
        float phase = (float)rnd.NextDouble() * 10f;
        for (int i = 0; i < count; i++)
        {
            PointF a = pts[i], b = pts[(i + 1) % pts.Count];
            float dx = b.X - a.X, dy = b.Y - a.Y;
            float len = MathF.Sqrt(dx * dx + dy * dy);
            int n = Math.Max(1, (int)(len / step));
            float nx = -dy / len, ny = dx / len;
            for (int k = 0; k < n; k++)
            {
                float t = k / (float)n;
                float off = (MathF.Sin(phase + (i * n + k) * 0.35f) * 0.6f + ((float)rnd.NextDouble() - 0.5f) * 0.8f) * amp;
                result.Add(new PointF(a.X + dx * t + nx * off, a.Y + dy * t + ny * off));
            }
        }
        if (!closed) result.Add(pts[pts.Count - 1]);
        return result.ToArray();
    }

    static void PencilStroke(Graphics g, IList<PointF> pts, bool closed, Color color, float width, float amp, int seed)
    {
        for (int pass = 0; pass < 2; pass++)
        {
            PointF[] w = Wobble(pts, closed, amp, seed + pass * 31);
            using var pen = new Pen(Color.FromArgb(pass == 0 ? 235 : 120, color), pass == 0 ? width : width * 0.55f)
            { LineJoin = LineJoin.Round, StartCap = LineCap.Round, EndCap = LineCap.Round };
            if (closed) g.DrawPolygon(pen, w); else g.DrawLines(pen, w);
        }
    }

    static void Hatch(Graphics g, GraphicsPath clip, RectangleF bounds, Color color, float spacing, float angleDeg)
    {
        Region old = g.Clip;
        g.SetClip(clip);
        using var pen = new Pen(Color.FromArgb(150, color), 3f) { StartCap = LineCap.Round, EndCap = LineCap.Round };
        float diag = bounds.Width + bounds.Height;
        float a = angleDeg * MathF.PI / 180f;
        var rnd = new Random(7);
        for (float s = -diag; s < diag; s += spacing)
        {
            float cx = bounds.X + bounds.Width / 2 + s * MathF.Cos(a + MathF.PI / 2);
            float cy = bounds.Y + bounds.Height / 2 + s * MathF.Sin(a + MathF.PI / 2);
            float jitter = ((float)rnd.NextDouble() - 0.5f) * 4f;
            g.DrawLine(pen, cx - diag * MathF.Cos(a), cy - diag * MathF.Sin(a) + jitter, cx + diag * MathF.Cos(a), cy + diag * MathF.Sin(a) - jitter);
        }
        g.Clip = old;
    }

    static PointF[] Offset(PointF[] pts, float dx, float dy, bool flipY, float h)
    {
        var r = new PointF[pts.Length];
        for (int i = 0; i < pts.Length; i++)
            r[i] = new PointF(pts[i].X + dx, flipY ? h - (pts[i].Y + dy) : pts[i].Y + dy);
        return r;
    }

    static PointF[] Regular(int n, float radius, float startDeg)
    {
        var r = new PointF[n];
        for (int i = 0; i < n; i++)
        {
            float a = (startDeg + 360f * i / n) * MathF.PI / 180f;
            r[i] = new PointF(radius + radius * MathF.Cos(a), radius + radius * MathF.Sin(a));
        }
        return r;
    }

    // =====================================================================
    //  text
    // =====================================================================

    static void Text(string name, string text, float sizePx, Color color, float rotation, bool underline = false)
    {
        using var font = new Font("Segoe Print", sizePx, FontStyle.Bold, GraphicsUnit.Pixel);
        Bitmap bmp = NewCanvas((int)(sizePx * text.Length * 1.1f + 200), (int)(sizePx * 3f), out Graphics g);
        using (g)
        {
            g.TranslateTransform(bmp.Width / 2f, bmp.Height / 2f);
            g.RotateTransform(rotation);
            SizeF size = g.MeasureString(text, font);
            using var brush = new SolidBrush(color);
            g.DrawString(text, font, brush, -size.Width / 2f, -size.Height / 2f);
            if (underline)
            {
                float y = size.Height * 0.32f;
                PencilStroke(g, new[] { P(-size.Width * 0.45f, y), P(size.Width * 0.45f, y + 4) }, false, color, 7f, 3f, name.Length);
            }
        }
        Save(bmp, name);
    }

    // =====================================================================
    //  shapes
    // =====================================================================

    static void Polygon(string name, PointF[] pts, Color fill, Color hatch, Color line)
    {
        float maxX = 0, maxY = 0;
        foreach (var p in pts) { maxX = Math.Max(maxX, p.X); maxY = Math.Max(maxY, p.Y); }
        int pad = 30;
        PointF[] shape = Offset(pts, pad, pad, true, maxY + pad * 2);
        Bitmap bmp = NewCanvas((int)maxX + pad * 2, (int)maxY + pad * 2, out Graphics g);
        using (g)
        {
            using var path = new GraphicsPath();
            path.AddPolygon(shape);
            using (var b = new SolidBrush(fill)) g.FillPath(b, path);
            Hatch(g, path, path.GetBounds(), hatch, 22f, -35f);
            PencilStroke(g, shape, true, line, 8f, 1.4f, name.Length * 13);
        }
        Save(bmp, name);
    }

    static void Square(string name, float size, Color fill, Color hatch, Color line) =>
        Polygon(name, new[] { P(0, 0), P(size, 0), P(size, size), P(0, size) }, fill, hatch, line);

    static void Circle(string name, float diameter, Color fill, Color hatch, Color line)
    {
        var pts = new List<PointF>();
        float r = diameter / 2f;
        for (int i = 0; i < 72; i++)
        {
            float a = i * MathF.PI * 2f / 72f;
            pts.Add(P(r + r * MathF.Cos(a), r + r * MathF.Sin(a)));
        }
        Polygon(name, pts.ToArray(), fill, hatch, line);
    }

    // Cube in cabinet projection: front face, top face, right face, dashed hidden edges.
    static void Cube(string name, float s, float d)
    {
        int pad = 30;
        int w = (int)(s + d) + pad * 2, h = (int)(s + d) + pad * 2;
        Bitmap bmp = NewCanvas(w, h, out Graphics g);
        using (g)
        {
            float x0 = pad, y0 = pad + d; // front face top-left (screen coords)
            PointF[] front = { P(x0, y0), P(x0 + s, y0), P(x0 + s, y0 + s), P(x0, y0 + s) };
            PointF[] top = { P(x0, y0), P(x0 + d, y0 - d), P(x0 + s + d, y0 - d), P(x0 + s, y0) };
            PointF[] side = { P(x0 + s, y0), P(x0 + s + d, y0 - d), P(x0 + s + d, y0 + s - d), P(x0 + s, y0 + s) };

            using (var b = new SolidBrush(Color.FromArgb(236, 236, 240))) g.FillPolygon(b, front);
            using (var b = new SolidBrush(Color.FromArgb(248, 248, 250))) g.FillPolygon(b, top);
            using (var b = new SolidBrush(Color.FromArgb(205, 205, 212))) g.FillPolygon(b, side);
            using (var path = new GraphicsPath()) { path.AddPolygon(side); Hatch(g, path, path.GetBounds(), Color.FromArgb(165, 165, 175), 20f, 60f); }

            using (var dash = new Pen(Color.FromArgb(150, Graphite), 4f) { DashStyle = DashStyle.Dash })
            {
                PointF back = P(x0 + d, y0 + s - d);
                g.DrawLine(dash, back, P(x0 + d, y0 - d));
                g.DrawLine(dash, back, P(x0, y0 + s));
                g.DrawLine(dash, back, P(x0 + s + d, y0 + s - d));
            }
            PencilStroke(g, front, true, Graphite, 8f, 1.2f, 3);
            PencilStroke(g, new[] { P(x0, y0), P(x0 + d, y0 - d), P(x0 + s + d, y0 - d), P(x0 + s + d, y0 + s - d), P(x0 + s, y0 + s) }, false, Graphite, 8f, 1.2f, 5);
            PencilStroke(g, new[] { P(x0 + s, y0), P(x0 + s + d, y0 - d) }, false, Graphite, 8f, 1.2f, 9);
        }
        Save(bmp, name);
    }

    // =====================================================================
    //  doodles
    // =====================================================================

    static void Blobs(Graphics g, RectangleF[] blobs, Color fill, Color line, float lineWidth)
    {
        using (var pen = new Pen(line, lineWidth * 2f))
            foreach (var r in blobs) g.DrawEllipse(pen, r);
        using (var b = new SolidBrush(fill))
            foreach (var r in blobs) g.FillEllipse(b, r);
    }

    static void Cloud(string name)
    {
        Bitmap bmp = NewCanvas(1060, 440, out Graphics g);
        using (g)
        {
            var blobs = new[]
            {
                new RectangleF(30, 170, 300, 230), new RectangleF(200, 70, 330, 320), new RectangleF(430, 40, 330, 340),
                new RectangleF(650, 120, 320, 270), new RectangleF(120, 250, 820, 160)
            };
            Blobs(g, blobs, Color.FromArgb(252, 252, 252), Color.FromArgb(120, 120, 132), 7f);
            using var pen = new Pen(Color.FromArgb(120, 170, 170, 180), 6f) { StartCap = LineCap.Round, EndCap = LineCap.Round };
            g.DrawArc(pen, 280, 140, 160, 120, 200, 100);
            g.DrawArc(pen, 520, 110, 160, 120, 200, 100);
        }
        Save(bmp, name);
    }

    static void Sun(string name)
    {
        float r = 220, rays = 95;
        int size = (int)((r + rays) * 2 + 40);
        Bitmap bmp = NewCanvas(size, size, out Graphics g);
        using (g)
        {
            float c = size / 2f;
            for (int i = 0; i < 12; i++)
            {
                float a = i * MathF.PI / 6f;
                PencilStroke(g, new[] { P(c + MathF.Cos(a) * (r + 20), c + MathF.Sin(a) * (r + 20)), P(c + MathF.Cos(a) * (r + rays), c + MathF.Sin(a) * (r + rays)) },
                    false, Color.FromArgb(240, 150, 30), 12f, 2f, i);
            }
            var pts = new List<PointF>();
            for (int i = 0; i < 72; i++)
            {
                float a = i * MathF.PI * 2f / 72f;
                pts.Add(P(c + r * MathF.Cos(a), c + r * MathF.Sin(a)));
            }
            using (var b = new SolidBrush(Color.FromArgb(255, 222, 95))) g.FillPolygon(b, pts.ToArray());
            using (var path = new GraphicsPath()) { path.AddPolygon(pts.ToArray()); Hatch(g, path, path.GetBounds(), Color.FromArgb(250, 190, 60), 24f, -30f); }
            PencilStroke(g, pts, true, Color.FromArgb(235, 140, 25), 9f, 2f, 77);

            using var face = new SolidBrush(Color.FromArgb(120, 70, 20));
            g.FillEllipse(face, c - 80, c - 70, 34, 44);
            g.FillEllipse(face, c + 46, c - 70, 34, 44);
            using var smile = new Pen(Color.FromArgb(120, 70, 20), 12f) { StartCap = LineCap.Round, EndCap = LineCap.Round };
            g.DrawArc(smile, c - 100, c - 60, 200, 150, 30, 120);
        }
        Save(bmp, name);
    }

    static void House(string name)
    {
        Bitmap bmp = NewCanvas(680, 760, out Graphics g);
        using (g)
        {
            var wall = new[] { P(90, 330), P(590, 330), P(590, 730), P(90, 730) };
            var roof = new[] { P(40, 340), P(340, 50), P(640, 340) };
            using (var b = new SolidBrush(Color.FromArgb(255, 236, 200))) g.FillPolygon(b, wall);
            using (var b = new SolidBrush(Color.FromArgb(225, 110, 90))) g.FillPolygon(b, roof);
            using (var path = new GraphicsPath()) { path.AddPolygon(roof); Hatch(g, path, path.GetBounds(), Color.FromArgb(190, 75, 60), 22f, 30f); }
            PencilStroke(g, wall, true, Color.FromArgb(140, 85, 45), 9f, 1.8f, 1);
            PencilStroke(g, roof, true, Color.FromArgb(150, 55, 40), 9f, 1.8f, 2);

            var door = new[] { P(380, 520), P(480, 520), P(480, 730), P(380, 730) };
            using (var b = new SolidBrush(Color.FromArgb(170, 110, 65))) g.FillPolygon(b, door);
            PencilStroke(g, door, true, Color.FromArgb(110, 65, 35), 7f, 1.5f, 3);
            using (var b = new SolidBrush(Color.FromArgb(250, 220, 90))) g.FillEllipse(b, 455, 615, 16, 16);

            var window = new[] { P(160, 420), P(300, 420), P(300, 560), P(160, 560) };
            using (var b = new SolidBrush(Color.FromArgb(220, 235, 225))) g.FillPolygon(b, window);
            PencilStroke(g, window, true, Color.FromArgb(110, 65, 35), 7f, 1.5f, 4);
            PencilStroke(g, new[] { P(230, 420), P(230, 560) }, false, Color.FromArgb(110, 65, 35), 6f, 1f, 5);
            PencilStroke(g, new[] { P(160, 490), P(300, 490) }, false, Color.FromArgb(110, 65, 35), 6f, 1f, 6);
        }
        Save(bmp, name);
    }

    static void Apple(string name)
    {
        Bitmap bmp = NewCanvas(260, 280, out Graphics g);
        using (g)
        {
            var blobs = new[] { new RectangleF(30, 70, 130, 170), new RectangleF(100, 70, 130, 170) };
            Blobs(g, blobs, Color.FromArgb(232, 55, 55), Color.FromArgb(150, 20, 25), 6f);
            using (var hl = new SolidBrush(Color.FromArgb(140, 255, 255, 255))) g.FillEllipse(hl, 65, 105, 28, 45);
            PencilStroke(g, new[] { P(130, 80), P(138, 40), P(150, 20) }, false, Color.FromArgb(100, 60, 30), 9f, 1f, 3);
            using var leaf = new GraphicsPath();
            leaf.AddBezier(140, 45, 170, 10, 215, 20, 225, 40);
            leaf.AddBezier(225, 40, 200, 65, 160, 65, 140, 45);
            using (var b = new SolidBrush(Color.FromArgb(110, 190, 80))) g.FillPath(b, leaf);
            using (var p = new Pen(Color.FromArgb(55, 120, 40), 5f)) g.DrawPath(p, leaf);
        }
        Save(bmp, name);
    }

    static void Heart(string name, Color fill, Color line)
    {
        Bitmap bmp = NewCanvas(260, 240, out Graphics g);
        using (g)
        {
            using var path = new GraphicsPath();
            path.AddBezier(130, 225, 30, 150, 0, 90, 30, 45);
            path.AddBezier(30, 45, 60, 0, 120, 10, 130, 60);
            path.AddBezier(130, 60, 140, 10, 200, 0, 230, 45);
            path.AddBezier(230, 45, 260, 90, 230, 150, 130, 225);
            path.CloseFigure();
            using (var b = new SolidBrush(fill)) g.FillPath(b, path);
            Hatch(g, path, path.GetBounds(), line, 18f, -40f);
            using (var p = new Pen(line, 8f) { LineJoin = LineJoin.Round }) g.DrawPath(p, path);
            using (var hl = new SolidBrush(Color.FromArgb(150, 255, 255, 255))) g.FillEllipse(hl, 55, 55, 30, 22);
        }
        Save(bmp, name);
    }
}
