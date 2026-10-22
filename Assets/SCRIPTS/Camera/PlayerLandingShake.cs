using UnityEngine;

public class PlayerLandingShake : MonoBehaviour
{
    public PlayerController2D player;
    public CameraShake shake;
    public float minImpactSpeed = 6f;
    public float amplitudePerUnitSpeed = 0.012f;
    public float duration = 0.25f;

    void OnEnable()
    {
        if (player != null) player.Landed += OnLanded;
    }

    void OnDisable()
    {
        if (player != null) player.Landed -= OnLanded;
    }

    void OnLanded(float speed)
    {
        if (shake != null && speed >= minImpactSpeed)
            shake.Shake(speed * amplitudePerUnitSpeed, duration);
    }
}
