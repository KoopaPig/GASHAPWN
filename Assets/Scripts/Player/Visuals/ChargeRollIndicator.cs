using UnityEngine;
using System.Collections;

public class ChargeRollIndicator : MonoBehaviour
{
    [Header("Charge Roll Settings")]
    public PlayerData playerData;
    public LineRenderer chargeLineRenderer;
    public float maxLineLength = 10f;

    [Header("Ghost Trail Settings")]
    public float trailDuration = 0.5f;

    private Vector3 chargeDirection;
    private GhostTrailEffect ghostTrailEffect;
    private Coroutine trailCoroutine;

    private void Start()
    {
        ghostTrailEffect = GetComponent<GhostTrailEffect>();
        chargeLineRenderer.positionCount = 2;
        chargeLineRenderer.enabled = false;
    }

    public void UpdateIndicator(float chargePercent, Vector3 direction)
    {
        chargeLineRenderer.enabled = true;
        chargeDirection = direction.normalized;

        float lineLength = Mathf.Lerp(0.5f, maxLineLength, chargePercent);
        Vector3 startPosition = transform.position;
        Vector3 endPosition = startPosition + chargeDirection * lineLength;

        chargeLineRenderer.SetPosition(0, startPosition);
        chargeLineRenderer.SetPosition(1, endPosition);
    }

    public void HideIndicator()
    {
        chargeLineRenderer.enabled = false;

        // Start the ghost trail for visual feedback
        if (ghostTrailEffect != null)
        {
            if (trailCoroutine != null)
                StopCoroutine(trailCoroutine);

            trailCoroutine = StartCoroutine(PlayTrail());
        }
    }

    private IEnumerator PlayTrail()
    {
        ghostTrailEffect.StartTrail();
        yield return new WaitForSeconds(trailDuration);
        ghostTrailEffect.StopTrail();
    }
}
