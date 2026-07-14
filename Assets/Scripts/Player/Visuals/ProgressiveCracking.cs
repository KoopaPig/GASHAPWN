using UnityEngine;

namespace GASHAPWN {
    public class ProgressiveCracking : MonoBehaviour
    {
        [SerializeField] private Texture2D[] detailMasks;

        private int maxHealth;
        private int currHealth;
        private PlayerData _pData;
        private Renderer _targetRenderer;

        private void Awake() => _pData = GetComponentInParent<PlayerData>();

        private void OnEnable()
        {
            _pData.healthEvents.SetMaxHealth.AddListener(SetMaxHealthCrack);
            _pData.healthEvents.OnDamage.AddListener(UpdateDetailMask_Dmg);
            _pData.healthEvents.SetHealth.AddListener(UpdateDetailMask_HP);
        }

        // Update detail mask based on damage
        private void UpdateDetailMask_Dmg(int damageAmt) => UpdateDetailMask_HP(currHealth - damageAmt);

        // Update detail mask based on currentHP
        private void UpdateDetailMask_HP(int hp)
        {
            if (_targetRenderer == null) return;
            currHealth = hp;
            // Select new mask index based on currHealth
            int maskIndex = Mathf.Clamp(maxHealth - currHealth, 0, detailMasks.Length - 1);
            // Apply the selected detail mask to the material
            _targetRenderer.material.SetTexture("_DetailMask", detailMasks[maskIndex]);
        }

        private void SetMaxHealthCrack(int health)
        {
            maxHealth = health;
            // Set the initial detail mask (no damage)
            UpdateDetailMask_HP(health);
        }

        private void OnDisable()
        {
            _pData.healthEvents.SetMaxHealth.RemoveListener(SetMaxHealthCrack);
            _pData.healthEvents.OnDamage.RemoveListener(UpdateDetailMask_Dmg);
            _pData.healthEvents.SetHealth.RemoveListener(UpdateDetailMask_HP);
        }

        public void SetRenderer(Renderer renderer) { _targetRenderer = renderer; }
    }
}