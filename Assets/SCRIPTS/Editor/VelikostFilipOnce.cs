using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

[InitializeOnLoad]
static class VelikostFilipOnce
{
    const string DoneKey = "PaperGuy.VelikostFilip.v1";
    const string ScenePath = "Assets/Scenes/Varvara.unity";
    const float CutX = 26f;
    const float Shift = 14.4f;
    const float Raise = 2.86f;
    const float Frame = 0.6f;

    static SceneRoots roots;

    class SceneRoots
    {
        public Transform texty, kresby, jidlo, mistnost, sipky, sheet, bounds;
    }

    static VelikostFilipOnce()
    {
        if (EditorPrefs.GetBool(DoneKey)) return;
        EditorApplication.update += Wait;
    }

    static void Wait()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling || EditorApplication.isUpdating) return;

        EditorApplication.update -= Wait;

        Scene scene = SceneManager.GetSceneByPath(ScenePath);
        bool opened = !scene.isLoaded;
        if (opened) scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Additive);

        roots = new SceneRoots();
        foreach (GameObject root in scene.GetRootGameObjects())
        {
            if (root.name == "Texty") roots.texty = root.transform;
            if (root.name == "Kresby") roots.kresby = root.transform;
            if (root.name == "Jidlo") roots.jidlo = root.transform;
            if (root.name == "Mistnost") roots.mistnost = root.transform;
            if (root.name == "Sipky") roots.sipky = root.transform;
            if (root.name == "Background_NotebookSheet") roots.sheet = root.transform;
            if (root.name == "Camera_Bounds") roots.bounds = root.transform;
        }

        MoveRight(roots.texty);
        MoveRight(roots.kresby);
        MoveRight(roots.jidlo);
        MoveRight(roots.mistnost);
        MoveRight(roots.sipky);

        Transform wall = roots.mistnost.Find("Stena_Prava");
        Undo.RecordObject(wall, "Velikost");
        wall.localScale += new Vector3(0f, Raise, 0f);
        wall.position += new Vector3(0f, Raise / 2f, 0f);

        Bridge("Schody1", 28.5f, 5.3f);
        Bridge("Pi", 33f, 6.3f);
        Bridge("Vypocitej", 37.5f, 5.6f);
        Bridge("Pi", 27f, 22.5f);
        Bridge("Schody1", 31.5f, 23.2f);
        Bridge("Vypocitej", 36f, 22.6f);

        foreach (GameObject root in scene.GetRootGameObjects())
            foreach (BoundaryWall b in root.GetComponentsInChildren<BoundaryWall>(true))
                Boundary(b.transform);

        Vector2 center = new Vector2(30.68f, 11.325f);
        Vector2 size = new Vector2(96.44f, 33.85f);

        Undo.RecordObject(roots.bounds, "Velikost");
        roots.bounds.position = new Vector3(center.x, center.y, roots.bounds.position.z);
        BoxCollider2D box = roots.bounds.GetComponent<BoxCollider2D>();
        Undo.RecordObject(box, "Velikost");
        box.size = size;

        Sheet(center, size);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        if (opened) EditorSceneManager.CloseScene(scene, true);

        EditorPrefs.SetBool(DoneKey, true);
        Debug.Log("Velikost urovne je hotova");
        AssetDatabase.DeleteAsset("Assets/SCRIPTS/Editor");
    }

    static void MoveRight(Transform group)
    {
        foreach (Transform t in group)
        {
            if (t.position.x < CutX && !IsFarChild(t)) continue;
            Undo.RecordObject(t, "Velikost");
            t.position += new Vector3(Shift, 0f, 0f);
        }
    }

    static bool IsFarChild(Transform t)
    {
        foreach (Transform c in t)
            if (c.position.x >= CutX) return true;
        return false;
    }

    static void Bridge(string source, float x, float y)
    {
        Transform from = roots.texty.Find("text_platform_" + source);
        GameObject copy = Object.Instantiate(from.gameObject, roots.texty);
        copy.name = "text_platform_Most_" + source + "_" + Mathf.RoundToInt(x);
        copy.transform.position = new Vector3(x, y, from.position.z);
        Undo.RegisterCreatedObjectUndo(copy, "Velikost");

        Transform food = roots.jidlo.Find(Mathf.RoundToInt(x) % 2 == 0 ? "Edible_3" : "Edible_4");
        GameObject bite = Object.Instantiate(food.gameObject, roots.jidlo);
        bite.name = "Edible_Most_" + Mathf.RoundToInt(x) + "_" + Mathf.RoundToInt(y);
        bite.transform.position = new Vector3(x, y + 1.4f, food.position.z);
        Undo.RegisterCreatedObjectUndo(bite, "Velikost");
    }

    static void Boundary(Transform t)
    {
        Undo.RecordObject(t, "Velikost");
        Vector3 s = t.localScale;
        Vector3 p = t.position;
        if (s.y > s.x)
        {
            s.y += Raise;
            p.y += Raise / 2f;
            if (p.x > 0f) p.x += Shift;
        }
        else
        {
            s.x += Shift;
            p.x += Shift / 2f;
            if (p.y > 0f) p.y += Raise;
        }
        t.localScale = s;
        t.position = p;
    }

    static void Sheet(Vector2 center, Vector2 size)
    {
        Transform sheet = roots.sheet;
        Vector2 sprite = sheet.GetComponent<SpriteRenderer>().sprite.bounds.size;
        Transform drawing = sheet.Find("Sesit_Kresba");
        Vector2 art = drawing.GetComponent<SpriteRenderer>().sprite.bounds.size;

        Undo.RecordObject(sheet, "Velikost");
        sheet.position = new Vector3(center.x, center.y, sheet.position.z);
        sheet.localScale = new Vector3(size.x / sprite.x, size.y / sprite.y, 1f);
        Vector3 p = sheet.localScale;

        Undo.RecordObject(drawing, "Velikost");
        drawing.localScale = new Vector3(size.x / art.x / p.x, size.y / art.y / p.y, 1f);

        Bar("Okraj_Nahore", new Vector2(0f, size.y / 2f + Frame / 2f), new Vector2(size.x + Frame * 2f, Frame), p);
        Bar("Okraj_Dole", new Vector2(0f, -size.y / 2f - Frame / 2f), new Vector2(size.x + Frame * 2f, Frame), p);
        Bar("Okraj_Vlevo", new Vector2(-size.x / 2f - Frame / 2f, 0f), new Vector2(Frame, size.y), p);
        Bar("Okraj_Vpravo", new Vector2(size.x / 2f + Frame / 2f, 0f), new Vector2(Frame, size.y), p);
    }

    static void Bar(string name, Vector2 pos, Vector2 size, Vector3 parent)
    {
        Transform bar = roots.sheet.Find(name);
        Vector2 pixel = bar.GetComponent<SpriteRenderer>().sprite.bounds.size;
        Undo.RecordObject(bar, "Velikost");
        bar.localPosition = new Vector3(pos.x / parent.x, pos.y / parent.y, 0f);
        bar.localRotation = Quaternion.identity;
        bar.localScale = new Vector3(size.x / pixel.x / parent.x, size.y / pixel.y / parent.y, 1f);
    }
}
