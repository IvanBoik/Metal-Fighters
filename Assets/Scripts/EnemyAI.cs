using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    // необходима установка пакета AI Navigation (Window > Package manager > AI Navigation)
    public NavMeshAgent agent;
    public Transform player;
    public LayerMask whatIsGround, whatIsPlayer;
    public Transform bulletSpawnPoint; 

    public Vector3 walkPoint;
    bool walkPointSet;
    public float walkPointRange;

    // Параметры атаки
    public float timeBetweenAttacks;
    bool alreadyAttacked;
    public GameObject projectile;

    // Состояния
    public float sightRange, attackRange;
    public bool playerInSightRange, playerInAttackRange;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    private void Update()
    {
        playerInSightRange = Physics.CheckSphere(transform.position, sightRange, whatIsPlayer);
        playerInAttackRange = Physics.Raycast(transform.position, transform.forward, attackRange, whatIsPlayer);

        if (!playerInSightRange && !playerInAttackRange) Patroling();
        if (playerInSightRange && !playerInAttackRange) ChasePlayer();
        if (playerInSightRange && playerInAttackRange) AttackPlayer();
    }

    private void Patroling()
    {
        if (!walkPointSet) SearchWalkPoint();

        if (walkPointSet)
            agent.SetDestination(walkPoint);

        Vector3 distanceToWalkPoint = transform.position - walkPoint;

        if (distanceToWalkPoint.magnitude < 1f)
            walkPointSet = false;
    }

    private void SearchWalkPoint()
    {
        float randomZ = Random.Range(-walkPointRange, walkPointRange);
        float randomX = Random.Range(-walkPointRange, walkPointRange);

        walkPoint = new Vector3(transform.position.x + randomX, transform.position.y, transform.position.z + randomZ);

        if (Physics.Raycast(walkPoint, -transform.up, 2f, whatIsGround))
            walkPointSet = true;
    }

    private void ChasePlayer()
    {
        agent.SetDestination(player.position);
    }

    private void AttackPlayer()
    {
        agent.SetDestination(transform.position);

        transform.LookAt(player);
        transform.eulerAngles = new Vector3 (0, transform.eulerAngles.y, 0);

        if (!alreadyAttacked)
        {
            Rigidbody rb = Instantiate(projectile, bulletSpawnPoint.position, bulletSpawnPoint.rotation).GetComponent<Rigidbody>();
            rb.AddForce(bulletSpawnPoint.forward * 32f, ForceMode.Impulse);
            alreadyAttacked = true;
            Invoke(nameof(ResetAttack), timeBetweenAttacks);
        }
    }

    private void ResetAttack()
    {
        alreadyAttacked = false;
    }
} 