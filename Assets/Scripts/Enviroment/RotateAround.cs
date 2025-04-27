using UnityEngine;

namespace GASHAPWN
{
    public class RotateAround : MonoBehaviour
    {
        public float speed = 10f; // Speed of rotation
        public float radius = 5f; // Distance from the center of the circular path

        private float angle = 0f;
        private Vector3 centerPoint;
        private Vector3 previousPosition;

        void Start()
        {
            // Store the initial position as the center point
            centerPoint = transform.position;
            previousPosition = transform.position + new Vector3(radius, 0, 0); // Initial offset
        }

        void Update()
        {
            angle += speed * Time.deltaTime; // Increase the angle over time
            float x = Mathf.Cos(angle) * radius;
            float z = Mathf.Sin(angle) * radius;

            // Update position relative to the center point
            Vector3 newPosition = centerPoint + new Vector3(x, 0, z);
            transform.position = newPosition;

            Vector3 movementDirection = (newPosition - previousPosition).normalized;
            if (movementDirection != Vector3.zero)
            {
                transform.rotation = Quaternion.LookRotation(movementDirection);
            }

            // Update previous position for next frame
            previousPosition = newPosition;
        }
    }
}

