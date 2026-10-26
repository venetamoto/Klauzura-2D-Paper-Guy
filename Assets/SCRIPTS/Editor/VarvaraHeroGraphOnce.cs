using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

// Jednorazovy skript: vetsi hrdina, celni pohled (hlava zvlast), jidlo ve 3 snimcich
// a graf jako v sesite z matiky (mrizka, osy se sipkami, modra krivka). Po dobehnuti se sam smaze.
[InitializeOnLoad]
public static class VarvaraHeroGraphOnce
{
    const string ScenePath = "Assets/Scenes/Varvara.unity";
    const string HeroFolder = "Assets/Art/Hrdina";
    const string SelfFolder = "Assets/SCRIPTS/Editor";
    static readonly string DoneKey = "PaperGuy.VarvaraHeroGraphOnce.v1." + Application.dataPath;

    const float HeroHeight = 1.9f;
    const float FrontPpu = 220f;

    // ceka, az Unity neni ve hre (Play) a je otevrena scena Varvara
    static VarvaraHeroGraphOnce()
    {
        if (Application.isBatchMode) return;
        EditorApplication.update += TryRun;
    }

    static void TryRun()
    {
        if (EditorPrefs.GetBool(DoneKey, false))
        {
            EditorApplication.update -= TryRun;
            DeleteSelf();
            return;
        }
        if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling || EditorApplication.isUpdating) return;
        if (EditorSceneManager.GetActiveScene().path != ScenePath) return;

        EditorApplication.update -= TryRun;
        EditorPrefs.SetBool(DoneKey, true);
        SetupImport();

        Undo.IncrementCurrentGroup();
        int group = Undo.GetCurrentGroup();
        Undo.SetCurrentGroupName("Hrdina + graf");
        SetupHero();
        SetupGraph();
        Undo.CollapseUndoOperations(group);

        // ulozit scenu, at se zmeny neztrati
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
        Debug.Log("Hotovo a ulozeno: vetsi hrdina, celni pohled s mrkanim, jidlo, novy graf.");
        DeleteSelf();
    }

    static void DeleteSelf()
    {
        if (AssetDatabase.IsValidFolder(SelfFolder))
            AssetDatabase.DeleteAsset(SelfFolder);
    }

    static void SetupImport()
    {
        foreach (string guid in AssetDatabase.FindAssets("t:Texture2D", new[] { HeroFolder }))
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            var imp = (TextureImporter)AssetImporter.GetAtPath(path);
            var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            bool front = path.Contains("Hlava") || path.Contains("Telo");

            imp.textureType = TextureImporterType.Sprite;
            imp.spriteImportMode = SpriteImportMode.Single;
            imp.spritePixelsPerUnit = front ? FrontPpu : tex.height / HeroHeight;
            imp.filterMode = FilterMode.Bilinear;
            imp.mipmapEnabled = false;
            imp.alphaIsTransparency = true;
            imp.textureCompression = TextureImporterCompression.Uncompressed;

            var s = new TextureImporterSettings();
            imp.ReadTextureSettings(s);
            s.spriteMeshType = SpriteMeshType.FullRect;
            s.spriteAlignment = (int)SpriteAlignment.Custom;
            s.spritePivot = front ? new Vector2(0.5f, 0f) : new Vector2(0.5f, 0.5f / HeroHeight);
            imp.SetTextureSettings(s);
            imp.SaveAndReimport();
        }
    }

    static Sprite Art(string name) => AssetDatabase.LoadAssetAtPath<Sprite>($"{HeroFolder}/{name}.png");
    static Sprite[] Arts(string prefix, int count) => Enumerable.Range(0, count).Select(i => Art($"{prefix}_{i}")).ToArray();

    static void SetupHero()
    {
        var player = Object.FindObjectsByType<PlayerController2D>(FindObjectsInactive.Include).FirstOrDefault();
        if (player == null) return;
        GameObject go = player.gameObject;

        var box = go.GetComponent<BoxCollider2D>();
        if (box != null)
        {
            Undo.RecordObject(box, "Hrdina");
            box.size = new Vector2(0.8f, 1.75f);
            box.offset = new Vector2(0f, 0.375f);
        }

        var anim = go.GetComponent<PlayerAnimator>();
        if (anim == null) return;

        Transform old = go.transform.Find("Zepredu");
        if (old != null) Undo.DestroyObjectImmediate(old.gameObject);

        var front = new GameObject("Zepredu");
        Undo.RegisterCreatedObjectUndo(front, "Zepredu");
        front.transform.SetParent(go.transform, false);

        var body = new GameObject("Telo");
        body.transform.SetParent(front.transform, false);
        body.transform.localPosition = new Vector3(0f, -0.5f, 0f);
        var bodySr = body.AddComponent<SpriteRenderer>();
        bodySr.sprite = Art("Hrdina_Telo");
        bodySr.sortingOrder = 11;

        float bodyHeight = bodySr.sprite.rect.height / FrontPpu;
        var head = new GameObject("Hlava");
        head.transform.SetParent(front.transform, false);
        head.transform.localPosition = new Vector3(0f, -0.5f + bodyHeight - 4f / FrontPpu, 0f);
        var headSr = head.AddComponent<SpriteRenderer>();
        headSr.sprite = Art("Hrdina_Hlava_0");
        headSr.sortingOrder = 10;
        front.SetActive(false);

        Undo.RecordObject(anim, "Hrdina");
        anim.walkFrames = Arts("Hrdina_Chuze", 4);
        anim.runFrames = Arts("Hrdina_Skok", 4);
        anim.eatFrames = Arts("Hrdina_Ji", 3);
        anim.tumbleFrame = Art("Hrdina_Kotrmelec");
        anim.headFrames = Arts("Hrdina_Hlava", 2);
        anim.front = front;
        anim.frontHead = headSr;
        anim.frontBody = bodySr;
        EditorUtility.SetDirty(anim);

        if (anim.spriteRenderer != null)
        {
            Undo.RecordObject(anim.spriteRenderer, "Hrdina");
            anim.spriteRenderer.sprite = anim.walkFrames[0];
            anim.spriteRenderer.sortingOrder = 10;
        }
    }

    // graf jako v sesite: ctvereckova mrizka, osy se sipkami a dilky, modra krivka, popisky
    static void SetupGraph()
    {
        var path = Object.FindObjectsByType<FunctionGraphPath>(FindObjectsInactive.Include).FirstOrDefault();
        if (path == null) return;

        Material mat = null;
        var anyLine = Object.FindObjectsByType<LineRenderer>(FindObjectsInactive.Include).FirstOrDefault(l => l.sharedMaterial != null);
        if (anyLine != null) mat = anyLine.sharedMaterial;

        Vector2 o = path.transform.position;
        float width = (path.xTo - path.xFrom) * path.scale.x;
        float drop = (path.xTo * path.xTo - path.xFrom * path.xFrom) * path.scale.y;
        float axisY = o.y - drop;
        float right = o.x + width + 1f;
        float top = o.y + 1.5f;

        if (path.lineRenderer != null)
        {
            var lr = path.lineRenderer;
            Undo.RecordObject(lr, "Graf");
            lr.startColor = lr.endColor = new Color(0.08f, 0.3f, 0.85f);
            lr.widthMultiplier = 0.11f;
            lr.numCapVertices = 4;
            lr.numCornerVertices = 2;
            lr.sortingOrder = 3;
            if (mat != null) lr.sharedMaterial = mat;
        }

        Transform oldArt = path.transform.Find("Graf_Kresba");
        if (oldArt != null) Undo.DestroyObjectImmediate(oldArt.gameObject);
        var art = new GameObject("Graf_Kresba");
        Undo.RegisterCreatedObjectUndo(art, "Graf");
        art.transform.SetParent(path.transform, false);

        var grid = new Color(0.62f, 0.78f, 0.95f, 0.7f);
        var ink = new Color(0.12f, 0.12f, 0.15f);

        for (float x = o.x; x <= right + 0.01f; x += 1f)
            Line(art.transform, "Mrizka", mat, grid, 0.025f, -5, new Vector2(x, axisY), new Vector2(x, top));
        for (float y = axisY; y <= top + 0.01f; y += 1f)
            Line(art.transform, "Mrizka", mat, grid, 0.025f, -5, new Vector2(o.x, y), new Vector2(right, y));

        Line(art.transform, "Osa_x", mat, ink, 0.06f, 2, new Vector2(o.x - 0.3f, axisY), new Vector2(right + 0.4f, axisY));
        Line(art.transform, "Sipka_x", mat, ink, 0.06f, 2, new Vector2(right + 0.1f, axisY + 0.2f), new Vector2(right + 0.4f, axisY), new Vector2(right + 0.1f, axisY - 0.2f));
        Line(art.transform, "Osa_y", mat, ink, 0.06f, 2, new Vector2(o.x, axisY - 0.3f), new Vector2(o.x, top + 0.4f));
        Line(art.transform, "Sipka_y", mat, ink, 0.06f, 2, new Vector2(o.x - 0.2f, top + 0.1f), new Vector2(o.x, top + 0.4f), new Vector2(o.x + 0.2f, top + 0.1f));

        for (float x = o.x + 1f; x <= right - 0.5f; x += 1f)
            Line(art.transform, "Dilek", mat, ink, 0.04f, 2, new Vector2(x, axisY - 0.12f), new Vector2(x, axisY + 0.12f));
        for (float y = axisY + 1f; y <= top - 0.5f; y += 1f)
            Line(art.transform, "Dilek", mat, ink, 0.04f, 2, new Vector2(o.x - 0.12f, y), new Vector2(o.x + 0.12f, y));

        Label(art.transform, "x", new Vector2(right + 0.3f, axisY - 0.45f), ink, 0.12f);
        Label(art.transform, "y", new Vector2(o.x - 0.45f, top + 0.3f), ink, 0.12f);
        Label(art.transform, "0", new Vector2(o.x - 0.35f, axisY - 0.4f), ink, 0.1f);
        Label(art.transform, "y = −x²", new Vector2(o.x + 3.6f, o.y + 0.7f), new Color(0.08f, 0.3f, 0.85f), 0.14f);

        // stare sede obdelniky os schovat (collider osy x zustava)
        foreach (string n in new[] { "Osa_X", "Osa_Y" })
        {
            GameObject g = GameObject.Find(n);
            if (g != null && g.TryGetComponent(out SpriteRenderer sr))
            {
                Undo.RecordObject(sr, "Graf");
                sr.enabled = false;
            }
        }
    }

    static void Line(Transform parent, string name, Material mat, Color color, float width, int order, params Vector2[] points)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var lr = go.AddComponent<LineRenderer>();
        lr.useWorldSpace = true;
        lr.positionCount = points.Length;
        for (int i = 0; i < points.Length; i++) lr.SetPosition(i, points[i]);
        lr.widthMultiplier = width;
        lr.startColor = lr.endColor = color;
        lr.numCapVertices = 2;
        lr.sortingOrder = order;
        if (mat != null) lr.sharedMaterial = mat;
    }

    static void Label(Transform parent, string text, Vector2 pos, Color color, float size)
    {
        var go = new GameObject("Popisek " + text);
        go.transform.SetParent(parent, false);
        go.transform.position = new Vector3(pos.x, pos.y, 0f);
        var tm = go.AddComponent<TextMesh>();
        Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        tm.font = font;
        tm.text = text;
        tm.fontSize = 64;
        tm.characterSize = size;
        tm.anchor = TextAnchor.MiddleCenter;
        tm.color = color;
        var mr = go.GetComponent<MeshRenderer>();
        mr.sharedMaterial = font.material;
        mr.sortingOrder = 4;
    }
}
