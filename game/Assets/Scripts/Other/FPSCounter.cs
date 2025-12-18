using UnityEngine;

public class FPSCounter : MonoBehaviour
{
    float deltaTime;
    public int fontSize = 24;
    public Color fontColor = Color.white;

    void Update()
    {
        deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
    }

    void OnGUI()
    {
        GUIStyle style = new GUIStyle();
        style.fontSize = fontSize;
        style.normal.textColor = fontColor;

        float fps = 1.0f / deltaTime;
        GUI.Label(new Rect(10, 10, 200, 50), $"FPS: {Mathf.Ceil(fps)}", style);
    }
}
