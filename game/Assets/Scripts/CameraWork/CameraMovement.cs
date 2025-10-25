using UnityEngine;
using UnityEngine.InputSystem;

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

    [Header("Czułość myszy")]
    public float mouseSensitivityY = 90f;

    private float xRotation = 0f;
    private Mouse mouse;
    private bool error = false;
    
    public ExitMenu exitMenu;


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

        // podłączenie myszki
        mouse = Mouse.current;
        if (mouse == null) Debug.LogError("Brak myszy!");
        Cursor.lockState = CursorLockMode.Locked;

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
        transform.position = targetPosition;

        // Różnica rotacji: "jak obrócić Fixed, by osiągnąć Moving"
        Quaternion deltaRot = Quaternion.Inverse(fixedCameraPoint.rotation) * movingCameraPoint.rotation;

        // Teraz chcemy tylko część tej rotacji (np. 0.5 = połowa obrotu)
        Quaternion partialRot = Quaternion.Slerp(Quaternion.identity, deltaRot, headRotationFactor);

        // zczytanie dodatkowo myszki
        mouseInput();
        Quaternion mouseRot = Quaternion.Euler(xRotation, 0f, 0f);

        // Finalna rotacja kamery = Fixed * partial
        transform.rotation = fixedCameraPoint.rotation * partialRot * mouseRot ;
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

    private void mouseInput()
    {
        if (ExitMenu.Instance != null && ExitMenu.Instance.inputBlocked) return;
        
        // Zwolnienie kursora
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
            Cursor.lockState = CursorLockMode.None;

        // Pobierz ruch myszy
        Vector2 delta = mouse.delta.ReadValue() * Time.deltaTime;
        float mouseY = delta.y * mouseSensitivityY;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);
        //transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
    }
}

