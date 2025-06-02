using UnityEngine;
using UnityEngine.UI;

public class SimpleCrosshair : MonoBehaviour
{
    void Start()
    {
        GameObject canvasGO = new GameObject("CrosshairCanvas");
        canvasGO.AddComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;

        GameObject crosshair = new GameObject("Crosshair");
        crosshair.transform.SetParent(canvasGO.transform);

        Text text = crosshair.AddComponent<Text>();
        text.text = "+";
        text.alignment = TextAnchor.MiddleCenter;
        text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        text.color = Color.white;
        text.fontSize = 32;

        RectTransform rt = text.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta = new Vector2(32, 32);
    }
}
