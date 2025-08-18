using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;




public class ExitMenu : MonoBehaviour
{
    public
     static ExitMenu Instance;  // Singleton dostępny globalnie

    public GameObject menuOverlay;    // UI: tło przyciemnione + przycisk "Exit"
    public Button exitButton;         // przycisk "Exit"

    private Keyboard keyboard;

    public bool inputBlocked = false;

    public void SetCursor(bool unlocked)
    {
        Cursor.lockState = unlocked ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = unlocked;
    }

    void Awake()
    {
        // Ustawienie Singletona
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Debug.LogWarning("Istnieje więcej niż jeden ExitMenu w scenie!");
            Destroy(this);  // usuń zdublowany komponent
        }
    }

    void Start()
    {
        keyboard = Keyboard.current;
        if (keyboard == null)
            Debug.LogError("Brak klawiatury!");

        // Ukryj menu na starcie
        if (menuOverlay != null)
            menuOverlay.SetActive(false);

        // Przypisz akcję do przycisku
        if (exitButton != null)
            exitButton.onClick.AddListener(ExitGame);
    }

    void Update()
    {
        if (keyboard != null && keyboard.escapeKey.wasPressedThisFrame)
        {
            if (menuOverlay != null)
            {
                bool isNowActive = !menuOverlay.activeSelf;
                menuOverlay.SetActive(isNowActive);
                inputBlocked = isNowActive;  // blokuj/odblokuj wejście

                SetCursor(isNowActive);
                //Debug.Log("Ustawiam kursor: visible = " + isNowActive + ", lockState = " + (isNowActive ? "None" : "Locked"));
            }

        }
    }

    void ExitGame()
    {
        Debug.Log("Zamykanie gry...");
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;  // działa w edytorze
#endif
    }
}
