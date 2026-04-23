using UnityEngine;

namespace GASHAPWN
{
    [RequireComponent(typeof(PlayerEffectsHub))]
    public class Effect_ChargeGlow : MonoBehaviour
    {
        private PlayerEffectsHub _pEffectsHub;

        [SerializeField] private ParticleSystem chargeLines;


        [Header("Emission Settings")]
        [SerializeField] private float maxEmission = 40f;

        private ParticleSystem.EmissionModule _emission;
        private float _currentEmission;
        private bool _isCharging;
        private bool _isHolding;

        private float chargeSpeed = 2f;

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
                _currentEmission = Mathf.MoveTowards(
                    _currentEmission,
                    maxEmission,
                    chargeSpeed * maxEmission * Time.deltaTime
                );

                ApplyEmission();
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

        private void UpdateState(ChargeRoll_SpecialMove.ChargeRollState state)
        {
            switch (state)
            {
                case ChargeRoll_SpecialMove.ChargeRollState.Charge:
                    chargeLines.gameObject.SetActive(true);

                    _isCharging = true;
                    _isHolding = false;
                    break;

                case ChargeRoll_SpecialMove.ChargeRollState.Hold:
                    _isCharging = false;
                    _isHolding = true;

                    _currentEmission = maxEmission;
                    ApplyEmission();
                    break;

                default:
                    _isCharging = false;
                    _isHolding = false;

                    _currentEmission = 0f;
                    ApplyEmission();

                    chargeLines.gameObject.SetActive(false);
                    break;
            }
        }
    }

}