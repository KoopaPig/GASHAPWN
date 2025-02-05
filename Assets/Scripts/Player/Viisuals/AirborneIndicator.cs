using UnityEngine;

public class AirborneIndicator : MonoBehaviour
{
    [Tooltip("Reference to the player's state script.")]
    public PlayerStates playerStates;

    [Tooltip("LineRenderer for the airborne indicator (assigned from the AirborneLine child).")]
    public LineRenderer airborneLineRenderer;

    [Tooltip("Maximum distance for the downward raycast.")]
    public float maxRayDistance = 100f;

    private void Start()
    {
        if (airborneLineRenderer == null)
        {
            Debug.LogError("Airborne LineRenderer is not assigned!");
            return;
        }

        // Ensure the LineRenderer only needs two positions.
        airborneLineRenderer.positionCount = 2;
        airborneLineRenderer.enabled = false;
    }

    private void Update()
    {
        // If no PlayerStates reference is set, try to get it from this GameObject.
        if (playerStates == null)
        {
            playerStates = GetComponent<PlayerStates>();
            if (playerStates == null)
            {
                Debug.LogWarning("PlayerStates component not found on " + gameObject.name);
                return;
            }
        }

        // Only show the airborne indicator when the player is not grounded.
        if (!playerStates.isGrounded)
        {
            airborneLineRenderer.enabled = true;

            Vector3 startPosition = transform.position;
            Vector3 endPosition;

            // Cast a ray downward from the player's position.
            if (Physics.Raycast(startPosition, Vector3.down, out RaycastHit hit, maxRayDistance))
            {
                endPosition = hit.point;
            }
            else
            {
                endPosition = startPosition + Vector3.down * maxRayDistance;
            }

            // Set the positions of the line.
            airborneLineRenderer.SetPosition(0, startPosition);
            airborneLineRenderer.SetPosition(1, endPosition);
        }
        else
        {
            // Hide the line when the player is on the ground.
            airborneLineRenderer.enabled = false;
        }
    }
}
