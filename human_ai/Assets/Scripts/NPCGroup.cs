using UnityEngine;

public class NPCGroup : MonoBehaviour
{
    public float groupRadius = 6f;

    public Vector3 GetRandomPoint()
    {
        Vector2 rand = Random.insideUnitCircle * groupRadius;
        return transform.position + new Vector3(rand.x, 0, rand.y);
    }
}
