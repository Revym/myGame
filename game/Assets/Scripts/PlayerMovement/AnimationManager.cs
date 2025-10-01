using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Animator))]

public class AnimationManager : MonoBehaviour
{
    private Keyboard keyboard;
    private Animator animator;
    private Mouse mouse;

    private float standing = 0;
    private float walkingForward = 1;
    private float running = 2;
    private float walkingBackward = -1;

    private float shooting = 1;
    private float idle = 0;

    void Start()
    {
        animator = GetComponent<Animator>();
        keyboard = Keyboard.current;
        if (keyboard == null)
            Debug.LogError("Nie wykryto klawiatury przez Input System!");

        mouse = Mouse.current;
        if (mouse == null) Debug.LogError("Brak myszy!");

        animator.SetFloat("Speed", standing);
    }

    void Update()
    {
        if (keyboard == null) return;

        if (keyboard.wKey.isPressed)
        {
            if (keyboard.leftShiftKey.isPressed)
            {
                animator.SetFloat("Speed", running);
            }
            else
            {
                animator.SetFloat("Speed", walkingForward);
            }

        }
        else if (keyboard.sKey.isPressed)
        {
            animator.SetFloat("Speed", walkingBackward);
        }
        else
        {
            animator.SetFloat("Speed", 0);
        }

        if (mouse.rightButton.isPressed)
        {
            animator.SetFloat("Shooting", shooting);
        }
        else
        {
            animator.SetFloat("Shooting", idle);
        }

        if (keyboard.dKey.isPressed)
        {
            animator.SetBool("walkingRight", true);
        }
        else
        {
            animator.SetBool("walkingRight", false);
        }

        if (keyboard.aKey.isPressed)
        {
            animator.SetBool("walkingLeft", true);
        }
        else
        {
            animator.SetBool("walkingLeft", false);
        }
        
    }


}