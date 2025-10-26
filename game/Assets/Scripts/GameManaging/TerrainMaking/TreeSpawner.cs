using UnityEngine;

public class TreeSpawner : MonoBehaviour
{
    public float xStart = 0;
    public float zStart = 0;
    public float xEnd = 256;
    public float zEnd = 256;
    public int amountOfTrees = 10;

    private int spawnHeight = 50;
    private GameObject treePrefab;
    private GameObject treeParent;



    public void Start()
    {
        treePrefab = Resources.Load<GameObject>("Models/tree/tree");

    }

    [ContextMenu("Spawn trees")]
    public void spawnTrees()
    {
        if (treePrefab == null) return;

        treeParent = new GameObject("TreeParent");
        treeParent.transform.position = new Vector3(xStart, 0, zStart);

        for (int i = 0; i < amountOfTrees; i++)
        {
            float x = Random.Range(xStart, xEnd);
            float z = Random.Range(zStart, zEnd);
            float scale = Random.Range(1f, 2f);

            Vector3 rayStart = new Vector3(x, spawnHeight, z);
            if (!Physics.Raycast(rayStart, Vector3.down, out RaycastHit hit, Mathf.Infinity)) continue;

            Vector3 spawnPosition = new Vector3(x, hit.point.y, z);
            int randomRotation = Random.Range(0, 360);
            Quaternion rotation = Quaternion.Euler(0f, randomRotation, 0f);

            GameObject tree = Instantiate(treePrefab, spawnPosition, rotation);
            tree.transform.localScale = Vector3.one * scale;
            tree.transform.SetParent(treeParent.transform);
        }
    }

    [ContextMenu("Remove trees")]
    public void removeTrees()
    {
        if (treeParent != null)
        {
            Destroy(treeParent);
            treeParent = null;
        }
    }
}