using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class EdibleCollectible : MonoBehaviour
{
    public float waitForTongue = 0.35f;
    public float flyTime = 0.3f;

    bool eaten;

    void Awake()
    {
        GetComponent<Collider2D>().isTrigger = true;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (eaten || !enabled) return;

        PlayerController2D player = other.GetComponentInParent<PlayerController2D>();
        if (player == null) return;

        eaten = true;
        player.Eat();
        Debug.Log("Snedeno: " + name);
        StartCoroutine(FlyIntoMouth(player));
    }

    // jazyk vec chnapne a ona zaleti do tlamy na brisku
    IEnumerator FlyIntoMouth(PlayerController2D player)
    {
        yield return new WaitForSeconds(waitForTongue);

        Vector3 start = transform.position;
        Vector3 startScale = transform.localScale;
        for (float t = 0f; t < flyTime; t += Time.deltaTime)
        {
            Vector3 mouth = player.transform.position + new Vector3(0.3f * player.Facing, 0.33f, 0f);
            float k = t / flyTime;
            transform.position = Vector3.Lerp(start, mouth, k);
            transform.localScale = startScale * (1f - k);
            yield return null;
        }
        gameObject.SetActive(false);
    }
}
