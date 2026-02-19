using UnityEngine;
using UnityEngine.AI;

public class NPCFSM : MonoBehaviour
{
    enum State { Idle, Walk, Talk }
    State state;

    NavMeshAgent agent;
    Animator anim;

    Vector3 homePos;

    public float roamRadius = 4f;
    public float idleTime = 2f;

    public float talkDistance = 2.2f;
    public float talkDuration = 3f;
    public float talkCooldown = 5f;

    float timer;
    float talkTimer;
    float lastTalkTime;

    NPCFSM talkPartner;
    Transform playerTarget;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponentInChildren<Animator>();

        homePos = transform.position;

        agent.autoBraking = false;
        anim.applyRootMotion = false;

        ChangeState(State.Idle);
    }

    void Update()
    {
        HandleFSM();
        HandlePlayerInteraction();
        HandleNPCTalk();
    }

    void LateUpdate()
    {
        if (state == State.Walk && agent.velocity.sqrMagnitude > 0.05f)
        {
            Quaternion rot = Quaternion.LookRotation(agent.velocity.normalized);
            transform.rotation = Quaternion.Slerp(transform.rotation, rot, Time.deltaTime * 8f);
        }

        if (state == State.Talk)
        {
            if (talkPartner)
                FaceTarget(talkPartner.transform.position);
            else if (playerTarget)
                FaceTarget(playerTarget.position);
        }
    }


    void HandleFSM()
    {
        if (state == State.Idle)
        {
            timer += Time.deltaTime;
            if (timer >= idleTime)
                ChangeState(State.Walk);
        }
        else if (state == State.Walk)
        {
            if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance + 0.1f)
                ChangeState(State.Idle);
        }
        else if (state == State.Talk)
        {
            talkTimer += Time.deltaTime;
            if (talkTimer >= talkDuration)
                EndTalk();
        }
    }

    void ChangeState(State newState)
    {
        state = newState;
        timer = 0f;

        anim.SetBool("isWalking", false);
        anim.SetBool("isTalking", false);

        if (state == State.Idle)
        {
            agent.isStopped = true;
        }
        else if (state == State.Walk)
        {
            agent.isStopped = false;
            SetRandomDestination();
            anim.SetBool("isWalking", true);
        }
        else if (state == State.Talk)
        {
            agent.isStopped = true;
            anim.SetBool("isTalking", true);
            talkTimer = 0f;
        }
    }

    void SetRandomDestination()
    {
        Vector3 random = Random.insideUnitSphere * roamRadius;
        random.y = 0;

        Vector3 target = homePos + random;

        if (NavMesh.SamplePosition(target, out NavMeshHit hit, roamRadius, NavMesh.AllAreas))
            agent.SetDestination(hit.position);
    }

    void HandleNPCTalk()
    {
        if (state != State.Idle) return;
        if (Time.time < lastTalkTime + talkCooldown) return;

        Collider[] hits = Physics.OverlapSphere(transform.position, talkDistance);

        foreach (Collider c in hits)
        {
            NPCFSM other = c.GetComponentInParent<NPCFSM>();
            if (!other || other == this) continue;

            if (other.transform.parent != transform.parent) continue;

            if (other.state == State.Idle)
            {
                StartTalk(other);
                break;
            }
        }
    }

    void StartTalk(NPCFSM other)
    {
        talkPartner = other;
        other.talkPartner = this;

        lastTalkTime = Time.time;
        other.lastTalkTime = Time.time;

        ChangeState(State.Talk);
        other.ChangeState(State.Talk);
    }

    void EndTalk()
    {
        talkPartner = null;
        ChangeState(State.Idle);
    }

    void HandlePlayerInteraction()
    {
        if (!Input.GetKeyDown(KeyCode.T)) return;

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (!player) return;

        if (Vector3.Distance(transform.position, player.transform.position) <= talkDistance)
        {
            talkPartner = null;
            playerTarget = player.transform;
            ChangeState(State.Talk);
        }
    }


    void FaceTarget(Vector3 pos)
    {
        Vector3 dir = pos - transform.position;
        dir.y = 0;

        if (dir.sqrMagnitude < 0.01f) return;

        Quaternion rot = Quaternion.LookRotation(dir);
        transform.rotation = Quaternion.Slerp(transform.rotation, rot, Time.deltaTime * 6f);
    }
}
