using DG.Tweening;
using MyBox;
using UnityEngine;

/// <summary>
/// Controller for defense shield bubble
/// </summary>

namespace GASHAPWN
{
    [RequireComponent(typeof(PlayerEffectsHub))]
    public class Effect_ShieldBubble : MonoBehaviour
    {
        [Min(0.005f), SerializeField] private float minShieldSize = 0.04f;
        [Min(0.005f), SerializeField] private float maxShieldSize = 0.05f;

        [Tooltip("Reference to shield bubble child object")]
        [SerializeField] private MeshRenderer shieldBubble;

        [Tooltip("Reference to shield prefab (appears during defend)")]
        [SerializeField] private GameObject pointShield;

        // Get reference to higher-level player components through Player Effects Hub
        private PlayerEffectsHub _pEffectsHub;

        private void Awake()
        {
            _pEffectsHub = GetComponent<PlayerEffectsHub>();
            shieldBubble.gameObject.SetActive(false);
        }

        private void Update()
        {
            if (_pEffectsHub.pSpecialMoveHandler.IsDefending)
            {
                if (!shieldBubble.gameObject.activeSelf) shieldBubble.gameObject.SetActive(true);

                float percent = _pEffectsHub.pSpecialMoveHandler.ShieldTimer / _pEffectsHub.pSpecialMoveHandler.MaxShieldDuration;
                float scale = Mathf.Lerp(minShieldSize, maxShieldSize, percent);

                shieldBubble.transform.localScale = Vector3.one * scale;
            }
            else
            {
                if (shieldBubble.gameObject.activeSelf) shieldBubble.gameObject.SetActive(false);
            }
        }

        private void OnEnable()
        {
            _pEffectsHub.pData.healthEvents.OnDeflect.AddListener(SpawnShield);
            _pEffectsHub.pSpecialMoveHandler.Events.OnDefenseActivated.AddListener(Brake);
        }

        private void OnDisable()
        {
            _pEffectsHub.pData.healthEvents.OnDeflect.RemoveListener(SpawnShield);
            _pEffectsHub.pSpecialMoveHandler.Events.OnDefenseActivated.RemoveListener(Brake);
        }

        private void SpawnShield(ContactPoint contactPoint, Transform other)
        {
            GameObject shield = Instantiate(pointShield, contactPoint.point, Quaternion.identity);

            shield.transform.rotation = Quaternion.LookRotation(-contactPoint.normal);
            
            Vector3 directionToOther = (other.position - contactPoint.point).normalized;
            shield.transform.rotation = Quaternion.LookRotation(directionToOther);

            shield.transform.position += contactPoint.normal * 0.01f;

            FlashShield(shield);
        }

        private void FlashShield(GameObject shield)
        {
            Light light = shield.GetComponent<Light>();
            Vector3 endSize = shield.transform.localScale / 2;

            if (light == null) return;

            light.intensity = 0f;

            // Quick flash up, then fade out
            Sequence seq = DOTween.Sequence();

            seq.Append(light.DOIntensity(0.5f, 0.05f));  // flash on
            seq.Append(light.DOIntensity(0f, 0.25f));   // fade out

            shield.transform.DOScale(endSize, 0.25f);

            // Optional: destroy after
            seq.OnComplete(() => Destroy(shield));
        }

        private void Brake() => _pEffectsHub.CapsuleAnimator.SetTrigger("IsBrake");
    }
}