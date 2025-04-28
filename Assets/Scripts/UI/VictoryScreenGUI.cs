using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;
using TMPro;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using System.Linq;
using Unity.VisualScripting;
using GASHAPWN.Utility;

namespace GASHAPWN.UI {
    public class VictoryScreenGUI : MonoBehaviour
    {
        [Header("Slide-In Settings")]
        [SerializeField] private GameObject victoryScreenFirstButton;
        [SerializeField] private float slideDuration = 0.3f;
        //[SerializeField] private float offsetInPixels = 200f;

        private Vector2 offscreenRight;
        private Vector2 onscreenPosition;
        private RectTransform rectTransform;

        [Header("Winner Elements")]
        [SerializeField] private TextMeshProUGUI winnerText;
        [SerializeField] GameObject winnerCrownGUI;

        private Vector3 winnerCrownPosition;

        [SerializeField] private ResultsContainer[] resultsContainers;

        /// PUBLIC METHODS ///

        // Slide In Victory Screen given waitDuration
        public IEnumerator SlideInVictoryScreen(float waitDuration)
        {
            // Stagger in results containers
            StartCoroutine(StaggerFadeInResultsContainers(waitDuration + 1f));

            yield return new WaitForSeconds(waitDuration);
            float elapsedTime = 0f;

            while (elapsedTime < slideDuration)
            {
                elapsedTime += Time.deltaTime;
                GetComponent<RectTransform>().anchoredPosition = Vector3.Lerp(offscreenRight, onscreenPosition, elapsedTime / slideDuration);
                yield return null;
            }

            GetComponent<RectTransform>().anchoredPosition = onscreenPosition;
            
            // Wait to turn on button
            StartCoroutine(WaitTurnOnButton(1.5f));
        }

        public void GoToNewFigure()
        {
            BattleManager.Instance.ChangeStateNewFigureScreen();
            // deactivate this stuff
            GetComponent<GraphicsFaderCanvas>().FadeTurnOff(true);
        }

        /// PRIVATE METHODS ///

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            if (GetComponentInParent<CanvasGroup>() != null)
            {
                GetComponentInParent<CanvasGroup>().interactable = false;
            }
            else Debug.LogError("VictoryScreenGUI: Parent Canvas of Victory Screen requires a CanvasGroup Component");

            // Screen starts offscreen
            float screenWidth = rectTransform.rect.width;
            onscreenPosition = Vector2.zero;
            offscreenRight = new Vector2(screenWidth, 0);
            rectTransform.anchoredPosition = offscreenRight;

            // Disable crown
            winnerCrownPosition = winnerCrownGUI.transform.position;
            winnerCrownGUI.SetActive(false);
            winnerCrownGUI.GetComponent<Animator>().enabled = false;

            // Disable winnerText
            winnerText.gameObject.SetActive(false);

            // Subscribe to OnWinningFigure and OnLosingFigure Events from BattleManager
            if (BattleManager.Instance != null)
            {
                BattleManager.Instance.OnWinner.AddListener(PopulateResults);
            }
        }

        // PopulateResults called when OnWinningFigure event triggered

        private void PopulateResults(GameObject player, string s, Figure f) {
            foreach (var (p, isWinner) in BattleManager.Instance.pendingPlayerResults) {
                PopulateResultsGivenPlayer(p, isWinner);
            }
            BattleManager.Instance.pendingPlayerResults.Clear();
            winnerCrownGUI.SetActive(true);
            // Can't figure out how to effectively set position of crown, so position is static
            StartCoroutine(SetCrown());
        }


        /// <summary>
        /// Populate results given player GameObject + isWinner bool
        /// </summary>
        /// <param name="player"></param>
        /// <param name="isWinner"></param>
        private void PopulateResultsGivenPlayer(GameObject player, bool isWinner)
        {
            var fig = player.GetComponent<PlayerAttachedFigure>().GetAttachedFigure();
            foreach (var container in resultsContainers)
            {
                // find container with matching tag
                if (container.playerTag == player.tag)
                {
                    container.playerIcon.sprite = fig.Icon;
                    container.playerText.text = fig.Name;

                    if (isWinner)
                    {
                        // move winning container to top
                        container.gameObject.GetComponent<RectTransform>().SetAsFirstSibling();
                        
                        // set winnerText
                        winnerText.text = fig.Name + " Wins!";
                    }
                    return;
                }
            }
            Debug.LogWarning($"VictoryScreenGUI: No ResultsContainer found for player tag: {player.tag}");
        }

        // Slides In Results Container given offset, duration, and wait buffer time
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

        // Stagger in all ResultsContainers given waitDuration
        private IEnumerator StaggerFadeInResultsContainers(float waitDuration)
        {
            yield return new WaitForEndOfFrame();
            foreach (var container in resultsContainers)
            {
                container.gameObject.SetActive(false);
            }
            yield return new WaitForSeconds(waitDuration);

            winnerText.gameObject.SetActive(true);
            float durationPerFade = 0.25f;
            Vector2 offset = new Vector2(150, 0);
            for (int i = 0; i < resultsContainers.Length; i++)
            {
                resultsContainers[i].gameObject.SetActive(true);
                var graphicsFader = resultsContainers[i].gameObject.GetComponent<GraphicsFaderCanvas>();
                graphicsFader.fadeInWaitDuration = durationPerFade * i;
                graphicsFader.FadeTurnOn(true);
                StartCoroutine(SlideInResultsContainer(resultsContainers[i].gameObject.GetComponent<RectTransform>(),
                    offset, durationPerFade, durationPerFade * i));
            }
        }

        // Wait to activate ContinueButton given waitDuration
        private IEnumerator WaitTurnOnButton(float waitDuration)
        {
            if (GetComponentInParent<CanvasGroup>() == null)
            {
                Debug.LogError("VictoryScreenGUI: CanvasGroup is missing.");
                yield break;
            }
            yield return new WaitForSeconds(waitDuration);
            EventSystemSelectHelper.SetSelectedGameObject(victoryScreenFirstButton); // Set new button here
            GetComponentInParent<CanvasGroup>().interactable = true;
        }

        private IEnumerator SetCrown() {
            yield return new WaitForNextFrameUnit();
            winnerCrownGUI.GetComponent<Animator>().enabled = true;
            winnerCrownGUI.GetComponent<GraphicsFaderCanvas>().FadeTurnOn(false);
        }

        private void OnDisable()
        {
            if (BattleManager.Instance != null)
            {
                BattleManager.Instance.OnWinner.RemoveListener(PopulateResults);
            }
        }
    }
}