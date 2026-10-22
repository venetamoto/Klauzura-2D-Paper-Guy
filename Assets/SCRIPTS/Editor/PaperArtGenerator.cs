using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace PaperGuy.EditorTools
{
    // Procedurálně kreslené sprity: ruka s tužkou a bílý list.
    public static class PaperArtGenerator
    {
        public const string Folder = "Assets/Art/Generated";

        // ---------- Ruka ----------
        private const int HandCanvas = 512;
        private const float HandWorldSize = 4f;          // velikost plátna ve světě (hráč je ~1 jednotka)
        private static readonly Vector2 PencilTip = new Vector2(0.30f, 0.03f); // pivot spritu = hrot tužky

        private static readonly Color Skin = new Color(0.96f, 0.80f, 0.66f);
        private static readonly Color SkinShade = new Color(0.88f, 0.66f, 0.52f);
        private static readonly Color SkinOutline = new Color(0.32f, 0.19f, 0.13f);
        private static readonly Color Nail = new Color(1f, 0.88f, 0.86f);
        private static readonly Color Sleeve = new Color(0.52f, 0.66f, 0.86f);
        private static readonly Color SleeveShade = new Color(0.42f, 0.55f, 0.75f);
        private static readonly Color Cuff = new Color(0.96f, 0.96f, 0.98f);
        private static readonly Color PencilOutline = new Color(0.16f, 0.12f, 0.10f);

        private delegate float Sdf(Vector2 p);

        private struct Layer
        {
            public Sdf Shape;
            public Func<Vector2, Color> Fill;
            public Color Outline;
            public float OutlinePx;
        }

        public static Sprite GenerateHandSprite()
        {
            var tex = new Texture2D(HandCanvas, HandCanvas, TextureFormat.RGBA32, false);
            tex.SetPixels(RenderHandPixels());
            return SaveSprite(tex, "Hand_Pencil", HandCanvas / HandWorldSize, PencilTip, FilterMode.Bilinear, TextureWrapMode.Clamp);
        }

        public static Color[] RenderHandPixels()
        {
            Layer[] layers =
            {
                // rukáv a manžeta
                Shaded(p => Capsule(p, new Vector2(0.76f, 0.69f), new Vector2(1.10f, 1.02f), 0.125f), Sleeve, SleeveShade, SkinOutline),
                Solid(p => Capsule(p, new Vector2(0.67f, 0.60f), new Vector2(0.72f, 0.645f), 0.13f), Cuff, SkinOutline),
                // dlaň
                Shaded(p => Ellipse(p, new Vector2(0.52f, 0.47f), new Vector2(0.20f, 0.15f), 35f), Skin, SkinShade, SkinOutline),
                // prsty za tužkou (prostředníček, prsteníček, malíček)
                Shaded(p => Ellipse(p, new Vector2(0.47f, 0.27f), new Vector2(0.060f, 0.040f), 0f), Skin, SkinShade, SkinOutline),
                Shaded(p => Ellipse(p, new Vector2(0.40f, 0.27f), new Vector2(0.070f, 0.045f), 10f), Skin, SkinShade, SkinOutline),
                Shaded(p => Ellipse(p, new Vector2(0.33f, 0.30f), new Vector2(0.075f, 0.045f), 20f), Skin, SkinShade, SkinOutline),
                // tužka
                new Layer { Shape = PencilSdf, Fill = PencilFill, Outline = PencilOutline, OutlinePx = 3f },
                // palec a ukazováček přes tužku
                Shaded(p => Capsule(p, new Vector2(0.50f, 0.40f), new Vector2(0.36f, 0.33f), 0.050f), Skin, SkinShade, SkinOutline),
                Shaded(p => Capsule(p, new Vector2(0.52f, 0.56f), new Vector2(0.335f, 0.405f), 0.048f), Skin, SkinShade, SkinOutline),
                new Layer
                {
                    Shape = p => Ellipse(p, new Vector2(0.345f, 0.41f), new Vector2(0.026f, 0.018f), 40f),
                    Fill = _ => Nail, Outline = SkinOutline, OutlinePx = 1.5f
                },
            };

            return Render(HandCanvas, layers);
        }

        // ---------- Tužka ----------
        private const float PencilX = 0.30f, PencilHalfWidth = 0.034f;
        private const float TipY = 0.03f, GraphiteY = 0.055f, ConeY = 0.12f, BodyY = 0.78f, FerruleY = 0.84f, EraserY = 0.89f;

        private static float PencilSdf(Vector2 p)
        {
            float halfWidth = p.y < TipY ? 0f
                : p.y < ConeY ? PencilHalfWidth * (p.y - TipY) / (ConeY - TipY)
                : PencilHalfWidth;
            float dx = Mathf.Abs(p.x - PencilX) - halfWidth;
            float dy = Mathf.Max(TipY - p.y, p.y - EraserY);
            return Mathf.Max(dx, dy);
        }

        private static Color PencilFill(Vector2 p)
        {
            if (p.y < GraphiteY) return new Color(0.25f, 0.25f, 0.28f);
            if (p.y < ConeY) return new Color(0.93f, 0.78f, 0.58f);
            if (p.y < BodyY)
            {
                float side = (p.x - PencilX) / PencilHalfWidth; // hrany šestihranu
                if (side < -0.33f) return new Color(0.86f, 0.60f, 0.10f);
                if (side > 0.33f) return new Color(1f, 0.86f, 0.38f);
                return new Color(0.98f, 0.74f, 0.18f);
            }
            if (p.y < FerruleY)
            {
                bool band = Mathf.Abs(p.y - 0.80f) < 0.004f || Mathf.Abs(p.y - 0.82f) < 0.004f;
                return band ? new Color(0.50f, 0.50f, 0.55f) : new Color(0.78f, 0.79f, 0.83f);
            }
            return new Color(0.95f, 0.56f, 0.62f);
        }

        // ---------- Bílý list ----------
        public static Sprite GenerateWhitePixel()
        {
            var tex = new Texture2D(4, 4, TextureFormat.RGBA32, false);
            for (int y = 0; y < 4; y++)
                for (int x = 0; x < 4; x++)
                    tex.SetPixel(x, y, Color.white);
            return SaveSprite(tex, "White_Pixel", 4f, new Vector2(0.5f, 0.5f), FilterMode.Point, TextureWrapMode.Clamp);
        }

        // =====================================================================
        //  Vykreslení
        // =====================================================================

        private static Layer Solid(Sdf shape, Color fill, Color outline) =>
            new Layer { Shape = shape, Fill = _ => fill, Outline = outline, OutlinePx = 3.5f };

        // Stín na straně odvrácené od světla (světlo zprava shora).
        private static Layer Shaded(Sdf shape, Color fill, Color shade, Color outline)
        {
            var shadowDir = new Vector2(-0.6f, -0.8f) * 0.022f;
            return new Layer
            {
                Shape = shape,
                Fill = p => shape(p + shadowDir) < 0f ? fill : shade,
                Outline = outline,
                OutlinePx = 3.5f
            };
        }

        private static Color[] Render(int size, Layer[] layers)
        {
            float px = 1f / size;
            var pixels = new Color[size * size];

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    var p = new Vector2((x + 0.5f) * px, (y + 0.5f) * px);
                    Color col = Color.clear;
                    foreach (Layer layer in layers)
                    {
                        float d = layer.Shape(p);
                        float outline = layer.OutlinePx * px;
                        if (d > outline + px) continue;

                        float fillCoverage = Mathf.Clamp01(0.5f - d / px);
                        float outerCoverage = Mathf.Clamp01(0.5f - (d - outline) / px);
                        Color c = Color.Lerp(layer.Outline, layer.Fill(p), fillCoverage);
                        c.a = outerCoverage;
                        col = Over(c, col);
                    }
                    pixels[y * size + x] = col;
                }
            }
            return pixels;
        }

        private static Color Over(Color top, Color bottom)
        {
            float a = top.a + bottom.a * (1f - top.a);
            if (a <= 0f) return Color.clear;
            Color rgb = (top * top.a + bottom * bottom.a * (1f - top.a)) / a;
            rgb.a = a;
            return rgb;
        }

        private static float Capsule(Vector2 p, Vector2 a, Vector2 b, float r)
        {
            Vector2 pa = p - a, ba = b - a;
            float h = Mathf.Clamp01(Vector2.Dot(pa, ba) / Vector2.Dot(ba, ba));
            return (pa - ba * h).magnitude - r;
        }

        private static float Ellipse(Vector2 p, Vector2 c, Vector2 r, float angleDeg)
        {
            float a = -angleDeg * Mathf.Deg2Rad;
            Vector2 q = p - c;
            q = new Vector2(q.x * Mathf.Cos(a) - q.y * Mathf.Sin(a), q.x * Mathf.Sin(a) + q.y * Mathf.Cos(a));
            float k = new Vector2(q.x / r.x, q.y / r.y).magnitude;
            return (k - 1f) * Mathf.Min(r.x, r.y);
        }

        // =====================================================================
        //  Uložení jako sprite
        // =====================================================================

        private static Sprite SaveSprite(Texture2D tex, string name, float pixelsPerUnit, Vector2 pivot,
            FilterMode filter, TextureWrapMode wrap)
        {
            Directory.CreateDirectory(Folder);
            string path = $"{Folder}/{name}.png";
            File.WriteAllBytes(path, tex.EncodeToPNG());
            UnityEngine.Object.DestroyImmediate(tex);
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);

            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = pixelsPerUnit;
            importer.filterMode = filter;
            importer.wrapMode = wrap;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;
            importer.textureCompression = TextureImporterCompression.Uncompressed;

            var settings = new TextureImporterSettings();
            importer.ReadTextureSettings(settings);
            settings.spriteMeshType = SpriteMeshType.FullRect;
            settings.spriteAlignment = (int)SpriteAlignment.Custom;
            settings.spritePivot = pivot;
            importer.SetTextureSettings(settings);
            importer.SaveAndReimport();

            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }
    }
}
