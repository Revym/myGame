using UnityEngine;
using UnityEngine.InputSystem;

public class BetterADS : MonoBehaviour
{
    [Header("Positions (local)")]
    public Vector3 hipPosition = new Vector3(0.125f, -0.19f, 0.6f);
    public Vector3 adsPosition = new Vector3(0f, -0.19f, 0.6f);
    public string crosshairName = "SimpleCrosshair";

    [Header("Settings")]
    public float   transitionSpeed = 10f;  // im wyższa, tym szybsze przejście

    [Header("Gun distance objects")]
    public Transform RightHand;
    public Transform Gun;
    private float distance;

    private Vector3 targetPosition;
    private GameObject crosshair;

    void Start()
    {
        // podłączenie do crosshair
        crosshair = GameObject.Find(crosshairName);
        if(crosshair==null)
            Debug.LogError("Couldnt find object "+crosshairName);

        // ustaw początkowo hip‐fire
        transform.localPosition = hipPosition;
        crosshair?.SetActive(true);
    }

    void Update()
    {
        if (ExitMenu.Instance != null && ExitMenu.Instance.inputBlocked) return;

        if(RightHand!=null && Gun!=null){
            distance = Vector3.Distance(RightHand.position, Gun.position);
            //Debug.Log("[HandGunDistance.cs] distance = " + distance);
        }else{
            //Debug.LogWarning("[HandGunDistance.cs] Either of objects is null");
        }

        
        bool aiming = Mouse.current != null && Mouse.current.rightButton.isPressed;
        targetPosition = aiming ? adsPosition : hipPosition;

        // przesunięcie dodatkowe na odl
        transform.position -= transform.right * distance;

        float maxOffSet = 2.0f;
        float localOffSetZ = -Mathf.Min(distance, maxOffSet);
        if (Mathf.Abs(localOffSetZ)<0.01) localOffSetZ=0;
        Vector3 finalTargetPosition = targetPosition + new Vector3(0f, 0f, localOffSetZ);

        // płynnie przechodzimy do targetPosition
        transform.localPosition = Vector3.Lerp(
            transform.localPosition,
            finalTargetPosition,
            Time.deltaTime * transitionSpeed
        );

        if (crosshair!=null)
            crosshair.SetActive(!aiming);
        
    }
}
