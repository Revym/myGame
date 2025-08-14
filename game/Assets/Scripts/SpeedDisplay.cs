using UnityEngine;
using UnityEngine.UI;
using TMPro;


[RequireComponent(typeof(CharacterController))]
public class SpeedDisplay : MonoBehaviour
{
    public TMP_Text speedText;        // Przypisz UI Text z Canvas
    private CharacterController cc;

    void Start()
    {
        cc = GetComponent<CharacterController>();
        if (speedText == null)
            Debug.LogError("Przypisz Text w inspektorze do SpeedDisplay.speedText");
    }

    void Update()
    {
        // Pobieramy prędkość poziomą (bez y)
        Vector3 horizontalVelocity = new Vector3(cc.velocity.x, 0f, cc.velocity.z);
        float speed = horizontalVelocity.magnitude;

        // Wyświetlamy prędkość z dwoma miejscami po przecinku
        speedText.text = $"Speed: {speed:F2}";
    }
}
