using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using UnityEngine.EventSystems;

namespace GASHAPWN.UI
{
    public class ControlsBindBox : MonoBehaviour, ISelectHandler
    {
        public bool IsControllerDetected = false;
        public ControlScheme controlScheme;
        public int playerIndex;

        [Header("Feedback GUI Elements")]
        public Image xInputImage;
        public Image keyboardImage;
        public TextMeshProUGUI feedbackText;
        [SerializeField] private Image background;
        [SerializeField] private Image border;
        [SerializeField] private Image gradientDots;
        [SerializeField] private Image belowBox;
        [SerializeField] private Color selectedColorBG;
        [SerializeField] private Color selectedColorFG;

        private Color backgroundColor;
        private Color borderColor;
        private Color gradientDotsColor;
        private Color belowBoxColor;

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

        public void UpdateControls()
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

            // Maybe around here check if already assigned and set corresponding image and set it active

            if (!IsControllerDetected)
            {
                feedbackText.text = "Deactivated";
                xInputImage.gameObject.SetActive(false);
                keyboardImage.gameObject.SetActive(false);
            }
            else 
            {
                feedbackText.text = $"Player {playerIndex+1} Ready!";
                // Device name could be fetched from ControllerManager if needed
            }
        }

        public void SetSelected(bool value)
        {
            if (value)
            {
                EventSystem.current.SetSelectedGameObject(this.gameObject);
                StartCoroutine(LerpColor(background, backgroundColor, selectedColorBG, 0.15f));
                StartCoroutine(LerpColor(border, borderColor, selectedColorBG, 0.15f));
                StartCoroutine(LerpColor(gradientDots, gradientDotsColor, selectedColorFG, 0.15f));
                StartCoroutine(LerpColor(belowBox, belowBoxColor, selectedColorBG, 0.15f));
            } 
            else
            {
                EventSystem.current.SetSelectedGameObject(null);
                StartCoroutine(LerpColor(background, selectedColorBG, backgroundColor, 0.15f));
                StartCoroutine(LerpColor(border, selectedColorBG, borderColor, 0.15f));
                StartCoroutine(LerpColor(gradientDots, selectedColorFG, gradientDotsColor, 0.15f));
                StartCoroutine(LerpColor(belowBox, selectedColorBG, belowBoxColor, 0.15f));
            }
        }

        public void OnSelect(BaseEventData eventData)
        {
            // If the box is already assigned, don't do anything
            if (IsControllerDetected)
            {
                Debug.Log($"ControlsBindBox: Player {playerIndex+1} already has a controller assigned");
                return;
            }

            // Make sure LevelSelect exists
            if (LevelSelect.Instance == null)
            {
                Debug.LogError("ControlsBindBox: LevelSelect.Instance is null!");
                return;
            }

            Debug.Log($"ControlsBindBox: Starting controller detection for Player {playerIndex+1}");
            feedbackText.text = "Press any button...";

            // Start listening for input for this controller
            LevelSelect.Instance.StartListeningForController(playerIndex);
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
    }

    public enum ControlScheme
    {
        XINPUT,
        KEYBOARD
    }
}