using UnityEngine;

public class RagdollActivator : MonoBehaviour
{
    private Animator animator;
    private Rigidbody[] ragdollBodies;
    private Collider[] ragdollColliders;
    private CapsuleCollider capsule;

    public bool ragdoll = false;

    // hit test
    private Renderer rend;

    void Awake()
    {
        animator = GetComponentInChildren<Animator>();
        ragdollBodies = GetComponentsInChildren<Rigidbody>();
        ragdollColliders = GetComponentsInChildren<Collider>();

        // Turn ragdoll off at start
        SetRagdoll(ragdoll);

        // connect renderer
        rend = GetComponent<Renderer>();

        // connect CapsuleCollider
        capsule = GetComponent<CapsuleCollider>();
    }

    // Toggle ragdoll state
    private void SetRagdoll(bool state)
    {
        // animator should only be on when not ragdolling
        if (animator != null) animator.enabled = !state;

        foreach (var rb in ragdollBodies)
        {
            rb.isKinematic = !state;
            rb.detectCollisions = state;
        }

        foreach (var col in ragdollColliders)
        {
            // If you have a root capsule collider for the player, 
            // don’t disable that unless you want it gone too
            if (col.gameObject == this.gameObject) continue;

            col.enabled = state;
        }
    }

    // Public method you can call when the character dies
    public void ActivateRagdoll()
    {
        SetRagdoll(true);
    }

    void Update(){
        SetRagdoll(ragdoll);
    }


    public void Hit()
    {
        //StopAllCoroutines();
        // v avoid flinging the char out bc of intersecting colliders
        capsule.enabled = false;
        ragdoll = true;
    }
}
