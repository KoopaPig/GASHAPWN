using EasyTransition;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
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
        [SerializeField] private InputActionAsset inputActions;

        [NonSerialized] public bool isControlsBindScreen = true;
        private bool isListeningForInput = false;
        private int currentListeningIndex = -1;

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

        private void OnEnable()
        {
            var inputActionAsset = GetComponent<PlayerInput>().actions;
            cancelAction = inputActionAsset["Cancel"];

            cancelAction.performed += HandleCancel;
            if (!cancelAction.enabled) { cancelAction.Enable(); }

            // Ensure controller manager has input actions
            if (inputActions != null)
            {
                ControllerManager.Instance.SetInputActions(inputActions);
            }

            // Refresh UI to match controller state
            RefreshControllerUI();
        }

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

        void Start()
        {
            if (controlsBindBoxes != null && controlsBindBoxes.Count > 0)
            {
                currentBindBox = controlsBindBoxes[0]; // assumes [0] == P1
                currentBindBox.SetSelected(true);
                
                // Check if controllers are already assigned from a previous scene
                RefreshControllerUI();
                
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
            cancelAction.performed -= HandleCancel;
            cancelAction.Disable();
        }

        public void RefreshControllerUI()
        {
            // Check if ControllerManager exists (it will auto-create if it doesn't)
            if (controlsBindBoxes == null || controlsBindBoxes.Count == 0)
                return;

            for (int i = 0; i < controlsBindBoxes.Count; i++)
            {
                string playerTag = $"Player{i+1}";
                bool isAssigned = ControllerManager.Instance.IsPlayerAssigned(playerTag);
                ControlScheme scheme = ControllerManager.Instance.GetPlayerControlScheme(playerTag);

                controlsBindBoxes[i].IsControllerDetected = isAssigned;
                controlsBindBoxes[i].controlScheme = scheme;
                controlsBindBoxes[i].UpdateControls();
            }

            // Highlight the first unassigned controller box
            UpdateSelectedBindBox();
            
            // Update the continue button
            toLevelSelectButton.interactable = IsAllControlsDetected();
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
                return;

            isListeningForInput = true;
            currentListeningIndex = playerIndex;
            
            string playerTag = $"Player{playerIndex+1}";
            controlsBindBoxes[playerIndex].feedbackText.text = "Press any button...";

            // Start listening for any button press
            StartCoroutine(ListenForInput(playerTag));
        }

        private IEnumerator ListenForInput(string playerTag)
        {
            // Wait a small delay to avoid detecting the click
            yield return new WaitForSeconds(0.1f);

            bool inputDetected = false;
            
            // First, try to find a gamepad/controller
            while (!inputDetected)
            {
                // Check for gamepad button presses first (prioritize controllers)
                foreach (Gamepad gamepad in ControllerManager.Instance.GetAvailableGamepads())
                {
                    if (ControllerManager.Instance.IsAnyButtonPressed(gamepad))
                    {
                        // Assign this gamepad to the player
                        bool success = ControllerManager.Instance.AssignControllerToPlayer(playerTag, gamepad);
                        if (success)
                        {
                            SetControllerDetected(currentListeningIndex, ControlScheme.XINPUT, true);
                            inputDetected = true;
                            break;
                        }
                    }
                }
                
                // If we found a controller, break out
                if (inputDetected)
                    break;
                
                // If no controller detected, check keyboard
                foreach (InputDevice keyboard in ControllerManager.Instance.GetAvailableKeyboards())
                {
                    if (ControllerManager.Instance.IsAnyButtonPressed(keyboard))
                    {
                        // Assign keyboard to this player
                        bool success = ControllerManager.Instance.AssignControllerToPlayer(playerTag, keyboard);
                        if (success)
                        {
                            SetControllerDetected(currentListeningIndex, ControlScheme.KEYBOARD, true);
                            inputDetected = true;
                            break;
                        }
                    }
                }
                
                yield return null;
            }

            isListeningForInput = false;
            currentListeningIndex = -1;
        }

        /// <summary>
        /// Set whether control is detected given index
        /// </summary>
        public void SetControllerDetected(int index, ControlScheme controlScheme, bool value)
        {
            if (index < controlsBindBoxes.Count && controlsBindBoxes != null)
            {
                controlsBindBoxes[index].IsControllerDetected = value;
                controlsBindBoxes[index].controlScheme = controlScheme;
                controlsBindBoxes[index].UpdateControls();
                
                if (currentBindBox != null)
                    currentBindBox.SetSelected(false);
                
                // Move to the next unassigned controller
                UpdateSelectedBindBox();
            }
            
            if (IsAllControlsDetected())
            {
                toLevelSelectButton.interactable = true;
                if (currentBindBox != null)
                    currentBindBox.SetSelected(false);
            }
        }

        private void HandleCancel(InputAction.CallbackContext context)
        {
            if (isControlsBindScreen) {
                TransitionManager.Instance().Transition(mainMenuSceneName, menuTransition, 0);
                GameManager.Instance.UpdateGameState(GameState.Title);
            } else
            {
                GetComponent<ScreenSwitcher>().ShowControlsBindScreen();
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
            TransitionManager.Instance().Transition(selectedLevel.levelSceneName, toLevelTransition, 0);
            GameManager.Instance.currentBattleTime = selectedTime;
            GameManager.Instance.currentLevel = selectedLevel;
            GameManager.Instance.UpdateGameState(GameState.Battle);
        }
    }
}