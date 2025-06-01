using UnityEngine;

public class AIWaypoint : MonoBehaviour
{
    [Header("Waypoint Settings")]
    public Transform[] waypoints;
    private int currentWP = 0;
    public float accuracy = 1.0f;

    [Header("Movement Settings")]
    public float speed = 5f;
    public float turnSpeed = 10f;
    public float snapTurnAngle = 60f;

    [Header("Raycast Settings")]
    public float rayDistance = 3f;
    public int rayCount = 7;
    public float coneAngle = 60f;
    public LayerMask obstacleLayer;

    private bool hasWon = false;

    void Update()
    {
        if (hasWon) return;

        if (waypoints == null || waypoints.Length == 0) return;

        Vector3 waypointPos = new Vector3(
            waypoints[currentWP].position.x,
            transform.position.y,
            waypoints[currentWP].position.z);

        Vector3 directionToWP = waypointPos - transform.position;

        // Obstacle avoidance
        Vector3 avoidDirection = Vector3.zero;
        int hitCount = 0;

        float halfCone = coneAngle / 2f;
        Vector3 rayOrigin = transform.position + Vector3.up * 0.5f;

        for (int i = 0; i < rayCount; i++)
        {
            float angle = Mathf.Lerp(-halfCone, halfCone, i / (float)(rayCount - 1));
            Quaternion rotation = Quaternion.AngleAxis(angle, transform.up);
            Vector3 rayDirection = rotation * transform.forward;

            RaycastHit hit;
            if (Physics.Raycast(rayOrigin, rayDirection, out hit, rayDistance, obstacleLayer))
            {
                Debug.DrawRay(rayOrigin, rayDirection * hit.distance, Color.red);
                avoidDirection += (rayOrigin - hit.point).normalized;
                hitCount++;
            }
            else
            {
                Debug.DrawRay(rayOrigin, rayDirection * rayDistance, Color.green);
            }
        }

        Vector3 moveDirection;

        if (hitCount > 0)
        {
            // Avoid obstacles
            avoidDirection /= hitCount;
            avoidDirection.y = 0;
            avoidDirection.Normalize();
            moveDirection = avoidDirection;
        }
        else
        {
            // Move toward waypoint
            moveDirection = directionToWP.normalized;
        }

        // Rotation with snapping for sharp turns
        float angleToMoveDir = Vector3.Angle(transform.forward, moveDirection);
        Quaternion targetRotation = Quaternion.LookRotation(moveDirection, Vector3.up);

        if (angleToMoveDir > snapTurnAngle)
        {
            transform.rotation = targetRotation; // Snap turn
        }
        else
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);
        }

        // Move forward
        transform.Translate(Vector3.forward * speed * Time.deltaTime);

        // Check if reached current waypoint
        if (directionToWP.magnitude < accuracy)
        {
            currentWP++;
            if (currentWP >= waypoints.Length)
            {
                currentWP = 0;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Treasure"))
        {
            hasWon = true;
            Debug.Log("Treasure reached! Level complete.");
        }
    }
}