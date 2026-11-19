using System.Collections.Generic;
using UnityEngine;

// Hriste ve startovnim menu: porad se objevuji nahodne cislice nakreslene tuzkou, hrdina je muze chytat a jist
public class MenuHriste : MonoBehaviour
{
    public GameObject jidlo;
    public int pocet = 3;
    public float kazdych = 2f;
    public Vector2 xRozsah = new Vector2(-11f, 11f);
    public Vector2 yRozsah = new Vector2(-3.6f, -2.6f);
    [Tooltip("Cislice 0-9 - jidlo dostane nahodnou z nich misto modreho ctverecku.")]
    public Sprite[] cifry;
    public float velikost = 1.3f;

    List<GameObject> ziva = new List<GameObject>();
    float cas;

    void Update()
    {
        for (int i = ziva.Count - 1; i >= 0; i--)
        {
            if (ziva[i] != null && ziva[i].activeSelf) continue;
            if (ziva[i] != null) Destroy(ziva[i]);
            ziva.RemoveAt(i);
        }

        cas += Time.deltaTime;
        if (ziva.Count >= pocet || cas < kazdych) return;
        cas = 0f;
        Vector3 kde = new Vector3(Random.Range(xRozsah.x, xRozsah.y), Random.Range(yRozsah.x, yRozsah.y), 0f);
        GameObject g = Instantiate(jidlo, kde, Quaternion.Euler(0f, 0f, Random.Range(-15f, 15f)));
        if (cifry != null && cifry.Length > 0)
        {
            SpriteRenderer sr = g.GetComponent<SpriteRenderer>();
            sr.sprite = cifry[Random.Range(0, cifry.Length)];
            sr.color = Color.white;
            float k = velikost / sr.sprite.bounds.size.y;
            g.transform.localScale = new Vector3(k, k, 1f);
            BoxCollider2D bc = g.GetComponent<BoxCollider2D>();
            if (bc != null) bc.size = sr.sprite.bounds.size * 0.8f;
        }
        ziva.Add(g);
    }
}
