using GASHAPWN;
using UnityEngine;

public class TrajectoryIndicator : MonoBehaviour
{
    [Header("Trajectory Settings")]
        [SerializeField] private int numPoints = 30;
        [SerializeField] private float timeStep = 0.1f;
        [SerializeField] private LayerMask collisionLayers;
        [SerializeField] private LineRenderer trajectoryLineRenderer;
        
    private Rigidbody _rb;
    private PlayerData _pData;
    private PlayerController _pController;
    private SpecialMoveHandler _pSpecialMoveHandler;

    private void Awake()
    {
        _pData = GetComponentInParent<PlayerData>();
        _pController = GetComponentInParent<PlayerController>();
        _rb = _pController.rb;
        _pSpecialMoveHandler = GetComponentInParent<SpecialMoveHandler>();
    }

    private void Start()
    {
        trajectoryLineRenderer.positionCount = numPoints;
        trajectoryLineRenderer.enabled = false;
    }

    private void Update()
    {
        if (_pData.IsDead)
        {
            trajectoryLineRenderer.enabled = false;
            return;
        }

        Vector3 horizontalVel = new Vector3(_pController.rb.linearVelocity.x, 0f, _pController.rb.linearVelocity.z);

        // Only enable when not grounded, not slamming, and horizontal velocity is great enough
        if (!_pController.IsGrounded && !_pSpecialMoveHandler.HasSlammed && horizontalVel.sqrMagnitude > 0.015f) {
            trajectoryLineRenderer.enabled = true;
            RenderTrajectory();
        }
        else trajectoryLineRenderer.enabled = false;        
    }

    // Render the trajectory line (draws a line based on velocity and predicted landing position)
    private void RenderTrajectory()
    {
        Vector3 startPos = _rb.position;
        Vector3 initialVelocity = _rb.linearVelocity;

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