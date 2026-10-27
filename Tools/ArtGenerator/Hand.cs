using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Numerics;

// Realistic-ish hand holding a pencil, rendered with signed distance fields.
// Canvas is normalized [0,1]^2, y up. Pencil tip is at (0.30, 0.03) = sprite pivot.
static class Hand
{
    struct C
    {
        public float R, G, B, A;
        public C(float r, float g, float b, float a = 1f) { R = r; G = g; B = b; A = a; }
        public static C Lerp(C a, C b, float t) => new C(a.R + (b.R - a.R) * t, a.G + (b.G - a.G) * t, a.B + (b.B - a.B) * t, a.A + (b.A - a.A) * t);
        public C Mul(float k) => new C(R * k, G * k, B * k, A);
    }

    delegate float Sdf(Vector2 p);
    delegate C Fill(Vector2 p);

    class Layer
    {
        public Sdf Shape;
        public Fill Fill;
        public C Outline;
        public float OutlineW;      // normalized
        public float ShadowStrength; // drop shadow cast onto what is already painted
        public Vector2 ShadowOffset = new Vector2(-0.006f, -0.010f);
    }

    static readonly Vector3 Light = Vector3.Normalize(new Vector3(0.45f, 0.6f, 0.75f));

    static readonly C SkinBase = new C(0.95f, 0.77f, 0.65f);
    static readonly C SkinShadow = new C(0.76f, 0.50f, 0.42f);
    static readonly C SkinLight = new C(1.00f, 0.92f, 0.85f);
    static readonly C SkinLine = new C(0.48f, 0.30f, 0.24f);

    public static bool Reversed;

    static Vector2 Flip(Vector2 p) => Reversed ? new Vector2(p.X, TipY + EraserY - p.Y) : p;

    public static void Render(string path, int size)
    {
        var layers = new List<Layer>();

        // sleeve + cuff
        layers.Add(Cyl(new Vector2(0.78f, 0.71f), new Vector2(1.18f, 1.08f), 0.13f,
            new C(0.47f, 0.60f, 0.80f), new C(0.30f, 0.40f, 0.60f), new C(0.66f, 0.77f, 0.92f), new C(0.20f, 0.26f, 0.40f), 0f));
        foreach (float k in new[] { 0.25f, 0.5f, 0.75f })
            layers.Add(Crease(Vector2.Lerp(new Vector2(0.80f, 0.64f), new Vector2(1.10f, 0.93f), k) + new Vector2(0.03f, 0.02f),
                Vector2.Lerp(new Vector2(0.80f, 0.64f), new Vector2(1.10f, 0.93f), k) + new Vector2(0.08f, 0.07f), 0.0035f, new C(0.28f, 0.37f, 0.56f, 0.7f)));
        layers.Add(Cyl(new Vector2(0.68f, 0.61f), new Vector2(0.74f, 0.665f), 0.135f,
            new C(0.95f, 0.95f, 0.97f), new C(0.72f, 0.74f, 0.80f), new C(1f, 1f, 1f), new C(0.35f, 0.36f, 0.42f), 0.25f));
        layers.Add(new Layer
        {
            Shape = p => Circle(p, new Vector2(0.63f, 0.71f), 0.012f),
            Fill = p => new C(0.85f, 0.85f, 0.88f),
            Outline = new C(0.4f, 0.4f, 0.45f), OutlineW = 0.002f
        });

        // back of the hand / palm
        layers.Add(Ell(new Vector2(0.53f, 0.48f), new Vector2(0.20f, 0.15f), 35f, 0.30f));
        // knuckle hints on the back of the hand
        layers.Add(Crease(new Vector2(0.47f, 0.56f), new Vector2(0.50f, 0.585f), 0.003f, SkinShadow.WithA(0.45f)));
        layers.Add(Crease(new Vector2(0.55f, 0.36f), new Vector2(0.60f, 0.39f), 0.003f, SkinShadow.WithA(0.35f)));

        // curled fingers behind the pencil: pinky, ring, middle
        layers.Add(CylSkin(new Vector2(0.52f, 0.29f), new Vector2(0.45f, 0.26f), 0.040f, 0.2f));
        layers.Add(CylSkin(new Vector2(0.47f, 0.30f), new Vector2(0.38f, 0.265f), 0.046f, 0.2f));
        layers.Add(CylSkin(new Vector2(0.42f, 0.33f), new Vector2(0.31f, 0.29f), 0.047f, 0.2f));
        layers.Add(Nail(new Vector2(0.292f, 0.287f), 0.020f, 0.015f, 190f));

        // pencil
        layers.Add(new Layer { Shape = PencilSdf, Fill = PencilFill, Outline = new C(0.18f, 0.13f, 0.10f), OutlineW = 0.0035f });

        // thumb (two segments) over the pencil
        layers.Add(CylSkin(new Vector2(0.55f, 0.43f), new Vector2(0.43f, 0.365f), 0.054f, 0.35f));
        layers.Add(CylSkin(new Vector2(0.43f, 0.365f), new Vector2(0.355f, 0.335f), 0.047f, 0.30f));
        layers.Add(Crease(new Vector2(0.425f, 0.40f), new Vector2(0.44f, 0.33f), 0.0025f, SkinShadow.WithA(0.6f)));
        layers.Add(Nail(new Vector2(0.348f, 0.336f), 0.026f, 0.019f, 205f));

        // index finger (two segments) on top of the pencil
        layers.Add(CylSkin(new Vector2(0.55f, 0.58f), new Vector2(0.425f, 0.475f), 0.052f, 0.35f));
        layers.Add(CylSkin(new Vector2(0.425f, 0.475f), new Vector2(0.335f, 0.405f), 0.046f, 0.30f));
        layers.Add(Crease(new Vector2(0.405f, 0.50f), new Vector2(0.445f, 0.45f), 0.0025f, SkinShadow.WithA(0.6f)));
        layers.Add(Crease(new Vector2(0.50f, 0.565f), new Vector2(0.52f, 0.53f), 0.0022f, SkinShadow.WithA(0.4f)));
        layers.Add(Nail(new Vector2(0.338f, 0.408f), 0.028f, 0.020f, 215f));

        var bmp = new Bitmap(size, size, PixelFormat.Format32bppArgb);
        int ss = 3;
        float px = 1f / size;
        var data = new int[size * size];
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float r = 0, g = 0, b = 0, a = 0;
                for (int sy = 0; sy < ss; sy++)
                for (int sx = 0; sx < ss; sx++)
                {
                    var p = new Vector2((x + (sx + 0.5f) / ss) * px, 1f - (y + (sy + 0.5f) / ss) * px);
                    C c = Shade(p, layers);
                    r += c.R * c.A; g += c.G * c.A; b += c.B * c.A; a += c.A;
                }
                int n = ss * ss;
                float A = a / n;
                if (A <= 0f) { data[y * size + x] = 0; continue; }
                int R = Clamp255(r / a), G = Clamp255(g / a), B = Clamp255(b / a);
                data[y * size + x] = (Clamp255(A) << 24) | (R << 16) | (G << 8) | B;
            }
        }
        var bd = bmp.LockBits(new Rectangle(0, 0, size, size), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
        System.Runtime.InteropServices.Marshal.Copy(data, 0, bd.Scan0, data.Length);
        bmp.UnlockBits(bd);
        bmp.Save(path, ImageFormat.Png);
    }

    static int Clamp255(float v) => Math.Max(0, Math.Min(255, (int)Math.Round(v * 255f)));

    static C Shade(Vector2 p, List<Layer> layers)
    {
        C col = new C(0, 0, 0, 0);
        foreach (Layer l in layers)
        {
            if (l.ShadowStrength > 0f && col.A > 0f)
            {
                float ds = l.Shape(p - l.ShadowOffset);
                float s = 1f - SmoothStep(-0.004f, 0.018f, ds);
                if (s > 0f) col = col.Mul(1f - l.ShadowStrength * s);
            }

            float d = l.Shape(p);
            if (d > l.OutlineW) continue;
            C c = d < 0f ? l.Fill(p) : l.Outline;
            col = Over(c, col);
        }
        return col;
    }

    static C Over(C top, C bottom)
    {
        float a = top.A + bottom.A * (1f - top.A);
        if (a <= 0f) return new C(0, 0, 0, 0);
        float k = bottom.A * (1f - top.A);
        return new C((top.R * top.A + bottom.R * k) / a, (top.G * top.A + bottom.G * k) / a, (top.B * top.A + bottom.B * k) / a, a);
    }

    static C WithA(this C c, float a) => new C(c.R, c.G, c.B, a);

    static float SmoothStep(float e0, float e1, float x)
    {
        float t = Math.Clamp((x - e0) / (e1 - e0), 0f, 1f);
        return t * t * (3f - 2f * t);
    }

    static float Noise(Vector2 p)
    {
        float n = MathF.Sin(p.X * 1270.1f + p.Y * 3117.7f) * 43758.5453f;
        return n - MathF.Floor(n);
    }

    // ---------- shapes ----------
    static float Capsule(Vector2 p, Vector2 a, Vector2 b, float r)
    {
        Vector2 pa = p - a, ba = b - a;
        float h = Math.Clamp(Vector2.Dot(pa, ba) / Vector2.Dot(ba, ba), 0f, 1f);
        return (pa - ba * h).Length() - r;
    }

    static float Circle(Vector2 p, Vector2 c, float r) => (p - c).Length() - r;

    static Vector2 Rot(Vector2 v, float deg)
    {
        float a = deg * MathF.PI / 180f;
        return new Vector2(v.X * MathF.Cos(a) - v.Y * MathF.Sin(a), v.X * MathF.Sin(a) + v.Y * MathF.Cos(a));
    }

    static float Ellipse(Vector2 p, Vector2 c, Vector2 r, float deg)
    {
        Vector2 q = Rot(p - c, -deg);
        float k = new Vector2(q.X / r.X, q.Y / r.Y).Length();
        return (k - 1f) * MathF.Min(r.X, r.Y);
    }

    // ---------- shading ----------
    static C Lit(Vector3 n, C baseC, C shadow, C light)
    {
        float diff = Math.Max(0f, Vector3.Dot(n, Light));
        C c = C.Lerp(shadow, baseC, SmoothStep(0.05f, 0.75f, diff));
        float spec = SmoothStep(0.88f, 1f, diff);
        return C.Lerp(c, light, spec * 0.35f);
    }

    static Layer Cyl(Vector2 a, Vector2 b, float r, C baseC, C shadow, C light, C outline, float castShadow)
    {
        return new Layer
        {
            Shape = p => Capsule(p, a, b, r),
            Fill = p =>
            {
                Vector2 pa = p - a, ba = b - a;
                float h = Math.Clamp(Vector2.Dot(pa, ba) / Vector2.Dot(ba, ba), 0f, 1f);
                Vector2 d = (p - (a + ba * h)) / r;
                float t = Math.Min(d.Length(), 0.999f);
                Vector3 n = Vector3.Normalize(new Vector3(d.X, d.Y, MathF.Sqrt(1f - t * t)));
                C c = Lit(n, baseC, shadow, light);
                return c.Mul(1f + (Noise(p) - 0.5f) * 0.03f);
            },
            Outline = outline, OutlineW = 0.0025f, ShadowStrength = castShadow
        };
    }

    static Layer CylSkin(Vector2 a, Vector2 b, float r, float castShadow) =>
        Cyl(a, b, r, SkinBase, SkinShadow, SkinLight, SkinLine, castShadow);

    static Layer Ell(Vector2 c, Vector2 r, float deg, float castShadow)
    {
        return new Layer
        {
            Shape = p => Ellipse(p, c, r, deg),
            Fill = p =>
            {
                Vector2 q = Rot(p - c, -deg);
                Vector2 d = new Vector2(q.X / r.X, q.Y / r.Y);
                float t = Math.Min(d.Length(), 0.999f);
                Vector2 n2 = Rot(d, deg);
                Vector3 n = Vector3.Normalize(new Vector3(n2.X * 0.8f, n2.Y * 0.8f, MathF.Sqrt(1f - t * t) + 0.3f));
                return Lit(n, SkinBase, SkinShadow, SkinLight).Mul(1f + (Noise(p) - 0.5f) * 0.03f);
            },
            Outline = SkinLine, OutlineW = 0.0025f, ShadowStrength = castShadow
        };
    }

    static Layer Crease(Vector2 a, Vector2 b, float w, C color) =>
        new Layer { Shape = p => Capsule(p, a, b, w), Fill = p => color, Outline = color, OutlineW = 0f };

    static Layer Nail(Vector2 c, float rx, float ry, float deg)
    {
        return new Layer
        {
            Shape = p => Ellipse(p, c, new Vector2(rx, ry), deg),
            Fill = p =>
            {
                Vector2 q = Rot(p - c, -deg);
                float u = q.X / rx; // -1 base .. +1 free edge
                C nail = C.Lerp(new C(0.96f, 0.76f, 0.74f), new C(1f, 0.95f, 0.92f), SmoothStep(0.3f, 0.9f, u));
                float hl = 1f - SmoothStep(0.0f, 0.35f, (new Vector2(q.X / rx + 0.1f, q.Y / ry - 0.35f)).Length());
                return C.Lerp(nail, new C(1f, 1f, 1f), hl * 0.6f);
            },
            Outline = new C(0.70f, 0.45f, 0.42f), OutlineW = 0.0018f
        };
    }

    // ---------- pencil ----------
    const float PX = 0.30f, PW = 0.034f;
    const float TipY = 0.03f, GraphiteY = 0.058f, ConeY = 0.125f, BodyY = 0.78f, FerruleY = 0.845f, EraserY = 0.895f;

    static float PencilSdf(Vector2 q)
    {
        Vector2 p = Flip(q);
        float hw = p.Y < TipY ? 0f : p.Y < ConeY ? PW * (p.Y - TipY) / (ConeY - TipY) : PW;
        float dx = MathF.Abs(p.X - PX);
        float rr = PW * 0.5f;
        if (p.Y > EraserY - rr && dx > PW - rr)
            return new Vector2(dx - (PW - rr), p.Y - (EraserY - rr)).Length() - rr;
        return MathF.Max(MathF.Max(dx - hw, TipY - p.Y), p.Y - EraserY);
    }

    static C PencilFill(Vector2 q)
    {
        Vector2 p = Flip(q);
        float u = (p.X - PX) / PW; // -1..1 across
        float cyl = MathF.Sqrt(Math.Max(0f, 1f - u * u));
        float light = 0.55f + 0.45f * Math.Clamp(cyl * 0.7f + u * 0.5f, 0f, 1f);

        if (p.Y < GraphiteY)
        {
            float shine = SmoothStep(0.6f, 1f, light);
            return C.Lerp(new C(0.18f, 0.18f, 0.20f), new C(0.55f, 0.56f, 0.60f), shine);
        }
        if (p.Y < ConeY)
        {
            float grain = 0.92f + 0.08f * MathF.Sin(u * 18f + p.Y * 40f);
            return new C(0.93f, 0.76f, 0.55f).Mul(light * grain);
        }
        if (p.Y < BodyY)
        {
            C face = u < -0.33f ? new C(0.84f, 0.56f, 0.08f) : u > 0.33f ? new C(1f, 0.84f, 0.32f) : new C(0.98f, 0.72f, 0.14f);
            if (MathF.Abs(MathF.Abs(u) - 0.33f) < 0.04f) face = face.Mul(0.85f);
            return face.Mul(0.8f + 0.25f * light);
        }
        if (p.Y < FerruleY)
        {
            float ridge = 0.85f + 0.15f * MathF.Sin(p.Y * 900f);
            C metal = C.Lerp(new C(0.45f, 0.46f, 0.50f), new C(0.92f, 0.93f, 0.96f), SmoothStep(0.5f, 1f, light));
            return metal.Mul(ridge);
        }
        return C.Lerp(new C(0.78f, 0.38f, 0.44f), new C(0.98f, 0.66f, 0.70f), light);
    }
}
