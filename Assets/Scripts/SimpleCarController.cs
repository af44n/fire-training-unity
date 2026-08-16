using UnityEngine;
using UnityEngine.InputSystem;

public class SimpleCarController : MonoBehaviour
{
    [Header("Engine & Speed")]
    public float topSpeed = 35f;
    public float acceleration = 15f;
    public float deceleration = 10f;
    public float brakePower = 25f;

    [Header("Steering & Handling")]
    public float turnSpeed = 90f;        // Degrees per second
    public float gripFactor = 10f;       // Higher means less sideways sliding
    public float downForce = 2f;         // Keeps the car glued to the ground

    private Rigidbody rb;
    private float throttleInput;
    private float steeringInput;
    private bool isBraking;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        
        // A low center of mass drastically prevents flipping over!
        rb.centerOfMass = new Vector3(0, -0.6f, 0);

        // Add standard drag to prevent infinite sliding
        rb.linearDamping = 0.5f;
        rb.angularDamping = 2f; // High angular drag stops the wild spinning

        // Remove physics friction so the script fully controls momentum and grip
        PhysicsMaterial frictionless = new PhysicsMaterial("Frictionless")
        {
            dynamicFriction = 0f,
            staticFriction = 0f,
            frictionCombine = PhysicsMaterialCombine.Minimum
        };
        foreach (Collider col in GetComponentsInChildren<Collider>())
        {
            col.material = frictionless;
        }
    }

    void Update()
    {
        throttleInput = 0f;
        steeringInput = 0f;
        isBraking = false;

        if (Keyboard.current != null)
        {
            // W/S for Throttle
            if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed) throttleInput = 1f;
            else if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed) throttleInput = -1f;

            // A/D for Steering
            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) steeringInput = -1f;
            else if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) steeringInput = 1f;

            // Spacebar for Brake
            if (Keyboard.current.spaceKey.isPressed) isBraking = true;
        }
    }

    void FixedUpdate()
    {
        // 1. Forward Speed Calculation
        float currentSpeed = Vector3.Dot(rb.linearVelocity, transform.forward);

        // 2. Acceleration / Braking
        float targetSpeed = throttleInput * topSpeed;
        float speedDiff = targetSpeed - currentSpeed;

        float accelRate = isBraking ? brakePower : (Mathf.Abs(throttleInput) > 0.1f ? acceleration : deceleration);
        
        // Only apply forward force if we need to change speed
        if (Mathf.Abs(speedDiff) > 0.1f)
        {
            // ForceMode.Acceleration ignores mass, giving consistent results
            rb.AddForce(transform.forward * Mathf.Sign(speedDiff) * accelRate, ForceMode.Acceleration);
        }

        // 3. Downforce (keeps car from flying when hitting bumps)
        rb.AddForce(-transform.up * Mathf.Abs(currentSpeed) * downForce, ForceMode.Acceleration);

        // 4. Steering (Using MoveRotation is much more stable than AddTorque)
        float minSpeedForSteer = 1f;
        if (Mathf.Abs(currentSpeed) > minSpeedForSteer)
        {
            // Reverse steering if driving backwards
            float dir = Mathf.Sign(currentSpeed);
            
            // Reduce steering sensitivity slightly at top speeds for realism
            float speedRatio = Mathf.Clamp01(Mathf.Abs(currentSpeed) / topSpeed);
            float steerMultiplier = Mathf.Lerp(1f, 0.6f, speedRatio);

            float turn = steeringInput * turnSpeed * steerMultiplier * dir * Time.fixedDeltaTime;
            
            Quaternion turnRotation = Quaternion.Euler(0f, turn, 0f);
            rb.MoveRotation(rb.rotation * turnRotation);
        }

        // 5. Tire Grip (Cancel out sliding sideways like real tires)
        Vector3 right = transform.right;
        float sidewaysSpeed = Vector3.Dot(rb.linearVelocity, right);
        
        // Apply an opposing force to kill sideways momentum
        rb.AddForce(-right * sidewaysSpeed * gripFactor, ForceMode.Acceleration);
    }
}