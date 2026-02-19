using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    public float attackRange = 3f;
    public int damage = 25;
    public LayerMask npcLayer;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            Attack();
        }
    }

    void Attack()
    {
        Ray ray = new Ray(transform.position, transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, attackRange, npcLayer))
        {
            npchealth npc = hit.collider.GetComponentInParent<npchealth>();

            if (npc != null)
            {
                npc.TakeDamage(damage);
            }
        }
    }
}
