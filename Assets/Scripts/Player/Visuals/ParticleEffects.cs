using GASHAPWN.Utility;
using UnityEngine;

namespace GASHAPWN
{
    /// <summary>
    /// Controller for impact-related graphic and particle effects
    /// </summary>
    public class ParticleEffects : MonoBehaviour
    {
        [Header("Graphic Effects")]
            [SerializeField] private GraphicEffect strikeEffectPrefab;
            [SerializeField] private GraphicEffect shieldEffectPrefab;

        [Header("Particle Effects")]
            [SerializeField] ParticleSystem hitSparksPrefab;
            [SerializeField] ParticleSystem deflectSparksPrefab;

        private PlayerEffectsHub _pEffectsHub;

        private void Awake()
        {
            _pEffectsHub = GetComponent<PlayerEffectsHub>();
        }

        private void OnEnable()
        {
            _pEffectsHub.pData.healthEvents.OnDeflect.AddListener(SpawnShield);
            _pEffectsHub.pData.healthEvents.OnHit.AddListener(SpawnStrike);
        }

        private void OnDisable()
        {
            _pEffectsHub.pData.healthEvents.OnDeflect.RemoveListener(SpawnShield);
            _pEffectsHub.pData.healthEvents.OnHit.RemoveListener(SpawnStrike);
        }

        private void SpawnShield(ContactPoint contactPoint, Transform other) { 
            shieldEffectPrefab.SpawnAtContact(contactPoint, other, 0.3f, true);
            shieldEffectPrefab.FlashAndShrink(0.2f);

            // Configure deflect sparks
            GameObject obj = Instantiate(deflectSparksPrefab.gameObject, contactPoint.point, Quaternion.identity);

            var ps = obj.GetComponent<ParticleSystem>();

            // Direction away from the other object
            Vector3 dir = (contactPoint.point - other.position).normalized;

            obj.transform.rotation = Quaternion.LookRotation(dir);

            ps.Play();
            StartCoroutine(PlayerHelpers.DestroyParticleSystemWhenDone(ps));
        }

        private void SpawnStrike(ContactPoint contactPoint, Transform other)
        {
            strikeEffectPrefab.SpawnAtContact(contactPoint, other, 0.5f, true);
            strikeEffectPrefab.FlashAndShrink(0.25f);

            // Configure hit sparks
            GameObject obj = Instantiate(hitSparksPrefab.gameObject, contactPoint.point, Quaternion.identity);

            var ps = obj.GetComponent<ParticleSystem>();

            // Direction away from the other object
            Vector3 dir = (contactPoint.point - other.position).normalized;

            obj.transform.rotation = Quaternion.LookRotation(dir);

            ps.Play();
            StartCoroutine(PlayerHelpers.DestroyParticleSystemWhenDone(ps));
        }
    }
}