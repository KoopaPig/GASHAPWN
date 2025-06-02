using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GASHAPWN.UI {
    /// <summary>
    /// Controller for Level Preview on Stage Select Screen
    /// </summary>
    public class LevelPreviewController : MonoBehaviour
    {
        [Tooltip("Seconds it takes for level icons to cycle")]
        [SerializeField] private float cycleInterval = 3f;

        [Header("Graphical Elements")]
            [SerializeField] private Image levelIconA;
            [SerializeField] private Image levelIconB;
            [SerializeField] private TextMeshProUGUI levelTitle;

            [Tooltip("Reference to TV static overlay")]
            [SerializeField] private GameObject staticOverlay;

        // Duration for transition between level icons
        private float fadeDuration = 1.0f;

        private Coroutine fadeCoroutine;


        /// PRIVATE METHODS ///

        private void Awake()
        {
            staticOverlay.SetActive(false);
            levelIconA.canvasRenderer.SetAlpha(1f);
            levelIconB.canvasRenderer.SetAlpha(0f);
        }

        private IEnumerator SetStatic()
        {
            staticOverlay.SetActive(true);
            yield return new WaitForSeconds(0.1f);
            staticOverlay.SetActive(false);
        }

        // Cycle between levelIconA and levelIconB given cycleInterval
        private IEnumerator CycleFade()
        {
            yield return new WaitForSeconds(cycleInterval / 2);
            bool fadeToB = true;

            while (true)
            {
                float elapsed = 0f;

                while (elapsed < fadeDuration)
                {
                    float t = elapsed / fadeDuration;
                    levelIconA.canvasRenderer.SetAlpha(fadeToB ? 1f - t : t);
                    levelIconB.canvasRenderer.SetAlpha(fadeToB ? t : 1f - t);
                    elapsed += Time.deltaTime;
                    yield return null;
                }

                // Final correction
                levelIconA.canvasRenderer.SetAlpha(fadeToB ? 0f : 1f);
                levelIconB.canvasRenderer.SetAlpha(fadeToB ? 1f : 0f);

                fadeToB = !fadeToB;

                yield return new WaitForSeconds(cycleInterval / 2);
            }
        }


        /// PUBLIC METHODS ///

        /// <summary>
        /// Sets graphical elements in preview from given Level
        /// </summary>
        public void SetLevelPreview(Level selectedLevel)
        {
            StartCoroutine(SetStatic());
            levelIconA.sprite = selectedLevel.levelPreviewIconA;
            levelIconB.sprite = selectedLevel.levelPreviewIconB;
            levelTitle.text = selectedLevel.levelName;

            if (fadeCoroutine != null)
                StopCoroutine(fadeCoroutine);
            levelIconA.canvasRenderer.SetAlpha(1f);
            levelIconB.canvasRenderer.SetAlpha(0f);
            fadeCoroutine = StartCoroutine(CycleFade());
        }
    }
}