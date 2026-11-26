using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;

// Kresba hrdiny ostre pro obrazovku: bila pryc (jen tuzka), cary trochu silnejsi,
// zmenseni primo na cilove PPU (zadne velke zmensovani v Unity), kontrast a doostreni.
static class Ostre
{
    public static void Udelej(string zdroj, string cil, float cilPpu)
    {
        string meta = File.ReadAllText(cil + ".meta");
        float ppu = float.Parse(Regex.Match(meta, @"spritePixelsToUnits: ([0-9.]+)").Groups[1].Value, CultureInfo.InvariantCulture);
        float f = Math.Min(1f, cilPpu / ppu);

        Bitmap src = new Bitmap(zdroj);
        int w = src.Width, h = src.Height;
        float[] a = new float[w * h];
        for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
            {
                Color c = src.GetPixel(x, y);
                float l = (c.R * 30 + c.G * 59 + c.B * 11) / 100f;
                float t = Math.Clamp((225f - l) / 190f * 1.15f, 0f, 1f);
                a[y * w + x] = t * c.A / 255f;
            }
        src.Dispose();

        // cary o kousek silnejsi, aby po zmenseni nezmizely
        int r = Math.Max(0, (int)Math.Round(0.3f / f));
        if (r > 0)
        {
            float[] b = new float[w * h];
            for (int y = 0; y < h; y++)
                for (int x = 0; x < w; x++)
                {
                    float m = 0f;
                    for (int dy = -r; dy <= r; dy++)
                        for (int dx = -r; dx <= r; dx++)
                        {
                            if (dx * dx + dy * dy > r * r) continue;
                            int xx = x + dx, yy = y + dy;
                            if (xx < 0 || yy < 0 || xx >= w || yy >= h) continue;
                            m = Math.Max(m, a[yy * w + xx]);
                        }
                    b[y * w + x] = m;
                }
            a = b;
        }

        // zmenseni prumerem plochy
        int nw = Math.Max(1, (int)Math.Round(w * f)), nh = Math.Max(1, (int)Math.Round(h * f));
        float[] z = new float[nw * nh];
        for (int y = 0; y < nh; y++)
            for (int x = 0; x < nw; x++)
            {
                int x0 = (int)(x * (float)w / nw), x1 = Math.Max(x0 + 1, (int)((x + 1) * (float)w / nw));
                int y0 = (int)(y * (float)h / nh), y1 = Math.Max(y0 + 1, (int)((y + 1) * (float)h / nh));
                float s = 0; int n = 0;
                for (int yy = y0; yy < y1 && yy < h; yy++)
                    for (int xx = x0; xx < x1 && xx < w; xx++) { s += a[yy * w + xx]; n++; }
                z[y * nw + x] = n > 0 ? s / n : 0f;
            }

        // doostreni a kontrast
        Bitmap dst = new Bitmap(nw, nh, PixelFormat.Format32bppArgb);
        for (int y = 0; y < nh; y++)
            for (int x = 0; x < nw; x++)
            {
                float s = 0; int n = 0;
                for (int dy = -1; dy <= 1; dy++)
                    for (int dx = -1; dx <= 1; dx++)
                    {
                        int xx = x + dx, yy = y + dy;
                        if (xx < 0 || yy < 0 || xx >= nw || yy >= nh) continue;
                        s += z[yy * nw + xx]; n++;
                    }
                float v = z[y * nw + x];
                v = Math.Clamp(v + 0.6f * (v - s / n), 0f, 1f);
                v = Math.Clamp((v - 0.08f) / 0.55f, 0f, 1f);
                v = v * v * (3f - 2f * v);
                dst.SetPixel(x, y, Color.FromArgb((int)(v * 255), 22, 22, 24));
            }
        dst.Save(cil + ".tmp.png", ImageFormat.Png);
        dst.Dispose();
        File.Move(cil + ".tmp.png", cil, true);
        float novePpu = ppu * nw / w;
        meta = Regex.Replace(meta, @"spritePixelsToUnits: [0-9.]+", "spritePixelsToUnits: " + novePpu.ToString("0.#####", CultureInfo.InvariantCulture));
        File.WriteAllText(cil + ".meta", meta);
        Console.WriteLine(Path.GetFileName(cil) + ": " + w + "x" + h + " -> " + nw + "x" + nh + " (ppu " + ppu.ToString("0.##", CultureInfo.InvariantCulture) + " -> " + (ppu * nw / w).ToString("0.###", CultureInfo.InvariantCulture) + ", r " + r + ")");
    }
}
