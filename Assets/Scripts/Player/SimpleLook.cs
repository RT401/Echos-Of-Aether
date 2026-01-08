using UnityEngine;
using UnityEngine.InputSystem;

public class SimpleLook : MonoBehaviour
{
    [Header("References")]
    public Transform playerBody;

    [Header("Sensitivity")]
    public float mouseSensitivity = 0.12f;      // tuned for mouse
    public float stickSensitivity = 120f;       // degrees/sec tuned for stick

    [Header("Clamp")]
    public float minPitch = -80f;
    public float maxPitch = 80f;

    private Vector2 lookInput;
    private float pitch;

    private void OnEnable()
    {
        // Optional: lock cursor for FPS-like look
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        if (!playerBody) return;

        // Decide scale based on device type
        // Mouse delta is already "per frame-ish", stick is "per second-ish"
        float dt = Time.deltaTime;

        bool usingMouse = Mouse.current != null && Mouse.current.delta.IsActuated();
        float sensitivity = usingMouse ? mouseSensitivity : stickSensitivity * dt;

        float yaw = lookInput.x * sensitivity;
        float pitchDelta = lookInput.y * sensitivity;

        pitch -= pitchDelta;
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

        // rotate player horizontally (yaw)
        playerBody.Rotate(Vector3.up * yaw);

        // rotate camera vertically (pitch)
        transform.localRotation = Quaternion.Euler(pitch, 0f, 0f);
    }

    // Input System callback
    public void OnLook(InputAction.CallbackContext ctx)
    {
        lookInput = ctx.ReadValue<Vector2>();
    }
}
