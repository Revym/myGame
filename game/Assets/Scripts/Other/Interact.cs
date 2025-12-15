using UnityEngine;
using UnityEngine.InputSystem; // for keyboard use
using System.Collections; // for IEnumerator
using UnityEngine.SceneManagement;

public class Interact : MonoBehaviour
{
    //########################################
    // status
    private bool isInsidePointer = false;

    // keyboard reading
    private Renderer rend;
    private Keyboard keyboard;

    // interaction text
    private GameObject interactionPrefab;
    private GameObject spawnedText;
    private Transform player;
    private bool isInFront = false;
    public bool textSide = false;

    // interaction delay
    [Header("Interaction settings")]
    public float interactionCooldown = 0.5f;
    private float nextUseTime = 0f;

    //########################################

    void Start()
    {
        // Pobieramy komponent Renderer z obiektu
        rend = GetComponent<Renderer>();
        if (rend == null)
        {
            Debug.LogError("Brak komponentu Renderer na tym obiekcie!");
        }

        // Pobieramy klawiature
        keyboard = Keyboard.current;
        if (keyboard == null)
            Debug.LogError("Nie wykryto klawiatury przez Input System!");

        // connecting the text prefab from files
        interactionPrefab = Resources.Load<GameObject>("Prefabs/TextPrefabs/InteractionTextPrefab");

        // connecting to player
        player = GameObject.Find("PlayerCamera").transform;

    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.name == "PlayerPointer")
        {
            isInsidePointer = true;

            // instantiating the text
            if (spawnedText == null){
                spawnedText = Instantiate(interactionPrefab, transform);
                int s1 = isInFront ? 1 : -1; // based on front/back to the door
                int s2 = textSide ? 1 : -1; // based on different pivot point
                spawnedText.transform.localPosition = new Vector3(s1 * 0.35f, 1.5f, s2 * 0.8f);
                spawnedText.transform.LookAt(player);
                spawnedText.transform.rotation = Quaternion.Euler(0f, spawnedText.transform.rotation.eulerAngles.y + 180f, 0f);
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.gameObject.name == "PlayerPointer")
        {
            isInsidePointer = false;

            // deleting the text
            Destroy(spawnedText);
        }
    }

    void Update()
    {
        isInFront = DoorPlayerOrientation();

        if (isInsidePointer && keyboard.fKey.isPressed && Time.time>=nextUseTime)
        {
            if (keyboard == null) return;
            
            SceneManager.LoadScene("MapScene");

            nextUseTime = Time.time + interactionCooldown;
        }
    }

    //---------------functions--------------------
    private bool DoorPlayerOrientation()
    {
        Vector3 toPlayer = (player.position - transform.position).normalized;
        float dot = Vector3.Dot(transform.right, toPlayer);

        if (dot > 0)
        {
            //Debug.Log(gameObject.name+" : Front");
            return true;
        }
        else
        {
            //Debug.Log(gameObject.name+" : Back");
            return false;
        }
    }

}
