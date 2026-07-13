using DG.Tweening;
using UnityEngine;

namespace GASHAPWN.Environment
{
    /// <summary>
    /// Main class for Roomba level object
    /// </summary>
    public class Roomba : RotateAround
    {

        [Tooltip("Reference to transform of visual model")]
        [SerializeField] private Transform meshTransform;

        private KnockbackOnTouch _knockbackOnTouch;
        private Tween _bounceTween;
        private Vector3 _originalScale;

        private void Awake()
        {
            _knockbackOnTouch = GetComponentInChildren<KnockbackOnTouch>();

            if (meshTransform == null)
                meshTransform = GetComponentInChildren<MeshRenderer>().transform;

            _originalScale = meshTransform.localScale;
        }

        private void OnEnable() =>
            _knockbackOnTouch.OnKnockback += KnockbackAnimation;

        private void OnDisable() =>
            _knockbackOnTouch.OnKnockback -= KnockbackAnimation;

        private void KnockbackAnimation(float force)
        {
            _bounceTween?.Kill();

            meshTransform.localScale = _originalScale;

            _bounceTween = meshTransform
                .DOScale(_originalScale * 1.07f, 0.08f)
                .SetEase(Ease.OutQuad)
                .OnComplete(() =>
                {
                    meshTransform
                        .DOScale(_originalScale, 0.12f)
                        .SetEase(Ease.OutBack);
                });
        }
    }
}