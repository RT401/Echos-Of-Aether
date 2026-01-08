using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerController3D : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 6f;
    public float acceleration = 20f;     // how fast to reach target speed
    public float airControl = 0.5f;      // 0..1 movement control in air
    
    [Header("Jump / Gravity")]
    public float jumpHeight = 1.2f;
    public float gravity = -20f;
    public float groundedStickForce = -2f; // keeps you grounded on slopes

    [Header("References (optional)")]
    public Transform cameraTransform; // if null, will use Camera.main

    private CharacterController controller;

    // Input state
    private Vector2 moveInput;
    private bool jumpPressed;

    // Movement state
    private Vector3 horizontalVelocity;
    private float verticalVelocity;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        if(cameraTransform == null && Camera.main != null)
        {
            cameraTransform = Camera.main.transform;
        }
    }

    private void Update()
    {
        HandleMovement(Time.deltaTime);
        jumpPressed = false; // consume jump (button press)
    }

    private void HandleMovement(float dt)
    {
        bool isGrounded = controller.isGrounded;

        if (isGrounded && verticalVelocity < 0f)
            verticalVelocity = groundedStickForce;

        // Camera-relative move direction
        Vector3 camForward = cameraTransform ? cameraTransform.forward : transform.forward;
        Vector3 camRight = cameraTransform ? cameraTransform.right : transform.right;

        camForward.y = 0f;
        camRight.y = 0f;
        camForward.Normalize();
        camRight.Normalize();

        Vector3 desiredDir = (camForward * moveInput.y + camRight * moveInput.x);
        desiredDir = Vector3.ClampMagnitude(desiredDir, 1f);

        float control = isGrounded ? 1f : airControl;
        Vector3 desiredVelocity = desiredDir * moveSpeed;

        // Smooth acceleration toward desired velocity (horizontal only)
        horizontalVelocity = Vector3.MoveTowards(
            horizontalVelocity,
            desiredVelocity,
            acceleration * control * dt
        );

        // Jump
        if (isGrounded && jumpPressed)
        {
            // v = sqrt(h * -2g)
            verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        // Gravity
        verticalVelocity += gravity * dt;

        // Final motion
        Vector3 velocity = horizontalVelocity + Vector3.up * verticalVelocity;
        controller.Move(velocity * dt);

        // Optional: rotate player toward movement direction (when moving)
        Vector3 flatMove = new Vector3(horizontalVelocity.x, 0f, horizontalVelocity.z);
        if (flatMove.sqrMagnitude > 0.001f)
        {
            Quaternion targetRot = Quaternion.LookRotation(flatMove, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, 12f * dt);
        }
    }

    // -------- Input System callbacks --------
    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }

    public void OnJump(InputValue value)
    {
        if (value.isPressed)
            jumpPressed = true;
    }
}
