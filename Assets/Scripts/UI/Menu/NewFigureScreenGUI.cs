using System.Collections;
using TMPro;
using UnityEngine;
using EasyTransition;
using static UnityEngine.InputSystem.InputAction;
using UnityEngine.InputSystem;
using GASHAPWN.Audio;
using GASHAPWN.Utility;

namespace GASHAPWN.UI
{
    public class NewFigureScreenGUI : MonoBehaviour
    {
        [Tooltip("Reference to \"Submit\" action")]
        [SerializeField] private InputActionReference submitAction;

        [Tooltip("First button to select on New Figure Screen")]
        [SerializeField] private GameObject figureScreenFirstButton;

        [Tooltip("Seconds it takes for NewFigureScreen to appear after BattleManager.OnNewFigureScreen is triggered")]
        [SerializeField] private float waitDuration = 3f;

        [Header("Main Components")]
            [Tooltip("Background GameObject of NewFigureScreen")]
            [SerializeField] private GameObject figureScreen_BG;
            [Tooltip("Foreground GameObject of NewFigureScreen")]
            [SerializeField] private GameObject figureScreen_FG;

        [Header("GUI")]
            [Tooltip("Parent GameObject that holds Figure GUI elements")]
            [SerializeField] private GameObject newFigureInfo;
            [Tooltip("\"New\" icon (appears when new figure)")]
            [SerializeField] private GameObject newIcon;
            [Tooltip("Figure name region")]
            [SerializeField] TextMeshProUGUI figureName;
            [Tooltip("Rarity stars for Figure")]
            [SerializeField] private StarsGUI starsGUI;
            [Tooltip("Prompt that appears on New Figure Screen")]
            [SerializeField] private GameObject buttonPrompt;

        [Header("Particles")]
            [Tooltip("Particles that appear in background when capsule is opened")]
            [SerializeField] private GameObject backgroundParticles;

        [Header("Objects")]
            [SerializeField] private PlayerAttachedFigure attachedFigure;
            [Tooltip("Container of all capsule-opening elements")]
            [SerializeField] private GameObject capsuleContainer;
            [Tooltip("Light that fades in on New Figure Screen")]
            [SerializeField] private Light directionalLight;

        // Number of presses to open capsule
        private int numPresses = 5;
        // Track remaining presses
        private int remainingPresses;

        // Animator of capsule to open
        private Animator _capsuleAnimator;
        // Animator of new figure UI
        private Animator _newFigureAnimator;
        // Reference to winning player's figure
        private Figure winningFigure;
        // Reference to winning player's tag
        private string winningPlayerTag;


        ///// PUBLIC METHODS /////
        
        /// <summary>
        /// Actions to execute when capsule opens
        /// </summary>
        public void StartCapsuleOpen()
        {
            buttonPrompt.GetComponent<GraphicsFaderCanvas>().FadeTurnOff(true);
            capsuleContainer.SetActive(true);
            backgroundParticles.SetActive(true);
            newFigureInfo.SetActive(true);
            _capsuleAnimator.SetBool("isCapsuleOpen", true);
            _newFigureAnimator.SetBool("isCapsuleOpen", true);

            // Handle whether newIcon should appear
            if (BattleManager.Instance.newFigure) { newIcon.SetActive(true); }
            else { newIcon.SetActive(false); }

            // Turn on button after a few seconds
            StartCoroutine(WaitTurnOnButton(2f));
        }

        public void ToCollection()
        {
           TransitionManager.Instance().Transition("Collection", 0);
           GameManager.Instance.UpdateGameState(GameState.Collection);
        }

        public void ToMainMenu()
        {
            TransitionManager.Instance().Transition("MainMenu", 0);
            GameManager.Instance.UpdateGameState(GameState.Title);
        }


        ///// PRIVATE METHODS /////

        private void Awake()
        {
            _capsuleAnimator = capsuleContainer.GetComponent<Animator>();
            _newFigureAnimator = newFigureInfo.GetComponent<Animator>();
            directionalLight.gameObject.SetActive(false);

            capsuleContainer.SetActive(false);
            backgroundParticles.SetActive(false);
            figureScreen_BG.SetActive(false);
            figureScreen_FG.SetActive(false);
            newFigureInfo.SetActive(false);

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

        private void OnDisable()
        {
            if (BattleManager.Instance != null)
            {
                BattleManager.Instance.ChangeToNewFigure.RemoveListener(OnNewFigureScreen);
                BattleManager.Instance.OnWinner.RemoveListener(SetWinningFigure);
            }
        }

        private void OnNewFigureScreen(BattleState state)
        {
            StartCoroutine(DelayedOnNewFigureScreen(waitDuration));
        }

        // Perform OnNewFigureScreen actions after some delay
        private IEnumerator DelayedOnNewFigureScreen(float waitDuration) 
        {
            yield return new WaitForSeconds(waitDuration);

            figureScreen_BG.SetActive(true);
            figureScreen_BG.GetComponent<GraphicsFaderCanvas>().FadeTurnOn(true);

            figureScreen_FG.SetActive(true);
            figureScreen_FG.GetComponentInParent<CanvasGroup>().interactable = false;

            // Fade in directionalLight
            StartCoroutine(LerpLightIntensity(directionalLight, 0, 0.7f, 2.5f));

            // set up input action here
            submitAction.action.performed += HandleCapsuleOpenInput;
            submitAction.action.Enable();

            // set winningFigure info
            figureName.text = winningFigure.Name;
            starsGUI.SetStars(winningFigure);

            // need to make the model a child of Capsule
            attachedFigure.SetFigureInCapsule(winningFigure);

            // Start sliding in graphics
            GetComponent<Animator>().SetBool("isOverlaySlide", true);

            // Modify capsule model according to winning player
            HandleCapsuleModel();

            // Capsule enters frame
            _capsuleAnimator.SetBool("isCapsuleOpen", false);
            _capsuleAnimator.SetTrigger("capsuleEnter");
            buttonPrompt.GetComponent<GraphicsFaderCanvas>().FadeTurnOn(false);
        }

        // Used as a buffer before activating buttons, triggered when capsule is opened
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

        // Handle the submitAction input and opening capsule
        private void HandleCapsuleOpenInput(CallbackContext context)
        {
            if (remainingPresses > 0)
            {
                _capsuleAnimator.Play("capsule-shake", 0, 0f);
                UI_SFXManager.Instance.Play_CapsuleShake();
                remainingPresses -= 1;
            } else
            {
                StartCapsuleOpen();
                submitAction.action.performed -= HandleCapsuleOpenInput;
            }
        }

        // Set correct metal component of capsule model that corresponds to winning player
        private void HandleCapsuleModel()
        {
            capsuleContainer.SetActive(true);

            var metal = capsuleContainer.transform.Find("PlayerCapsule/CapsuleModel/Metal");
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

        private IEnumerator LerpLightIntensity(Light light, float startIntensity, float targetIntensity, float duration)
        {
            if (light == null)
            {
                yield break;
            }
            light.gameObject.SetActive(true);

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