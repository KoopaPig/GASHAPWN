using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using EasyTransition;

namespace GASHAPWN.UI
{
    public class NewFigureScreenGUI : MonoBehaviour
    {
        // Debug: Toggle this to make New Figure Screen immediately appear
        [SerializeField] private bool debugActive;

        [Header("Main Components")]
        [SerializeField] private GameObject figureScreen_BG;
        [SerializeField] private GameObject figureScreen_FG;
        
        [Header("GUI")]
        [SerializeField] private GameObject newIcon;
        [SerializeField] TextMeshProUGUI figureName;
        [SerializeField] private StarsGUI starsGUI;
        [SerializeField] private GameObject figureScreenFirstButton;

        [Header("Particles")]
        [SerializeField] private GameObject backgroundParticles;

        [Header("Objects")]
        [SerializeField] private GameObject figureModel;
        [SerializeField] private GameObject capsule;
        private Animator capsuleAnimator;
        [SerializeField] private Figure winningFigure;

        [Header("Scene Info")]
        [SerializeField] private string levelSelectSceneName;
        [SerializeField] private string collectionSceneName;
        [SerializeField] private TransitionSettings fromNewFigureTransition;

        ///// PUBLIC METHODS /////
        public void StartCapsuleOpen()
        {
            capsule.SetActive(true);
            backgroundParticles.SetActive(true);
            capsuleAnimator.SetBool("isCapsuleEnter", false);
            capsuleAnimator.SetBool("isCapsuleOpen", true);
            StartCoroutine(ResetCapsuleBool());
            // Handle whether newIcon should appear
            if (BattleManager.Instance.newFigure) { newIcon.SetActive(true); }
            else { newIcon.SetActive(false); }
        }

        public void OnCapsuleOpenAnimationComplete()
        {
            capsuleAnimator.SetBool("isCapsuleOpen", false);
        }

        ///// PRIVATE METHODS /////

        private void Awake()
        {
            capsuleAnimator = capsule.GetComponent<Animator>();

            if (!figureModel.transform.IsChildOf(capsule.transform))
            {
                Debug.LogError("NewFigureScreenGUI: FigureModel must be child of Capsule");
            }

            capsule.SetActive(false);
            backgroundParticles.SetActive(false);
            figureScreen_BG.SetActive(false);
            figureScreen_FG.SetActive(false);

            if (debugActive) { OnNewFigureScreen(BattleState.NewFigureScreen); }
        }

        private void Start()
        {
            BattleManager.Instance.ChangeToNewFigure.AddListener(OnNewFigureScreen);
            BattleManager.Instance.OnWinningFigure.AddListener(SetWinningFigure);
        }

        private void OnNewFigureScreen(BattleState state)
        {
            figureScreen_BG.SetActive(true);
            figureScreen_BG.GetComponent<GraphicsFaderCanvas>().FadeTurnOn();
            figureScreen_FG.SetActive(true);
            figureScreen_FG.GetComponent<GraphicsFaderCanvas>().FadeTurnOn();

            // TODO: Get winning figure from BattleManager
            figureName.text = winningFigure.Name;
            starsGUI.SetStars(winningFigure);

            // need to make the model a child of Capsule
            Instantiate(winningFigure.capsuleModelPrefab, figureModel.transform);

            GetComponent<Animator>().SetBool("isOverlaySlide", true);
            StartCoroutine(WaitTurnOnButton(4f));

            capsule.SetActive(true);
            // Capsule enters frame
            capsuleAnimator.SetBool("isCapsuleEnter", true);
        }

        private void OnDisable()
        {
            BattleManager.Instance.ChangeToNewFigure.RemoveListener(OnNewFigureScreen);
            BattleManager.Instance.OnWinningFigure.RemoveListener(SetWinningFigure);
        }

        // Use as a buffer before activating buttons
        private IEnumerator WaitTurnOnButton(float waitDuration)
        {
            if (figureScreen_FG.GetComponentInParent<CanvasGroup>() == null)
            {
                Debug.LogError("VictoryScreenGUI: CanvasGroup is missing.");
                yield break;
            } else figureScreen_FG.GetComponentInParent<CanvasGroup>().interactable = false;

            yield return new WaitForSeconds(waitDuration);
            EventSystem.current.SetSelectedGameObject(figureScreenFirstButton); // Set new button here
            figureScreen_FG.GetComponentInParent<CanvasGroup>().interactable = true;
        }

        // Resets Capsule After "isCapsuleOpen" animation
        private IEnumerator ResetCapsuleBool()
        {
            yield return new WaitForEndOfFrame();
            yield return new WaitUntil(() => capsuleAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime > 0);

            float animLength = capsuleAnimator.GetCurrentAnimatorStateInfo(0).length;
            yield return new WaitForSeconds(animLength); // Wait for animation to complete
            // Reset "isCapsuleOpen"
            // Note: for capsule opening animation to play again, "isCapsuleEnter" must be set to true.
            capsuleAnimator.SetBool("isCapsuleOpen", false);

        }

        private void SetWinningFigure(string tag, Figure figure)
        {
            winningFigure = figure;
        }
    }
}

