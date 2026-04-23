using UnityEngine;

namespace GASHAPWN
{
    public class ParticleEffects : MonoBehaviour
    {
        [Header("Impact Effects")]
        public GameObject hitEffectPrefab;
        public GameObject deflectEffectPrefab;

        [Header("Particle Effects")]
        public GameObject sparksPrefab;

        public float sparkThreshold = 8f;
        public float dustThreshold = 6f;
        public float trailSpeedThreshold = 10f;

        private Rigidbody _rb;

        private PlayerEffectsHub _pEffectsHub;

        private void Awake()
        {
            _pEffectsHub = GetComponent<PlayerEffectsHub>();
            _rb = _pEffectsHub.rb;
        }

        void OnCollisionEnter(Collision collision)
        {
            float speed = _rb.linearVelocity.magnitude;

            // DISABLED FOR NOW BECAUSE NOT PROPERLY MANAGED
            
            //if (collision.gameObject.CompareTag("Player") && speed >= sparkThreshold)
            //{
            //    Instantiate(sparksPrefab, collision.contacts[0].point, Quaternion.identity);
            //}
            //else if (!collision.gameObject.CompareTag("Player") && speed >= dustThreshold)
            //{
            //    Instantiate(dustImpactPrefab, collision.contacts[0].point, Quaternion.identity);
            //}
        }

        public void PlayHitEffect(Vector3 position)
        {
            if (hitEffectPrefab != null)
            {
                Instantiate(hitEffectPrefab, position, Quaternion.identity);
            }
            else
            {
                Debug.LogWarning("Hit effect prefab is not assigned!");
            }
        }
    }
}
