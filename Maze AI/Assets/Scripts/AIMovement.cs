using UnityEngine;
using UnityEngine.AI;

public class AIMovement : MonoBehaviour
{
    public Transform[] waypoints;
    private int currentWP = 0;
    public float accuracy = 1f;

    private NavMeshAgent agent;
    private bool hasWon = false;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        if (waypoints.Length > 0)
            agent.SetDestination(waypoints[currentWP].position);
    }

    void Update()
    {
        if (hasWon) return;
        if (waypoints.Length == 0) return;

        if (!agent.pathPending && agent.remainingDistance < accuracy)
        {
            currentWP = (currentWP + 1) % waypoints.Length;
            agent.SetDestination(waypoints[currentWP].position);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Treasure"))
        {
            hasWon = true;
            agent.isStopped = true;
            Debug.Log("Treasure reached! Level complete.");
        }
    }
}