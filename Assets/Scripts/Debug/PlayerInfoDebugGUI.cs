using TMPro;
using UnityEngine;

/// <summary>
/// GUI Controller to displau debug info for player
/// </summary>

namespace GASHAPWN.UI
{
    public class PlayerInfoDebugGUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI velocityText;
        [SerializeField] private TextMeshProUGUI specialMoveStateText;
        [SerializeField] private TextMeshProUGUI healthText;
        [SerializeField] private TextMeshProUGUI staminaText;
        [SerializeField] private TextMeshProUGUI iFramesText;

        private PlayerController _pController;
        private PlayerData _pData;
        private SpecialMoveHandler _pSpecialMoveHandler;
        private PlayerCollisionHandler _pCollisionHandler;

        public void Initialize(PlayerController playerController_, PlayerData playerData_,
            SpecialMoveHandler _specialMoveHandler, PlayerCollisionHandler playerCollisionHandler_)
        {
            _pController = playerController_;
            _pData = playerData_;
            _pSpecialMoveHandler = _specialMoveHandler;
            _pCollisionHandler = playerCollisionHandler_;
        }

        private void FixedUpdate()
        {
            velocityText.SetText(_pController.rb.linearVelocity.ToString());

            var sp = _pSpecialMoveHandler.activeSpecialMove;
            specialMoveStateText.SetText((sp == null) ? "None" : sp.Name
                + (sp.GetSubState() == null ? "" : " | " + sp.GetSubState().ToString()));

            healthText.SetText(_pData.currentHealth.ToString());
            staminaText.SetText(_pData.currentStamina.ToString());
            iFramesText.SetText(_pData.IsInvincible ? "Active" : "Inactive");
        }
    }
}