using UnityEngine;
using UnityEngine.InputSystem;

public class FlyCamera : MonoBehaviour
{
    [Header("Movement")]
    public float MoveSpeed = 10f;
    public float FastMoveMultiplier = 3f;
    public float VerticalSpeed = 10f;
    public float ScrollSpeedStep = 2f;
    public float MinMoveSpeed = 1f;

    [Header("Look")]
    public float LookSensitivity = 0.15f;

    private float yaw;
    private float pitch;
    private bool looking;

    private void Start()
    {
        Vector3 angles = transform.eulerAngles;
        yaw = angles.y;
        pitch = angles.x;
    }

    private void Update()
    {
        Mouse mouse = Mouse.current;
        Keyboard keyboard = Keyboard.current;
        if (mouse == null || keyboard == null) return;

        UpdateLook(mouse);
        UpdateMove(keyboard);
        UpdateSpeed(mouse);
    }

    private void UpdateLook(Mouse mouse)
    {
        bool wantsLook = mouse.rightButton.isPressed;
        if (wantsLook != looking)
        {
            looking = wantsLook;
            Cursor.lockState = looking ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !looking;
        }

        if (!looking) return;

        Vector2 delta = mouse.delta.ReadValue();
        yaw += delta.x * LookSensitivity;
        pitch -= delta.y * LookSensitivity;
        pitch = Mathf.Clamp(pitch, -89f, 89f);
        transform.rotation = Quaternion.Euler(pitch, yaw, 0f);
    }

    private void UpdateMove(Keyboard keyboard)
    {
        Vector3 move = Vector3.zero;
        if (keyboard.wKey.isPressed) move += transform.forward;
        if (keyboard.sKey.isPressed) move -= transform.forward;
        if (keyboard.aKey.isPressed) move -= transform.right;
        if (keyboard.dKey.isPressed) move += transform.right;

        float vertical = 0f;
        if (keyboard.eKey.isPressed) vertical += 1f;
        if (keyboard.qKey.isPressed) vertical -= 1f;

        float speed = MoveSpeed * (keyboard.leftShiftKey.isPressed ? FastMoveMultiplier : 1f);

        transform.position += move.normalized * speed * Time.deltaTime;
        transform.position += Vector3.up * vertical * VerticalSpeed * Time.deltaTime;
    }

    private void UpdateSpeed(Mouse mouse)
    {
        float scroll = mouse.scroll.ReadValue().y;
        if (scroll == 0f) return;

        MoveSpeed = Mathf.Max(MinMoveSpeed, MoveSpeed + Mathf.Sign(scroll) * ScrollSpeedStep);
    }
}
