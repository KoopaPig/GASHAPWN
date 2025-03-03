using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;
using TMPro;
using UnityEngine.Events;

namespace GASHAPWN.UI {
    public class VictoryScreenGUI : MonoBehaviour
    {
        [Header("Slide-In Settings")]
        [SerializeField] private GameObject victoryScreenFirstButton;
        [SerializeField] private float slideDuration = 0.3f;
        [SerializeField] private float offsetInPixels = 200f;

        private Vector2 offscreenRight;
        private Vector2 onscreenPosition;
        private RectTransform rectTransform;

        [Header("Winner Elements")]
        [SerializeField] private TextMeshProUGUI winnerText;
        [SerializeField] GameObject winnerCrownGUI;


        private string winnerTag = null;
        private ResultsContainer[] resultsContainers;

        // TEMPORARY DEBUG
        [SerializeField] private Figure debugFigure;


        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            if (GetComponentInParent<CanvasGroup>() != null) 
            {
                GetComponentInParent<CanvasGroup>().interactable = false;
            } else Debug.LogError("VictoryScreenGUI: Parent Canvas of Victory Screen requires a CanvasGroup Component");

            // Screen starts offscreen
            float screenWidth = rectTransform.rect.width; 
            onscreenPosition = Vector2.zero;
            offscreenRight = new Vector2(screenWidth, 0);
            rectTransform.anchoredPosition = offscreenRight;

            // Disable crown
            winnerCrownGUI.SetActive(false);
            winnerCrownGUI.GetComponent<Animator>().enabled = false;

            // Find all resultsContainers
            resultsContainers = FindObjectsByType<ResultsContainer>(FindObjectsSortMode.None);

            // Disable winnerText
            winnerText.gameObject.SetActive(false);
        }

        // need to subscribe to OnWinningFigure event from BattleManager

        /// <summary>
        /// Set winner given playerTag + figure, very important that this is called after victory screen activated
        /// </summary>
        /// <param name="playerTag"></param>
        /// <param name="figure"></param>
        public void SetWinner(string playerTag, Figure figure)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag(playerTag);
            if (playerObj != null)
            {
                winnerText.text = figure.Name + " Wins!";
                winnerTag = playerTag;
            }
            else
            {
                Debug.LogError($"VictoryScreenGUI: Cannot set winner. No GameObject found with tag {playerTag}");
            }
        }

        /// <summary>
        /// Populate results given playerTag + figure, very important that this is called after victory screen activated
        /// </summary>
        /// <param name="playerTag"></param>
        /// <param name="figure"></param>
        public void PopulateResultsGivenPlayer(string playerTag, Figure figure)
        {
            foreach (var container in resultsContainers)
            {
                // find container with matching playerTag
                if (container.playerTag == playerTag)
                {
                    container.playerIcon.sprite = figure.Icon;
                    container.playerText.text = figure.Name;


                    if (container.playerTag == winnerTag)
                    {
                        // set active crown here
                        winnerCrownGUI.SetActive(true);
                        // move winning container to top
                        container.gameObject.GetComponent<RectTransform>().SetAsFirstSibling();

                        Canvas.ForceUpdateCanvases();

                        // set crown position
                        Vector3 newPosition = winnerCrownGUI.transform.position;
                        newPosition.y = container.transform.position.y;
                        winnerCrownGUI.transform.position = newPosition;
                        winnerCrownGUI.GetComponent<Animator>().enabled = true;
                    }
                    return;
                }
            }
            Debug.LogWarning($"VictoryScreenGUI: No ResultsContainer found for playerTag: {playerTag}");
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
            StartCoroutine(WaitTurnOnButton(1.5f));
        }

        public void StaggerFadeInResultsContainers()
        {
            winnerText.gameObject.SetActive(true);
            float durationPerFade = 0.25f;
            Vector2 offset = new Vector2(150, 0);
            for (int i = 0; i < resultsContainers.Length; i++)
            {
                var graphicsFader = resultsContainers[i].gameObject.GetComponent<GraphicsFader>();
                graphicsFader.fadeInWaitDuration = durationPerFade * i;
                graphicsFader.FadeTurnOn();
                StartCoroutine(SlideInResultsContainer(resultsContainers[i].gameObject.GetComponent<RectTransform>(), 
                    offset, durationPerFade, durationPerFade * i));
            }
        }

        public void GoToNewFigure()
        {
            BattleManager.Instance.ChangeStateNewFigureScreen();
            // deactivate this stuff
            GetComponent<GraphicsFaderCanvas>().FadeTurnOff();
        }


        // TEMPORARY DEBUG
        public void SimulatePlayer2WinningDebug()
        {
            SetWinner("Player2", debugFigure);
            PopulateResultsGivenPlayer("Player2", debugFigure);
            PopulateResultsGivenPlayer("Player1", debugFigure);
            StaggerFadeInResultsContainers();

            // will have to figure out how to populate results for players with only one event call
        }

        /// PRIVATE METHODS ///

        private IEnumerator SlideInResultsContainer(RectTransform transform, Vector2 offset, float duration, float waitDuration)
        {
            yield return new WaitForSeconds(waitDuration);
            Vector2 startPos = transform.anchoredPosition + offset;
            Vector2 targetPos = transform.anchoredPosition;
            float elapsedTime = 0f;

            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                transform.anchoredPosition = Vector2.Lerp(startPos, targetPos, elapsedTime / duration);
                yield return null;
            }

            transform.anchoredPosition = targetPos;
        }

        private IEnumerator WaitTurnOnButton(float waitDuration)
        {
            if (GetComponentInParent<CanvasGroup>() == null)
            {
                Debug.LogError("VictoryScreenGUI: CanvasGroup is missing.");
                yield break;
            }
            yield return new WaitForSeconds(waitDuration);
            EventSystem.current.SetSelectedGameObject(victoryScreenFirstButton); // Set new button here
            GetComponentInParent<CanvasGroup>().interactable = true;
        }
    }
}