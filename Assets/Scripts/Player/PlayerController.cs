using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float rotationSpeed = 10f;
    public float jumpHeight = 1.5f;
    public float gravity = -9.81f;

    [Header("Camera Settings")]
    public Transform cameraTransform;
    public float lookSensitivity = 2f;
    public float pitchClamp = 80f;

    private CharacterController controller;
    private Vector2 moveInput;
    private Vector2 lookInput;
    private Vector3 velocity;
    private bool isGrounded;
    private bool jumpPressed;
    private float cameraPitch = 0f;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        if (cameraTransform == null && Camera.main != null)
            cameraTransform = Camera.main.transform;
    }

    void Update()
    {
        isGrounded = controller.isGrounded;

        if (isGrounded && velocity.y < 0)
            velocity.y = -2f;

        HandleMovement();
        HandleRotation();

        if (jumpPressed && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            jumpPressed = false;
        }

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    void HandleMovement()
    {
        // Use camera-relative movement
        Vector3 foward = cameraTransform.foward;
        Vector3 right = cameraTransform.right;

        foward.y = 0;
        right.y = 0;
        foward.Normalize();
        right.Normalize();

        Vector3 move = (foward * moveInput.y + right * moveInput.x).Normalized;
        controller.Move(move * moveSpeed * Time.deltaTime);

        // Rotate character in movement direction
        if (move.magnitude > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(move);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }

    void HandleRotation()
    {
        if (cameraTransform == null) return;

        // Horizontal rotation (player)
        transform.Rotate(Vector3.up * lookInput.x * lookSensitivity * Time.deltaTime);

        // vertical rotation (camera)
        cameraPitch -= lookInput.y * lookSensitivity * Time.deltaTime;
        cameraPitch = Mathf.Clamp(cameraPitch, -pitchClamp, pitchClamp);
        cameraTransform.localEulerAngles = new Vector3(cameraPitch, 0f, 0f);
    }

    // Input system callbacks
    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    public void OnLook(InputAction.CallbackContext context)
    {
        lookInput = context.ReadValue<Vector2>();
    }
    
    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.performed)
            jumpPressed = true;
        if (context.canceled)
            jumpPressed = false;
    }
}
