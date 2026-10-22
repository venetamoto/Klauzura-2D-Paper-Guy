using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using PaperGuy.Animations;
using PaperGuy.Boss;
using PaperGuy.Cameras;
using PaperGuy.Interactions;
using PaperGuy.Level;
using PaperGuy.Player;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;

namespace PaperGuy.EditorTools
{
    public static class VarvaraLevelBuilder
    {
        private const string ScenePath = "Assets/Scenes/Varvara.unity";
        private const string SpriteFolder = "Assets/Art/Greybox";
        private const string InputActionsPath = "Assets/Scenes/Settings/InputSystem_Actions.inputactions";
        private const string ArchiveName = "_PUVODNI_ROZLOZENI (vypnuto)";
        private const string EquationGroupId = "1";

        private static readonly string[] GeneratedRoots =
        {
            "Background_NotebookSheet", "BOUNDARIES", "Player", "ENVIRONMENT",
            "GRAPH_SECTION", "COLLECTIBLES", "BOSS_EVENT", "LEVEL_LOGIC"
        };

        // ---------- Rozvržení úrovně (v jednotkách; 1 jednotka = 1 čtvereček sešitu) ----------
        private const float LevelMinX = -1f, LevelMaxX = 142f, LevelMinY = -10f, LevelMaxY = 12f;
        private const float FloorTop = -5f;

        // ---------- Barvy greyboxů ----------
        private static readonly Color PaperWhite = Color.white;
        private static readonly Color TextBlockColor = new Color(0.55f, 0.55f, 0.6f);
        private static readonly Color DrawingColor = new Color(0.35f, 0.5f, 0.75f);
        private static readonly Color TableColor = new Color(0.6f, 0.45f, 0.3f);
        private static readonly Color InkColor = new Color(0.12f, 0.15f, 0.35f);
        private static readonly Color PlayerColor = new Color(0.1f, 0.1f, 0.1f);
        private static readonly Color NumberColor = new Color(1f, 0.85f, 0.3f);
        private static readonly Color GateColor = new Color(0.8f, 0.3f, 0.3f);
        private static readonly Color EraserColor = new Color(1f, 0.55f, 0.65f);
        private static readonly Color HandColor = new Color(0.95f, 0.78f, 0.65f);

        private static Sprite _square, _triangle, _notebookCell;
        private static Material _spriteMaterial;
        private static Font _font;
        private static int _groundLayer, _playerLayer;

        // =====================================================================
        //  Vstupní body
        // =====================================================================

        [MenuItem("Paper Guy/Postavit úroveň Varvara", priority = 0)]
        public static void BuildFromMenu()
        {
            if (!EditorUtility.DisplayDialog("Postavit úroveň Varvara",
                    "Vygeneruje greybox úroveň ve scéně Varvara.\n\n" +
                    "Původní ručně rozmístěné objekty se přesunou do vypnutého kontejneru " +
                    $"„{ArchiveName}“ (nic se nesmaže). Krok lze vrátit přes Ctrl+Z.",
                    "Postavit", "Zrušit"))
                return;

            if (!OpenVarvaraScene()) return;
            Build();
            Debug.Log(Validate());
        }

        [MenuItem("Paper Guy/Ověřit propojení scény", priority = 1)]
        public static void ValidateFromMenu() => Debug.Log(Validate());

        public static void BuildFromCommandLine()
        {
            EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            Build();
            Debug.Log(Validate());
        }

        private static bool OpenVarvaraScene()
        {
            if (EditorSceneManager.GetActiveScene().path == ScenePath) return true;
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return false;
            EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            return true;
        }

        // =====================================================================
        //  Stavba
        // =====================================================================

        private static void Build()
        {
            Undo.IncrementCurrentGroup();
            int undoGroup = Undo.GetCurrentGroup();
            Undo.SetCurrentGroupName("Postavit úroveň Varvara");

            SetupLayers();
            LoadSharedAssets();
            RemovePreviouslyGenerated();
            ArchiveManualObjects();

            // --- Kamera ---
            Camera cam = SetupCamera(out CameraController2D camController, out CameraShake shake);

            // --- Pozadí (list ze sešitu) ---
            GameObject background = NewRoot("Background_NotebookSheet");
            background.transform.position = new Vector3((LevelMinX + LevelMaxX) * 0.5f, (LevelMinY + LevelMaxY) * 0.5f, 0f);
            var bgRenderer = background.AddComponent<SpriteRenderer>();
            bgRenderer.sprite = _notebookCell;
            bgRenderer.drawMode = SpriteDrawMode.Tiled;
            bgRenderer.size = new Vector2(LevelMaxX - LevelMinX, LevelMaxY - LevelMinY);
            bgRenderer.color = PaperWhite;
            bgRenderer.sortingOrder = -100;
            ApplySpriteMaterial(bgRenderer);

            // --- Hráč ---
            PlayerController2D player = BuildPlayer(camController);

            // --- Hranice úrovně + stěna kamery ---
            BoxCollider2D cameraBounds = BuildBoundaries(camController);

            // --- Prostředí ---
            GameObject environment = NewRoot("ENVIRONMENT");
            environment.layer = _groundLayer;
            Transform textPlatforms = NewChild("Text_Platforms", environment.transform, _groundLayer).transform;
            Transform drawingPlatforms = NewChild("Drawing_Platforms", environment.transform, _groundLayer).transform;
            Transform tablePlatforms = NewChild("Table_Platforms", environment.transform, _groundLayer).transform;

            // Startovní zóna: nadpis „Úkol“ jako první plošina.
            Block("Text_Platform_Ukol", textPlatforms, new Vector2(4f, 0f), new Vector2(9f, 1f), TextBlockColor, _groundLayer);
            Label("Label_Ukol", textPlatforms, "Úkol 1: Sjeď po grafu a vyřeš rovnici", new Vector2(4f, 2.5f), 0.12f, InkColor);

            // Linka sešitu — souvislá „podlaha“ pod celou úrovní od konce grafu.
            float floorStart = 13f;
            Block("Text_Platform_Linka", textPlatforms,
                new Vector2((floorStart + LevelMaxX) * 0.5f, FloorTop - 0.5f),
                new Vector2(LevelMaxX - floorStart, 1f), TextBlockColor, _groundLayer);

            // --- Sjezd po grafu ---
            FunctionGraphPath graph = BuildGraph();

            // --- Rovnice: brána + jedlá čísla ---
            BuildEquation(environment.transform, out _);

            // --- Plošinovka: tabulka a geometrické tvary ---
            BuildTable(tablePlatforms);
            BuildShapes(drawingPlatforms);

            // --- Finální zóna ---
            Label("Label_PraznaStranka", textPlatforms, "Prázdná stránka…", new Vector2(80f, 1f), 0.15f, InkColor);
            Label("Label_Konec", textPlatforms, "Konec", new Vector2(138f, -2f), 0.15f, InkColor);

            // --- Logika úrovně ---
            LevelManager levelManager = BuildLevelLogic(player);

            // --- Boss ---
            BuildBoss(levelManager, camController, player);

            // --- Propojení kamery ---
            Set(camController, "target", player.transform);
            Set(camController, "targetBody", player.GetComponent<Rigidbody2D>());
            Set(camController, "levelBounds", cameraBounds);
            Set(camController, "shake", shake);
            cam.transform.position = new Vector3(player.transform.position.x + 3f, player.transform.position.y, -10f);

            var landingShake = cam.GetComponent<PlayerLandingShake>();
            Set(landingShake, "player", player);
            Set(landingShake, "shake", shake);

            EnsureSceneInBuildSettings();

            Undo.CollapseUndoOperations(undoGroup);
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
            Selection.activeGameObject = player.gameObject;
            Debug.Log("[VarvaraLevelBuilder] Úroveň Varvara postavena a uložena.");
        }

        // =====================================================================
        //  Sekce úrovně
        // =====================================================================

        private static Camera SetupCamera(out CameraController2D controller, out CameraShake shake)
        {
            Camera cam = Object.FindObjectsByType<Camera>(FindObjectsInactive.Include)
                .FirstOrDefault(c => c.name == "Main Camera");
            if (cam == null) cam = Camera.main;

            if (cam == null)
            {
                var go = NewRoot("Main Camera");
                go.tag = "MainCamera";
                cam = go.AddComponent<Camera>();
            }

            Undo.RecordObject(cam, "Kamera");
            cam.orthographic = true;
            cam.orthographicSize = 5f;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = PaperWhite;

            shake = GetOrAdd<CameraShake>(cam.gameObject);
            controller = GetOrAdd<CameraController2D>(cam.gameObject);
            GetOrAdd<PlayerLandingShake>(cam.gameObject);
            return cam;
        }

        private static PlayerController2D BuildPlayer(CameraController2D camController)
        {
            GameObject go = NewRoot("Player");
            go.layer = _playerLayer;
            go.transform.position = new Vector3(1.5f, 1.2f, 0f);

            var rb = go.AddComponent<Rigidbody2D>();
            rb.freezeRotation = true;
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

            var box = go.AddComponent<BoxCollider2D>();
            box.size = new Vector2(0.8f, 1f);

            var input = go.AddComponent<PlayerInputReader>();
            var controller = go.AddComponent<PlayerController2D>();
            var playerAnimator = go.AddComponent<PlayerAnimator>();

            // Vizuál: samostatný potomek, aby se mohl naklánět při klouzání.
            GameObject visual = NewChild("Visual", go.transform, _playerLayer);
            var sr = visual.AddComponent<SpriteRenderer>();
            sr.sprite = _square;
            sr.drawMode = SpriteDrawMode.Sliced;
            sr.size = new Vector2(0.8f, 1f);
            sr.color = PlayerColor;
            sr.sortingOrder = 10;
            ApplySpriteMaterial(sr);
            var animator = visual.AddComponent<Animator>();

            GameObject groundCheck = NewChild("GroundCheck", go.transform, _playerLayer);
            groundCheck.transform.localPosition = new Vector3(0f, -0.5f, 0f);

            // Vstup z InputSystem_Actions (Player/Move, Player/Jump).
            var refs = AssetDatabase.LoadAllAssetsAtPath(InputActionsPath).OfType<InputActionReference>().ToArray();
            Set(input, "moveAction", refs.FirstOrDefault(r => r.name == "Player/Move"));
            Set(input, "jumpAction", refs.FirstOrDefault(r => r.name == "Player/Jump"));

            Set(controller, "groundCheck", groundCheck.transform);
            Set(controller, "groundLayers", 1 << _groundLayer);
            Set(controller, "cameraController", camController);
            Set(controller, "visualRoot", visual.transform);

            Set(playerAnimator, "player", controller);
            Set(playerAnimator, "animator", animator);
            Set(playerAnimator, "spriteRenderer", sr);

            return controller;
        }

        private static BoxCollider2D BuildBoundaries(CameraController2D camController)
        {
            GameObject root = NewRoot("BOUNDARIES");
            root.layer = _groundLayer;

            float cx = (LevelMinX + LevelMaxX) * 0.5f, cy = (LevelMinY + LevelMaxY) * 0.5f;
            float w = LevelMaxX - LevelMinX, h = LevelMaxY - LevelMinY;

            // Průhledné stěny: jen collider, žádný renderer.
            Wall("Wall_Left", root.transform, new Vector2(LevelMinX - 0.5f, cy), new Vector2(1f, h + 2f));
            Wall("Wall_Right", root.transform, new Vector2(LevelMaxX + 0.5f, cy), new Vector2(1f, h + 2f));
            Wall("Wall_Bottom", root.transform, new Vector2(cx, LevelMinY - 0.5f), new Vector2(w + 2f, 1f));
            Wall("Wall_Top", root.transform, new Vector2(cx, LevelMaxY + 0.5f), new Vector2(w + 2f, 1f));

            // Hranice pro kameru (confiner) — trigger, nikoho nezastaví.
            GameObject bounds = NewChild("Camera_Bounds", root.transform, 2 /* Ignore Raycast */);
            bounds.transform.position = new Vector3(cx, cy, 0f);
            var boundsBox = bounds.AddComponent<BoxCollider2D>();
            boundsBox.isTrigger = true;
            boundsBox.size = new Vector2(w, h);

            // Neviditelná stěna levého okraje kamery (tlačí hráče, nepouští ho zpět).
            GameObject leftWall = NewChild("Camera_LeftWall", root.transform, 0);
            var wallRb = leftWall.AddComponent<Rigidbody2D>();
            wallRb.bodyType = RigidbodyType2D.Kinematic;
            leftWall.AddComponent<BoxCollider2D>();
            var leftBoundary = leftWall.AddComponent<CameraLeftBoundary>();
            Set(leftBoundary, "cameraController", camController);
            Set(leftBoundary, "collideWith", 1 << _playerLayer);

            return boundsBox;
        }

        private static FunctionGraphPath BuildGraph()
        {
            GameObject section = NewRoot("GRAPH_SECTION");
            GameObject go = NewChild("Graph_Parabola", section.transform, _groundLayer);
            go.transform.position = new Vector3(8.5f, 0.5f, 0f); // navazuje na pravý okraj plošiny „Úkol“

            var line = go.AddComponent<LineRenderer>();
            line.useWorldSpace = false;
            line.widthMultiplier = 0.08f;
            line.startColor = line.endColor = InkColor;
            line.numCapVertices = 4;
            line.sharedMaterial = _spriteMaterial;
            line.sortingOrder = 1;

            var edge = go.AddComponent<EdgeCollider2D>();
            var graph = go.AddComponent<FunctionGraphPath>();
            Set(graph, "function", (int)FunctionGraphPath.GraphFunction.NegativeParabola);
            Set(graph, "xFrom", 0f);
            Set(graph, "xTo", 3f);
            Set(graph, "scale", new Vector2(2f, 0.6f));
            Set(graph, "lineRenderer", line);
            Set(graph, "edgeCollider", edge);
            graph.Rebuild();

            Label("Label_Graf", section.transform, "y = −x²", new Vector2(12.5f, -1f), 0.12f, InkColor);

            // Trigger na začátku grafu.
            GameObject trigger = NewChild("Slide_Trigger", section.transform, 0);
            trigger.transform.position = new Vector3(8.9f, 1.3f, 0f);
            var triggerBox = trigger.AddComponent<BoxCollider2D>();
            triggerBox.isTrigger = true;
            triggerBox.size = new Vector2(0.6f, 1.6f);
            var slideTrigger = trigger.AddComponent<SlideTrigger>();
            Set(slideTrigger, "pathSource", graph);

            return graph;
        }

        private static void BuildEquation(Transform environment, out EquationGate gate)
        {
            // Brána s rovnicí — v ENVIRONMENT, protože fyzicky blokuje cestu.
            GameObject gateGo = NewChild("Equation_Gate", environment, _groundLayer);
            gateGo.transform.position = new Vector3(30f, FloorTop + 3.5f, 0f);
            var gateRenderer = gateGo.AddComponent<SpriteRenderer>();
            gateRenderer.sprite = _square;
            gateRenderer.drawMode = SpriteDrawMode.Sliced;
            gateRenderer.size = new Vector2(1f, 7f);
            gateRenderer.color = GateColor;
            ApplySpriteMaterial(gateRenderer);
            var blocker = gateGo.AddComponent<BoxCollider2D>();
            blocker.size = new Vector2(1f, 7f);
            gate = gateGo.AddComponent<EquationGate>();
            Set(gate, "groupId", EquationGroupId);
            Set(gate, "blocker", blocker);

            // Po otevření brány schováme i její sprite.
            UnityEditor.Events.UnityEventTools.AddBoolPersistentListener(
                GetUnityEvent(gate, "onOpened"), gateRenderer.gameObject.SetActive, false);
            EditorUtility.SetDirty(gate);

            Label("Label_Rovnice", environment, "2 + 3 = ?", new Vector2(22f, 0.5f), 0.25f, InkColor);

            // Jedlá čísla.
            GameObject collectibles = NewRoot("COLLECTIBLES");
            NumberItem("Eat_Number_Correct", collectibles.transform, new Vector2(20f, -2.8f), 5, true);
            NumberItem("Eat_Number_Wrong", collectibles.transform, new Vector2(25f, -2.8f), 4, false);
        }

        private static void BuildTable(Transform parent)
        {
            Label("Label_Tabulka", parent, "Tabulka", new Vector2(40f, 2.5f), 0.12f, InkColor);
            float[] heights = { -3.5f, -2f, -0.5f, 1f };
            for (int i = 0; i < heights.Length; i++)
                Block($"Table_Step_{i + 1}", parent, new Vector2(35f + i * 3.5f, heights[i]), new Vector2(2.6f, 0.5f), TableColor, _groundLayer);
        }

        private static void BuildShapes(Transform parent)
        {
            Triangle("Drawing_Trojuhelnik_1", parent, new Vector2(51f, FloorTop + 1.5f), new Vector2(3f, 3f));
            Block("Drawing_Krychle_1", parent, new Vector2(55f, FloorTop + 1.5f), new Vector2(3f, 3f), DrawingColor, _groundLayer);
            Block("Drawing_Krychle_2", parent, new Vector2(59f, FloorTop + 2.5f), new Vector2(3f, 5f), DrawingColor, _groundLayer);
            Triangle("Drawing_Trojuhelnik_2", parent, new Vector2(63.5f, FloorTop + 2f), new Vector2(4f, 4f));
            Block("Drawing_Krychle_3", parent, new Vector2(68f, FloorTop + 1f), new Vector2(2f, 2f), DrawingColor, _groundLayer);
            Label("Label_Tvary", parent, "Trojúhelník a krychle", new Vector2(57f, 3f), 0.15f, InkColor);
        }

        private static LevelManager BuildLevelLogic(PlayerController2D player)
        {
            GameObject root = NewRoot("LEVEL_LOGIC");
            var manager = root.AddComponent<LevelManager>();
            Set(manager, "player", player);
            Set(manager, "startPhase", (int)LevelPhase.GraphSlide);

            Zone("Zone_EquationBarrier", root.transform, 16f, LevelPhase.EquationBarrier, manager);
            Zone("Zone_ShapePlatforming", root.transform, 32f, LevelPhase.ShapePlatforming, manager);
            Zone("Zone_FinalChase", root.transform, 74f, LevelPhase.FinalChase, manager);
            Zone("Zone_Complete", root.transform, 136f, LevelPhase.Complete, manager);
            return manager;
        }

        private static void BuildBoss(LevelManager levelManager, CameraController2D camController, PlayerController2D player)
        {
            GameObject root = NewRoot("BOSS_EVENT");
            GameObject go = NewChild("Eraser_Hand", root.transform, 0);
            var chase = go.AddComponent<HandChaseController>();

            // Ruka autora — zapne se během intra.
            GameObject hand = Block("Hand", go.transform, new Vector2(78f, 7f), new Vector2(4f, 2.5f), HandColor, 0, collider: false, order: 20);
            hand.SetActive(false);

            // Guma — jede u levého okraje kamery, zabíjí hráče.
            GameObject eraserGo = Block("Eraser", go.transform, new Vector2(74f, 0f), new Vector2(1.5f, 12f), EraserColor, 0, collider: true, order: 20);
            eraserGo.GetComponent<BoxCollider2D>().isTrigger = true;
            var eraser = eraserGo.AddComponent<EraserHazard>();
            eraserGo.SetActive(false);

            Set(chase, "levelManager", levelManager);
            Set(chase, "cameraController", camController);
            Set(chase, "player", player);
            Set(chase, "eraser", eraser);
            Set(chase, "hand", hand);
        }

        // =====================================================================
        //  Stavební kostky
        // =====================================================================

        private static GameObject Block(string name, Transform parent, Vector2 center, Vector2 size, Color color,
            int layer, bool collider = true, int order = 0)
        {
            GameObject go = NewChild(name, parent, layer);
            go.transform.position = center;
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = _square;
            sr.drawMode = SpriteDrawMode.Sliced;
            sr.size = size;
            sr.color = color;
            sr.sortingOrder = order;
            ApplySpriteMaterial(sr);
            if (collider) go.AddComponent<BoxCollider2D>().size = size;
            return go;
        }

        private static void Triangle(string name, Transform parent, Vector2 center, Vector2 size)
        {
            GameObject go = NewChild(name, parent, _groundLayer);
            go.transform.position = center;
            go.transform.localScale = new Vector3(size.x, size.y, 1f);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = _triangle;
            sr.color = DrawingColor;
            ApplySpriteMaterial(sr);
            var poly = go.AddComponent<PolygonCollider2D>();
            poly.pathCount = 1;
            poly.SetPath(0, new[] { new Vector2(-0.5f, -0.5f), new Vector2(0.5f, -0.5f), new Vector2(0f, 0.5f) });
        }

        private static void Wall(string name, Transform parent, Vector2 center, Vector2 size)
        {
            GameObject go = NewChild(name, parent, _groundLayer);
            go.transform.position = center;
            go.AddComponent<BoxCollider2D>().size = size;
        }

        private static void NumberItem(string name, Transform parent, Vector2 position, int value, bool correct)
        {
            GameObject go = Block(name, parent, position, new Vector2(0.8f, 0.8f), NumberColor, 0, collider: true, order: 5);
            go.GetComponent<BoxCollider2D>().isTrigger = true;
            var number = go.AddComponent<NumberCollectible>();
            Set(number, "groupId", EquationGroupId);
            Set(number, "value", value);
            Set(number, "isCorrectAnswer", correct);
            Label("Label", go.transform, value.ToString(), position, 0.12f, InkColor);
        }

        private static void Zone(string name, Transform parent, float x, LevelPhase phase, LevelManager manager)
        {
            GameObject go = NewChild(name, parent, 2 /* Ignore Raycast */);
            go.transform.position = new Vector3(x, (LevelMinY + LevelMaxY) * 0.5f, 0f);
            var box = go.AddComponent<BoxCollider2D>();
            box.isTrigger = true;
            box.size = new Vector2(1f, LevelMaxY - LevelMinY);
            var zone = go.AddComponent<LevelZoneTrigger>();
            Set(zone, "levelManager", manager);
            Set(zone, "phase", (int)phase);
        }

        private static void Label(string name, Transform parent, string text, Vector2 position, float charSize, Color color)
        {
            GameObject go = NewChild(name, parent, 0);
            go.transform.position = new Vector3(position.x, position.y, -0.1f);
            var mesh = go.AddComponent<TextMesh>();
            mesh.text = text;
            mesh.font = _font;
            mesh.fontSize = 64;
            mesh.characterSize = charSize;
            mesh.anchor = TextAnchor.MiddleCenter;
            mesh.alignment = TextAlignment.Center;
            mesh.color = color;
            var renderer = go.GetComponent<MeshRenderer>();
            renderer.sharedMaterial = _font.material;
            renderer.sortingOrder = 15;
        }

        private static GameObject NewRoot(string name)
        {
            var go = new GameObject(name);
            Undo.RegisterCreatedObjectUndo(go, "Vytvořit " + name);
            return go;
        }

        private static GameObject NewChild(string name, Transform parent, int layer)
        {
            var go = new GameObject(name) { layer = layer };
            go.transform.SetParent(parent, false);
            return go;
        }

        // Pozor: GetComponent v editoru vrací „falešný null“, operátor ?? by nefungoval.
        private static T GetOrAdd<T>(GameObject go) where T : Component =>
            go.TryGetComponent(out T existing) ? existing : Undo.AddComponent<T>(go);

        private static void ApplySpriteMaterial(Renderer renderer)
        {
            if (_spriteMaterial != null) renderer.sharedMaterial = _spriteMaterial;
        }

        // =====================================================================
        //  Úklid a příprava
        // =====================================================================

        private static void RemovePreviouslyGenerated()
        {
            foreach (GameObject root in EditorSceneManager.GetActiveScene().GetRootGameObjects())
                if (GeneratedRoots.Contains(root.name))
                    Undo.DestroyObjectImmediate(root);
        }

        private static void ArchiveManualObjects()
        {
            GameObject[] roots = EditorSceneManager.GetActiveScene().GetRootGameObjects();
            GameObject archive = roots.FirstOrDefault(r => r.name == ArchiveName);

            foreach (GameObject root in roots)
            {
                if (root == archive || root.name == "Main Camera") continue;
                if (root.GetComponent<Camera>() != null) continue;
                if (root.GetComponents<Component>().Any(c => c != null && c.GetType().Name == "Light2D")) continue;

                if (archive == null)
                {
                    archive = NewRoot(ArchiveName);
                    archive.SetActive(false);
                }
                Undo.SetTransformParent(root.transform, archive.transform, "Archivovat " + root.name);
            }
        }

        internal static void SetupLayers()
        {
            var tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            SerializedProperty layers = tagManager.FindProperty("layers");

            // Překlep „Grourd“ z ruční přípravy přejmenujeme na „Ground“ (index se nemění, objekty zůstanou).
            for (int i = 0; i < layers.arraySize; i++)
                if (layers.GetArrayElementAtIndex(i).stringValue == "Grourd")
                    layers.GetArrayElementAtIndex(i).stringValue = "Ground";

            EnsureLayer(layers, "Ground");
            EnsureLayer(layers, "Player");
            tagManager.ApplyModifiedPropertiesWithoutUndo();

            _groundLayer = LayerMask.NameToLayer("Ground");
            _playerLayer = LayerMask.NameToLayer("Player");
        }

        private static void EnsureLayer(SerializedProperty layers, string name)
        {
            for (int i = 0; i < layers.arraySize; i++)
                if (layers.GetArrayElementAtIndex(i).stringValue == name) return;

            for (int i = 6; i < layers.arraySize; i++)
            {
                SerializedProperty slot = layers.GetArrayElementAtIndex(i);
                if (string.IsNullOrEmpty(slot.stringValue))
                {
                    slot.stringValue = name;
                    return;
                }
            }
            Debug.LogError($"[VarvaraLevelBuilder] Není volná vrstva pro „{name}“.");
        }

        private static void EnsureSceneInBuildSettings()
        {
            List<EditorBuildSettingsScene> scenes = EditorBuildSettings.scenes.ToList();
            if (scenes.Any(s => s.path == ScenePath)) return;
            scenes.Add(new EditorBuildSettingsScene(ScenePath, true));
            EditorBuildSettings.scenes = scenes.ToArray();
        }

        // =====================================================================
        //  Assety (sprity greyboxů)
        // =====================================================================

        private static void LoadSharedAssets()
        {
            Directory.CreateDirectory(SpriteFolder);

            _square = EnsureSprite("Greybox_Square", 32, (x, y, s) => Color.white, SpriteMeshType.FullRect, TextureWrapMode.Clamp);
            _triangle = EnsureSprite("Greybox_Triangle", 64, (x, y, s) =>
            {
                // Rovnoramenný trojúhelník: základna dole, vrchol nahoře uprostřed.
                float halfWidth = (1f - (y + 0.5f) / s) * 0.5f;
                float dx = Mathf.Abs((x + 0.5f) / s - 0.5f);
                return dx <= halfWidth ? Color.white : Color.clear;
            }, SpriteMeshType.Tight, TextureWrapMode.Clamp);
            _notebookCell = EnsureSprite("Greybox_NotebookCell", 32, (x, y, s) =>
                x == 0 || y == s - 1 ? new Color(0.72f, 0.82f, 0.95f) : Color.white,
                SpriteMeshType.FullRect, TextureWrapMode.Repeat);

            // Výchozí sprite materiál aktuální render pipeline (URP 2D).
            var temp = new GameObject("_temp") { hideFlags = HideFlags.HideAndDontSave };
            _spriteMaterial = temp.AddComponent<SpriteRenderer>().sharedMaterial;
            Object.DestroyImmediate(temp);

            _font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        }

        private static Sprite EnsureSprite(string name, int size, System.Func<int, int, int, Color> pixel,
            SpriteMeshType meshType, TextureWrapMode wrap)
        {
            string path = $"{SpriteFolder}/{name}.png";
            if (!File.Exists(path))
            {
                var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
                for (int y = 0; y < size; y++)
                    for (int x = 0; x < size; x++)
                        tex.SetPixel(x, y, pixel(x, y, size));
                File.WriteAllBytes(path, tex.EncodeToPNG());
                Object.DestroyImmediate(tex);
                AssetDatabase.ImportAsset(path);

                var importer = (TextureImporter)AssetImporter.GetAtPath(path);
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.spritePixelsPerUnit = size;
                importer.filterMode = FilterMode.Point;
                importer.wrapMode = wrap;
                importer.alphaIsTransparency = true;
                importer.mipmapEnabled = false;
                var settings = new TextureImporterSettings();
                importer.ReadTextureSettings(settings);
                settings.spriteMeshType = meshType;
                importer.SetTextureSettings(settings);
                importer.SaveAndReimport();
            }
            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }

        // =====================================================================
        //  Nastavení privátních [SerializeField] polí
        // =====================================================================

        private static void Set(Object target, string property, object value)
        {
            var so = new SerializedObject(target);
            SerializedProperty p = so.FindProperty(property);
            if (p == null)
            {
                Debug.LogError($"[VarvaraLevelBuilder] {target.GetType().Name} nemá pole „{property}“.", target);
                return;
            }

            switch (p.propertyType)
            {
                case SerializedPropertyType.ObjectReference: p.objectReferenceValue = (Object)value; break;
                case SerializedPropertyType.Float: p.floatValue = (float)value; break;
                case SerializedPropertyType.Integer: p.intValue = (int)value; break;
                case SerializedPropertyType.Boolean: p.boolValue = (bool)value; break;
                case SerializedPropertyType.String: p.stringValue = (string)value; break;
                case SerializedPropertyType.Enum: p.enumValueIndex = (int)value; break;
                case SerializedPropertyType.LayerMask: p.intValue = (int)value; break;
                case SerializedPropertyType.Vector2: p.vector2Value = (Vector2)value; break;
                default:
                    Debug.LogError($"[VarvaraLevelBuilder] Nepodporovaný typ pole {p.propertyType} ({property}).", target);
                    return;
            }
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static UnityEngine.Events.UnityEvent GetUnityEvent(Object target, string field) =>
            (UnityEngine.Events.UnityEvent)target.GetType()
                .GetField(field, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
                .GetValue(target);

        // =====================================================================
        //  Ověření
        // =====================================================================

        public static string Validate()
        {
            var problems = new List<string>();
            var report = new StringBuilder("[VarvaraLevelBuilder] Ověření propojení scény:\n");

            // Nepovinné reference — prázdné jsou v pořádku.
            var optional = new HashSet<string> { "AnimationEventRelay.events" };

            MonoBehaviour[] scripts = Object.FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include)
                .Where(m => m != null && m.GetType().Namespace != null && m.GetType().Namespace.StartsWith("PaperGuy"))
                .Where(m => m.transform.root.name != ArchiveName) // archiv původních objektů se neověřuje
                .OrderBy(m => GetPath(m.transform))
                .ToArray();

            foreach (MonoBehaviour script in scripts)
            {
                var so = new SerializedObject(script);
                SerializedProperty it = so.GetIterator();
                int refs = 0;
                while (it.NextVisible(true))
                {
                    if (it.propertyType != SerializedPropertyType.ObjectReference || it.name == "m_Script") continue;
                    refs++;
                    if (it.objectReferenceValue == null && !optional.Contains($"{script.GetType().Name}.{it.name}"))
                        problems.Add($"{GetPath(script.transform)} → {script.GetType().Name}.{it.name} je prázdné");
                }
                report.AppendLine($"  ✓ {GetPath(script.transform)} — {script.GetType().Name} ({refs} referencí)");
            }

            string[] required = { "Main Camera", "Background_NotebookSheet", "BOUNDARIES", "Player", "Player/GroundCheck",
                "ENVIRONMENT/Text_Platforms", "ENVIRONMENT/Drawing_Platforms", "ENVIRONMENT/Table_Platforms",
                "GRAPH_SECTION/Graph_Parabola", "COLLECTIBLES/Eat_Number_Correct", "COLLECTIBLES/Eat_Number_Wrong",
                "BOSS_EVENT/Eraser_Hand" };
            foreach (string path in required)
                if (FindByPath(path) == null) problems.Add($"Chybí objekt „{path}“");

            if (LayerMask.NameToLayer("Ground") < 0) problems.Add("Chybí vrstva Ground");
            if (LayerMask.NameToLayer("Player") < 0) problems.Add("Chybí vrstva Player");

            report.AppendLine(problems.Count == 0
                ? "VÝSLEDEK: vše propojeno, žádné prázdné reference."
                : "PROBLÉMY:\n  - " + string.Join("\n  - ", problems));
            return report.ToString();
        }

        private static GameObject FindByPath(string path)
        {
            string[] parts = path.Split('/');
            GameObject root = EditorSceneManager.GetActiveScene().GetRootGameObjects().FirstOrDefault(r => r.name == parts[0]);
            if (root == null || parts.Length == 1) return root;
            Transform child = root.transform.Find(string.Join("/", parts.Skip(1)));
            return child != null ? child.gameObject : null;
        }

        private static string GetPath(Transform t) => t.parent == null ? t.name : GetPath(t.parent) + "/" + t.name;
    }
}
