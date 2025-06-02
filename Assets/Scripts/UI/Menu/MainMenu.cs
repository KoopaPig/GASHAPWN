using UnityEngine;
using EasyTransition;
using UnityEngine.InputSystem;
using GASHAPWN.Audio;
using GASHAPWN.Utility;

namespace GASHAPWN.UI {
    public class MainMenu : MonoBehaviour
    {
        public static MainMenu Instance;

        [Header("Main Screens")]
            [Tooltip("Title Screen Object")]
            public GameObject titleScreen;
            [Tooltip("Controls Screen Object")]
            public GameObject controlsScreen;
            [Tooltip("Options Screen Object")]
            public GameObject optionsScreen;
            [Tooltip("Credits Screen Object")]
            public GameObject creditsScreen;

        [Header("First Objects to Select")]
            [Tooltip("First button to select on Controls Screen")]
            public GameObject controlsFirstButton;
            [Tooltip("First button to select when Controls Screen closed")]
            public GameObject controlsClosedButton;
            [Tooltip("First button to select on Options Screen")]
            public GameObject optionsFirstButton;
            [Tooltip("First button to select when Options Screen closed")]
            public GameObject optionsClosedButton;
            [Tooltip("First object to select on Credits Screen")]
            public GameObject creditsFirstObject;
            [Tooltip("First button to select when Credits Screen closed")]
            public GameObject creditsClosedButton;


        // IsMenuTransition bool
        // CURRENTLY NO TRANSITION IS IMPLEMENTED
        public bool IsMenuTransition
        {
            get { return _isMenuTransition; }
            private set
            {
                _isMenuTransition = value;
                // Set animation bool upon Menu Transition
                animator.SetBool(AnimationStrings.isMenuTransition, value);
            }
        }

        private bool _isMenuTransition = false;

        // Reference to "Cancel" InputAction
        private InputAction cancelAction;

        private Animator animator;


        /// PRIVATE METHODS ///

        private void Awake()
        {
            animator = GetComponent<Animator>();
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                return;
            }
            Instance = this;
        }

        private void OnEnable()
        {
            var inputActionAsset = GetComponent<PlayerInput>().actions;
            cancelAction = inputActionAsset["Cancel"];

            cancelAction.performed += HandleCancel;
            if (!cancelAction.enabled) { cancelAction.Enable(); }
        }

        private void HandleCancel(InputAction.CallbackContext context)
        {
            if (optionsScreen.activeSelf)
            {
                CloseOptions();
            }
            else if (controlsScreen.activeSelf)
            {
                CloseControls();
            }
            else if (creditsScreen.activeSelf)
            {
                CloseCredits();
            }
            // Close any open confirmation windows
            var windows = FindObjectsByType<ConfirmationWindow>(FindObjectsSortMode.None);
            if (windows != null)
            {
                foreach (var w in windows)
                {
                    if (w.gameObject.activeSelf) w.gameObject.SetActive(false);
                }
            }
        }

        private void OnDisable()
        {
            cancelAction.performed -= HandleCancel;
            cancelAction.Disable();
        }


        /// PUBLIC METHODS ///

        public void PlayButton()
        {
            UI_SFXManager.Instance.Play_PlayButton();
            TransitionManager.Instance().Transition("LevelSelect", 0);
            GameManager.Instance.UpdateGameState(GameState.LevelSelect);
        }

        public void ToCollection()
        {
            TransitionManager.Instance().Transition("Collection", 0);
            GameManager.Instance.UpdateGameState(GameState.Collection);
        }

        public void OpenControls()
        {
            if (!IsMenuTransition)
            {
                controlsScreen.SetActive(true);
                EventSystemSelectHelper.SetSelectedGameObject(controlsFirstButton);
            }
        }

        public void CloseControls()
        {
            if (!IsMenuTransition)
            {
                controlsScreen.SetActive(false);
                EventSystemSelectHelper.SetSelectedGameObject(controlsClosedButton);
            }
        }

        public void OpenOptions()
        {
            if (!IsMenuTransition)
            {
                optionsScreen.SetActive(true);
                EventSystemSelectHelper.SetSelectedGameObject(optionsFirstButton);
            }
        }

        public void CloseOptions()
        {
            if (!IsMenuTransition)
            {
                optionsScreen.SetActive(false);
                EventSystemSelectHelper.SetSelectedGameObject(optionsClosedButton);
            }
        }

        public void OpenCredits()
        {
            if (!IsMenuTransition)
            {
                creditsScreen.SetActive(true);
                EventSystemSelectHelper.SetSelectedGameObject(creditsFirstObject);
            }
        }

        public void CloseCredits()
        {
            if (!IsMenuTransition)
            {
                creditsScreen.SetActive(false);
                EventSystemSelectHelper.SetSelectedGameObject(creditsClosedButton);
            }
        }

        public void QuitGame()
        {
            if (!IsMenuTransition)
            {
                #if UNITY_EDITOR
                    UnityEditor.EditorApplication.isPlaying = false;
                #endif
                Application.Quit();
                Debug.Log("Quit game");
            }
        }
    }
}