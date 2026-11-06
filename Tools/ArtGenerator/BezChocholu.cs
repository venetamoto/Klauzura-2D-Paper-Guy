using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

// Z vyrezanych kreseb hrdiny smaze chocholku nad hlavou (guma ji v Varvara2 smaze).
// Hlava = nejvetsi bila plocha; vse nad jejim obrysem zmizi. Rozmer obrazku zustane stejny (pivot u nohou sedi).
public static class BezChocholu
{
    public static void Render(string outDir, string[] files)
    {
        Directory.CreateDirectory(outDir);
        foreach (string file in files)
        {
            using var src = new Bitmap(file);
            var bmp = new Bitmap(src);
            int w = bmp.Width, h = bmp.Height;
            var light = new bool[w, h];
            for (int y = 0; y < h; y++)
                for (int x = 0; x < w; x++)
                {
                    Color c = bmp.GetPixel(x, y);
                    light[x, y] = c.A > 240 && c.R > 215;
                }

            var seen = new bool[w, h];
            int best = 0, top = 0, cx = 0, headW = 0, bot = 0, left = 0;
            for (int y = 0; y < h; y++)
                for (int x = 0; x < w; x++)
                {
                    if (!light[x, y] || seen[x, y]) continue;
                    int count = 0, minY = y, maxY = y, sumX = 0, lo = x, hi = x;
                    var q = new Queue<(int, int)>();
                    q.Enqueue((x, y)); seen[x, y] = true;
                    while (q.Count > 0)
                    {
                        var (px, py) = q.Dequeue();
                        count++; minY = Math.Min(minY, py); maxY = Math.Max(maxY, py); sumX += px; lo = Math.Min(lo, px); hi = Math.Max(hi, px);
                        foreach (var (nx, ny) in new[] { (px - 1, py), (px + 1, py), (px, py - 1), (px, py + 1) })
                            if (nx >= 0 && ny >= 0 && nx < w && ny < h && light[nx, ny] && !seen[nx, ny]) { seen[nx, ny] = true; q.Enqueue((nx, ny)); }
                    }
                    if (count > best) { best = count; top = minY; bot = maxY; left = lo; cx = sumX / count; headW = hi - lo; }
                }

            // hlava = kruh; nad stredem hlavy zmizi vse, co je dal nez obrys (chocholka), obrys zustane kulaty
            float hcx = left + headW / 2f, hcy = (top + bot) / 2f;
            float r = Math.Max(headW, bot - top) / 2f;
            float t = Math.Max(6f, headW * 0.08f);
            int cut = top;
            for (int y = 0; y < (int)hcy; y++)
                for (int x = 0; x < w; x++)
                {
                    float dx = x - hcx, dy = y - hcy;
                    if (dx * dx + dy * dy > (r + t) * (r + t)) bmp.SetPixel(x, y, Color.Transparent);
                }

            string name = Path.GetFileNameWithoutExtension(file) + "_Bez.png";
            bmp.Save(Path.Combine(outDir, name));
            Console.WriteLine(name + " hlava nahore " + top + " obrys " + t + " smazano nad " + cut);
            bmp.Dispose();
        }
    }
}
