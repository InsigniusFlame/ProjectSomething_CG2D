using UnityEngine;

public class treespawner : MonoBehaviour
{
     public GameObject treePrefab;
    public Collider groundCollider;
    public int numberOfTrees = 15;
    public Vector2 scaleRange = new Vector2(0.9f, 1.2f);

    [ContextMenu("Spawn Trees")]
    void SpawnTrees()
    {
        ClearTrees();

        for (int i = 0; i < numberOfTrees; i++)
        {
            Vector3 pos = RandomPointInBounds(groundCollider.bounds);

            GameObject tree = Instantiate(treePrefab, pos, Quaternion.identity);
            tree.transform.parent = transform;

            tree.transform.Rotate(0, Random.Range(0, 360), 0);

            float scale = Random.Range(scaleRange.x, scaleRange.y);
            tree.transform.localScale = Vector3.one * scale;
        }
    }

    [ContextMenu("Clear Trees")]
    void ClearTrees()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(transform.GetChild(i).gameObject);
        }
    }

    Vector3 RandomPointInBounds(Bounds bounds)
    {
        return new Vector3(
            Random.Range(bounds.min.x, bounds.max.x),
            bounds.min.y,
            Random.Range(bounds.min.z, bounds.max.z)
        );
    }
}
