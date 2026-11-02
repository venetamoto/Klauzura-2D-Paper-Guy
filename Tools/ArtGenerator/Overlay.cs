using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

// Z obrazku sesitu udela pruhledny obrazek: bily papir zmizi, zustanou linky a propiska.
public static class Overlay
{
    public static void Render(string src, string dst, string paper, string preview)
    {
        const int Zoom = 3;
        using var source = new Bitmap(src);
        using var big = new Bitmap(source.Width * Zoom, source.Height * Zoom, PixelFormat.Format32bppArgb);
        using (var g = Graphics.FromImage(big))
        {
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.DrawImage(source, 0, 0, big.Width, big.Height);
        }

        for (int y = 0; y < big.Height; y++)
            for (int x = 0; x < big.Width; x++)
            {
                Color c = big.GetPixel(x, y);
                float a = Math.Min(1f, (1f - Math.Min(c.R, Math.Min(c.G, c.B)) / 245f) * 1.5f);
                if (a < 0.04f) { big.SetPixel(x, y, Color.Transparent); continue; }
                big.SetPixel(x, y, Color.FromArgb((int)(a * 255), Back(c.R, a), Back(c.G, a), Back(c.B, a)));
            }
        big.Save(dst);

        using var photo = new Bitmap(paper);
        using var view = new Bitmap(1968, 744);
        using var gv = Graphics.FromImage(view);
        gv.InterpolationMode = InterpolationMode.HighQualityBicubic;
        gv.DrawImage(photo, new Rectangle(0, 0, 1968, 744), new Rectangle(0, photo.Height / 4, photo.Width, photo.Height / 2), GraphicsUnit.Pixel);
        gv.DrawImage(big, 0, 0, 1968, 744);
        view.Save(preview);
    }

    static int Back(int v, float a)
    {
        return Math.Max(0, Math.Min(255, (int)((v - (1f - a) * 255f) / a)));
    }
}
