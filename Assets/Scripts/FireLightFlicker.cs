using UnityEngine;

/// <summary>
/// FireLightFlicker: Realistic multi-frequency light flicker and dynamic shadow movement.
/// </summary>
public class FireLightFlicker : MonoBehaviour
{
    public Light fireLight;
    public float baseIntensity = 3.5f;
    public float flickerSpeed = 12f;
    public float intensityJitter = 1.2f;
    public float positionJitter = 0.05f;

    private Vector3 basePosition;

    void Start()
    {
        if (fireLight == null)
            fireLight = GetComponent<Light>();

        if (fireLight != null)
            basePosition = fireLight.transform.localPosition;
    }

    void Update()
    {
        if (fireLight == null) return;

        // Multi-layered noise for organic flickering
        float noise1 = Mathf.PerlinNoise(Time.time * flickerSpeed, 0.0f);
        float noise2 = Mathf.PerlinNoise(Time.time * (flickerSpeed * 2.3f), 10.0f);
        float combinedNoise = (noise1 + noise2 * 0.5f) / 1.5f;

        fireLight.intensity = Mathf.Max(0.5f, baseIntensity + (combinedNoise - 0.5f) * intensityJitter);

        // Subtle position shift for dynamic shadow movement
        float offsetX = (Mathf.PerlinNoise(Time.time * 5f, 0f) - 0.5f) * positionJitter;
        float offsetZ = (Mathf.PerlinNoise(0f, Time.time * 5f) - 0.5f) * positionJitter;
        fireLight.transform.localPosition = basePosition + new Vector3(offsetX, 0f, offsetZ);
    }
}
