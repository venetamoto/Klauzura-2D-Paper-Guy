using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;

// Plastik z boku (zvonecek), padani (rozevreny plast) a jidlo (krovky + tlama na brisku) - styl konceptu.
// Velka platna 1000x800, bod uchyceni = spodni lem plastiku uprostred (500, 700); 400 px = 1 jednotka.
public static class HeroEat
{
    static readonly Color Ink = Color.FromArgb(20, 20, 24);
    const float HX = 500, HY = 700;   // lem / kycle
    const float Top = 412;            // horni okraj plastiku

    public static void RenderAll(string dir)
    {
        Directory.CreateDirectory(dir);
        SideBell(dir, "Hrdina_Plast_Bok");

        Fall(dir, "Hrdina_Plast_Pad_0", 0f);
        Fall(dir, "Hrdina_Plast_Pad_1", 1f);

        Eat(dir, "Hrdina_Plast_Ji_0", open: 0.3f, mouth: 0f, tongue: 0f, stitches: false);
        Eat(dir, "Hrdina_Plast_Ji_1", open: 1f, mouth: 1f, tongue: 0f, stitches: false);
        Eat(dir, "Hrdina_Plast_Ji_2", open: 1f, mouth: 1f, tongue: 1f, stitches: false);
        Eat(dir, "Hrdina_Plast_Ji_3", open: 0.7f, mouth: 0.45f, tongue: 0.35f, stitches: false);
        Eat(dir, "Hrdina_Plast_Ji_4", open: 0.25f, mouth: 0f, tongue: 0f, stitches: true);
    }

    static Bitmap Canvas(int w, int h, out Graphics g)
    {
        var b = new Bitmap(w, h, PixelFormat.Format32bppArgb);
        g = Graphics.FromImage(b);
        g.SmoothingMode = SmoothingMode.HighQuality;
        g.PixelOffsetMode = PixelOffsetMode.HighQuality;
        g.Clear(Color.Transparent);
        return b;
    }

    static Pen InkPen(float w) => new Pen(Ink, w) { StartCap = LineCap.Round, EndCap = LineCap.Round, LineJoin = LineJoin.Round };

    // tmava latka s bilymi skrabanci a obrysem
    static void Cloth(Graphics g, GraphicsPath path, int seed)
    {
        RectangleF bb = path.GetBounds();
        using (var dark = new LinearGradientBrush(bb, Color.FromArgb(8, 8, 10), Color.FromArgb(58, 58, 64), LinearGradientMode.ForwardDiagonal))
            g.FillPath(dark, path);
        Region old = g.Clip;
        g.SetClip(path, CombineMode.Intersect);
        var rnd = new Random(seed);
        int n = (int)(bb.Width * bb.Height / 900f);
        using (var p = new Pen(Color.FromArgb(85, 235, 235, 235), 2f))
            for (int i = 0; i < n; i++)
            {
                float x = bb.Left + (float)rnd.NextDouble() * bb.Width, y = bb.Top + (float)rnd.NextDouble() * bb.Height;
                float len = 40 + (float)rnd.NextDouble() * 70;
                float a = ((i % 2 == 0) ? 55f : -55f) + (float)(rnd.NextDouble() * 20 - 10);
                float r = a * MathF.PI / 180f;
                g.DrawLine(p, x, y, x + MathF.Cos(r) * len, y + MathF.Sin(r) * len);
            }
        g.Clip = old;
        using (var p = InkPen(5f)) g.DrawPath(p, path);
    }

    // ---------- plastik z boku: kulaty zvonecek (440x320 jako zepredu) ----------
    static void SideBell(string dir, string name)
    {
        Bitmap b = Canvas(440, 320, out Graphics g);
        using (g)
        {
            using var path = new GraphicsPath();
            path.AddBezier(118, 30, 170, 4, 270, 4, 322, 30);
            path.AddBezier(322, 30, 372, 70, 398, 170, 400, 246);
            path.AddBezier(400, 246, 320, 308, 120, 308, 40, 246);
            path.AddBezier(40, 246, 42, 170, 68, 70, 118, 30);
            path.CloseFigure();
            Cloth(g, path, 11);
            using var rim = new Pen(Color.FromArgb(200, 245, 245, 245), 3f);
            g.DrawBezier(rim, 124, 46, 84, 90, 62, 170, 58, 236);
        }
        Save(b, dir, name);
    }

    // ---------- padani: plast se rozevre jako padak ----------
    static void Fall(string dir, string name, float flutter)
    {
        Bitmap b = Canvas(1000, 800, out Graphics g);
        using (g)
        {
            float tipY = 655 - flutter * 35, spread = 300 + flutter * 45;
            var lt = new PointF(HX - spread, tipY);
            var rt = new PointF(HX + spread, tipY);
            using var path = new GraphicsPath();
            path.AddBezier(HX - 70, Top, HX - 190, Top + 10, lt.X + 40, tipY - 150, lt.X, lt.Y);
            Scallops(path, lt, rt, 4, 70 - flutter * 15);
            path.AddBezier(rt.X, rt.Y, rt.X - 40, tipY - 150, HX + 190, Top + 10, HX + 70, Top);
            path.AddBezier(HX + 70, Top, HX + 30, Top - 12, HX - 30, Top - 12, HX - 70, Top);
            path.CloseFigure();
            Cloth(g, path, 21 + (int)flutter);
            using var rim = new Pen(Color.FromArgb(200, 245, 245, 245), 3f);
            g.DrawBezier(rim, HX - 60, Top + 18, HX - 150, Top + 40, lt.X + 130, tipY - 90, lt.X + 20, tipY - 8);
            g.DrawBezier(rim, HX + 60, Top + 18, HX + 150, Top + 40, rt.X - 130, tipY - 90, rt.X - 20, tipY - 8);
        }
        Save(b, dir, name);
    }

    // vlnity spodni okraj (mezi dvema spickami)
    static void Scallops(GraphicsPath path, PointF from, PointF to, int count, float depth)
    {
        PointF prev = from;
        for (int i = 1; i <= count; i++)
        {
            float t = i / (float)count;
            float arc = MathF.Sin(t * MathF.PI) * 70f; // stred okraje je niz
            var p = new PointF(from.X + (to.X - from.X) * t, from.Y + (to.Y - from.Y) * t + arc);
            var c = new PointF((prev.X + p.X) / 2f, Math.Max(prev.Y, p.Y) + depth * 0.25f);
            path.AddBezier(prev, new PointF(prev.X + (c.X - prev.X) * 0.4f, c.Y), new PointF(p.X - (p.X - c.X) * 0.4f, c.Y - depth * 0.6f), p);
            prev = p;
        }
    }

    // ---------- jidlo: krovky + tlama na brisku ----------
    static void Eat(string dir, string name, float open, float mouth, float tongue, bool stitches)
    {
        Bitmap b = Canvas(1000, 800, out Graphics g);
        using (g)
        {
            var belly = new PointF(HX + 30, HY - 140);

            // zadni a predni kridlo (netopyri)
            Wing(g, back: true, open);
            Wing(g, back: false, open);

            // telo plastiku (bricho) mezi kridly
            using (var body = new GraphicsPath())
            {
                body.AddBezier(HX - 70, Top, HX - 120, Top + 80, HX - 150, HY - 80, HX - 120, HY - 10);
                body.AddBezier(HX - 120, HY - 10, HX - 40, HY + 10, HX + 100, HY + 10, HX + 170, HY - 20);
                body.AddBezier(HX + 170, HY - 20, HX + 180, HY - 120, HX + 120, Top + 70, HX + 70, Top);
                body.AddBezier(HX + 70, Top, HX + 30, Top - 10, HX - 30, Top - 10, HX - 70, Top);
                body.CloseFigure();
                Cloth(g, body, 31);
            }

            if (mouth > 0f)
            {
                float mw = 230 * (0.55f + 0.45f * mouth), mh = 190 * mouth;
                var rect = new RectangleF(belly.X - mw / 2, belly.Y - mh / 2, mw, mh);
                using (var inside = new SolidBrush(Color.FromArgb(110, 18, 32))) g.FillEllipse(inside, rect);
                using (var throat = new SolidBrush(Color.FromArgb(40, 5, 10))) g.FillEllipse(throat, belly.X - mw * 0.22f, belly.Y - mh * 0.18f, mw * 0.44f, mh * 0.4f);
                Teeth(g, rect, top: true);
                Teeth(g, rect, top: false);
                using (var p = InkPen(6f)) g.DrawEllipse(p, rect);

                if (tongue > 0f)
                {
                    float len = 380 * tongue;
                    var tip = new PointF(belly.X + 60 + len, belly.Y - 30 * tongue);
                    using var tpath = new GraphicsPath();
                    tpath.AddBezier(belly.X - 10, belly.Y + mh * 0.15f, belly.X + len * 0.4f, belly.Y + 40, tip.X - 80, tip.Y + 40, tip.X, tip.Y);
                    tpath.AddBezier(tip.X, tip.Y, tip.X + 30, tip.Y - 40, tip.X - 40, tip.Y - 60, tip.X - 50, tip.Y - 20);
                    tpath.AddBezier(tip.X - 50, tip.Y - 20, tip.X - 120, tip.Y + 10, belly.X + len * 0.3f, belly.Y + 5, belly.X + 10, belly.Y - mh * 0.05f);
                    tpath.CloseFigure();
                    using (var tb = new SolidBrush(Color.FromArgb(225, 95, 115))) g.FillPath(tb, tpath);
                    using (var p = InkPen(5f)) g.DrawPath(p, tpath);
                    using (var p = new Pen(Color.FromArgb(150, 160, 40, 60), 3f)) g.DrawBezier(p, belly.X + 20, belly.Y + 10, belly.X + len * 0.4f, belly.Y + 25, tip.X - 100, tip.Y + 20, tip.X - 30, tip.Y);
                }
            }
            else if (stitches)
            {
                // zavrena tlama: cik-cak zuby (pozvykuje)
                var pts = new List<PointF>();
                for (int i = 0; i <= 8; i++) pts.Add(new PointF(belly.X - 100 + i * 25, belly.Y + (i % 2 == 0 ? -10 : 12)));
                using var p = new Pen(Color.FromArgb(230, 240, 240, 240), 5f) { LineJoin = LineJoin.Round };
                g.DrawLines(p, pts.ToArray());
            }
        }
        Save(b, dir, name);
    }

    static void Wing(Graphics g, bool back, float open)
    {
        float side = back ? -1 : 1;
        var shoulder = new PointF(HX + side * 55, Top + 20);
        // zavreno: spicka u lemu; otevreno: spicka vysoko a daleko (jako v konceptu)
        var closedTip = new PointF(HX + side * 190, HY - 30);
        var openTip = new PointF(HX + side * 420, Top - 90);
        var tip = new PointF(closedTip.X + (openTip.X - closedTip.X) * open, closedTip.Y + (openTip.Y - closedTip.Y) * open);
        var hem = new PointF(HX + side * 110, HY - 20);

        using var path = new GraphicsPath();
        path.AddBezier(shoulder, new PointF(shoulder.X + side * 90, shoulder.Y - 40 * open), new PointF(tip.X - side * 120, tip.Y + 10), tip);
        Scallops(path, tip, hem, 3, 60 * open + 10);
        path.AddBezier(hem, new PointF(hem.X - side * 30, hem.Y - 120), new PointF(shoulder.X, shoulder.Y + 120), shoulder);
        path.CloseFigure();
        Cloth(g, path, back ? 41 : 51);
        using var rim = new Pen(Color.FromArgb(200, 245, 245, 245), 3f);
        g.DrawBezier(rim, new PointF(shoulder.X + side * 10, shoulder.Y + 10), new PointF(shoulder.X + side * 90, shoulder.Y - 20 * open), new PointF(tip.X - side * 120, tip.Y + 25), new PointF(tip.X - side * 15, tip.Y + 12));
    }

    static void Teeth(Graphics g, RectangleF r, bool top)
    {
        int n = 7;
        using var white = new SolidBrush(Color.FromArgb(250, 250, 248));
        using var p = InkPen(2.5f);
        for (int i = 0; i < n; i++)
        {
            float t0 = (i + 0.1f) / n, t1 = (i + 0.9f) / n, tm = (i + 0.5f) / n;
            PointF E(float t) // bod na elipse (horni/dolni pulka)
            {
                float a = MathF.PI * (top ? 1f + t : 1f - t);
                return new PointF(r.X + r.Width / 2 + MathF.Cos(a) * r.Width / 2 * 0.98f, r.Y + r.Height / 2 + MathF.Sin(a) * r.Height / 2 * 0.98f);
            }
            PointF a0 = E(t0), a1 = E(t1), am = E(tm);
            float len = r.Height * 0.28f * (1f - MathF.Abs(tm - 0.5f));
            var tipP = new PointF(am.X, am.Y + (top ? len : -len));
            var tri = new[] { a0, tipP, a1 };
            g.FillPolygon(white, tri);
            g.DrawPolygon(p, tri);
        }
    }

    static void Save(Bitmap b, string dir, string name)
    {
        b.Save(Path.Combine(dir, name + ".png"), ImageFormat.Png);
        b.Dispose();
    }
}
