using DG.Tweening;
using GASHAPWN.Audio;
using UnityEngine;

namespace GASHAPWN.Environment
{
    /// <summary>
    /// Launches colliding player object given launch vector
    /// </summary>
    public class LaunchPad : MonoBehaviour
    {
        [Tooltip("Force with which to launch the player")]
        public float launchForce = 1f;

        [Tooltip("Launch angle is calculated from the transform of this point.")]
        [SerializeField] private Transform _launchPoint;

        [Tooltip("Reference to launch pad mesh renderer")]
        [SerializeField] private Renderer launchPadRenderer;

        private Material _launchPadMaterial;
        private Tween _emissionTween;
        private static readonly int EmissionStrengthID =
            Shader.PropertyToID("_EmissionStrength");

        private void Awake()
        {
            _launchPadMaterial = launchPadRenderer.material;
            _launchPadMaterial.SetFloat(EmissionStrengthID, 0f);
        }

        private void OnCollisionEnter(Collision collision)
        {
            string tag = collision.gameObject.tag;
            if (tag.Contains("Player"))
            {
                Rigidbody playerRb = collision.gameObject.GetComponent<PlayerController>().rb;
                if (playerRb != null)
                {

                    playerRb.linearVelocity = Vector3.zero;
                    playerRb.AddForce(
                    _launchPoint.forward * launchForce,
                    ForceMode.VelocityChange);
                }
                GAME_SFXManager.Instance.Play_LaunchPad(collision.transform);
                FlashEmission();
            }
        }

        private void FlashEmission()
        {
            _emissionTween?.Kill();

            _launchPadMaterial.SetFloat(EmissionStrengthID, 0f);

            _emissionTween = DOTween.Sequence()
                .Append(
                    _launchPadMaterial
                        .DOFloat(1.2f, EmissionStrengthID, 0.05f)
                        .SetEase(Ease.OutQuad)
                )
                .Append(
                    _launchPadMaterial
                        .DOFloat(0f, EmissionStrengthID, 0.08f)
                        .SetEase(Ease.InQuad)
                );
        }
    }
}