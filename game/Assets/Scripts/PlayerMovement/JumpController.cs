using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class JumpController : MonoBehaviour
{
    [Header("Skok i grawitacja")]
    public float jumpHeight = 1.25f;    // wysokość skoku
    public float gravity = 20f;         // siła grawitacji

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
    }

    void Update()
    {
        if (keyboard == null) return;

        bool isGrounded = cc.isGrounded;
        if (isGrounded && verticalVelocity < 0f)
            verticalVelocity = -1f;

        // skok
        if (isGrounded && keyboard.spaceKey.wasPressedThisFrame)
        {
            verticalVelocity = Mathf.Sqrt(2f * gravity * jumpHeight);
        }

        // grawitacja
        verticalVelocity -= gravity * Time.deltaTime;

        // ruch poziomy z MovementController
        Vector3 move = movementController.ProcessMovement(isGrounded);

        // dodanie osi Y
        move.y = verticalVelocity;

        // finalne przesunięcie
        CollisionFlags flags = cc.Move(move * Time.deltaTime);

        // kolizja z sufitem
        if ((flags & CollisionFlags.Above) != 0)
        {
            cc.Move(Vector3.down * 0.01f);
            verticalVelocity = -0.1f;
        }

        // reset prędkości pionowej gdy dotkniemy ziemi
        if (cc.isGrounded && verticalVelocity < 0f)
        {
            verticalVelocity = -1f;
        }
        
    }
}
