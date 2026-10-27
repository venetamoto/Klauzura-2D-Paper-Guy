using System.Linq;
using System;
using System.IO;

string outDir = args.Length > 0 ? args[0] : ".";
string what = args.Length > 1 ? args[1] : "all";
Directory.CreateDirectory(outDir);

if (what == "all" || what == "hand")
{
    Hand.Render(Path.Combine(outDir, "Hand_Pencil.png"), 1024);
    Hand.Reversed = true;
    Hand.Render(Path.Combine(outDir, "Hand_Eraser.png"), 1024);
    Hand.Reversed = false;
    Console.WriteLine("hand ok");
}
if (what == "all" || what == "doodles")
{
    Doodles.RenderAll(outDir);
    Console.WriteLine("doodles ok");
}
if (what == "layout")
{
    float ppu = 24f, minX = -17.5f, maxX = 50.5f, minY = -5.6f, maxY = 25.4f;
    int W = (int)((maxX - minX) * ppu), H = (int)((maxY - minY) * ppu);
    using var bmp = new System.Drawing.Bitmap(W, H);
    using var g = System.Drawing.Graphics.FromImage(bmp);
    g.Clear(System.Drawing.Color.White);
    g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
    g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
    System.Drawing.PointF S(float x, float y) => new System.Drawing.PointF((x - minX) * ppu, (maxY - y) * ppu);
    void Put(string name, float x, float y, float srcPpu = 200f)
    {
        using var img = System.Drawing.Image.FromFile(Path.Combine(outDir, name + ".png"));
        float w = img.Width / srcPpu * ppu, h = img.Height / srcPpu * ppu;
        var c = S(x, y);
        g.DrawImage(img, c.X - w / 2, c.Y - h / 2, w, h);
    }
    using (var wall = new System.Drawing.SolidBrush(System.Drawing.Color.FromArgb(60, 0, 0, 0)))
    {
        var a = S(minX, -5f); g.FillRectangle(wall, a.X, a.Y, W, 0.6f * ppu);
    }
    Put("Text_Ukol", -12f, -3.3f); Put("Text_Vypocitej", -7.5f, -0.9f); Put("Text_Rovnice", -12.5f, 1.6f);
    Put("Text_Reseni", -8f, 3.9f); Put("Text_Graf", -4.4f, 6.3f);
    var pts = new System.Collections.Generic.List<System.Drawing.PointF>();
    for (int i = 0; i <= 40; i++) { float x = 3f * i / 40f; pts.Add(S(-3.2f + x * 2f, 6.67f - x * x * 0.6f)); }
    using (var pen = new System.Drawing.Pen(System.Drawing.Color.FromArgb(32, 44, 120), 3f)) g.DrawLines(pen, pts.ToArray());
    Put("Kresba_Mrak", 6.5f, -2.2f);
    Put("Shape_Ctverec", 12.5f, -3.69f); Put("Shape_Trojuhelnik", 16.5f, -3.75f); Put("Shape_Kruh", 20f, -1.2f);
    Put("Shape_Sestiuhelnik", 24f, 1.2f); Put("Shape_Lichobeznik", 28.5f, 3.9f); Put("Shape_Krychle", 32.5f, 5.5f);
    Put("Kresba_Mrak", 37.5f, 7.6f); Put("Text_Matematika", 43.5f, 10.6f); Put("Kresba_Slunce", 39.2f, 12.2f); Put("Text_Hotovo", 44.5f, 15.3f);
    Put("Kresba_Dum", 45.5f, -3.25f);
    Put("Edible_X4", -10f, 3.4f); Put("Edible_Pi", -4.4f, 8.3f); Put("Edible_Jablko", 7.5f, -0.4f); Put("Edible_Kruh", 18.3f, 0.8f);
    Put("Edible_Trojuhelnik", 26.3f, 3.6f); Put("Edible_Ctverec", 30.5f, 6.6f); Put("Edible_Nasobeni", 35.5f, 10.3f); Put("Edible_Srdce", 47.3f, 16.6f);
    Put("Hand_Pencil", 40f + 1.1f, -3.99f + 1.9f, 256f);
    using (var b = new System.Drawing.SolidBrush(System.Drawing.Color.Black)) { var p = S(-15.4f, -3.9f); g.FillRectangle(b, p.X, p.Y, 0.8f * ppu, 1f * ppu); }
    bmp.Save(args[2]);
}
if (what == "osnova")
{
    float ppu = 24f, minX = -17.5f, maxX = 50.5f, minY = -5.6f, maxY = 25.4f;
    int W = (int)((maxX - minX) * ppu), H = (int)((maxY - minY) * ppu);
    using var bmp = new System.Drawing.Bitmap(W, H);
    using var g = System.Drawing.Graphics.FromImage(bmp);
    g.Clear(System.Drawing.Color.White);
    using var font = new System.Drawing.Font("Segoe UI", 8);
    var inv = System.Globalization.CultureInfo.InvariantCulture;
    foreach (var line in File.ReadAllLines(args[2]))
    {
        var parts = line.Split(';');
        var p = parts[1].Split(','); var s = parts[2].Split(','); var c = parts[3].Split(',');
        float x = float.Parse(p[0], inv), y = float.Parse(p[1], inv);
        float sx = s[0] == "" ? 1f : Math.Abs(float.Parse(s[0], inv)), sy = s[1] == "" ? 17.74f : Math.Abs(float.Parse(s[1], inv));
        if (parts[0].StartsWith("Square") && s[0] == "") sx = 1f;
        var color = System.Drawing.Color.FromArgb(200, (int)(float.Parse(c[0], inv) * 255), (int)(float.Parse(c[1], inv) * 255), (int)(float.Parse(c[2], inv) * 255));
        float rx = (x - sx / 2 - minX) * ppu, ry = (maxY - (y + sy / 2)) * ppu;
        using var b = new System.Drawing.SolidBrush(color);
        g.FillRectangle(b, rx, ry, sx * ppu, sy * ppu);
        g.DrawString(parts[0].Replace("text_platform", "t").Replace("Drawing_platforms", "D").Replace("Edible_", "E_"), font, System.Drawing.Brushes.Black, rx, ry - 12);
    }
    bmp.Save(args[3]);
}
if (what == "sheet")
{
    var files = Directory.GetFiles(outDir, "*.png");
    int cell = 260, cols = 6, rows = (files.Length + cols - 1) / cols;
    using var sheet = new System.Drawing.Bitmap(cols * cell, rows * (cell + 24));
    using var g = System.Drawing.Graphics.FromImage(sheet);
    g.Clear(System.Drawing.Color.White);
    g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
    using var font = new System.Drawing.Font("Segoe UI", 11);
    for (int i = 0; i < files.Length; i++)
    {
        using var img = System.Drawing.Image.FromFile(files[i]);
        float k = Math.Min((cell - 20f) / img.Width, (cell - 20f) / img.Height);
        int x = (i % cols) * cell, y = (i / cols) * (cell + 24);
        g.DrawImage(img, x + (cell - img.Width * k) / 2, y + (cell - img.Height * k) / 2, img.Width * k, img.Height * k);
        g.DrawString(Path.GetFileNameWithoutExtension(files[i]) + $" {img.Width}x{img.Height}", font, System.Drawing.Brushes.Gray, x + 4, y + cell);
    }
    sheet.Save(args[2]);
}
if (what == "plan")
{
    Plan.Check(outDir);
    Plan.Preview(outDir, args[2]);
}
if (what == "export")
{
    Console.Write(Plan.ExportCSharp());
}
if (what == "frames")
{
    // args: framesDir tag outDir outName
    Frames.Cut(outDir, args[2], args[3], args[4]);
}
if (what == "pose")
{
    float k = 200f; // px per unit in preview
    using var bmp = new System.Drawing.Bitmap(900, 620);
    using var g = System.Drawing.Graphics.FromImage(bmp);
    g.Clear(System.Drawing.Color.White);
    g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
    g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
    using var pen = new System.Drawing.Pen(System.Drawing.Color.FromArgb(20, 20, 26), 0.04f * k) { StartCap = System.Drawing.Drawing2D.LineCap.Round, EndCap = System.Drawing.Drawing2D.LineCap.Round, LineJoin = System.Drawing.Drawing2D.LineJoin.Round };
    void Pose(float ox, string headName, string capeName, (float, float)[][] legs, bool flip)
    {
        float feetY = 560; // y of local -0.5
        System.Drawing.PointF P(float x, float y) => new(ox + (flip ? -x : x) * k, feetY - (y + 0.5f) * k);
        foreach (var leg in legs) g.DrawLines(pen, leg.Select(v => P(v.Item1, v.Item2)).ToArray());
        using var cape = System.Drawing.Image.FromFile(Path.Combine(outDir, capeName + ".png"));
        float cw = cape.Width / 400f * k, ch = cape.Height / 400f * k;
        var cb = P(0, -0.02f); g.DrawImage(cape, cb.X - cw / 2, cb.Y - ch * (1 - 15f / 320f), cw, ch);
        using var head = System.Drawing.Image.FromFile(Path.Combine(outDir, headName + ".png"));
        float hw = head.Width / 400f * k, hh = head.Height / 400f * k;
        var hb = P(0, 0.68f); g.DrawImage(head, hb.X - hw / 2, hb.Y - hh * (1 - 34f / 560f), hw, hh);
    }
    Pose(220, "Hrdina_Hlava_Zepredu", "Hrdina_Plast_Zepredu", new[] {
        new[] { (-0.16f, 0f), (-0.17f, -0.25f), (-0.18f, -0.5f), (-0.27f, -0.5f) },
        new[] { (0.16f, 0f), (0.17f, -0.25f), (0.18f, -0.5f), (0.27f, -0.5f) } }, false);
    Pose(640, "Hrdina_Hlava_Bok", "Hrdina_Plast_Bok", new[] {
        new[] { (-0.04f, 0f), (0.1f, -0.22f), (0.16f, -0.5f), (0.25f, -0.5f) },
        new[] { (0.04f, 0f), (0.02f, -0.24f), (-0.2f, -0.4f), (-0.11f, -0.4f) } }, false);
    bmp.Save(args[2]);
}
if (what == "heroeat")
{
    HeroEat.RenderAll(outDir);
}
if (what == "eatpose")
{
    // args: heroDir eatDir out ; poses: walk side, fall, eat 1, eat 2
    string heroDir = outDir, eatDir = args[2];
    float k = 150f;
    using var bmp = new System.Drawing.Bitmap(1500, 520);
    using var g = System.Drawing.Graphics.FromImage(bmp);
    g.Clear(System.Drawing.Color.White);
    g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
    g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
    using var pen = new System.Drawing.Pen(System.Drawing.Color.FromArgb(20, 20, 26), 0.03f * k) { StartCap = System.Drawing.Drawing2D.LineCap.Round, EndCap = System.Drawing.Drawing2D.LineCap.Round };
    void Img(string path, float px, float py, float pivotX, float pivotY)
    {
        using var im = System.Drawing.Image.FromFile(path);
        float w = im.Width / 400f * k, h = im.Height / 400f * k;
        g.DrawImage(im, px - w * pivotX, py - h * (1 - pivotY), w, h);
    }
    void Pose(float ox, string cape, bool big, string headName, (float, float)[][] legs)
    {
        float fy = 470;
        System.Drawing.PointF P(float x, float y) => new(ox + x * k, fy - (y + 0.5f) * k);
        foreach (var leg in legs) g.DrawLines(pen, leg.Select(v => P(v.Item1, v.Item2)).ToArray());
        var hb = P(0, 0.68f); Img(Path.Combine(heroDir, headName + ".png"), hb.X, hb.Y, 0.5f, 34f / 560f);
        var cb = P(0, -0.02f);
        if (big) Img(Path.Combine(eatDir, cape + ".png"), cb.X, cb.Y, 0.5f, 100f / 800f);
        else Img(Path.Combine(eatDir, cape + ".png"), cb.X, cb.Y, 0.5f, 15f / 320f);
    }
    var walk = new[] { new[] { (-0.04f, 0f), (0.1f, -0.22f), (0.16f, -0.5f), (0.25f, -0.5f) }, new[] { (0.04f, 0f), (0.02f, -0.24f), (-0.2f, -0.4f), (-0.11f, -0.4f) } };
    var dangle = new[] { new[] { (-0.04f, 0f), (-0.02f, -0.24f), (-0.08f, -0.48f), (0.01f, -0.48f) }, new[] { (0.04f, 0f), (0.1f, -0.22f), (0.1f, -0.45f), (0.19f, -0.45f) } };
    Pose(150, "Hrdina_Plast_Bok", false, "Hrdina_Hlava_Bok", walk);
    Pose(480, "Hrdina_Plast_Pad_0", true, "Hrdina_Hlava_Bok_Ji_0", dangle);
    Pose(850, "Hrdina_Plast_Ji_1", true, "Hrdina_Hlava_Bok_Ji_1", walk);
    Pose(1220, "Hrdina_Plast_Ji_2", true, "Hrdina_Hlava_Bok_Ji_1", walk);
    bmp.Save(args[3]);
}
if (what == "hero")
{
    Hero.RenderAll(outDir);
}
if (what == "concept")
{
    // args: src x0 y0 x1 y1 out
    Frames.CutConcept(args[2], int.Parse(args[3]), int.Parse(args[4]), int.Parse(args[5]), int.Parse(args[6]), args[7]);
}
if (what == "front")
{
    Frames.MakeFront(args[2], outDir);
}
if (what == "eat")
{
    Frames.MakeEat(outDir);
}
if (what == "extra")
{
    Frames.MakeExtra(outDir);
}
if (what == "inspect")
{
    Frames.Inspect(args[2]);
}
if (what == "rects")
{
    Rects.Check(outDir);
    Rects.Preview(outDir, args[2]);
}
if (what == "rexport")
{
    Console.Write(Rects.Export(outDir));
}
if (what == "level2")
{
    Level2.Check();
    Level2.Preview(args[2]);
}
if (what == "level2export")
{
    Console.Write(Level2.Export());
}
