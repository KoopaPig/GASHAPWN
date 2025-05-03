using UnityEngine;

public class AirborneIndicator : MonoBehaviour
{
    public PlayerData playerData;
    public LineRenderer airborneLineRenderer;
    public float maxRayDistance = 100f;

    private void Start()
    {
        airborneLineRenderer.positionCount = 2;
        airborneLineRenderer.gameObject.SetActive(true);
        airborneLineRenderer.enabled = false;
    }

    private void Update()
    {
        // Deactivate if !playerData.controlsEnabled
        if (!playerData.controlsEnabled)
        {
            airborneLineRenderer.gameObject.SetActive(false);
            return;
        }

        if (!playerData.isGrounded)
        {
            airborneLineRenderer.enabled = true;

            Vector3 startPosition = transform.position;
            Vector3 endPosition;

            if (Physics.Raycast(startPosition, Vector3.down, out RaycastHit hit, maxRayDistance))
            {
                endPosition = hit.point;
            }
            else
            {
                endPosition = startPosition + Vector3.down * maxRayDistance;
            }

            airborneLineRenderer.SetPosition(0, startPosition);
            airborneLineRenderer.SetPosition(1, endPosition);
        }
        else
        {
            airborneLineRenderer.enabled = false;
        }
    }
}
