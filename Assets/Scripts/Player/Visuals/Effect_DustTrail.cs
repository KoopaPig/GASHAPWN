using UnityEngine;

namespace GASHAPWN
{
    // Movement Dust: A trail of dust appears behind the player proportional to their speed.
    // If a sudden increase of speed happens (a-la charge roll release), a big puff of dust expels.
    [RequireComponent(typeof(PlayerEffectsHub))]
    public class Effect_DustTrail : MonoBehaviour
    {
        [SerializeField] private ParticleSystem dustTrail;
        [SerializeField] private float sampleInterval = 0.1f;


        [SerializeField] private float accelThreshold = 0.15f;
        [SerializeField] private float brakeThreshold = -0.15f;
        [SerializeField] private int burstAmount = 20;

        // Get reference to higher-level player components through Player Effects Hub
        private PlayerEffectsHub _pEffectsHub;

        private float _maxSpeed;

        private float _speedSampleTimer;
        private float _lastSampledSpeed;

        private void Awake()
        {
            _pEffectsHub = GetComponent<PlayerEffectsHub>();
            _maxSpeed = _pEffectsHub.pController.MoveSpeed;
        }

        private void Update()
        {
            Vector3 velocity = _pEffectsHub.rb.linearVelocity;
            float speed = velocity.magnitude;

            //_speedSampleTimer += Time.deltaTime;

            //if (_speedSampleTimer >= sampleInterval)
            //{
            //    float deltaSpeed = speed - _lastSampledSpeed;

            //    EvaluateBurst(_rb.linearVelocity, deltaSpeed);

            //    _lastSampledSpeed = speed;
            //    _speedSampleTimer = 0f;
            //}

            HandleTrail(speed);
        }

        private void HandleTrail(float speed)
        {
            var emission = dustTrail.emission;

            // Normalize speed (tune this max value)
            float normalizedSpeed = Mathf.InverseLerp(0f, _maxSpeed, speed);

            emission.rateOverTime = Mathf.Lerp(0f, 5f, normalizedSpeed);

        }
        //private void EvaluateBurst(Vector3 velocity, float deltaSpeed)
        //{
        //    if (velocity.sqrMagnitude < 0.1f) return;

        //    Vector3 dir = velocity.normalized;

        //    if (deltaSpeed > accelThreshold)
        //    {
        //        EmitBurst(-dir);
        //    }
        //    else if (deltaSpeed < brakeThreshold)
        //    {
        //        EmitBurst(dir);
        //    }
        //}
        //private void EmitBurst(Vector3 direction)
        //{
        //    // Rotate system to face direction
        //    dustTrail.transform.rotation = Quaternion.LookRotation(direction);

        //    dustTrail.Emit(burstAmount);
        //}

    }
}
