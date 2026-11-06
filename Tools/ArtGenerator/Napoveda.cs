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
                text.AddString("Stiskni", font, (int)FontStyle.Regular, 70, new PointF(12, 28), StringFormat.GenericDefault);
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
