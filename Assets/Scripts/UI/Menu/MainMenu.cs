using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using EasyTransition;
using System;
using static GASHAPWN.GameManager;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace GASHAPWN.UI {
    public class MainMenu : MonoBehaviour
    {
        public static MainMenu Instance;

        public string initSceneName;
        public string playSceneName;
        public GameObject titleScreen; // Get reference to Title Screen object
        public GameObject controlsScreen; // Get reference to Controls Screen object
        public GameObject optionsScreen; // Get refeence to Options Screen object
        public GameObject creditsScreen; // Get reference to Credits Screen object

        private bool isTitleScreen = true;
        private bool isCreditsScreen = false;
        private bool isOptionsScreen = false;
        private bool isControlsScreen = false;


        // Get reference to buttons that open and close submenus
        public GameObject controlsFirstButton, controlsClosedButton, optionsFirstButton, optionsClosedButton, creditsFirstButton, creditsClosedButton;

        //public GameObject optionsFirstButton, optionsClosedButton;
        Animator animator;

        [SerializeField] public TransitionSettings menuTransition;

        // IsMenuTransition bool
        [SerializeField]
        private bool _isMenuTransition = false;
        public bool IsMenuTransition
        {
            get { return _isMenuTransition; }
            private set
            {
                _isMenuTransition = value;
                animator.SetBool(AnimationStrings.isMenuTransition, value);
            }
        }

        private InputAction cancelAction;

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
            OnGameStateChanged += StateChanged;

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
        }

        public void PlayButton()
        {
            TransitionManager.Instance().Transition(playSceneName, menuTransition, 0);
            GameManager.Instance.UpdateGameState(GameState.LevelSelect);
        }

        public void OpenControls()
        {
            if (!IsMenuTransition)
            {
                controlsScreen.SetActive(true);

                // clear selected object
                EventSystem.current.SetSelectedGameObject(null);
                // set new selected object
                EventSystem.current.SetSelectedGameObject(controlsFirstButton);
            }
        }

        public void CloseControls()
        {
            if (!IsMenuTransition)
            {
                controlsScreen.SetActive(false);

                // clear selected object
                EventSystem.current.SetSelectedGameObject(null);
                // set new selected object in one frame
                EventSystem.current.SetSelectedGameObject(controlsClosedButton);
            }
        }

        public void OpenOptions()
        {
            if (!IsMenuTransition)
            {
                optionsScreen.SetActive(true);

                // clear selected object
                EventSystem.current.SetSelectedGameObject(null);
                // set new selected object
                EventSystem.current.SetSelectedGameObject(optionsFirstButton);
            }
        }

        public void CloseOptions()
        {
            if (!IsMenuTransition)
            {
                //DisableNavigationForOptions();
                optionsScreen.SetActive(false);

                // clear selected object
                EventSystem.current.SetSelectedGameObject(null);

                // set new selected object in one frame
                EventSystem.current.SetSelectedGameObject(optionsClosedButton);
            }
        }

        public void OpenCredits()
        {
            if (!IsMenuTransition)
            {
                creditsScreen.SetActive(true);

                // clear selected object
                EventSystem.current.SetSelectedGameObject(null);
                // set new selected object
                //EventSystem.current.SetSelectedGameObject(creditsFirstButton);
            }
        }
        public void CloseCredits()
        {
            if (!IsMenuTransition)
            {
                creditsScreen.SetActive(false);

                // clear selected object
                EventSystem.current.SetSelectedGameObject(null);
                // set new selected object in one frame
                EventSystem.current.SetSelectedGameObject(creditsFirstButton);
                
            }
        }

        public void QuitGame()
        {
            if (!IsMenuTransition)
            {
                Application.Quit();
                Debug.Log("Quit game");
            }
        }

        public void OnDisable()
        {
            OnGameStateChanged -= StateChanged;
            cancelAction.performed -= HandleCancel;
            cancelAction.Disable();
        }
        private void StateChanged(GameState state)
        {
            Debug.Log(state.ToString());
        }

    }

}

