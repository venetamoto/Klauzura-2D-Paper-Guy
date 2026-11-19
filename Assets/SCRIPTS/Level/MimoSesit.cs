using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MimoSesit : MonoBehaviour
{
    [Tooltip("Obdelnik sesitu (Camera_Bounds).")]
    public BoxCollider2D sesit;
    [Tooltip("Jak daleko za okrajem uz je hrac pryc.")]
    public float okraj = 0.5f;
    public float delay = 0.6f;

    [Header("Rozpad postavy (kresby autorky)")]
    public Sprite[] rozpad;
    public float rozpadTime = 1.2f;

    bool pryc;

    void Update()
    {
        if (pryc || sesit == null) return;

        Bounds b = sesit.bounds;
        Vector3 p = transform.position;
        if (p.x > b.min.x - okraj && p.x < b.max.x + okraj && p.y > b.min.y - okraj) return;

        // hrac odesel ze sesitu -> rozpadne se a level jede znovu
        pryc = true;
        Debug.Log("Hrac odesel ze sesitu, level znovu od zacatku");
        StartCoroutine(Rozpadnout(b));
    }

    IEnumerator Rozpadnout(Bounds b)
    {
        int order = 0;
        foreach (SpriteRenderer sr in GetComponentsInChildren<SpriteRenderer>())
        {
            order = Mathf.Max(order, sr.sortingOrder);
            sr.enabled = false;
        }
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null) rb.simulated = false;

        if (rozpad != null && rozpad.Length > 0)
        {
            // rozpad u kraje listu, aby byl videt
            Vector3 p = transform.position;
            Vector3 kde = new Vector3(Mathf.Clamp(p.x, b.min.x + 0.6f, b.max.x - 0.6f), Mathf.Max(p.y - 0.5f, b.min.y + 0.3f), 0f);
            SpriteRenderer r = new GameObject("Rozpad").AddComponent<SpriteRenderer>();
            r.transform.position = kde;
            r.sortingOrder = order;
            for (float t = 0f; t < rozpadTime; t += Time.deltaTime)
            {
                int i = Mathf.Min(rozpad.Length - 1, (int)(t / rozpadTime * rozpad.Length));
                r.sprite = rozpad[i];
                yield return null;
            }
            r.enabled = false;
        }

        yield return new WaitForSeconds(delay);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
