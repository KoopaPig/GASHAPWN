using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class AirborneIndicator : MonoBehaviour
{
    private PlayerData playerData;

    [Header("Line Renderer Settings")]
    public LineRenderer airborneLineRenderer;
    public float maxRayDistance = 100f;

    [Header("Slam Target Settings")]
    [SerializeField] private DecalProjector targetProjector;
    private float minDistance = 7f;
    private Vector3 targetProjectorInitialSize;

    [SerializeField] private LayerMask groundLayer;

    private void Awake()
    {
        playerData = GetComponent<PlayerData>();
        targetProjectorInitialSize = targetProjector.size;
    }

    private void OnEnable()
    {
        playerData.OnSlam.AddListener(HandleTargetEffect);
    }

    private void Start()
    {
        airborneLineRenderer.positionCount = 2;
        airborneLineRenderer.enabled = false;
        targetProjector.enabled = false;
    }

    private void Update()
    {
        // Deactivate if player is dead
        if (playerData.isDead)
        {
            airborneLineRenderer.enabled = false;
            targetProjector.enabled = false;
            return;
        }

        if (!playerData.isGrounded)
        {
            airborneLineRenderer.enabled = true;

            Vector3 startPosition = transform.position;
            Vector3 endPosition;

            if (Physics.Raycast(startPosition, Vector3.down, out RaycastHit hit, maxRayDistance, groundLayer))
            {
                endPosition = hit.point;

                float distanceToGround = Vector3.Distance(startPosition, endPosition);

                // Only activate targetProjector if certain distance from ground
                if (distanceToGround > minDistance)
                {
                    // Activate and position tragetProjector
                    targetProjector.enabled = true;
                    targetProjector.transform.SetPositionAndRotation(hit.point + Vector3.down * 0.1f, Quaternion.LookRotation(hit.normal));
                } else
                {
                    targetProjector.enabled = false;
                }
            }
            else 
            {
                // no point hit, so set end position based on maxRayDistance
                endPosition = startPosition + Vector3.down * maxRayDistance;
                targetProjector.enabled = false;
            }

            airborneLineRenderer.SetPosition(0, startPosition);
            airborneLineRenderer.SetPosition(1, endPosition);
        }
        else // deactivate if grounded
        {
            airborneLineRenderer.enabled = false;
            targetProjector.enabled = false;
        }
    }

    private void HandleTargetEffect()
    {
        StartCoroutine(TargetEffect((targetProjector.size * 1.25f), playerData.slamAirborneTime));
    }


    // Increases the size of the targetProjector for given duration, then sets back to initial size
    private IEnumerator TargetEffect(Vector3 targetSize, float duration)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            // Smooth interpolate
            targetProjector.size = Vector3.Lerp(targetProjectorInitialSize, targetSize, t);
            yield return null;
        }

        targetProjector.size = targetSize; // Ensure exact final size
        yield return null;
        targetProjector.size = targetProjectorInitialSize;
    }

    private void OnDisable()
    {
        playerData.OnSlam.RemoveListener(HandleTargetEffect);
    }
}