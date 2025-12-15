using UnityEngine;

public class TerrainMakingManager : MonoBehaviour
{
    public static TerrainMakingManager Instance;
    public Texture2D heightMap1;
    public Texture2D heightMap2;

    [Header("Map settings")]
    public int height = 256;
    public int width = 256;
    [Tooltip("Sets terrain generation values to random, from hard set sets")]
    public bool random = false;

    public int totalHeight = 514;
    public int totalWidth = 514;
    
    public float heightMultiplier = 20f;

    public float terrainScale1 = 7f;
    public float terrainScale2 = 5f;

    public float offsetX = 100f;
    public float offsetY = 100f;

    public float power1 = 2;
    public float power2 = 1;

    [Range(0f, 1f)]
    public float weight1 = 0.25f;
    [Range(0f, 1f)]
    public float weight2 = 0.5f;

    
    public void GenerateParameters() // generates terrain making parameters for TilingHeightMapTerrain scripts
    {
        Instance = this;

        if (random) {RandomizeValues();}

        if (weight1 + weight2 > 1)
        {
            float tooMuch = (weight1 + weight2 - 1) / 2;
            weight1 -= tooMuch;
            weight2 -= tooMuch;
        }

        offsetX = Random.Range(0f, 9999f);
        offsetY = Random.Range(0f, 9999f);
        heightMap1 = PerlinNoise.GenerateTexture(terrainScale1, offsetX, offsetY, totalWidth, totalHeight);

        offsetX = Random.Range(0f, 9999f);
        offsetY = Random.Range(0f, 9999f);
        heightMap2 = PerlinNoise.GenerateTexture(terrainScale2, offsetX, offsetY, totalWidth, totalHeight);
    }

    public void RandomizeValues()
    {
        heightMultiplier = Random.Range(40, 100);

        terrainScale1 = Random.Range(6f, 8f);
        terrainScale2 = Random.Range(1.5f, 2.5f);

        power1 = Random.Range(0.5f, 1.25f);
        power2 = Random.Range(1f, 2f);

        weight1 = Random.Range(0.1f, 0.25f);
        weight2 = Random.Range(0.75f, 0.9f);
    }

}
