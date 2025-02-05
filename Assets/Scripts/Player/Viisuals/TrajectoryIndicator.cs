using UnityEngine;

public class TrajectoryIndicator : MonoBehaviour
{
    [Header("Trajectory Settings")]
    [Tooltip("Number of points in the trajectory line.")]
    public int numPoints = 30;
    [Tooltip("Time interval (seconds) between trajectory points.")]
    public float timeStep = 0.1f;
    [Tooltip("Layer mask to check for obstacles along the trajectory.")]
    public LayerMask collisionLayers;

    [Header("Player Settings")]
    [Tooltip("Reference to the player's Rigidbody.")]
    public Rigidbody playerRb;
    [Tooltip("Reference to the player's state script.")]
    public PlayerStates playerStates;

    [Tooltip("LineRenderer for the trajectory indicator (assigned from the TrajectoryLine child).")]
    public LineRenderer trajectoryLineRenderer;

    private void Start()
    {
        if (trajectoryLineRenderer == null)
        {
            Debug.LogError("Trajectory LineRenderer is not assigned!");
            return;
        }

        // Initialize the trajectory line with the desired number of points.
        trajectoryLineRenderer.positionCount = numPoints;
        trajectoryLineRenderer.enabled = false;
    }

    private void Update()
    {
        // Check for PlayerStates if not already assigned.
        if (playerStates == null)
        {
            playerStates = GetComponent<PlayerStates>();
            if (playerStates == null)
            {
                Debug.LogWarning("PlayerStates component not found on " + gameObject.name);
                return;
            }
        }

        // Only show the trajectory indicator when the player is airborne.
        if (!playerStates.isGrounded)
        {
            trajectoryLineRenderer.enabled = true;
            RenderTrajectory();
        }
        else
        {
            trajectoryLineRenderer.enabled = false;
        }
    }

    private void RenderTrajectory()
    {
        if (playerRb == null)
        {
            Debug.LogWarning("Player Rigidbody is not assigned!");
            return;
        }

        // Starting position and the current velocity.
        Vector3 startPos = playerRb.position;
        Vector3 initialVelocity = playerRb.linearVelocity;

        // Prepare an array for trajectory points.
        Vector3[] points = new Vector3[numPoints];
        points[0] = startPos;

        Vector3 prevPoint = startPos;
        bool collisionDetected = false;

        // Calculate each trajectory point using the projectile motion equation.
        for (int i = 1; i < numPoints; i++)
        {
            float t = i * timeStep;
            Vector3 point = startPos + initialVelocity * t + 0.5f * Physics.gravity * t * t;

            // If no collision has been detected, check for obstacles between points.
            if (!collisionDetected)
            {
                Ray ray = new Ray(prevPoint, point - prevPoint);
                float dist = Vector3.Distance(prevPoint, point);
                if (Physics.Raycast(ray, out RaycastHit hit, dist, collisionLayers))
                {
                    point = hit.point;
                    collisionDetected = true;

                    // Optionally, reduce the number of displayed points.
                    trajectoryLineRenderer.positionCount = i + 1;
                }
            }

            points[i] = point;
            prevPoint = point;
        }

        // Update the LineRenderer with the calculated trajectory.
        trajectoryLineRenderer.SetPositions(points);
    }
}
