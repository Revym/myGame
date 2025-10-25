using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Animator))]

public class NewAnimationManager : MonoBehaviour
{
    private Keyboard keyboard;
    private Animator animator;
    private Mouse mouse;
    private CharacterController cc;

    void Start()
    {
        animator = GetComponent<Animator>();
        keyboard = Keyboard.current;
        if (keyboard == null)
            Debug.LogError("No keyboard!");

        mouse = Mouse.current;
        if (mouse == null) Debug.LogError("No mouse!");

        cc = GetComponentInParent<CharacterController>();
    }

    void Update()
    {
        if (keyboard == null) { Debug.LogError("No keyboard"); return; }
        if (cc == null) { Debug.LogError("No CharacterControler in parent object"); return; }

        if (keyboard.wKey.isPressed)
        {
            animator.SetBool("WalkingForward", true);
            animator.SetBool("WalkingBackward", false);
            
            if (keyboard.leftShiftKey.isPressed)
            {
                animator.SetBool("Running", true);
            }
            else
            {
                animator.SetBool("Running", false);
            }

        }
        else if (keyboard.sKey.isPressed)
        {
            animator.SetBool("WalkingForward", false);
            animator.SetBool("WalkingBackward", true);
        }
        else
        {
            animator.SetBool("WalkingBackward", false);
            animator.SetBool("WalkingForward", false);
        }


        if (mouse.rightButton.isPressed)
        {
            animator.SetBool("Aiming", true);
        }
        else
        {
            animator.SetBool("Aiming", false);
        }


        if (keyboard.dKey.isPressed)
        {
            animator.SetBool("WalkingRight", true);
        }
        else
        {
            animator.SetBool("WalkingRight", false);
        }


        if (keyboard.aKey.isPressed)
        {
            animator.SetBool("WalkingLeft", true);
        }
        else
        {
            animator.SetBool("WalkingLeft", false);
        }

        if (!cc.isGrounded)
        {
            animator.SetBool("Jumping", true);
        }
        else
        {
            animator.SetBool("Jumping", false);
        }
    }


}