using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;

// Foto (pozadi) zvetsit a doostrit (unsharp mask), aby na obrazovce nebylo mydlove.
static class Zaostrit
{
    public static void Udelej(string zdroj, string cil, float meritko, float sila)
    {
        Bitmap src = new Bitmap(zdroj);
        int w = (int)Math.Round(src.Width * meritko), h = (int)Math.Round(src.Height * meritko);
        Bitmap velky = new Bitmap(w, h, PixelFormat.Format24bppRgb);
        using (Graphics g = Graphics.FromImage(velky))
        {
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            using var ia = new ImageAttributes();
            ia.SetWrapMode(WrapMode.TileFlipXY);
            g.DrawImage(src, new Rectangle(0, 0, w, h), 0, 0, src.Width, src.Height, GraphicsUnit.Pixel, ia);
        }
        src.Dispose();

        BitmapData d = velky.LockBits(new Rectangle(0, 0, w, h), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
        int stride = d.Stride;
        byte[] px = new byte[stride * h];
        System.Runtime.InteropServices.Marshal.Copy(d.Scan0, px, 0, px.Length);

        // rozmazani 5x5 (dva pruchody) a unsharp
        int r = 2;
        float[] tmp = new float[stride * h], blur = new float[stride * h];
        for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
                for (int c = 0; c < 3; c++)
                {
                    float s = 0; int n = 0;
                    for (int k = -r; k <= r; k++) { int xx = Math.Clamp(x + k, 0, w - 1); s += px[y * stride + xx * 3 + c]; n++; }
                    tmp[y * stride + x * 3 + c] = s / n;
                }
        for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
                for (int c = 0; c < 3; c++)
                {
                    float s = 0; int n = 0;
                    for (int k = -r; k <= r; k++) { int yy = Math.Clamp(y + k, 0, h - 1); s += tmp[yy * stride + x * 3 + c]; n++; }
                    blur[y * stride + x * 3 + c] = s / n;
                }
        for (int y = 0; y < h; y++)
            for (int x = 0; x < w * 3; x++)
            {
                int i = y * stride + x;
                float v = px[i] + sila * (px[i] - blur[i]);
                px[i] = (byte)Math.Clamp((int)Math.Round(v), 0, 255);
            }
        System.Runtime.InteropServices.Marshal.Copy(px, 0, d.Scan0, px.Length);
        velky.UnlockBits(d);

        ImageCodecInfo jpg = ImageCodecInfo.GetImageEncoders().First(e => e.MimeType == "image/jpeg");
        var ep = new EncoderParameters(1);
        ep.Param[0] = new EncoderParameter(Encoder.Quality, 95L);
        velky.Save(cil, jpg, ep);
        velky.Dispose();
        Console.WriteLine(cil + ": " + w + "x" + h + ", doostreno " + sila);
    }
}
