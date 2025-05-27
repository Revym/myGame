using UnityEngine;
using UnityEngine.InputSystem;

public class ColorSwitcher : MonoBehaviour
{
    // Kolory do wyboru
    public Color color1 = Color.red;
    public Color color2 = Color.green;
    public Color color3 = Color.blue;

    // Referencja do Rendereru kapsuły
    private Renderer rend;

    void Start()
    {
        // Pobieramy komponent Renderer z obiektu
        rend = GetComponent<Renderer>();
        if (rend == null)
        {
            Debug.LogError("Brak komponentu Renderer na tym obiekcie!");
        }
    }

    void Update()
    {
        var keyboard = Keyboard.current;
        if (keyboard == null) return;

        if (keyboard.digit1Key.wasPressedThisFrame)
            rend.material.color = color1;
        else if (keyboard.digit2Key.wasPressedThisFrame)
            rend.material.color = color2;
        else if (keyboard.digit3Key.wasPressedThisFrame)
            rend.material.color = color3;
    }
}
