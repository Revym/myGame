using System.Collections.Generic;
using UnityEngine;

public class TreeManager : MonoBehaviour
{
    // --- KLASA WEWNĘTRZNA DLA CHUNKÓW (wzór z GrassManager) ---
    private class TreeChunk
    {
        public List<Matrix4x4> matrices = new List<Matrix4x4>();
        public Bounds bounds;
        private bool boundsInitialized = false;

        public void AddInstance(Vector3 pos, Quaternion rot, Vector3 scale)
        {
            matrices.Add(Matrix4x4.TRS(pos, rot, scale));

            // Rozszerzamy Bounds chunka o nowe drzewo (zakładamy, że drzewo ma max 10m wysokości dla bezpieczeństwa bounds)
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

    [Header("Ustawienia Drzewa")]
    public Mesh treeMesh;
    public Material[] treeMaterials;

    [Header("Ustawienia Lasu")]
    private int height;
    private int width;
    public bool randomize = false;
    public float treeDensity = 5.0f;
    public float noiseScale = 0.05f;
    [Range(0, 1)] public float forestThreshold = 0.4f;
    public float mapScale = 4f;
    private Texture2D map;
    [Range(0f, 10f)] public float randomOffset = 1f;

    private int offsetX;
    private int offsetZ;
    private int spawnHeight = 100;

    [Header("Culling Settings")]
    public Camera playerCamera;
    public float renderDistance = 350f; 
    public int chunkSize = 64;

    [Header("Debug")]
    [Tooltip("Rysuje cały las, ignorując kamerę i odległość. UWAGA: Może obniżyć FPS!")]
    public bool debugRenderAll = false;

    private Dictionary<Vector2Int, TreeChunk> chunks = new Dictionary<Vector2Int, TreeChunk>();
    private Plane[] frustumPlanes;
    private const int BATCH_SIZE = 1023;

    public void Load()
    {
        if (playerCamera == null)
        {
            // Próba auto-przypisania kamery, jeśli nie ustawiona
            playerCamera = Camera.main;
            if (playerCamera == null) Debug.LogError("Brak przypisanej kamery w TreeManager!");
        }

        // Inicjalizacja tablicy płaszczyzn
        frustumPlanes = new Plane[6];

        var prefab = Resources.Load<GameObject>("Models/tree/pine tree/pineTree2");
        
        if (prefab == null) { Debug.LogError("Failed to load tree"); return; }

        MeshFilter mf = prefab.GetComponentInChildren<MeshFilter>();
        MeshRenderer mr = prefab.GetComponentInChildren<MeshRenderer>();

        if (mf == null || mr == null)
        {
            Debug.LogError("BŁĄD: Prefab nie ma komponentów Mesh!");
            return;
        }

        // Upewniamy się, że materiały mają włączony instancing
        foreach(var mat in mr.sharedMaterials)
        {
            if(!mat.enableInstancing) mat.enableInstancing = true;
        }

        treeMesh = mf.sharedMesh;
        treeMaterials = mr.sharedMaterials;
        
        width = TerrainMakingManager.Instance.totalWidth;
        height = TerrainMakingManager.Instance.totalHeight;

        offsetX = Random.Range(0, 9999);
        offsetZ = Random.Range(0, 9999);

        map = PerlinNoise.GenerateTexture(mapScale, offsetX, offsetZ, width, height);

        if (randomize)
        {
            treeDensity = Random.Range(5f, 9f);
            mapScale = Random.Range(2f,5f);
            forestThreshold = Random.Range(0.4f, 0.7f);
        }
    }

    public void GenerateForest()
    {
        chunks.Clear(); // Czyścimy stare chunki
        
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

                if (!Physics.Raycast(rayStart, Vector3.down, out RaycastHit hit, Mathf.Infinity)) continue;
                if (hit.collider.GetComponent<Terrain>() == null) continue;

                float y = hit.point.y;

                Vector3 position = new Vector3(x2, y, z2);
                Quaternion rotation = Quaternion.Euler(0, Random.Range(0, 360), 0);
                float scaleVal = Random.Range(0.8f, 1.5f);
                Vector3 scale = Vector3.one * scaleVal;

                // --- LOGIKA CHUNKÓW ---
                // Obliczamy do którego "kwadratu" (chunka) należy to drzewo
                int chunkX = Mathf.FloorToInt(position.x / chunkSize);
                int chunkZ = Mathf.FloorToInt(position.z / chunkSize);
                Vector2Int chunkCoord = new Vector2Int(chunkX, chunkZ);

                // Pobieramy lub tworzymy nowy chunk
                if (!chunks.TryGetValue(chunkCoord, out TreeChunk chunk))
                {
                    chunk = new TreeChunk();
                    chunks[chunkCoord] = chunk;
                }

                // Dodajemy drzewo do chunka
                chunk.AddInstance(position, rotation, scale);
            }
        }
        Debug.Log($"Wygenerowano las: {chunks.Count} chunków.");
    }

    void Update()
    {
        if (chunks.Count == 0 || playerCamera == null) return;
        if (!debugRenderAll && playerCamera == null) return;

        Vector3 cameraPosition = Vector3.zero;

        // Obliczamy frustum tylko jeśli jest potrzebne (nie jesteśmy w trybie debugRenderAll)
        if (!debugRenderAll && playerCamera != null)
        {
            GeometryUtility.CalculateFrustumPlanes(playerCamera, frustumPlanes);
            cameraPosition = playerCamera.transform.position;
        }

        foreach (TreeChunk chunk in chunks.Values)
        {
            if (!debugRenderAll)
            {
                // A. Frustum Culling: Czy chunk jest w ogóle w kadrze?
                if (!GeometryUtility.TestPlanesAABB(frustumPlanes, chunk.bounds)) continue;
    
                // B. Distance Culling: Czy chunk jest wystarczająco blisko?
                float distanceToChunk = chunk.bounds.SqrDistance(cameraPosition); 
                // Używamy SqrDistance dla wydajności (porównujemy z kwadratem odległości)
                if (distanceToChunk > renderDistance * renderDistance) continue;
            }

            // C. Renderowanie (dzielenie na paczki po 1023)
            List<Matrix4x4> matrices = chunk.matrices;
            
            for (int i = 0; i < matrices.Count; i += BATCH_SIZE)
            {
                int count = Mathf.Min(BATCH_SIZE, matrices.Count - i);
                
                // Tutaj niestety musimy stworzyć tymczasową tablicę dla DrawMeshInstanced
                // (To jest standardowy koszt GPU Instancingu w Unity bez ComputeBufferów)
                Matrix4x4[] batch = new Matrix4x4[count];
                matrices.CopyTo(i, batch, 0, count);

                // Rysujemy WSZYSTKIE materiały (pień + liście)
                for (int m = 0; m < treeMesh.subMeshCount; m++)
                {
                    if (m < treeMaterials.Length)
                    {
                        Graphics.DrawMeshInstanced(
                            treeMesh, 
                            m, 
                            treeMaterials[m], 
                            batch, 
                            count,
                            null,
                            UnityEngine.Rendering.ShadowCastingMode.On, // Włączamy cienie dla drzew
                            true
                        );
                    }
                }
            }
        }
    }
}