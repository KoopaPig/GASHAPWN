using UnityEngine;

namespace GASHAPWN
{
    [RequireComponent(typeof(PlayerEffectsHub))]
    public class Effect_ChargeGlow : MonoBehaviour
    {
        [Tooltip("Reference to \"Charge Lines\" Particle System")]
        [SerializeField] private ParticleSystem chargeLines;

        [Tooltip("Maximum particle emission on full charge")]
        [SerializeField] private float maxEmission = 40f;

        private PlayerEffectsHub _pEffectsHub;
        private ParticleSystem.EmissionModule _emission;
        private float _currentEmission;
        private bool _isCharging;
        private float _chargeDuration = 2f;
        private float _chargeElapsed = 0f;

        private void Awake()
        {
            _pEffectsHub = GetComponent<PlayerEffectsHub>();
            _emission = chargeLines.emission;
            chargeLines.gameObject.SetActive(false);
        }

        private void OnEnable()
        {
            _pEffectsHub.pSpecialMoveHandler.Events.OnChargeRoll.AddListener(UpdateState);
        }

        private void Update()
        {
            if (_isCharging)
            {
                // Ramp up emission
                _currentEmission = Mathf.MoveTowards(_currentEmission, maxEmission,
                (maxEmission / _chargeDuration) * Time.deltaTime);

                ApplyEmission();

                _chargeElapsed += Time.deltaTime;
                float chargePercent = Mathf.Clamp01(_chargeElapsed / _chargeDuration);

                // Ramp up spin speed
                float spin = Mathf.Lerp(1f, 5f, chargePercent);
                _pEffectsHub.CapsuleAnimator.SetFloat(AnimationStrings.chargeSpeed, spin);
            }
        }

        private void OnDisable()
        {
            _pEffectsHub.pSpecialMoveHandler.Events.OnChargeRoll.RemoveListener(UpdateState);
        }

        private void ApplyEmission()
        {
            var rate = _emission.rateOverTime;
            rate.constant = _currentEmission;
            _emission.rateOverTime = rate;
        }

        // Change particle state based on Charge Roll State
        private void UpdateState(ChargeRoll_SpecialMove.ChargeRollState state)
        {
            switch (state)
            {
                case ChargeRoll_SpecialMove.ChargeRollState.Charge:
                    _isCharging = true;
                    chargeLines.gameObject.SetActive(true);
                    chargeLines.Play();
                    _pEffectsHub.CapsuleAnimator.SetBool(AnimationStrings.isCharging, true);
                    break;

                case ChargeRoll_SpecialMove.ChargeRollState.Hold:
                    _isCharging = false;
                    _currentEmission = maxEmission;
                    ApplyEmission();
                    break;

                default:
                    _isCharging = false;
                    _currentEmission = 0f;
                    _chargeElapsed = 0f;
                    ApplyEmission();
                    chargeLines.Stop();
                    _pEffectsHub.CapsuleAnimator.SetBool(AnimationStrings.isCharging, false);
                    chargeLines.gameObject.SetActive(false);
                    break;
            }
        }
    }
}