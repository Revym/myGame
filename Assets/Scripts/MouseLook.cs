using UnityEngine;
using UnityEngine.InputSystem;

public class MouseLook : MonoBehaviour
{
    [Header("Czułość myszy")]
    public float mouseSensitivity = 100f;

    private Transform playerBody;
    private float xRotation = 0f;
    private Mouse mouse;

    public ExitMenu exitMenu;

    void Start()
    {
        mouse = Mouse.current;
        if (mouse == null) Debug.LogError("Brak myszy!");
        // Odnajdź ciało gracza (jego Capsule) — zakładamy, że skrypt jest na Camera
        playerBody = transform.parent;
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
        Vector2 delta = mouse.delta.ReadValue() * mouseSensitivity * Time.deltaTime;

        // Obrót w pionie (pitch)
        xRotation -= delta.y;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);
        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        // Obrót w poziomie (yaw) – ruch całego ciała
        playerBody.Rotate(Vector3.up * delta.x);


    }
}
