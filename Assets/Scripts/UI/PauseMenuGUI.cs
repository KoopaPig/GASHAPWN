using EasyTransition;
using Febucci.UI;
using GASHAPWN;
using GASHAPWN.UI;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PauseMenuGUI : SlideableScreenGUI
{
    public static PauseMenuGUI Instance;

    // Reference to "Cancel" InputAction
    private InputAction cancelAction;

    private Animator animator;

    [SerializeField] private TextAnimator_TMP pausedPlayerText;


    /// PROTECTED / PRIVATE METHODS ///

    protected override void Awake()
    {
        animator = GetComponent<Animator>();
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }
        Instance = this;

        base.Awake();
    }

    private void OnEnable()
    {
        if (BattleManager.Instance != null)
        {
            BattleManager.Instance.OnGamePaused += PauseGUI;
            BattleManager.Instance.OnGameUnpaused += UnpauseGUI;
        }
    }

    private void HandleCancel(InputAction.CallbackContext context)
    {
        BattleManager.Instance.UnpauseGame();
    }

    private void UnpauseGUI()
    {
        // Close any open confirmation windows
        var windows = FindObjectsByType<ConfirmationWindow>(FindObjectsSortMode.None);
        if (windows != null)
        {
            foreach (var w in windows)
            {
                if (w.gameObject.activeSelf) w.gameObject.SetActive(false);
            }
        }

        SetVisible(false, true, true);
        pausedPlayerText.SetText("");

        cancelAction.performed -= HandleCancel;
        cancelAction.Disable();
    }

    private void PauseGUI(PlayerInput currentPauser)
    {
        SetVisible(true, true, true);

        var inputActionAsset = currentPauser.actions;
        cancelAction = inputActionAsset["Cancel"];

        StartCoroutine(EnableCancelNextFrame());

        pausedPlayerText.SetText("Paused by " + currentPauser.tag);
    }

    private void OnDisable()
    {
        if (BattleManager.Instance != null)
        {
            BattleManager.Instance.OnGamePaused -= PauseGUI;
            BattleManager.Instance.OnGameUnpaused -= UnpauseGUI;
        }
    }

    private IEnumerator EnableCancelNextFrame()
    {
        yield return null; // wait one frame
        cancelAction.performed += HandleCancel;
        if (!cancelAction.enabled) cancelAction.Enable();
    }


    /// PUBLIC METHODS ///

    public void Resume()
    {
        BattleManager.Instance.UnpauseGame();
    }

    // Make sure there is a confirmation window when quitting

    public void ToCollection()
    {
        TransitionManager.Instance().Transition("Collection", 0);
        GameManager.Instance.UpdateGameState(GameState.Collection);
    }

    public void ToLevelSelect()
    {
        TransitionManager.Instance().Transition("LevelSelect", 0);
        GameManager.Instance.UpdateGameState(GameState.LevelSelect);
    }

    public void ToMainMenu()
    {
        BattleManager.Instance.UnpauseGame();
        BattleManager.Instance.BattleEndActions(true);
        TransitionManager.Instance().Transition("MainMenu", 0);
        GameManager.Instance.UpdateGameState(GameState.Title);
    }

}