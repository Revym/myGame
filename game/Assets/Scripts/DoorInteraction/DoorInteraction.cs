using UnityEngine;
using UnityEngine.InputSystem; // for keyboard use
using System.Collections; // for IEnumerator
//using TMPro;

public class DoorInteraction : MonoBehaviour
{
    //########################################
    // status
    private bool isOpen = false;
    private bool isInsidePointer = false;

    // keyboard reading
    private Renderer rend;
    private Keyboard keyboard;

    [Header("Door rotation settings")]
    public bool antiClockwiseRotation = false;
    public float openDoorAngle = 120f;
    public float openSpeed = 2f;

    // rotation
    private Quaternion startRotation;
    private Quaternion targetRotation;
    private Quaternion closedRotation;

    // interaction text
    private GameObject interactionPrefab;
    private GameObject spawnedText;
    private Transform player;
    private bool isInFront = false;
    public bool textSide = false;

    // interaction delay
    [Header("Door interaction settings")]
    public float interactionCooldown = 0.5f;
    private float nextUseTime = 0f;

    // door sounds
    private AudioSource audioSource;
    private AudioClip doorOpen;
    private AudioClip doorClose;

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
        interactionPrefab = Resources.Load<GameObject>("TextPrefabs/InteractionTextPrefab");

        // connecting to player
        player = GameObject.Find("Camera").transform;

        // setting the start rotation
        closedRotation = transform.rotation;

        // connecting all audio
        audioSource = GetComponent<AudioSource>();
        doorOpen = Resources.Load<AudioClip>("Sounds/Doors/OpenMetalDoor");
        doorClose = Resources.Load<AudioClip>("Sounds/Doors/CloseMetalDoor");
        
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
                spawnedText.transform.localPosition = new Vector3(s1*0.35f, 1.5f, s2*0.8f);
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
            DoorRotate();
            nextUseTime = Time.time + interactionCooldown;
        }
    }

    //---------------functions--------------------

    public void DoorRotate()
    {
        int rotation = antiClockwiseRotation ? -1 : 1; // rotation orientation

        if (!isOpen) // closed > opening
        {
            startRotation = transform.rotation;
            targetRotation = closedRotation*Quaternion.Euler(0, openDoorAngle*rotation, 0); 
            audioSource.PlayOneShot(doorOpen);
        } else { // opened > closing
            startRotation = transform.rotation;
            targetRotation = closedRotation; 
            audioSource.PlayOneShot(doorClose);
        }

        StopAllCoroutines();
        StartCoroutine(RotateDoor());

        isOpen=!isOpen;
    }

    private IEnumerator RotateDoor()
    {
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * openSpeed;
            transform.rotation = Quaternion.Slerp(startRotation, targetRotation, t);
            yield return null;
        }
    }

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
