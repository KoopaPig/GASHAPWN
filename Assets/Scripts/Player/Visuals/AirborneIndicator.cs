using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace GASHAPWN
{
    public class AirborneIndicator : MonoBehaviour
    {
        private PlayerData playerData;
        [SerializeField] private GhostTrailEffect ghostTrailEffect;

        [Header("Line Renderer Settings")]
            public LineRenderer airborneLineRenderer;
            public float maxRayDistance = 100f;

        [Header("Slam Target Settings")]
            [SerializeField] private DecalProjector targetProjector;
            [SerializeField] private float minDistance = 10f;
            private Vector3 targetProjectorInitialSize;

        [SerializeField] private LayerMask groundLayer;

        [SerializeField] private Color baseLineColor;
        [SerializeField] private Color targetLineColor;

        private float targetAlpha;

        private void Awake()
        {
            playerData = GetComponentInParent<PlayerData>();
            targetProjectorInitialSize = targetProjector.size;
            airborneLineRenderer.startColor = baseLineColor;
            airborneLineRenderer.endColor = baseLineColor;
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

                    targetAlpha = Mathf.Clamp01(distanceToGround / minDistance);
                    airborneLineRenderer.startColor = new Color(baseLineColor.r, baseLineColor.g, baseLineColor.b, targetAlpha);
                    airborneLineRenderer.endColor = new Color(baseLineColor.r, baseLineColor.g, baseLineColor.b, targetAlpha);

                    // Only activate targetProjector if certain distance from ground
                    if (distanceToGround > minDistance)
                    {
                        // Activate and position tragetProjector
                        targetProjector.enabled = true;
                        targetProjector.transform.SetPositionAndRotation(hit.point + Vector3.down * 0.1f, Quaternion.LookRotation(hit.normal));
                    }
                    else
                    {
                        targetProjector.enabled = false;
                    }
                }
                else
                {
                    // no point hit, so set end position based on maxRayDistance
                    endPosition = startPosition + Vector3.down * maxRayDistance;
                    airborneLineRenderer.startColor = Color.clear;
                    airborneLineRenderer.endColor = Color.clear;
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
            // Start ghost trail effect on slam
            if (ghostTrailEffect != null)
            {
                StartCoroutine(SlamTrailEffect(playerData.slamAirborneTime));
            }
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
                airborneLineRenderer.endColor = Color.Lerp(baseLineColor, targetLineColor, t);
                yield return null;
            }

            airborneLineRenderer.endColor = targetLineColor;
            targetProjector.size = targetSize; // Ensure exact final size
            yield return null;
            targetProjector.size = targetProjectorInitialSize;
            airborneLineRenderer.endColor = baseLineColor;
        }

        private IEnumerator SlamTrailEffect(float duration)
        {
            ghostTrailEffect.StartTrail();
            yield return new WaitForSeconds(duration);
            ghostTrailEffect.StopTrail();
        }

        private void OnDisable()
        {
            playerData.OnSlam.RemoveListener(HandleTargetEffect);
        }
    }
}