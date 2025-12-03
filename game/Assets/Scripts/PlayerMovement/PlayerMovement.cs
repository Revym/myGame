using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Ruch")]
    public float speed = 5f;
    public float sprintMultiplier = 2f;
    [Range(0.1f, 20f)]
    public float movementSpeedGain = 8f;
    public float jumpHeight = 1.25f;
    public float gravity = 20f;
    
    // ZMIANA 1: Zwiększamy siłę docisku, -1f to za mało dla szybkiego ruchu w dół
    [Tooltip("Siła dociskająca gracza do ziemi, gdy nie skacze")]
    public float stickToGroundForce = 5f; 

    // ZMIANA 2: Margines dla wykrywania ziemi (Raycast)
    [Tooltip("Jak daleko pod graczem szukamy ziemi, aby pozwolić na sprint")]
    public float groundCheckDistance = 0.2f; 
    public LayerMask groundLayer; // Przypisz warstwę ziemi w inspektorze (np. Default)

    private CharacterController cc;
    private Keyboard keyboard;
    private float verticalVelocity = 0f;

    public ExitMenu exitMenu;

    [Header("Air Control")]
    public float jumpControlMultiplier = 0.8f;

    private Vector3 currentVelocity = Vector3.zero;

    void Start()
    {
        cc = GetComponent<CharacterController>();
        keyboard = Keyboard.current;
        if (keyboard == null)
            Debug.LogError("Nie wykryto klawiatury przez Input System!");

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
        // Zabezpieczenie: jeśli groundLayer nie jest ustawiony, ustawiamy na wszystko
        if(groundLayer == 0) groundLayer = ~0; 
    }

    void Update()
    {
        if (keyboard == null) return;
        if (ExitMenu.Instance != null && ExitMenu.Instance.inputBlocked) return;

        // --- DETEKCJA ZIEMI (ULEPSZONA) ---
        // Sprawdzamy standardowy isGrounded ORAZ dodatkowy Raycast
        // Pozwala to na sprintowanie, gdy 'podskakujemy' na nierównościach
        bool isGroundedPhysical = cc.isGrounded;
        bool isGroundedForSprint = isGroundedPhysical || CheckGroundNear();

        // Resetowanie prędkości wertykalnej na ziemi
        if (isGroundedPhysical && verticalVelocity < 0f)
        {
            // ZMIANA 3: Używamy większej siły docisku (-stickToGroundForce), 
            // aby 'przykleić' gracza do zbocza podczas schodzenia
            verticalVelocity = -stickToGroundForce;
        }

        // --- WEJŚCIE ---
        float h = (keyboard.dKey.isPressed ? 1f : 0f) + (keyboard.aKey.isPressed ? -1f : 0f);
        float v = (keyboard.wKey.isPressed ? 1f : 0f) + (keyboard.sKey.isPressed ? -1f : 0f);

        // --- SPRINT ---
        // Używamy naszej nowej, bardziej wyrozumiałej zmiennej isGroundedForSprint
        bool isSprinting = isGroundedForSprint
                           && keyboard.leftShiftKey.isPressed
                           && v > 0f;

        float targetSpeed = speed * (isSprinting ? sprintMultiplier : 1f);

        Vector3 inputDir = transform.right * h + transform.forward * v;
        Vector3 inputNorm = inputDir.sqrMagnitude > 0f ? inputDir.normalized : Vector3.zero;

        // --- PORUSZANIE ---
        // Tu też używamy wyrozumiałej detekcji, aby nie tracić kontroli na wybojach
        float airControl = isGroundedForSprint ? 1f : jumpControlMultiplier;
        Vector3 desiredVelocity = inputNorm * targetSpeed * airControl;

        currentVelocity = Vector3.Lerp(currentVelocity, desiredVelocity, movementSpeedGain * Time.deltaTime);

        // --- SKOK ---
        // Skakać pozwalamy tylko, gdy fizycznie dotykamy ziemi (isGroundedPhysical),
        // żeby uniknąć 'coyote jump' w nieskończoność, chociaż można tu też użyć isGroundedForSprint dla lepszego feelingu
        if (isGroundedForSprint && keyboard.spaceKey.wasPressedThisFrame)
        {
            verticalVelocity = Mathf.Sqrt(2f * gravity * jumpHeight);
        }

        // --- GRAWITACJA I RUCH FINALNY ---
        verticalVelocity -= gravity * Time.deltaTime;

        Vector3 move = currentVelocity;
        move.y = verticalVelocity;

        CollisionFlags flags = cc.Move(move * Time.deltaTime);

        // Obsługa uderzenia głową w sufit
        if ((flags & CollisionFlags.Above) != 0)
        {
             if(verticalVelocity > 0) verticalVelocity = -0.1f; // zerujemy tylko jeśli wznosiliśmy się
        }
    }

    // Dodatkowa metoda pomocnicza
    private bool CheckGroundNear()
    {
        // Puszczamy promień od spodu gracza nieco w dół
        // (cc.height / 2) to połowa wysokości kapsuły + mały margines
        float rayStartHeight = cc.height / 2f; 
        // Offsetujemy start nieco w górę, żeby raycast nie zaczynał się "pod ziemią" w skrajnych przypadkach
        Vector3 origin = transform.position + Vector3.up * (rayStartHeight * 0.5f); 
        
        float distance = (rayStartHeight * 0.5f) + 0.1f + groundCheckDistance; 

        return Physics.Raycast(origin, Vector3.down, distance, groundLayer);
    }
}