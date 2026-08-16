using UnityEngine;

/// <summary>
/// FoamCollisionReceiver: placed on the FireRoot trigger collider.
/// Receives particle collision events from the foam particle system
/// and calls FireManager.ApplyFoam().
/// </summary>
[RequireComponent(typeof(Collider))]
public class FoamCollisionReceiver : MonoBehaviour
{
    private FireManager fireManager;

    void Start()
    {
        fireManager = GetComponentInParent<FireManager>();
        if (fireManager == null)
            fireManager = Object.FindAnyObjectByType<FireManager>();

        var col = GetComponent<Collider>();
        col.isTrigger = true;
    }

    // Called by Unity's particle collision system
    void OnParticleCollision(GameObject other)
    {
        if (fireManager != null)
            fireManager.ApplyFoam();
    }

    // Also handle physical trigger overlap from foam particles (fallback)
    void OnTriggerStay(Collider other)
    {
        var extinguisher = other.GetComponentInParent<ExtinguisherController>();
        if (extinguisher != null && extinguisher.isSpraying)
        {
            if (fireManager != null)
                fireManager.ApplyFoam();
        }
    }
}
