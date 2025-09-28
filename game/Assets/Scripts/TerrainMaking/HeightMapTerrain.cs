using UnityEngine;

public class HeightMapTerrain : MonoBehaviour
{
    private Texture2D terrainHeightMap1;
    private Texture2D terrainHeightMap2;   

    [Header("Terrain generating settings")]
    public float heightMultiplier = 20f; // Maksymalna wysokość gór

    public int height=256;
    public int width=256;

    public float terrainScale1 = 7f;
    public float terrainScale2 = 5f;

    public float offsetX = 100f;
    public float offsetY = 100f;

    [Range(0f, 1f)]
    public float weight1 = 0.25f;
    [Range(0f, 1f)]
    public float weight2 = 0.5f;

    void Start()
    {
        if (weight1+weight2 > 1)
        {
            float tooMuch = (weight1+weight2-1) / 2;
            weight1-=tooMuch;
            weight2-=tooMuch;
        }
        
        offsetX = Random.Range(0f, 9999f);
        offsetY = Random.Range(0f, 9999f);
        terrainHeightMap1 = PerlinNoise.GenerateTexture(terrainScale1, offsetX, offsetY, width, height);

        offsetX = Random.Range(0f, 9999f);
        offsetY = Random.Range(0f, 9999f);
        terrainHeightMap2 = PerlinNoise.GenerateTexture(terrainScale2, offsetX, offsetY, width, height);


        ApplyToTerrain(terrainHeightMap1, terrainHeightMap2);
        
    }

    void ApplyToTerrain(Texture2D givenHeightMap1, Texture2D givenHeightMap2)
    {
        Terrain terrain = GetComponent<Terrain>();
        TerrainData terrainData = terrain.terrainData;

        // break cases
        if (terrain == null) { Debug.LogError("Terrain component not loaded"); return; }
        if (givenHeightMap1 == null) Debug.Log("Height map 1 not loaded");
        if(givenHeightMap2 == null) Debug.Log("Height map 2 not loaded");
        if (givenHeightMap1 == null || givenHeightMap2 == null) return;
        

        float[,] heights = new float[height, width];

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float gray1 = givenHeightMap1.GetPixel(x, y).grayscale; // 0 = czarny, 1 = biały
                float gray2 = givenHeightMap2.GetPixel(x, y).grayscale; // 0 = czarny, 1 = biały
                heights[y, x] = gray1/4+gray2/2;
            }
        }

        terrainData.heightmapResolution = width;
        terrainData.size = new Vector3(width, heightMultiplier, height);
        terrainData.SetHeights(0, 0, heights);
    }

}
