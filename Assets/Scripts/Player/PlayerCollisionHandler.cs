using GASHAPWN.Audio;
using MyBox;
using Unity.AppUI.UI;
using UnityEngine;

namespace GASHAPWN
{
    /// <summary>
    /// Handles player collision and hit detection; forwards to PlayerData
    /// </summary>
    public class PlayerCollisionHandler : MonoBehaviour
    {
        private Rigidbody _rb;
        private PlayerData _pData;
        private SpecialMoveHandler _pSpecialMoveHandler;


        [Tooltip("Speed threshold for hit to register (given appropriate conditions)")]
        [Min(0)] public float hitSpeedThreshold = 1f;

        [Tooltip("Speed threshold for general non-damage impact (used for audio / vfx)")]
        [SerializeField] private float generalImpactSpeedThreshold = 2f;

        [Tooltip("In the case of a \"Bounce\" interaction (no hit, no deflect), the ratio of knockback applied equally to each player.")]
        [Range(0.2f, 1f), SerializeField] private float bounceRatio = 0.4f;

        [Tooltip("In the case the player is defending, this is the ratio of knockback they receive.")]
        [Range(0.2f, 1f), SerializeField] private float knockbackReductionRatio = 0.3f;

        [Tooltip("Multiplier applied to knockback given a \"Deflect\".")]
        [Range(0.2f, 1f), SerializeField] private float deflectKnockbackMultiplier = 1f;

        [Tooltip("Multiplier applied to knockback given a \"Hit\".")]
        [Range(0.2f, 1f), SerializeField] private float hitKnockbackMultiplier = 1f;

        private void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.CompareTag("Ground") || collision.gameObject.CompareTag("Wall"))
            {
                if (_rb.linearVelocity.magnitude > generalImpactSpeedThreshold) GAME_SFXManager.Instance.Play_ImpactGeneral(transform);
            }

            if (!collision.gameObject.CompareTag("Player1") &&
                !collision.gameObject.CompareTag("Player2"))
                return;

            // Only one side processes the collision to eliminate conflicting or race conditions
            // In future, if > 2 players, need to make a centralized collision resolver like "CombatResolver.Resolve(PlayerA, PlayerB, collision)"
            if (GetInstanceID() > collision.gameObject.GetInstanceID())
                return;

            ResolvePlayerCollision(collision);
        }

        // ISSUE: Still some instances where hits just don't register when it feels like they should
        private void ResolvePlayerCollision(Collision collision)
        {
            // Get collision partner's components
            Rigidbody otherRb = collision.rigidbody;
            PlayerData otherPlayerData = collision.gameObject.GetComponent<PlayerData>();
            SpecialMoveHandler otherSpMoveHandler = collision.gameObject.GetComponent<SpecialMoveHandler>();

            if (otherRb == null) return;

            // Setup temporary variables
            float mySpeed = _rb.linearVelocity.magnitude;
            float otherSpeed = otherRb.linearVelocity.magnitude;
            float relativeSpeed = (_rb.linearVelocity - otherRb.linearVelocity).magnitude;

            bool myOffensive = _pSpecialMoveHandler.HasSlammed || _pSpecialMoveHandler.IsBursting;
            bool otherOffensive = otherSpMoveHandler.HasSlammed || otherSpMoveHandler.IsBursting;

            bool myDefending = _pSpecialMoveHandler.IsDefending;
            bool otherDefending = otherSpMoveHandler.IsDefending;

            //Vector3 contactPoint = collision.GetContact(0).point;
            ContactPoint contactPoint = collision.GetContact(0);
            Vector3 dirToOther = (collision.transform.position - transform.position).normalized;
            Vector3 dirFromOther = -dirToOther;

            // ADD ANOTHER CONDITION HERE: If in air and no special moves, maybe make it bounce. Otherwise jumping always guarantees a hit on the way down

            #region CASE 1: Bounce
            // Neither player has overcome hitSpeedThreshold, so apply equal knockback.
            if (mySpeed < hitSpeedThreshold && otherSpeed < hitSpeedThreshold)
            {
                float bounceForce = relativeSpeed * bounceRatio;

                _rb.AddForce(dirFromOther * bounceForce, ForceMode.Impulse);
                otherRb.AddForce(dirToOther * bounceForce, ForceMode.Impulse);

                //particleEffects?.PlayBounceEffect(contactPoint);
                return;
            }
            #endregion

            #region CASE 2: Deflect
            // A deflect occurs when the opponent is performing a special move that guarantees a deflect.
            if (otherDefending)
            {
                float force = deflectKnockbackMultiplier * relativeSpeed;

                // Attacker gets more knockback
                _rb.AddForce(dirFromOther * force * deflectKnockbackMultiplier, ForceMode.Impulse);
                // Defender gets reduced knockback
                otherRb.AddForce(dirToOther * force * 0.3f, ForceMode.Impulse);

                otherPlayerData.healthEvents.OnDeflect.Invoke(contactPoint, otherRb.transform);
                GAME_SFXManager.Instance.Play_ImpactDeflect(transform);
                return;
            }

            if (myDefending)
            {
                float force = deflectKnockbackMultiplier * relativeSpeed;
                // Attacker gets more knockback
                otherRb.AddForce(dirToOther * force, ForceMode.Impulse);
                // Defender gets reduced knockback
                _rb.AddForce(dirFromOther * force * knockbackReductionRatio, ForceMode.Impulse);

                _pData.healthEvents.OnDeflect.Invoke(contactPoint, _rb.transform);
                GAME_SFXManager.Instance.Play_ImpactDeflect(transform);
                return;
            }
            #endregion

            #region CASE 3: Hit
            // To hit and damage their opponent, the player must overcome a certain speed threshold and be moving
            // at a greater speed than the opponent when striking them, or be performing a special move that guarantees a hit. 
            bool myWin =
                (myOffensive && !otherOffensive) || // guaranteed move
                (mySpeed >= hitSpeedThreshold && mySpeed > otherSpeed);

            bool otherWin =
                (otherOffensive && !myOffensive) ||
                (otherSpeed >= hitSpeedThreshold && otherSpeed > mySpeed);

            if (myWin)
            {

                int damageAmount = otherPlayerData.CalculateDamageAmount(relativeSpeed, myOffensive, this);
                otherPlayerData.TakeDamage(damageAmount);
                ApplyHitKnockback(otherRb, _rb, relativeSpeed);
                otherPlayerData.healthEvents.OnHit.Invoke(contactPoint, otherRb.transform);
                return;
            }

            if (otherWin)
            {
                int damageAmount = _pData.CalculateDamageAmount(relativeSpeed, otherOffensive, otherPlayerData.GetComponent<PlayerCollisionHandler>());
                _pData.TakeDamage(damageAmount);
                ApplyHitKnockback(_rb, otherRb, relativeSpeed);
                _pData.healthEvents.OnHit.Invoke(contactPoint, _rb.transform);
                return;
            }
            #endregion
        }

        public void Initialize(PlayerController playerController)
        {
            _rb = playerController.rb;
            _pData = GetComponent<PlayerData>();
            _pSpecialMoveHandler = GetComponent<SpecialMoveHandler>();
        }

        void ApplyHitKnockback(Rigidbody loser, Rigidbody winner, float relativeSpeed)
        {
            float force = hitKnockbackMultiplier * Mathf.Sqrt(relativeSpeed);

            loser.AddForce((loser.position - winner.position).normalized * force, ForceMode.Impulse);
            winner.AddForce((winner.position - loser.position).normalized * force * 0.5f, ForceMode.Impulse);
        }

        // Some particle effects that require collision data can be handled in here
    }
}