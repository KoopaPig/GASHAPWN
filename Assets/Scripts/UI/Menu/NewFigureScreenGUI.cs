using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using EasyTransition;
using static UnityEngine.InputSystem.InputAction;
using UnityEngine.InputSystem;
using GASHAPWN.Audio;
using GASHAPWN.Utility;

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
        private Figure winningFigure;
        private string winningPlayerTag;

        [SerializeField] private Light directionalLight;

        [Header("Scene Info")]
        [SerializeField] private string mainMenuSceneName;
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

        public void ToCollection()
        {
           TransitionManager.Instance().Transition(collectionSceneName, fromNewFigureTransition, 0);
           GameManager.Instance.UpdateGameState(GameState.Collection);
        }

        public void ToMainMenu()
        {
            TransitionManager.Instance().Transition(mainMenuSceneName, fromNewFigureTransition, 0);
            GameManager.Instance.UpdateGameState(GameState.Title);
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
                BattleManager.Instance.OnWinner.AddListener(SetWinningFigure);
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
                BattleManager.Instance.OnWinner.RemoveListener(SetWinningFigure);
            }
        }

        // Perform OnNewFigureScreen actions after some delay

        private IEnumerator DelayedOnNewFigureScreen(float waitDuration) 
        {
            yield return new WaitForSeconds(waitDuration);
            figureScreen_BG.SetActive(true);
            figureScreen_BG.GetComponent<GraphicsFaderCanvas>().FadeTurnOn(true);
            figureScreen_FG.SetActive(true);
            StartCoroutine(LerpLightIntensity(directionalLight, 0, 1, 2.5f));
            figureScreen_FG.GetComponentInParent<CanvasGroup>().interactable = false;

            // set up input action here
            submitAction.action.performed += HandleCapsuleOpenInput;
            submitAction.action.Enable();

            // set winningFigure info
            figureName.text = winningFigure.Name;
            starsGUI.SetStars(winningFigure);

            // need to make the model a child of Capsule
            var obj = Instantiate(winningFigure.capsuleModelPrefab, figureModel.transform);
            FigureResizeHelper.ResizeFigureObject(obj, figureModel.transform, 0.3f);

            GetComponent<Animator>().SetBool("isOverlaySlide", true);

            HandleCapsuleModel();

            // Capsule enters frame
            capsuleAnimator.SetBool("isCapsuleOpen", false);
            capsuleAnimator.SetTrigger("capsuleEnter");
            buttonPrompt.GetComponent<GraphicsFaderCanvas>().FadeTurnOn(false);
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
            EventSystemSelectHelper.SetSelectedGameObject(figureScreenFirstButton); // Set new button here
            figureScreen_FG.GetComponentInParent<CanvasGroup>().interactable = true;
        }

        private void SetWinningFigure(GameObject player, string tag, Figure figure)
        {
            winningPlayerTag = tag;
            winningFigure = figure;
        }

        private void HandleCapsuleOpenInput(CallbackContext context)
        {
            if (remainingPresses > 0)
            {
                capsuleAnimator.Play("capsule-shake", 0, 0f);
                UI_SFXManager.Instance.Play_CapsuleShake();
                remainingPresses -= 1;
            } else
            {
                StartCapsuleOpen();
                submitAction.action.performed -= HandleCapsuleOpenInput;
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

        private void HandleCapsuleModel()
        {
            capsule.SetActive(true);

            var metal = capsule.transform.Find("PlayerCapsule/Metal");
            if (metal == null)
            {
                Debug.LogError("NewFigureScreenGUI: Metal object not found in PlayerCapsule.");
                return;
            }

            // Deactivate all inner metal components
            foreach (Transform child in metal)
            {
                child.gameObject.SetActive(false);
            }

            // Activate the matching sphere
            var winnerSphere = metal.Find(winningPlayerTag + "Sphere");
            if (winnerSphere != null)
            {
                winnerSphere.gameObject.SetActive(true);
            }
            else
            {
                Debug.LogWarning($"NewFigureScreenGUI: No matching capsule sphere found for tag: {winningPlayerTag}");
            }
        }

    }
}

