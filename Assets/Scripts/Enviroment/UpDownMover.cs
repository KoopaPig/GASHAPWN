using UnityEngine;

namespace GASHAPWN.Environment
{
    public class UpDownMover : MonoBehaviour
    {
        [Header("Base Movement")]
            [SerializeField] private float moveDistance = 0.03f;
            [SerializeField] private float moveSpeed = 0.95f;

            private Vector3 startPosition;
            private float timeOffset;
            private float baseOffset;


        private void Start()
        {
            startPosition = transform.position;

            // Start at random point in cycle
            timeOffset = Random.Range(0f, Mathf.PI * 2f);
        }

        private void Update()
        {
            HandleOscillation();
            ApplyFinalPosition();
        }

        private void HandleOscillation()
        {
            float t = Time.time * moveSpeed + timeOffset;
            float yOffset = Mathf.Sin(t) * moveDistance;

            baseOffset = yOffset;
        }

        private void ApplyFinalPosition()
        {
            Vector3 pos = startPosition;
            pos.y += baseOffset;
            transform.position = pos;
        }
    }
}