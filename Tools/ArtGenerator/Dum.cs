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
    const float X0 = 22.4f, Y1 = 6.0f;          // levy horni roh obrazku ve svete (pod textem cervene plosiny)
    const float W = 18.5f, H = 11.0f;            // x 22.4..40.9, y -5..6.0
    static readonly Color Tuzka = Color.FromArgb(18, 40, 125);   // modra propiska jako texty v sesitu
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

            KresliDum(g, rnd);
        }
        string cesta2 = Path.Combine(slozka, "Dum.png");
        bmp.Save(cesta2, ImageFormat.Png);
        Console.WriteLine($"{cesta2}: {bmp.Width}x{bmp.Height}, levy horni roh ({X0}, {Y1}), {PPU} px = 1");

        Dvere(Path.Combine(slozka, "Dvere_Leve.png"), 1.87f, 3.40f, false, 22.76f, -1.61f);
        Dvere(Path.Combine(slozka, "Dvere_Prave.png"), 2.06f, 3.42f, true, 38.53f, -1.56f);
    }

    // cely domek "od ruky": kriva stena, prkna ruzne siroka a sikma s letokruhy, prohnuta strecha,
    // kriva okna, tonovani tuzkou (tmavsi dole a vpravo)
    static void KresliDum(Graphics g, Random rnd)
    {
        // --- stena ---
        PointF[] stenaS = { new(22.75f, 3.95f), new(31.6f, 3.82f), new(40.55f, 3.66f), new(40.63f, -1.2f), new(40.6f, -5f), new(22.68f, -5f), new(22.72f, -0.6f) };
        PointF[] stena = Svet(stenaS);
        using (var b = PapirFoto(PPU, X0, Y1)) g.FillPolygon(b, stena);
        using (var cesta = new GraphicsPath())
        {
            cesta.AddPolygon(stena);
            Ton(g, cesta, rnd, 2600, (x, y) => 0.15f + 0.45f * (1f - (y + 5f) / 9f) + 0.25f * (x - 22.7f) / 18f, 62f);
        }

        // prkna: ruzna sirka, mirne sikma, spara = dve cary tesne u sebe, letokruhy
        float px = 22.7f;
        while (px < 40.2f)
        {
            float sirka = 0.75f + (float)rnd.NextDouble() * 0.8f;
            float x = px + sirka;
            if (x > 40.45f) break;
            float naklon = ((float)rnd.NextDouble() - 0.5f) * 0.22f;
            float top = 3.95f - (x - 22.7f) * 0.016f;
            Cara(g, new[] { P(x + naklon, top), P(x + naklon * 0.4f, -1.2f), P(x, -5f) }, 2.3f, 3f, rnd.Next());
            if (rnd.NextDouble() < 0.6) Cara(g, new[] { P(x + naklon + 0.07f, top - 0.1f), P(x + 0.07f, -4.9f) }, 1.1f, 2f, rnd.Next());
            // letokruhy - par dlouhych vlnitych car uvnitr prkna
            int car = 1 + rnd.Next(3);
            for (int i = 0; i < car; i++)
            {
                float lx = px + sirka * (0.25f + (float)rnd.NextDouble() * 0.5f);
                float y0 = -4.8f + (float)rnd.NextDouble() * 4f, y1 = y0 + 1.5f + (float)rnd.NextDouble() * 3.5f;
                if (y1 > top - 0.2f) y1 = top - 0.2f;
                var vlna = new List<PointF>();
                for (float y = y0; y < y1; y += 0.25f) vlna.Add(P(lx + MathF.Sin(y * 3.1f + i) * 0.05f, y));
                if (vlna.Count > 1) using (var pen = new Pen(Color.FromArgb(70, Tuzka), 1.1f)) g.DrawLines(pen, vlna.ToArray());
            }
            if (rnd.NextDouble() < 0.45) Suk(g, px + sirka * 0.5f, -4.3f + (float)rnd.NextDouble() * 7.5f, rnd);
            px = x;
        }
        // spodni lista a obrys steny
        Cara(g, new[] { P(22.7f, -4.55f), P(31.5f, -4.62f), P(40.6f, -4.5f) }, 2.5f, 3f, rnd.Next());
        Cara(g, stena, 4.2f, 3.5f, 5, true);

        // --- prohnuta strecha z drany ---
        float Spodek(float x) => 3.45f + (x - 22.35f) * -0.012f - 0.18f * MathF.Sin((x - 22.35f) / 18.6f * MathF.PI);
        float Vrsek(float x) => 5.86f + (x - 23.2f) * 0.004f - 0.12f * MathF.Sin((x - 23.2f) / 16.9f * MathF.PI);
        var strechaS = new List<PointF>();
        for (float x = 23.2f; x <= 40.1f; x += 0.5f) strechaS.Add(new PointF(x, Vrsek(x)));
        strechaS.Add(new PointF(40.95f, 3.1f));
        for (float x = 40.95f; x >= 22.35f; x -= 0.5f) strechaS.Add(new PointF(x, Spodek(x)));
        PointF[] strecha = Svet(strechaS.ToArray());
        using (var b = PapirFoto(PPU, X0, Y1)) g.FillPolygon(b, strecha);
        using (var cesta = new GraphicsPath())
        {
            cesta.AddPolygon(strecha);
            Ton(g, cesta, rnd, 1400, (x, y) => 0.2f + 0.5f * (1f - (y - 3.4f) / 2.5f), 20f);
        }
        // rady sindelu po prohnuti, svisle spary posunute
        for (int rada = 1; rada < 6; rada++)
        {
            float k = rada / 6f;
            var cara = new List<PointF>();
            for (float x = 22.6f + k * 0.7f; x <= 40.7f - k * 0.7f; x += 1.2f)
                cara.Add(P(x, Spodek(x) + (Vrsek(x) - Spodek(x)) * k + ((float)rnd.NextDouble() - 0.5f) * 0.06f));
            Cara(g, cara.ToArray(), 2f, 2.5f, rnd.Next());
            float kDal = (rada + 1) / 6f;
            for (float x = 22.9f + rnd.Next(3) * 0.25f + k; x < 40.4f - k; x += 0.55f + (float)rnd.NextDouble() * 0.6f)
            {
                float ya = Spodek(x) + (Vrsek(x) - Spodek(x)) * k, yb = Spodek(x) + (Vrsek(x) - Spodek(x)) * kDal;
                Cara(g, new[] { P(x, ya), P(x + 0.03f, yb) }, 1.4f, 1f, rnd.Next());
            }
        }
        // roztrepeny okraj: konce drany visi pod strechou
        for (float x = 22.5f; x < 40.8f; x += 0.3f + (float)rnd.NextDouble() * 0.35f)
        {
            float y = Spodek(x);
            Cara(g, new[] { P(x, y + 0.05f), P(x + 0.02f, y - 0.08f - (float)rnd.NextDouble() * 0.18f) }, 1.8f, 1f, rnd.Next());
        }
        Cara(g, strecha, 4.6f, 4f, 9, true);
        // stin pod strechou na stene
        using (var cesta = new GraphicsPath())
        {
            cesta.AddPolygon(Svet(new PointF[] { new(22.72f, 3.95f), new(40.55f, 3.66f), new(40.55f, 2.9f), new(22.72f, 3.25f) }));
            Ton(g, cesta, rnd, 900, (x, y) => 0.55f + 0.4f * (y - 2.9f), 25f);
        }

        // --- okna a dvere (diry) ---
        RectangleF[] oknaR = {
            R(26.3f, -4.7f, 29.4f, -1.9f), R(32.6f, -4.7f, 35.7f, -1.9f),   // dole - hrdina chodi po zemi
            R(27.1f, 0.1f, 29.7f, 2.5f), R(33.3f, 0.1f, 35.9f, 2.5f) };     // nahore - kdyz pada shora
        var okna = new List<PointF[]>();
        foreach (var o in oknaR) okna.Add(Krive(o, rnd));
        RectangleF[] dvere = { R(22.76f, -5f, 24.63f, -1.61f), R(38.53f, -5f, 40.59f, -1.56f) };

        g.CompositingMode = CompositingMode.SourceCopy;
        using (var dira = new SolidBrush(Color.Transparent))
        {
            foreach (var q in okna) g.FillPolygon(dira, Svet(q));
            foreach (var d in dvere) g.FillRectangle(dira, Px(d));
        }
        g.CompositingMode = CompositingMode.SourceOver;

        for (int i = 0; i < okna.Count; i++) OknoKrive(g, okna[i], i >= 2, rnd);
        foreach (var d in dvere)
        {
            // zarubne, trochu nakrivo
            Cara(g, new[] { P(d.Left - 0.1f, d.Top), P(d.Left - 0.14f, d.Bottom + 0.12f), P(d.Right + 0.1f, d.Bottom + 0.2f), P(d.Right + 0.13f, d.Top) }, 3.8f, 3f, rnd.Next());
            Cara(g, new[] { P(d.Left - 0.25f, d.Top), P(d.Left - 0.28f, d.Bottom + 0.28f), P(d.Right + 0.24f, d.Bottom + 0.34f), P(d.Right + 0.27f, d.Top) }, 1.6f, 3f, rnd.Next());
        }

        // tráva a kaminky u zeme
        using (var pen = new Pen(Color.FromArgb(160, Tuzka), 1.6f) { StartCap = LineCap.Round, EndCap = LineCap.Round })
            for (int i = 0; i < 220; i++)
            {
                PointF z = P(22.5f + (float)rnd.NextDouble() * 18.3f, -5f);
                float v = 5f + (float)rnd.NextDouble() * 16f, nakl = ((float)rnd.NextDouble() - 0.5f) * 12f;
                g.DrawLine(pen, z.X, z.Y - 1, z.X + nakl, z.Y - v);
            }
        using (var pen = new Pen(Color.FromArgb(170, Tuzka), 1.8f))
            for (int i = 0; i < 14; i++)
            {
                PointF z = P(22.8f + (float)rnd.NextDouble() * 17.6f, -5f);
                float s = 6f + (float)rnd.NextDouble() * 9f;
                g.DrawArc(pen, z.X, z.Y - s * 0.7f, s * 1.6f, s, 180, 180);
            }
    }

    // obdelnik okna -> kriva ctyruhelnik (rohy posunute o par setin)
    static PointF[] Krive(RectangleF o, Random rnd)
    {
        float J() => ((float)rnd.NextDouble() - 0.5f) * 0.2f;
        return new PointF[] {
            new(o.Left + J(), o.Bottom + J()), new(o.Right + J(), o.Bottom + J()),
            new(o.Right + J(), o.Top + J()), new(o.Left + J(), o.Top + J()) };
    }

    static PointF[] Svet(PointF[] body)
    {
        var r = new PointF[body.Length];
        for (int i = 0; i < body.Length; i++) r[i] = P(body[i].X, body[i].Y);
        return r;
    }

    // kriva okno: dvojity ram, krizek, parapet; horni okna maji okenice z prken
    static void OknoKrive(Graphics g, PointF[] q, bool okenice, Random rnd)
    {
        Cara(g, Svet(q), 4.2f, 3f, rnd.Next(), true);
        var vnejsi = new PointF[4];
        float[] dx = { -0.13f, 0.13f, 0.13f, -0.13f }, dy = { 0.13f, 0.13f, -0.11f, -0.11f };
        for (int i = 0; i < 4; i++) vnejsi[i] = new PointF(q[i].X + dx[i], q[i].Y + dy[i]);
        Cara(g, Svet(vnejsi), 1.8f, 3f, rnd.Next(), true);
        PointF hore = Mezi(q[0], q[1]), dole = Mezi(q[3], q[2]), vlevo = Mezi(q[0], q[3]), vpravo = Mezi(q[1], q[2]);
        Cara(g, new[] { P(hore.X, hore.Y), P(dole.X, dole.Y) }, 2.6f, 2f, rnd.Next());
        Cara(g, new[] { P(vlevo.X, vlevo.Y), P(vpravo.X, vpravo.Y) }, 2.6f, 2f, rnd.Next());
        // parapet - prkno pod oknem, sikme
        Cara(g, new[] { P(q[3].X - 0.35f, q[3].Y - 0.1f), P(q[2].X + 0.35f, q[2].Y - 0.16f) }, 4.2f, 2f, rnd.Next());
        Cara(g, new[] { P(q[3].X - 0.3f, q[3].Y - 0.3f), P(q[2].X + 0.3f, q[2].Y - 0.34f) }, 1.8f, 2f, rnd.Next());
        if (!okenice) return;
        float vys = q[0].Y - q[3].Y;
        foreach (float strana in new[] { -1f, 1f })
        {
            PointF a = strana < 0 ? q[0] : q[1], b = strana < 0 ? q[3] : q[2];
            float x0 = a.X + strana * 0.15f, x1 = a.X + strana * 0.95f;
            PointF[] ok = { new(x0, a.Y), new(x1, a.Y - 0.05f), new(x1, b.Y + 0.05f), new(x0, b.Y) };
            using (var br = PapirFoto(PPU, X0, Y1)) g.FillPolygon(br, Svet(ok));
            Cara(g, Svet(ok), 2.8f, 2.5f, rnd.Next(), true);
            for (int i = 1; i < 3; i++)
            {
                float lx = x0 + (x1 - x0) * i / 3f;
                Cara(g, new[] { P(lx, a.Y - 0.03f), P(lx, b.Y + 0.03f) }, 1.4f, 1.5f, rnd.Next());
            }
            Cara(g, new[] { P(x0, b.Y + vys * 0.25f), P(x1, b.Y + vys * 0.75f) }, 2f, 2f, rnd.Next());
        }
    }

    static PointF Mezi(PointF a, PointF b) => new((a.X + b.X) / 2, (a.Y + b.Y) / 2);

    // vypln = presne ten kus fotky papiru, ktery je ve hre pod domkem (pozadi Varvara: 2000 x 1125 px,
    // 37.5 px = 1 jednotka, levy horni roh listu (-1.27, 25)) - domek pak vypada nakresleny primo v sesitu
    static Bitmap papir;
    static TextureBrush PapirFoto(float ppu, float x0, float y1)
    {
        papir ??= new Bitmap(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "Assets", "Art", "Generated", "Pozadi_Papir_16x9.jpg"));
        const float fotoPpu = 37.5f, listX = -1.27f, listY = 25f;
        float s = ppu / fotoPpu;
        var b = new TextureBrush(papir, WrapMode.Clamp);
        b.Transform = new Matrix(s, 0, 0, s, -(x0 - listX) * ppu, -(listY - y1) * ppu);
        return b;
    }

    // tonovani tuzkou: kratke sikme tahy, hustsi tam, kde funkce tmy (0..1) vraci vic
    static void Ton(Graphics g, GraphicsPath clip, Random rnd, int pocet, Func<float, float, float> tma, float uhel)
    {
        Region stary = g.Clip;
        g.SetClip(clip);
        RectangleF b = clip.GetBounds();
        float a = uhel * MathF.PI / 180f;
        for (int i = 0; i < pocet; i++)
        {
            float x = b.X + (float)rnd.NextDouble() * b.Width, y = b.Y + (float)rnd.NextDouble() * b.Height;
            float sx = X0 + x / PPU, sy = Y1 - y / PPU;
            float t = Math.Clamp(tma(sx, sy), 0f, 1f);
            if (rnd.NextDouble() > t) continue;
            float d = 8f + (float)rnd.NextDouble() * 18f;
            using var pen = new Pen(Color.FromArgb((int)(35 + 70 * t), Tuzka), 1.2f + (float)rnd.NextDouble() * 0.6f);
            g.DrawLine(pen, x, y, x + d * MathF.Cos(a), y - d * MathF.Sin(a));
        }
        g.Clip = stary;
    }

    // karticka misto cerneho bloku: ram modrou propiskou po krajich bloku a uvnitr rovna kresba autorky
    // (blok je ve hre otoceny o uhel - kresba se otoci zpet, aby byla rovne)
    public static void Karta(string kresba, string vystup, float sirkaJ, float vyskaJ, float uhelBloku)
    {
        const float ppu = 120f;
        int w = (int)(sirkaJ * ppu), h = (int)(vyskaJ * ppu);
        using var bmp = new Bitmap(w, h, PixelFormat.Format32bppArgb);
        using (var g = Graphics.FromImage(bmp))
        {
            g.SmoothingMode = SmoothingMode.HighQuality;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.Clear(Color.Transparent);
            var rnd = new Random(kresba.Length);
            CaraPx(g, new[] { new PointF(6, 6), new PointF(w - 6, 6), new PointF(w - 6, h - 6), new PointF(6, h - 6) }, 5f, 3f, rnd.Next(), true);
            CaraPx(g, new[] { new PointF(14, 14), new PointF(w - 14, 14), new PointF(w - 14, h - 14), new PointF(14, h - 14) }, 2f, 3f, rnd.Next(), true);

            using var k = new Bitmap(kresba);
            float a = MathF.Abs(uhelBloku * MathF.PI / 180f);
            float co = MathF.Abs(MathF.Cos(a)), si = MathF.Abs(MathF.Sin(a));
            // otocena kresba se musi vejit do ramu (obrys otoceneho obdelniku)
            float s = Math.Min((w - 40) / (k.Width * co + k.Height * si), (h - 40) / (k.Width * si + k.Height * co));
            g.TranslateTransform(w / 2f, h / 2f);
            g.RotateTransform(uhelBloku); // zpet proti otoceni bloku ve hre (v obrazku y jde dolu)
            g.DrawImage(k, -k.Width * s / 2f, -k.Height * s / 2f, k.Width * s, k.Height * s);
        }
        bmp.Save(vystup, ImageFormat.Png);
        Console.WriteLine($"{vystup}: {w}x{h} ({ppu} px = 1), kresba otocena o {uhelBloku}");
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
    static void Dvere(string soubor, float sw, float sh, bool klicova, float svetX, float svetY)
    {
        float ppu = 120f;
        int w = (int)(sw * ppu), h = (int)(sh * ppu);
        using var bmp = new Bitmap(w, h, PixelFormat.Format32bppArgb);
        using (var g = Graphics.FromImage(bmp))
        {
            g.SmoothingMode = SmoothingMode.HighQuality;
            g.Clear(Color.Transparent);
            var rnd = new Random(klicova ? 3 : 4);
            using (var b = PapirFoto(ppu, svetX, svetY)) g.FillRectangle(b, 2, 2, w - 4, h - 4);
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
    // skica: kazdy usek je samostatny tah, 3x pres sebe, konce prelezaji pres rohy, obcas mezera
    static void CaraPx(Graphics g, PointF[] body, float sirka, float amp, int seed, bool zavrena = false)
    {
        sirka *= 0.7f; // propiska je tenci nez tuzka
        int n = zavrena ? body.Length : body.Length - 1;
        for (int pruchod = 0; pruchod < 3; pruchod++)
        {
            var rnd = new Random(seed + pruchod * 31);
            int alfa = pruchod == 0 ? 215 : pruchod == 1 ? 130 : 75;
            float tl = pruchod == 0 ? sirka : sirka * (0.45f + 0.15f * pruchod);
            using var pen = new Pen(Color.FromArgb(alfa, Tuzka), tl) { LineJoin = LineJoin.Round, StartCap = LineCap.Round, EndCap = LineCap.Round };
            for (int i = 0; i < n; i++)
            {
                PointF a = body[i], b = body[(i + 1) % body.Length];
                float dx = b.X - a.X, dy = b.Y - a.Y, len = MathF.Sqrt(dx * dx + dy * dy);
                if (len < 0.01f) continue;
                float ux = dx / len, uy = dy / len, nx = -uy, ny = ux;
                // prelez pres konce a posun celeho tahu
                float pred = 3f + (float)rnd.NextDouble() * 12f, za = 3f + (float)rnd.NextDouble() * 12f;
                float posun = ((float)rnd.NextDouble() - 0.5f) * 2.5f * (pruchod + 1);
                PointF z = new PointF(a.X - ux * pred + nx * posun, a.Y - uy * pred + ny * posun);
                PointF k = new PointF(b.X + ux * za + nx * posun, b.Y + uy * za + ny * posun);
                float L = len + pred + za;
                int kroku = Math.Max(2, (int)(L / 10));
                var tah = new List<PointF>();
                for (int j = 0; j <= kroku; j++)
                {
                    float t = j / (float)kroku, off = ((float)rnd.NextDouble() - 0.5f) * amp * 1.4f;
                    // obcas tuzka odskoci - kratka mezera
                    if (pruchod == 0 && j > 1 && j < kroku - 1 && rnd.NextDouble() < 0.025)
                    {
                        if (tah.Count > 1) g.DrawLines(pen, tah.ToArray());
                        tah.Clear();
                        continue;
                    }
                    tah.Add(new PointF(z.X + (k.X - z.X) * t + nx * off, z.Y + (k.Y - z.Y) * t + ny * off));
                }
                if (tah.Count > 1) g.DrawLines(pen, tah.ToArray());
            }
        }
    }

    // krizove srafovani do kouta (svetove souradnice obdelniku), hustota = pocet tahu
    static void Kriz(Graphics g, float x0, float y0, float x1, float y1, int hustota, Random rnd)
    {
        using var pen = new Pen(Color.FromArgb(95, Tuzka), 1.5f) { StartCap = LineCap.Round, EndCap = LineCap.Round };
        PointF a = P(x0, y1), b = P(x1, y0);
        for (int i = 0; i < hustota; i++)
        {
            float x = a.X + (float)rnd.NextDouble() * (b.X - a.X), y = a.Y + (float)rnd.NextDouble() * (b.Y - a.Y);
            float d = 10f + (float)rnd.NextDouble() * 22f;
            if (i % 2 == 0) g.DrawLine(pen, x, y, x + d * 0.6f, y - d);
            else g.DrawLine(pen, x, y, x + d, y + d * 0.35f);
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
