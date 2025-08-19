using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteAlways]
public class InstanceTintForHierarchy : MonoBehaviour
{
    [Header("Target material (searches in each renderer)")]
    [Tooltip("Fragment nazwy materiału który chcesz tintować (np. 'MetalColor'). Jeżeli pusty - użyje materialIndex.")]
    public string targetMaterialName = "MetalColor";

    [Tooltip("Jeżeli chcesz wskazać bezpośrednio indeks slota materiału (0 = pierwszy). -1 = auto-wyszukiwanie po nazwie.")]
    public int materialIndex = -1;

    [Header("Color")]
    public Color tint = Color.white;

    [Tooltip("Jeżeli Twój shader używa innej nazwy property, wpisz ją (np. _BaseColor dla URP, _Color dla Standard). Zostaw puste aby wykryć automatycznie.")]
    public string colorPropertyName = "";

    // cache
    MaterialPropertyBlock mpb;
    static readonly int baseColorID = Shader.PropertyToID("_BaseColor");
    static readonly int colorID = Shader.PropertyToID("_Color");

    void OnEnable()
    {
        ApplyToHierarchy();
    }

    void OnValidate()
    {
        // apply in editor when you change fields
        ApplyToHierarchy();
    }

    /// <summary>
    /// Przechodzi po wszystkich rendererach w drzewie i aplikuje kolor do znalezionego materiału.
    /// </summary>
    /// 
    public void ApplyToHierarchy()
    {
        if (mpb == null) mpb = new MaterialPropertyBlock();

        // zbierz wszystkie renderery na root i w children
        var renderers = new List<Renderer>();
        GetComponentsInChildren<Renderer>(true, renderers);

        if (renderers.Count == 0)
        {
            Debug.LogWarning($"[{name}] Nie znaleziono żadnych Renderer'ów w obiekcie i dzieciach.");
            return;
        }

        int changedCount = 0;

        foreach (var rend in renderers)
        {
            if (rend == null) continue;

            var mats = rend.sharedMaterials;
            if (mats == null || mats.Length == 0) continue;

            // spróbuj znaleźć index docelowego materiału dla tego renderera
            int idx = -1;
            if (materialIndex >= 0 && materialIndex < mats.Length) idx = materialIndex;
            else if (!string.IsNullOrEmpty(targetMaterialName))
            {
                for (int i = 0; i < mats.Length; i++)
                {
                    var m = mats[i];
                    if (m == null) continue;
                    if (m.name.IndexOf(targetMaterialName, System.StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        idx = i;
                        break;
                    }
                }
            }
            else if (mats.Length == 1)
            {
                idx = 0; // fallback: single material
            }

            if (idx < 0)
            {
                // nie znaleziono w tym rendererze — pomiń
                // Debug.Log($"[{name}] Renderer '{rend.name}' nie zawiera materiału '{targetMaterialName}'.");
                continue;
            }

            // upewnijmy się, że nazwa property dopasowana do materiału
            var mat = rend.sharedMaterials[idx];
            if (mat == null)
            {
                Debug.LogWarning($"[{name}] Renderer '{rend.name}' ma null w slocie {idx}.");
                continue;
            }

            int propID = DetermineColorProperty(mat);
            if (propID == -1)
            {
                Debug.LogWarning($"[{name}] Materiał '{mat.name}' w rendererze '{rend.name}' nie ma właściwości _BaseColor ani _Color. Podaj colorPropertyName albo użyj innego shaderu.");
                continue;
            }

            // Pobieramy istniejący block (dla tego slota, jeśli API wspiera)
            bool usedIndexedSet = false;
            try
            {
                rend.GetPropertyBlock(mpb, idx);
                mpb.SetColor(propID, tint);
                rend.SetPropertyBlock(mpb, idx);
                usedIndexedSet = true;
            }
            catch
            {
                // starsze Unity mogą nie mieć overloadów dla slot-index — fallback poniżej
            }

            if (!usedIndexedSet)
            {
                // fallback — ustaw blok globalny dla renderera (wpłynie na wszystkie sloty)
                rend.GetPropertyBlock(mpb);
                mpb.SetColor(propID, tint);
                rend.SetPropertyBlock(mpb);
            }

            changedCount++;
            Debug.Log($"[{name}] Zmieniono kolor w rendererze '{rend.name}' materiał '{mat.name}' (slot {idx}) na {tint}.");
        }

        Debug.Log($"[{name}] ApplyToHierarchy: przetworzono {renderers.Count} rendererów, zmieniono kolory w {changedCount} rendererach.");
    }

    int DetermineColorProperty(Material mat)
    {
        if (!string.IsNullOrEmpty(colorPropertyName))
        {
            int id = Shader.PropertyToID(colorPropertyName);
            if (mat.HasProperty(id)) return id;
            // jeśli podana nazwa nie działa, ostrzeż:
            Debug.LogWarning($"[{name}] Podana nazwa property '{colorPropertyName}' nie istnieje w materiale '{mat.name}'.");
            // dalej spróbujemy auto
        }
        if (mat.HasProperty(baseColorID)) return baseColorID;
        if (mat.HasProperty(colorID)) return colorID;
        return -1;
    }

    // runtime API
    public void SetColor(Color c)
    {
        tint = c;
        ApplyToHierarchy();
    }
}
