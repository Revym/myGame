using UnityEngine;

public class TerrainFilling : MonoBehaviour
{
    [Header("Terrain filling settings")]
    public int xStart = 0;
    public int zStart = 0;
    public int xMax = 256;
    public int zMax = 256;
    public int xInstances = 256;
    public int zInstances = 256;

    [Header("Grass settings")]
    public float grassSize = 1.2f;

    public GameObject grassPrefab;

    private int spawnHeight = 50;

    private GameObject grassParent;

    void Start()
    {
        grassPrefab = Resources.Load<GameObject>("Models/grass/grass3p");
        //ClearGrass();
        //GenerateGrass();
    }

    [ContextMenu("Generate Grass")]
    public void GenerateGrass()
    {
        //ClearGrass();
        
        float xStep = (float)(xMax-xStart)/xInstances;
        float zStep = (float)(zMax-zStart)/zInstances;
        //Debug.Log("xStep = "+xStep+", zStep = "+zStep);

        grassParent = new GameObject("GrassParent");
        grassParent.transform.position = new Vector3(xStart, 0, zStart);

        for(float x = xStart; x<=xMax; x+=xStep)
        {
            for(float z = zStart; z<=zMax; z+=zStep)
            {
                Vector3 rayStart = new Vector3(x, spawnHeight, z);
                if (!Physics.Raycast(rayStart, Vector3.down, out RaycastHit hit, Mathf.Infinity)) continue;

                //Debug.Log("x "+x+", y "+hit.point.y+", z "+z);
                Vector3 spawnPosition = new Vector3(x, hit.point.y, z);
                GameObject grass = Instantiate(grassPrefab, spawnPosition, Quaternion.identity);
                grass.transform.SetParent(grassParent.transform);

            }
        }
    }

    [ContextMenu("Clear Grass")]
    public void ClearGrass()
    {
        if (grassParent!=null)
        {
            Destroy(grassParent);
            grassParent=null;
        }
    }

}