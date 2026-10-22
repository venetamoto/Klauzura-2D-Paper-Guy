using UnityEngine;

public class CameraShake : MonoBehaviour
{
    public float maxAmplitude = 0.6f;

    public Vector2 Offset { get; private set; }

    float strength;
    float duration;
    float timer;

    public void Shake(float amount, float time)
    {
        strength = Mathf.Min(amount, maxAmplitude);
        duration = time;
        timer = time;
    }

    void Update()
    {
        if (timer <= 0f)
        {
            Offset = Vector2.zero;
            return;
        }

        timer -= Time.deltaTime;
        Offset = Random.insideUnitCircle * strength * (timer / duration);
    }
}
