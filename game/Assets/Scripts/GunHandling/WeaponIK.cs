using UnityEngine;

[RequireComponent(typeof(Animator))]
public class WeaponIK : MonoBehaviour
{
    private Animator animator;

    [Header("Right Hand IK")]
    [Tooltip("Target transform (RightHandGrip) z Twojej broni")]
    public Transform rightHandTarget;

    [Header("Left Hand IK (opcjonalnie)")]
    [Tooltip("Target transform (LeftHandGrip) z Twojej broni")]
    public Transform leftHandTarget;

    [Header("IK Weights")]
    [Range(0f, 1f)] public float rightHandWeight = 1f;
    [Range(0f, 1f)] public float leftHandWeight = 1f;

    void Awake()
    {
        animator = GetComponent<Animator>();
    }

    void OnAnimatorIK(int layerIndex)
    {
        if (animator == null || !animator.isHuman)
        {
            if (animator != null && !animator.isHuman)
                Debug.LogWarning($"{name}: Animator nie jest typu Humanoid – AvatarIK nie zadziała. Użyj Animation Rigging.");

            return;
        }

        // === PRAWa RĘKA ===
        if (rightHandTarget != null)
        {
            animator.SetIKPositionWeight(AvatarIKGoal.RightHand, rightHandWeight);
            animator.SetIKRotationWeight(AvatarIKGoal.RightHand, rightHandWeight);
            animator.SetIKPosition(AvatarIKGoal.RightHand, rightHandTarget.position);
            animator.SetIKRotation(AvatarIKGoal.RightHand, rightHandTarget.rotation);
        }
        else
        {
            animator.SetIKPositionWeight(AvatarIKGoal.RightHand, 0f);
            animator.SetIKRotationWeight(AvatarIKGoal.RightHand, 0f);
        }

        // === LEWA RĘKA ===
        if (leftHandTarget != null)
        {
            animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, leftHandWeight);
            animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, leftHandWeight);
            animator.SetIKPosition(AvatarIKGoal.LeftHand, leftHandTarget.position);
            animator.SetIKRotation(AvatarIKGoal.LeftHand, leftHandTarget.rotation);
        }
        else
        {
            animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 0f);
            animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, 0f);
        }
    }
}
