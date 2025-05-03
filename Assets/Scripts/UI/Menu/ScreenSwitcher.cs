using System.Collections;
using GASHAPWN.Utility;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
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

        [SerializeField] private bool isLevelSelectDisplayDebug = false;

        private void Start()
        {
            var parentWidth = controlsBindScreen.parent.GetComponent<RectTransform>().rect.width;
            onscreenPosition = controlsBindScreen.anchoredPosition;
            offscreenRight = new Vector2(parentWidth + offsetInPixels, levelSelectScreen.anchoredPosition.y);
            offscreenLeft = new Vector2(-parentWidth - offsetInPixels, levelSelectScreen.anchoredPosition.y);
            if (!isLevelSelectDisplayDebug)
            {
                levelSelectScreen.anchoredPosition = offscreenRight; // Start levelSelectScreen off-screen
                //levelSelectScreen.GetComponent<CanvasGroup>().interactable = false;
            }
            
        }

        public void ShowLevelSelectScreen()
        {
            if (transitionCoroutine != null) return;
            GetComponent<LevelSelect>().isControlsBindScreen = false;
            transitionCoroutine = StartCoroutine(SlideInLevelSelectScreen());
            PlayerInputManager.instance.DisableJoining();
        }

        public void ShowControlsBindScreen()
        {
            if (transitionCoroutine != null) return;
            GetComponent<LevelSelect>().isControlsBindScreen = true;
            transitionCoroutine = StartCoroutine(SlideInControlsBindScreen());
            PlayerInputManager.instance.EnableJoining();
        }

        // Slides in LevelSelectScreen, sets new selected button
        private IEnumerator SlideInLevelSelectScreen()
        {
            float elapsedTime = 0f;
            //controlsBindScreen.GetComponent<CanvasGroup>().interactable = false;

            while (elapsedTime < slideDuration)
            {
                elapsedTime += Time.deltaTime;
                levelSelectScreen.anchoredPosition = Vector3.Lerp(offscreenRight, onscreenPosition, elapsedTime / slideDuration);
                controlsBindScreen.anchoredPosition = Vector3.Lerp(onscreenPosition, offscreenLeft, elapsedTime / slideDuration);
                yield return null;
            }

            levelSelectScreen.anchoredPosition = onscreenPosition;
            EventSystemSelectHelper.SetSelectedGameObject(levelSelectFirstButton); // Set new button here
            transitionCoroutine = null; // reset
            //levelSelectScreen.GetComponent<CanvasGroup>().interactable = true;
        }


        // Slides in ControlsBindScreen, sets new selected button
        private IEnumerator SlideInControlsBindScreen()
        {
            float elapsedTime = 0f;
            //levelSelectScreen.GetComponent<CanvasGroup>().interactable = false;

            while (elapsedTime < slideDuration)
            {
                elapsedTime += Time.deltaTime;
                levelSelectScreen.anchoredPosition = Vector3.Lerp(onscreenPosition, offscreenRight , elapsedTime / slideDuration);
                controlsBindScreen.anchoredPosition = Vector3.Lerp(offscreenLeft, onscreenPosition , elapsedTime / slideDuration);
                yield return null;
            }

            controlsBindScreen.anchoredPosition = onscreenPosition;
            EventSystemSelectHelper.SetSelectedGameObject(controlsBindFirstButton); // Set new button here
            transitionCoroutine = null; // reset
            //controlsBindScreen.GetComponent<CanvasGroup>().interactable = true;
        }
    }
}