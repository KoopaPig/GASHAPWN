using UnityEngine;
using System.Collections;
using TMPro;
using Unity.VisualScripting;
using GASHAPWN.Utility;

namespace GASHAPWN.UI {
    /// <summary>
    /// Controller for Results Screen GUI
    /// </summary>
    public class ResultsScreenGUI : MonoBehaviour
    {
        // IN FUTURE: If I want support for more than 2 players, this needs to be more dynamic
        [Header("Results Containers")]
            [Tooltip("Array of ResultsContainers, one per player")]
            [SerializeField] private ResultsContainer[] resultsContainers;

        [Header("Winner Elements")]
            [Tooltip("Reference to text for winner name")]
            [SerializeField] private TextMeshProUGUI winnerText;

            [Tooltip("Crown GUI element for winner")]
            [SerializeField] GameObject winnerCrownGUI;

            // Store position of winnerCrownGUI
            private Vector3 winnerCrownPosition;

        [Header("Slide-In Settings")]
            [Tooltip("First button to select on Results Screen")]
            [SerializeField] private GameObject resultsScreenFirstButton;

            [Tooltip("Duration of slide-in animation")]
            [SerializeField] private float slideDuration = 0.3f;

            // Define what position is offscreen to the right
            private Vector2 offscreenRight;
            // Define what position is onscreen (0,0)
            private Vector2 onscreenPosition;
            // Reference to RectTransform of Canvas
            private RectTransform rectTransform;


        /// PUBLIC METHODS ///

        /// <summary> 
        /// Slide In Results Screen given waitDuration
        /// </summary>
        public IEnumerator SlideInResultsScreen(float waitDuration)
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
            StartCoroutine(WaitTurnOnButton(slideDuration + 1.2f));
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

        private void OnDisable()
        {
            if (BattleManager.Instance != null)
            {
                BattleManager.Instance.OnWinner.RemoveListener(PopulateResults);
            }
        }

        // PopulateResults called when OnWinningFigure event triggered

        private void PopulateResults(GameObject player, string s, Figure f) {
            foreach (var (p, isWinner) in BattleManager.Instance.pendingPlayerResults) {
                PopulateResultsGivenPlayer(p, isWinner);
            }
            winnerCrownGUI.SetActive(true);
            // Can't figure out how to effectively set position of crown, so position is static
            StartCoroutine(SetCrown());
        }


        // Populate results given player GameObject + isWinner bool
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


        // Slides In ResultsContainer given offset, duration, and wait buffer time
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
            EventSystemSelectHelper.SetSelectedGameObject(resultsScreenFirstButton); // Set new button here
            GetComponentInParent<CanvasGroup>().interactable = true;
        }

        private IEnumerator SetCrown() {
            yield return new WaitForNextFrameUnit();
            winnerCrownGUI.GetComponent<Animator>().enabled = true;
            winnerCrownGUI.GetComponent<GraphicsFaderCanvas>().FadeTurnOn(false);
        }
    }
}