using UnityEngine;

public class HeightMapTerrain : MonoBehaviour
{
    public Texture2D heightMap;   
    public float heightMultiplier = 10f; // Maksymalna wysokość gór

    
    [Header("Terrain generating settings")]
    public int height=256;
    public int width=256;

    public float scale = 7f;

    public float offsetX = 100f;
    public float offsetY = 100f;

    void Start()
    {
        offsetX = Random.Range(0f, 999f);
        offsetY = Random.Range(0f, 999f);
        
        heightMap = GenerateTexture();

        Terrain terrain = GetComponent<Terrain>();
        TerrainData terrainData = terrain.terrainData;

        if(heightMap == null) Debug.Log("Height map not loaded");

        int width = heightMap.width;
        int height = heightMap.height;

        float[,] heights = new float[height, width];

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float gray = heightMap.GetPixel(x, y).grayscale; // 0 = czarny, 1 = biały
                heights[y, x] = gray;
            }
        }

        terrainData.heightmapResolution = width;
        terrainData.size = new Vector3(width, heightMultiplier, height);
        terrainData.SetHeights(0, 0, heights);
    }

    


    // Perlin noise height map generation functions

    Texture2D GenerateTexture()
    {
        Texture2D texture = new Texture2D(width, height);

        for(int x=0; x<width; x++)
        {
            for(int y=0; y<height; y++)
            {
                Color color = CalculateColor(x,y);
                texture.SetPixel(x,y,color);
            }
        }

        texture.Apply();
        return texture;
    }

    Color CalculateColor(int x, int y)
    {
        float xCoord = (float)x/width * scale + offsetX;
        float yCoord = (float)y/height * scale + offsetY;

        float sample = Mathf.PerlinNoise(xCoord,yCoord);
        return new Color(sample, sample, sample);
    }
}
