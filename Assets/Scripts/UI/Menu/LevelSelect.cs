using EasyTransition;
using GASHAPWN.Audio;
using GASHAPWN.UI;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using static GASHAPWN.GameManager;
using static UnityEngine.CullingGroup;


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

        [NonSerialized] public bool isControlsBindScreen = true;

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
        [SerializeField] private TMP_Text timeLabel;

        private void OnEnable()
        {
            var inputActionAsset = GetComponent<PlayerInput>().actions;
            cancelAction = inputActionAsset["Cancel"];

            cancelAction.performed += HandleCancel;
            if (!cancelAction.enabled) { cancelAction.Enable(); }
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                return;
            }
            Instance = this;
        }
        void Start()
        {
            if (controlsBindBoxes != null)
            {
                currentBindBox = controlsBindBoxes[0]; // asumes [0] == P1
                currentBindBox.SetSelected(true);
                toLevelSelectButton.interactable = false;
            }
            else Debug.LogError("LevelSelect: controlsBindBoxes list is empty!");

            if (levels.Count != 0 && levels != null)
            {
                // nothing here for now
            }
            else Debug.LogError("LevelSelect: levels list is empty!");

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

        // Checks that all Bind Boxes have detected controls
        public bool IsAllControlsDetected()
        {
            foreach (var i in controlsBindBoxes) {
                if (!i.IsControllerDetected) return false;
            }
            return true;
        }

        /// <summary>
        /// Set whether control is detected given index
        /// </summary>
        /// <param name="index"></param>
        /// <param name="controlScheme"></param>
        /// <param name="value"></param>
        public void SetControllerDetected(int index, ControlScheme controlScheme, bool value)
        {
            if (index < controlsBindBoxes.Count && controlsBindBoxes != null)
            {
                controlsBindBoxes[index].IsControllerDetected = value;
                controlsBindBoxes[index].controlScheme = controlScheme;
                controlsBindBoxes[index].UpdateControls();
                currentBindBox.SetSelected(false);
                currentBindBox = controlsBindBoxes[(index + 1) % controlsBindBoxes.Count];
                currentBindBox.SetSelected(true);
            }
            if (IsAllControlsDetected())
            {
                toLevelSelectButton.interactable = true;
                currentBindBox.SetSelected(false);
            }
        }

        public void SimulateP1ControllerDetectedDebug()
        {
            SetControllerDetected(0, ControlScheme.XINPUT, true);
        }
        public void SimulateP2ControllerDetectedDebug()
        {
            SetControllerDetected(1, ControlScheme.KEYBOARD, true);
        }

        private void HandleCancel(InputAction.CallbackContext context)
        {
            if (isControlsBindScreen) {
                TransitionManager.Instance().Transition(mainMenuSceneName, menuTransition, 0);
                GameManager.Instance.UpdateGameState(GameState.Title);
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
            TransitionManager.Instance().Transition(selectedLevel.levelSceneName, toLevelTransition, 0);
            GameManager.Instance.currentBattleTime = selectedTime;
            GameManager.Instance.currentLevel = selectedLevel;
            GameManager.Instance.UpdateGameState(GameState.Battle);
        }
    }

}



