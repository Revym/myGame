using UnityEngine;

public class Target : MonoBehaviour
{
    private Renderer rend;
    private Color originalColor;
    public Color hitColor = Color.red;
    public float hitDuration = 0.4f;

    private void Start()
    {
        rend = GetComponent<Renderer>();
        originalColor = rend.material.color;
    }

    public void Hit()
    {
        StopAllCoroutines(); // Żeby nie nałożyły się zmiany kolorów
        StartCoroutine(FlashColor());
    }

    private System.Collections.IEnumerator FlashColor()
    {
        rend.material.color = hitColor;
        yield return new WaitForSeconds(hitDuration);
        rend.material.color = originalColor;
    }
}
