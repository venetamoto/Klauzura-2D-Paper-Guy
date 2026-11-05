using System;
using System.Drawing;
using System.Drawing.Drawing2D;

// Okraj sesitu na lavici: mekky stin a dva listy pod hornim listem, kazdy trochu jinak natoceny.
public static class Okraj
{
    public const float PPU = 40f;
    public const float Margin = 1.5f;

    public static void Render(string file, string edgeFile, string paperFile, float paperW, float paperH)
    {
        Color tone = Average(paperFile);
        int W = (int)((paperW + Margin * 2) * PPU), H = (int)((paperH + Margin * 2) * PPU);
        using var b = new Bitmap(W, H);
        using var g = Graphics.FromImage(b);
        g.SmoothingMode = SmoothingMode.AntiAlias;
        var rnd = new Random(3);

        float px = Margin * PPU, py = Margin * PPU, pw = paperW * PPU, ph = paperH * PPU;

        // stin pod sesitem, posunuty doprava dolu
        for (int i = 30; i >= 0; i -= 2)
        {
            using var br = new SolidBrush(Color.FromArgb(9, 20, 10, 0));
            g.FillRectangle(br, px + 22 - i, py + 30 - i, pw + i * 2, ph + i * 2);
        }

        Sheet(g, rnd, px, py, pw, ph, 0.4f, 16f, 12f, Shade(tone, 0.86f));
        Sheet(g, rnd, px, py, pw, ph, -0.25f, -9f, 7f, Shade(tone, 0.93f));
        b.Save(file);

        // horni list: okraj trochu ztmavne, jako by se papir zvedal
        int ew = (int)(paperW * PPU), eh = (int)(paperH * PPU);
        using var e = new Bitmap(ew, eh);
        int band = (int)(0.7f * PPU);
        for (int y = 0; y < eh; y++)
            for (int x = 0; x < ew; x++)
            {
                int d = Math.Min(Math.Min(x, ew - 1 - x), Math.Min(y, eh - 1 - y));
                if (d >= band) continue;
                float k = 1f - d / (float)band;
                e.SetPixel(x, y, Color.FromArgb((int)(k * k * 70), 30, 25, 20));
            }
        e.Save(edgeFile);
    }

    static Color Shade(Color c, float k) => Color.FromArgb((int)(c.R * k), (int)(c.G * k), (int)(c.B * k));

    static Color Average(string file)
    {
        using var img = new Bitmap(file);
        long r = 0, gg = 0, bl = 0, n = 0;
        for (int y = 0; y < img.Height; y += 8)
            for (int x = 0; x < img.Width; x += 8)
            {
                Color c = img.GetPixel(x, y);
                r += c.R; gg += c.G; bl += c.B; n++;
            }
        return Color.FromArgb((int)(r / n), (int)(gg / n), (int)(bl / n));
    }

    // list papiru s mirne zvlnenymi okraji
    static void Sheet(Graphics g, Random rnd, float x, float y, float w, float h, float angle, float dx, float dy, Color c)
    {
        var pts = new System.Collections.Generic.List<PointF>();
        void Edge(float x1, float y1, float x2, float y2)
        {
            int n = 40;
            for (int i = 0; i < n; i++)
            {
                float k = i / (float)n;
                pts.Add(new PointF(x1 + (x2 - x1) * k + (float)(rnd.NextDouble() - 0.5) * 2.5f,
                                   y1 + (y2 - y1) * k + (float)(rnd.NextDouble() - 0.5) * 2.5f));
            }
        }
        Edge(x, y, x + w, y);
        Edge(x + w, y, x + w, y + h);
        Edge(x + w, y + h, x, y + h);
        Edge(x, y + h, x, y);

        var state = g.Save();
        g.TranslateTransform(x + w / 2 + dx, y + h / 2 + dy);
        g.RotateTransform(angle);
        g.TranslateTransform(-(x + w / 2), -(y + h / 2));
        using (var br = new SolidBrush(c)) g.FillPolygon(br, pts.ToArray());
        using (var pen = new Pen(Color.FromArgb(120, 150, 145, 135), 1.6f)) g.DrawPolygon(pen, pts.ToArray());
        g.Restore(state);
    }
}
