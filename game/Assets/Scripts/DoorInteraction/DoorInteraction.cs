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

    // interaction text
    public GameObject interactionPrefab;
    private GameObject spawnedText;

    // interaction delay
    public float interactionCooldown = 1f;
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
        interactionPrefab = Resources.Load<GameObject>("TextPrefabs/InteractionTextPrefab");

    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.name == "PlayerPointer")
        {
            isInsidePointer = true;

            // instantiating the text
            if (spawnedText == null){
                spawnedText = Instantiate(interactionPrefab, transform);
                spawnedText.transform.localPosition = new Vector3(0.5f, 2f, 1f);
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
        if (isInsidePointer && keyboard.fKey.isPressed && Time.time>=nextUseTime)
        {
            if (keyboard == null) return;
            DoorRotate();
            nextUseTime = Time.time + interactionCooldown;
        }
    }

    public void DoorRotate()
    {
        int rotation = antiClockwiseRotation ? -1 : 1; // rotation orientation

        if (!isOpen) // closed > opening
        {
            startRotation = transform.rotation;
            targetRotation = Quaternion.Euler(0, openDoorAngle*rotation, 0); 
        } else { // opened > closing
            startRotation = transform.rotation;
            targetRotation = Quaternion.Euler(0, 0, 0); 
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
}
