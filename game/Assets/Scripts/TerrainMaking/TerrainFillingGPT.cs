using UnityEngine;
using System.Collections.Generic;
// GPU instancing (to learn)

public class TerrainFillingGPU : MonoBehaviour
{
    [Header("Terrain filling settings")]
    public int xStart = 0;
    public int zStart = 0;
    public int xMax = 256;
    public int zMax = 256;
    public int xInstances = 256;
    public int zInstances = 256;

    [Header("Grass settings")]
    public float minGrassSize = 1.2f;
    public float maxGrassSize = 1.5f;
    public float grassScale = 7f;
    [Range(0f, 4f)]
    public float grassSideOffset = 0.5f;
    public float lowerGrassMult = 1.5f;
    private const int BATCH_SIZE = 1023;

    private int spawnHeight = 50;
    private Texture2D grassHeightMap;

    public List<string> grassPrefabPaths = new List<string>
    {
        "grass1",
        "grass3"
    };
    private List<GrassType> grassTypes = new List<GrassType>();
    private Dictionary<GrassType, List<Matrix4x4>> matricesByType = new Dictionary<GrassType, List<Matrix4x4>>();


    void Start()
    {
        LoadGrassTypes();

        float offsetX = Random.Range(0f, 9999f);
        float offsetY = Random.Range(0f, 9999f);
        int height = xInstances;
        int width = zInstances;
        grassHeightMap = PerlinNoise.GenerateTexture(grassScale, offsetX, offsetY, width, height);

        wait(0.5f);
        GenerateGrass();
        
    }

    private void wait(float time)
    {
        time -= Time.deltaTime;
        if (time <= 0f)
        {
            return;
        }
    }

    void Update()
    {
        if (matricesByType.Count == 0) return;

        foreach (var kvp in matricesByType)
        {
            GrassType type = kvp.Key;
            List<Matrix4x4> matrices = kvp.Value;
            if (matrices.Count == 0) continue;

            for (int i = 0; i < matrices.Count; i += BATCH_SIZE)
            {
                int count = Mathf.Min(BATCH_SIZE, matrices.Count - i);
                Matrix4x4[] batch = new Matrix4x4[count];
                matrices.CopyTo(i, batch, 0, count);

                Graphics.DrawMeshInstanced(type.mesh, 0, type.material, batch, count);
            }
        }
    }


    [ContextMenu("Generate Grass")]
    public void GenerateGrass()
    {
        if (grassHeightMap == null) Debug.Log("grassHeightMap == null");
        
        matricesByType.Clear();

        float xStep = (float)(xMax - xStart) / xInstances;
        float zStep = (float)(zMax - zStart) / zInstances;

        for (float x = xStart; x <= xMax; x += xStep)
        {
            for (float z = zStart; z <= zMax; z += zStep)
            {
                // side-to-side random offset
                float offsetX = Random.Range(-grassSideOffset, grassSideOffset);
                float offsetZ = Random.Range(-grassSideOffset, grassSideOffset);

                // setting the start point of a raycast for grass instantiating
                Vector3 rayStart = new Vector3(x + offsetX, spawnHeight, z + offsetZ);

                if (!Physics.Raycast(rayStart, Vector3.down, out RaycastHit hit, Mathf.Infinity)) continue;
                if (hit.collider.GetComponent<Terrain>() == null) continue;

                // value for lower based on designated height map
                float lower = grassHeightMap.GetPixel((int)x, (int)z).grayscale * lowerGrassMult;
                if (lower >= 1) continue; // means: dont instantiate grass if too low

                // instatiating position
                Vector3 pos = new Vector3(x + offsetX, hit.point.y - lower, z + offsetZ);

                // losowy obrót wokół Y i losowa skala (dla naturalności)
                Quaternion rot = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
                float s = Random.Range(minGrassSize, maxGrassSize);
                Vector3 scale = Vector3.one * s;

                // choosing a random grass type {To-do}
                GrassType type = grassTypes[Random.Range(0, grassTypes.Count)];

                //matrices.Add(Matrix4x4.TRS(pos, rot, scale));
                if (!matricesByType.ContainsKey(type))
                    matricesByType[type] = new List<Matrix4x4>();

                matricesByType[type].Add(Matrix4x4.TRS(pos, rot, scale));
            }
        }

        //int totalInstances = 0;
        //foreach (var list in matricesByType.Values) totalInstances += list.Count;
        //Debug.Log("Instances to render: " + totalInstances);
    }


    [ContextMenu("Clear Grass")]
    public void ClearGrass()
    {
        matricesByType.Clear();
    }

    private void LoadGrassTypes()
    {
        grassTypes.Clear();

        foreach (var prefabName in grassPrefabPaths)
        {
            // loading model prefab from name
            var prefab = Resources.Load<GameObject>("Models/grass/" + prefabName);
            if (prefab == null) { Debug.Log("Didnt find prefab "+prefabName); continue; }

            // loading model meshfilter from prefab
            var mf = prefab.GetComponent<MeshFilter>();
            if (mf == null) { Debug.Log("Couldnt load MeshFilter for "+prefabName); continue; }

            // loading model meshrenderer from prefab
            var mr = prefab.GetComponent<MeshRenderer>();
            if (mr == null) { Debug.Log("Couldnt load MeshRenderer for "+prefabName); continue; }

            // adding the GrassType variable to our grassTypes list
            grassTypes.Add(new GrassType
            {
                resourceName = prefabName,
                prefab = prefab,
                mesh = mf.sharedMesh,
                material = mr.sharedMaterial
            });
        }

        //Debug.Log($"Załadowano {grassTypes.Count} typów trawy");
    }
}
