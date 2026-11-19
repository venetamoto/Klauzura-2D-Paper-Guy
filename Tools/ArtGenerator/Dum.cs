using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;

// Domek z prken (tuzkou) pres mistnost ve Varvara: dvoje dvere = diry (dvere jsou zvlast), okna = diry,
// aby byl hrdina videt jen v oknech. Souradnice jsou svetove jednotky Unity, 60 px = 1 jednotka.
static class Dum
{
    const float PPU = 60f;
    const float X0 = 22.4f, Y1 = 6.8f;          // levy horni roh obrazku ve svete
    const float W = 18.5f, H = 11.8f;            // x 22.4..40.9, y -5..6.8
    static readonly Color Tuzka = Color.FromArgb(55, 52, 50);
    static readonly Color Papir = Color.FromArgb(242, 238, 228);
    static readonly Color PapirTmavsi = Color.FromArgb(228, 222, 208);

    static PointF P(float x, float y) => new PointF((x - X0) * PPU, (Y1 - y) * PPU);

    public static void Kresli(string slozka)
    {
        int w = (int)(W * PPU), h = (int)(H * PPU);
        using var bmp = new Bitmap(w, h, PixelFormat.Format32bppArgb);
        using (var g = Graphics.FromImage(bmp))
        {
            g.SmoothingMode = SmoothingMode.HighQuality;
            g.Clear(Color.Transparent);
            var rnd = new Random(11);

            // --- stena z prken ---
            PointF[] stena = { P(22.7f, 4.7f), P(40.6f, 4.5f), P(40.6f, -5f), P(22.7f, -5f) };
            using (var b = new SolidBrush(Papir)) g.FillPolygon(b, stena);
            using (var cesta = new GraphicsPath()) { cesta.AddPolygon(stena); Srafuj(g, cesta, 0.2f, 45, 72, rnd); }
            for (float x = 23.8f; x < 40.4f; x += 1.15f + (float)rnd.NextDouble() * 0.25f)
            {
                float top = 4.7f - (x - 22.7f) * 0.011f;
                Cara(g, new[] { P(x, top), P(x + 0.05f, -1.5f), P(x - 0.03f, -5f) }, 2.2f, 3f, rnd.Next());
                if (rnd.NextDouble() < 0.5) Suk(g, x + 0.55f, -4.5f + (float)rnd.NextDouble() * 8.5f, rnd);
            }
            Cara(g, stena, 4f, 3f, 5, true);
            // stin pod strechou
            using (var cesta = new GraphicsPath()) { cesta.AddPolygon(new[] { P(22.7f, 4.7f), P(40.6f, 4.5f), P(40.6f, 3.9f), P(22.7f, 4.1f) }); Srafuj(g, cesta, 0.06f, 140, 30, rnd); }

            // --- krivá strecha z prken ---
            PointF[] strecha = { P(22.4f, 4.3f), P(23.3f, 6.7f), P(40.0f, 6.78f), P(40.9f, 4.0f), P(31.5f, 4.45f) };
            using (var b = new SolidBrush(PapirTmavsi)) g.FillPolygon(b, strecha);
            for (float y = 4.6f; y < 6.7f; y += 0.42f)
            {
                float sag = (float)rnd.NextDouble() * 0.12f;
                Cara(g, new[] { P(22.6f + (y - 4.3f) * 0.35f, y), P(31.5f, y - 0.12f - sag), P(40.6f - (y - 4.0f) * 0.25f, y + 0.05f) }, 2f, 3f, rnd.Next());
            }
            for (int i = 0; i < 40; i++)
            {
                float x = 23f + (float)rnd.NextDouble() * 17f, y = 4.6f + (float)rnd.NextDouble() * 1.9f;
                Cara(g, new[] { P(x, y), P(x + 0.5f + (float)rnd.NextDouble() * 0.8f, y + 0.05f) }, 1.3f, 1.5f, rnd.Next());
            }
            // svisle spary mezi sindeli
            for (int i = 0; i < 70; i++)
            {
                float x = 23.2f + (float)rnd.NextDouble() * 17f, y = 4.5f + (float)rnd.NextDouble() * 2.1f;
                Cara(g, new[] { P(x, y), P(x + 0.04f, y + 0.36f) }, 1.6f, 1f, rnd.Next());
            }
            using (var cesta = new GraphicsPath()) { cesta.AddPolygon(strecha); Srafuj(g, cesta, 0.12f, 70, 15, rnd); }
            Cara(g, strecha, 4.5f, 4f, 9, true);

            // --- okna (diry) a dvere (diry) ---
            RectangleF[] okna = {
                R(26.3f, -4.7f, 29.4f, -1.9f), R(32.6f, -4.7f, 35.7f, -1.9f),   // dole - hrdina chodi po zemi
                R(27.1f, 0.5f, 29.7f, 3.1f), R(33.3f, 0.5f, 35.9f, 3.1f) };     // nahore - kdyz pada shora
            RectangleF[] dvere = { R(22.76f, -5f, 24.63f, -1.61f), R(38.53f, -5f, 40.59f, -1.56f) };

            g.CompositingMode = CompositingMode.SourceCopy;
            using (var dira = new SolidBrush(Color.Transparent))
            {
                foreach (var o in okna) g.FillRectangle(dira, Px(o));
                foreach (var d in dvere) g.FillRectangle(dira, Px(d));
            }
            g.CompositingMode = CompositingMode.SourceOver;

            foreach (var o in okna) Okno(g, o, rnd);
            foreach (var d in dvere)
            {
                // zarubne
                Cara(g, new[] { P(d.Left - 0.12f, d.Top), P(d.Left - 0.12f, d.Bottom + 0.15f), P(d.Right + 0.12f, d.Bottom + 0.15f), P(d.Right + 0.12f, d.Top) }, 3.5f, 2.5f, rnd.Next());
            }
        }
        string cesta2 = Path.Combine(slozka, "Dum.png");
        bmp.Save(cesta2, ImageFormat.Png);
        Console.WriteLine($"{cesta2}: {bmp.Width}x{bmp.Height}, levy horni roh ({X0}, {Y1}), {PPU} px = 1");

        Dvere(Path.Combine(slozka, "Dvere_Leve.png"), 1.87f, 3.40f, false);
        Dvere(Path.Combine(slozka, "Dvere_Prave.png"), 2.06f, 3.42f, true);
    }

    // okno: ram ze dvou car, krizek, parapet
    static void Okno(Graphics g, RectangleF o, Random rnd)
    {
        float l = o.Left, r = o.Right, t = o.Bottom, b = o.Top; // RectangleF: Top = mensi y = spodek ve svete
        Cara(g, new[] { P(l, t), P(r, t), P(r, b), P(l, b) }, 4f, 2.5f, rnd.Next(), true);
        Cara(g, new[] { P(l - 0.12f, t + 0.12f), P(r + 0.12f, t + 0.12f), P(r + 0.12f, b - 0.1f), P(l - 0.12f, b - 0.1f) }, 2f, 2.5f, rnd.Next(), true);
        float sx = (l + r) / 2, sy = (t + b) / 2;
        Cara(g, new[] { P(sx, t), P(sx, b) }, 2.5f, 2f, rnd.Next());
        Cara(g, new[] { P(l, sy), P(r, sy) }, 2.5f, 2f, rnd.Next());
        // parapet
        Cara(g, new[] { P(l - 0.3f, b - 0.12f), P(r + 0.3f, b - 0.15f) }, 4f, 2f, rnd.Next());
        Cara(g, new[] { P(l - 0.25f, b - 0.28f), P(r + 0.25f, b - 0.3f) }, 2f, 2f, rnd.Next());
    }

    // dvere z prken (samostatny obrazek, cely nepruhledny), 120 px = 1 jednotka
    static void Dvere(string soubor, float sw, float sh, bool klicova)
    {
        float ppu = 120f;
        int w = (int)(sw * ppu), h = (int)(sh * ppu);
        using var bmp = new Bitmap(w, h, PixelFormat.Format32bppArgb);
        using (var g = Graphics.FromImage(bmp))
        {
            g.SmoothingMode = SmoothingMode.HighQuality;
            g.Clear(Color.Transparent);
            var rnd = new Random(klicova ? 3 : 4);
            using (var b = new SolidBrush(Color.FromArgb(232, 225, 210))) g.FillRectangle(b, 2, 2, w - 4, h - 4);
            using (var cesta = new GraphicsPath()) { cesta.AddRectangle(new RectangleF(2, 2, w - 4, h - 4)); SrafujPx(g, cesta, 22, 70, 50, rnd); }
            int prken = 4;
            for (int i = 1; i < prken; i++) CaraPx(g, new[] { new PointF(w * i / (float)prken, 4), new PointF(w * i / (float)prken + 2, h - 4) }, 2.5f, 2.5f, rnd.Next());
            // prickla do Z
            CaraPx(g, new[] { new PointF(8, h * 0.25f), new PointF(w - 8, h * 0.25f) }, 4f, 2.5f, rnd.Next());
            CaraPx(g, new[] { new PointF(8, h * 0.75f), new PointF(w - 8, h * 0.75f) }, 4f, 2.5f, rnd.Next());
            CaraPx(g, new[] { new PointF(10, h * 0.73f), new PointF(w - 10, h * 0.27f) }, 3.5f, 2.5f, rnd.Next());
            CaraPx(g, new[] { new PointF(3, 3), new PointF(w - 3, 3), new PointF(w - 3, h - 3), new PointF(3, h - 3) }, 5f, 2.5f, rnd.Next(), true);
            // klika
            float kx = klicova ? w * 0.18f : w * 0.82f, ky = h * 0.52f;
            using (var p = new Pen(Tuzka, 4f)) g.DrawEllipse(p, kx - 10, ky - 10, 20, 20);
            if (klicova)
            {
                // zamek s klicovou dirkou - tyhle dvere chteji klic
                using var p = new Pen(Tuzka, 3f);
                g.DrawEllipse(p, kx - 8, ky + 22, 16, 16);
                using var b = new SolidBrush(Tuzka);
                g.FillPolygon(b, new[] { new PointF(kx - 4, ky + 36), new PointF(kx + 4, ky + 36), new PointF(kx + 6, ky + 52), new PointF(kx - 6, ky + 52) });
            }
        }
        bmp.Save(soubor, ImageFormat.Png);
        Console.WriteLine($"{soubor}: {bmp.Width}x{bmp.Height} (120 px = 1)");
    }

    static RectangleF R(float x0, float y0, float x1, float y1) => RectangleF.FromLTRB(x0, y0, x1, y1);

    static RectangleF Px(RectangleF svet)
    {
        PointF a = P(svet.Left, svet.Bottom), b = P(svet.Right, svet.Top);
        return RectangleF.FromLTRB(a.X, a.Y, b.X, b.Y);
    }

    static void Suk(Graphics g, float x, float y, Random rnd)
    {
        PointF s = P(x, y);
        using var p = new Pen(Color.FromArgb(150, Tuzka), 1.8f);
        g.DrawEllipse(p, s.X - 6, s.Y - 9, 12 + rnd.Next(5), 18 + rnd.Next(6));
    }

    static void Cara(Graphics g, PointF[] body, float sirka, float amp, int seed, bool zavrena = false) => CaraPx(g, body, sirka, amp, seed, zavrena);

    // carka tuzkou: dvakrat pres sebe, trochu roztresena
    static void CaraPx(Graphics g, PointF[] body, float sirka, float amp, int seed, bool zavrena = false)
    {
        for (int pruchod = 0; pruchod < 2; pruchod++)
        {
            var rnd = new Random(seed + pruchod * 31);
            var vys = new List<PointF>();
            int n = zavrena ? body.Length : body.Length - 1;
            for (int i = 0; i < n; i++)
            {
                PointF a = body[i], b = body[(i + 1) % body.Length];
                float dx = b.X - a.X, dy = b.Y - a.Y, len = MathF.Sqrt(dx * dx + dy * dy);
                if (len < 0.01f) continue;
                int kroku = Math.Max(1, (int)(len / 12));
                float nx = -dy / len, ny = dx / len;
                for (int j = 0; j < kroku; j++)
                {
                    float t = j / (float)kroku, off = ((float)rnd.NextDouble() - 0.5f) * amp;
                    vys.Add(new PointF(a.X + dx * t + nx * off, a.Y + dy * t + ny * off));
                }
            }
            vys.Add(zavrena ? vys[0] : body[body.Length - 1]);
            using var pen = new Pen(Color.FromArgb(pruchod == 0 ? 230 : 110, Tuzka), pruchod == 0 ? sirka : sirka * 0.6f)
            { LineJoin = LineJoin.Round, StartCap = LineCap.Round, EndCap = LineCap.Round };
            if (vys.Count > 1) g.DrawLines(pen, vys.ToArray());
        }
    }

    // srafovani tuzkou uvnitr cesty (rozestup ve svetovych jednotkach)
    static void Srafuj(Graphics g, GraphicsPath clip, float rozestup, int alfa, float uhel, Random rnd) => SrafujPx(g, clip, rozestup * PPU, alfa, uhel, rnd);

    static void SrafujPx(Graphics g, GraphicsPath clip, float rozestup, int alfa, float uhel, Random rnd)
    {
        Region stary = g.Clip;
        g.SetClip(clip);
        RectangleF b = clip.GetBounds();
        float diag = b.Width + b.Height, a = uhel * MathF.PI / 180f;
        using var pen = new Pen(Color.FromArgb(alfa, Tuzka), 1.4f);
        for (float s = -diag; s < diag; s += rozestup * (0.7f + (float)rnd.NextDouble() * 0.6f))
        {
            float cx = b.X + b.Width / 2 + s * MathF.Cos(a + MathF.PI / 2), cy = b.Y + b.Height / 2 + s * MathF.Sin(a + MathF.PI / 2);
            float delka = diag * (0.05f + (float)rnd.NextDouble() * 0.12f);
            float posun = ((float)rnd.NextDouble() - 0.5f) * diag;
            float x = cx + posun * MathF.Cos(a), y = cy + posun * MathF.Sin(a);
            g.DrawLine(pen, x, y, x + delka * MathF.Cos(a), y + delka * MathF.Sin(a));
        }
        g.Clip = stary;
    }
}
