using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;
using TMPro;
using UnityEngine.Events;

namespace GASHAPWN.UI {
    public class VictoryScreenGUI : MonoBehaviour
    {
        [SerializeField] private GameObject victoryScreenFirstButton;
        [SerializeField] private float slideDuration = 0.3f;
        [SerializeField] private float offsetInPixels = 200f;

        private Vector2 offscreenRight;
        private Vector2 onscreenPosition;
        private RectTransform rectTransform;

        private TextMeshProUGUI winnerText;

        private bool isResultsPopulated = false;

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

        public void SetWinner(string playerTag, Figure figure)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag(playerTag);
            if (playerObj != null)
            {
                winnerText.text = figure.Name;
            }
            else
            {
                Debug.LogError($"VictoryScreenGUI: Cannot set winner. No GameObject found with tag {playerTag}");
            }
        }

        public void PopulateResultsGivenPlayer(string playerTag, Figure figure)
        {
            if (!isResultsPopulated)
            {
                ResultsContainer[] resultsContainers = FindObjectsByType<ResultsContainer>(FindObjectsSortMode.None);

                foreach (var container in resultsContainers)
                {
                    // find container with matching playerTag
                    if (container.playerTag == playerTag)
                    {
                        container.playerIcon.sprite = figure.Icon;
                        container.playerText.text = figure.Name;
                        return;
                    }
                }
                Debug.LogWarning($"VictoryScreenGUI: No ResultsContainer found for playerTag: {playerTag}");
            }
            else { Debug.Log("VictoryScreenGUI: Results have already been set."); return; }

            
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