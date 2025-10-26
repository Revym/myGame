using UnityEngine;

public class TilingHeightMapTerrain : MonoBehaviour
{
    private Texture2D heightMap1;
    private Texture2D heightMap2;

    private int baseOffsetX;
    private int baseOffsetZ;

    private int height;
    private int width;

    private float heightMultiplier;

    private float weight1;
    private float weight2;

    private float power1;
    private float power2;
    
    void Start()
    {
        heightMap1 = TerrainMakingManager.Instance.heightMap1;
        heightMap2 = TerrainMakingManager.Instance.heightMap2;

        height = TerrainMakingManager.Instance.height;
        width = TerrainMakingManager.Instance.width;
        heightMultiplier = TerrainMakingManager.Instance.heightMultiplier;

        baseOffsetX = (int)transform.position.x;
        baseOffsetZ = (int)transform.position.z;

        weight1 = TerrainMakingManager.Instance.weight1;
        weight2 = TerrainMakingManager.Instance.weight2;

        power1 = TerrainMakingManager.Instance.power1;
        power2 = TerrainMakingManager.Instance.power2;

        ApplyToTerrain(heightMap1, heightMap2);
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
        

        float[,] heights = new float[height+1, width+1];

        for (int y = 0; y < height+1; y++)
        {
            for (int x = 0; x < width+1; x++)
            {
                float gray1 = givenHeightMap1.GetPixel(x + baseOffsetX, y + baseOffsetZ).grayscale; // 0 = czarny, 1 = biały
                float gray2 = givenHeightMap2.GetPixel(x + baseOffsetX, y + baseOffsetZ).grayscale; // 0 = czarny, 1 = biały

                gray1 = Mathf.Pow(gray1, power1);
                gray2 = Mathf.Pow(gray2, power2);

                heights[y, x] = gray1*weight1+gray2*weight2;
            }
        }

        terrainData.heightmapResolution = width;
        terrainData.size = new Vector3(width, heightMultiplier, height);
        terrainData.SetHeights(0, 0, heights);
    }
}
