using System.Collections;
using UnityEngine;

namespace GASHAPWN {
    public class ProgressiveCracking : MonoBehaviour
    {
        [SerializeField] private Renderer targetRenderer;
        [SerializeField] private Texture2D[] detailMasks;
        private int maxHealth;
        private int currHealth;
        private PlayerData playerData;

        private void Awake()
        {
            playerData = GetComponent<PlayerData>();
        }

        private void OnEnable()
        {
            playerData.SetMaxHealth.AddListener(SetMaxHealthCrack);
            playerData.OnDamage.AddListener(UpdateDetailMask_Dmg);
            playerData.SetHealth.AddListener(UpdateDetailMask_HP);
        }

        // Update detail mask based on damage
        private void UpdateDetailMask_Dmg(int damageAmt)
        {
            UpdateDetailMask_HP(currHealth - damageAmt);
        }

        // Update detail mask based on currentHP
        private void UpdateDetailMask_HP(int hp)
        {
            currHealth = hp;
            // Select new mask index based on currHealth
            int maskIndex = Mathf.Clamp(maxHealth - currHealth, 0, detailMasks.Length - 1);
            // Apply the selected detail mask to the material
            targetRenderer.material.SetTexture("_DetailMask", detailMasks[maskIndex]);
        }

        private void SetMaxHealthCrack(int health)
        {
            maxHealth = health;
            // Set the initial detail mask (no damage)
            UpdateDetailMask_HP(health);
        }

        private void OnDisable()
        {
            playerData.SetMaxHealth.RemoveListener(SetMaxHealthCrack);
            playerData.OnDamage.RemoveListener(UpdateDetailMask_Dmg);
            playerData.SetHealth.RemoveListener(UpdateDetailMask_HP);
        }

        public void SetRenderer(Renderer renderer) { targetRenderer = renderer; }
    }
}