using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Ruch")]
    public float speed = 5f;                // prędkość maksymalna
    public float sprintMultiplier = 2f;     // mnożnik sprintu na ziemi
    [Range(0.1f, 20f)]
    public float movementSpeedGain = 8f;   // im wyższy, tym szybciej dogonisz desiredVelocity
    public float jumpHeight = 1.25f;         // wysokość skoku
    public float gravity = 20f;           // siła grawitacji
    Vector3 jumpVector;

    private CharacterController cc;
    private Keyboard keyboard;
    private float verticalVelocity = 0f;

    public ExitMenu exitMenu;           // publiczne zmienne z ExitMenu.cs, referencja

    [Header("Air Control")]
    public float jumpControlMultiplier=0.8f;
    private Vector3 lastGroundedDirection = Vector3.zero;



    // wektor aktualnej prędkości (bez y)
    private Vector3 currentVelocity = Vector3.zero;

    void Start()
    {
        cc = GetComponent<CharacterController>();
        keyboard = Keyboard.current;
        if (keyboard == null)
            Debug.LogError("Nie wykryto klawiatury przez Input System!");

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        if (keyboard == null) return;

        if (ExitMenu.Instance != null && ExitMenu.Instance.inputBlocked) return;

        // spr czy na ziemii
        bool isGrounded = cc.isGrounded;
        if (isGrounded && verticalVelocity < 0f)
            verticalVelocity = -1f;

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

        // Normalizacja wejścia, aby uniknąć gain > sqrt(2)
        Vector3 inputDir = transform.right * h + transform.forward * v;
        Vector3 inputNorm = inputDir.sqrMagnitude > 0f ? inputDir.normalized : Vector3.zero;

        // wektor prędkości docelowej (bez y)
        float airControl = isGrounded ? 1f : jumpControlMultiplier;
        Vector3 desiredVelocity = inputNorm * targetSpeed * airControl;


        // płynne przybliżanie currentVelocity do desiredVelocity
        currentVelocity = Vector3.Lerp(currentVelocity, desiredVelocity, movementSpeedGain * Time.deltaTime);

        // skok
        if (isGrounded && keyboard.spaceKey.wasPressedThisFrame)
        {
            verticalVelocity = Mathf.Sqrt(2f * gravity * jumpHeight);
        }

        // grawitacja
        verticalVelocity -= gravity * Time.deltaTime;

        // finalny wektor ruchu: poziomy + pionowy
        Vector3 move = currentVelocity; //+lastGroundedDirection
        move.y = verticalVelocity;

        cc.Move(move * Time.deltaTime);

    }
}
/* devLog com

ruch CHYBA na razie git

*/