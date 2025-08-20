using UnityEngine;

public class Sprite : MonoBehaviour
{
    public Transform target;

    void Start()
    {
        target = GameObject.Find("Camera").transform;
    }

    void Update()
    {
        if (target != null)
        {
            transform.LookAt(target);
            transform.Rotate(0,180,0);
        }
    }
}