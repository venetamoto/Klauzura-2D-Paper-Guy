using System;
using System.Drawing;
using System.Drawing.Imaging;

// Rucne psany text autora (modra propiska na bilem papiru) -> PNG s pruhlednym pozadim, oriznute na text
static class Rukopis
{
    public static void Vyrizni(string vstup, string vystup)
    {
        using var src = new Bitmap(vstup);
        int w = src.Width, h = src.Height;
        var a = new byte[w, h];
        bool cerny = src.GetPixel(0, 0).GetBrightness() < 0.1f; // webp bez alfy -> pozadi cerne
        long r = 0, g = 0, b = 0, n = 0;
        int x0 = w, y0 = h, x1 = -1, y1 = -1;
        for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
            {
                Color c = src.GetPixel(x, y);
                int jas = Math.Min(c.R, Math.Min(c.G, c.B));
                int al = Math.Clamp((235 - jas) * 255 / 160, 0, 255);
                if (cerny) al = Math.Clamp((c.B - c.R - 10) * 255 / 55, 0, 255); // jen modra propiska, sede okraje pryc
                a[x, y] = (byte)al;
                if (al > 60)
                {
                    x0 = Math.Min(x0, x); x1 = Math.Max(x1, x);
                    y0 = Math.Min(y0, y); y1 = Math.Max(y1, y);
                }
                if (al > 240) { r += c.R; g += c.G; b += c.B; n++; }
            }
        Color inkoust = cerny ? Color.FromArgb(12, 36, 120) : n > 0 ? Color.FromArgb((int)(r / n), (int)(g / n), (int)(b / n)) : Color.Black;
        int okraj = 6;
        x0 = Math.Max(0, x0 - okraj); y0 = Math.Max(0, y0 - okraj);
        x1 = Math.Min(w - 1, x1 + okraj); y1 = Math.Min(h - 1, y1 + okraj);
        using var dst = new Bitmap(x1 - x0 + 1, y1 - y0 + 1, PixelFormat.Format32bppArgb);
        for (int y = y0; y <= y1; y++)
            for (int x = x0; x <= x1; x++)
                dst.SetPixel(x - x0, y - y0, Color.FromArgb(a[x, y], inkoust));
        dst.Save(vystup, ImageFormat.Png);
        Console.WriteLine($"{vystup}: {dst.Width}x{dst.Height}, inkoust {inkoust.R},{inkoust.G},{inkoust.B}");
    }
}

// Text na 2 radky (rozdeli v mezere mezi slovy nejblize stredu), tlustsi a tmavsi tahy
static class Rukopis2
{
    public static void Dva(string vstup, string vystup)
    {
        using var src = new Bitmap(vstup);
        int w = src.Width, h = src.Height;
        var a = new int[w, h];
        for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
                a[x, y] = src.GetPixel(x, y).A;

        // sloupce bez inkoustu -> mezery mezi slovy
        bool[] prazdny = new bool[w];
        for (int x = 0; x < w; x++)
        {
            prazdny[x] = true;
            for (int y = 0; y < h; y++) if (a[x, y] > 40) { prazdny[x] = false; break; }
        }
        int rez = w / 2, nejlepsi = int.MaxValue;
        for (int x = 0; x < w; x++)
        {
            if (!prazdny[x]) continue;
            int z = x, k = x;
            while (k < w && prazdny[k]) k++;
            if (k - z >= 15 && Math.Abs((z + k) / 2 - w / 2) < nejlepsi) { nejlepsi = Math.Abs((z + k) / 2 - w / 2); rez = (z + k) / 2; }
            x = k;
        }

        int t = 0; // ztlusteni tahu (0 = jak napsala autorka)
        int mezera = h / 4;
        int sirka = Math.Max(rez, w - rez) + 2 * t;
        int vyska = 2 * h + mezera + 2 * t;
        Color inkoust = Color.FromArgb(12, 36, 120);
        var dst = new int[sirka, vyska];
        Kopiruj(a, 0, rez, h, dst, t, t, t);
        Kopiruj(a, rez, w, h, dst, t, t + h + mezera, t);

        // druhy radek zacina u prvniho slova - posunout doleva
        using var bmp = new Bitmap(sirka, vyska, PixelFormat.Format32bppArgb);
        for (int y = 0; y < vyska; y++)
            for (int x = 0; x < sirka; x++)
                bmp.SetPixel(x, y, Color.FromArgb(Math.Min(255, dst[x, y]), inkoust));
        bmp.Save(vystup, ImageFormat.Png);
        Console.WriteLine($"{vystup}: {sirka}x{vyska}, rez {rez}");
    }

    static void Kopiruj(int[,] a, int x0, int x1, int h, int[,] dst, int dx, int dy, int t)
    {
        int prvni = x1;
        for (int x = x0; x < x1 && prvni == x1; x++)
            for (int y = 0; y < h; y++) if (a[x, y] > 40) { prvni = x; break; }
        for (int y = 0; y < h; y++)
            for (int x = prvni; x < x1; x++)
            {
                int v = a[x, y];
                if (v < 90) continue; // sum z jpg pryc
                v = Math.Min(255, v * 3 / 2);
                for (int oy = -t; oy <= t; oy++)
                    for (int ox = -t; ox <= t; ox++)
                    {
                        if (ox * ox + oy * oy > t * t) continue;
                        int px = x - prvni + dx + ox, py = y + dy + oy;
                        if (px < 0 || py < 0 || px >= dst.GetLength(0) || py >= dst.GetLength(1)) continue;
                        if (v > dst[px, py]) dst[px, py] = v;
                    }
            }
    }
}

// Ztlusti tahy uz vyrizleho textu (o t pixelu na kazdou stranu), plne nepruhledne
static class Ztlustit
{
    public static void Udelej(string soubor, int t)
    {
        int w, h; int[,] a; Color barva;
        using (var src = new Bitmap(soubor))
        {
            w = src.Width; h = src.Height; a = new int[w, h];
            barva = Color.FromArgb(12, 36, 120);
            for (int y = 0; y < h; y++)
                for (int x = 0; x < w; x++) a[x, y] = src.GetPixel(x, y).A;
        }
        int W = w + 2 * t, H = h + 2 * t;
        var b = new int[W, H];
        for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
            {
                int v = a[x, y];
                if (v < 90) continue; // sum z jpg pryc
                for (int oy = -t; oy <= t; oy++)
                    for (int ox = -t; ox <= t; ox++)
                    {
                        if (ox * ox + oy * oy > t * t + 1) continue;
                        int px = x + t + ox, py = y + t + oy;
                        if (v > b[px, py]) b[px, py] = v;
                    }
            }
        using var dst = new Bitmap(W, H, PixelFormat.Format32bppArgb);
        for (int y = 0; y < H; y++)
            for (int x = 0; x < W; x++)
                dst.SetPixel(x, y, Color.FromArgb(Math.Min(255, b[x, y] * 3 / 2), barva));
        dst.Save(soubor, ImageFormat.Png);
        Console.WriteLine($"{soubor}: {W}x{H}, +{t} px");
    }
}

// Vyraznejsi text: tmavsi inkoust + bily svetly okraj kolem tahu (aby byl videt na papire i na linkach)
static class Svatozar
{
    public static void Udelej(string soubor, int r)
    {
        int w, h; int[,] a;
        using (var src = new Bitmap(soubor))
        {
            w = src.Width; h = src.Height; a = new int[w, h];
            for (int y = 0; y < h; y++)
                for (int x = 0; x < w; x++) a[x, y] = src.GetPixel(x, y).A;
        }
        int W = w + 2 * r, H = h + 2 * r;
        var halo = new int[W, H];
        for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
            {
                if (a[x, y] < 90) continue;
                for (int oy = -r; oy <= r; oy++)
                    for (int ox = -r; ox <= r; ox++)
                    {
                        int d2 = ox * ox + oy * oy;
                        if (d2 > r * r) continue;
                        // na kraji svatozare slabsi
                        int v = (int)(230 * (1f - 0.6f * d2 / (float)(r * r)));
                        int px = x + r + ox, py = y + r + oy;
                        if (v > halo[px, py]) halo[px, py] = v;
                    }
            }
        Color inkoust = Color.FromArgb(6, 22, 90);
        using var dst = new Bitmap(W, H, PixelFormat.Format32bppArgb);
        for (int y = 0; y < H; y++)
            for (int x = 0; x < W; x++)
            {
                int ia = 0;
                int sx = x - r, sy = y - r;
                if (sx >= 0 && sy >= 0 && sx < w && sy < h) ia = Math.Min(255, a[sx, sy] * 4 / 3);
                float fi = ia / 255f, fh = halo[x, y] / 255f;
                float outA = fi + fh * (1f - fi);
                if (outA <= 0f) { dst.SetPixel(x, y, Color.Transparent); continue; }
                // inkoust pres bilou
                float cr = (inkoust.R * fi + 255 * fh * (1f - fi)) / outA;
                float cg = (inkoust.G * fi + 255 * fh * (1f - fi)) / outA;
                float cb = (inkoust.B * fi + 255 * fh * (1f - fi)) / outA;
                dst.SetPixel(x, y, Color.FromArgb((int)(outA * 255), (int)cr, (int)cg, (int)cb));
            }
        dst.Save(soubor, ImageFormat.Png);
        Console.WriteLine($"{soubor}: {W}x{H}, svatozar {r} px");
    }
}
