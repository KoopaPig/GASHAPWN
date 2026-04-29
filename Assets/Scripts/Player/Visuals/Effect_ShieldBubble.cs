using DG.Tweening;
using MyBox;
using UnityEngine;
using UnityEngine.EventSystems;

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
        [SerializeField] private GameObject shieldBubble;

        // Get reference to higher-level player components through Player Effects Hub
        private PlayerEffectsHub _pEffectsHub;

        private void Awake()
        {
            _pEffectsHub = GetComponent<PlayerEffectsHub>();
            shieldBubble.gameObject.SetActive(false);
        }

        private void FixedUpdate()
        {
            if (_pEffectsHub.pSpecialMoveHandler.IsDefending)
            {
                if (!shieldBubble.activeSelf) shieldBubble.gameObject.SetActive(true);

                float percent = _pEffectsHub.pSpecialMoveHandler.ShieldTimer / _pEffectsHub.pSpecialMoveHandler.MaxShieldDuration;
                float scale = Mathf.Lerp(minShieldSize, maxShieldSize, percent);

                shieldBubble.transform.localScale = Vector3.one * scale;
            }
            else
            {
                if (shieldBubble.activeSelf) shieldBubble.gameObject.SetActive(false);
            }
        }

        private void OnEnable()
        {
            _pEffectsHub.pSpecialMoveHandler.Events.OnDefenseActivated.AddListener(Brake);
        }

        private void OnDisable()
        {
            _pEffectsHub.pSpecialMoveHandler.Events.OnDefenseActivated.RemoveListener(Brake);
        }

        private void Brake() => _pEffectsHub.CapsuleAnimator.SetTrigger(AnimationStrings.isBrake);
    }
}