using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

/// <summary>
/// ExtinguisherController handles foam emission and fire damage when trigger is pressed.
/// Attach to the fire extinguisher GameObject.
/// </summary>
public class ExtinguisherController : MonoBehaviour
{
    [Header("Foam Particle System")]
    public ParticleSystem foamParticles;

    [Header("Fire Detection")]
    public float foamRange = 4f;
    public LayerMask fireLayers;

    [Header("State")]
    public bool isHeld = false;
    public bool isSpraying = false;

    private XRGrabInteractable grabInteractable;
    private TutorialManager tutorialManager;

    void Awake()
    {
        grabInteractable = GetComponent<XRGrabInteractable>();
        if (grabInteractable != null)
        {
            grabInteractable.selectEntered.AddListener(OnGrabbed);
            grabInteractable.selectExited.AddListener(OnReleased);
            grabInteractable.activated.AddListener(OnActivated);
            grabInteractable.deactivated.AddListener(OnDeactivated);
        }

        if (foamParticles != null)
            foamParticles.Stop();
    }

    void Start()
    {
        tutorialManager = Object.FindAnyObjectByType<TutorialManager>();
    }

    void OnDestroy()
    {
        if (grabInteractable != null)
        {
            grabInteractable.selectEntered.RemoveListener(OnGrabbed);
            grabInteractable.selectExited.RemoveListener(OnReleased);
            grabInteractable.activated.RemoveListener(OnActivated);
            grabInteractable.deactivated.RemoveListener(OnDeactivated);
        }
    }

    void OnGrabbed(SelectEnterEventArgs args)
    {
        isHeld = true;
        Debug.Log("[Extinguisher] Picked up!");
        if (tutorialManager != null)
            tutorialManager.OnExtinguisherPickedUp();
    }

    void OnReleased(SelectExitEventArgs args)
    {
        isHeld = false;
        StopFoam();
    }

    void OnActivated(ActivateEventArgs args)
    {
        if (isHeld) StartFoam();
    }

    void OnDeactivated(DeactivateEventArgs args)
    {
        StopFoam();
    }

    void Update()
    {
        // G-key spray is handled by KeyboardPickup.cs to avoid old Input API conflicts.
        // XR trigger spray is handled via OnActivated / OnDeactivated callbacks above.

        // Raycast-based foam damage while spraying
        if (isSpraying && foamParticles != null)
        {
            Vector3 origin    = foamParticles.transform.position;
            Vector3 direction = foamParticles.transform.forward;
            int     mask      = fireLayers != 0 ? (int)fireLayers : Physics.DefaultRaycastLayers;

            RaycastHit[] hits = Physics.RaycastAll(origin, direction, foamRange, mask, QueryTriggerInteraction.Collide);
            foreach (var hit in hits)
            {
                var fire = hit.collider.GetComponentInParent<FireManager>();
                if (fire != null)
                    fire.ApplyFoam();
            }
        }
    }

    public void StartFoam()
    {
        if (!isSpraying)
        {
            isSpraying = true;
            if (foamParticles != null && !foamParticles.isPlaying)
                foamParticles.Play();
            Debug.Log("[Extinguisher] Spraying foam!");
        }
    }

    public void StopFoam()
    {
        if (isSpraying)
        {
            isSpraying = false;
            if (foamParticles != null)
                foamParticles.Stop();
        }
    }
}
