using UnityEngine;

[System.Serializable]
public class GrassType
{
    public string resourceName;   // nazwa modelu
    public GameObject prefab;     // sam prefab
    public Mesh mesh;             // wyciągnięty mesh
    public Material material;     // wyciągnięty materiał
}
