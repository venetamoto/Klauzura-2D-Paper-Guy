using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;

// Snimky animace od autorky (z videa): zvetsit, papir pryc (tuzka zustane jak je),
// vsechny oriznout stejnym obdelnikem, aby postava mezi snimky neposkakovala.
// Vypise sirku hlavy a vysku chodidel v prvnim snimku (pro PPU a pivot).
public static class Animace
{
    public static int Zoom = 5;
    const int Close = 2;

    public static void Render(string outDir, string prefix, string[] files)
    {
        var frames = new List<Bitmap>();
        int minX = int.MaxValue, minY = int.MaxValue, maxX = 0, maxY = 0, feet = 0, head = 0;
        for (int i = 0; i < files.Length; i++)
        {
            using var src = new Bitmap(files[i]);
            var big = new Bitmap(src.Width * Zoom, src.Height * Zoom);
            using (var g = Graphics.FromImage(big))
            {
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                g.DrawImage(src, 0, 0, big.Width, big.Height);
            }
            var cut = Cut(big, out int x0, out int y0, out int x1, out int y1, out int hw);
            big.Dispose();
            frames.Add(cut);
            minX = Math.Min(minX, x0); minY = Math.Min(minY, y0); maxX = Math.Max(maxX, x1); maxY = Math.Max(maxY, y1);
            if (i == 0) { feet = y1; head = hw; }
        }

        int pad = 8;
        minX -= pad; minY -= pad; maxX += pad; maxY += pad;
        int w = maxX - minX, h = maxY - minY;
        for (int i = 0; i < frames.Count; i++)
        {
            using var crop = new Bitmap(w, h);
            using (var g = Graphics.FromImage(crop))
                g.DrawImage(frames[i], new Rectangle(0, 0, w, h), new Rectangle(minX, minY, w, h), GraphicsUnit.Pixel);
            crop.Save(Path.Combine(outDir, prefix + "_" + (i + 1) + ".png"));
            frames[i].Dispose();
        }
        Console.WriteLine($"{w}x{h} hlava {head} chodidla odspodu {maxY - feet} px (pivot y = {(maxY - feet) / (float)h:0.0000}) stred x {(0.5f):0.0}");
    }

    static Bitmap Cut(Bitmap image, out int x0, out int y0, out int x1, out int y1, out int headW)
    {
        int w = image.Width, h = image.Height;
        var lum = new float[w, h];
        for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
            {
                Color c = image.GetPixel(x, y);
                lum[x, y] = (c.R + c.G + c.B) / 3f;
            }

        var near = new int[w, h];
        var q = new Queue<(int, int)>();
        for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
            {
                near[x, y] = int.MaxValue;
                if (lum[x, y] < 200f) { near[x, y] = 0; q.Enqueue((x, y)); }
            }
        Spread(q, near, w, h, Close * Zoom);
        var ink = new bool[w, h];
        for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
                ink[x, y] = near[x, y] <= Close * Zoom;

        var outside = new int[w, h];
        for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
            {
                outside[x, y] = int.MaxValue;
                bool border = x == 0 || y == 0 || x == w - 1 || y == h - 1;
                if (border && !ink[x, y]) { outside[x, y] = 0; q.Enqueue((x, y)); }
            }
        while (q.Count > 0)
        {
            var (x, y) = q.Dequeue();
            foreach (var (nx, ny) in Around(x, y, w, h))
                if (!ink[nx, ny] && outside[nx, ny] == int.MaxValue) { outside[nx, ny] = 0; q.Enqueue((nx, ny)); }
        }
        for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
                if (outside[x, y] == 0) q.Enqueue((x, y));
        int band = Close * Zoom + 1;
        Spread(q, outside, w, h, band);

        x0 = w; y0 = h; x1 = 0; y1 = 0;
        var res = new Bitmap(w, h);
        for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
            {
                float l = lum[x, y];
                Color c;
                if (outside[x, y] <= band)
                {
                    float a = Math.Clamp((245f - l) / 215f, 0f, 1f);
                    c = Color.FromArgb((int)(a * 255), 28, 28, 30);
                    if (a < 0.08f) c = Color.Transparent;
                }
                else
                {
                    int v = (int)Math.Clamp(l, 0f, 255f);
                    c = Color.FromArgb(255, v, v, v);
                }
                res.SetPixel(x, y, c);
                if (c.A > 40) { x0 = Math.Min(x0, x); y0 = Math.Min(y0, y); x1 = Math.Max(x1, x); y1 = Math.Max(y1, y); }
            }

        // hlava = nejvetsi uzavrena svetla plocha uvnitr postavy
        var seen = new bool[w, h];
        int best = 0; headW = 0;
        for (int y = y0; y <= y1; y++)
            for (int x = x0; x <= x1; x++)
            {
                if (seen[x, y] || outside[x, y] <= band || lum[x, y] < 215f) continue;
                int count = 0, lo = x, hi = x;
                var hq = new Queue<(int, int)>();
                hq.Enqueue((x, y)); seen[x, y] = true;
                while (hq.Count > 0)
                {
                    var (px, py) = hq.Dequeue();
                    count++; lo = Math.Min(lo, px); hi = Math.Max(hi, px);
                    foreach (var (nx, ny) in Around(px, py, w, h))
                        if (!seen[nx, ny] && outside[nx, ny] > band && lum[nx, ny] >= 215f) { seen[nx, ny] = true; hq.Enqueue((nx, ny)); }
                }
                if (count > best) { best = count; headW = hi - lo; }
            }
        return res;
    }

    static void Spread(Queue<(int, int)> queue, int[,] dist, int w, int h, int max)
    {
        while (queue.Count > 0)
        {
            var (x, y) = queue.Dequeue();
            int d = dist[x, y] + 1;
            if (d > max) continue;
            foreach (var (nx, ny) in Around(x, y, w, h))
                if (dist[nx, ny] > d) { dist[nx, ny] = d; queue.Enqueue((nx, ny)); }
        }
    }

    static IEnumerable<(int, int)> Around(int x, int y, int w, int h)
    {
        if (x > 0) yield return (x - 1, y);
        if (x < w - 1) yield return (x + 1, y);
        if (y > 0) yield return (x, y - 1);
        if (y < h - 1) yield return (x, y + 1);
    }
}
