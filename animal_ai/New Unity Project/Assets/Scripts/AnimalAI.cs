using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Animal AI controller that handles continuous random roaming and fleeing from player.
/// Animals roam naturally like deer in a forest - sometimes slow, sometimes faster,
/// with longer pauses to graze or rest.
/// Attach to any GameObject with NavMeshAgent component.
/// </summary>
[RequireComponent(typeof(NavMeshAgent))]
public class AnimalAI : MonoBehaviour
{
    [Header("Boundary Settings")]
    [Tooltip("Center of the roaming area (set automatically to spawn position if not specified)")]
    public Vector3 territoryCenter;
    
    [Tooltip("Maximum distance from territory center the animal can roam")]
    public float territoryRadius = 40f;
    
    [Tooltip("Distance from edge to start turning back")]
    public float edgeBuffer = 5f;

    [Header("Roaming Settings")]
    [Tooltip("Minimum distance for next roam destination")]
    public float minRoamDistance = 3f;
    
    [Tooltip("Maximum distance for next roam destination")]
    public float maxRoamDistance = 10f;
    
    [Tooltip("Minimum time to wait/graze at destination")]
    public float minWaitTime = 5f;
    
    [Tooltip("Maximum time to wait/graze at destination")]
    public float maxWaitTime = 10f;
    
    [Tooltip("Slowest walking speed (grazing/wandering)")]
    public float slowSpeed = 0.8f;
    
    [Tooltip("Normal walking speed")]
    public float normalSpeed = 1.5f;
    
    [Tooltip("Faster trotting speed (occasional)")]
    public float fastSpeed = 2.5f;
    
    [Tooltip("Chance to use slow speed (0-1)")]
    [Range(0f, 1f)]
    public float slowSpeedChance = 0.4f;
    
    [Tooltip("Chance to use fast speed (0-1)")]
    [Range(0f, 1f)]
    public float fastSpeedChance = 0.15f;

    [Header("Flee Settings")]
    [Tooltip("Distance at which the animal detects the player")]
    public float detectionRadius = 3f;
    
    [Tooltip("Distance to flee from player")]
    public float fleeDistance = 15f;
    
    [Tooltip("Speed when fleeing from player (fastest)")]
    public float fleeSpeed = 3f;
    
    [Tooltip("How long to keep fleeing after losing sight of player")]
    public float fleeCooldown = 3f;

    [Header("Debug")]
    [Tooltip("Show detection radius in editor")]
    public bool showGizmos = true;
    
    [Tooltip("Color for detection radius gizmo")]
    public Color gizmoColor = new Color(1f, 0.5f, 0f, 0.3f);

    [Header("Health & Collection")]
    public int maxHealth = 1;
    public int currentHealth;
    public string animalName = "Wild Animal";
    public Color damageFlashColor = Color.red;

    private NavMeshAgent agent;
    private Transform playerTransform;
    private float waitTimer;
    private bool isWaiting;
    private bool isFleeing;
    private float fleeTimer;
    private float stuckTimer;
    private Vector3 lastPosition;
    private float destinationTimeout;
    private float currentRoamSpeed;
    private MeshRenderer[] meshRenderers;
    private Color[] originalColors;
    private float flashTimer;

    private void Start()
    {
        // FORCE EASY MODE (Overrides inspector)
        maxHealth = 1;
        currentHealth = 1;
        detectionRadius = 2f;
        slowSpeed = 0.5f;
        normalSpeed = 0.5f;
        fastSpeed = 0.5f;
        fleeSpeed = 0.5f;

        agent = GetComponent<NavMeshAgent>();
        agent.speed = 0.5f;
        agent.acceleration = 2f;
        if (territoryCenter == Vector3.zero)
        {
            territoryCenter = transform.position;
        }
        
        // Find the player by tag
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }
        else
        {
            Debug.LogWarning($"AnimalAI on {gameObject.name}: No GameObject with 'Player' tag found!");
        }

        // Start with a random speed
        PickRandomRoamSpeed();
        agent.speed = currentRoamSpeed;
        agent.stoppingDistance = 0.5f;
        lastPosition = transform.position;
        
        // Initialize health
        currentHealth = maxHealth;

        // Get renderers for flash effect
        meshRenderers = GetComponentsInChildren<MeshRenderer>();
        if (meshRenderers != null && meshRenderers.Length > 0)
        {
            originalColors = new Color[meshRenderers.Length];
            for (int i = 0; i < meshRenderers.Length; i++)
            {
                originalColors[i] = meshRenderers[i].material.color;
            }
        }

        // Start roaming immediately
        SetRandomRoamDestination();
    }

    /// <summary>
    /// Apply damage to the animal.
    /// </summary>
    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        
        // Flash red
        flashTimer = 0.2f;
        ApplyFlashColor(damageFlashColor);

        Debug.Log($"{gameObject.name} took {damage} damage! Health: {currentHealth}");

        if (currentHealth <= 0)
        {
            CollectAnimal();
        }
    }

    private void ApplyFlashColor(Color color)
    {
        if (meshRenderers == null) return;
        foreach (var renderer in meshRenderers)
        {
            renderer.material.color = color;
        }
    }

    private void ResetFlashColor()
    {
        if (meshRenderers == null) return;
        for (int i = 0; i < meshRenderers.Length; i++)
        {
            meshRenderers[i].material.color = originalColors[i];
        }
    }

    private void CollectAnimal()
    {
        Debug.Log($"{animalName} collected!");
        
        // Notify inventory system
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.AddItem(animalName, 1);
        }
        else
        {
            Debug.LogWarning("InventoryManager not found in scene!");
        }

        // Remove from scene
        Destroy(gameObject);
    }

    /// <summary>
    /// Pick a random speed for this roaming segment.
    /// </summary>
    private void PickRandomRoamSpeed()
    {
        float roll = Random.value;
        
        if (roll < slowSpeedChance)
        {
            // Slow grazing speed
            currentRoamSpeed = slowSpeed;
        }
        else if (roll > (1f - fastSpeedChance))
        {
            // Occasional faster trot
            currentRoamSpeed = fastSpeed;
        }
        else
        {
            // Normal walking speed
            currentRoamSpeed = normalSpeed;
        }
    }

    /// <summary>
    /// Check if the agent is properly placed on the NavMesh.
    /// </summary>
    private bool IsAgentOnNavMesh()
    {
        return agent != null && agent.isOnNavMesh;
    }

    private void Update()
    {
        // Don't run AI if agent isn't on NavMesh
        if (!IsAgentOnNavMesh())
        {
            // Try to warp agent to nearest NavMesh point
            NavMeshHit hit;
            if (NavMesh.SamplePosition(transform.position, out hit, 5f, NavMesh.AllAreas))
            {
                agent.Warp(hit.position);
            }
            return;
        }

        // Check if we're near the territory edge - PRIORITY over everything else
        if (IsNearTerritoryEdge())
        {
            TurnBackFromEdge();
            return;
        }

        // Handle flee cooldown
        if (fleeTimer > 0)
        {
            fleeTimer -= Time.deltaTime;
        }

        // Check for player proximity
        float distanceToPlayer = playerTransform != null ? 
            Vector3.Distance(transform.position, playerTransform.position) : float.MaxValue;

        if (distanceToPlayer < detectionRadius)
        {
            // Check if fleeing would take us out of bounds
            Vector3 fleeDirection = (transform.position - playerTransform.position).normalized;
            Vector3 potentialFleePos = transform.position + fleeDirection * fleeDistance;
            
            if (IsPositionInTerritory(potentialFleePos))
            {
                FleeFromPlayer();
            }
            else
            {
                // Can't flee that way - run perpendicular or towards center
                FleeTowardsSafeArea();
            }
        }
        else
        {
            Roam();
        }

        // Check if stuck (not moving for too long)
        CheckIfStuck();

        // Handle damage flash reset
        if (flashTimer > 0)
        {
            flashTimer -= Time.deltaTime;
            if (flashTimer <= 0)
            {
                ResetFlashColor();
            }
        }
    }

    /// <summary>
    /// Check if current position is near the territory boundary.
    /// </summary>
    private bool IsNearTerritoryEdge()
    {
        float distanceFromCenter = Vector3.Distance(
            new Vector3(transform.position.x, territoryCenter.y, transform.position.z),
            territoryCenter
        );
        return distanceFromCenter > (territoryRadius - edgeBuffer);
    }

    /// <summary>
    /// Check if a position is within the territory bounds.
    /// </summary>
    private bool IsPositionInTerritory(Vector3 position)
    {
        float distanceFromCenter = Vector3.Distance(
            new Vector3(position.x, territoryCenter.y, position.z),
            territoryCenter
        );
        return distanceFromCenter < (territoryRadius - edgeBuffer);
    }

    /// <summary>
    /// Turn back towards center when reaching territory edge.
    /// </summary>
    private void TurnBackFromEdge()
    {
        isFleeing = false;
        isWaiting = false;
        PickRandomRoamSpeed();
        agent.speed = currentRoamSpeed;

        // Calculate direction back towards center
        Vector3 directionToCenter = (territoryCenter - transform.position).normalized;
        
        // Add some randomness so it doesn't always go straight to center
        Vector3 randomOffset = new Vector3(
            Random.Range(-0.5f, 0.5f),
            0,
            Random.Range(-0.5f, 0.5f)
        );
        Vector3 targetDirection = (directionToCenter + randomOffset).normalized;
        
        // Pick a point towards the center
        float distance = Random.Range(minRoamDistance, maxRoamDistance);
        Vector3 targetPosition = transform.position + targetDirection * distance;

        NavMeshHit hit;
        if (NavMesh.SamplePosition(targetPosition, out hit, maxRoamDistance, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
            destinationTimeout = 15f;
        }
    }

    /// <summary>
    /// Flee towards a safe area when normal flee direction would exit territory.
    /// </summary>
    private void FleeTowardsSafeArea()
    {
        if (!isFleeing)
        {
            isFleeing = true;
            isWaiting = false;
            agent.speed = fleeSpeed; // Fastest speed when fleeing
        }
        fleeTimer = fleeCooldown;

        // Calculate a flee direction that stays in bounds
        Vector3 awayFromPlayer = (transform.position - playerTransform.position).normalized;
        Vector3 toCenter = (territoryCenter - transform.position).normalized;
        
        // Blend between away from player and towards center
        Vector3 safeDirection = (awayFromPlayer + toCenter * 2f).normalized;
        Vector3 fleePosition = transform.position + safeDirection * fleeDistance;

        NavMeshHit hit;
        if (NavMesh.SamplePosition(fleePosition, out hit, fleeDistance, NavMesh.AllAreas))
        {
            if (IsPositionInTerritory(hit.position))
            {
                agent.SetDestination(hit.position);
            }
            else
            {
                // Just go towards center
                agent.SetDestination(territoryCenter);
            }
        }
    }

    /// <summary>
    /// Handles continuous roaming behavior like a deer in a forest.
    /// </summary>
    private void Roam()
    {
        if (isFleeing)
        {
            // Only stop fleeing if cooldown is done
            if (fleeTimer <= 0)
            {
                isFleeing = false;
                PickRandomRoamSpeed();
                agent.speed = currentRoamSpeed;
            }
            else
            {
                return; // Keep fleeing
            }
        }

        // Decrease destination timeout
        destinationTimeout -= Time.deltaTime;

        // Check if we need a new destination
        bool needsNewDestination = false;

        // Reached destination
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance + 0.1f)
        {
            if (!isWaiting)
            {
                isWaiting = true;
                // Longer wait times for natural grazing behavior
                waitTimer = Random.Range(minWaitTime, maxWaitTime);
            }
            else
            {
                waitTimer -= Time.deltaTime;
                if (waitTimer <= 0f)
                {
                    isWaiting = false;
                    needsNewDestination = true;
                }
            }
        }
        // No path or path invalidated
        else if (!agent.hasPath && !agent.pathPending)
        {
            needsNewDestination = true;
        }
        // Destination timeout (been trying to reach same destination too long)
        else if (destinationTimeout <= 0)
        {
            needsNewDestination = true;
        }

        if (needsNewDestination)
        {
            SetRandomRoamDestination();
        }
    }

    /// <summary>
    /// Check if the animal is stuck and needs a new destination.
    /// </summary>
    private void CheckIfStuck()
    {
        float movedDistance = Vector3.Distance(transform.position, lastPosition);
        
        if (movedDistance < 0.05f && !isWaiting)
        {
            stuckTimer += Time.deltaTime;
            if (stuckTimer > 3f)
            {
                // We're stuck, pick a new destination
                SetRandomRoamDestination();
                stuckTimer = 0f;
            }
        }
        else
        {
            stuckTimer = 0f;
        }
        
        lastPosition = transform.position;
    }

    /// <summary>
    /// Makes the animal flee away from the player at maximum speed.
    /// </summary>
    private void FleeFromPlayer()
    {
        if (!isFleeing)
        {
            isFleeing = true;
            isWaiting = false;
            agent.speed = fleeSpeed; // Fastest speed when player is nearby
        }
        fleeTimer = fleeCooldown;

        // Calculate flee direction (away from player)
        Vector3 fleeDirection = (transform.position - playerTransform.position).normalized;
        Vector3 fleePosition = transform.position + fleeDirection * fleeDistance;

        // Try to find a valid position on the NavMesh
        NavMeshHit hit;
        if (NavMesh.SamplePosition(fleePosition, out hit, fleeDistance, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
        }
    }

    /// <summary>
    /// Sets a random destination for continuous roaming within territory.
    /// </summary>
    private void SetRandomRoamDestination()
    {
        // Pick a new random speed for variety
        PickRandomRoamSpeed();
        agent.speed = currentRoamSpeed;

        // Try multiple times to find a valid destination
        for (int i = 0; i < 10; i++)
        {
            // Pick a random direction
            Vector2 randomCircle = Random.insideUnitCircle.normalized;
            Vector3 randomDirection = new Vector3(randomCircle.x, 0, randomCircle.y);
            
            // Pick a random distance
            float distance = Random.Range(minRoamDistance, maxRoamDistance);
            
            Vector3 targetPosition = transform.position + randomDirection * distance;
            
            // Make sure it's within territory
            if (!IsPositionInTerritory(targetPosition))
            {
                // Try a position towards center instead
                Vector3 toCenter = (territoryCenter - transform.position).normalized;
                targetPosition = transform.position + toCenter * distance;
            }

            NavMeshHit hit;
            if (NavMesh.SamplePosition(targetPosition, out hit, maxRoamDistance, NavMesh.AllAreas))
            {
                if (IsPositionInTerritory(hit.position))
                {
                    agent.SetDestination(hit.position);
                    destinationTimeout = 15f;
                    return;
                }
            }
        }

        // Fallback: go towards center
        NavMeshHit centerHit;
        if (NavMesh.SamplePosition(territoryCenter, out centerHit, territoryRadius, NavMesh.AllAreas))
        {
            agent.SetDestination(centerHit.position);
            destinationTimeout = 15f;
        }
    }

    /// <summary>
    /// Draw debug gizmos in the editor.
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        if (!showGizmos) return;

        Vector3 center = Application.isPlaying ? territoryCenter : 
            (territoryCenter != Vector3.zero ? territoryCenter : transform.position);

        // Territory boundary
        Gizmos.color = new Color(0f, 0.5f, 1f, 0.3f);
        Gizmos.DrawWireSphere(center, territoryRadius);
        
        // Edge buffer zone
        Gizmos.color = new Color(1f, 1f, 0f, 0.2f);
        Gizmos.DrawWireSphere(center, territoryRadius - edgeBuffer);

        // Detection radius (flee trigger zone)
        Gizmos.color = new Color(1f, 0f, 0f, 0.3f);
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }

    private void OnDrawGizmos()
    {
        if (!showGizmos) return;
        
        // Always show detection radius
        Gizmos.color = gizmoColor;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}
