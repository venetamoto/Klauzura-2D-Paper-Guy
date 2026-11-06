using System;
using System.Collections.Generic;
using System.Drawing;

// Smaze z vyrezane kresby slabe cary pohybu kolem postavy (vypadaly jako treti noha).
// Necha jen nejvetsi pevnou cast (postavu) a jeji mekky okraj do par px.
public static class Ocistit
{
    const int Okraj = 6;

    public static void Render(string[] files)
    {
        foreach (string file in files)
        {
            Bitmap bmp;
            using (var src = new Bitmap(file)) bmp = new Bitmap(src);
            int w = bmp.Width, h = bmp.Height;
            var solid = new bool[w, h];
            for (int y = 0; y < h; y++)
                for (int x = 0; x < w; x++)
                    solid[x, y] = bmp.GetPixel(x, y).A > 140;

            // nejvetsi souvisla pevna cast = postava
            var label = new int[w, h];
            int best = 0, bestId = 0, id = 0;
            for (int y = 0; y < h; y++)
                for (int x = 0; x < w; x++)
                {
                    if (!solid[x, y] || label[x, y] != 0) continue;
                    id++;
                    int count = 0;
                    var q = new Queue<(int, int)>();
                    q.Enqueue((x, y)); label[x, y] = id;
                    while (q.Count > 0)
                    {
                        var (px, py) = q.Dequeue();
                        count++;
                        for (int dy = -1; dy <= 1; dy++)
                            for (int dx = -1; dx <= 1; dx++)
                            {
                                int nx = px + dx, ny = py + dy;
                                if (nx < 0 || ny < 0 || nx >= w || ny >= h || !solid[nx, ny] || label[nx, ny] != 0) continue;
                                label[nx, ny] = id; q.Enqueue((nx, ny));
                            }
                    }
                    if (count > best) { best = count; bestId = id; }
                }

            // vzdalenost od postavy
            var dist = new int[w, h];
            var dq = new Queue<(int, int)>();
            for (int y = 0; y < h; y++)
                for (int x = 0; x < w; x++)
                {
                    dist[x, y] = int.MaxValue;
                    if (label[x, y] == bestId) { dist[x, y] = 0; dq.Enqueue((x, y)); }
                }
            while (dq.Count > 0)
            {
                var (x, y) = dq.Dequeue();
                int d = dist[x, y] + 1;
                if (d > Okraj) continue;
                foreach (var (nx, ny) in new[] { (x - 1, y), (x + 1, y), (x, y - 1), (x, y + 1) })
                    if (nx >= 0 && ny >= 0 && nx < w && ny < h && dist[nx, ny] > d) { dist[nx, ny] = d; dq.Enqueue((nx, ny)); }
            }

            int removed = 0;
            for (int y = 0; y < h; y++)
                for (int x = 0; x < w; x++)
                    if (dist[x, y] > Okraj && bmp.GetPixel(x, y).A > 0)
                    {
                        bmp.SetPixel(x, y, Color.Transparent);
                        removed++;
                    }
            // stred hlavy (nejvetsi bila plocha) - podle nej se v Unity nastavi pivot x
            var seen = new bool[w, h];
            int hb = 0; float hc = 0f;
            for (int y = 0; y < h; y++)
                for (int x = 0; x < w; x++)
                {
                    Color c0 = bmp.GetPixel(x, y);
                    if (seen[x, y] || c0.A < 240 || c0.R < 215) continue;
                    int cnt = 0, lo = x, hi = x;
                    var hq = new Queue<(int, int)>();
                    hq.Enqueue((x, y)); seen[x, y] = true;
                    while (hq.Count > 0)
                    {
                        var (px, py) = hq.Dequeue();
                        cnt++; lo = Math.Min(lo, px); hi = Math.Max(hi, px);
                        foreach (var (nx, ny) in new[] { (px - 1, py), (px + 1, py), (px, py - 1), (px, py + 1) })
                        {
                            if (nx < 0 || ny < 0 || nx >= w || ny >= h || seen[nx, ny]) continue;
                            Color cn = bmp.GetPixel(nx, ny);
                            if (cn.A < 240 || cn.R < 215) continue;
                            seen[nx, ny] = true; hq.Enqueue((nx, ny));
                        }
                    }
                    if (cnt > hb) { hb = cnt; hc = (lo + hi) / 2f; }
                }
            Console.WriteLine(file + " stred hlavy x " + hc + " z " + w + " (pivot x = " + (hc / w).ToString("0.0000") + ")");
            bmp.Save(file);
            bmp.Dispose();
            Console.WriteLine(file + " smazano " + removed + " px");
        }
    }
}
