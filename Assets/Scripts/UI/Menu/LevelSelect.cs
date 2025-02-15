using EasyTransition;
using GASHAPWN.UI;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
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
        // Get list of controlsBindBoxes
        [Header("Controls Bind Boxes")]
        public List<ControlsBindBox> controlsBindBoxes;
        private ControlsBindBox currentBindBox = null;
        [SerializeField] private Button toLevelSelectButton;

        [NonSerialized] public bool isControlsBindScreen = true;

        [Header("Transition Settings")]
        [SerializeField] public TransitionSettings menuTransition;
        [SerializeField] private string mainMenuSceneName;

        private InputAction cancelAction;

        [Header("Level Settings")]
        public List<Level> levels;
        public Level selectedLevel;


        private void OnEnable()
        {
            OnGameStateChanged += StateChanged;

            var inputActionAsset = GetComponent<PlayerInput>().actions;
            cancelAction = inputActionAsset["Cancel"];

            cancelAction.performed += HandleCancel;
            if (!cancelAction.enabled) { cancelAction.Enable(); }
        }

        void Start()
        {
            if (controlsBindBoxes != null)
            {
                currentBindBox = controlsBindBoxes[0]; // asumes [0] == P1
                currentBindBox.SetSelected(true);
                toLevelSelectButton.interactable = false;
            }
            else Debug.LogError("controlsBindBoxes list is empty!");

            if (levels.Count != 0 && levels != null)
            {
                // nothing here for now
            }
            else Debug.LogError("levels list is empty!");
        }

        public void OnDisable()
        {
            OnGameStateChanged -= StateChanged;
            cancelAction.performed -= HandleCancel;
            cancelAction.Disable();
        }

        //
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
            }
        }

        private void StateChanged(GameState state)
        {
            Debug.Log(state.ToString());
        }

        public void PureDebug()
        {
            Debug.Log("Button was clicked!");
        }
    }

}



