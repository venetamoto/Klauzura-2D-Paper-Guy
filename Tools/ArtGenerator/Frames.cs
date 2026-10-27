using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;

// Cuts the hand-drawn character out of FlipaClip video frames: paper -> transparent, same crop for all frames, feet aligned.
public static class Frames
{
    public static void Cut(string dir, string tag, string outDir, string outName)
    {
        Directory.CreateDirectory(outDir);
        var files = Directory.GetFiles(dir, tag + "_*.png").OrderBy(f => int.Parse(Path.GetFileNameWithoutExtension(f).Split('_')[1])).ToArray();
        var bmps = files.Select(f => new Bitmap(f)).ToArray();
        int w = bmps[0].Width, h = bmps[0].Height;

        // union bbox of ink pixels (ignore FlipaClip watermark in the bottom-left corner)
        int minX = w, minY = h, maxX = 0, maxY = 0;
        foreach (var b in bmps)
            for (int y = 0; y < h; y++)
                for (int x = 0; x < w; x++)
                {
                    if (x < 300 && y > h - 150) continue;
                    if (Lum(b.GetPixel(x, y)) < 150)
                    {
                        minX = Math.Min(minX, x); maxX = Math.Max(maxX, x);
                        minY = Math.Min(minY, y); maxY = Math.Max(maxY, y);
                    }
                }
        int pad = 12;
        var rect = Rectangle.FromLTRB(Math.Max(0, minX - pad), Math.Max(0, minY - pad), Math.Min(w, maxX + pad), Math.Min(h, maxY + pad));
        Console.WriteLine($"{tag}: crop {rect.Width}x{rect.Height} at {rect.X},{rect.Y}");

        for (int i = 0; i < bmps.Length; i++)
        {
            var src = bmps[i];
            var dst = new Bitmap(rect.Width, rect.Height, PixelFormat.Format32bppArgb);
            for (int y = 0; y < rect.Height; y++)
                for (int x = 0; x < rect.Width; x++)
                {
                    Color c = src.GetPixel(rect.X + x, rect.Y + y);
                    float lum = Lum(c);
                    float a = Math.Clamp((232f - lum) / 150f, 0f, 1f);
                    dst.SetPixel(x, y, Color.FromArgb((int)(a * 255), 30, 30, 36));
                }
            dst.Save(Path.Combine(outDir, $"{outName}_{i}.png"), ImageFormat.Png);
            dst.Dispose();
        }
        foreach (var b in bmps) b.Dispose();
    }

    static float Lum(Color c) => 0.299f * c.R + 0.587f * c.G + 0.114f * c.B;

    static readonly Color InkC = Color.FromArgb(255, 30, 30, 36);

    // vyrizne postavu z konceptu: pozadi (bile, spojene s okrajem) pruhledne, vnitrek hlavy zustane bily
    public static void CutConcept(string src, int x0, int y0, int x1, int y1, string outPath)
    {
        using var img = new Bitmap(src);
        int w = x1 - x0, h = y1 - y0;
        var outside = new bool[w, h];
        var stack = new System.Collections.Generic.Stack<(int, int)>();
        for (int x = 0; x < w; x++) { stack.Push((x, 0)); stack.Push((x, h - 1)); }
        for (int y = 0; y < h; y++) { stack.Push((0, y)); stack.Push((w - 1, y)); }
        while (stack.Count > 0)
        {
            var (x, y) = stack.Pop();
            if (x < 0 || y < 0 || x >= w || y >= h || outside[x, y]) continue;
            if (Lum(img.GetPixel(x0 + x, y0 + y)) < 225) continue;
            outside[x, y] = true;
            stack.Push((x + 1, y)); stack.Push((x - 1, y)); stack.Push((x, y + 1)); stack.Push((x, y - 1));
        }
        using var dst = new Bitmap(w, h, PixelFormat.Format32bppArgb);
        for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
            {
                Color c = img.GetPixel(x0 + x, y0 + y);
                if (!outside[x, y]) { dst.SetPixel(x, y, Color.FromArgb(255, c.R, c.G, c.B)); continue; }
                float a = Math.Clamp((250f - Lum(c)) / 120f, 0f, 1f);
                dst.SetPixel(x, y, Color.FromArgb((int)(a * 255), 30, 30, 36));
            }
        using var trimmed = TrimAlpha(dst, 4);
        trimmed.Save(outPath, ImageFormat.Png);
    }

    // celni pohled: hlava (2 snimky - oci/mrknuti) a telo zvlast, 2x zvetsene
    public static void MakeFront(string frontPath, string outDir)
    {
        using var src0 = new Bitmap(frontPath);
        int k = 2;
        using var src = new Bitmap(src0.Width * k, src0.Height * k, PixelFormat.Format32bppArgb);
        using (var g = Graphics.FromImage(src))
        {
            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
            g.DrawImage(src0, 0, 0, src.Width, src.Height);
        }

        int neck = 99 * k;
        using (var head = src.Clone(new Rectangle(0, 0, src.Width, neck), PixelFormat.Format32bppArgb))
        {
            head.Save(Path.Combine(outDir, "Hrdina_Hlava_0.png"), ImageFormat.Png);

            // oci = tmave body uvnitr hlavy
            var eyes = new System.Collections.Generic.List<Point>();
            for (int y = 50 * k; y < 90 * k; y++)
                for (int x = 15 * k; x < 90 * k; x++)
                {
                    Color c = head.GetPixel(x, y);
                    if (c.A > 200 && Lum(c) < 110) eyes.Add(new Point(x, y));
                }
            var left = eyes.Where(p => p.X < src.Width / 2).ToList();
            var right = eyes.Where(p => p.X >= src.Width / 2).ToList();
            Console.WriteLine($"eyes: L {left.Count} R {right.Count}");

            using var blink = new Bitmap(head);
            using var g = Graphics.FromImage(blink);
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            using var pen = new Pen(InkC, 3f * k / 2f) { StartCap = System.Drawing.Drawing2D.LineCap.Round, EndCap = System.Drawing.Drawing2D.LineCap.Round };
            foreach (var group in new[] { left, right })
            {
                if (group.Count == 0) continue;
                int cx = (int)group.Average(p => p.X), cy = (int)group.Average(p => p.Y);
                using (var white = new SolidBrush(Color.White)) g.FillEllipse(white, cx - 6 * k / 2 - 2, cy - 6 * k / 2 - 2, 6 * k + 4, 6 * k + 4);
                g.DrawLine(pen, cx - 5 * k / 2 - 1, cy, cx + 5 * k / 2 + 1, cy);
            }
            blink.Save(Path.Combine(outDir, "Hrdina_Hlava_1.png"), ImageFormat.Png);
        }
        using (var body = src.Clone(new Rectangle(0, neck - 2 * k, src.Width, src.Height - neck + 2 * k), PixelFormat.Format32bppArgb))
            body.Save(Path.Combine(outDir, "Hrdina_Telo.png"), ImageFormat.Png);
        Console.WriteLine($"front: {src.Width}x{src.Height}, neck {neck}");
    }

    // jidlo z boku: pusinka -> velke O -> prezvykne
    public static void MakeEat(string dir)
    {
        using var baseFrame = new Bitmap(Path.Combine(dir, "Hrdina_Chuze_0.png"));
        var mouth = new Point(146, 160);
        for (int i = 0; i < 3; i++)
        {
            using var b = new Bitmap(baseFrame);
            using var g = Graphics.FromImage(b);
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            using var pen = new Pen(InkC, 5f) { StartCap = System.Drawing.Drawing2D.LineCap.Round, EndCap = System.Drawing.Drawing2D.LineCap.Round };
            using var ink = new SolidBrush(InkC);
            if (i == 0)
                g.DrawEllipse(pen, mouth.X - 6, mouth.Y - 7, 12, 14);
            else if (i == 1)
                g.FillEllipse(ink, mouth.X - 11, mouth.Y - 14, 22, 28);
            else
            {
                g.DrawArc(pen, mouth.X - 10, mouth.Y - 8, 18, 12, 10, 160);
                g.DrawArc(pen, mouth.X - 22, mouth.Y - 12, 10, 10, 200, 120);
            }
            b.Save(Path.Combine(dir, $"Hrdina_Ji_{i}.png"), ImageFormat.Png);
        }
    }

    static Bitmap TrimAlpha(Bitmap b, int pad)
    {
        int minX = b.Width, minY = b.Height, maxX = -1, maxY = -1;
        for (int y = 0; y < b.Height; y++)
            for (int x = 0; x < b.Width; x++)
                if (b.GetPixel(x, y).A > 20) { minX = Math.Min(minX, x); maxX = Math.Max(maxX, x); minY = Math.Min(minY, y); maxY = Math.Max(maxY, y); }
        var r = Rectangle.FromLTRB(Math.Max(0, minX - pad), Math.Max(0, minY - pad), Math.Min(b.Width, maxX + pad + 1), Math.Min(b.Height, maxY + pad + 1));
        return b.Clone(r, PixelFormat.Format32bppArgb);
    }

    // stani (rovne nohy) + mrknuti; mezisnimky chuze
    public static void MakeExtra(string dir)
    {
        // snimky zarovnat podle oka, aby postava necukala
        Align(dir, "Hrdina_Han", 4);
        Align(dir, "Hrdina_Sin", 7);

        using var baseFrame = new Bitmap(Path.Combine(dir, "Hrdina_Han_0.png"));

        Bitmap Standing()
        {
            var b = new Bitmap(baseFrame);
            for (int y = 263; y < b.Height; y++)
                for (int x = 0; x < b.Width; x++) b.SetPixel(x, y, Color.Transparent);
            using var g = Graphics.FromImage(b);
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            using var pen = new Pen(InkC, 9f) { StartCap = System.Drawing.Drawing2D.LineCap.Round, EndCap = System.Drawing.Drawing2D.LineCap.Round, LineJoin = System.Drawing.Drawing2D.LineJoin.Round };
            g.DrawLines(pen, new[] { new PointF(70, 256), new PointF(68, 288), new PointF(55, 292) });
            g.DrawLines(pen, new[] { new PointF(106, 256), new PointF(108, 288), new PointF(121, 292) });
            return b;
        }

        using (var open = Standing()) open.Save(Path.Combine(dir, "Hrdina_Stoji_0.png"), ImageFormat.Png);
        using (var closed = Standing())
        {
            // smazat oko a nakreslit zavrene vicko
            for (int y = 127; y <= 145; y++)
                for (int x = 113; x <= 131; x++)
                    if ((x - 122) * (x - 122) + (y - 136) * (y - 136) <= 90) closed.SetPixel(x, y, Color.Transparent);
            using var g = Graphics.FromImage(closed);
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            using var pen = new Pen(InkC, 5f) { StartCap = System.Drawing.Drawing2D.LineCap.Round, EndCap = System.Drawing.Drawing2D.LineCap.Round };
            g.DrawLine(pen, 115, 137, 129, 135);
            closed.Save(Path.Combine(dir, "Hrdina_Stoji_1.png"), ImageFormat.Png);
        }

        // mezisnimky: kazdy par sousednich snimku smichat (silnejsi pristi snimek)
        var walk = Enumerable.Range(0, 4).Select(i => new Bitmap(Path.Combine(dir, $"Hrdina_Han_{i}.png"))).ToArray();
        int k = 0;
        for (int i = 0; i < 4; i++)
        {
            walk[i].Save(Path.Combine(dir, $"Hrdina_Chuze_{k++}.png"), ImageFormat.Png);
            var a = walk[i]; var c = walk[(i + 1) % 4];
            var mix = new Bitmap(a.Width, a.Height, PixelFormat.Format32bppArgb);
            for (int y = 0; y < a.Height; y++)
                for (int x = 0; x < a.Width; x++)
                {
                    int aa = a.GetPixel(x, y).A, ca = c.GetPixel(x, y).A;
                    int alpha = Math.Max((int)(aa * 0.55f), ca);
                    mix.SetPixel(x, y, Color.FromArgb(alpha, 30, 30, 36));
                }
            mix.Save(Path.Combine(dir, $"Hrdina_Chuze_{k++}.png"), ImageFormat.Png);
            mix.Dispose();
        }
        foreach (var w in walk) w.Dispose();
    }

    static void Align(string dir, string prefix, int count)
    {
        var frames = Enumerable.Range(0, count).Select(i => new Bitmap(Path.Combine(dir, $"{prefix}_{i}.png"))).ToArray();
        var eyes = frames.Select(Eye).ToArray();
        Console.WriteLine(prefix + " eyes: " + string.Join(" ", eyes.Select(e => $"({e.X},{e.Y})")));
        for (int i = 0; i < count; i++)
        {
            int dx = eyes[0].X - eyes[i].X, dy = eyes[0].Y - eyes[i].Y;
            var moved = new Bitmap(frames[i].Width, frames[i].Height, PixelFormat.Format32bppArgb);
            for (int y = 0; y < moved.Height; y++)
                for (int x = 0; x < moved.Width; x++)
                {
                    int sx = x - dx, sy = y - dy;
                    moved.SetPixel(x, y, sx >= 0 && sy >= 0 && sx < moved.Width && sy < moved.Height ? frames[i].GetPixel(sx, sy) : Color.Transparent);
                }
            frames[i].Dispose();
            moved.Save(Path.Combine(dir, $"{prefix}_{i}.png"), ImageFormat.Png);
            moved.Dispose();
        }
    }

    // oko = nejtmavsi mala skvrna v horni polovine obrazku
    static Point Eye(Bitmap b)
    {
        var seen = new bool[b.Width, b.Height];
        Point best = new Point(b.Width / 2, b.Height / 3);
        int bestScore = int.MinValue;
        for (int y = 0; y < b.Height / 2 + 20; y++)
            for (int x = 0; x < b.Width; x++)
            {
                if (seen[x, y] || b.GetPixel(x, y).A < 170) continue;
                var stack = new System.Collections.Generic.Stack<(int, int)>();
                stack.Push((x, y)); seen[x, y] = true;
                int n = 0, sx = 0, sy = 0;
                while (stack.Count > 0)
                {
                    var (px, py) = stack.Pop(); n++; sx += px; sy += py;
                    foreach (var (ddx, ddy) in new[] { (1, 0), (-1, 0), (0, 1), (0, -1) })
                    {
                        int nx = px + ddx, ny = py + ddy;
                        if (nx < 0 || ny < 0 || nx >= b.Width || ny >= b.Height || seen[nx, ny] || b.GetPixel(nx, ny).A < 170) continue;
                        seen[nx, ny] = true; stack.Push((nx, ny));
                    }
                }
                if (n < 40 || n > 400) continue;
                int score = n;
                if (score > bestScore) { bestScore = score; best = new Point(sx / n, sy / n); }
            }
        return best;
    }

    // prints ink extents per row band and dark blobs (to find the eye)
    public static void Inspect(string path)
    {
        using var b = new Bitmap(path);
        Console.WriteLine($"{b.Width}x{b.Height}");
        for (int y = 0; y < b.Height; y += 8)
        {
            var xs = Enumerable.Range(0, b.Width).Where(x => b.GetPixel(x, y).A > 120).ToArray();
            Console.WriteLine($"y{y,3}: " + (xs.Length == 0 ? "-" : string.Join(" ", Runs(xs))));
        }
        // blobs of strong ink
        var seen = new bool[b.Width, b.Height];
        for (int y = 0; y < b.Height; y++)
            for (int x = 0; x < b.Width; x++)
            {
                if (seen[x, y] || b.GetPixel(x, y).A < 150) continue;
                var stack = new System.Collections.Generic.Stack<(int, int)>();
                stack.Push((x, y)); seen[x, y] = true;
                int n = 0, sx = 0, sy = 0, x0 = x, x1 = x, y0 = y, y1 = y;
                while (stack.Count > 0)
                {
                    var (px, py) = stack.Pop(); n++; sx += px; sy += py;
                    x0 = Math.Min(x0, px); x1 = Math.Max(x1, px); y0 = Math.Min(y0, py); y1 = Math.Max(y1, py);
                    foreach (var (dx, dy) in new[] { (1, 0), (-1, 0), (0, 1), (0, -1) })
                    {
                        int nx = px + dx, ny = py + dy;
                        if (nx < 0 || ny < 0 || nx >= b.Width || ny >= b.Height || seen[nx, ny] || b.GetPixel(nx, ny).A < 150) continue;
                        seen[nx, ny] = true; stack.Push((nx, ny));
                    }
                }
                Console.WriteLine($"blob n={n} center=({sx / n},{sy / n}) box=({x0},{y0})-({x1},{y1})");
            }
    }

    static System.Collections.Generic.IEnumerable<string> Runs(int[] xs)
    {
        int s = xs[0], p = xs[0];
        for (int i = 1; i <= xs.Length; i++)
        {
            if (i < xs.Length && xs[i] == p + 1) { p = xs[i]; continue; }
            yield return $"{s}-{p}";
            if (i < xs.Length) { s = p = xs[i]; }
        }
    }
}
