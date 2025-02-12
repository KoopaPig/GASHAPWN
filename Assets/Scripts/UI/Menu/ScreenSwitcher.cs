using System.Collections;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace GASHAPWN.UI {
    public class ScreenSwitcher : MonoBehaviour
    {
        [Header("Slide Transition Objects")]
        [SerializeField] private RectTransform controlsBindScreen;
        [SerializeField] private RectTransform levelSelectScreen;
        [SerializeField] private float slideDuration = 0.3f;
        [SerializeField] private float offsetInPixels = 300;

        [Header("Buttons Selected On Transition")]
        [SerializeField] private GameObject controlsBindFirstButton;
        [SerializeField] private GameObject levelSelectFirstButton;

        private Vector2 offscreenRight;
        private Vector2 offscreenLeft;
        private Vector2 onscreenPosition;
        private Coroutine transitionCoroutine;

        private void Start()
        {
            var parentWidth = controlsBindScreen.parent.GetComponent<RectTransform>().rect.width;
            onscreenPosition = controlsBindScreen.anchoredPosition;
            offscreenRight = new Vector2(parentWidth + offsetInPixels, levelSelectScreen.anchoredPosition.y);
            offscreenLeft = new Vector2(-parentWidth - offsetInPixels, levelSelectScreen.anchoredPosition.y);
            
            levelSelectScreen.anchoredPosition = offscreenRight; // Start levelSelectScreen off-screen
        }

        public void ShowLevelSelectScreen()
        {
            if (transitionCoroutine != null) return;
            GetComponent<LevelSelect>().isControlsBindScreen = false;
            transitionCoroutine = StartCoroutine(SlideInLevelSelectScreen());
        }

        public void ShowControlsBindScreen()
        {
            if (transitionCoroutine != null) return;
            GetComponent<LevelSelect>().isControlsBindScreen = true;
            transitionCoroutine = StartCoroutine(SlideInControlsBindScreen());
        }

        // Slides in LevelSelectScreen, sets new selected button
        private IEnumerator SlideInLevelSelectScreen()
        {
            float elapsedTime = 0f;

            while (elapsedTime < slideDuration)
            {
                elapsedTime += Time.deltaTime;
                levelSelectScreen.anchoredPosition = Vector3.Lerp(offscreenRight, onscreenPosition, elapsedTime / slideDuration);
                controlsBindScreen.anchoredPosition = Vector3.Lerp(onscreenPosition, offscreenLeft, elapsedTime / slideDuration);
                yield return null;
            }

            levelSelectScreen.anchoredPosition = onscreenPosition;
            EventSystem.current.SetSelectedGameObject(levelSelectFirstButton); // Set new button here

            transitionCoroutine = null; // reset
        }

        // Slides in ControlsBindScreen, sets new selected button

        private IEnumerator SlideInControlsBindScreen()
        {
            float elapsedTime = 0f;

            while (elapsedTime < slideDuration)
            {
                elapsedTime += Time.deltaTime;
                levelSelectScreen.anchoredPosition = Vector3.Lerp(onscreenPosition, offscreenRight , elapsedTime / slideDuration);
                controlsBindScreen.anchoredPosition = Vector3.Lerp(offscreenLeft, onscreenPosition , elapsedTime / slideDuration);
                yield return null;
            }

            controlsBindScreen.anchoredPosition = onscreenPosition;
            EventSystem.current.SetSelectedGameObject(controlsBindFirstButton); // Set new button here

            transitionCoroutine = null; // reset
        }
    }
}