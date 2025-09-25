using UnityEngine;

public class HandGunDistance : MonoBehaviour
{
    public Transform RightHand;
    public Transform Gun;

    void Update()
    {
        if(RightHand!=null && Gun!=null)
        {
            float distance = Vector3.Distance(RightHand.position, Gun.position);
            Debug.Log("[HandGunDistance.cs] distance = " + distance);
        }
        else
        {
            Debug.LogWarning("[HandGunDistance.cs] Either of objects is null");
        }
    }
}