using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

namespace GASHAPWN.UI
{
    public class ControlsBindBox : MonoBehaviour
    {
        public bool IsControllerDetected = false;
        public ControlScheme controlScheme;

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

            if (!IsControllerDetected)
            {
                feedbackText.text = "Press any button";
                xInputImage.gameObject.SetActive(false);
                keyboardImage.gameObject.SetActive(false);
            }
            else feedbackText.text = "Controls detected!";
        }

        public void SetSelected(bool value)
        {
            if (value)
            {
                StartCoroutine(LerpColor(background, backgroundColor, selectedColorBG, 0.15f));
                StartCoroutine(LerpColor(border, borderColor, selectedColorBG, 0.15f));
                StartCoroutine(LerpColor(gradientDots, gradientDotsColor, selectedColorFG, 0.15f));
                StartCoroutine(LerpColor(belowBox, belowBoxColor, selectedColorBG, 0.15f));
            } else
            {
                StartCoroutine(LerpColor(background, selectedColorBG, backgroundColor, 0.15f));
                StartCoroutine(LerpColor(border, selectedColorBG, borderColor, 0.15f));
                StartCoroutine(LerpColor(gradientDots, selectedColorFG, gradientDotsColor, 0.15f));
                StartCoroutine(LerpColor(belowBox, selectedColorBG, belowBoxColor,  0.15f));
            }
        }

        private IEnumerator LerpColor(Image targetImage, Color startColor, Color endColor, float duration)
        {
            float timeElapsed = 0f;

            while (timeElapsed < duration)
            {
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

