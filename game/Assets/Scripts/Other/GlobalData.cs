using UnityEngine;

public class GlobalData : MonoBehaviour
{
    void Update()
    {
        Debug.Log($"Glob pos = {transform.position}, glob rot = {transform.rotation}");
    }
}