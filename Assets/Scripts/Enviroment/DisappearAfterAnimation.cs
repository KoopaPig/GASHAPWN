using System.Collections;
using UnityEngine;

namespace GASHAPWN.Environment {

    /// <summary>
    /// Call HideObject via an Animation Event to fade out an object with this script
    /// </summary>
    [RequireComponent(typeof(Animator))]
    public class DisappearAfterAnimation : MonoBehaviour
    {
        private Renderer objectRenderer;


        /// PRIVATE METHODS ///
        
        private void Start()
        {
            objectRenderer = GetComponent<Renderer>();
        }

        private IEnumerator FadeOut(float fadeDuration)
        {
            float startAlpha = objectRenderer.material.color.a;
            float targetAlpha = 0.0f;
            float time = 0f;

            while (time < fadeDuration)
            {
                float alpha = Mathf.Lerp(startAlpha, targetAlpha, time / fadeDuration);
                foreach (var mat in objectRenderer.materials)
                {
                    Color c = mat.color;
                    c.a = alpha;
                    mat.color = c;
                }
                time += Time.deltaTime;
                yield return null;
            }

            foreach (var mat in objectRenderer.materials)
            {
                Color c = mat.color;
                c.a = targetAlpha;
                mat.color = c;
            }

            if (targetAlpha <= 0.01f)
            {
                foreach (var mat in objectRenderer.materials)
                    MaterialObjectFade.MakeFade(mat);
            }
        }


        /// PUBLIC METHODS ///

        // Can be called via Animation Event to fade out object
        public void HideObject()
        {
            StartCoroutine(FadeOut(0.2f));
        }

        // Can be called via Animation Event to disable object
        public void DisableObject()
        {
            this.gameObject.SetActive(false);
        }
    }
}