using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Collections;
using static UnityEngine.Rendering.DebugUI;
using Unity.Collections.LowLevel.Unsafe;
using Unity.VisualScripting;
using GASHAPWN.Audio;

namespace GASHAPWN.UI {
    public class BattleGUIController : MonoBehaviour
    {
        [Header("Player Reference")]
        // Set corresponding tag here: "Player1" or "Player2"
        [SerializeField] private string playerTag; 
        // Reference to the specific player's data
        private PlayerData playerData;

        [Header("Healthbar GUI Elements")]
        // Healthbar
        [SerializeField] private Slider healthSlider;
        // Healthbar Secondary BG
        [SerializeField] private Slider healthSliderBG;
        // Healthbar Gradient
        [SerializeField] private Gradient healthGradient;
        // Healthbar Fill
        [SerializeField] private Image healthFill;

        [Header("Staminabar GUI Elements")]
        // Staminabar
        [SerializeField] private Slider staminaSlider;
        // Staminabar Gradient
        [SerializeField] private Gradient staminaGradient;
        // Staminabar Fill
        [SerializeField] private Image staminaFill;
        // Staminabar Animator
        [SerializeField] private Animator staminaAnimator;
        // Staminabar sections
        [SerializeField] private int staminaBarSections = 6;


        [Header("Other GUI Elements")]
        // Icon for Figure
        [SerializeField] private Image charIcon;
        // Name of Figure
        [SerializeField] private TextMeshProUGUI charName;
        // Healthbar Border
        [SerializeField] private Image healthBarBorder;
        // Reference to capsule group GUI
        [SerializeField] private GameObject capsuleGroup;

        [Header("Animator for Capsule Group")]
        [SerializeField] private Animator capsuleAnimator;

        [SerializeField] private Color grayColor = Color.gray;
        [SerializeField] private Color damageColor = Color.red;
        [SerializeField] private Color healColor = Color.green;

        private Coroutine healthChangeCoroutine;
        private float currHealth;
        private float maxHealth;

        private Coroutine staminaChangeCoroutine;
        private float currStamina;
        private float maxStamina;
        private float staminaInterval;
        private int lastIntervalIndex = -1;

        ////// PUBLIC METHODS /////

        // Set Figure Icon
        public void SetFigureIcon(Sprite sprite)
        {
            charIcon.sprite = sprite;
        }

        // Set Figure Name
        public void SetFigureName(string name)
        {
            charName.text = name;
        }

        // Set MaxHealth (GUI)
        public void SetMaxHealthGUI(int health)
        {
            healthSlider.maxValue = health;
            healthSliderBG.maxValue = health;
            maxHealth = health;
            currHealth = maxHealth;

            healthSlider.value = health;
            healthSliderBG.value = health;
            healthFill.color = healthGradient.Evaluate(1f);
        }

        // Take Damage (GUI)
        public void TakeDamageGUI(int damage)
        {
            float targetHealth = currHealth - damage;
            if (targetHealth < currHealth)
            {
                capsuleAnimator.SetBool("isDamageShake", true);
                StartCoroutine(FlashGUI(damageColor, 0.1f));
            }

            SetHealthGUI(targetHealth);
            if (healthChangeCoroutine != null)
            {
                StopCoroutine(healthChangeCoroutine);
            }
            healthChangeCoroutine = StartCoroutine(AnimateHealthChange(currHealth, targetHealth, 0.2f));
        }

        // Heal (GUI)
        public void HealGUI(int value)
        {
            float targetHealth = currHealth + value;
            if (targetHealth > currHealth)
            {
                StartCoroutine(FlashGUI(healColor, 0.1f));
            }
            SetHealthGUI(targetHealth);
            if (healthChangeCoroutine != null)
            {
                StopCoroutine(healthChangeCoroutine);
            }
            healthChangeCoroutine = StartCoroutine(AnimateHealthChange(currHealth, targetHealth, 0.2f));
        }

        // SetHealthSuddenDeath (GUI)
        public void SetHealthSuddenDeathGUI(int value)
        {
            SetHealthGUI(value);
            if (healthChangeCoroutine != null)
            {
                StopCoroutine(healthChangeCoroutine);
            }
            if (currHealth != value)
            {
                healthChangeCoroutine = StartCoroutine(AnimateHealthChange(currHealth, value, 0.2f));
            }
        }

        // SetMaxStamina (GUI)
        public void SetMaxStaminaGUI(float value)
        {
            staminaSlider.maxValue = value;
            maxStamina = value;
            currStamina = maxStamina;

            staminaSlider.value = value;
            staminaFill.color = staminaGradient.Evaluate(1f);

            staminaInterval = maxStamina / staminaBarSections;
        }

        // LoseStamina (GUI)
        public void LoseStaminaGUI(float newValue)
        {
            if (newValue < currStamina)
            {
                staminaAnimator.SetBool("isIntervalEffect", true);
            }

            SetStaminaGUI(newValue);

            if (staminaChangeCoroutine != null)
            {
                StopCoroutine(staminaChangeCoroutine);
            }
            staminaChangeCoroutine = StartCoroutine(AnimateStaminaChange(currStamina, newValue, 0.15f));
        }

        // RecoverStamina (GUI)
        public void RecoverStaminaGUI(float newValue)
        {
            if (newValue > currStamina)
            {
                staminaAnimator.SetBool("isIntervalEffect", true);
            }
            SetStaminaGUI(newValue);

            if (staminaChangeCoroutine != null)
            {
                StopCoroutine(staminaChangeCoroutine);
            }
            staminaChangeCoroutine = StartCoroutine(AnimateStaminaChange(currStamina, newValue, 0.15f));
        }

        // LowStamina (GUI)

        public void LowStaminaGUI(float value)
        {
            staminaAnimator.SetTrigger("isStaminaLowEffect");
            UI_SFXManager.Instance.Play_StaminaLow();
            StartCoroutine(ResetStaminaLowEffect());
        }


        ////// PRIVATE METHODS /////

        private void Awake()
        {
            // Find the player object with the given tag
            GameObject playerObj = GameObject.FindGameObjectWithTag(playerTag);

            if (playerObj != null)
            {
                PlayerData player = playerObj.GetComponent<PlayerData>();
                if (player != null)
                {
                    Initialize(player);
                }
                else
                {
                    Debug.LogError($"BattleGUIController: No PlayerData found on object with tag {playerTag}");
                }
            }
            else
            {
                Debug.LogError($"BattleGUIController: No GameObject found with tag {playerTag}");
            }
        }

        private void Start()
        {
            staminaInterval = maxStamina / staminaBarSections;
        }

        // Initialize event listeners for PlayerData
        private void Initialize(PlayerData player)
        {
            if (playerData != null)
            {
                // Unsubscribe from previous player's health-based events
                playerData.OnDamage.RemoveListener(TakeDamageGUI);
                playerData.SetMaxHealth.RemoveListener(SetMaxHealthGUI);
                playerData.SetHealth.RemoveListener(SetHealthSuddenDeathGUI);

                // Stamina-based Listeners
                playerData.OnStaminaChanged.RemoveListener(SetStaminaGUI_Wrapper);
                playerData.SetMaxStamina.RemoveListener(SetMaxStaminaGUI);
                playerData.OnStaminaHardDecrease.RemoveListener(LoseStaminaGUI);
                playerData.OnStaminaHardIncrease.RemoveListener(RecoverStaminaGUI);
                playerData.OnLowStamina.RemoveListener(LowStaminaGUI);
            }

            playerData = player;

            if (playerData != null)
            {
                // Subscribe to the new player's health-based events
                playerData.OnDamage.AddListener(TakeDamageGUI);
                playerData.SetMaxHealth.AddListener(SetMaxHealthGUI);
                playerData.SetHealth.AddListener(SetHealthSuddenDeathGUI);

                // Stamina-based Listeners
                playerData.OnStaminaChanged.AddListener(SetStaminaGUI_Wrapper);
                playerData.SetMaxStamina.AddListener(SetMaxStaminaGUI);
                playerData.OnStaminaHardDecrease.AddListener(LoseStaminaGUI);
                playerData.OnStaminaHardIncrease.AddListener(RecoverStaminaGUI);
                playerData.OnLowStamina.AddListener(LowStaminaGUI);
            }
        }

        // Set Current Health (GUI)
        private void SetHealthGUI(float health)
        {
            // maybe need player ID as parameter
            healthSlider.value = health;
            healthFill.color = healthGradient.Evaluate(healthSlider.normalizedValue);
        }

        private void SetHealthBG_GUI(float value)
        {
            healthSliderBG.value = value;
        }

        // IEnumerator for controlling animation during health change
        private IEnumerator AnimateHealthChange(float startHealth, float targetHealth, float duration)
        {
            float elapsedTime = 0f;
            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;

                // Linearly interpolate between start and target health values
                currHealth = Mathf.Lerp(startHealth, targetHealth, elapsedTime / duration);

                // Update the GUI
                SetHealthBG_GUI(currHealth);

                yield return null;
            }

            currHealth = targetHealth;
            SetHealthBG_GUI(currHealth);
            UpdateGUIColor(currHealth);

            capsuleAnimator.SetBool("isDamageShake", false);
            healthChangeCoroutine = null;
        }

        // IEnumerator for controlling animation during stamina change
        private IEnumerator AnimateStaminaChange(float startValue, float targetValue, float duration)
        {
            float elapsedTime = 0f;
            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;

                // Linearly interpolate between start and target stamina values
                currStamina = Mathf.Lerp(startValue, targetValue, elapsedTime / duration);

                // Update the GUI
                SetStaminaGUI(currStamina);

                yield return null;
            }

            currStamina = targetValue;
            SetStaminaGUI(currStamina);

            staminaAnimator.SetBool("isIntervalEffect", false);
            staminaChangeCoroutine = null;
        }

        // IEnumerator for controlling color flash during health change
        private IEnumerator FlashGUI(Color flashColor, float flashDuration)
        {
            SetGUIColor(flashColor);

            yield return new WaitForSeconds(flashDuration);

            UpdateGUIColor(currHealth);
        }

        // Updates GUI color to/from gray (used in HealthChageGUI)
        private void UpdateGUIColor(float value)
        {
            Color targetColor = value > 0 ? Color.white : grayColor;
            healthBarBorder.color = targetColor;

            foreach (Transform child in capsuleGroup.transform)
            {
                Image image = child.GetComponent<Image>();
                if (image != null)
                {
                    image.color = targetColor;
                }
            }
        }

        // sets GUI color (used in FlashGUI)
        private void SetGUIColor(Color color)
        {
            healthBarBorder.color = color;
            foreach (Transform child in capsuleGroup.transform)
            {
                Image image = child.GetComponent<Image>();
                if (image != null)
                {
                    image.color = color;
                }
            }
        }

        private void SetStaminaGUI(float value)
        {
            staminaSlider.value = value;
            staminaFill.color = staminaGradient.Evaluate(staminaSlider.normalizedValue);
        }

        private void SetStaminaGUI_Wrapper(float value)
        {
            SetStaminaGUI(value);
            int currentIntervalIndex = Mathf.FloorToInt(value / staminaInterval);
            if (currentIntervalIndex != lastIntervalIndex)
            {
                staminaAnimator.SetBool("isIntervalEffect", true);
                lastIntervalIndex = currentIntervalIndex;

                StartCoroutine(ResetIntervalEffect());
            }
        }

        private IEnumerator ResetIntervalEffect()
        {
            yield return new WaitForSeconds(0.3f);
            staminaAnimator.SetBool("isIntervalEffect", false);
        }

        private IEnumerator ResetStaminaLowEffect()
        {
            yield return new WaitForNextFrameUnit();
            staminaAnimator.ResetTrigger("isStaminaLowEffect");
        }

        private void OnDisable()
        {
            // unsub from health and stamina events here
            if (playerData != null)
            {
                // Health-based listeners
                playerData.OnDamage.RemoveListener(TakeDamageGUI);
                playerData.SetMaxHealth.RemoveListener(SetMaxHealthGUI);
                playerData.SetHealth.RemoveListener(SetHealthSuddenDeathGUI);

                // Stamina-based Listeners
                playerData.OnStaminaChanged.RemoveListener(SetStaminaGUI_Wrapper);
                playerData.SetMaxStamina.RemoveListener(SetMaxStaminaGUI);
                playerData.OnStaminaHardDecrease.RemoveListener(LoseStaminaGUI);
                playerData.OnStaminaHardIncrease.RemoveListener(RecoverStaminaGUI);
                playerData.OnLowStamina.RemoveListener(LowStaminaGUI);
            }
        }

    }
}