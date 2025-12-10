using UnityEngine;
using System.Collections.Generic;

public class GrassManager : MonoBehaviour
{
    // ... (Klasa GrassChunk bez zmian) ...
    private class GrassChunk
    {
        public Dictionary<GrassType, List<Matrix4x4>> matricesByType = new Dictionary<GrassType, List<Matrix4x4>>();
        public Bounds bounds;
        private bool boundsInitialized = false;

        public void AddInstance(GrassType type, Vector3 pos, Quaternion rot, Vector3 scale, float maxScaleValue)
        {
            if (!matricesByType.ContainsKey(type))
            {
                matricesByType[type] = new List<Matrix4x4>();
            }
            matricesByType[type].Add(Matrix4x4.TRS(pos, rot, scale));

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
    public Camera playerCamera;

    private int spawnHeight = 100;
    private Texture2D grassHeightMap;
    private MaterialPropertyBlock propertyBlock;

    [Header("Render distance")]
    public float renderDistance = 60f;
    
    // --- NOWOŚĆ: Dystans, od którego włącza się LOD ---
    [Tooltip("LOD grass model load distance")]
    public float lodDistance = 30f; 
    [Tooltip("Distance, from end, where fading happens")]
    public float fadeRange = 10f;

    public List<string> grassPrefabPaths = new List<string>
    {
        "grass1",
        "grass3"
    };

    private List<GrassType> grassTypes = new List<GrassType>();
    private Dictionary<Vector2Int, GrassChunk> chunks = new Dictionary<Vector2Int, GrassChunk>();
    private Plane[] frustumPlanes;

    void Update()
    {
        if (chunks.Count == 0 || playerCamera == null) return;

        GeometryUtility.CalculateFrustumPlanes(playerCamera, frustumPlanes);
        Vector3 cameraPosition = playerCamera.transform.position;

        foreach (GrassChunk chunk in chunks.Values)
        {
            if (!GeometryUtility.TestPlanesAABB(frustumPlanes, chunk.bounds)) continue;

            float distanceToChunk = Vector3.Distance(chunk.bounds.center, cameraPosition);
            if (distanceToChunk > renderDistance) continue;

            // --- 1. OBLICZANIE MNOŻNIKA SKALI ---
            float scaleMultiplier = 1.0f;
            float fadeStart = renderDistance - fadeRange;

            if (distanceToChunk > fadeStart)
            {
                // Obliczamy jak daleko jesteśmy w strefie zanikania (0..1)
                float t = (distanceToChunk - fadeStart) / fadeRange;
                // Im dalej, tym mniejsza skala (od 1.0 do 0.0)
                scaleMultiplier = Mathf.Lerp(1.0f, 0.0f, t);
            }

            // Optymalizacja: Jeśli trawa jest mikroskopijna, nie rysuj jej wcale
            if (scaleMultiplier < 0.05f) continue;
            // ------------------------------------

            foreach (var kvp in chunk.matricesByType)
            {
                GrassType type = kvp.Key;
                List<Matrix4x4> matrices = kvp.Value;
                if (matrices.Count == 0) continue;

                // LOD Logic
                Mesh meshToDraw = type.mesh;
                if (distanceToChunk > lodDistance && type.meshLOD != null)
                {
                    meshToDraw = type.meshLOD;
                }

                for (int i = 0; i < matrices.Count; i += BATCH_SIZE)
                {
                    int count = Mathf.Min(BATCH_SIZE, matrices.Count - i);
                    Matrix4x4[] batch = new Matrix4x4[count];
                    matrices.CopyTo(i, batch, 0, count);

                    // --- 2. MODYFIKACJA SKALI W BATCHU ---
                    // Wykonujemy to TYLKO jeśli jesteśmy w strefie zanikania
                    if (scaleMultiplier < 0.99f)
                    {
                        for (int k = 0; k < count; k++)
                        {
                            // Pobieramy aktualną macierz
                            Matrix4x4 mat = batch[k];

                            // Wyciągamy rotację i pozycję (kolumna 3 to pozycja)
                            Vector3 pos = new Vector3(mat.m03, mat.m13, mat.m23);
                            Quaternion rot = mat.rotation;
                            Vector3 scale = mat.lossyScale;

                            // Mnożymy wysokość (Y) przez nasz fade factor
                            scale.y *= scaleMultiplier;

                            // Nadpisujemy macierz w batchu nową, spłaszczoną wersją
                            batch[k] = Matrix4x4.TRS(pos, rot, scale);
                        }
                    }
                    // -------------------------------------

                    Graphics.DrawMeshInstanced(
                        meshToDraw,
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

    // ... (GenerateGrass i ClearGrass bez zmian) ...
    [ContextMenu("Generate Grass")]
    public void GenerateGrass()
    {
        // Kod GenerateGrass pozostaje bez zmian, taki jak w Twoim poście.
        // Skróciłem go tutaj dla czytelności odpowiedzi, ale wklej tu swoją wersję.
        if (grassHeightMap == null) Debug.Log("grassHeightMap == null. Generowanie przerwane.");
        chunks.Clear();
        if (grassTypes.Count == 0) {
            Debug.LogError("Lista grassTypes jest pusta. Wywołaj LoadGrassTypes() przed GenerateGrass().");
            LoadGrassTypes();
            if (grassTypes.Count == 0) return;
        }

        float xStep = (float)(xMax - xStart) / xInstances;
        float zStep = (float)(zMax - zStart) / zInstances;

        for (float x = xStart; x <= xMax; x += xStep)
        {
            for (float z = zStart; z <= zMax; z += zStep)
            {
                // setting random offset from grid
                float offsetX = Random.Range(-grassSideOffset, grassSideOffset);
                float offsetZ = Random.Range(-grassSideOffset, grassSideOffset);
                Vector3 rayStart = new Vector3(x + offsetX, spawnHeight, z + offsetZ);

                // checking if on terrain
                if (!Physics.Raycast(rayStart, Vector3.down, out RaycastHit hit, Mathf.Infinity)) continue;
                if (hit.collider.GetComponent<Terrain>() == null) continue;

                float u_normalized = (x - xStart) / (float)(xMax - xStart);
                float v_normalized = (z - zStart) / (float)(zMax - zStart);
                int texX = Mathf.FloorToInt(u_normalized * grassHeightMap.width);
                int texZ = Mathf.FloorToInt(v_normalized * grassHeightMap.height);

                float lower = grassHeightMap.GetPixel(texX, texZ).grayscale * lowerGrassMult;
                if (lower >= 1) continue;

                // position and rotation
                Vector3 pos = new Vector3(x + offsetX, hit.point.y, z + offsetZ);
                Quaternion rot = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);

                // determinating grass size
                float baseScale = Random.Range(minGrassSize, maxGrassSize);
                float heightFactor = Mathf.Clamp01(1.0f - lower);
                Vector3 scale = new Vector3(baseScale, baseScale * heightFactor, baseScale);
                GrassType type = grassTypes[Random.Range(0, grassTypes.Count)];

                // adding to proper chunk
                int chunkX = Mathf.FloorToInt(pos.x / chunkSize);
                int chunkZ = Mathf.FloorToInt(pos.z / chunkSize);
                Vector2Int chunkCoord = new Vector2Int(chunkX, chunkZ);

                if (!chunks.TryGetValue(chunkCoord, out GrassChunk chunk))
                {
                    chunk = new GrassChunk();
                    chunks[chunkCoord] = chunk;
                }
                chunk.AddInstance(type, pos, rot, scale, maxGrassSize);
            }
        }
    }
    
    [ContextMenu("Clear Grass")]
    public void ClearGrass()
    {
        chunks.Clear();
    }
    // ...

    // --- ZMODYFIKOWANA METODA ŁADOWANIA ---
    private void LoadGrassTypes()
    {
        grassTypes.Clear();

        foreach (var prefabName in grassPrefabPaths)
        {
            // 1. Ładowanie podstawowego modelu
            var prefab = Resources.Load<GameObject>("Models/grass/" + prefabName);
            if (prefab == null) { Debug.Log("Didnt find prefab " + prefabName); continue; }

            var mf = prefab.GetComponent<MeshFilter>();
            if (mf == null) { Debug.Log("Couldnt load MeshFilter for " + prefabName); continue; }

            var mr = prefab.GetComponent<MeshRenderer>();
            if (mr == null) { Debug.Log("Couldldnt load MeshRenderer for " + prefabName); continue; }

            if (!mr.sharedMaterial.enableInstancing)
            {
                Debug.LogWarning($"Materiał dla {prefabName} nie ma włączonego 'Enable GPU Instancing'!");
                mr.sharedMaterial.enableInstancing = true;
            }

            // 2. Próba załadowania modelu LOD (szukamy nazwy + "LOD")
            Mesh lodMesh = null;
            string lodName = prefabName + "LOD"; // np. "grass1LOD"
            var lodPrefab = Resources.Load<GameObject>("Models/grass/" + lodName);
            
            if (lodPrefab != null)
            {
                var lodMf = lodPrefab.GetComponent<MeshFilter>();
                if (lodMf != null)
                {
                    lodMesh = lodMf.sharedMesh;
                    //Debug.Log($"Załadowano LOD dla: {prefabName}");
                }
            }

            // 3. Dodanie do listy
            grassTypes.Add(new GrassType
            {
                resourceName = prefabName,
                prefab = prefab,
                mesh = mf.sharedMesh,
                meshLOD = lodMesh, // <-- Przypisujemy znaleziony LOD (lub null)
                material = mr.sharedMaterial
            });
        }
    }

    public void Load()
    {
        if (playerCamera == null)
        {
            Debug.LogError("Kamera gracza (playerCamera) nie jest ustawiona!", this);
            return;
        }

        frustumPlanes = new Plane[6];
        LoadGrassTypes();

        float offsetX = Random.Range(0f, 9999f);
        float offsetY = Random.Range(0f, 9999f);
        int height = xInstances;
        int width = zInstances;
        grassHeightMap = PerlinNoise.GenerateTexture(grassScale, offsetX, offsetY, width, height);
    }
}