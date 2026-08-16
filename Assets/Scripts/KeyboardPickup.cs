using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// KeyboardPickup: Press E near the extinguisher to grab it.
/// It snaps to a natural "held in right hand" position in front of the camera.
/// G = spray, E = drop.
/// </summary>
public class KeyboardPickup : MonoBehaviour
{
    [Header("Grab Settings")]
    public float grabRange = 2.5f;

    [Header("Hold Pose (camera-local)")]
    [Tooltip("Position relative to camera. X=right, Y=up, Z=forward.")]
    public Vector3 holdOffset   = new Vector3(0.25f, -0.35f, 0.65f);

    [Tooltip("Euler rotation applied to the held object so it faces correctly.")]
    public Vector3 holdRotation = new Vector3(0f, 0f, 0f);

    [Tooltip("Scale multiplier applied while held (shrinks huge models).")]
    public float holdScale = 0.5f;

    [Tooltip("How fast the item lerps to the hold position.")]
    public float holdLerpSpeed = 20f;

    // ── private state ──
    private ExtinguisherController heldExtinguisher = null;
    private Transform              cameraTransform;
    private bool                   isHolding = false;
    private Vector3                originalScale;
    private TutorialManager        tutorialManager;

    void Start()
    {
        cameraTransform = Camera.main?.transform
                       ?? GetComponentInChildren<Camera>()?.transform;
        tutorialManager = Object.FindAnyObjectByType<TutorialManager>();
    }

    void Update()
    {
        var keyboard = Keyboard.current;
        if (keyboard == null) return;

        // E = pick up / drop
        if (keyboard.eKey.wasPressedThisFrame)
        {
            if (isHolding) Drop();
            else           TryPickup();
        }

        // G = spray while holding
        if (isHolding && heldExtinguisher != null)
        {
            if (keyboard.gKey.isPressed)
                heldExtinguisher.StartFoam();
            else
                heldExtinguisher.StopFoam();

            // Smoothly move extinguisher to the hold pose
            if (cameraTransform != null)
            {
                Vector3    targetPos = cameraTransform.TransformPoint(holdOffset);
                Quaternion targetRot = cameraTransform.rotation
                                     * Quaternion.Euler(holdRotation);

                float t = 1f - Mathf.Exp(-holdLerpSpeed * Time.deltaTime);
                heldExtinguisher.transform.position = Vector3.Lerp(
                    heldExtinguisher.transform.position, targetPos, t);
                heldExtinguisher.transform.rotation = Quaternion.Slerp(
                    heldExtinguisher.transform.rotation, targetRot, t);
            }
        }
    }

    void TryPickup()
    {
        if (cameraTransform == null) return;

        var extinguishers = Object.FindObjectsByType<ExtinguisherController>(FindObjectsSortMode.None);
        float closest = grabRange;
        ExtinguisherController target = null;

        foreach (var ext in extinguishers)
        {
            float d = Vector3.Distance(cameraTransform.position, ext.transform.position);
            if (d < closest) { closest = d; target = ext; }
        }

        if (target != null)
        {
            heldExtinguisher  = target;
            isHolding         = true;
            heldExtinguisher.isHeld = true;

            // Disable physics while held
            var rb = heldExtinguisher.GetComponent<Rigidbody>();
            if (rb != null) { rb.isKinematic = true; rb.useGravity = false; }

            // Scale down to a hand-held size
            originalScale = heldExtinguisher.transform.localScale;
            heldExtinguisher.transform.localScale = originalScale * holdScale;

            if (tutorialManager != null) tutorialManager.OnExtinguisherPickedUp();
            Debug.Log("[KeyboardPickup] Picked up extinguisher!");
        }
        else
        {
            Debug.Log($"[KeyboardPickup] Nothing within {grabRange}m. Get closer!");
        }
    }

    void Drop()
    {
        if (heldExtinguisher == null) return;

        heldExtinguisher.StopFoam();
        heldExtinguisher.isHeld = false;

        // Restore scale
        heldExtinguisher.transform.localScale = originalScale;

        // Re-enable physics
        var rb = heldExtinguisher.GetComponent<Rigidbody>();
        if (rb != null) { rb.isKinematic = false; rb.useGravity = true; }

        heldExtinguisher = null;
        isHolding        = false;
        Debug.Log("[KeyboardPickup] Dropped extinguisher.");
    }

    void OnGUI()
    {
        if (tutorialManager != null && tutorialManager.currentPhase == TutorialManager.TrainingPhase.Success)
            return;

        if (!isHolding && cameraTransform != null)
        {
            foreach (var ext in Object.FindObjectsByType<ExtinguisherController>(FindObjectsSortMode.None))
            {
                if (Vector3.Distance(cameraTransform.position, ext.transform.position) < grabRange)
                {
                    GUI.color = Color.yellow;
                    GUI.skin.label.fontSize  = 22;
                    GUI.skin.label.fontStyle = FontStyle.Bold;
                    GUI.Label(new Rect(Screen.width / 2 - 200, Screen.height / 2 + 20, 400, 40),
                        "[E]  Pick up Fire Extinguisher");
                    break;
                }
            }
        }
        else if (isHolding)
        {
            GUI.color = Color.white;
            GUI.skin.label.fontSize  = 18;
            GUI.skin.label.fontStyle = FontStyle.Normal;
            GUI.Label(new Rect(Screen.width / 2 - 150, Screen.height / 2 + 20, 300, 40),
                "[G] Spray   |   [E] Drop");
        }
    }
}
