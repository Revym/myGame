using UnityEngine;

public class GameManager : MonoBehaviour
{
    private GrassManager grassManager;
    private TerrainMakingManager terrainMakingManager;
    private TreeManager treeManager;
    [SerializeField] private GameObject terrainParent;

    void Start()
    {
        grassManager = GetComponent<GrassManager>();
        terrainMakingManager = GetComponent<TerrainMakingManager>();
        treeManager = GetComponent<TreeManager>();

        // generating terrain making settings
        terrainMakingManager.GenerateParameters();

        // invokes the method for terrain generation
        generateTerrain();

        // invokes the methos of GrassManager script, responsible for instantiating given types of grass
        grassManager.Load();
        grassManager.GenerateGrass();

        treeManager.Load();
        treeManager.GenerateForest();
    }

    void generateTerrain() // begins terrain generating, by iterating through all Terrain instances and invoking terrain making method
    {
        if (terrainParent == null)
        {
            Debug.LogError("terrainParent == null");
            return;
        }

        // iterates through all children of Terrains object
        foreach (Transform child in terrainParent.transform)
        {
            TilingHeightMapTerrain script = child.GetComponent<TilingHeightMapTerrain>();

            if (script != null)
            {
                script.ApplyToTerrain();
            }
        }
    }
}
