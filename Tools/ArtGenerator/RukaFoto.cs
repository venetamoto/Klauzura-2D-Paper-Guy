using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;

// Fotky ruky autorky (s gumou / s tuzkou) -> vyrez bez steny podle obrysu, otoceni o 90 st. doleva
static class RukaFoto
{
    // pomocny nahled: znak "ruzovosti" (B - G) po rozmazani
    public static void Mapa(string vstup, string vystup)
    {
        using var src = new Bitmap(vstup);
        int w = src.Width / 4, h = src.Height / 4;
        using var mala = new Bitmap(src, w, h);
        using var dst = new Bitmap(w, h);
        for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
            {
                Color c = mala.GetPixel(x, y);
                int v = Math.Clamp((c.B - c.G + 40) * 4, 0, 255);
                dst.SetPixel(x, y, Color.FromArgb(v, v, v));
            }
        dst.Save(vystup, ImageFormat.Png);
    }

    // obrys.txt: 1. radek "x0 y0 n meritko pivX pivY" (vyrez n x n od (x0, y0), body obrysu jsou v nahledu * meritko,
    // pivX pivY = bod (spicka/guma) v nahledu), dalsi radky "x,y" body obrysu
    public static void Vyrez(string vstup, string obrysSoubor, string vystup)
    {
        string[] radky = File.ReadAllLines(obrysSoubor);
        string[] p = radky[0].Split(' ', StringSplitOptions.RemoveEmptyEntries);
        int X0 = int.Parse(p[0]), Y0 = int.Parse(p[1]), N = int.Parse(p[2]);
        float sk = float.Parse(p[3], CultureInfo.InvariantCulture);
        float bodX = float.Parse(p[4], CultureInfo.InvariantCulture) * sk, bodY = float.Parse(p[5], CultureInfo.InvariantCulture) * sk;
        var body = new System.Collections.Generic.List<PointF>();
        for (int i = 1; i < radky.Length; i++)
        {
            string[] xy = radky[i].Split(',', StringSplitOptions.RemoveEmptyEntries);
            if (xy.Length < 2) continue;
            body.Add(new PointF(float.Parse(xy[0], CultureInfo.InvariantCulture) * sk, float.Parse(xy[1], CultureInfo.InvariantCulture) * sk));
        }
        const int OUT = 1024;

        using var src = new Bitmap(vstup);
        using var maska = new Bitmap(N, N);
        using (var g = Graphics.FromImage(maska))
        {
            g.Clear(Color.Black);
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            g.FillPolygon(Brushes.White, body.ToArray());
        }

        // mekky okraj: rozmazat masku (vodorovne, pak svisle)
        int k = 4;
        var m = new float[N, N];
        for (int y = 0; y < N; y++) for (int x = 0; x < N; x++) m[x, y] = maska.GetPixel(x, y).R / 255f;
        var mb = new float[N, N];
        for (int y = 0; y < N; y++)
            for (int x = 0; x < N; x++)
            {
                float s = 0; int n = 0;
                for (int o = -k; o <= k; o++) { int px = x + o; if (px >= 0 && px < N) { s += m[px, y]; n++; } }
                mb[x, y] = s / n;
            }
        for (int y = 0; y < N; y++)
            for (int x = 0; x < N; x++)
            {
                float s = 0; int n = 0;
                for (int o = -k; o <= k; o++) { int py = y + o; if (py >= 0 && py < N) { s += mb[x, py]; n++; } }
                m[x, y] = s / n;
            }

        using var vyrez = new Bitmap(N, N, PixelFormat.Format32bppArgb);
        for (int y = 0; y < N; y++)
            for (int x = 0; x < N; x++)
            {
                float a = m[x, y];
                // ruka konci na kraji fotky (dole / vpravo) - posledni kus se rozplyne
                if (y > N - 220) a *= Math.Max(0f, (N - y) / 220f);
                if (x > N - 120) a *= Math.Max(0f, (N - x) / 120f);
                int sx = X0 + x, sy = Y0 + y;
                if (a <= 0.01f || sx >= src.Width || sy >= src.Height) { vyrez.SetPixel(x, y, Color.Transparent); continue; }
                Color c = src.GetPixel(sx, sy);
                vyrez.SetPixel(x, y, Color.FromArgb((int)(a * 255), c.R, c.G, c.B));
            }
        vyrez.RotateFlip(RotateFlipType.Rotate270FlipNone); // 90 st. proti smeru hodin: ruka jde nahoru doprava
        using var mensi = new Bitmap(OUT, OUT, PixelFormat.Format32bppArgb);
        using (var g = Graphics.FromImage(mensi))
        {
            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            g.DrawImage(vyrez, 0, 0, OUT, OUT);
        }
        mensi.Save(vystup, ImageFormat.Png);
        // bod (x, y) ve vyrezu -> po otoceni (y, N-1-x); pivot v Unity se meri zdola
        float pivX = bodY / N, pivY = 1f - (N - 1 - bodX) / N;
        Console.WriteLine($"{vystup}: {OUT}x{OUT}, pivot ({pivX.ToString("0.000", CultureInfo.InvariantCulture)}, {pivY.ToString("0.000", CultureInfo.InvariantCulture)})");
    }
}
