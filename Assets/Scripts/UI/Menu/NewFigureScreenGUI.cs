using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using EasyTransition;
using static UnityEngine.InputSystem.InputAction;
using UnityEngine.InputSystem;

namespace GASHAPWN.UI
{
    public class NewFigureScreenGUI : MonoBehaviour
    {

        [SerializeField] private InputActionReference submitAction;

        [Header("Main Components")]
        [SerializeField] private GameObject figureScreen_BG;
        [SerializeField] private GameObject figureScreen_FG;

        [Header("GUI")]
        [SerializeField] private GameObject figureInfo;
        [SerializeField] private GameObject newIcon;
        [SerializeField] TextMeshProUGUI figureName;
        [SerializeField] private StarsGUI starsGUI;
        [SerializeField] private GameObject figureScreenFirstButton;
        [SerializeField] private GameObject buttonPrompt;

        [SerializeField] private float waitDuration = 3f;
        private int numPresses = 5;
        private int remainingPresses;


        [Header("Particles")]
        [SerializeField] private GameObject backgroundParticles;

        [Header("Objects")]
        [SerializeField] private GameObject figureModel;
        [SerializeField] private GameObject capsule;
        private Animator capsuleAnimator;
        [SerializeField] private Figure winningFigure;

        [SerializeField] private Light directionalLight;

        [Header("Scene Info")]
        [SerializeField] private string levelSelectSceneName;
        [SerializeField] private string collectionSceneName;
        [SerializeField] private TransitionSettings fromNewFigureTransition;

        ///// PUBLIC METHODS /////
        public void StartCapsuleOpen()
        {
            buttonPrompt.GetComponent<GraphicsFaderCanvas>().FadeTurnOff(true);
            capsule.SetActive(true);
            backgroundParticles.SetActive(true);
            figureInfo.SetActive(true);
            capsuleAnimator.SetBool("isCapsuleOpen", true);
            // Handle whether newIcon should appear
            if (BattleManager.Instance.newFigure) { newIcon.SetActive(true); }
            else { newIcon.SetActive(false); }

            // Turn on button after a few seconds
            StartCoroutine(WaitTurnOnButton(2f));
        }

        //public void ToCollection()
        //{
        //    TransitionManager.Instance().Transition(collectionSceneName, menuTransition, 0);
        //    GameManager.Instance.UpdateGameState(GameState.Collection);
        //}

        public void ToLevelSelect()
        {
            TransitionManager.Instance().Transition(levelSelectSceneName, fromNewFigureTransition, 0);
            GameManager.Instance.UpdateGameState(GameState.LevelSelect);
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
            figureInfo.SetActive(false);

            remainingPresses = numPresses;

        }

        private void Start()
        {
            if (BattleManager.Instance != null)
            {
                BattleManager.Instance.ChangeToNewFigure.AddListener(OnNewFigureScreen);
                BattleManager.Instance.OnWinningFigure.AddListener(SetWinningFigure);
            }
        }

        private void OnNewFigureScreen(BattleState state)
        {
            StartCoroutine(DelayedOnNewFigureScreen(waitDuration));
        }

        private void OnDisable()
        {
            if (BattleManager.Instance != null)
            {
                BattleManager.Instance.ChangeToNewFigure.RemoveListener(OnNewFigureScreen);
                BattleManager.Instance.OnWinningFigure.RemoveListener(SetWinningFigure);
            }
            submitAction.action.performed -= HandleCapsuleOpenInput;
        }

        // Perform OnNewFigureScreen actions after some delay

        private IEnumerator DelayedOnNewFigureScreen(float waitDuration) 
        {
            yield return new WaitForSeconds(waitDuration);
            figureScreen_BG.SetActive(true);
            figureScreen_BG.GetComponent<GraphicsFaderCanvas>().FadeTurnOn(true);
            figureScreen_FG.SetActive(true);
            StartCoroutine(LerpLightIntensity(directionalLight, 0, 1, 2.5f));
            figureScreen_FG.GetComponent<GraphicsFaderCanvas>().FadeTurnOn(true);
            figureScreen_FG.GetComponentInParent<CanvasGroup>().interactable = false;

            // set up input action here
            submitAction.action.performed += HandleCapsuleOpenInput;
            submitAction.action.Enable();

            // set winningFigure info
            figureName.text = winningFigure.Name;
            starsGUI.SetStars(winningFigure);

            // need to make the model a child of Capsule
            Instantiate(winningFigure.capsuleModelPrefab, figureModel.transform);

            GetComponent<Animator>().SetBool("isOverlaySlide", true);

            capsule.SetActive(true);
            // Capsule enters frame
            capsuleAnimator.SetBool("isCapsuleOpen", false);
            capsuleAnimator.SetTrigger("capsuleEnter");
            buttonPrompt.GetComponent<GraphicsFaderCanvas>().FadeTurnOn(true);
        }

        // Use as a buffer before activating buttons
        // This should actually be called after Capsule is opened
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

        private void SetWinningFigure(string tag, Figure figure)
        {
            winningFigure = figure;
        }

        private void HandleCapsuleOpenInput(CallbackContext context)
        {
            if (remainingPresses > 0)
            {
                capsuleAnimator.Play("capsule-shake", 0, 0f);
                remainingPresses -= 1;
            } else
            {
                StartCapsuleOpen();
            }
        }

        private IEnumerator LerpLightIntensity(Light light, float startIntensity, float targetIntensity, float duration)
        {
            if (light == null)
            {
                yield break;
            }

            float elapsedTime = 0f;

            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                light.intensity = Mathf.Lerp(startIntensity, targetIntensity, elapsedTime / duration);
                yield return null;
            }

            light.intensity = targetIntensity;
        }
    }
}

