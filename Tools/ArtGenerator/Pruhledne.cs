using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

// Bila (papir) -> pruhledna, zustane jen tuzka. Pak doostreni hran (unsharp na alfe).
static class Pruhledne
{
    public static bool Tvrde;

    public static void Udelej(string cesta)
    {
        Bitmap src = new Bitmap(cesta);
        int w = src.Width, h = src.Height;
        float[] a = new float[w * h];
        int nejtmavsi = 255;
        for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
            {
                Color c = src.GetPixel(x, y);
                if (c.A < 20) continue;
                int l = (c.R * 30 + c.G * 59 + c.B * 11) / 100;
                if (l < nejtmavsi) nejtmavsi = l;
            }
        float ink = Math.Min(nejtmavsi + 10, 80);
        const float papir = 225f;
        for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
            {
                Color c = src.GetPixel(x, y);
                float l = (c.R * 30 + c.G * 59 + c.B * 11) / 100f;
                float t = (papir - l) / (papir - ink);
                t = Math.Clamp(t * 1.1f, 0f, 1f);
                a[y * w + x] = t * c.A / 255f;
            }
        src.Dispose();

        // doostreni: a + 0.8 * (a - rozmazane a)
        float[] b = new float[w * h];
        for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
            {
                float s = 0; int n = 0;
                for (int dy = -2; dy <= 2; dy++)
                    for (int dx = -2; dx <= 2; dx++)
                    {
                        int xx = x + dx, yy = y + dy;
                        if (xx < 0 || yy < 0 || xx >= w || yy >= h) continue;
                        s += a[yy * w + xx]; n++;
                    }
                b[y * w + x] = s / n;
            }

        Bitmap dst = new Bitmap(w, h, PixelFormat.Format32bppArgb);
        for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
            {
                float v = a[y * w + x];
                v = Math.Clamp(v + 0.8f * (v - b[y * w + x]), 0f, 1f);
                if (Tvrde) { v = Math.Clamp((v - 0.2f) / 0.5f, 0f, 1f); v = v * v * (3f - 2f * v); }
                if (v < 0.04f) v = 0f;
                dst.SetPixel(x, y, Color.FromArgb((int)(v * 255), 28, 28, 30));
            }
        dst.Save(cesta + ".tmp.png", ImageFormat.Png);
        dst.Dispose();
        File.Move(cesta + ".tmp.png", cesta, true);
        Console.WriteLine(cesta + ": jen tuzka, ink " + ink);
    }
}
