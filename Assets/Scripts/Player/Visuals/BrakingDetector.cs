using UnityEngine;

namespace GASHAPWN {
    public class BrakingDetector : MonoBehaviour
    {
        [SerializeField] private ParticleSystem dustParticles;
        [SerializeField] private float brakingThreshold = 5f;

        private Rigidbody rb;
        private Vector3 lastVelocity;

        private void Start()
        {
            rb = GetComponent<Rigidbody>();
            lastVelocity = rb.linearVelocity;
        }

        private void FixedUpdate()
        {
            Vector3 currentVelocity = rb.linearVelocity;
            float speedDelta = (lastVelocity - currentVelocity).magnitude;

            // Trigger dust if rapid deceleration
            if (speedDelta > brakingThreshold && currentVelocity.magnitude < lastVelocity.magnitude)
            {
                EmitDust();
            }

            lastVelocity = currentVelocity;
        }

        private void EmitDust()
        {
            if (dustParticles != null)
            {
                dustParticles.Play();
            }
        }

        public void SetDustParticles(ParticleSystem ps)
        {
            dustParticles = ps;
        }
    }
}
