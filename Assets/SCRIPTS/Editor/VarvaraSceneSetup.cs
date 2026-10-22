using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PaperGuy.Animations;
using PaperGuy.Interactions;
using PaperGuy.Level;
using PaperGuy.Player;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;

namespace PaperGuy.EditorTools
{
    [InitializeOnLoad]
    public static class VarvaraSceneSetup
    {
        private const string ScenePath = "Assets/Scenes/Varvara.unity";
        private const string InputActionsPath = "Assets/Scenes/Settings/InputSystem_Actions.inputactions";
        private static readonly string AutoRunKey = "PaperGuy.VarvaraSceneSetup.v1." + Application.dataPath;
        private static readonly string CameraAutoRunKey = "PaperGuy.VarvaraSceneSetup.camera.v1." + Application.dataPath;
        private static readonly string GraphHandAutoRunKey = "PaperGuy.VarvaraSceneSetup.graphHand.v1." + Application.dataPath;
        private static readonly string SheetLevelAutoRunKey = "PaperGuy.VarvaraSceneSetup.sheetLevel.v1." + Application.dataPath;
        private static readonly string WhiteNarrowAutoRunKey = "PaperGuy.VarvaraSceneSetup.whiteNarrow.v1." + Application.dataPath;

        static VarvaraSceneSetup()
        {
            if (Application.isBatchMode) return;
            if (EditorPrefs.GetBool(AutoRunKey, false) && EditorPrefs.GetBool(CameraAutoRunKey, false)
                && EditorPrefs.GetBool(GraphHandAutoRunKey, false) && EditorPrefs.GetBool(SheetLevelAutoRunKey, false)
                && EditorPrefs.GetBool(WhiteNarrowAutoRunKey, false)) return;
            EditorApplication.delayCall += TryAutoRun;
            EditorSceneManager.sceneOpened += (_, _) => TryAutoRun();
        }

        private static void TryAutoRun()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            if (EditorSceneManager.GetActiveScene().path != ScenePath) return;

            if (!EditorPrefs.GetBool(AutoRunKey, false))
            {
                EditorPrefs.SetBool(AutoRunKey, true);
                Run();
            }
            if (!EditorPrefs.GetBool(CameraAutoRunKey, false))
            {
                EditorPrefs.SetBool(CameraAutoRunKey, true);
                SetupCamera();
            }
            if (!EditorPrefs.GetBool(GraphHandAutoRunKey, false))
            {
                EditorPrefs.SetBool(GraphHandAutoRunKey, true);
                SetupGraphAndHand();
            }
            if (!EditorPrefs.GetBool(SheetLevelAutoRunKey, false))
            {
                EditorPrefs.SetBool(SheetLevelAutoRunKey, true);
                ExpandLevel(2.5f, 1.7f); // jen jednou — opakované spuštění by úroveň zvětšilo znovu
                SetupSheetAndHand();
            }
            if (!EditorPrefs.GetBool(WhiteNarrowAutoRunKey, false))
            {
                EditorPrefs.SetBool(WhiteNarrowAutoRunKey, true);
                ExpandLevel(0.8f, 1f);   // list o kousek užší
                SetupSheetAndHand();     // čistě bílý list
            }
        }

        [MenuItem("Paper Guy/Graf, ruka a otřes při dopadu (Varvara)", priority = 4)]
        public static void SetupGraphAndHandFromMenu()
        {
            if (EditorSceneManager.GetActiveScene().path != ScenePath)
            {
                if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
                EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            }
            SetupGraphAndHand();
        }

        // 1) Kamera se třese jen při dopadu (odstraní občasné otřesy).
        // 2) Graph_Parabola: propojí LineRenderer + EdgeCollider2D a přidá Slide_Trigger na začátek.
        // 3) Hand_Drawer: ruka za koncem grafu nakreslí hvězdu, kterou hráč pak sní.
        private static void SetupGraphAndHand()
        {
            Undo.IncrementCurrentGroup();
            int group = Undo.GetCurrentGroup();
            Undo.SetCurrentGroupName("Graf, ruka a otřes při dopadu");

            var log = new StringBuilder("[VarvaraSceneSetup] Graf, ruka a otřes:\n");
            List<GameObject> all = AllObjects();
            int ground = LayerMask.NameToLayer("Ground");
            Material lineMaterial = DefaultSpriteMaterial();

            // --- 1) Otřes jen při dopadu ---
            Camera cam = all.Select(g => g.GetComponent<Camera>()).FirstOrDefault(c => c != null);
            if (cam != null)
            {
                Undo.RegisterCompleteObjectUndo(cam.gameObject, "Odstranit občasné otřesy");
                int removed = GameObjectUtility.RemoveMonoBehavioursWithMissingScript(cam.gameObject);
                var landing = cam.GetComponent<PaperGuy.Cameras.PlayerLandingShake>();
                if (landing != null)
                {
                    Set(landing, "minImpactSpeed", 6f);         // třese se při každém skoku/pádu
                    Set(landing, "amplitudePerUnitSpeed", 0.012f);
                }
                log.AppendLine($"  ▪ kamera: jen otřes při dopadu (odstraněno chybějících skriptů: {removed})");
            }

            // --- 2) Graf ---
            GameObject graphGo = all.FirstOrDefault(g => g.GetComponent<FunctionGraphPath>() != null);
            Vector2 graphEnd = Vector2.zero;
            if (graphGo != null)
            {
                var graph = graphGo.GetComponent<FunctionGraphPath>();
                var line = GetOrAdd<LineRenderer>(graphGo);
                Undo.RecordObject(line, "Graf");
                line.widthMultiplier = 0.08f;
                line.startColor = line.endColor = new Color(0.12f, 0.15f, 0.35f);
                line.numCapVertices = 4;
                if (line.sharedMaterial == null) line.sharedMaterial = lineMaterial;
                var edge = GetOrAdd<EdgeCollider2D>(graphGo);
                SetLayer(graphGo, ground);

                Set(graph, "lineRenderer", line);
                Set(graph, "edgeCollider", edge);
                graph.Rebuild();

                Vector2 start = graph.GetPoint(0f);
                graphEnd = graph.GetPoint(graph.Length);

                Transform trigger = graphGo.transform.Find("Slide_Trigger");
                if (trigger == null)
                {
                    var t = new GameObject("Slide_Trigger") { layer = 2 /* Ignore Raycast */ };
                    Undo.RegisterCreatedObjectUndo(t, "Slide_Trigger");
                    t.transform.SetParent(graphGo.transform, false);
                    trigger = t.transform;
                }
                trigger.position = start + new Vector2(0.3f, 0.8f);
                var box = GetOrAdd<BoxCollider2D>(trigger.gameObject);
                Undo.RecordObject(box, "Slide_Trigger");
                box.isTrigger = true;
                box.size = new Vector2(0.8f, 1.6f);
                var slideTrigger = GetOrAdd<SlideTrigger>(trigger.gameObject);
                Set(slideTrigger, "pathSource", graph);
                Set(slideTrigger, "oneShot", false);

                log.AppendLine($"  ▪ {graphGo.name}: čára + collider, Slide_Trigger na začátku {start:F1}, konec {graphEnd:F1}");
            }
            else
            {
                log.AppendLine("  ⚠ Ve scéně není objekt s FunctionGraphPath — graf přeskočen.");
            }

            // --- 3) Ruka, která kreslí ---
            GameObject old = all.FirstOrDefault(g => g.name == "Hand_Drawer");
            if (old != null) Undo.DestroyObjectImmediate(old);

            Vector2 drawCenter = graphGo != null ? graphEnd + new Vector2(3f, 0f) : new Vector2(10f, -3.6f);
            drawCenter.y = FindFloorTop(drawCenter, all) + 1.0f; // tvar těsně nad zemí, aby na něj hráč došel

            var root = new GameObject("Hand_Drawer") { layer = 2 /* Ignore Raycast */ };
            Undo.RegisterCreatedObjectUndo(root, "Hand_Drawer");
            root.transform.position = drawCenter;
            var zone = root.AddComponent<BoxCollider2D>();
            zone.isTrigger = true;
            zone.size = new Vector2(10f, 8f); // ruka začne kreslit, jakmile hráč vstoupí do zóny
            var drawer = root.AddComponent<PaperGuy.Boss.HandDrawer>();

            var drawingGo = new GameObject("Drawn_Edible_Star");
            int collectible = LayerMask.NameToLayer("Collectible");
            if (collectible >= 0) drawingGo.layer = collectible;
            drawingGo.transform.SetParent(root.transform, false);
            var drawLine = drawingGo.AddComponent<LineRenderer>();
            drawLine.useWorldSpace = true;
            drawLine.positionCount = 0;
            drawLine.widthMultiplier = 0.07f;
            drawLine.startColor = drawLine.endColor = new Color(0.85f, 0.2f, 0.2f);
            drawLine.numCapVertices = 4;
            drawLine.numCornerVertices = 2;
            drawLine.sharedMaterial = lineMaterial;
            drawLine.sortingOrder = 5;
            var drawBox = drawingGo.AddComponent<BoxCollider2D>();
            drawBox.isTrigger = true;
            drawBox.size = new Vector2(1.6f, 1.5f);
            var edible = drawingGo.AddComponent<EdibleCollectible>();
            Set(edible, "kind", (int)EdibleCollectible.EdibleKind.Drawing);
            Set(edible, "description", "nakreslená hvězda");

            var handGo = new GameObject("Hand");
            handGo.transform.SetParent(root.transform, false);
            ApplyDetailedHand(handGo.transform, PaperArtGenerator.GenerateHandSprite(), lineMaterial);

            Set(drawer, "hand", handGo.transform);
            Set(drawer, "handOffset", Vector2.zero);
            Set(drawer, "offscreenOffset", new Vector2(5f, 10f));
            Set(drawer, "drawing", edible);
            Set(drawer, "line", drawLine);

            log.AppendLine($"  ▪ Hand_Drawer u {drawCenter:F1}: ruka nakreslí hvězdu, hráč ji pak sní");

            Undo.CollapseUndoOperations(group);
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            log.AppendLine("Scénu zkontrolujte a uložte (Ctrl+S). Vrátit jde přes Ctrl+Z.");
            Debug.Log(log.ToString());
        }

        private static List<GameObject> AllObjects() =>
            EditorSceneManager.GetActiveScene().GetRootGameObjects()
                .SelectMany(r => r.GetComponentsInChildren<Transform>(true))
                .Select(t => t.gameObject)
                .ToList();

        // Horní hrana nejbližší pevné plochy pod bodem (podle spritů — mají platné bounds i v editoru).
        private static float FindFloorTop(Vector2 point, List<GameObject> all)
        {
            float best = point.y - 1f;
            float bestDistance = float.MaxValue;
            foreach (GameObject g in all)
            {
                if (g.GetComponent<BoundaryWall>() == null && g.GetComponent<SolidPlatform>() == null) continue;
                if (!g.TryGetComponent(out SpriteRenderer sr)) continue;
                Bounds b = sr.bounds;
                if (point.x < b.min.x || point.x > b.max.x || b.max.y > point.y + 2f) continue;
                float d = point.y - b.max.y;
                if (d >= -2f && d < bestDistance)
                {
                    bestDistance = d;
                    best = b.max.y;
                }
            }
            return best;
        }

        // Ruka s tužkou jako jeden sprite; pivot je hrot tužky, nakloněná jako při psaní pravou rukou.
        private static void ApplyDetailedHand(Transform hand, Sprite sprite, Material material)
        {
            for (int i = hand.childCount - 1; i >= 0; i--)
                Undo.DestroyObjectImmediate(hand.GetChild(i).gameObject);

            Undo.RecordObject(hand, "Ruka");
            hand.localScale = Vector3.one;
            hand.localRotation = Quaternion.Euler(0f, 0f, -20f);

            var sr = GetOrAdd<SpriteRenderer>(hand.gameObject);
            Undo.RecordObject(sr, "Ruka");
            sr.sprite = sprite;
            sr.color = Color.white;
            sr.sortingOrder = 30;
            if (material != null) sr.sharedMaterial = material;
        }

        // =====================================================================
        //  Bílý sešit, detailní ruka, větší úroveň
        // =====================================================================

        [MenuItem("Paper Guy/Bílý sešit + detailní ruka (Varvara)", priority = 5)]
        public static void SheetAndHandFromMenu()
        {
            if (!EnsureVarvaraOpen()) return;
            SetupSheetAndHand();
        }

        [MenuItem("Paper Guy/Rozšířit úroveň Varvara (šířka ×2.5, výška ×1.7)", priority = 6)]
        public static void ExpandLevelFromMenu()
        {
            if (!EnsureVarvaraOpen()) return;
            ExpandLevel(2.5f, 1.7f);
            SetupSheetAndHand(); // pozadí a hranice kamery podle nové velikosti
        }

        [MenuItem("Paper Guy/Zúžit úroveň Varvara (šířka ×0.8)", priority = 7)]
        public static void NarrowLevelFromMenu()
        {
            if (!EnsureVarvaraOpen()) return;
            ExpandLevel(0.8f, 1f);
            SetupSheetAndHand();
        }

        private static bool EnsureVarvaraOpen()
        {
            if (EditorSceneManager.GetActiveScene().path == ScenePath) return true;
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return false;
            EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            return true;
        }

        private struct LevelWalls
        {
            public GameObject Left, Right, Floor, Ceiling;
            public bool Valid => Left != null && Right != null && Floor != null && Ceiling != null;
            public Bounds Outer => new Bounds(
                new Vector3((Left.GetComponent<SpriteRenderer>().bounds.min.x + Right.GetComponent<SpriteRenderer>().bounds.max.x) * 0.5f,
                    (Floor.GetComponent<SpriteRenderer>().bounds.min.y + Ceiling.GetComponent<SpriteRenderer>().bounds.max.y) * 0.5f),
                new Vector3(Right.GetComponent<SpriteRenderer>().bounds.max.x - Left.GetComponent<SpriteRenderer>().bounds.min.x,
                    Ceiling.GetComponent<SpriteRenderer>().bounds.max.y - Floor.GetComponent<SpriteRenderer>().bounds.min.y));
        }

        // Levá/pravá = nejkrajnější svislé stěny „Square…“, podlaha/strop = nejnižší/nejvyšší vodorovná.
        private static LevelWalls FindWalls(List<GameObject> all)
        {
            var walls = all.Where(g => g.GetComponent<BoundaryWall>() != null && g.GetComponent<SpriteRenderer>() != null).ToList();
            var tall = walls.Where(g => Size(g).y > Size(g).x).OrderBy(g => Center(g).x).ToList();
            var wide = walls.Where(g => Size(g).x >= Size(g).y).OrderBy(g => Center(g).y).ToList();
            return new LevelWalls
            {
                Left = tall.FirstOrDefault(), Right = tall.LastOrDefault(),
                Floor = wide.FirstOrDefault(), Ceiling = wide.LastOrDefault()
            };
        }

        private static Vector2 Size(GameObject g) => g.GetComponent<SpriteRenderer>().bounds.size;
        private static Vector2 Center(GameObject g) => g.GetComponent<SpriteRenderer>().bounds.center;

        // Pravou stěnu posune doprava a strop nahoru; stěny, podlahu a strop natáhne, aby na sebe navazovaly.
        private static void ExpandLevel(float widthFactor, float heightFactor)
        {
            Undo.IncrementCurrentGroup();
            int group = Undo.GetCurrentGroup();
            Undo.SetCurrentGroupName("Rozšířit úroveň Varvara");

            LevelWalls w = FindWalls(AllObjects());
            if (!w.Valid)
            {
                Debug.LogWarning("[VarvaraSceneSetup] Nenašel jsem 4 hranice „Square…“ (levá, pravá, podlaha, strop) — úroveň nerozšířena.");
                return;
            }

            Bounds left = w.Left.GetComponent<SpriteRenderer>().bounds;
            Bounds right = w.Right.GetComponent<SpriteRenderer>().bounds;
            Bounds floor = w.Floor.GetComponent<SpriteRenderer>().bounds;
            Bounds ceiling = w.Ceiling.GetComponent<SpriteRenderer>().bounds;

            float innerLeft = left.max.x, innerRight = right.min.x;
            float innerBottom = floor.max.y, innerTop = ceiling.min.y;
            float newInnerRight = innerLeft + (innerRight - innerLeft) * widthFactor;
            float newInnerTop = innerBottom + (innerTop - innerBottom) * heightFactor;

            float outerLeft = left.min.x;
            float outerRight = newInnerRight + right.size.x;
            float outerBottom = floor.min.y;
            float outerTop = newInnerTop + ceiling.size.y;

            // Pravá stěna: nová pozice X; obě svislé stěny přes celou výšku.
            Place(w.Right, new Vector2(newInnerRight + right.size.x * 0.5f, (outerBottom + outerTop) * 0.5f),
                new Vector2(right.size.x, outerTop - outerBottom));
            Place(w.Left, new Vector2(left.center.x, (outerBottom + outerTop) * 0.5f),
                new Vector2(left.size.x, outerTop - outerBottom));
            // Podlaha a strop přes celou šířku.
            Place(w.Floor, new Vector2((outerLeft + outerRight) * 0.5f, floor.center.y),
                new Vector2(outerRight - outerLeft, floor.size.y));
            Place(w.Ceiling, new Vector2((outerLeft + outerRight) * 0.5f, newInnerTop + ceiling.size.y * 0.5f),
                new Vector2(outerRight - outerLeft, ceiling.size.y));

            Undo.CollapseUndoOperations(group);
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            Debug.Log($"[VarvaraSceneSetup] Úroveň rozšířena: vnitřek x {innerLeft:F1}…{newInnerRight:F1}, " +
                      $"y {innerBottom:F1}…{newInnerTop:F1} (dříve {innerRight - innerLeft:F1} × {innerTop - innerBottom:F1}). " +
                      "Uložte scénu (Ctrl+S).");
        }

        // Nastaví střed a velikost spritu ve světě úpravou pozice a měřítka.
        private static void Place(GameObject go, Vector2 center, Vector2 worldSize)
        {
            var sr = go.GetComponent<SpriteRenderer>();
            Vector2 spriteSize = sr.sprite != null ? (Vector2)sr.sprite.bounds.size : Vector2.one;
            Transform t = go.transform;
            Vector3 parentScale = t.parent != null ? t.parent.lossyScale : Vector3.one;

            Undo.RecordObject(t, "Rozšířit úroveň");
            t.position = new Vector3(center.x, center.y, t.position.z);
            t.localScale = new Vector3(
                worldSize.x / (spriteSize.x * parentScale.x),
                worldSize.y / (spriteSize.y * parentScale.y),
                t.localScale.z);
        }

        // Čistě bílý list přes celou úroveň, bílé pozadí kamery,
        // hranice kamery podle stěn a detailní ruka s tužkou.
        private static void SetupSheetAndHand()
        {
            Undo.IncrementCurrentGroup();
            int group = Undo.GetCurrentGroup();
            Undo.SetCurrentGroupName("Bílý sešit + detailní ruka");

            var log = new StringBuilder("[VarvaraSceneSetup] Sešit a ruka:\n");
            List<GameObject> all = AllObjects();
            Material material = DefaultSpriteMaterial();
            LevelWalls walls = FindWalls(all);

            // Kamera: bílé pozadí.
            Camera cam = all.Select(g => g.GetComponent<Camera>()).FirstOrDefault(c => c != null);
            if (cam != null)
            {
                Undo.RecordObject(cam, "Bílé pozadí");
                cam.clearFlags = CameraClearFlags.SolidColor;
                cam.backgroundColor = Color.white;
            }

            if (walls.Valid)
            {
                Bounds outer = walls.Outer;

                // List sešitu.
                GameObject sheet = all.FirstOrDefault(g => g.name == "Background_NotebookSheet");
                if (sheet == null)
                {
                    sheet = new GameObject("Background_NotebookSheet");
                    Undo.RegisterCreatedObjectUndo(sheet, "Pozadí");
                }
                Undo.RecordObject(sheet.transform, "Pozadí");
                sheet.transform.position = new Vector3(outer.center.x, outer.center.y, 1f);
                var sheetRenderer = GetOrAdd<SpriteRenderer>(sheet);
                Undo.RecordObject(sheetRenderer, "Pozadí");
                sheetRenderer.sprite = PaperArtGenerator.GenerateWhitePixel();
                sheetRenderer.drawMode = SpriteDrawMode.Sliced;
                sheetRenderer.size = outer.size;
                sheetRenderer.color = Color.white;
                sheetRenderer.sortingOrder = -100;
                sheetRenderer.sharedMaterial = material;

                // List je čistě bílý — případnou starou linku okraje odstraníme.
                Transform margin = sheet.transform.Find("Margin_Line");
                if (margin != null) Undo.DestroyObjectImmediate(margin.gameObject);

                // Hranice kamery = celá úroveň.
                GameObject boundsGo = all.FirstOrDefault(g => g.name == "Camera_Bounds");
                if (boundsGo != null && boundsGo.TryGetComponent(out BoxCollider2D box))
                {
                    Undo.RecordObject(boundsGo.transform, "Hranice kamery");
                    Undo.RecordObject(box, "Hranice kamery");
                    boundsGo.transform.position = new Vector3(outer.center.x, outer.center.y, 0f);
                    box.offset = Vector2.zero;
                    box.size = outer.size;
                }
                log.AppendLine($"  ▪ čistě bílý list {outer.size.x:F0} × {outer.size.y:F0}, hranice kamery");
            }
            else
            {
                log.AppendLine("  ⚠ nenašel jsem 4 hranice „Square…“ — list sešitu nevytvořen, jen bílé pozadí kamery");
            }

            // Detailní ruka ve všech HandDrawer.
            Sprite handSprite = PaperArtGenerator.GenerateHandSprite();
            foreach (PaperGuy.Boss.HandDrawer drawer in all.Select(g => g.GetComponent<PaperGuy.Boss.HandDrawer>()).Where(d => d != null))
            {
                var so = new SerializedObject(drawer);
                var hand = so.FindProperty("hand").objectReferenceValue as Transform;
                if (hand == null) continue;
                ApplyDetailedHand(hand, handSprite, material);
                Set(drawer, "handOffset", Vector2.zero);
                Set(drawer, "offscreenOffset", new Vector2(5f, 10f));
                log.AppendLine($"  ▪ {drawer.name}: detailní ruka s tužkou (4 jednotky)");
            }

            Undo.CollapseUndoOperations(group);
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            log.AppendLine("Scénu zkontrolujte a uložte (Ctrl+S). Vrátit jde přes Ctrl+Z.");
            Debug.Log(log.ToString());
        }

        private static Material DefaultSpriteMaterial()
        {
            var temp = new GameObject("_temp") { hideFlags = HideFlags.HideAndDontSave };
            Material m = temp.AddComponent<SpriteRenderer>().sharedMaterial;
            UnityEngine.Object.DestroyImmediate(temp);
            return m;
        }

        [MenuItem("Paper Guy/Nastavit kameru ve scéně Varvara", priority = 3)]
        public static void SetupCameraFromMenu()
        {
            if (EditorSceneManager.GetActiveScene().path != ScenePath)
            {
                if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
                EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            }
            SetupCamera();
        }

        private static void SetupCamera()
        {
            Undo.IncrementCurrentGroup();
            int group = Undo.GetCurrentGroup();
            Undo.SetCurrentGroupName("Nastavit kameru ve scéně Varvara");

            var log = new StringBuilder("[VarvaraSceneSetup] Kamera:\n");
            var all = EditorSceneManager.GetActiveScene().GetRootGameObjects()
                .SelectMany(r => r.GetComponentsInChildren<Transform>(true))
                .Select(t => t.gameObject)
                .ToList();

            Camera cam = all.Select(g => g.GetComponent<Camera>()).FirstOrDefault(c => c != null && c.name == "Main Camera");
            if (cam == null) cam = all.Select(g => g.GetComponent<Camera>()).FirstOrDefault(c => c != null);
            PlayerController2D player = all.Select(g => g.GetComponent<PlayerController2D>()).FirstOrDefault(p => p != null);
            if (cam == null || player == null)
            {
                Debug.LogWarning("[VarvaraSceneSetup] Chybí kamera nebo hráč s PlayerController2D — kamera nenastavena.");
                return;
            }

            Undo.RecordObject(cam, "Kamera");
            cam.orthographic = true;

            var shake = GetOrAdd<PaperGuy.Cameras.CameraShake>(cam.gameObject);
            var controller = GetOrAdd<PaperGuy.Cameras.CameraController2D>(cam.gameObject);
            var landing = GetOrAdd<PaperGuy.Cameras.PlayerLandingShake>(cam.gameObject);

            Set(controller, "target", player.transform);
            Set(controller, "targetBody", player.GetComponent<Rigidbody2D>());
            Set(controller, "shake", shake);
            // Úroveň je i vlevo a nahoře/dole — kamera se smí vracet.
            Set(controller, "oneWay", false);
            Set(landing, "player", player);
            Set(landing, "shake", shake);

            // Hranice kamery = obal všech stěn „Square…“ (podle spritů, ty mají platné bounds i v editoru).
            var walls = all.Where(g => g.GetComponent<BoundaryWall>() != null && g.GetComponent<SpriteRenderer>() != null).ToList();
            if (walls.Count > 0)
            {
                Bounds b = walls[0].GetComponent<SpriteRenderer>().bounds;
                foreach (GameObject w in walls.Skip(1)) b.Encapsulate(w.GetComponent<SpriteRenderer>().bounds);

                GameObject boundsGo = all.FirstOrDefault(g => g.name == "Camera_Bounds");
                if (boundsGo == null)
                {
                    boundsGo = new GameObject("Camera_Bounds") { layer = 2 /* Ignore Raycast */ };
                    Undo.RegisterCreatedObjectUndo(boundsGo, "Camera_Bounds");
                }
                Undo.RecordObject(boundsGo.transform, "Camera_Bounds");
                boundsGo.transform.position = new Vector3(b.center.x, b.center.y, 0f);
                var box = GetOrAdd<BoxCollider2D>(boundsGo);
                Undo.RecordObject(box, "Camera_Bounds");
                box.isTrigger = true; // jen pro kameru, nikoho nezastaví
                box.size = b.size;
                Set(controller, "levelBounds", box);
                log.AppendLine($"  ▪ hranice kamery: střed {b.center:F1}, velikost {b.size:F1}");
            }
            else
            {
                log.AppendLine("  ▪ žádné stěny BoundaryWall — kamera bez hranic");
            }

            // Hráč: kamera je teď ve scéně, ale omezení „ne vlevo od kamery“ pro tuto úroveň nechceme.
            Set(player, "cameraController", controller);
            Set(player, "useBackLimit", false);

            Undo.CollapseUndoOperations(group);
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            log.AppendLine($"  ▪ {cam.name}: sleduje „{player.name}“, CameraShake + otřes při dopadu");
            log.AppendLine("Scénu zkontrolujte a uložte (Ctrl+S). Vrátit jde přes Ctrl+Z.");
            Debug.Log(log.ToString());
        }

        [MenuItem("Paper Guy/Doplnit skripty do scény Varvara", priority = 2)]
        public static void RunFromMenu()
        {
            if (EditorSceneManager.GetActiveScene().path != ScenePath)
            {
                if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
                EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            }
            Run();
        }

        private static void Run()
        {
            Undo.IncrementCurrentGroup();
            int group = Undo.GetCurrentGroup();
            Undo.SetCurrentGroupName("Doplnit skripty do scény Varvara");

            VarvaraLevelBuilder.SetupLayers(); // „Grourd“ → „Ground“, přidá vrstvu „Player“
            int ground = LayerMask.NameToLayer("Ground");
            int player = LayerMask.NameToLayer("Player");
            int collectible = LayerMask.NameToLayer("Collectible");

            var log = new StringBuilder("[VarvaraSceneSetup] Doplněno do scény Varvara:\n");
            var all = EditorSceneManager.GetActiveScene().GetRootGameObjects()
                .SelectMany(r => r.GetComponentsInChildren<Transform>(true))
                .Select(t => t.gameObject)
                .ToList();

            foreach (GameObject go in all)
            {
                string n = go.name;
                if (StartsWith(n, "Square"))
                {
                    EnsureCollider<BoxCollider2D>(go, trigger: false);
                    GetOrAdd<BoundaryWall>(go);
                    SetLayer(go, ground);
                    log.AppendLine($"  ▪ {n}: hranice (pevná, ve hře průhledná)");
                }
                else if (StartsWith(n, "text_platform") || StartsWith(n, "Drawing_platform"))
                {
                    EnsureCollider<BoxCollider2D>(go, trigger: false);
                    GetOrAdd<SolidPlatform>(go);
                    SetLayer(go, ground);
                    log.AppendLine($"  ▪ {n}: pevná plošina");
                }
                else if (StartsWith(n, "Edible"))
                {
                    EnsureCollider<BoxCollider2D>(go, trigger: true);
                    var edible = GetOrAdd<EdibleCollectible>(go);
                    bool isDrawing = n.IndexOf("draw", StringComparison.OrdinalIgnoreCase) >= 0;
                    Set(edible, "kind", isDrawing ? (int)EdibleCollectible.EdibleKind.Drawing : (int)EdibleCollectible.EdibleKind.Text);
                    if (collectible >= 0) SetLayer(go, collectible);
                    log.AppendLine($"  ▪ {n}: jedlý {(isDrawing ? "obrázek" : "text")}");
                }
            }

            GameObject playerGo = all.FirstOrDefault(g => g.TryGetComponent(out PlayerController2D _));
            if (playerGo != null)
            {
                SetupPlayer(playerGo, ground, player);
                log.AppendLine($"  ▪ {playerGo.name}: collider, GroundCheck, Ground Layers = Ground, vstup, PlayerAnimator");
            }
            else
            {
                log.AppendLine("  ⚠ Ve scéně není objekt s PlayerController2D.");
            }

            Undo.CollapseUndoOperations(group);
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            log.AppendLine("Scénu zkontrolujte a uložte (Ctrl+S). Vrátit jde přes Ctrl+Z.");
            Debug.Log(log.ToString());
        }

        private static void SetupPlayer(GameObject go, int groundLayer, int playerLayer)
        {
            if (playerLayer >= 0) SetLayer(go, playerLayer);

            var rb = go.GetComponent<Rigidbody2D>();
            Undo.RecordObject(rb, "Hráč");
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.freezeRotation = true;
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

            // Bez collideru hráč propadal vším — proto se „nehýbal“.
            Collider2D col = EnsureCollider<BoxCollider2D>(go, trigger: false);

            // GroundCheck u nohou (spodní hrana collideru).
            Transform groundCheck = go.transform.Find("GroundCheck");
            if (groundCheck == null)
            {
                var gc = new GameObject("GroundCheck");
                Undo.RegisterCreatedObjectUndo(gc, "GroundCheck");
                gc.transform.SetParent(go.transform, false);
                groundCheck = gc.transform;
            }
            groundCheck.localPosition = col is BoxCollider2D box
                ? new Vector3(box.offset.x, box.offset.y - box.size.y * 0.5f, 0f)
                : new Vector3(0f, -0.5f, 0f);

            var controller = go.GetComponent<PlayerController2D>();
            Set(controller, "groundCheck", groundCheck);
            Set(controller, "groundLayers", groundLayer >= 0 ? 1 << groundLayer : 1);

            var refs = AssetDatabase.LoadAllAssetsAtPath(InputActionsPath).OfType<InputActionReference>().ToArray();
            var input = GetOrAdd<PlayerInputReader>(go);
            InputActionReference move = refs.FirstOrDefault(r => r.name == "Player/Move");
            InputActionReference jump = refs.FirstOrDefault(r => r.name == "Player/Jump");
            if (move != null && jump != null)
            {
                Set(input, "moveAction", move);
                Set(input, "jumpAction", jump);
            }

            var animator = GetOrAdd<PlayerAnimator>(go);
            Set(animator, "player", controller);
            if (go.TryGetComponent(out SpriteRenderer sr)) Set(animator, "spriteRenderer", sr);
            if (go.TryGetComponent(out Animator anim)) Set(animator, "animator", anim);
        }

        // ---------------------------------------------------------------------

        private static bool StartsWith(string name, string prefix) =>
            name.StartsWith(prefix, StringComparison.OrdinalIgnoreCase);

        private static Collider2D EnsureCollider<T>(GameObject go, bool trigger) where T : Collider2D
        {
            if (!go.TryGetComponent(out Collider2D col)) col = Undo.AddComponent<T>(go);
            Undo.RecordObject(col, "Collider");
            col.isTrigger = trigger;
            return col;
        }

        private static T GetOrAdd<T>(GameObject go) where T : Component =>
            go.TryGetComponent(out T existing) ? existing : Undo.AddComponent<T>(go);

        private static void SetLayer(GameObject go, int layer)
        {
            if (layer < 0 || go.layer == layer) return;
            Undo.RecordObject(go, "Vrstva");
            go.layer = layer;
        }

        private static void Set(UnityEngine.Object target, string property, object value)
        {
            var so = new SerializedObject(target);
            SerializedProperty p = so.FindProperty(property);
            if (p == null)
            {
                Debug.LogError($"[VarvaraSceneSetup] {target.GetType().Name} nemá pole „{property}“.", target);
                return;
            }
            switch (p.propertyType)
            {
                case SerializedPropertyType.ObjectReference: p.objectReferenceValue = (UnityEngine.Object)value; break;
                case SerializedPropertyType.Enum: p.enumValueIndex = (int)value; break;
                case SerializedPropertyType.LayerMask: p.intValue = (int)value; break;
                case SerializedPropertyType.Boolean: p.boolValue = (bool)value; break;
                case SerializedPropertyType.Float: p.floatValue = (float)value; break;
                case SerializedPropertyType.String: p.stringValue = (string)value; break;
                case SerializedPropertyType.Vector2: p.vector2Value = (Vector2)value; break;
                default: Debug.LogError($"[VarvaraSceneSetup] Nepodporovaný typ {p.propertyType} ({property})."); return;
            }
            so.ApplyModifiedProperties(); // s Undo
        }
    }
}
