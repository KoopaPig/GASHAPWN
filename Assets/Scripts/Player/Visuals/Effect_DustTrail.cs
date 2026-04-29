using GASHAPWN.Utility;
using UnityEngine;
using UnityEngine.Android;
using static UnityEngine.Rendering.DebugUI.Table;

namespace GASHAPWN
{
    // Movement Dust: A trail of dust appears behind the player proportional to their speed.
    // If a sudden increase of speed happens (a-la charge roll release), a big puff of dust expels.
    [RequireComponent(typeof(PlayerEffectsHub))]
    public class Effect_DustTrail : MonoBehaviour
    {
        [Header("Particle Effects")]
            [SerializeField] private ParticleSystem dustTrail;
            [SerializeField] private ParticleSystem dustBurstPrefab;
            [SerializeField] private ParticleSystem brakeDustPrefab;

        [Header("Settings")]
            [SerializeField] private float accelThreshold = 0.15f;

        // Get reference to higher-level player components through Player Effects Hub
        private PlayerEffectsHub _pEffectsHub;

        private float _maxSpeed;

        //private float _speedSampleTimer;
        private float _lastSampledSpeed;

        private void Awake()
        {
            _pEffectsHub = GetComponent<PlayerEffectsHub>();
            _maxSpeed = _pEffectsHub.pController.MoveSpeed;
        }

        private void OnEnable()
        {
            _pEffectsHub.pSpecialMoveHandler.Events.OnDefenseActivated.AddListener(EmitBrakeDust);
        }

        private void OnDisable()
        {
            _pEffectsHub.pSpecialMoveHandler.Events.OnDefenseActivated.RemoveListener(EmitBrakeDust);
        }

        private void Update()
        {
            Vector3 velocity = _pEffectsHub.rb.linearVelocity;
            float speed = velocity.magnitude;

            float deltaSpeed = speed - _lastSampledSpeed;

            HandleTrail(speed);

            if (deltaSpeed > accelThreshold)
            {
                EmitBurst(velocity);
            }

            _lastSampledSpeed = speed;
        }

        private void HandleTrail(float speed)
        {
            var emission = dustTrail.emission;
            // Normalize speed (tune this max value)
            float normalizedSpeed = Mathf.InverseLerp(0f, _maxSpeed, speed);
            emission.rateOverTime = Mathf.Lerp(0f, 5f, normalizedSpeed);
        }

        private void EmitBurst(Vector3 velocity)
        {
            if (velocity.sqrMagnitude < 0.001f) return;

            // Opposite direction of movement
            Vector3 dir = -velocity.normalized;

            GameObject obj = Instantiate(dustBurstPrefab.gameObject, this.transform);
            obj.transform.parent = null;

            // Rotate emitter so burst goes backward
            obj.transform.rotation = Quaternion.LookRotation(dir);

            var ps = obj.GetComponent<ParticleSystem>();
            ps.Play();
            StartCoroutine(PlayerHelpers.DestroyParticleSystemWhenDone(ps));
        }

        private void EmitBrakeDust()
        {
            Vector3 velocity = _pEffectsHub.rb.linearVelocity;

            if (velocity.sqrMagnitude < 0.08f) return;
            ParticleSystem ps = Instantiate(brakeDustPrefab);

            // Position at bottom of player
            Vector3 bottomOffset = Vector3.down * 0.01f;
            ps.transform.position = transform.position + bottomOffset;

            // Face opposite movement direction
            ps.transform.rotation = Quaternion.LookRotation(velocity.normalized, Vector3.up);

            // Play
            ps.Play();
            StartCoroutine(PlayerHelpers.DestroyParticleSystemWhenDone(ps));
        }
    }
}