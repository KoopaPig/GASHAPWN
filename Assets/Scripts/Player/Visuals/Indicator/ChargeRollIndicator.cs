using UnityEngine;

namespace GASHAPWN
{
    public class ChargeRollIndicator : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer arrow;
        [SerializeField] private Transform centerPoint;

        [SerializeField] private float maxWidth = 5f;
        private float _height;

        private void Start()
        {
            arrow.gameObject.SetActive(false);
            if (arrow.drawMode != SpriteDrawMode.Sliced)
                Debug.LogError($"{nameof(ChargeRollIndicator)}: Arrow is not set up with a sliced draw mode.");
            _height = arrow.size.y;
        }

        public void UpdateIndicator(float percent, Vector2 chargeDirection)
        {
            if (!arrow.gameObject.activeSelf)
                arrow.gameObject.SetActive(true);

            arrow.size = new Vector2(Mathf.Lerp(0.1f, maxWidth, percent), _height);

            // Get ground normal
            Vector3 groundNormal = Vector3.up;

            if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, 2f))
                groundNormal = hit.normal;

            // Direction projected onto ground
            Vector3 dir = new Vector3(chargeDirection.x, 0f, chargeDirection.y).normalized;
            Vector3 projectedDir = Vector3.ProjectOnPlane(dir, groundNormal).normalized;

            // Rotation aligned to ground
            Quaternion lookRot = Quaternion.LookRotation(projectedDir, groundNormal);
            arrow.transform.rotation = lookRot * Quaternion.Euler(90f, -90f, 0f);
        }

        public void HideIndicator()
        {
            arrow.gameObject.SetActive(false);
        }
    }
}