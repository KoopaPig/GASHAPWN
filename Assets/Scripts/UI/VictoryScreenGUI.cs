using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;

namespace GASHAPWN.UI {
    public class VictoryScreenGUI : MonoBehaviour
    {
        [SerializeField] private GameObject victoryScreenFirstButton;
        [SerializeField] private float slideDuration = 0.3f;
        [SerializeField] private float offsetInPixels = 200f;

        private Vector2 offscreenRight;
        private Vector2 onscreenPosition;
        private RectTransform rectTransform;

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            if (GetComponentInParent<CanvasGroup>() != null) 
            {
                GetComponentInParent<CanvasGroup>().interactable = false;
            } else Debug.LogError("Parent Canvas of Victory Screen requires a CanvasGroup Component");

            float screenWidth = rectTransform.rect.width; 
            onscreenPosition = Vector2.zero;
            offscreenRight = new Vector2(screenWidth, 0);
            rectTransform.anchoredPosition = offscreenRight;
        }

        public void SetWinner()
        {

        }

        // Slide In Victory Screen given waitDuration
        public IEnumerator SlideInVictoryScreen(float waitDuration)
        {
            yield return new WaitForSeconds(waitDuration);
            float elapsedTime = 0f;

            while (elapsedTime < slideDuration)
            {
                elapsedTime += Time.deltaTime;
                GetComponent<RectTransform>().anchoredPosition = Vector3.Lerp(offscreenRight, onscreenPosition, elapsedTime / slideDuration);
                yield return null;
            }

            GetComponent<RectTransform>().anchoredPosition = onscreenPosition;
            EventSystem.current.SetSelectedGameObject(victoryScreenFirstButton); // Set new button here
            GetComponentInParent<CanvasGroup>().interactable = true;
        }
    }
}