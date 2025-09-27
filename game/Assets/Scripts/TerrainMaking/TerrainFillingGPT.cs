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

    public GameObject grassPrefab;
    private Mesh grassMesh;
    private Material grassMaterial;

    private List<Matrix4x4> matrices = new List<Matrix4x4>();
    private const int BATCH_SIZE = 1023;

    private int spawnHeight = 50;

    void Start()
    {
        grassPrefab = Resources.Load<GameObject>("Models/grass/grass1");
        if (grassPrefab == null) { Debug.LogError("Nie znaleziono prefaba!"); return; }

        MeshFilter mf = grassPrefab.GetComponent<MeshFilter>();
        MeshRenderer mr = grassPrefab.GetComponent<MeshRenderer>();
        if (mf == null || mr == null) { Debug.LogError("Prefab musi mieć MeshFilter i MeshRenderer!"); return; }

        grassMesh = mf.sharedMesh;
        grassMaterial = mr.sharedMaterial;

        // upewnij się, że materiał ma włączone "Enable GPU Instancing"
        //GenerateGrass();
    }

    [ContextMenu("Generate Grass")]
    public void GenerateGrass()
    {
        matrices.Clear();

        float xStep = (float)(xMax - xStart) / xInstances;
        float zStep = (float)(zMax - zStart) / zInstances;

        for (float x = xStart; x <= xMax; x += xStep)
        {
            for (float z = zStart; z <= zMax; z += zStep)
            {
                Vector3 rayStart = new Vector3(x, spawnHeight, z);
                if (!Physics.Raycast(rayStart, Vector3.down, out RaycastHit hit, Mathf.Infinity)) continue;

                float lower = Random.Range(0f, 0.75f);
                float offsetX = Random.Range(-0.3f, 0.3f);
                float offsetY = Random.Range(-0.3f, 0.3f);
                Vector3 pos = new Vector3(x, hit.point.y - lower, z);


                // losowy obrót wokół Y i losowa skala (dla naturalności)
                Quaternion rot = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
                float s = Random.Range(minGrassSize, maxGrassSize);
                Vector3 scale = Vector3.one * s;

                matrices.Add(Matrix4x4.TRS(pos, rot, scale));
            }
        }

        Debug.Log("Instancji do renderu: " + matrices.Count);
    }

    void Update()
    {
        if (grassMesh == null || grassMaterial == null || matrices.Count == 0) return;

        // unikamy alokowania nowych list w pętli — kopiujemy do tymczasowej tablicy
        for (int i = 0; i < matrices.Count; i += BATCH_SIZE)
        {
            int count = Mathf.Min(BATCH_SIZE, matrices.Count - i);
            Matrix4x4[] batch = new Matrix4x4[count];
            matrices.CopyTo(i, batch, 0, count);

            // Możesz przekazać też MaterialPropertyBlock jeśli używasz instanced props
            Graphics.DrawMeshInstanced(grassMesh, 0, grassMaterial, batch, count);
        }
    }

    [ContextMenu("Clear Grass")]
    public void ClearGrass()
    {
        matrices.Clear();
    }
}
