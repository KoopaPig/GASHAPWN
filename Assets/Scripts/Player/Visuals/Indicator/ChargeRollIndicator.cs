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
                Debug.LogError("CHARGE ROLL INDICATOR: Arrow is not set up with a sliced draw mode.");
            _height = arrow.size.y;
        }

        // This should work differently: The directional input should apply a rotation force to it,
        // otherwise when using keyboard, it can only snap to 8 directions.

        public void UpdateIndicator(float percent, Vector2 chargeDirection)
        {
            Debug.Log("updating indicator: " + percent);

            if (!arrow.gameObject.activeSelf) 
                arrow.gameObject.SetActive(true);

            float angle = Mathf.Atan2(chargeDirection.x, chargeDirection.y) * Mathf.Rad2Deg;

            arrow.size = new Vector2(Mathf.Lerp(0.1f, maxWidth, percent), _height);
            Vector3 dir = new Vector3(chargeDirection.x, 0f, chargeDirection.y);
            arrow.transform.rotation = Quaternion.LookRotation(dir, Vector3.up);
        }

        [SerializeField] private float radius = 0.02f;

        private float currentAngle;



        [SerializeField] private float rotationSpeed = 180f;

        public void HideIndicator()
        {
            arrow.gameObject.SetActive(false);
        }
    }
}