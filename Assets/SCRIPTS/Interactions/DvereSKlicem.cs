using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class DvereSKlicem : MonoBehaviour
{
    public float fadeTime = 0.5f;
    [Tooltip("Jak blizko musi hrac byt, aby otevrel dvere klavesou F.")]
    public float dosah = 0.6f;
    public float napovedaOd = 3f;

    bool opening;
    Collider2D col;
    PlayerController2D player;
    Collider2D playerCol;
    NapovedaF napoveda;

    void Awake()
    {
        Klic.mam = false;
        col = GetComponent<Collider2D>();
        col.isTrigger = false;
        napoveda = NapovedaF.Vytvor(col.bounds.center);
    }

    // bez klice se neda projit, s klicem u dveri zmackni F
    void Update()
    {
        if (opening || !Klic.mam) return;
        if (player == null)
        {
            player = FindFirstObjectByType<PlayerController2D>();
            if (player == null) return;
            playerCol = player.GetComponent<Collider2D>();
        }
        float d = Physics2D.Distance(col, playerCol).distance;
        napoveda.gameObject.SetActive(d < napovedaOd);
        if (d > dosah || !Klic.StisklF()) return;

        opening = true;
        napoveda.gameObject.SetActive(false);
        Debug.Log("Dvere odemceny");
        StartCoroutine(Open());
    }

    IEnumerator Open()
    {
        col.enabled = false;
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        Color c = sr.color;
        for (float t = 0f; t < fadeTime; t += Time.deltaTime)
        {
            c.a = 1f - t / fadeTime;
            sr.color = c;
            yield return null;
        }
        gameObject.SetActive(false);
    }
}
