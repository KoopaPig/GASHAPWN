using GASHAPWN.Utility;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.ProBuilder;
using UnityEngine.UI;

namespace GASHAPWN.UI
{
    [RequireComponent(typeof(GraphicsFaderCanvas))]
    public class SlideableScreenGUI : MonoBehaviour 
    {
        [Header("Slide Settings")]
        [Tooltip("First button to select when screen slides in")]
        [SerializeField] protected GameObject firstSelectedButton;

        [Tooltip("Duration of slide-in animation")]
        [SerializeField] protected float slideDuration = 0.3f;

        [Tooltip("Slide-In direction")]
        [SerializeField] protected SlideDirection slideDirection = SlideDirection.Left;

        [Tooltip("Offset to add to slide direction")]
        [SerializeField] protected float offset = 0f;

        // Define what position is offscreen
        protected Vector2 offscreenPosition;
        // Define what position is onscreen (0,0)
        protected Vector2 onscreenPosition;
        // Reference to RectTransform of Canvas
        protected RectTransform rectTransform;

        private Coroutine activeSlideRoutine;

        protected GraphicsFaderCanvas fader;


        /// PUBLIC METHODS ///

        /// <summary>
        /// Main interface for toggling SlideableScreen visibility
        /// </summary>
        /// <param name="visible"> Whether screen appears or disappears</param>
        /// <param name="deactivate"> Whether screen should be deactivated when offscreen </param>
        /// <param name="delay"> Delay before screen slides </param>
        /// <param name="buttonActivationWaitTime"> Wait time before buttons active when sliding screen in </param>
        public void SetVisible(bool visible, bool deactivate = true, bool fade = false, float delay = 0f, float buttonActivationWaitTime = 0f)
        {
            // stop any currently running slide
            if (activeSlideRoutine != null)
                StopCoroutine(activeSlideRoutine);

            if (visible)
            {
                activeSlideRoutine = StartCoroutine(SlideInScreen(delay, fade, buttonActivationWaitTime));
            }
            else
            {
                activeSlideRoutine = StartCoroutine(SlideOutScreen(deactivate, fade, delay));
            }
                
        }

        public enum SlideDirection { Left, Right, Up, Down }

        /// PROTECTED / PRIVATE METHODS ///

        protected virtual void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            fader = GetComponent<GraphicsFaderCanvas>();
            GetComponentInParent<CanvasGroup>().interactable = false;

            // Screen starts offscreen
            float screenWidth = rectTransform.rect.width;
            onscreenPosition = rectTransform.anchoredPosition;
            offscreenPosition = GetOffscreenPosition(slideDirection, offset);
            rectTransform.anchoredPosition = offscreenPosition;
        }

        // Returns offscreen position given SlideDirection
        protected Vector2 GetOffscreenPosition(SlideDirection dir, float offset = 0f)
        {
            float width = rectTransform.rect.width;
            float height = rectTransform.rect.height;

            return dir switch
            {
                SlideDirection.Right => onscreenPosition + new Vector2(width + offset, 0),
                SlideDirection.Left => onscreenPosition + new Vector2(-width - offset, 0),
                SlideDirection.Up => onscreenPosition + new Vector2(0, height + offset),
                SlideDirection.Down => onscreenPosition + new Vector2(0, -height - offset),
                _ => onscreenPosition
            };
        }
        
        /// <summary>
        /// Slide In Screen given wait duration and wait time for button activation
        /// </summary>
        protected virtual IEnumerator SlideInScreen(float waitDuration, bool fade = false, float buttonActivationWaitTime = 0f)
        {
            yield return new WaitForSeconds(waitDuration);
            if (firstSelectedButton != null)
            {
                if (GetComponentInParent<CanvasGroup>() == null)
                {
                    Debug.LogError($"{nameof(SlideableScreenGUI)}: CanvasGroup is missing.");
                    yield break;
                }
                yield return new WaitForSeconds(buttonActivationWaitTime);
                EventSystemSelectHelper.SetSelectedGameObject(firstSelectedButton);
                GetComponentInParent<CanvasGroup>().interactable = true;
            }
            yield return SlideRoutine(offscreenPosition, onscreenPosition);
        }

        /// <summary>
        /// Slide out screen given wait duration
        /// </summary>
        protected virtual IEnumerator SlideOutScreen(bool deactivate, bool fade = false, float waitDuration = 0f)
        {
            EventSystemSelectHelper.SetSelectedGameObject(null);
            if (firstSelectedButton != null)
            {
                if (GetComponentInParent<CanvasGroup>() == null)
                {
                    Debug.LogError($"{nameof(SlideableScreenGUI)}: CanvasGroup is missing.");
                    yield break;
                }
                GetComponentInParent<CanvasGroup>().interactable = false;
            }

            yield return new WaitForSeconds(waitDuration);
            yield return SlideRoutine(onscreenPosition, offscreenPosition);
        }

        // Slide screen from start position to end position
        private IEnumerator SlideRoutine(Vector2 start, Vector2 end)
        {
            float elapsedTime = 0f;
            while (elapsedTime < slideDuration)
            {
                elapsedTime += Time.unscaledDeltaTime;
                rectTransform.anchoredPosition = Vector2.Lerp(start, end, elapsedTime / slideDuration);
                yield return null;
            }
            rectTransform.anchoredPosition = end;
        }
    }
}