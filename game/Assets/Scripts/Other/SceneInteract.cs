using UnityEngine;
using UnityEngine.InputSystem; // for keyboard use
using System.Collections; // for IEnumerator
using UnityEngine.SceneManagement;

public class SceneInteract : MonoBehaviour
{
    //########################################
    // status
    private bool isInsidePointer = false;

    // keyboard reading
    private Renderer rend;
    private Keyboard keyboard;

    // interaction text
    private Transform player;

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

        // connecting to player
        player = GameObject.Find("PlayerCamera").transform;

    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.name == "PlayerPointer")
        {
            isInsidePointer = true;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.gameObject.name == "PlayerPointer")
        {
            isInsidePointer = false;
        }
    }

    void Update()
    {

        if (isInsidePointer && keyboard.fKey.isPressed && Time.time>=nextUseTime)
        {
            if (keyboard == null) return;
            
            SceneManager.LoadScene("MapScene");

            nextUseTime = Time.time + interactionCooldown;
        }
    }

}
