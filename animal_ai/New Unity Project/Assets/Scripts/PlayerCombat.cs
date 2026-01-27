using UnityEngine;

/// <summary>
/// Handles player attacks when clicking the left mouse button.
/// </summary>
public class PlayerCombat : MonoBehaviour
{
    [Header("Attack Settings")]
    [Tooltip("How far the 'aim' reaches")]
    public float attackRange = 30f;
    [Tooltip("How close you need to be for automatic hitting")]
    public float proximityRange = 20f;
    public int attackDamage = 15;
    public float attackCooldown = 0.3f;
    
    [Header("Visuals")]
    public Transform attackPoint; // Optional point to cast the attack from
    
    private float nextAttackTime;

    private void Start()
    {
        // BALANCED EASY MODE (Overrides inspector)
        attackRange = 8f;
        proximityRange = 8f;
        attackDamage = 100;
    }

    private void Update()
    {
        if (Time.time >= nextAttackTime)
        {
            // Left Click OR press 'K' to collect
            if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.K)) 
            {
                Attack();
                nextAttackTime = Time.time + attackCooldown;
            }
        }
    }

    private void Attack()
    {
        Debug.Log("Left Click Pressed - Searching for animals...");

        // Strategy 1: Look at all animals in the scene and see if any are very close
        CheckProximityAttack();

        // Strategy 2: Raycast (Aiming)
        Camera cam = GetComponentInChildren<Camera>();
        if (cam == null) return;

        Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, attackRange))
        {
            Debug.Log($"Raycast hit: {hit.collider.gameObject.name}");
            
            // Check for passive animals
            AnimalAI animal = hit.collider.GetComponentInParent<AnimalAI>();
            if (animal != null)
            {
                Debug.Log("Hit Passive Animal!");
                animal.TakeDamage(attackDamage);
                return;
            }

            // Check for aggressive animals
            AggressiveAnimalAI aggroAnimal = hit.collider.GetComponentInParent<AggressiveAnimalAI>();
            if (aggroAnimal != null)
            {
                Debug.Log("Hit Aggressive Animal!");
                aggroAnimal.TakeDamage(attackDamage);
            }
        }
    }

    private void CheckProximityAttack()
    {
        // Find all passive animals
        AnimalAI[] passiveAnimals = Object.FindObjectsByType<AnimalAI>(FindObjectsSortMode.None);
        foreach (var animal in passiveAnimals)
        {
            if (Vector3.Distance(transform.position, animal.transform.position) <= proximityRange)
            {
                Debug.Log("Proximity Hit on Passive Animal!");
                animal.TakeDamage(attackDamage);
            }
        }

        // Find all aggressive animals
        AggressiveAnimalAI[] aggroAnimals = Object.FindObjectsByType<AggressiveAnimalAI>(FindObjectsSortMode.None);
        foreach (var animal in aggroAnimals)
        {
            if (Vector3.Distance(transform.position, animal.transform.position) <= proximityRange)
            {
                Debug.Log("Proximity Hit on Aggressive Animal!");
                animal.TakeDamage(attackDamage);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        // Debug line showing attack range
        Camera cam = GetComponentInChildren<Camera>();
        if (cam != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(cam.transform.position, cam.transform.position + cam.transform.forward * attackRange);
        }
    }
}
