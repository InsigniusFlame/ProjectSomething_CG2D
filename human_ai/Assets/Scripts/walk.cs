using UnityEngine;
using UnityEngine.AI;

public class walk : MonoBehaviour
{
    NavMeshAgent agent;
    Animator anim;

    public float walkRadius = 10f;
    public float idleTime = 2f;
    public float talkTime = 3f;

    float timer;
    bool isTalking = false;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponentInChildren<Animator>();

        agent.autoBraking = false;
        anim.applyRootMotion = false;

        GoIdle();
    }

    void Update()
    {
        anim.SetBool("isWalking", agent.velocity.magnitude > 0.1f);

        if (isTalking)
        {
            timer += Time.deltaTime;
            if (timer >= talkTime)
            {
                isTalking = false;
                anim.SetBool("isTalking", false);
                GoIdle();
            }
            return;
        }

        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            timer += Time.deltaTime;
            if (timer >= idleTime)
            {
                GoWalk();
            }
        }
    }

    void GoIdle()
    {
        agent.isStopped = true;
        timer = 0f;
    }

    void GoWalk()
    {
        agent.isStopped = false;
        timer = 0f;

        Vector3 randomDir = Random.insideUnitSphere * walkRadius + transform.position;
        if (NavMesh.SamplePosition(randomDir, out NavMeshHit hit, walkRadius, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
        }
    }

    // TALK trigger (call this)
    public void Talk()
    {
        agent.isStopped = true;
        agent.ResetPath();
        isTalking = true;
        timer = 0f;
        anim.SetBool("isTalking", true);
    }

    void OnGUI()
    {
        if (GUI.Button(new Rect(20, 20, 100, 30), "TALK"))
        {
            Talk();
        }
    }
}
