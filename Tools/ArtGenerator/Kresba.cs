using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;

// Kresby postavy od autorky -> sprity: papir kolem zmizi, postava je cela neprusvitna.
// Male mezery v uhlu (plastik, nohy) se zaplni, vnitrek hlavy zustane bily.
// Vyrez je stredem na postave a dole ma 6 px, aby pivot byl u chodidel.
// Vypise sirku hlavy v px - podle ni se v Unity nastavi velikost, aby vsechny kresby mely stejne velkou hlavu.
public static class Kresba
{
    const int Close = 6;

    public static void Render(string outDir, string[] pairs)
    {
        foreach (string pair in pairs)
        {
            string[] p = pair.Split('=');
            using var image = new Bitmap(p[1]);
            if (p[0].EndsWith("_Mrk"))
            {
                Blink(image, 932, 570);
                Blink(image, 1083, 568);
            }
            using var sprite = Cut(image, out int head);
            sprite.Save(Path.Combine(outDir, p[0] + ".png"));
            Console.WriteLine(p[0] + " " + sprite.Width + "x" + sprite.Height + " hlava " + head);
        }
    }

    static Bitmap Cut(Bitmap image, out int head)
    {
        int w = image.Width, h = image.Height;
        var lum = new float[w, h];
        for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
            {
                Color c = image.GetPixel(x, y);
                lum[x, y] = (c.R + c.G + c.B) / 3f;
            }

        // tuzka roztazena o par px, aby se uzavrely mezery v cmarani
        var ink = new bool[w, h];
        var near = new int[w, h];
        var queue = new Queue<(int, int)>();
        for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
            {
                near[x, y] = int.MaxValue;
                if (lum[x, y] < 200f) { near[x, y] = 0; queue.Enqueue((x, y)); }
            }
        Spread(queue, near, w, h, Close);
        for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
                ink[x, y] = near[x, y] <= Close;

        // venek = papir spojeny s okrajem obrazku, ktery neni u tuzky
        var outside = new int[w, h];
        for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
            {
                outside[x, y] = int.MaxValue;
                bool border = x == 0 || y == 0 || x == w - 1 || y == h - 1;
                if (border && !ink[x, y]) { outside[x, y] = 0; queue.Enqueue((x, y)); }
            }
        while (queue.Count > 0)
        {
            var (x, y) = queue.Dequeue();
            foreach (var (nx, ny) in Around(x, y, w, h))
                if (!ink[nx, ny] && outside[nx, ny] == int.MaxValue) { outside[nx, ny] = 0; queue.Enqueue((nx, ny)); }
        }
        // a pas kolem venku vratit zpet na venek (jinak by kolem postavy byl bily lem)
        for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
                if (outside[x, y] == 0) queue.Enqueue((x, y));
        Spread(queue, outside, w, h, Close + 1);

        int minX = w, maxX = 0, minY = h, maxY = 0;
        for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
                if (lum[x, y] < 200f)
                {
                    minX = Math.Min(minX, x); maxX = Math.Max(maxX, x);
                    minY = Math.Min(minY, y); maxY = Math.Max(maxY, y);
                }

        head = HeadWidth(lum, outside, w, h, minX, maxX, minY, maxY);

        int cx = (minX + maxX) / 2;
        int half = Math.Max(cx - minX, maxX - cx) + 10;
        int top = minY - 10, bottom = maxY + 6;
        var sprite = new Bitmap(half * 2, bottom - top);
        for (int y = top; y < bottom; y++)
            for (int x = cx - half; x < cx + half; x++)
            {
                Color c = Color.Transparent;
                if (x >= 0 && y >= 0 && x < w && y < h)
                {
                    float l = lum[x, y];
                    if (outside[x, y] <= Close + 1 && l > 120f)
                    {
                        // okraj: jen tuzka, papir pryc
                        float a = (float)Math.Pow(Math.Clamp((245f - l) / 110f, 0f, 1f), 0.75);
                        c = Color.FromArgb((int)(a * 255), 28, 28, 30);
                    }
                    else
                    {
                        int v = (int)Math.Clamp(l, 0f, 255f);
                        if (v < 170) v = (int)(v * 0.85f);
                        c = Color.FromArgb(255, v, v, v);
                    }
                }
                sprite.SetPixel(x - (cx - half), y - top, c);
            }
        return sprite;
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

    // hlava = nejvetsi uzavrena svetla plocha uvnitr postavy
    static int HeadWidth(float[,] lum, int[,] outside, int w, int h, int minX, int maxX, int minY, int maxY)
    {
        var seen = new bool[w, h];
        int best = 0, bestWidth = 0;
        for (int y = minY; y <= maxY; y++)
            for (int x = minX; x <= maxX; x++)
            {
                if (seen[x, y] || outside[x, y] != int.MaxValue || lum[x, y] < 215f) continue;
                int count = 0, lo = x, hi = x;
                var q = new Queue<(int, int)>();
                q.Enqueue((x, y));
                seen[x, y] = true;
                while (q.Count > 0)
                {
                    var (px, py) = q.Dequeue();
                    count++; lo = Math.Min(lo, px); hi = Math.Max(hi, px);
                    foreach (var (nx, ny) in Around(px, py, w, h))
                        if (!seen[nx, ny] && outside[nx, ny] == int.MaxValue && lum[nx, ny] >= 215f) { seen[nx, ny] = true; q.Enqueue((nx, ny)); }
                }
                if (count > best) { best = count; bestWidth = hi - lo; }
            }
        return bestWidth;
    }

    static void Blink(Bitmap image, int cx, int cy)
    {
        using var g = Graphics.FromImage(image);
        g.SmoothingMode = SmoothingMode.AntiAlias;
        using (var white = new SolidBrush(Color.White)) g.FillEllipse(white, cx - 20, cy - 20, 40, 40);
        using var pen = new Pen(Color.FromArgb(35, 35, 38), 6f) { StartCap = LineCap.Round, EndCap = LineCap.Round };
        g.DrawArc(pen, cx - 16, cy - 10, 32, 16, 20, 140);
    }
}
