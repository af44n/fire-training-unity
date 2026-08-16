using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// SimpleVRMovement: Smooth WASD + Mouse look using the New Input System.
/// - Mouse delta is smoothed with an exponential moving average to eliminate jank.
/// - Sensitivity is normalised against screen DPI so it feels consistent at any resolution.
/// - Movement uses a velocity lerp for smooth acceleration/deceleration.
/// Controls: WASD/Arrows = move, Mouse = look, Escape = unlock cursor.
/// </summary>
public class SimpleVRMovement : MonoBehaviour
{
    // ───────────── Inspector ─────────────
    [Header("Look")]
    [Tooltip("Degrees per pixel of mouse movement (before smoothing).")]
    public float lookSensitivity = 0.12f;

    [Tooltip("Higher = snappier (less smooth). Range 5-25.")]
    public float lookSmoothing = 12f;

    [Header("Movement")]
    public float moveSpeed = 4f;

    [Tooltip("How quickly speed ramps up / down.")]
    public float moveSmoothing = 10f;

    [Header("References")]
    public Transform cameraTransform;

    // ───────────── Private state ─────────────
    private float yaw;
    private float pitch;

    // Auto-lock: cursor is unlocked until the first click in the Game view
    private bool hasLockedOnce = false;

    // Smoothed look deltas
    private Vector2 smoothedDelta;

    // Smoothed movement velocity
    private Vector3 smoothedVelocity;

    private CharacterController cc;
    private bool cursorLocked = true;

    // ───────────── Init ─────────────
    void Start()
    {
        // Character controller
        cc = GetComponent<CharacterController>();
        if (cc == null)
        {
            cc = gameObject.AddComponent<CharacterController>();
            cc.height = 1.8f;
            cc.center = new Vector3(0, 0.9f, 0);
            cc.radius = 0.3f;
            cc.stepOffset = 0.35f;
            cc.skinWidth = 0.02f;
        }

        // Camera reference
        if (cameraTransform == null)
            cameraTransform = Camera.main?.transform;
        if (cameraTransform == null)
        {
            var cam = GetComponentInChildren<Camera>();
            if (cam != null) cameraTransform = cam.transform;
        }

        // Seed yaw from current rotation so we don't snap on Start
        yaw   = transform.eulerAngles.y;
        pitch = cameraTransform != null
            ? cameraTransform.localEulerAngles.x
            : 0f;
        // Convert 270-360 range (Unity's negative pitch storage) to -90..0
        if (pitch > 180f) pitch -= 360f;

        // Don't force-lock yet — wait for the user to click the Game view
        // so Unity Editor grants focus first.
        LockCursor(false);
    }

    // Re-lock whenever the Game view regains focus after alt-tab etc.
    void OnApplicationFocus(bool hasFocus)
    {
        if (hasFocus && hasLockedOnce)
            LockCursor(true);
    }

    // ───────────── Per-frame ─────────────
    void Update()
    {
        var keyboard = Keyboard.current;
        var mouse    = Mouse.current;

        if (keyboard == null) return;

        // First left-click in Game view → lock cursor immediately
        if (!hasLockedOnce && mouse != null && mouse.leftButton.wasPressedThisFrame)
        {
            hasLockedOnce = true;
            LockCursor(true);
        }

        // Escape to unlock (click anywhere in Game view to re-lock)
        if (keyboard.escapeKey.wasPressedThisFrame)
            LockCursor(false);

        // Click to re-lock after Escape
        if (!cursorLocked && mouse != null && mouse.leftButton.wasPressedThisFrame)
            LockCursor(true);

        // ── Smooth mouse look ──
        if (cursorLocked && mouse != null)
        {
            // Raw delta from Input System is in pixels / update.
            // Dividing by deltaTime then multiplying back in keeps it
            // frame-rate independent before the smoothing step.
            Vector2 rawDelta = mouse.delta.ReadValue();

            // Clamp extreme spikes (e.g. cursor warp on window focus)
            rawDelta.x = Mathf.Clamp(rawDelta.x, -50f, 50f);
            rawDelta.y = Mathf.Clamp(rawDelta.y, -50f, 50f);

            // Exponential moving average — the key to silky smoothness
            float t = 1f - Mathf.Exp(-lookSmoothing * Time.deltaTime);
            smoothedDelta = Vector2.Lerp(smoothedDelta, rawDelta, t);

            yaw   += smoothedDelta.x * lookSensitivity;
            pitch -= smoothedDelta.y * lookSensitivity;
            pitch  = Mathf.Clamp(pitch, -80f, 80f);

            // Apply rotations — body yaw, camera pitch
            transform.rotation = Quaternion.Euler(0f, yaw, 0f);
            if (cameraTransform != null)
                cameraTransform.localRotation = Quaternion.Euler(pitch, 0f, 0f);
        }
        else
        {
            // Drain smoothed delta when not looking so there's no residual drift
            smoothedDelta = Vector2.zero;
        }

        // ── Smooth WASD movement ──
        float h = 0f, v = 0f;
        if (keyboard.dKey.isPressed     || keyboard.rightArrowKey.isPressed) h += 1f;
        if (keyboard.aKey.isPressed     || keyboard.leftArrowKey.isPressed)  h -= 1f;
        if (keyboard.wKey.isPressed     || keyboard.upArrowKey.isPressed)    v += 1f;
        if (keyboard.sKey.isPressed     || keyboard.downArrowKey.isPressed)  v -= 1f;

        Vector3 desiredMove = (transform.right * h + transform.forward * v);
        if (desiredMove.sqrMagnitude > 1f) desiredMove.Normalize();
        desiredMove *= moveSpeed;

        // Smoothly accelerate / decelerate
        smoothedVelocity = Vector3.Lerp(smoothedVelocity, desiredMove,
                               1f - Mathf.Exp(-moveSmoothing * Time.deltaTime));

        // Gravity
        Vector3 finalMove = smoothedVelocity;
        finalMove.y = cc != null && cc.isGrounded ? -0.5f : Physics.gravity.y * 0.5f;

        if (cc != null)
            cc.Move(finalMove * Time.deltaTime);
    }

    void LockCursor(bool locked)
    {
        cursorLocked          = locked;
        Cursor.lockState      = locked ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible        = !locked;
    }
}
