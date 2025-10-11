using UnityEngine;

public class ray : MonoBehaviour
{
    void Start()
    {
        Vector3 coord = new Vector3(126f, 10f, 126f);
        Debug.DrawRay(coord, Vector3.up * 200f, Color.green, 999999f, false);
    }

    
}
