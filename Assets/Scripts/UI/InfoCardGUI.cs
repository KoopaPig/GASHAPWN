using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

namespace GASHAPWN.UI
{
    public class InfoCardGUI : MonoBehaviour
    {
        [Header("Info Card")]
        [SerializeField] TextMeshProUGUI figureName;
        [SerializeField] StarsGUI starsGUI;
        [SerializeField] TextMeshProUGUI figureAmount;
        [SerializeField] TextMeshProUGUI figureDescription;

        [SerializeField] Image seriesIcon;
        [SerializeField] TextMeshProUGUI numberInSeriesText;

        [Header("Slide-In Settings")]
        [SerializeField] private float slideDuration = 0.3f;
        [SerializeField] private float yOffsetInPixels = -1000f;
        private Vector2 offscreenPosition;
        private Vector2 onscreenPosition;
        private RectTransform rectTransform;

        private bool isSliding = false;
        private Coroutine slideCoroutine;

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();

            // Card starts offscreen
            onscreenPosition = rectTransform.anchoredPosition;
            offscreenPosition = new Vector2(onscreenPosition.x, onscreenPosition.y + yOffsetInPixels);
            rectTransform.anchoredPosition = offscreenPosition;
        }

        /// PUBLIC METHODS ///

        // Sets all information in Info Card given Figure
        public void SetFigureInfoCard(Figure figure)
        {
            // Set Figure Info
            figureName.text = figure.Name;
            starsGUI.SetStars(figure);
            figureDescription.text = figure.Description;

            // TODO: Need to find out how to set figureAmount (might have to interface with Collection)

            // Set seriesIcon and numberInSeriesText
            seriesIcon.sprite = figure.GetSeries().SeriesIcon;
            numberInSeriesText.text = string.Format("{0:000} / {1:000}", figure.GetNumberInSeries(), figure.GetSeries().Size());
        }

        // Slide in card from offscreen position
        public IEnumerator SlideCardIn()
        {
            isSliding = true;
            float elapsedTime = 0f;

            while (elapsedTime < slideDuration)
            {
                elapsedTime += Time.deltaTime;
                GetComponent<RectTransform>().anchoredPosition = Vector3.Lerp(offscreenPosition, onscreenPosition, elapsedTime / slideDuration);
                yield return null;
            }

            GetComponent<RectTransform>().anchoredPosition = onscreenPosition;
            isSliding = false;
        }

    }
}


