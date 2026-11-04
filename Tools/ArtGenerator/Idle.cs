using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;

// Kresba autorky (postava stoji) -> hlava, hlava s mrknutim a telo s pruhlednym pozadim.
public static class Idle
{
    public const int Left = 850, Right = 1175, Top = 295, Neck = 662, Bottom = 965;

    public static void Render(string src, string dir)
    {
        using var image = new Bitmap(src);
        using (var head = Part(image, Top, Neck, 445, 650)) head.Save(Path.Combine(dir, "Stoji_Hlava.png"));
        using (var blink = Part(image, Top, Neck, 445, 650))
        {
            Blink(blink, 932 - Left, 570 - Top);
            Blink(blink, 1083 - Left, 568 - Top);
            blink.Save(Path.Combine(dir, "Stoji_Hlava_Mrk.png"));
        }
        using (var body = Part(image, Neck, Bottom, Neck + 3, 835)) body.Save(Path.Combine(dir, "Stoji_Telo.png"));
    }

    static float Lum(Color c) => (c.R + c.G + c.B) / 3f;

    // tuzka zustane, bily papir zmizi; vnitrek hlavy/plastiku se vyplni bilou
    static Bitmap Part(Bitmap image, int top, int bottom, int fillFrom, int fillTo)
    {
        var part = new Bitmap(Right - Left, bottom - top);
        for (int y = top; y < bottom; y++)
        {
            int first = -1, last = -1;
            if (y >= fillFrom && y <= fillTo)
                for (int x = Left; x < Right; x++)
                    if (Lum(image.GetPixel(x, y)) < 150) { if (first < 0) first = x; last = x; }

            for (int x = Left; x < Right; x++)
            {
                float lum = Lum(image.GetPixel(x, y));
                float a = Math.Clamp((250f - lum) / 200f, 0f, 1f);
                Color c = Color.FromArgb((int)(a * 255), 35, 35, 38);
                if (first >= 0 && x > first && x < last && a < 1f)
                {
                    int v = a < 0.35f ? 255 : (int)(255 - a * 220);
                    c = Color.FromArgb(255, v, v, v);
                }
                part.SetPixel(x - Left, y - top, c);
            }
        }
        return part;
    }

    static void Blink(Bitmap head, int cx, int cy)
    {
        using var g = Graphics.FromImage(head);
        g.SmoothingMode = SmoothingMode.AntiAlias;
        using (var white = new SolidBrush(Color.White)) g.FillEllipse(white, cx - 20, cy - 20, 40, 40);
        using var pen = new Pen(Color.FromArgb(235, 35, 35, 38), 6f) { StartCap = LineCap.Round, EndCap = LineCap.Round };
        g.DrawArc(pen, cx - 16, cy - 10, 32, 16, 20, 140);
    }
}
