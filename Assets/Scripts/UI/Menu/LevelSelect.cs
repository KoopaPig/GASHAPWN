using EasyTransition;
using GASHAPWN.Audio;
using GASHAPWN.Utility;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace GASHAPWN.UI {
    /// <summary>
    /// Primary controller for Stage Select UI
    /// </summary>
    [RequireComponent(typeof(ScreenSwitcher))]
    public class LevelSelect : MonoBehaviour
    {
        public static LevelSelect Instance;

        // Get list of controlsBindBoxes
        [Header("Controls Bind Boxes")]
            [Tooltip("List of all ControlsBindBox")]
            public List<ControlsBindBox> controlsBindBoxes;

            // Current selected ControlsBindBox
            private ControlsBindBox currentBindBox = null;

            // Current ControlsBindBox index where input is being listened for
            private int currentListeningIndex = 0;


        [Tooltip("Button to switch to LevelSelect")]
            [SerializeField] private Button toLevelSelectButton;

            // If true, ControlsBindScreen is active
            [NonSerialized] public bool IsControlsBindScreen = true;

        [Header("Level Settings")]
            [Tooltip("List of all selectable levels (should match number of stageButtons)")]
            public List<Level> levels;

        [Tooltip("Currently selected level (defaults to levels[0]")]
        public Level selectedLevel;

        [Header("Battle Time")]
            [Tooltip("Label to display battleTime")]
            [SerializeField] private TextMeshProUGUI timeLabel;

            [Tooltip("List of battle times (in seconds)")]
            public List<float> battleTimes = new List<float>();

            // currently selected battleTime
            private float selectedTime;
            // index of currently selected battleTime
            private int selectedTimeIndex;

        // Reference to "Cancel" InputAction
        private InputAction cancelAction;


        /// PRIVATE METHODS ///

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

        private void Start()
        {
            // TODO: Some form of auto-population and the ability to back out of joining

            if (IsControlsBindScreen)
            {
                PlayerInputAssigner.Instance.playerInputManager.EnableJoining();
                PlayerInputAssigner.Instance.ClearAssignments();
            }

            if (controlsBindBoxes != null && controlsBindBoxes.Count > 0)
            {
                currentBindBox = controlsBindBoxes[0];
                currentBindBox.SetSelected(true);

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

        private void OnDisable()
        {
            if (PlayerInputAssigner.Instance != null)
            {
                PlayerInputAssigner.Instance.playerInputManager.onPlayerJoined -= OnPlayerJoinedGUI;
                PlayerInputAssigner.Instance.playerInputManager.onPlayerLeft -= OnPlayerLeftGUI;
            }
            cancelAction.performed -= HandleCancel;
            cancelAction.Disable();
        }

        /// <summary>
        /// Actions to execute when backing out of LevelSelect or ControlsBindScreen
        /// </summary>
        /// <param name="context"></param>
        private void HandleCancel(InputAction.CallbackContext context)
        {
            if (IsControlsBindScreen)
            {
                TransitionManager.Instance().Transition("MainMenu", 0);
                GameManager.Instance.UpdateGameState(GameState.Title);
                PlayerInputAssigner.Instance.DisableJoining();
                PlayerInputAssigner.Instance.ClearAssignments();
            }
            else
            {
                GetComponent<ScreenSwitcher>().ShowControlsBindScreen();
                UI_SFXManager.Instance.Play_ScreenWoosh();
            }
        }

        /// <summary>
        /// Update the selection states of the ControlsBindBoxes
        /// </summary>
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

        // Update/format timeLabel based on currently selected battleTime
        private void UpdateTimeLabel()
        {
            selectedTime = battleTimes[selectedTimeIndex];
            int minutes = Mathf.FloorToInt(selectedTime / 60);
            int seconds = Mathf.FloorToInt(selectedTime % 60);
            timeLabel.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        }

        /// PUBLIC METHODS ///

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

        /// <summary>
        /// Updates GUI according to dropped players (NOT IMPLEMENTED)
        /// </summary>
        /// <param name="playerInput"></param>
        public void OnPlayerLeftGUI(PlayerInput playerInput)
        {
            // nothing here yet
        }



        /// <summary>
        /// Checks that all ControlsBindBoxes have detected controls
        /// </summary>
        public bool IsAllControlsDetected()
        {
            foreach (var box in controlsBindBoxes) {
                if (!box.IsControllerDetected) return false;
            }
            return true;
        }


        /// <summary>
        /// Set whether control is detected given index.
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


        /// <summary>
        /// Left button pressed actions
        /// </summary>
        public void TimeLeft()
        {
            selectedTimeIndex--;
            if (selectedTimeIndex < 0)
            {
                selectedTimeIndex = battleTimes.Count - 1;
            }        
            UpdateTimeLabel();
        }

        /// <summary>
        /// Right button pressed actions
        /// </summary>
        public void TimeRight()
        {
            selectedTimeIndex = (selectedTimeIndex + 1) % battleTimes.Count;
            UpdateTimeLabel();
        }

        public void GameStart()
        {
            // Only set PlayerInputs persistent before transitioning to battle scene
            PlayerInputAssigner.Instance.SetPlayerInputsPersistent();

            // Transition to level scene
            TransitionManager.Instance().Transition(selectedLevel, 0);

            // Update GameManager global variables
            GameManager.Instance.currentBattleTime = selectedTime;
            GameManager.Instance.currentLevel = selectedLevel;
            GameManager.Instance.UpdateGameState(GameState.Battle);
        }
    }
}