using UnityEngine;

namespace GASHAPWN
{
    /// <summary>
    /// Acts as a central hub of references to higher-order components for Player Effects
    /// </summary>
    public class PlayerEffectsHub : MonoBehaviour
    {
        [Tooltip("Reference to Player Capsule Root")]
        public Transform PlayerCapsuleRoot;

        // Core player components
        public PlayerData pData { get; private set; }
        public SpecialMoveHandler pSpecialMoveHandler { get; private set; }
        public PlayerController pController { get; private set; }

        // Forwards PlayerController.rb to this script
        public Rigidbody rb { get; private set; }
        public Animator CapsuleAnimator { get; private set; }

        private void Awake()
        {
            pData = GetComponentInParent<PlayerData>();
            pSpecialMoveHandler = GetComponentInParent<SpecialMoveHandler>();
            pController = GetComponentInParent<PlayerController>();
            rb = pController.rb;
            CapsuleAnimator = PlayerCapsuleRoot.GetComponent<Animator>();
        }
    }
}