using UnityEngine;
using UnityEngine.InputSystem;

public class SimpleADS : MonoBehaviour
{
    [Header("Positions (local)")]
    public Vector3 hipPosition = new Vector3(0.3f, -0.19f, 0.94f);
    public Vector3 adsPosition = new Vector3(0f, -0.19f, 0.94f);
    public string crosshairName = "SimpleCrosshair";
    public bool forceADS = false;

    [Header("Settings")]
    public float   transitionSpeed = 10f;  // im wyższa, tym szybsze przejście

    private Vector3 targetPosition;

    private GameObject crosshair;

    void Start()
    {
        // podłączenie do crosshair
        crosshair = GameObject.Find(crosshairName);
        if(crosshair==null)
            Debug.LogError("Couldnt find object "+crosshairName);

        // ustaw początkowo hip‐fire
        transform.localPosition = hipPosition;
        crosshair?.SetActive(true);
    }

    void Update()
    {
        if (ExitMenu.Instance != null && ExitMenu.Instance.inputBlocked) return;

        bool aiming = Mouse.current != null && Mouse.current.rightButton.isPressed;

        targetPosition = aiming ? adsPosition : hipPosition;
        if (forceADS) targetPosition = adsPosition;

        // płynnie przechodzimy do targetPosition
        transform.localPosition = Vector3.Lerp(
            transform.localPosition,
            targetPosition,
            Time.deltaTime * transitionSpeed
        );

        if (crosshair!=null)
            crosshair.SetActive(!aiming);

    }
}
