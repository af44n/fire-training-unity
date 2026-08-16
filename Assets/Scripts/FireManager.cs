using UnityEngine;

/// <summary>
/// FireManager controls the fire simulation health, visual scaling,
/// light flicker, regeneration, and extinguishing logic.
/// Attach this to the FireRoot GameObject.
/// </summary>
public class FireManager : MonoBehaviour
{
    [Header("Fire Health")]
    public float maxHealth = 100f;
    public float currentHealth = 100f;
    public float regenRate = 5f;          // Health per second when not being extinguished
    public float damagePerSecond = 30f;   // Health reduced per second when foam hits

    [Header("Fire Start Delay")]
    public float startDelay = 5f;
    private bool fireActive = false;
    private float startTimer = 0f;

    [Header("Visual References")]
    public ParticleSystem flameParticles;
    public ParticleSystem smokeParticles;
    public ParticleSystem sparkParticles;
    public Light fireLight;
    public float maxLightIntensity = 3.5f;

    [Header("State")]
    public bool isBeingExtinguished = false;
    private bool isExtinguished = false;

    // Reference to the TutorialManager
    private TutorialManager tutorialManager;

    // Emission rates at full health
    private float flameMaxEmission;
    private float smokeMaxEmission;
    private float sparkMaxEmission;

    void Start()
    {
        tutorialManager = Object.FindAnyObjectByType<TutorialManager>();

        // Cache default emission rates
        if (flameParticles != null)
        {
            var emission = flameParticles.emission;
            flameMaxEmission = emission.rateOverTime.constant;
            flameParticles.Stop();
        }
        if (smokeParticles != null)
        {
            var emission = smokeParticles.emission;
            smokeMaxEmission = emission.rateOverTime.constant;
            smokeParticles.Stop();
        }
        if (sparkParticles != null)
        {
            var emission = sparkParticles.emission;
            sparkMaxEmission = emission.rateOverTime.constant;
            sparkParticles.Stop();
        }
        if (fireLight != null)
            fireLight.enabled = false;
    }

    void Update()
    {
        if (isExtinguished) return;

        // Countdown to fire start
        if (!fireActive)
        {
            startTimer += Time.deltaTime;
            if (startTimer >= startDelay)
            {
                ActivateFire();
            }
            return;
        }

        // Health logic
        if (isBeingExtinguished)
        {
            currentHealth -= damagePerSecond * Time.deltaTime;
            currentHealth = Mathf.Max(currentHealth, 0f);
        }
        else
        {
            // Regen
            currentHealth += regenRate * Time.deltaTime;
            currentHealth = Mathf.Min(currentHealth, maxHealth);
        }

        // Reset flag each frame - ExtinguisherController sets it
        isBeingExtinguished = false;

        float ratio = currentHealth / maxHealth;
        UpdateVisuals(ratio);

        if (currentHealth <= 0f)
        {
            ExtinguishFire();
        }
    }

    public void ActivateFire()
    {
        fireActive = true;
        if (flameParticles != null) flameParticles.Play();
        if (smokeParticles != null) smokeParticles.Play();
        if (sparkParticles != null) sparkParticles.Play();
        if (fireLight != null) fireLight.enabled = true;

        // Notify tutorial
        if (tutorialManager != null)
            tutorialManager.OnFireStarted();

        Debug.Log("[FireManager] Fire activated!");
    }

    void UpdateVisuals(float ratio)
    {
        // Scale flame emission
        if (flameParticles != null)
        {
            var emission = flameParticles.emission;
            var rate = emission.rateOverTime;
            rate.constant = flameMaxEmission * ratio;
            emission.rateOverTime = rate;

            var main = flameParticles.main;
            main.startSizeMultiplier = Mathf.Lerp(0.1f, 1f, ratio);
        }

        // Scale smoke emission
        if (smokeParticles != null)
        {
            var emission = smokeParticles.emission;
            var rate = emission.rateOverTime;
            rate.constant = smokeMaxEmission * ratio;
            emission.rateOverTime = rate;
        }

        // Scale spark emission
        if (sparkParticles != null)
        {
            var emission = sparkParticles.emission;
            var rate = emission.rateOverTime;
            rate.constant = sparkMaxEmission * ratio;
            emission.rateOverTime = rate;
        }

        // Scale light intensity
        if (fireLight != null)
        {
            var flicker = fireLight.GetComponent<FireLightFlicker>();
            if (flicker != null)
                flicker.baseIntensity = maxLightIntensity * ratio;
            else
                fireLight.intensity = maxLightIntensity * ratio;
        }
    }

    void ExtinguishFire()
    {
        isExtinguished = true;
        if (flameParticles != null) flameParticles.Stop();
        if (smokeParticles != null) smokeParticles.Stop();
        if (sparkParticles != null) sparkParticles.Stop();
        if (fireLight != null) fireLight.enabled = false;

        if (tutorialManager != null)
            tutorialManager.OnFireExtinguished();

        Debug.Log("[FireManager] Fire extinguished! Training complete.");
    }

    /// <summary>
    /// Called by the foam collision trigger to apply extinguisher damage.
    /// </summary>
    public void ApplyFoam()
    {
        if (!isExtinguished && fireActive)
            isBeingExtinguished = true;
    }

    public bool IsActive() => fireActive && !isExtinguished;
}
