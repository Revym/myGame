using UnityEngine;

[RequireComponent(typeof(Animator))]
public class WeaponIK : MonoBehaviour
{
    Animator animator;
    [Tooltip("Target transform (RightHandGrip) z Twojej broni")]
    public Transform rightHandTarget;

    void Awake()
    {
        animator = GetComponent<Animator>();
    }

    void OnAnimatorIK(int layerIndex)
    {
        if (animator == null) return;

        if (rightHandTarget != null && animator.isHuman)
        {
            animator.SetIKPositionWeight(AvatarIKGoal.RightHand, 1f);
            animator.SetIKRotationWeight(AvatarIKGoal.RightHand, 1f);
            animator.SetIKPosition(AvatarIKGoal.RightHand, rightHandTarget.position);
            animator.SetIKRotation(AvatarIKGoal.RightHand, rightHandTarget.rotation);
        }
        else
        {
            // wyłączamy peso jeśli nie mamy targetu / nie jest humanoid
            animator.SetIKPositionWeight(AvatarIKGoal.RightHand, 0f);
            animator.SetIKRotationWeight(AvatarIKGoal.RightHand, 0f);

            if (!animator.isHuman)
                Debug.LogWarning("Animator nie jest Humanoid. AvatarIK won't work. Consider switching Rig->Humanoid or use Animation Rigging.");
        }
    }
}