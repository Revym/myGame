using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    private Transform fixedCameraPoint;
    private Transform movingCameraPoint;

    private string fixedName = "FixedCameraPoint";
    private string movingName = "MovingCameraPoint";
    [SerializeField]private string rigName = "playerCharNoRig";

    [Range(0f, 2f)]
    [SerializeField] private float headMoveFactor = 0.5f;
    [Range(0f, 2f)]
    [SerializeField] private float headRotationFactor = 0.5f;

    private bool error = false;

    void Start()
    {
        if (transform.parent != null)
        {
            fixedCameraPoint = transform.parent.Find(fixedName);
            movingCameraPoint = FindDeepChild(transform.parent.transform, movingName);
        }

        if (fixedCameraPoint == null || movingCameraPoint == null)
        {
            error = true;
            Debug.Log("Error, 1 or 2 camera points = NULL");
        }

    }

    void LateUpdate()
    {

        //Debug.Log("mov point: " + movingCameraPoint.position);
        //Debug.Log("fixed point: " + fixedCameraPoint.position);

        if (error) return;

        // calculating the value of a vector from static point to moving point
        Vector3 diffVec = movingCameraPoint.position - fixedCameraPoint.position;

        // applying the vector to the camera
        Vector3 targetPosition = fixedCameraPoint.position + diffVec * headMoveFactor;
        //Debug.Log("vector: " + diffVec);
        //Debug.Log("target pos: " + targetPosition);
        //Debug.Log("");
        transform.position = targetPosition;

        // Różnica rotacji: "jak obrócić Fixed, by osiągnąć Moving"
        Quaternion deltaRot = Quaternion.Inverse(fixedCameraPoint.rotation) * movingCameraPoint.rotation;

        // Teraz chcemy tylko część tej rotacji (np. 0.5 = połowa obrotu)
        Quaternion partialRot = Quaternion.Slerp(Quaternion.identity, deltaRot, headRotationFactor);

        // Finalna rotacja kamery = Fixed * partial
        transform.rotation = fixedCameraPoint.rotation * partialRot;
    }

    private Transform FindDeepChild(Transform parent, string name)
    {
        foreach (Transform child in parent)
        {
            if (child.name == name)
                return child;

            Transform result = FindDeepChild(child, name);
            if (result != null)
                return result;
        }
        return null;
    }
}
