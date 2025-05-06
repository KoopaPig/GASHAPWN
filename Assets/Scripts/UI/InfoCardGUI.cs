using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using GASHAPWN.Audio;

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
        
        [Header("Collection Specific Elements")]
        [SerializeField] private GameObject newFigureBadge;
        [SerializeField] private TextMeshProUGUI collectionCountText;
        [SerializeField] private GameObject leftNavigationHint;
        [SerializeField] private GameObject rightNavigationHint;
        [SerializeField] private GameObject rotationHint;

        [Header("Slide-In Settings")]
        [SerializeField] private float slideDuration = 0.3f;
        [SerializeField] private float yOffsetInPixels = -1000f;
        [SerializeField] private AnimationCurve slideCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        
        private Vector2 offscreenPosition;
        private Vector2 onscreenPosition;
        private RectTransform rectTransform;

        private bool isSliding = false;
        private Coroutine slideCoroutine;
        public bool isVisible = false;

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();

            // Card starts offscreen
            onscreenPosition = rectTransform.anchoredPosition;
            offscreenPosition = new Vector2(onscreenPosition.x, onscreenPosition.y + yOffsetInPixels);
            rectTransform.anchoredPosition = offscreenPosition;
            
            // Hide optional elements initially
            HideNavigationHints();
            if (newFigureBadge != null)
                newFigureBadge.SetActive(false);
        }

        /// PUBLIC METHODS ///

        // Sets all information in Info Card given Figure
        public void SetFigureInfoCard(Figure figure, int amt)
        {
            // Set Figure Info
            figureName.text = figure.Name;
            starsGUI.SetStars(figure);
            figureDescription.text = figure.Description;

            figureAmount.text = "x " + amt.ToString();

            // Set seriesIcon and numberInSeriesText
            seriesIcon.sprite = figure.GetSeries().SeriesIcon;
            numberInSeriesText.text = string.Format("{0:000} / {1:000}", figure.GetNumberInSeries(), figure.GetSeries().Size());
        }

        // Slide in card from offscreen position
        public IEnumerator SlideCardIn()
        {
            isSliding = true;
            float elapsedTime =.0f;

            while (elapsedTime < slideDuration)
            {
                elapsedTime += Time.deltaTime;
                float t = slideCurve.Evaluate(elapsedTime / slideDuration);
                rectTransform.anchoredPosition = Vector2.Lerp(offscreenPosition, onscreenPosition, t);
                yield return null;
            }

            rectTransform.anchoredPosition = onscreenPosition;
            isSliding = false;
            isVisible = true;
        }
        
        // Slide out card to offscreen position
        public IEnumerator SlideCardOut()
        {
            isSliding = true;
            float elapsedTime = 0f;

            while (elapsedTime < slideDuration)
            {
                elapsedTime += Time.deltaTime;
                float t = slideCurve.Evaluate(elapsedTime / slideDuration);
                rectTransform.anchoredPosition = Vector2.Lerp(onscreenPosition, offscreenPosition, t);
                yield return null;
            }

            rectTransform.anchoredPosition = offscreenPosition;
            isSliding = false;
            isVisible = false;
        }
        
        // Show navigation hints for collection browsing
        public void ShowNavigationHints(bool hasLeftNode, bool hasRightNode)
        {
            if (leftNavigationHint != null)
                leftNavigationHint.SetActive(hasLeftNode);
                
            if (rightNavigationHint != null)
                rightNavigationHint.SetActive(hasRightNode);
                
            if (rotationHint != null)
                rotationHint.SetActive(true);
        }
        
        // Hide all navigation hints
        public void HideNavigationHints()
        {
            if (leftNavigationHint != null)
                leftNavigationHint.SetActive(false);
                
            if (rightNavigationHint != null)
                rightNavigationHint.SetActive(false);
                
            if (rotationHint != null)
                rotationHint.SetActive(false);
        }
        
        // Public method to slide in the card (starts the coroutine)
        public void SlideIn()
        {
            if (!isVisible && !isSliding)
            {
                if (slideCoroutine != null)
                    StopCoroutine(slideCoroutine);
                    
                slideCoroutine = StartCoroutine(SlideCardIn());
            }
        }
        
        // Public method to slide out the card (starts the coroutine)
        public void SlideOut()
        {
            if (isVisible && !isSliding)
            {
                if (slideCoroutine != null)
                    StopCoroutine(slideCoroutine);
                    
                slideCoroutine = StartCoroutine(SlideCardOut());
            }
        }
    }
}