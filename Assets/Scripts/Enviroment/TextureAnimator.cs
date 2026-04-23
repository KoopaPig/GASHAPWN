using System.Collections.Generic;
using UnityEngine;

namespace GASHAPWN.Environment
{
    // Adapted from icauroboros on Unity Forums
    public class TextureAnimator : MonoBehaviour
    {
        [SerializeField] private List<Texture2D> textures;
        [SerializeField] private Renderer targetRenderer;

        [Tooltip("Seconds per frame")]
        [Range(0.5f, 5f)] public float frameDuration = 1f; // seconds per frame

        [Tooltip("Index of material in target renderer to apply the texture to.")]
        [SerializeField] private int materialIndex = 1;

        private int _currFrame;
        private float _timer;

        private void Update()
        {
            _timer += Time.deltaTime;

            if (_timer >= frameDuration)
            {
                _timer -= frameDuration;

                _currFrame++;
                if (_currFrame >= textures.Count)
                    _currFrame = 0;

                targetRenderer.materials[materialIndex]
                    .SetTexture("_BaseMap", textures[_currFrame]);
            }
        }

        private void OnEnable()
        {
            _timer = frameDuration;
        }
    }
}