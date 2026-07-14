using DG.Tweening;
using GASHAPWN.Audio;
using System.Collections;
using UnityEngine;

namespace GASHAPWN
{
    [RequireComponent(typeof(PlayerEffectsHub))]
    public class Effect_Stunned : MonoBehaviour
    {
        [Tooltip("Dizzy ring child object")]
        [SerializeField] private GameObject dizzyRing;

        // Get reference to higher-level player components through Player Effects Hub
        private PlayerEffectsHub _pEffectsHub;

        #region DIZZY RING MOTION
        float rotationSpeed = 150f;
        float height = 0.025f;
        float bobAmount = 0.005f;
        float bobSpeed = 4f;
        #endregion

        private bool _isStunned = false;
        private float _spinAngle;

        private void Awake()
        {
            _pEffectsHub = GetComponent<PlayerEffectsHub>();
            dizzyRing.SetActive(false);
        }

        private void OnEnable()
        {
            _pEffectsHub.pSpecialMoveHandler.Events.OnStunned.AddListener(StunEffect);
        }

        private void Update()
        {
            if (!_isStunned) return;
            float bob = Mathf.Sin(Time.time * bobSpeed) * bobAmount;
            dizzyRing.transform.position = _pEffectsHub.rb.position + Vector3.up * (height + bob);
            _spinAngle += rotationSpeed * Time.deltaTime;

            float tiltX = Mathf.Sin(Time.time * 2f) * 8f;
            float tiltZ = Mathf.Cos(Time.time * 2f) * 8f;

            transform.rotation = Quaternion.Euler(tiltX, _spinAngle, tiltZ);
        }

        private void OnDisable()
        {
            _pEffectsHub.pSpecialMoveHandler.Events.OnStunned.RemoveListener(StunEffect);
        }

        private void StunEffect(float stunDuration)
        {
            if (stunDuration < 0.5f)
            {
                Debug.LogWarning($"{nameof(Effect_Stunned)}: Stunned durtion was less than 0.5 seconds. Did not play stunned effect.");
                return;
            }
            StartCoroutine(Stun(stunDuration));
        }

        private IEnumerator Stun(float stunDuration)
        {
            _isStunned = true;
            dizzyRing.SetActive(true);
            _pEffectsHub.CapsuleAnimator.SetBool(AnimationStrings.isStunned, true);
            GAME_SFXManager.Instance.Play_Stunned(this.transform);
            dizzyRing.transform.localScale = Vector3.zero;
            dizzyRing.transform.DOScale(1f, 0.15f);
            yield return new WaitForSeconds(stunDuration - 0.3f);

            dizzyRing.transform.DOScale(0f, 0.15f);
            yield return new WaitForSeconds(0.2f);

            _isStunned = false;
            dizzyRing.SetActive(false);
            _pEffectsHub.CapsuleAnimator.SetBool(AnimationStrings.isStunned, false);
        }
    }
}