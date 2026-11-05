using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Collider2D))]
public class Klic : MonoBehaviour
{
    public static bool mam;

    public LineRenderer outline;
    public float flyTime = 0.4f;
    [Tooltip("Jak blizko musi hrac byt, aby mohl klic vzit klavesou F.")]
    public float dosah = 1f;
    [Tooltip("Odkud uz je videt napoveda \"Stiskni F\".")]
    public float napovedaOd = 3f;

    bool taken;
    Collider2D col;
    PlayerController2D player;
    Collider2D playerCol;
    NapovedaF napoveda;

    void Awake()
    {
        col = GetComponent<Collider2D>();
        col.isTrigger = true;
        napoveda = NapovedaF.Vytvor(new Vector3(col.bounds.center.x, col.bounds.max.y + 0.9f, 0f));
    }

    // hrac stoji u klice a zmackne F
    void Update()
    {
        if (taken) return;
        if (player == null)
        {
            player = FindFirstObjectByType<PlayerController2D>();
            if (player == null) return;
            playerCol = player.GetComponent<Collider2D>();
        }
        float d = Physics2D.Distance(col, playerCol).distance;
        napoveda.gameObject.SetActive(d < napovedaOd);
        if (d > dosah || !StisklF()) return;

        taken = true;
        napoveda.gameObject.SetActive(false);
        mam = true;
        Debug.Log("Klic sebran");
        StartCoroutine(FlyToPlayer(player.transform));
    }

    public static bool StisklF()
    {
        if (Keyboard.current != null && Keyboard.current.fKey.wasPressedThisFrame) return true;
        return Gamepad.current != null && Gamepad.current.buttonWest.wasPressedThisFrame;
    }

    // nakresleny klic doleti k postave a zmizi
    IEnumerator FlyToPlayer(Transform player)
    {
        Vector3 start = transform.position;
        Vector3 startScale = transform.localScale;
        Vector3[] drawn = new Vector3[outline != null ? outline.positionCount : 0];
        if (outline != null) outline.GetPositions(drawn);

        for (float t = 0f; t < flyTime; t += Time.deltaTime)
        {
            float k = t / flyTime;
            transform.position = Vector3.Lerp(start, player.position + Vector3.up, k);
            transform.localScale = startScale * (1f - k);
            for (int i = 0; i < drawn.Length; i++)
                outline.SetPosition(i, transform.position + (drawn[i] - start) * (1f - k));
            yield return null;
        }
        if (outline != null) outline.positionCount = 0;
        gameObject.SetActive(false);
    }
}
