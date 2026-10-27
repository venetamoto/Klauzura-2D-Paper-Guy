using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;

// Hrdina ve stylu konceptu, kresleny ostre ve vysokem rozliseni (400 px = 1 jednotka).
public static class Hero
{
    static readonly Color Ink = Color.FromArgb(20, 20, 24);
    static readonly Color Freckle = Color.FromArgb(95, 95, 100);
    const int HeadW = 480, HeadH = 560;          // platno hlavy
    const float Cx = 240, Cy = 330, R = 196;     // kruh hlavy (spodek = krk v y 526)

    public static void RenderAll(string dir)
    {
        Directory.CreateDirectory(dir);
        Head(dir, "Hrdina_Hlava_Zepredu", front: true, eyes: "open", mouth: 0);
        Head(dir, "Hrdina_Hlava_Zepredu_Mrk", front: true, eyes: "closed", mouth: 0);
        Head(dir, "Hrdina_Hlava_Bok", front: false, eyes: "open", mouth: 0);
        Head(dir, "Hrdina_Hlava_Bok_Mrk", front: false, eyes: "closed", mouth: 0);
        Head(dir, "Hrdina_Hlava_Bok_Ji_0", front: false, eyes: "open", mouth: 1);
        Head(dir, "Hrdina_Hlava_Bok_Ji_1", front: false, eyes: "happy", mouth: 2);
        Head(dir, "Hrdina_Hlava_Bok_Ji_2", front: false, eyes: "happy", mouth: 3);
        Cape(dir, "Hrdina_Plast_Zepredu", front: true);
        Cape(dir, "Hrdina_Plast_Bok", front: false);
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

    // ---------- hlava ----------
    static void Head(string dir, string name, bool front, string eyes, int mouth)
    {
        Bitmap b = Canvas(HeadW, HeadH, out Graphics g);
        using (g)
        {
            Leaf(g, front);

            using (var white = new SolidBrush(Color.White)) g.FillEllipse(white, Cx - R, Cy - R, 2 * R, 2 * R);
            // obrys: dva tahy tuzkou kvuli rucnimu vzhledu
            using (var p = InkPen(6f)) g.DrawEllipse(p, Cx - R, Cy - R, 2 * R, 2 * R);
            using (var p = new Pen(Color.FromArgb(90, Ink), 3f)) g.DrawEllipse(p, Cx - R + 3, Cy - R - 2, 2 * R - 4, 2 * R + 1);

            // pihy
            float fx = front ? Cx - 10 : Cx - 60;
            using (var fb = new SolidBrush(Freckle))
            {
                g.FillEllipse(fb, fx - 22, Cy - 150, 11, 11);
                g.FillEllipse(fb, fx + 2, Cy - 168, 10, 10);
                g.FillEllipse(fb, fx + 10, Cy - 138, 11, 11);
            }

            // oci
            var eyePos = front ? new[] { new PointF(Cx - 78, Cy + 6), new PointF(Cx + 78, Cy + 6) } : new[] { new PointF(Cx + 70, Cy - 4) };
            foreach (var e in eyePos)
            {
                if (eyes == "open")
                {
                    using var eb = new SolidBrush(Ink);
                    g.FillEllipse(eb, e.X - 10, e.Y - 14, 20, 28);
                    using var hl = new SolidBrush(Color.FromArgb(200, 255, 255, 255));
                    g.FillEllipse(hl, e.X - 4, e.Y - 10, 6, 7);
                }
                else if (eyes == "closed")
                {
                    using var p = InkPen(7f);
                    g.DrawLine(p, e.X - 15, e.Y + 2, e.X + 15, e.Y + 2);
                }
                else
                {
                    using var p = InkPen(7f);
                    g.DrawArc(p, e.X - 15, e.Y - 8, 30, 20, 200, 140);
                }
            }

            // pusa pri jidle (z boku, vpredu dole)
            var m = new PointF(Cx + 125, Cy + 70);
            if (mouth == 1)
            {
                using var p = InkPen(7f);
                g.DrawEllipse(p, m.X - 14, m.Y - 16, 28, 32);
            }
            else if (mouth == 2)
            {
                using var mb = new SolidBrush(Ink);
                g.FillEllipse(mb, m.X - 30, m.Y - 38, 54, 72);
                using var tongue = new SolidBrush(Color.FromArgb(230, 120, 130));
                g.FillEllipse(tongue, m.X - 18, m.Y + 8, 30, 20);
            }
            else if (mouth == 3)
            {
                using var p = InkPen(7f);
                g.DrawArc(p, m.X - 26, m.Y - 12, 40, 24, 10, 160);
                using var cheek = new SolidBrush(Color.FromArgb(70, 230, 120, 130));
                g.FillEllipse(cheek, m.X - 58, m.Y - 30, 44, 30);
            }
        }
        Save(b, dir, name);
    }

    // listek navrchu: tmavy prechod a bile cik-cak srafy
    static void Leaf(Graphics g, bool front)
    {
        using var path = new GraphicsPath();
        if (front)
        {
            path.AddBezier(170, 128, 220, 40, 330, 20, 390, 40);
            path.AddBezier(390, 40, 340, 110, 250, 140, 170, 128);
        }
        else
        {
            path.AddBezier(150, 132, 210, 70, 330, 50, 420, 78);
            path.AddBezier(420, 78, 340, 120, 230, 140, 150, 132);
        }
        path.CloseFigure();
        RectangleF bb = path.GetBounds();
        using (var grad = new LinearGradientBrush(bb, Color.FromArgb(15, 15, 18), Color.FromArgb(120, 120, 126), LinearGradientMode.Horizontal))
            g.FillPath(grad, path);

        Region old = g.Clip;
        g.SetClip(path);
        using (var p = new Pen(Color.FromArgb(170, 235, 235, 235), 3.5f))
        {
            var pts = new System.Collections.Generic.List<PointF>();
            for (float x = bb.Left + 10; x < bb.Right - 10; x += 14)
                pts.Add(new PointF(x, (pts.Count % 2 == 0) ? bb.Top + bb.Height * 0.35f : bb.Bottom - bb.Height * 0.25f));
            if (pts.Count > 1) g.DrawLines(p, pts.ToArray());
        }
        g.Clip = old;
        using (var p = InkPen(4f)) g.DrawPath(p, path);

        // stopka listku
        using (var p = InkPen(6f)) g.DrawBezier(p, 230, 140, 225, 125, 228, 115, 236, 108);
    }

    // ---------- plastik ----------
    static void Cape(string dir, string name, bool front)
    {
        Bitmap b = Canvas(440, 320, out Graphics g);
        using (g)
        {
            using var path = new GraphicsPath();
            if (front)
            {
                path.AddBezier(120, 22, 170, 8, 270, 8, 320, 22);
                path.AddLine(320, 22, 410, 250);
                path.AddBezier(410, 250, 330, 305, 110, 305, 30, 250);
                path.AddLine(30, 250, 120, 22);
            }
            else
            {
                path.AddBezier(95, 20, 150, 6, 230, 8, 270, 20);
                path.AddLine(270, 20, 410, 225);
                path.AddBezier(410, 225, 330, 300, 140, 310, 70, 280);
                path.AddBezier(70, 280, 60, 200, 70, 90, 95, 20);
            }
            path.CloseFigure();
            RectangleF bb = path.GetBounds();

            using (var dark = new LinearGradientBrush(bb, Color.FromArgb(8, 8, 10), Color.FromArgb(55, 55, 60), LinearGradientMode.ForwardDiagonal))
                g.FillPath(dark, path);

            // bile skrabance (krizove srafovani)
            Region old = g.Clip;
            g.SetClip(path);
            var rnd = new Random(front ? 3 : 7);
            using (var p = new Pen(Color.FromArgb(85, 235, 235, 235), 2f))
                for (int i = 0; i < 150; i++)
                {
                    float x = bb.Left + (float)rnd.NextDouble() * bb.Width, y = bb.Top + (float)rnd.NextDouble() * bb.Height;
                    float len = 40 + (float)rnd.NextDouble() * 70;
                    float a = (i % 2 == 0 ? 55f : -55f) + (float)(rnd.NextDouble() * 20 - 10);
                    float r = a * MathF.PI / 180f;
                    g.DrawLine(p, x, y, x + MathF.Cos(r) * len, y + MathF.Sin(r) * len);
                }
            g.Clip = old;

            // svetla linka podel okraje a uprostred (jako v konceptu)
            using (var rim = new Pen(Color.FromArgb(200, 245, 245, 245), 3f))
            {
                if (front)
                {
                    g.DrawLine(rim, 220, 30, 220, 285);
                    g.DrawLine(rim, 128, 34, 46, 240);
                    g.DrawLine(rim, 312, 34, 394, 240);
                }
                else
                {
                    g.DrawLine(rim, 262, 30, 395, 225);
                    g.DrawBezier(rim, 84, 270, 74, 200, 82, 100, 104, 32);
                }
            }
            using (var p = InkPen(5f)) g.DrawPath(p, path);
        }
        Save(b, dir, name);
    }

    static void Save(Bitmap b, string dir, string name)
    {
        b.Save(Path.Combine(dir, name + ".png"), ImageFormat.Png);
        b.Dispose();
    }
}
