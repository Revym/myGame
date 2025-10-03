using UnityEngine;
using UnityEngine.InputSystem;

public class GunAnimationController : MonoBehaviour
{
    private Animator animator;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        animator = GetComponent<Animator>();
        Debug.Log($"Animator = {animator}");
    }

    // Update is called once per frame
    void Update()
    {
        
        if (animator.GetBool("Shot"))
        {
            animator.SetBool("Shot", false);
        }
        

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            animator.SetBool("Shot", true);
        }
        
        
    }
}
