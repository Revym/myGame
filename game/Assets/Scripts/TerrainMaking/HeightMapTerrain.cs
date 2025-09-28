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
        terrainHeightMap1 = GenerateTexture(terrainScale1, offsetX, offsetY);

        offsetX = Random.Range(0f, 9999f);
        offsetY = Random.Range(0f, 9999f);
        terrainHeightMap2 = GenerateTexture(terrainScale2, offsetX, offsetY);


        ApplyToTerrain(terrainHeightMap1, terrainHeightMap2);
        
    }

    void ApplyToTerrain(Texture2D givenHeightMap1, Texture2D givenHeightMap2)
    {
        Terrain terrain = GetComponent<Terrain>();
        TerrainData terrainData = terrain.terrainData;
        if (terrain == null) { Debug.LogError("Terrain component not loaded"); return; }
        
        if(givenHeightMap1 == null) Debug.Log("Height map 1 not loaded");
        if(givenHeightMap2 == null) Debug.Log("Height map 2 not loaded");
        if (givenHeightMap1 == null || givenHeightMap2 == null) return;
        
        //int width = givenHeightMap1.width;
        //int height = givenHeightMap1.height;

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


    // Perlin noise height map generation functions

    Texture2D GenerateTexture(float scale, float offsetX, float offsetY)
    {
        Texture2D texture = new Texture2D(width, height);

        for(int x=0; x<width; x++)
        {
            for(int y=0; y<height; y++)
            {
                Color color = CalculateColor(x,y, scale, offsetX, offsetY);
                texture.SetPixel(x,y,color);
            }
        }

        texture.Apply();
        return texture;
    }

    Color CalculateColor(int x, int y, float scale, float offsetX, float offsetY)
    {
        float xCoord = (float)x/width * scale + offsetX;
        float yCoord = (float)y/height * scale + offsetY;

        float sample = Mathf.PerlinNoise(xCoord,yCoord);
        return new Color(sample, sample, sample);
    }
}
