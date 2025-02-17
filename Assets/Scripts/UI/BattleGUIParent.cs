using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace GASHAPWN.UI
{
    public class BattleGUIParent : MonoBehaviour
    {
        private Coroutine fadeCoroutine = null;

        public void FadeTurnOff()
        {
            if (fadeCoroutine == null) fadeCoroutine = StartCoroutine(FadeOutElements(0.15f, 3f));
        }

        public void FadeTurnOn()
        {
            if (fadeCoroutine == null) fadeCoroutine = StartCoroutine(FadeInElements(0.5f));
        }

        private IEnumerator FadeOutElements(float fadeDuration, float waitDuration)
        {
            yield return new WaitForSeconds(waitDuration);
            // Get all UI components under the parent
            Graphic[] graphics = GetComponentsInChildren<Graphic>();

            float timeElapsed = 0f;

            while (timeElapsed < fadeDuration)
            {
                float alpha = Mathf.Lerp(1f, 0f, timeElapsed / fadeDuration);

                foreach (var graphic in graphics)
                {
                    if (graphic != null)
                    {
                        Color color = graphic.color;
                        color.a = alpha;
                        graphic.color = color;
                    }
                }

                timeElapsed += Time.deltaTime;
                yield return null;
            }

            foreach (var graphic in graphics)
            {
                if (graphic != null)
                {
                    Color color = graphic.color;
                    color.a = 0;
                    graphic.color = color;
                    graphic.gameObject.SetActive(false);
                }
            }
        }

        private IEnumerator FadeInElements(float duration)
        {
            // Get all UI components under the parent
            Graphic[] graphics = GetComponentsInChildren<Graphic>();

            float timeElapsed = 0f;

            while (timeElapsed < duration)
            {
                float alpha = Mathf.Lerp(0f, 1f, timeElapsed / duration);

                foreach (var graphic in graphics)
                {
                    if (graphic != null)
                    {
                        Color color = graphic.color;
                        color.a = alpha;
                        graphic.color = color;
                    }
                }

                timeElapsed += Time.deltaTime;
                yield return null;
            }

            foreach (var graphic in graphics)
            {
                if (graphic != null)
                {
                    Color color = graphic.color;
                    color.a = 1;
                    graphic.color = color;
                    graphic.gameObject.SetActive(true);
                }
            }
        }
    }
}


