using UnityEngine;
using System.Collections.Generic;

// POPRAWKA: Nazwa klasy zmieniona na GrassManager
public class GrassManager : MonoBehaviour
{
    // POPRAWKA BŁĘDÓW 2 i 3:
    // Klasa GrassChunk jest teraz zdefiniowana WEWNĄTRZ GrassManager.
    // Dzięki temu może być 'private' i jest dostępna dla GrassManagera.
    private class GrassChunk
    {
        public Dictionary<GrassType, List<Matrix4x4>> matricesByType = new Dictionary<GrassType, List<Matrix4x4>>();
        public Bounds bounds;
        private bool boundsInitialized = false;

        // POPRAWKA: Usunęliśmy statyczne pole 'maxGrassSize'.
        // Zamiast tego, będziemy przekazywać maksymalną skalę w metodzie AddInstance,
        // ponieważ ta klasa nie ma bezpośredniego dostępu do pól GrassManagera.
        public void AddInstance(GrassType type, Vector3 pos, Quaternion rot, Vector3 scale, float maxScaleValue)
        {
            if (!matricesByType.ContainsKey(type))
            {
                matricesByType[type] = new List<Matrix4x4>();
            }
            matricesByType[type].Add(Matrix4x4.TRS(pos, rot, scale));

            // Używamy przekazanej wartości maxScaleValue
            Vector3 grassTopPoint = pos + rot * (Vector3.up * scale.y * maxScaleValue); 

            if (!boundsInitialized)
            {
                bounds = new Bounds(pos, Vector3.one * 0.1f);
                bounds.Encapsulate(grassTopPoint);
                boundsInitialized = true;
            }
            else
            {
                bounds.Encapsulate(pos);
                bounds.Encapsulate(grassTopPoint);
            }
        }
    }
    // Koniec definicji klasy GrassChunk

    
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

    [Header("Culling settings")]
    public int chunkSize = 32;
    // POPRAWKA: Publiczne pole na kamerę gracza.
    // Pamiętaj, aby przeciągnąć obiekt "PlayerCamera" na to pole w inspektorze!
    public Camera playerCamera; 

    private int spawnHeight = 50;
    private Texture2D grassHeightMap;
    [Header("Render distance")]
    public float renderDistance = 60f;

    public List<string> grassPrefabPaths = new List<string>
    {
        "grass1",
        "grass3"
    };
    
    // ZMIANA: Ta lista będzie teraz używać Twojej zewnętrznej klasy GrassType
    private List<GrassType> grassTypes = new List<GrassType>();
    
    // Ten słownik używa naszej zagnieżdżonej klasy GrassChunk i jest już OK
    private Dictionary<Vector2Int, GrassChunk> chunks = new Dictionary<Vector2Int, GrassChunk>();
    
    private Plane[] frustumPlanes;

    void Start()
    {
        // ZMIANA: Sprawdzamy publiczne pole 'playerCamera'
        if (playerCamera == null)
        {
            Debug.LogError("Kamera gracza (playerCamera) nie jest ustawiona w inspektorze! Culling nie będzie działać.", this);
            return;
        }
        
        frustumPlanes = new Plane[6];
        
        // Usunięto ustawianie statycznego pola GrassChunk.maxGrassSize

        LoadGrassTypes();

        float offsetX = Random.Range(0f, 9999f);
        float offsetY = Random.Range(0f, 9999f);
        int height = xInstances;
        int width = zInstances;
        // Zakładam, że masz gdzieś klasę PerlinNoise
        grassHeightMap = PerlinNoise.GenerateTexture(grassScale, offsetX, offsetY, width, height);

        GenerateGrass();
    }
    
    void Update()
    {
        // ZMIANA: Sprawdzamy 'playerCamera'
        if (chunks.Count == 0 || playerCamera == null) return;

        // ZMIANA: Używamy 'playerCamera'
        GeometryUtility.CalculateFrustumPlanes(playerCamera, frustumPlanes);

        foreach (GrassChunk chunk in chunks.Values)
        {
            if (!GeometryUtility.TestPlanesAABB(frustumPlanes, chunk.bounds))
            {
                continue; 
            }
            
            foreach (var kvp in chunk.matricesByType)
            {
                GrassType type = kvp.Key;
                List<Matrix4x4> matrices = kvp.Value;
                if (matrices.Count == 0 || playerCamera == null) continue;

                Vector3 cameraPosition = playerCamera.transform.position;
                GeometryUtility.CalculateFrustumPlanes(playerCamera, frustumPlanes);

                float distanceToChunk = Vector3.Distance(chunk.bounds.center, cameraPosition);
                if (distanceToChunk > renderDistance)
                {
                    continue; // Chunk jest za daleko
                }

                for (int i = 0; i < matrices.Count; i += BATCH_SIZE)
                {
                    int count = Mathf.Min(BATCH_SIZE, matrices.Count - i);
                    Matrix4x4[] batch = new Matrix4x4[count];
                    matrices.CopyTo(i, batch, 0, count);

                    Graphics.DrawMeshInstanced(
                        type.mesh,
                        0,
                        type.material,
                        batch,
                        count,
                        null,
                        UnityEngine.Rendering.ShadowCastingMode.Off,
                        true
                    );
                }
            }
        }
    }


    [ContextMenu("Generate Grass")]
    public void GenerateGrass()
    {
        if (grassHeightMap == null) Debug.Log("grassHeightMap == null. Generowanie przerwane.");
        
        chunks.Clear();

        // Upewnij się, że grassTypes są załadowane
        if (grassTypes.Count == 0)
        {
            Debug.LogError("Lista grassTypes jest pusta. Wywołaj LoadGrassTypes() przed GenerateGrass().");
            LoadGrassTypes(); // Spróbuj załadować
            if (grassTypes.Count == 0) return; // Jeśli nadal pusta, przerwij
        }

        float xStep = (float)(xMax - xStart) / xInstances;
        float zStep = (float)(zMax - zStart) / zInstances;

        for (float x = xStart; x <= xMax; x += xStep)
        {
            for (float z = zStart; z <= zMax; z += zStep)
            {
                float offsetX = Random.Range(-grassSideOffset, grassSideOffset);
                float offsetZ = Random.Range(-grassSideOffset, grassSideOffset);
                Vector3 rayStart = new Vector3(x + offsetX, spawnHeight, z + offsetZ);

                if (!Physics.Raycast(rayStart, Vector3.down, out RaycastHit hit, Mathf.Infinity)) continue;
                if (hit.collider.GetComponent<Terrain>() == null) continue;
                
                // Poprawka mapowania UV (była w poprzednim kodzie, zostaje)
                float u_normalized = (x - xStart) / (float)(xMax - xStart);
                float v_normalized = (z - zStart) / (float)(zMax - zStart);
                int texX = Mathf.FloorToInt(u_normalized * grassHeightMap.width);
                int texZ = Mathf.FloorToInt(v_normalized * grassHeightMap.height);

                float lower = grassHeightMap.GetPixel(texX, texZ).grayscale * lowerGrassMult;
                if (lower >= 1) continue; 

                Vector3 pos = new Vector3(x + offsetX, hit.point.y - lower, z + offsetZ);
                Quaternion rot = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
                float s = Random.Range(minGrassSize, maxGrassSize);
                Vector3 scale = Vector3.one * s;
                GrassType type = grassTypes[Random.Range(0, grassTypes.Count)];

                int chunkX = Mathf.FloorToInt(pos.x / chunkSize);
                int chunkZ = Mathf.FloorToInt(pos.z / chunkSize);
                Vector2Int chunkCoord = new Vector2Int(chunkX, chunkZ);

                if (!chunks.TryGetValue(chunkCoord, out GrassChunk chunk))
                {
                    chunk = new GrassChunk();
                    chunks[chunkCoord] = chunk;
                }

                // POPRAWKA: Przekazujemy 'maxGrassSize' do metody AddInstance
                chunk.AddInstance(type, pos, rot, scale, maxGrassSize);
            }
        }
    }


    [ContextMenu("Clear Grass")]
    public void ClearGrass()
    {
        chunks.Clear();
    }

    private void LoadGrassTypes()
    {
        grassTypes.Clear();

        foreach (var prefabName in grassPrefabPaths)
        {
            var prefab = Resources.Load<GameObject>("Models/grass/" + prefabName);
            if (prefab == null) { Debug.Log("Didnt find prefab "+prefabName); continue; }

            var mf = prefab.GetComponent<MeshFilter>();
            if (mf == null) { Debug.Log("Couldnt load MeshFilter for "+prefabName); continue; }

            var mr = prefab.GetComponent<MeshRenderer>();
            if (mr == null) { Debug.Log("Couldldnt load MeshRenderer for "+prefabName); continue; }

            if (!mr.sharedMaterial.enableInstancing)
            {
                Debug.LogWarning($"Materiał dla {prefabName} nie ma włączonego 'Enable GPU Instancing'!");
                mr.sharedMaterial.enableInstancing = true;
            }

            // Używamy Twojej zewnętrznej klasy GrassType
            grassTypes.Add(new GrassType
            {
                resourceName = prefabName,
                prefab = prefab,
                mesh = mf.sharedMesh,
                material = mr.sharedMaterial
            });
        }
    }
}