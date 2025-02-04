using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using EasyTransition;
using System;
using GASHAPWN;
using static GASHAPWN.GameManager;

public class MainMenu : MonoBehaviour
{
    public string initScene;
    public GameObject titleScreen; // Get reference to Title Screen object
    public GameObject controlsScreen; // Get reference to Controls Screen object
    public GameObject optionsScreen; // Get refeence to Options Screen object
    public GameObject creditsScreen; // Get reference to Credits Screen object

    private bool isTitleScreen = true;
    private bool isCreditsScreen = false;
    private bool isOptionsScreen = false;
    private bool isControlsScreen = false;


    // Get reference to buttons that open and close submenus
    public GameObject controlsFirstButton, controlsClosedButton, optionsFirstButton, optionsClosedButton;

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

    private void Awake()
    {
        OnGameStateChanged += StateChanged;
        animator = GetComponent<Animator>();
    }

    public void PressStart()
    {
        IsMenuTransition = true;

    }

    public void StartGame()
    {
        TransitionManager.Instance().Transition("LevelSelect", menuTransition, 0);
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
            // set new selected object
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
            // set new selected object
            //EventSystem.current.SetSelectedGameObject(creditsFirstButton);
        }
    }

    public void CloseOptions()
    {
        if (!IsMenuTransition)
        {
            optionsScreen.SetActive(false);

            // clear selected object
            EventSystem.current.SetSelectedGameObject(null);
            // set new selected object
            EventSystem.current.SetSelectedGameObject(optionsClosedButton);
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

    public void OnDestroy()
    {
        OnGameStateChanged -= StateChanged;
    }
    private void StateChanged(GameState state)
    {
        throw new NotImplementedException();
    }

}
