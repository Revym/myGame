using UnityEngine;

public class DoorDistance : MonoBehaviour
{
    Vector3 toPlayer;

    public GameObject player;
    public Transform parent;

    void Start()
    {
        //player = GameObject.FindWithTag("Player5");
        parent = player.transform.parent;
    }

    void Update()
    {
        toPlayer = (parent.position - transform.position).normalized;

        float dot = Vector3.Dot(transform.right, toPlayer);

        if (dot > 0)
        {
            Debug.Log("Gracz stoi z przodu drzwi");
        }
        else
        {
            Debug.Log("Gracz stoi z tyłu drzwi");
        }
    }
}