using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;

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
                SetupPlayersWithDebugControllers();
            }
            else
            {
                Debug.Log("Setting up player controls from controller assignments...");
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

        private void SetupPlayersWithDebugControllers()
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
            // Get or add PlayerInput component
            PlayerInput playerInput = playerObject.GetComponent<PlayerInput>();
            if (playerInput == null)
            {
                playerInput = playerObject.AddComponent<PlayerInput>();
                playerInput.actions = inputActions;
            }

            // Determine control scheme based on device type
            string controlScheme = (device is Gamepad) ? "Gamepad" : "Keyboard";
            
            // Configure and apply control scheme
            try
            {
                // Check that the control scheme exists in the input actions
                if (HasControlScheme(inputActions, controlScheme))
                {
                    // Disable and enable to force rebinding
                    bool wasEnabled = playerInput.enabled;
                    playerInput.enabled = false;
                    
                    // Configure PlayerInput settings
                    playerInput.neverAutoSwitchControlSchemes = true;
                    playerInput.defaultControlScheme = controlScheme;
                    
                    // Re-enable the component
                    playerInput.enabled = wasEnabled;
                    
                    // Apply device directly to playerInput
                    playerInput.SwitchCurrentControlScheme(controlScheme, device);
                    
                    Debug.Log($"Player {playerObject.name} set up with {controlScheme} scheme using device {device.name}");
                }
                else
                {
                    Debug.LogError($"Control scheme '{controlScheme}' not found in input actions. " +
                                   $"Available schemes: {GetAvailableControlSchemes(inputActions)}");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error setting up control scheme: {e.Message}");
                
                // Fallback method - just try to make the device usable
                try
                {
                    // Just force the input actions and device
                    playerInput.actions = inputActions;
                    playerInput.defaultControlScheme = null; // Let it auto-detect
                    playerInput.neverAutoSwitchControlSchemes = false; // Allow switching
                    
                    Debug.Log($"Player {playerObject.name} set up with fallback method for device {device.name}");
                }
                catch (System.Exception ex)
                {
                    Debug.LogError($"Error using fallback method: {ex.Message}");
                }
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

            // Check that the control scheme exists in the input actions
            if (HasControlScheme(inputActions, controlScheme))
            {
                // Disable and enable to force rebinding
                bool wasEnabled = playerInput.enabled;
                playerInput.enabled = false;
                
                // Configure PlayerInput settings
                playerInput.neverAutoSwitchControlSchemes = true;
                playerInput.defaultControlScheme = controlScheme;
                
                // Re-enable the component
                playerInput.enabled = wasEnabled;
                
                Debug.Log($"Player {playerObject.name} set up with debug control scheme: {controlScheme}");
            }
            else
            {
                Debug.LogError($"Control scheme '{controlScheme}' not found in input actions. " +
                               $"Available schemes: {GetAvailableControlSchemes(inputActions)}");
            }
        }
        
        // Helper method to check if a control scheme exists in the input actions
        private bool HasControlScheme(InputActionAsset actions, string schemeName)
        {
            if (actions == null)
                return false;
                
            foreach (var scheme in actions.controlSchemes)
            {
                if (scheme.name == schemeName)
                    return true;
            }
            
            return false;
        }
        
        // Helper method to get a list of available control schemes
        private string GetAvailableControlSchemes(InputActionAsset actions)
        {
            if (actions == null)
                return "No input actions assigned";
                
            string schemes = "";
            foreach (var scheme in actions.controlSchemes)
            {
                schemes += scheme.name + ", ";
            }
            
            return schemes.TrimEnd(' ', ',');
        }
    }
}