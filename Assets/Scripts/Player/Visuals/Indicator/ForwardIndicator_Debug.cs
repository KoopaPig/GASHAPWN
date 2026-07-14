using UnityEngine;

namespace GASHAPWN.Utility
{
    /// <summary>
    /// Debug script to show the forward direction of the player
    /// </summary>
    public class ForwardIndicator_Debug : MonoBehaviour
    {
        private PlayerController _pController;
        [SerializeField] private float lineLength = 0.1f;

        [SerializeField] private LineRenderer forwardLineRenderer;

        private void Awake()
        {
            forwardLineRenderer.positionCount = 2;
            _pController = GetComponentInParent<PlayerController>();
        }

        private void Update()
        {
            Vector3 forward = _pController.MovementForward;
            if (forward.sqrMagnitude < 0.001f) return;

            forward.y = 0f;
            forward.Normalize();

            Vector3 start = _pController.transform.position;
            Vector3 end = start + forward * lineLength;

            forwardLineRenderer.SetPosition(0, start);
            forwardLineRenderer.SetPosition(1, end);
        }
    }
}
