using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class MovementController : MonoBehaviour
{
    [Header("Ruch")]
    public float speed = 5f;                // prędkość maksymalna
    public float sprintMultiplier = 2f;     // mnożnik sprintu na ziemi
    [Range(0.1f, 20f)]
    public float movementSpeedGain = 8f;    // im wyższy, tym szybciej dogonisz desiredVelocity

    [Header("Air Control")]
    public float jumpControlMultiplier = 0.8f;

    private CharacterController cc;
    private Keyboard keyboard;

    // wektor aktualnej prędkości (bez y)
    private Vector3 currentVelocity = Vector3.zero;

    public Vector3 CurrentVelocity => currentVelocity;

    void Start()
    {
        cc = GetComponent<CharacterController>();
        keyboard = Keyboard.current;
        if (keyboard == null)
            Debug.LogError("Nie wykryto klawiatury przez Input System!");
    }

    public Vector3 ProcessMovement(bool isGrounded)
    {
        if (keyboard == null) return Vector3.zero;

        // odczyt wejścia poziomego
        float h = (keyboard.dKey.isPressed ? 1f : 0f)
                + (keyboard.aKey.isPressed ? -1f : 0f);
        float v = (keyboard.wKey.isPressed ? 1f : 0f)
                + (keyboard.sKey.isPressed ? -1f : 0f);

        // sprint tylko na ziemi do przodu
        bool isSprinting = isGrounded
                           && keyboard.leftShiftKey.isPressed
                           && v > 0f;
        float targetSpeed = speed * (isSprinting ? sprintMultiplier : 1f);

        // Normalizacja wejścia
        Vector3 inputDir = transform.right * h + transform.forward * v;
        Vector3 inputNorm = inputDir.sqrMagnitude > 0f ? inputDir.normalized : Vector3.zero;

        // wektor prędkości docelowej (bez y)
        float airControl = isGrounded ? 1f : jumpControlMultiplier;
        Vector3 desiredVelocity = inputNorm * targetSpeed * airControl;

        // płynne przybliżanie currentVelocity do desiredVelocity
        currentVelocity = Vector3.Lerp(currentVelocity, desiredVelocity, movementSpeedGain * Time.deltaTime);

        return currentVelocity;
    }
}
