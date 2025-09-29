using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class JumpControllerFixed : MonoBehaviour
{
    [Header("Skok i grawitacja")]
    [SerializeField] private float jumpHeight = 1.25f;    // wysokość skoku (m)
    [SerializeField] private float gravity = 20f;         // siła grawitacji (m/s^2)
    [SerializeField] private float groundedStick = 0.05f; // małe ujemne v przy ziemi, żeby CC 'trzymał się' podłoża
    [SerializeField] private float ceilingBuffer = 0.05f; // bufor przy suficie (m)
    [SerializeField] private LayerMask obstacleMask = ~0; // warstwy, które traktujemy jako przeszkody (domyślnie wszystkie)

    private CharacterController cc;
    private MovementController movementController;
    private Keyboard keyboard;

    private float verticalVelocity = 0f;

    void Start()
    {
        cc = GetComponent<CharacterController>();
        movementController = GetComponent<MovementController>();
        keyboard = Keyboard.current;

        if (keyboard == null)
            Debug.LogError("Nie wykryto klawiatury przez Input System!");
        if (movementController == null)
            Debug.LogWarning("Brakuje komponentu MovementController — upewnij się, że istnieje.");
    }

    void Update()
    {
        if (keyboard == null) return;

        bool isGrounded = cc.isGrounded;

        // gdy na ziemi i mamy ujemną prędkość -> utrzymaj małe ujemne żeby CharacterController nie 'odksoczył'
        if (isGrounded && verticalVelocity < 0f)
            verticalVelocity = -groundedStick;

        // skok (tylko gdy na ziemi)
        if (isGrounded && keyboard.spaceKey.wasPressedThisFrame)
        {
            // v = sqrt(2 * g * h)
            verticalVelocity = Mathf.Sqrt(2f * gravity * jumpHeight);
        }

        // grawitacja
        verticalVelocity -= gravity * Time.deltaTime;

        // --- SPRAWDZENIE SUFITU (pre-check) ---
        // oblicz planowany ruch w pionie tej klatki
        float deltaY = verticalVelocity * Time.deltaTime;
        if (deltaY > 0f)
        {
            // punkt startowy dla SphereCast: środek CharacterController w świecie
            Vector3 sphereOrigin = transform.TransformPoint(cc.center);
            float radius = Mathf.Max(0.01f, cc.radius);
            float castDistance = deltaY + ceilingBuffer;

            RaycastHit hit;
            // jeśli SphereCast znajdzie coś nad nami w odległości castDistance
            if (Physics.SphereCast(sphereOrigin, radius, Vector3.up, out hit, castDistance, obstacleMask, QueryTriggerInteraction.Ignore))
            {
                // odległość do kolizji (hit.distance) może być mniejsza niż castDistance
                // obliczamy maksymalną prędkość pionową, która nie spowoduje penetracji
                float maxAllowed = (hit.distance - ceilingBuffer) / Time.deltaTime;

                // jeśli maxAllowed jest małe/ujemne -> ustawiamy niewielką ujemną prędkość (żeby nie 'przykleić' się do sufitu)
                if (maxAllowed <= 0f)
                {
                    verticalVelocity = -groundedStick;
                }
                else
                {
                    verticalVelocity = Mathf.Min(verticalVelocity, maxAllowed);
                }
            }
        }

        // ruch poziomy (z MovementController) — domyślam, że zwraca wektor w world space z x/z
        Vector3 move = Vector3.zero;
        if (movementController != null)
            move = movementController.ProcessMovement(isGrounded);

        // dołóż oś Y
        move.y = verticalVelocity;

        // wykonaj ostateczne przesunięcie
        CollisionFlags flags = cc.Move(move * Time.deltaTime);

        // --- POST-CHECK: jeśli CharacterController zgłosił kolizję od góry -> wyzeruj prędkość wzwyż ---
        if ((flags & CollisionFlags.Above) != 0 && verticalVelocity > 0f)
        {
            verticalVelocity = 0f;
        }

        // (opcjonalnie) jeśli chcesz mieć bardziej precyzyjny ground-check możesz tu nadpisać isGrounded
        // korzystając z Physics.CheckSphere itp. — ale to już zależy od Twojego MovementControllera/architektury.
    }
}
