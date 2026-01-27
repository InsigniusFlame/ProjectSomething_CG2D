using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Aggressive Animal AI that attacks the player when they enter its territory.
/// The animal charges at the player and deals damage on contact.
/// </summary>
[RequireComponent(typeof(NavMeshAgent))]
public class AggressiveAnimalAI : MonoBehaviour
{
    [Header("Boundary Settings")]
    [Tooltip("Center of the territory (set automatically to spawn position if not specified)")]
    public Vector3 territoryCenter;
    
    [Tooltip("Maximum distance from territory center")]
    public float territoryRadius = 40f;
    
    [Tooltip("Distance from edge to start turning back")]
    public float edgeBuffer = 5f;

    [Header("Roaming Settings")]
    [Tooltip("Minimum distance for next roam destination")]
    public float minRoamDistance = 3f;
    
    [Tooltip("Maximum distance for next roam destination")]
    public float maxRoamDistance = 8f;
    
    [Tooltip("Minimum time to wait at destination")]
    public float minWaitTime = 3f;
    
    [Tooltip("Maximum time to wait at destination")]
    public float maxWaitTime = 6f;
    
    [Tooltip("Speed when casually roaming")]
    public float roamSpeed = 1.2f;

    [Header("Attack Settings")]
    [Tooltip("Distance at which the animal detects and attacks the player")]
    public float aggroRadius = 5f;
    
    [Tooltip("Speed when charging at player")]
    public float chargeSpeed = 4f;
    
    [Tooltip("Distance to trigger attack animation/jump")]
    public float attackDistance = 2f;
    
    [Tooltip("Time between attacks")]
    public float attackCooldown = 1.5f;
    
    [Tooltip("How high the animal jumps when attacking")]
    public float jumpHeight = 1.5f;
    
    [Tooltip("Duration of the jump attack")]
    public float jumpDuration = 0.4f;

    [Header("Debug")]
    [Tooltip("Show detection radius in editor")]
    public bool showGizmos = true;
    
    [Tooltip("Color for aggro radius gizmo")]
    public Color gizmoColor = new Color(1f, 0f, 0f, 0.3f);

    [Header("Health & Collection")]
    public int maxHealth = 1;
    public int currentHealth;
    public string animalName = "Aggressive Animal";
    public Color damageFlashColor = Color.red;

    private NavMeshAgent agent;
    private Transform playerTransform;
    private float waitTimer;
    private bool isWaiting;
    private bool isAggro;
    private float attackTimer;
    private float stuckTimer;
    private Vector3 lastPosition;
    private float destinationTimeout;
    
    // Jump attack variables
    private bool isJumping;
    private float jumpTimer;
    private Vector3 jumpStartPos;
    private Vector3 jumpTargetPos;
    private float originalY;

    private MeshRenderer[] meshRenderers;
    private Color[] originalColors;
    private float flashTimer;

    private void Start()
    {
        // FORCE EASY MODE (Overrides inspector)
        maxHealth = 1;
        currentHealth = 1;
        aggroRadius = 3f;
        roamSpeed = 0.5f;
        chargeSpeed = 0.5f;

        agent = GetComponent<NavMeshAgent>();
        agent.speed = 0.5f;
        originalY = transform.position.y;
        
        // Set territory center to spawn position if not manually set
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
            Debug.LogWarning($"AggressiveAnimalAI on {gameObject.name}: No GameObject with 'Player' tag found!");
        }

        agent.speed = roamSpeed;
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

    private bool IsAgentOnNavMesh()
    {
        return agent != null && agent.isOnNavMesh;
    }

    private void Update()
    {
        if (!IsAgentOnNavMesh())
        {
            NavMeshHit hit;
            if (NavMesh.SamplePosition(transform.position, out hit, 5f, NavMesh.AllAreas))
            {
                agent.Warp(hit.position);
            }
            return;
        }

        // Handle jump attack animation
        if (isJumping)
        {
            UpdateJumpAttack();
            return;
        }

        // Update attack cooldown
        if (attackTimer > 0)
        {
            attackTimer -= Time.deltaTime;
        }

        // Check for player proximity
        float distanceToPlayer = playerTransform != null ? 
            Vector3.Distance(transform.position, playerTransform.position) : float.MaxValue;

        if (distanceToPlayer < aggroRadius)
        {
            ChasePlayer(distanceToPlayer);
        }
        else
        {
            if (isAggro)
            {
                isAggro = false;
                agent.speed = roamSpeed;
            }
            Roam();
        }

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
    /// Chase and attack the player.
    /// </summary>
    private void ChasePlayer(float distanceToPlayer)
    {
        if (!isAggro)
        {
            isAggro = true;
            isWaiting = false;
            agent.speed = chargeSpeed;
        }

        // Set destination to player
        agent.SetDestination(playerTransform.position);

        // Check if close enough to attack
        if (distanceToPlayer <= attackDistance && attackTimer <= 0)
        {
            StartJumpAttack();
        }
    }

    /// <summary>
    /// Start a jumping attack towards the player.
    /// </summary>
    private void StartJumpAttack()
    {
        isJumping = true;
        jumpTimer = 0f;
        jumpStartPos = transform.position;
        jumpTargetPos = playerTransform.position;
        agent.isStopped = true;
        attackTimer = attackCooldown;
    }

    /// <summary>
    /// Update the jump attack animation.
    /// </summary>
    private void UpdateJumpAttack()
    {
        jumpTimer += Time.deltaTime;
        float progress = jumpTimer / jumpDuration;

        if (progress >= 1f)
        {
            // Jump complete
            isJumping = false;
            agent.isStopped = false;
            
            // Reset Y position
            Vector3 pos = transform.position;
            pos.y = originalY;
            transform.position = pos;

            // Check for collision with player
            float distToPlayer = Vector3.Distance(transform.position, playerTransform.position);
            if (distToPlayer < 2f)
            {
                DealDamageToPlayer();
            }
            return;
        }

        // Calculate jump arc
        Vector3 currentPos = Vector3.Lerp(jumpStartPos, jumpTargetPos, progress);
        
        // Add parabolic height
        float heightProgress = 1f - Mathf.Pow(2f * progress - 1f, 2f);
        currentPos.y = originalY + (jumpHeight * heightProgress);
        
        transform.position = currentPos;

        // Face the player
        Vector3 lookDir = (jumpTargetPos - jumpStartPos).normalized;
        lookDir.y = 0;
        if (lookDir != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(lookDir);
        }
    }

    /// <summary>
    /// Deal damage to the player (trigger red flash).
    /// </summary>
    private void DealDamageToPlayer()
    {
        // Find DamageSystem and trigger damage
        DamageSystem damageSystem = Object.FindFirstObjectByType<DamageSystem>();
        if (damageSystem != null)
        {
            damageSystem.TakeDamage(10);
        }
        else
        {
            Debug.Log($"{gameObject.name} attacked the player!");
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player") && isAggro)
        {
            DealDamageToPlayer();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && isAggro)
        {
            DealDamageToPlayer();
        }
    }

    /// <summary>
    /// Check if near territory edge.
    /// </summary>
    private bool IsNearTerritoryEdge()
    {
        float distanceFromCenter = Vector3.Distance(
            new Vector3(transform.position.x, territoryCenter.y, transform.position.z),
            territoryCenter
        );
        return distanceFromCenter > (territoryRadius - edgeBuffer);
    }

    private bool IsPositionInTerritory(Vector3 position)
    {
        float distanceFromCenter = Vector3.Distance(
            new Vector3(position.x, territoryCenter.y, position.z),
            territoryCenter
        );
        return distanceFromCenter < (territoryRadius - edgeBuffer);
    }

    /// <summary>
    /// Roaming behavior when player is not nearby.
    /// </summary>
    private void Roam()
    {
        if (IsNearTerritoryEdge())
        {
            TurnBackFromEdge();
            return;
        }

        destinationTimeout -= Time.deltaTime;
        bool needsNewDestination = false;

        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance + 0.1f)
        {
            if (!isWaiting)
            {
                isWaiting = true;
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
        else if (!agent.hasPath && !agent.pathPending)
        {
            needsNewDestination = true;
        }
        else if (destinationTimeout <= 0)
        {
            needsNewDestination = true;
        }

        if (needsNewDestination)
        {
            SetRandomRoamDestination();
        }
    }

    private void TurnBackFromEdge()
    {
        isWaiting = false;
        agent.speed = roamSpeed;

        Vector3 directionToCenter = (territoryCenter - transform.position).normalized;
        Vector3 randomOffset = new Vector3(Random.Range(-0.3f, 0.3f), 0, Random.Range(-0.3f, 0.3f));
        Vector3 targetDirection = (directionToCenter + randomOffset).normalized;
        
        float distance = Random.Range(minRoamDistance, maxRoamDistance);
        Vector3 targetPosition = transform.position + targetDirection * distance;

        NavMeshHit hit;
        if (NavMesh.SamplePosition(targetPosition, out hit, maxRoamDistance, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
            destinationTimeout = 15f;
        }
    }

    private void CheckIfStuck()
    {
        float movedDistance = Vector3.Distance(transform.position, lastPosition);
        
        if (movedDistance < 0.05f && !isWaiting && !isJumping)
        {
            stuckTimer += Time.deltaTime;
            if (stuckTimer > 3f)
            {
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

    private void SetRandomRoamDestination()
    {
        agent.speed = roamSpeed;

        for (int i = 0; i < 10; i++)
        {
            Vector2 randomCircle = Random.insideUnitCircle.normalized;
            Vector3 randomDirection = new Vector3(randomCircle.x, 0, randomCircle.y);
            float distance = Random.Range(minRoamDistance, maxRoamDistance);
            Vector3 targetPosition = transform.position + randomDirection * distance;
            
            if (!IsPositionInTerritory(targetPosition))
            {
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

        NavMeshHit centerHit;
        if (NavMesh.SamplePosition(territoryCenter, out centerHit, territoryRadius, NavMesh.AllAreas))
        {
            agent.SetDestination(centerHit.position);
            destinationTimeout = 15f;
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (!showGizmos) return;

        Vector3 center = Application.isPlaying ? territoryCenter : 
            (territoryCenter != Vector3.zero ? territoryCenter : transform.position);

        // Territory boundary
        Gizmos.color = new Color(0f, 0.5f, 1f, 0.3f);
        Gizmos.DrawWireSphere(center, territoryRadius);

        // Aggro radius
        Gizmos.color = new Color(1f, 0f, 0f, 0.4f);
        Gizmos.DrawWireSphere(transform.position, aggroRadius);
        
        // Attack range
        Gizmos.color = new Color(1f, 0.5f, 0f, 0.5f);
        Gizmos.DrawWireSphere(transform.position, attackDistance);
    }

    private void OnDrawGizmos()
    {
        if (!showGizmos) return;
        
        Gizmos.color = gizmoColor;
        Gizmos.DrawWireSphere(transform.position, aggroRadius);
    }
}
