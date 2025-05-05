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
        public GameObject dustImpactPrefab;
        public ParticleSystem dustTrail;

        public float sparkThreshold = 8f;
        public float dustThreshold = 6f;
        public float trailSpeedThreshold = 10f;

        private Rigidbody rb;

        void Start()
        {
            rb = GetComponent<Rigidbody>();
            dustTrail.Stop();
        }

        void Update()
        {
            if (rb.linearVelocity.magnitude >= trailSpeedThreshold)
            {
                if (!dustTrail.isPlaying)
                dustTrail.Play();
            }
            else
            {
                if (dustTrail.isPlaying) dustTrail.Stop();
            }
        }

        void OnCollisionEnter(Collision collision){
            float speed = collision.relativeVelocity.magnitude;

            ContactPoint contact = collision.contacts[0];
            Quaternion rot = Quaternion.LookRotation(contact.normal);

            if (collision.gameObject.CompareTag("Player") && speed >= sparkThreshold){
                Instantiate(sparksPrefab, contact.point, rot);
                Debug.Log("Sparks flew!");
            }
            else if (!collision.gameObject.CompareTag("Player") && speed >= dustThreshold){
                Instantiate(dustImpactPrefab, contact.point, rot);
            }
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

        public void PlayDeflectEffect(Vector3 position)
        {
            if (deflectEffectPrefab != null)
            {
                Instantiate(deflectEffectPrefab, position, Quaternion.identity);
            }
            else
            {
                Debug.LogWarning("Deflect effect prefab is not assigned!");
            }
        }
    }
}
