using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;

// napis "Stiskni [F]" jako kresba tuzkou, nekolik roztresenych snimku
static class Napoveda
{
    public static void Render(string outDir, int frames)
    {
        Directory.CreateDirectory(outDir);
        for (int f = 0; f < frames; f++)
        {
            var rnd = new Random(1234 + f * 77);
            using var bmp = new Bitmap(460, 170, PixelFormat.Format32bppArgb);
            using (var g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.Clear(Color.Transparent);
                g.TranslateTransform(230, 85);
                g.RotateTransform((float)(rnd.NextDouble() * 3 - 1.5));
                g.TranslateTransform(-220 + J(rnd, 2), -75 + J(rnd, 2));

                using var font = new FontFamily("Ink Free");
                var text = new GraphicsPath();
                text.AddString("Press", font, (int)FontStyle.Regular, 70, new PointF(12, 28), StringFormat.GenericDefault);
                float right = text.GetBounds().Right;
                Pencil(g, text, rnd);

                // klavesa nakreslena rukou
                float kx = right + 22, ky = 22, ks = 104;
                for (int pass = 0; pass < 2; pass++)
                {
                    var box = new GraphicsPath();
                    box.AddLines(Wobbly(rnd, new PointF[] {
                        new(kx, ky), new(kx + ks, ky), new(kx + ks, ky + ks), new(kx, ky + ks), new(kx, ky + 3) }));
                    using var pen = new Pen(Color.FromArgb(pass == 0 ? 255 : 170, 25, 25, 35), pass == 0 ? 6f : 3f);
                    pen.LineJoin = LineJoin.Round;
                    g.DrawPath(pen, box);
                }
                var key = new GraphicsPath();
                key.AddString("F", font, (int)FontStyle.Bold, 78, new PointF(kx + 26, ky + 8), StringFormat.GenericDefault);
                Pencil(g, key, rnd);
            }
            Grain(bmp, rnd);
            Halo(bmp);
            string path = Path.Combine(outDir, "Stiskni_F_" + f + ".png");
            bmp.Save(path, ImageFormat.Png);
            Console.WriteLine(path);
        }
    }

    // jeden radek textu tuzkou (epilog na bile obrazovce)
    public static void Text(string path, string text)
    {
        var rnd = new Random(99);
        using var font = new FontFamily("Ink Free");
        var p = new GraphicsPath();
        p.AddString(text, font, (int)FontStyle.Regular, 90, new PointF(0, 0), StringFormat.GenericDefault);
        RectangleF r = p.GetBounds();
        using var bmp = new Bitmap((int)r.Width + 60, (int)r.Height + 60, PixelFormat.Format32bppArgb);
        using (var g = Graphics.FromImage(bmp))
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.Clear(Color.Transparent);
            g.TranslateTransform(30 - r.X, 30 - r.Y);
            Pencil(g, p, rnd);
        }
        Grain(bmp, rnd);
        bmp.Save(path, ImageFormat.Png);
        Console.WriteLine(path + " " + bmp.Width + "x" + bmp.Height);
    }

    // tlacitko do menu: napis Ink Free v rucne nakreslenem ramecku
    public static void Tlacitko(string path, string text)
    {
        var rnd = new Random(text.Length * 13 + 5);
        using var font = new FontFamily("Ink Free");
        var p = new GraphicsPath();
        p.AddString(text, font, (int)FontStyle.Regular, 80, new PointF(0, 0), StringFormat.GenericDefault);
        RectangleF r = p.GetBounds();
        int pad = 40;
        int w = (int)r.Width + pad * 2 + 20, h = (int)r.Height + pad * 2 + 10;
        using var bmp = new Bitmap(w, h, PixelFormat.Format32bppArgb);
        using (var g = Graphics.FromImage(bmp))
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.Clear(Color.Transparent);
            using (var b = new SolidBrush(Color.FromArgb(150, 255, 255, 250)))
                g.FillRectangle(b, 14, 14, w - 28, h - 28);
            for (int pass = 0; pass < 2; pass++)
            {
                var box = new GraphicsPath();
                box.AddLines(Wobbly(rnd, new PointF[] {
                    new(12, 12), new(w - 12, 14), new(w - 14, h - 12), new(14, h - 14), new(12, 16) }));
                using var pen = new Pen(Color.FromArgb(pass == 0 ? 255 : 160, 25, 25, 35), pass == 0 ? 5f : 2.5f) { LineJoin = LineJoin.Round };
                g.DrawPath(pen, box);
            }
            g.TranslateTransform(pad + 10 - r.X, pad + 5 - r.Y);
            Pencil(g, p, rnd);
        }
        Grain(bmp, rnd);
        bmp.Save(path, ImageFormat.Png);
        Console.WriteLine(path + " " + w + "x" + h);
    }

    // nekolik radku textu tuzkou pod sebou (ovladani, autori); | = novy radek
    public static void Radky(string path, string text)
    {
        var rnd = new Random(text.Length);
        using var font = new FontFamily("Ink Free");
        var all = new GraphicsPath();
        all.FillMode = FillMode.Winding;
        string[] lines = text.Split('|');
        for (int i = 0; i < lines.Length; i++)
        {
            if (lines[i].Length == 0) continue;
            var p = new GraphicsPath();
            p.AddString(lines[i], font, (int)FontStyle.Regular, 64, new PointF(0, 0), StringFormat.GenericDefault);
            using var m = new Matrix();
            m.Translate(J(rnd, 6f), i * 95f + J(rnd, 3f));
            m.Rotate(J(rnd, 1.2f));
            p.Transform(m);
            all.AddPath(p, false);
        }
        RectangleF r = all.GetBounds();
        using var bmp = new Bitmap((int)r.Width + 80, (int)r.Height + 80, PixelFormat.Format32bppArgb);
        using (var g = Graphics.FromImage(bmp))
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.Clear(Color.Transparent);
            g.TranslateTransform(40 - r.X, 40 - r.Y);
            Pencil(g, all, rnd);
        }
        Grain(bmp, rnd);
        bmp.Save(path, ImageFormat.Png);
        Console.WriteLine(path + " " + bmp.Width + "x" + bmp.Height);
    }

    // tuzka nakreslena tuzkou (lezi na ostrove v dobrem konci), spicka vpravo
    public static void TuzkaKresba(string path)
    {
        var rnd = new Random(31);
        using var bmp = new Bitmap(440, 110, PixelFormat.Format32bppArgb);
        using (var g = Graphics.FromImage(bmp))
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.Clear(Color.Transparent);
            float top = 30, bot = 80, mid = 55;
            var body = new PointF[] { new(20, top), new(320, top), new(320, bot), new(20, bot) };
            using (var b = new SolidBrush(Color.FromArgb(255, 245, 243, 238))) g.FillPolygon(b, body);
            // srafovani tela
            using (var hp = new Pen(Color.FromArgb(120, 40, 40, 45), 2f))
                for (float x = 30; x < 315; x += 9)
                    g.DrawLine(hp, x + J(rnd, 2), top + 4, x + 12 + J(rnd, 2), bot - 4);
            // gumicka a plechove ocko
            using (var b = new SolidBrush(Color.FromArgb(255, 70, 68, 72))) g.FillRectangle(b, 20, top, 32, bot - top);
            using (var p = new Pen(Color.FromArgb(230, 25, 25, 30), 3f))
            {
                g.DrawLine(p, 58, top, 58, bot);
                g.DrawLine(p, 64, top, 64, bot);
            }
            // spicka
            var tip = new PointF[] { new(320, top), new(400, mid - 4), new(412, mid), new(400, mid + 4), new(320, bot) };
            using (var b = new SolidBrush(Color.FromArgb(255, 235, 225, 210))) g.FillPolygon(b, tip);
            using (var b = new SolidBrush(Color.FromArgb(255, 30, 30, 35))) g.FillPolygon(b, new PointF[] { new(392, mid - 7), new(414, mid), new(392, mid + 7) });
            var outline = new GraphicsPath();
            outline.AddPolygon(new PointF[] { new(20, top), new(320, top), new(400, mid - 4), new(414, mid), new(400, mid + 4), new(320, bot), new(20, bot) });
            outline.AddLine(320, top, 320, bot);
            for (int i = 0; i < 3; i++)
            {
                var c = (GraphicsPath)outline.Clone();
                using var m = new Matrix();
                m.Translate(J(rnd, 1.8f), J(rnd, 1.8f));
                c.Transform(m);
                using var pen = new Pen(Color.FromArgb(i == 0 ? 255 : 150, 25, 25, 30), i == 0 ? 4f : 2f) { LineJoin = LineJoin.Round };
                g.DrawPath(pen, c);
            }
        }
        Grain(bmp, rnd);
        bmp.Save(path, ImageFormat.Png);
        Console.WriteLine(path);
    }

    // krive psane pismo: kazde pismeno trochu natocene, poskakuje a radek leze nahoru
    public static void Rukopis(string path, string text)
    {
        var rnd = new Random(7);
        using var font = new FontFamily("Segoe Script");
        using var measureFont = new Font(font, 100, FontStyle.Regular, GraphicsUnit.Pixel);
        var all = new GraphicsPath();
        all.FillMode = FillMode.Winding;
        float x = 0f;
        float lineY = 0f;
        int col = 0;
        using (var tmp = new Bitmap(10, 10))
        using (var mg = Graphics.FromImage(tmp))
        {
            for (int i = 0; i < text.Length; i++)
            {
                if (text[i] == '|')
                {
                    x = 0f;
                    lineY += 150f;
                    col = 0;
                    continue;
                }
                col++;
                string ch = text[i].ToString();
                float adv = ch == " " ? 62f : mg.MeasureString(ch, measureFont, PointF.Empty, StringFormat.GenericTypographic).Width;
                if (ch != " ")
                {
                    var p = new GraphicsPath();
                    float size = 100f * (0.9f + (float)rnd.NextDouble() * 0.2f);
                    p.AddString(ch, font, (int)FontStyle.Regular, size, new PointF(0, 0), StringFormat.GenericTypographic);
                    using var m = new Matrix();
                    float drift = -col * 1.6f;
                    m.Translate(x + lineY * 0.12f, J(rnd, 9f) + drift + lineY);
                    m.Rotate(J(rnd, 8f));
                    p.Transform(m);
                    all.AddPath(p, false);
                }
                x += Math.Max(adv, 34f) * (0.92f + (float)rnd.NextDouble() * 0.12f);
            }
        }
        RectangleF r = all.GetBounds();
        using var bmp = new Bitmap((int)r.Width + 100, (int)r.Height + 80, PixelFormat.Format32bppArgb);
        using (var g = Graphics.FromImage(bmp))
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.Clear(Color.Transparent);
            g.TranslateTransform(40 - r.X, 40 - r.Y);
            Pencil(g, all, rnd);
        }
        Grain(bmp, rnd);
        bmp.Save(path, ImageFormat.Png);
        Console.WriteLine(path + " " + bmp.Width + "x" + bmp.Height);
    }

    static float J(Random rnd, float a) => (float)(rnd.NextDouble() * 2 - 1) * a;

    static void Pencil(Graphics g, GraphicsPath p, Random rnd)
    {
        using (var b = new SolidBrush(Color.FromArgb(255, 25, 25, 35))) g.FillPath(b, p);
        for (int i = 0; i < 3; i++)
        {
            var c = (GraphicsPath)p.Clone();
            using var m = new Matrix();
            m.Translate(J(rnd, 1.6f), J(rnd, 1.6f));
            c.Transform(m);
            using var pen = new Pen(Color.FromArgb(170, 20, 20, 30), 2.2f);
            g.DrawPath(pen, c);
        }
    }

    static PointF[] Wobbly(Random rnd, PointF[] corners)
    {
        var pts = new System.Collections.Generic.List<PointF>();
        for (int i = 0; i < corners.Length - 1; i++)
            for (int s = 0; s < 6; s++)
            {
                float t = s / 6f;
                pts.Add(new PointF(corners[i].X + (corners[i + 1].X - corners[i].X) * t + J(rnd, 1.8f),
                                   corners[i].Y + (corners[i + 1].Y - corners[i].Y) * t + J(rnd, 1.8f)));
            }
        pts.Add(corners[^1]);
        return pts.ToArray();
    }

    // bily okraj kolem kresby, aby byla videt i na tmave dvere
    static void Halo(Bitmap bmp)
    {
        int w = bmp.Width, h = bmp.Height, r = 7;
        var a = new int[w, h];
        for (int y = 0; y < h; y++) for (int x = 0; x < w; x++) a[x, y] = bmp.GetPixel(x, y).A;
        for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
            {
                if (a[x, y] > 200) continue;
                int best = 0;
                for (int dy = -r; dy <= r && best < 255; dy += 1)
                    for (int dx = -r; dx <= r; dx += 1)
                    {
                        int xx = x + dx, yy = y + dy;
                        if (xx < 0 || yy < 0 || xx >= w || yy >= h || dx * dx + dy * dy > r * r) continue;
                        if (a[xx, yy] > 60) { best = 255; break; }
                    }
                if (best == 0) continue;
                Color c = bmp.GetPixel(x, y);
                float k = c.A / 255f;
                bmp.SetPixel(x, y, Color.FromArgb(Math.Max((int)c.A, 235), (int)(255 + (c.R - 255) * k), (int)(255 + (c.G - 255) * k), (int)(252 + (c.B - 252) * k)));
            }
    }

    // zrnitost tuzky
    static void Grain(Bitmap bmp, Random rnd)
    {
        for (int y = 0; y < bmp.Height; y++)
            for (int x = 0; x < bmp.Width; x++)
            {
                Color c = bmp.GetPixel(x, y);
                if (c.A == 0) continue;
                double k = 0.88 + rnd.NextDouble() * 0.12;
                if (rnd.NextDouble() < 0.04) k *= 0.6;
                bmp.SetPixel(x, y, Color.FromArgb((int)(c.A * k), c.R, c.G, c.B));
            }
    }
}
