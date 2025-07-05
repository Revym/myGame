using UnityEngine;
using UnityEngine.InputSystem;

public class SimpleADS : MonoBehaviour
{
    [Header("Positions (local)")]
    public Vector3 hipPosition = new Vector3(0.3f, -0.19f, 0.94f);
    public Vector3 adsPosition = new Vector3(0f, -0.19f, 0.94f);

    [Header("Settings")]
    public float   transitionSpeed = 10f;  // im wyższa, tym szybsze przejście

    private Vector3 targetPosition;

    void Start()
    {
        // ustaw początkowo hip‐fire
        transform.localPosition = hipPosition;
    }

    void Update()
    {
        bool aiming = Mouse.current != null && Mouse.current.rightButton.isPressed;

        targetPosition = aiming ? adsPosition : hipPosition;

        // płynnie przechodzimy do targetPosition
        transform.localPosition = Vector3.Lerp(
            transform.localPosition,
            targetPosition,
            Time.deltaTime * transitionSpeed
        );
    }
}
