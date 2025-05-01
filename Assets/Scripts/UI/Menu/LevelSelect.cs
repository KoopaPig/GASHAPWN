using EasyTransition;
using GASHAPWN.Audio;
using GASHAPWN.UI;
using GASHAPWN.Utility;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace GASHAPWN.UI {
    [RequireComponent(typeof(ScreenSwitcher))]
    public class LevelSelect : MonoBehaviour
    {
        public static LevelSelect Instance;

        // Get list of controlsBindBoxes
        [Header("Controls Bind Boxes")]
        public List<ControlsBindBox> controlsBindBoxes;
        private ControlsBindBox currentBindBox = null;
        [SerializeField] private Button toLevelSelectButton;
        //[SerializeField] private InputActionAsset inputActions;

        [NonSerialized] public bool isControlsBindScreen = true;
        private bool isListeningForInput = false;
        private int currentListeningIndex = 0;

        [Header("Transition Settings")]
        [SerializeField] public TransitionSettings menuTransition;
        [SerializeField] public TransitionSettings toLevelTransition;
        [SerializeField] private string mainMenuSceneName; // name of mainMenu scene

        private InputAction cancelAction;

        [Header("Level Settings")]
        // List of all selectable levels (should match number of stageButtons)
        public List<Level> levels;
        // currently selected level (defaults to levels[0])
        public Level selectedLevel;

        [Header("Battle Time")]
        // list of battle times (in seconds)
        public List<float> battleTimes = new List<float>();
        private float selectedTime;
        private int selectedTimeIndex;
        [SerializeField] private TMPro.TMP_Text timeLabel;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                return;
            }
            Instance = this;

            // Set indices for controls bind boxes
            for (int i = 0; i < controlsBindBoxes.Count; i++)
            {
                controlsBindBoxes[i].playerIndex = i;
            }
        }

        private void OnEnable()
        {
            if (PlayerInputAssigner.Instance != null)
            {
                PlayerInputAssigner.Instance.playerInputManager.onPlayerJoined += OnPlayerJoinedGUI;
                PlayerInputAssigner.Instance.playerInputManager.onPlayerLeft += OnPlayerLeftGUI;
            }
            //else StartCoroutine(WaitForPlayerInputAssigner());

            var inputActionAsset = GetComponent<PlayerInput>().actions;
            cancelAction = inputActionAsset["Cancel"];

            cancelAction.performed += HandleCancel;
            if (!cancelAction.enabled) { cancelAction.Enable(); }
        }

        private IEnumerator WaitForPlayerInputAssigner()
        {
            while (PlayerInputAssigner.Instance == null)
            {
                yield return null;
            }

            PlayerInputAssigner.Instance.playerInputManager.onPlayerJoined += OnPlayerJoinedGUI;
            PlayerInputAssigner.Instance.playerInputManager.onPlayerLeft += OnPlayerLeftGUI;
        }

        /// <summary>
        /// Updates GUI according to joined players
        /// </summary>
        /// <param name="playerInput"></param>
        public void OnPlayerJoinedGUI(PlayerInput playerInput)
        {
            if (currentListeningIndex >= controlsBindBoxes.Count)
            {
                Debug.LogWarning("LevelSelect: All player slots filled.");
                return;
            }

            var bindBox = controlsBindBoxes[currentListeningIndex];
            currentBindBox = bindBox;

            // Assign control scheme for UI
            ControlScheme scheme = playerInput.currentControlScheme switch
            {
                "Gamepad" => ControlScheme.XINPUT,
                "KeyboardMouse" or "Keyboard" => ControlScheme.KEYBOARD,
                _ => ControlScheme.KEYBOARD
            };

            // Let currentBindBox know that a detection occurred
            SetControllerDetectedGUI(currentListeningIndex, scheme, true);
            currentListeningIndex++;

            currentBindBox.SetSelected(false);
            if (currentListeningIndex < controlsBindBoxes.Count)
            {
                controlsBindBoxes[currentListeningIndex].SetSelected(true);
            }

            // Disable joining once all bindBoxes are filled
            if (currentListeningIndex >= controlsBindBoxes.Count)
            {
                PlayerInputManager.instance.DisableJoining();
            }
        }

        public void OnPlayerLeftGUI(PlayerInput playerInput)
        {
            // nothing here yet
        }


        void Start()
        {
            if (isControlsBindScreen) { 
                PlayerInputAssigner.Instance.playerInputManager.EnableJoining(); 
                PlayerInputAssigner.Instance.ClearAssignments();
            }

            if (controlsBindBoxes != null && controlsBindBoxes.Count > 0)
            {
                currentBindBox = controlsBindBoxes[0];
                currentBindBox.SetSelected(true);


                // TODO: I stil want some form of auto-population and the ability to back out of joining

                // make sure to set the currentBindBox to whatever index is not assigned
                // this prevents the UI from breaking when assigning one controller,
                // backing out, coming back, and assigning another
                //int index = PlayerInputAssigner.Instance.playerAssignments.Count;
                //currentBindBox = controlsBindBoxes[index]; // assumes [0] == P1

                //currentBindBox.SetSelected(true);

                // Refresh UI according to previously assigned controllers
                //StartCoroutine(RefreshControllerUI());

                // Enable the next screen button only if all controllers are assigned
                toLevelSelectButton.interactable = IsAllControlsDetected();
            }
            else Debug.LogError("LevelSelect: controlsBindBoxes list is empty!");

            if (levels.Count == 0 || levels == null)
            {
                Debug.LogError("LevelSelect: levels list is empty!");
            }

            if (battleTimes.Count > 0)
            {
                // automatically set to whatever time is at index 0
                selectedTimeIndex = 0;
                UpdateTimeLabel();
            }
            else Debug.LogError("battleTimes in LevelSelect are not populated.");
        }

        public void OnDisable()
        {
            if (PlayerInputAssigner.Instance != null)
            {
                PlayerInputAssigner.Instance.playerInputManager.onPlayerJoined -= OnPlayerJoinedGUI;
                PlayerInputAssigner.Instance.playerInputManager.onPlayerLeft -= OnPlayerLeftGUI;
            }
            cancelAction.performed -= HandleCancel;
            cancelAction.Disable();
        }

        private IEnumerator RefreshControllerUI()
        {
            yield return null;
            // If controllers already assigned, UI should be updated
            //if (controlsBindBoxes != null && controlsBindBoxes.Count != 0) {
            //    for (int i = 0; i < controlsBindBoxes.Count; i++)
            //    {
            //        string playerTag = $"Player{i+1}";
            //        bool isAssigned = ControllerManager.Instance.IsPlayerAssigned(playerTag);
            //        ControlScheme scheme = ControllerManager.Instance.GetPlayerControlScheme(playerTag);

            //        SetControllerDetectedGUI(i, scheme, isAssigned);
            //    }
            //}
        }

        private void UpdateSelectedBindBox()
        {
            if (currentBindBox != null)
                currentBindBox.SetSelected(false);

            // Find the first unassigned box
            for (int i = 0; i < controlsBindBoxes.Count; i++)
            {
                if (!controlsBindBoxes[i].IsControllerDetected)
                {
                    currentBindBox = controlsBindBoxes[i];
                    currentBindBox.SetSelected(true);
                    return;
                }
            }

            // If all are assigned, don't highlight any box
            currentBindBox = null;
        }

        // Checks that all Bind Boxes have detected controls
        public bool IsAllControlsDetected()
        {
            foreach (var box in controlsBindBoxes) {
                if (!box.IsControllerDetected) return false;
            }
            return true;
        }

        public void StartListeningForController(int playerIndex)
        {
            if (playerIndex >= controlsBindBoxes.Count || isListeningForInput)
            {
                return;
            }

            isListeningForInput = true;
            currentListeningIndex = playerIndex;
            
            string playerTag = $"Player{playerIndex+1}";
            controlsBindBoxes[playerIndex].feedbackText.text = "Press any button...";

            // Start listening for any button press


            //StartCoroutine(ListenForInput(playerTag));
        }

        private IEnumerator DelayedUpdateSelectedBindBox()
        {
            yield return null; // wait one frame
            UpdateSelectedBindBox();
        }

        private IEnumerator WaitToActivateContinueButton()
        {
            yield return null;
            yield return null;
            toLevelSelectButton.interactable = true;
            EventSystemSelectHelper.SetSelectedGameObject(toLevelSelectButton.gameObject);
        }

        //private IEnumerator ListenForInput(string playerTag)
        //{
        //    // The issue with not being able to do keyboard then controller is happening in here
        //    // Has something to do with inputDetected and seeming to always break out becaue


        //    // Wait a small delay to avoid detecting the click
        //    yield return new WaitForSeconds(0.1f);

        //    bool inputDetected = false;
            
        //    // First, try to find a gamepad/controller
        //    while (!inputDetected)
        //    {
        //        // Check for gamepad button presses first (prioritize controllers)
        //        foreach (Gamepad gamepad in ControllerManager.Instance.GetAvailableGamepads())
        //        {
        //            if (ControllerManager.Instance.IsAnyButtonPressed(gamepad))
        //            {
        //                // Assign this gamepad to the player
        //                bool success = ControllerManager.Instance.AssignControllerToPlayer(playerTag, gamepad);
        //                if (success)
        //                {
        //                    SetControllerDetectedGUI(currentListeningIndex, ControlScheme.XINPUT, true);
        //                    inputDetected = true;
        //                    break;
        //                }
        //            }
        //        }

        //        // If we found a controller, break out
        //        if (inputDetected)
        //            break;

        //        // If no controller detected, check keyboard
        //        foreach (InputDevice keyboard in ControllerManager.Instance.GetAvailableKeyboards())
        //        {
        //            if (ControllerManager.Instance.IsAnyButtonPressed(keyboard))
        //            {
        //                // Assign keyboard to this player
        //                bool success = ControllerManager.Instance.AssignControllerToPlayer(playerTag, keyboard);
        //                if (success)
        //                {
        //                    SetControllerDetectedGUI(currentListeningIndex, ControlScheme.KEYBOARD, true);
        //                    inputDetected = true;
        //                    break;
        //                }
        //            }
        //        }
                
        //        yield return null;
        //    }

        //    isListeningForInput = false;
        //    currentListeningIndex = -1;
        //}

        /// <summary>
        /// Set whether control is detected given index
        /// Also updates currentBindBox after a delay to eliminate race condition
        /// </summary>
        public void SetControllerDetectedGUI(int index, ControlScheme controlScheme, bool value)
        {
            if (index < controlsBindBoxes.Count && controlsBindBoxes != null)
            {
                controlsBindBoxes[index].IsControllerDetected = value;
                controlsBindBoxes[index].controlScheme = controlScheme;
                controlsBindBoxes[index].UpdateControllerIcon();

                // Move to the next unassigned controller
                // Have to delay a frame to eliminate race condition
                StartCoroutine(DelayedUpdateSelectedBindBox());
            }

            // Activate continue button after two frames to eliminate race condition
            if (IsAllControlsDetected()) StartCoroutine(WaitToActivateContinueButton());
        }

        private void HandleCancel(InputAction.CallbackContext context)
        {
            if (isControlsBindScreen) {
                TransitionManager.Instance().Transition(mainMenuSceneName, menuTransition, 0);
                GameManager.Instance.UpdateGameState(GameState.Title);
                PlayerInputAssigner.Instance.DisableJoining();
                PlayerInputAssigner.Instance.ClearAssignments();
            } else
            {
                GetComponent<ScreenSwitcher>().ShowControlsBindScreen();
                UI_SFXManager.Instance.Play_ScreenWoosh();
            }
        }

        public void TimeLeft()
        {
            selectedTimeIndex--;
            if (selectedTimeIndex < 0)
            {
                selectedTimeIndex = battleTimes.Count - 1;
            }        
            UpdateTimeLabel();
        }

        public void TimeRight()
        {
            selectedTimeIndex = (selectedTimeIndex + 1) % battleTimes.Count;
            UpdateTimeLabel();
        }

        public void UpdateTimeLabel()
        {
            selectedTime = battleTimes[selectedTimeIndex];
            int minutes = Mathf.FloorToInt(selectedTime / 60);
            int seconds = Mathf.FloorToInt(selectedTime % 60);
            timeLabel.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        }

        public void GameStart()
        {
            // Only set PlayerInputs persistent before transitioning to battle scene
            PlayerInputAssigner.Instance.SetPlayerInputsPersistent();

            TransitionManager.Instance().Transition(selectedLevel.levelSceneName, toLevelTransition, 0);
            GameManager.Instance.currentBattleTime = selectedTime;
            GameManager.Instance.currentLevel = selectedLevel;
            GameManager.Instance.UpdateGameState(GameState.Battle);
        }
    }
}