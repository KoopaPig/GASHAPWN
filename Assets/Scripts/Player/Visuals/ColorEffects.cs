using System.Collections;
using UnityEngine;

namespace GASHAPWN
{
    public class ColorEffects : MonoBehaviour
    {
        [Tooltip("Reference to parent container of Player Capsule")]
        [SerializeField] private Transform playerCapsuleRoot;

        [Header("Color Effect Settings")]
            [Tooltip("Flash Effect for damage")]
            [SerializeField] private FlashEffect damageEffect;

            [Tooltip("Hold Effect for defense")]
            [SerializeField] private HoldEffect defenseEffect;

            [Tooltip("Hold Effect for charge")]
            [SerializeField] private HoldEffect chargeEffect;

        // Get reference to higher-level player components through Player Effects Hub
        private PlayerEffectsHub _pEffectsHub;

        // Current coroutine for color effect
        private Coroutine effectCoroutine;

        // Cache current hold effect
        private HoldEffect _currHoldEffect = null;

        private MeshRenderer[] playerRenderers;
        private Color[] originalColors;
        private bool _renderersCached = false;


        /// PRIVATE METHODS ///

        // Logic for separating triggering different subclasses of ColorEffect
        private void TriggerEffect(ColorEffect effect)
        {
            if (effect == null) return;
            if (playerRenderers.Length == 0) return;

            if (effectCoroutine != null)
            {
                StopCoroutine(effectCoroutine);
            }

            // This could pose an issue if MeshRenders switch out mid-battle
            if (!_renderersCached)
            {
                CacheRenderers();
                _renderersCached = true;
            }

            if (effect is FlashEffect)
                effectCoroutine = StartCoroutine(FlashEffectCoroutine(effect as FlashEffect));
            else if (effect is HoldEffect)
                effectCoroutine = StartCoroutine(HoldEffectCoroutine(effect as HoldEffect));

        }

        // Executes given Flash Effect
        private IEnumerator FlashEffectCoroutine(FlashEffect effect)
        {
            for (int i = 0; i < effect.Repetitions; i++)
            {
                // Fade to effect color
                float elapsed = 0f;
                while (elapsed < effect.FadeDuration)
                {
                    elapsed += Time.deltaTime;
                    for (int j = 0; j < playerRenderers.Length; j++)
                        SetRendererColor(j, Color.Lerp(originalColors[j], effect.EffectColor, elapsed / effect.FadeDuration));
                    yield return null;
                }
                for (int j = 0; j < playerRenderers.Length; j++)
                    SetRendererColor(j, effect.EffectColor);

                // Hold for given duration
                if (effect.HoldDuration > 0) yield return new WaitForSeconds(effect.HoldDuration);

                // Fade back to original color
                elapsed = 0f;
                while (elapsed < effect.FadeDuration)
                {
                    elapsed += Time.deltaTime;
                    for (int j = 0; j < playerRenderers.Length; j++)
                        SetRendererColor(j, Color.Lerp(effect.EffectColor, originalColors[j], elapsed / effect.FadeDuration));
                    yield return null;
                }
                ResetAllRenderersToOriginalColors();

                // Wait between repetitions
                if (i < effect.Repetitions - 1)
                {
                    yield return new WaitForSeconds(0.05f);
                }
            }
            effectCoroutine = null;
        }

        // Executes and/or resets given Hold Effect
        private IEnumerator HoldEffectCoroutine(HoldEffect effect)
        {
            float elapsed = 0f;

            // CASE 1: Same effect, deactivate
            if (_currHoldEffect == effect)
            {
                elapsed = 0f;
                while (elapsed < effect.FadeDuration)
                {
                    elapsed += Time.deltaTime;
                    for (int j = 0; j < playerRenderers.Length; j++)
                        SetRendererColor(j,
                            Color.Lerp(effect.EffectColor, originalColors[j], elapsed / effect.FadeDuration));
                    yield return null;
                }

                ResetAllRenderersToOriginalColors();
                _currHoldEffect = null;
                yield break;
            }

            // CASE 2: Different effect active, clear it first
            if (_currHoldEffect != null)
            {
                ResetAllRenderersToOriginalColors();
                _currHoldEffect = null;
            }

            // CASE 3: Activate new effect
            _currHoldEffect = effect;

            elapsed = 0f;
            while (elapsed < effect.FadeDuration)
            {
                elapsed += Time.deltaTime;
                for (int j = 0; j < playerRenderers.Length; j++)
                    SetRendererColor(j,
                        Color.Lerp(originalColors[j], effect.EffectColor, elapsed / effect.FadeDuration));
                yield return null;
            }

            for (int j = 0; j < playerRenderers.Length; j++)
                SetRendererColor(j, effect.EffectColor);
        }

        // Helper method to reset all renderers to original colors
        private void ResetAllRenderersToOriginalColors()
        {
            for (int i = 0; i < playerRenderers.Length; i++)
            {
                if (playerRenderers[i] != null)
                {
                    playerRenderers[i].material.color = originalColors[i];

                    // Force update material
                    Material currentMat = playerRenderers[i].material;
                    playerRenderers[i].material = currentMat;
                }
            }
        }

        private void CacheRenderers()
        {
            playerRenderers = playerCapsuleRoot.Find("PlayerCapsule").GetComponentsInChildren<MeshRenderer>();
            originalColors = new Color[playerRenderers.Length];
            for (int i = 0; i < playerRenderers.Length; i++)
                originalColors[i] = playerRenderers[i].material.color;
        }

        private void SetRendererColor(int index, Color color)
        {
            var r = playerRenderers[index];
            if (!r) return;
            r.material.color = color;
        }

        private void Awake()
        {
            _pEffectsHub = GetComponent<PlayerEffectsHub>();
        }

        private void OnEnable()
        {
            // Add listeners for events
            _pEffectsHub.pData.healthEvents.OnDamage.AddListener((int amt) => TriggerEffect(damageEffect));
            _pEffectsHub.pSpecialMoveHandler.Events.OnDefenseActivated.AddListener(() => TriggerEffect(defenseEffect));
            _pEffectsHub.pSpecialMoveHandler.Events.OnDefenseDeactivated.AddListener(() => TriggerEffect(defenseEffect));
            //_pSpecialMoveHandler.Events.OnChargeRoll.AddListener((ChargeRoll_SpecialMove.ChargeRollState state) => TriggerEffect(chargeEffect));
        }

        private void Start()
        {
            CacheRenderers();
        }

        private void OnDisable()
        {
            // Remove listeners for events
            _pEffectsHub.pData.healthEvents.OnDamage.RemoveAllListeners();
            _pEffectsHub.pSpecialMoveHandler.Events.OnDefenseActivated.RemoveAllListeners();
            _pEffectsHub.pSpecialMoveHandler.Events.OnDefenseDeactivated.RemoveAllListeners();
            //_pSpecialMoveHandler.Events.OnChargeRoll.RemoveAllListeners();
            ResetAllRenderersToOriginalColors();
        }

        /// <summary>
        /// Abstract data class for Color Effect
        /// </summary>
        public abstract class ColorEffect
        {
            [Tooltip("Effect color")]
            public Color EffectColor = Color.white;

            [Tooltip("Seconds to fade in/out")]
            public float FadeDuration = 0.5f; // time to fade in/out
        }

        /// <summary>
        /// Concrete Color Effect: Data for color flashing
        /// </summary>
        [System.Serializable]
        public class FlashEffect : ColorEffect
        {
            [Tooltip("Seconds the color is held")]
            public float HoldDuration = 0.1f; // time to hold the color

            [Tooltip("How many times to repeat the effect")]
            public int Repetitions = 1; // how many times to repeat the flash
        }

        /// <summary>
        /// Concrete Color Effect: Empty class for color holding
        /// </summary>
        [System.Serializable]
        public class HoldEffect : ColorEffect { }
    }
}