using GASHAPWN.Utility;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace GASHAPWN.UI
{
    /// <summary>
    /// UI Interface for binding player input in LevelSelect scene
    /// </summary>
    public class ControlsBindBox : MonoBehaviour, ISelectHandler
    {
        // Flag for whether this ControlsBindBox has a corresponding controller
        [NonSerialized] public bool IsControllerDetected = false;
        // ControlScheme corresponding to this ControlsBindBox
        [NonSerialized] public ControlScheme controlScheme;

        [Tooltip("Player index corresponding to this object")]
        public int playerIndex;

        [Header("Feedback GUI Elements")]
            [Tooltip("Image for XInput control scheme")]
            public Image xInputImage;

            [Tooltip("Image for Keyboard control scheme")]
            public Image keyboardImage;

        [Tooltip("Reference to feedback text controls help element")]
        [SerializeField] private ControlsBindFeedbackText feedbackText;

            // Graphical Elements
            [SerializeField] private Image background;
            [SerializeField] private Image border;
            [SerializeField] private Image gradientDots;
            [SerializeField] private Image belowBox;

            // Selection Colors
            [SerializeField] private Color selectedColorBG;
            [SerializeField] private Color selectedColorFG;

        private Color backgroundColor;
        private Color borderColor;
        private Color gradientDotsColor;
        private Color belowBoxColor;


        /// PRIVATE METHODS ///
        
        private void Awake()
        {
            backgroundColor = background.color;
            borderColor = border.color;
            gradientDotsColor = gradientDots.color;
            belowBoxColor = belowBox.color;
        }

        private void Start()
        {
            xInputImage.gameObject.SetActive(false);
            keyboardImage.gameObject.SetActive(false);
        }

        private IEnumerator LerpColor(Image targetImage, Color startColor, Color endColor, float duration)
        {
            float timeElapsed = 0f;

            while (timeElapsed < duration)
            {
                timeElapsed += Time.deltaTime;
                float t = timeElapsed / duration;
                targetImage.color = Color.Lerp(startColor, endColor, t);

                timeElapsed += Time.deltaTime;
                yield return null;
            }

            targetImage.color = endColor;
        }


        /// PUBLIC METHODS ///

        /// <summary>
        /// Update controller icon according to controlScheme and set feedback text
        /// </summary>
        public void UpdateControllerIcon()
        {
            switch (controlScheme)
            {
                case ControlScheme.XINPUT:
                    xInputImage.gameObject.SetActive(true);
                    keyboardImage.gameObject.SetActive(false);
                    break;
                case ControlScheme.KEYBOARD:
                    xInputImage.gameObject.SetActive(false);
                    keyboardImage.gameObject.SetActive(true);
                    break;
                default: 
                    break;
            }

            if (!IsControllerDetected)
            {
                feedbackText.SetState(ControlsBindFeedbackText.FeedbackState.Waiting, playerIndex);
                xInputImage.gameObject.SetActive(false);
                keyboardImage.gameObject.SetActive(false);
            }
            else 
            {
                feedbackText.SetState(ControlsBindFeedbackText.FeedbackState.Connected, playerIndex);
                // Device name could be fetched from ControllerManager if needed
            }
        }

        /// <summary>
        /// Set ControlsBindBox as selected or not selected
        /// </summary>
        /// <param name="value"></param>
        public void SetSelected(bool value)
        {
            if (value)
            {
                EventSystemSelectHelper.SetSelectedGameObject(this.gameObject);
                EventSystem.current.SetSelectedGameObject(this.gameObject);
                StartCoroutine(LerpColor(background, backgroundColor, selectedColorBG, 0.15f));
                StartCoroutine(LerpColor(border, borderColor, selectedColorBG, 0.15f));
                StartCoroutine(LerpColor(gradientDots, gradientDotsColor, selectedColorFG, 0.15f));
                StartCoroutine(LerpColor(belowBox, belowBoxColor, selectedColorBG, 0.15f));
            } 
            else
            {
                EventSystemSelectHelper.SetSelectedGameObject(null);
                StartCoroutine(LerpColor(background, selectedColorBG, backgroundColor, 0.15f));
                StartCoroutine(LerpColor(border, selectedColorBG, borderColor, 0.15f));
                StartCoroutine(LerpColor(gradientDots, selectedColorFG, gradientDotsColor, 0.15f));
                StartCoroutine(LerpColor(belowBox, selectedColorBG, belowBoxColor, 0.15f));
            }
        }

        /// <summary>
        /// Actions to execution when object is selected
        /// </summary>
        /// <param name="eventData"></param>
        public void OnSelect(BaseEventData eventData)
        {
            // If the box is already assigned, don't do anything
            if (IsControllerDetected)
            {
                Debug.Log($"{nameof(ControlsBindBox)}: Player {playerIndex+1} already has a controller assigned");
                return;
            }

            // Make sure LevelSelect exists
            if (LevelSelect.Instance == null)
            {
                Debug.LogError($"{nameof(ControlsBindBox)}: LevelSelect.Instance is null!");
                return;
            }

            Debug.Log($"{nameof(ControlsBindBox)}: Starting controller detection for Player {playerIndex+1}");

            // TODO: This feedback should be more specific
            feedbackText.SetState(ControlsBindFeedbackText.FeedbackState.Prompting, playerIndex);
        }
    }
}