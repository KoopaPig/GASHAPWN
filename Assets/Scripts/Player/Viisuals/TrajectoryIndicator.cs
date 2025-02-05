using UnityEngine;

public class TrajectoryIndicator : MonoBehaviour
{
    [Header("Trajectory Settings")]
    public int numPoints = 30;
    public float timeStep = 0.1f;
    public LayerMask collisionLayers;

    [Header("Player Settings")]
    public Rigidbody playerRb;
    public PlayerStates playerStates;
    public LineRenderer trajectoryLineRenderer;

    private void Start()
    {
        trajectoryLineRenderer.positionCount = numPoints;
        trajectoryLineRenderer.enabled = false;
    }

    private void Update()
    {
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
        Vector3 startPos = playerRb.position;
        Vector3 initialVelocity = playerRb.linearVelocity;

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

        trajectoryLineRenderer.SetPositions(points);
    }
}
