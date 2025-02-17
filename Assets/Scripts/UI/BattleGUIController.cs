using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Collections;
using static UnityEngine.Rendering.DebugUI;

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

        [Header("Other GUI Elements")]
        // Icon for Figure
        [SerializeField] private Image charIcon;
        // Name of Figure
        [SerializeField] private TextMeshProUGUI charName;
        // Healthbar Border
        [SerializeField] private Image healthBarBorder;
        // Reference to capsule group GUI
        [SerializeField] private GameObject capsuleGroup;

        //public enum PlayerID { P1, P2 };

        [Header("Animator for Capsule Group")]
        [SerializeField] private Animator animator;

        [SerializeField] private Color grayColor = Color.gray;
        [SerializeField] private Color damageColor = Color.red;
        [SerializeField] private Color healColor = Color.green;

        private Coroutine healthChangeCoroutine;
        private float currHealth;
        private float maxHealth;

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
                animator.SetBool("isDamageShake", true);
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

        // Initialize event listeners for PlayerData
        private void Initialize(PlayerData player)
        {
            if (playerData != null)
            {
                // Unsubscribe from previous player events
                playerData.OnDamage.RemoveListener(TakeDamageGUI);
                playerData.SetMaxHealth.RemoveListener(SetMaxHealthGUI);
                playerData.SetHealth.RemoveListener(SetHealthSuddenDeathGUI);
            }

            playerData = player;

            if (playerData != null)
            {
                // Subscribe to the new player's events
                playerData.OnDamage.AddListener(TakeDamageGUI);
                playerData.SetMaxHealth.AddListener(SetMaxHealthGUI);
                playerData.SetHealth.AddListener(SetHealthSuddenDeathGUI);
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

            animator.SetBool("isDamageShake", false);
            healthChangeCoroutine = null;
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

        private void OnDisable()
        {
            // unsub from damage and health events here
            if (playerData != null)
            {
                playerData.OnDamage.RemoveListener(TakeDamageGUI);
                playerData.SetMaxHealth.RemoveListener(SetMaxHealthGUI);
                playerData.SetHealth.RemoveListener(SetHealthSuddenDeathGUI);
            }
        }

    }
}