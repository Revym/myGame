using UnityEngine;
using UnityEngine.InputSystem;

public class MouseLookHorizontal : MonoBehaviour
{
    [Header("Czułość myszy")]
    public float mouseSensitivityX = 90f;

    private Transform playerBody;
    //private float xRotation = 0f;
    private Mouse mouse;

    public ExitMenu exitMenu;

    void Start()
    {
        mouse = Mouse.current;
        if (mouse == null) Debug.LogError("Brak myszy!");
        playerBody = transform;
        // Zablokuj widoczny kursor
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        if (mouse == null) return;
        if (ExitMenu.Instance != null && ExitMenu.Instance.inputBlocked) return;

        // Zwolnienie kursora
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
            Cursor.lockState = CursorLockMode.None;

        // Pobierz ruch myszy
        Vector2 delta = mouse.delta.ReadValue() * Time.deltaTime;
        float mouseX = delta.x * mouseSensitivityX;

        // Obrót w poziomie (yaw) – ruch całego ciała
        playerBody.Rotate(Vector3.up * mouseX);
    }
}