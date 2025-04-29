using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;
using System.Collections;
using System.Collections.Generic;
using static GASHAPWN.ControllerManager;
using UnityEngine.InputSystem.UI;

namespace GASHAPWN
{
    public class BattleInitializer : MonoBehaviour
    {
        [SerializeField] private GameObject player1Object;
        [SerializeField] private GameObject player2Object;
        [SerializeField] private InputActionAsset inputActions;

        // Force-set specific control schemes (useful for debugging)
        [Header("Debug Options")]
        [SerializeField] private bool useDebugControllers = false;
        [SerializeField] private string player1ControlScheme = "Gamepad";
        [SerializeField] private string player2ControlScheme = "Keyboard";

        private void Start()
        {
            // Set input actions in the ControllerManager if provided
            if (inputActions != null)
            {
                ControllerManager.Instance.SetInputActions(inputActions);
            }

            // Wait a frame to ensure everything is initialized
            StartCoroutine(DelayedSetup());
        }

        private IEnumerator DelayedSetup()
        {
            // Wait for a couple of frames to ensure everything is initialized
            yield return null;
            yield return null;

            if (useDebugControllers)
            {
                Debug.Log("Using debug controller assignments");
                SetupPlayersWithDebugControlSchemes();
            }
            else
            {
                Debug.Log("BattleInitializer: Setting up player controls from controller assignments...");
                SetupPlayersWithAssignedControllers();
            }
        }

        private void SetupPlayersWithAssignedControllers()
        {
            // Get assigned devices from ControllerManager
            InputDevice player1Device = ControllerManager.Instance.GetAssignedDevice("Player1");
            InputDevice player2Device = ControllerManager.Instance.GetAssignedDevice("Player2");


            // Log the assigned devices
            Debug.Log($"Player1 assigned device: {(player1Device != null ? player1Device.name : "None")}");
            Debug.Log($"Player2 assigned device: {(player2Device != null ? player2Device.name : "None")}");

            // Set up Player 1
            if (player1Object != null && player1Device != null)
            {
                SetupPlayerWithDevice(player1Object, player1Device);
            }
            else
            {
                Debug.LogWarning("Player1 has no assigned device");
            }

            // Set up Player 2
            if (player2Object != null && player2Device != null)
            {
                SetupPlayerWithDevice(player2Object, player2Device);
            }
            else
            {
                Debug.LogWarning("Player2 has no assigned device");
            }
        }

        private void SetupPlayersWithDebugControlSchemes()
        {
            // Set up Player 1 with debug control scheme
            if (player1Object != null)
            {
                SetupPlayerWithDebugScheme(player1Object, player1ControlScheme);
            }

            // Set up Player 2 with debug control scheme
            if (player2Object != null)
            {
                SetupPlayerWithDebugScheme(player2Object, player2ControlScheme);
            }
        }

        private void SetupPlayerWithDevice(GameObject playerObject, InputDevice device)
        {
            string playerTag = playerObject.tag;
            
            // Get control scheme from controller manager
            var playerAssignment = ControllerManager.Instance.FindPlayerAssignment(playerTag);
            if (playerAssignment == null || !playerAssignment.isAssigned)
            {
                Debug.LogError($"No valid assignment found for {playerTag}");
                return;
            }
            
            string controlScheme = GetControlSchemeName(playerAssignment.controlScheme);
            
            // Get or add PlayerInput component
            PlayerInput playerInput = playerObject.GetComponent<PlayerInput>();
            if (playerInput == null)
            {
                playerInput = playerObject.AddComponent<PlayerInput>();
            }

            Debug.Log($"Setting up PlayerInput for {playerTag}");
            
            try
            {
                //playerInput.uiInputModule = FindFirstObjectByType<InputSystemUIInputModule>();
                //playerInput.camera = Camera.main;

                // First, ensure the player input has the correct actions
                //playerInput.actions = inputActions;
                //playerInput.defaultControlScheme = controlScheme;
                //playerInput.defaultActionMap = "BattleControls";

                // Create a dedicated InputUser for this player and pair with device
                //InputUser user = InputUser.CreateUserWithoutPairedDevices();
                //user = InputUser.PerformPairingWithDevice(device, user);

                playerAssignment.user = InputUser.PerformPairingWithDevice(device, playerInput.user);

                // Associate actions with user
                //user.AssociateActionsWithUser(playerInput.actions);
                
                // Set the default action map and switch to it
                //playerInput.SwitchCurrentActionMap("BattleControls");
                
                // Critical: Prevent auto-switching to avoid players interfering with each other
                playerInput.neverAutoSwitchControlSchemes = true;
                
                // Setup proper playerInput reference for PlayerInputAssignment
                playerAssignment.playerInput = playerInput;
                //playerAssignment.user = user;

                Debug.Log($"Successfully set up {playerTag} with {device.name}");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error setting up player control: {e.Message}");
            }
        }

        private void SetupPlayerWithDebugScheme(GameObject playerObject, string controlScheme)
        {
            // Get or add PlayerInput component
            PlayerInput playerInput = playerObject.GetComponent<PlayerInput>();
            if (playerInput == null)
            {
                playerInput = playerObject.AddComponent<PlayerInput>();
                playerInput.actions = inputActions;
            }

            try
            {
                // Set the action map directly
                playerInput.defaultActionMap = controlScheme;
                playerInput.SwitchCurrentActionMap(controlScheme);
                playerInput.neverAutoSwitchControlSchemes = true;
                
                Debug.Log($"Player {playerObject.name} set up with debug control scheme: {controlScheme}");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error setting up debug control scheme: {e.Message}");
            }
        }

    }
}