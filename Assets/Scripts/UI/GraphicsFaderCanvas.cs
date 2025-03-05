using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

namespace GASHAPWN.UI
{
    [RequireComponent(typeof(CanvasGroup))]
    public class GraphicsFaderCanvas : MonoBehaviour
    {
        private Coroutine fadeCoroutine = null;
        private CanvasGroup canvasGroup = null;

        [SerializeField] private float fadeInDuration = 0.5f;
        public float fadeInWaitDuration = 0f;
        [SerializeField] private float fadeOutDuration = 0.5f;
        public float fadeOutWaitDuration = 0f;

        private void Awake()
        {
            canvasGroup = GetComponent<CanvasGroup>();
        }

        public void FadeTurnOff()
        {
            if (fadeCoroutine == null) fadeCoroutine = StartCoroutine(FadeOutElements(fadeOutDuration, fadeOutWaitDuration));
        }

        public void FadeTurnOn()
        {
            if (fadeCoroutine == null) fadeCoroutine = StartCoroutine(FadeInElements(fadeInDuration, fadeInWaitDuration));
        }

        private IEnumerator FadeInElements(float fadeDuration, float waitDuration)
        {
            if (canvasGroup == null)
            {
                Debug.LogWarning("UIFader: CanvasGroup is missing!");
                yield break;
            }

            if (!canvasGroup.gameObject.activeSelf)
                canvasGroup.gameObject.SetActive(true);

            // Linq method to find all child gameObjects that have graphic components
            GameObject[] childObjects = GetComponentsInChildren<Graphic>()
                .Where(t => t != transform)
                .Select(t => t.gameObject)
                .ToArray();

            // Disable all child objects initially
            foreach (var child in childObjects)
            {
                if (child != null) child.SetActive(false);
            }

            canvasGroup.alpha = 0f;

            // Wait before fading in
            yield return new WaitForSeconds(waitDuration);

            // Activate child objects at the start of fade
            foreach (var child in childObjects)
            {
                if (child != null) child.SetActive(true);
            }

            float elapsedTime = 0f;
            while (elapsedTime < fadeDuration)
            {
                elapsedTime += Time.deltaTime;
                canvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsedTime / fadeDuration);
                yield return null;
            }
            canvasGroup.alpha = 1f;
        }

        private IEnumerator FadeOutElements(float fadeDuration, float waitDuration)
        {
            if (canvasGroup == null)
            {
                Debug.LogWarning("UIFader: CanvasGroup is missing!");
                yield break;
            }

            // Linq method to find all child gameObjects that have graphic components
            GameObject[] childObjects = GetComponentsInChildren<Graphic>()
                .Where(t => t != transform)
                .Select(t => t.gameObject)
                .ToArray();

            // Wait before starting fade out
            yield return new WaitForSeconds(waitDuration);

            float elapsedTime = 0f;
            while (elapsedTime < fadeDuration)
            {
                elapsedTime += Time.deltaTime;
                canvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsedTime / fadeDuration);
                yield return null;
            }

            canvasGroup.alpha = 0f;

            // Deactivate child objects after fade
            foreach (var child in childObjects)
            {
                if (child != null) child.SetActive(false);
            }
        }


    }
}


    


