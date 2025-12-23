using System.Collections.Generic;
using UnityEngine;

public class TreeManager : MonoBehaviour
{
    private class TreeChunk
    {
        public List<Matrix4x4> matrices = new List<Matrix4x4>();
        public Bounds bounds;
        private bool boundsInitialized = false;

        public void AddInstance(Vector3 pos, Quaternion rot, Vector3 scale)
        {
            matrices.Add(Matrix4x4.TRS(pos, rot, scale));
            Vector3 treeTop = pos + Vector3.up * 10f; 

            if (!boundsInitialized)
            {
                bounds = new Bounds(pos, Vector3.one * 1f);
                bounds.Encapsulate(treeTop);
                boundsInitialized = true;
            }
            else
            {
                bounds.Encapsulate(pos);
                bounds.Encapsulate(treeTop);
            }
        }
    }

    [Header("Ustawienia Drzewa (LOD 0 - Wysoka jakość)")]
    public string treeFileName = "pineTree4";
    private Mesh treeMesh;
    private Material[] treeMaterials;

    [Header("Ustawienia Drzewa (LOD 1 - Niska jakość)")]
    public string lod1FileName = "pineTreeLOD1";
    public float lodTransitionDistance = 100f; // Dystans, przy którym zmieniamy model
    private Mesh lodMesh;
    private Material[] lodMaterials;

    [Header("Ustawienia Lasu")]
    private int height;
    private int width;
    public bool randomize = false;
    public float treeDensity = 5.0f;
    [Range(0, 1)] public float forestThreshold = 0.4f;
    public float mapScale = 4f;
    private Texture2D map;
    [Range(0f, 10f)] public float randomOffset = 1f;

    private int offsetX;
    private int offsetZ;
    private int spawnHeight = 100;
    public float heightOffset = 2.75f;

    [Header("Culling Settings")]
    public Camera playerCamera;
    public float renderDistance = 350f; 
    public int chunkSize = 64;

    [Header("Debug")]
    public bool debugRenderAll = false;

    private Dictionary<Vector2Int, TreeChunk> chunks = new Dictionary<Vector2Int, TreeChunk>();
    private Plane[] frustumPlanes;
    private const int BATCH_SIZE = 1023;
    private Matrix4x4[] cachedBatch = new Matrix4x4[BATCH_SIZE];

    public void Load()
    {
        if (playerCamera == null) playerCamera = Camera.main;
        frustumPlanes = new Plane[6];

        // Wczytywanie modeli
        LoadTreeData("Models/tree/pine tree/" + treeFileName, out treeMesh, out treeMaterials);
        LoadTreeData("Models/tree/pine tree/" + lod1FileName, out lodMesh, out lodMaterials);
        
        width = TerrainMakingManager.Instance.totalWidth;
        height = TerrainMakingManager.Instance.totalHeight;
        offsetX = Random.Range(0, 9999);
        offsetZ = Random.Range(0, 9999);
        map = PerlinNoise.GenerateTexture(mapScale, offsetX, offsetZ, width, height);

        if (randomize)
        {
            treeDensity = Random.Range(5f, 9f);
            mapScale = Random.Range(2f, 5f);
            forestThreshold = Random.Range(0.35f, 0.7f);
        }
    }

    // Pomocnicza metoda do wczytywania danych modelu
    private void LoadTreeData(string path, out Mesh mesh, out Material[] materials)
    {
        var prefab = Resources.Load<GameObject>(path);
        if (prefab == null) { Debug.LogError($"Failed to load tree from {path}"); mesh = null; materials = null; return; }

        MeshFilter mf = prefab.GetComponentInChildren<MeshFilter>();
        MeshRenderer mr = prefab.GetComponentInChildren<MeshRenderer>();

        mesh = mf.sharedMesh;
        materials = mr.sharedMaterials;

        foreach (var mat in materials)
        {
            if (!mat.enableInstancing) mat.enableInstancing = true;
        }
    }

    public void GenerateForest()
    {
        chunks.Clear();
        for (float x = 0; x < width; x += treeDensity)
        {
            for (float z = 0; z < height; z += treeDensity)
            {
                int posX = Mathf.FloorToInt(x);
                int posZ = Mathf.FloorToInt(z);
                if (posX >= map.width || posZ >= map.height) continue;
                
                float noiseVal = map.GetPixel(posX, posZ).grayscale;
                if (noiseVal < forestThreshold + Random.Range(-0.1f, 0.1f)) continue;

                float x2 = posX + Random.Range(-randomOffset, randomOffset);
                float z2 = posZ + Random.Range(-randomOffset, randomOffset);
                Vector3 rayStart = new Vector3(x2, spawnHeight, z2);

                if (Physics.Raycast(rayStart, Vector3.down, out RaycastHit hit, Mathf.Infinity))
                {
                    if (hit.collider.GetComponent<Terrain>() != null)
                    {
                        float y = hit.point.y + heightOffset;
                        Vector3 position = new Vector3(x2, y, z2);
                        int chunkX = Mathf.FloorToInt(position.x / chunkSize);
                        int chunkZ = Mathf.FloorToInt(position.z / chunkSize);
                        Vector2Int chunkCoord = new Vector2Int(chunkX, chunkZ);

                        if (!chunks.TryGetValue(chunkCoord, out TreeChunk chunk))
                        {
                            chunk = new TreeChunk();
                            chunks[chunkCoord] = chunk;
                        }
                        chunk.AddInstance(position, Quaternion.Euler(0, Random.Range(0, 360), 0), Vector3.one * Random.Range(0.8f, 1.5f));
                    }
                }
            }
        }
    }

    void Update()
    {
        if (chunks.Count == 0 || playerCamera == null) return;

        GeometryUtility.CalculateFrustumPlanes(playerCamera, frustumPlanes);
        Vector3 cameraPos = playerCamera.transform.position;

        foreach (TreeChunk chunk in chunks.Values)
        {
            if (!debugRenderAll)
            {
                if (!GeometryUtility.TestPlanesAABB(frustumPlanes, chunk.bounds)) continue;
                float distSq = chunk.bounds.SqrDistance(cameraPos); 
                if (distSq > renderDistance * renderDistance) continue;

                // --- LOGIKA WYBORU LOD ---
                bool useLOD1 = distSq > lodTransitionDistance * lodTransitionDistance;
                RenderChunk(chunk, useLOD1);
            }
            else
            {
                RenderChunk(chunk, false);
            }
        }
    }

    private void RenderChunk(TreeChunk chunk, bool useLOD)
    {
        Mesh targetMesh = useLOD ? lodMesh : treeMesh;
        Material[] targetMaterials = useLOD ? lodMaterials : treeMaterials;

        for (int i = 0; i < chunk.matrices.Count; i += BATCH_SIZE)
        {
            int count = Mathf.Min(BATCH_SIZE, chunk.matrices.Count - i);
            chunk.matrices.CopyTo(i, cachedBatch, 0, count);

            for (int m = 0; m < targetMesh.subMeshCount; m++)
            {
                if (m < targetMaterials.Length)
                {
                    Graphics.DrawMeshInstanced(targetMesh, m, targetMaterials[m], cachedBatch, count, null, UnityEngine.Rendering.ShadowCastingMode.On, true);
                }
            }
        }
    }
}