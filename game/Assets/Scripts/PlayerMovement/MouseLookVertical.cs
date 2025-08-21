using UnityEngine;
using UnityEngine.InputSystem;

public class MouseLookVertical : MonoBehaviour
{
    [Header("Czułość myszy")]
    public float mouseSensitivityY = 90f;

    private Transform playerBody;
    private float xRotation = 0f;
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
        float mouseY = delta.y * mouseSensitivityY;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);
        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
    }
}