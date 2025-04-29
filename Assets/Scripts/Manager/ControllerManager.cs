using System.Collections.Generic;
using System.Linq;
using GASHAPWN.UI;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.Users;

namespace GASHAPWN
{
    public class ControllerManager : MonoBehaviour
    {
        private static ControllerManager _instance;
        public static ControllerManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    // Create instance if it doesn't exist
                    GameObject controllerManagerObj = new GameObject("ControllerManager");
                    _instance = controllerManagerObj.AddComponent<ControllerManager>();
                    DontDestroyOnLoad(controllerManagerObj);

                    Debug.Log("ControllerManager: Auto-created instance");

                    // Initialize the instance
                    _instance.Initialize();
                }
                return _instance;
            }
        }

        [System.Serializable]
        public class PlayerControllerAssignment
        {
            public string playerTag;
            public InputDevice device;
            public InputUser user;
            public PlayerInput playerInput;
            public ControlScheme controlScheme;
            public bool isAssigned = false;

            public PlayerControllerAssignment(string tag)
            {
                playerTag = tag;
            }
        }

        public InputActionAsset inputActions;

        public List<PlayerControllerAssignment> playerAssignments = new List<PlayerControllerAssignment>();
        private List<InputDevice> assignedDevices = new List<InputDevice>();

        // Separate available devices by type to ensure uniqueness
        private List<Gamepad> availableGamepads = new List<Gamepad>();
        private List<InputDevice> availableKeyboards = new List<InputDevice>();
        private bool initialized = false;

        private void Awake()
        {
            // Singleton pattern
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);

            Initialize();
        }

        private void Initialize()
        {
            if (initialized)
                return;

            // Initialize player assignments
            // Later, this should not be so static
            if (playerAssignments.Count == 0)
            {
                playerAssignments.Add(new PlayerControllerAssignment("Player1"));
                playerAssignments.Add(new PlayerControllerAssignment("Player2"));
            }

            // COME BACK
            // Listen for device changes
            InputSystem.onDeviceChange += OnDeviceChange;

            // Populate initial available devices
            RefreshAvailableDevices();

            // Try to automatically find the input actions if not set
            if (inputActions == null)
            {
                // Look for PlayerInput in the scene and get its actions
                PlayerInput[] playerInputs = FindObjectsByType<PlayerInput>(FindObjectsSortMode.None);
                if (playerInputs.Length > 0 && playerInputs[0].actions != null)
                {
                    inputActions = playerInputs[0].actions;
                    Debug.Log("ControllerManager: Auto-found input actions from PlayerInput component");
                }
            }

            initialized = true;
            Debug.Log("ControllerManager: Initialized");
        }

        private void OnDestroy()
        {
            if (_instance == this)
            {
                // COME BACK
                InputSystem.onDeviceChange -= OnDeviceChange;
                _instance = null;
            }
        }

        private void OnDeviceChange(InputDevice device, InputDeviceChange change)
        {
            // Completely ignore Mouse devices
            if (device is Mouse)
                return;

            Debug.Log($"Device change: {device.name} - {change}");

            if (change == InputDeviceChange.Added || change == InputDeviceChange.Reconnected)
            {
                // Add device to appropriate list
                if (device is Gamepad gamepad && !assignedDevices.Contains(gamepad))
                {
                    if (!availableGamepads.Contains(gamepad))
                        availableGamepads.Add(gamepad);
                }
                else if (device is Keyboard keyboard && !assignedDevices.Contains(keyboard))
                {
                    if (!availableKeyboards.Contains(keyboard))
                        availableKeyboards.Add(keyboard);
                }
            }
            else if (change == InputDeviceChange.Removed || change == InputDeviceChange.Disconnected)
            {
                // Remove from available lists
                if (device is Gamepad gamepad)
                    availableGamepads.Remove(gamepad);
                else if (device is Keyboard keyboard)
                    availableKeyboards.Remove(keyboard);

                // Handle disconnection of assigned device
                foreach (var assignment in playerAssignments)
                {
                    if (assignment.device == device)
                    {
                        assignment.isAssigned = false;
                        assignment.device = null;
                        assignedDevices.Remove(device);
                        Debug.Log($"Device disconnected from {assignment.playerTag}");
                    }
                }
            }

            // Notify LevelSelect if it's active
            GASHAPWN.UI.LevelSelect levelSelect = FindFirstObjectByType<GASHAPWN.UI.LevelSelect>();
            if (levelSelect != null)
            {
                // GET RID OF FOR NOW, COME BACK
                //levelSelect.RefreshControllerUI();
            }
        }

        public void RefreshAvailableDevices()
        {
            availableGamepads.Clear();
            availableKeyboards.Clear();

            var allDevices = InputSystem.devices;
            foreach (var device in allDevices)
            {
                // Skip devices that are already assigned
                if (assignedDevices.Contains(device))
                    continue;

                // Add to appropriate list
                if (device is Gamepad gamepad)
                {
                    availableGamepads.Add(gamepad);
                }
                else if (device is Keyboard keyboard)
                {
                    availableKeyboards.Add(keyboard);
                }
            }
        }

        public bool AssignControllerToPlayer(string playerTag, InputDevice device)
        {
            // Don't allow mouse assignments
            if (device is Mouse)
                return false;

            Debug.Log($"Attempting to assign {device.name} to {playerTag}");

            // Find the player assignment
            PlayerControllerAssignment targetAssignment = null;
            foreach (var assignment in playerAssignments)
            {
                if (assignment.playerTag == playerTag)
                {
                    targetAssignment = assignment;
                    break;
                }
            }

            if (targetAssignment == null)
            {
                Debug.LogError($"No player assignment found for tag: {playerTag}");
                return false;
            }

            // If device is already assigned to another player, return false
            foreach (var assignment in playerAssignments)
            {
                if (assignment != targetAssignment && assignment.isAssigned && assignment.device == device)
                {
                    Debug.LogWarning($"Device {device.name} is already assigned to {assignment.playerTag}");
                    return false;
                }
            }

            // Unassign previous device if there was one
            if (targetAssignment.isAssigned && targetAssignment.device != null)
            {
                assignedDevices.Remove(targetAssignment.device);

                // Return the device to available lists
                if (targetAssignment.device is Gamepad gamepad)
                {
                    if (!availableGamepads.Contains(gamepad))
                        availableGamepads.Add(gamepad);
                }
                else if (targetAssignment.device is Keyboard keyboard)
                {
                    if (!availableKeyboards.Contains(keyboard))
                        availableKeyboards.Add(keyboard);
                }

                Debug.Log($"Unassigned {targetAssignment.device.name} from {playerTag}");
            }

            // Assign new device
            targetAssignment.device = device;
            targetAssignment.isAssigned = true;

            // Determine control scheme
            if (device is Gamepad)
            {
                targetAssignment.controlScheme = ControlScheme.XINPUT;

                // Remove from available gamepads list
                availableGamepads.Remove(device as Gamepad);

                Debug.Log($"Assigned {device.name} (Controller) to {playerTag}");
            }
            else if (device is Keyboard)
            {
                targetAssignment.controlScheme = ControlScheme.KEYBOARD;

                // Remove from available keyboards list
                availableKeyboards.Remove(device);

                Debug.Log($"Assigned {device.name} (Keyboard) to {playerTag}");
            }

            assignedDevices.Add(device);

            return true;
        }

        public bool IsPlayerAssigned(string playerTag)
        {
            foreach (var assignment in playerAssignments)
            {
                if (assignment.playerTag == playerTag)
                {
                    return assignment.isAssigned;
                }
            }
            return false;
        }

        public ControlScheme GetPlayerControlScheme(string playerTag, System.Action onFallback = null)
        {
            foreach (var assignment in playerAssignments)
            {
                if (assignment.playerTag == playerTag && assignment.isAssigned)
                {
                    if (assignment.device is Gamepad)
                    {
                        return ControlScheme.XINPUT;
                    }
                    else
                    {
                        return ControlScheme.KEYBOARD;
                    }
                }
            }
            onFallback?.Invoke(); // Invoke fallback action if default condition is reached
            return ControlScheme.KEYBOARD; // Default
        }

        public List<Gamepad> GetAvailableGamepads()
        {
            return availableGamepads;
        }

        public List<InputDevice> GetAvailableKeyboards()
        {
            return availableKeyboards;
        }

        public List<InputDevice> GetAvailableDevices()
        {
            List<InputDevice> allDevices = new List<InputDevice>();
            allDevices.AddRange(availableGamepads);
            allDevices.AddRange(availableKeyboards);
            return allDevices;
        }

        public InputDevice GetAssignedDevice(string playerTag)
        {
            foreach (var assignment in playerAssignments)
            {
                if (assignment.playerTag == playerTag && assignment.isAssigned)
                {
                    return assignment.device;
                }
            }
            return null;
        }

        // Use this to explicitly set the InputActionAsset if needed
        public void SetInputActions(InputActionAsset actions)
        {
            if (actions != null)
            {
                inputActions = actions;
                Debug.Log("ControllerManager: Input actions set manually");
            }
        }

        // Check if any button is pressed on a device
        public bool IsAnyButtonPressed(InputDevice device)
        {
            // Make an exception for cancel action
            var cancelAction = inputActions["Cancel"].controls;

            if (device is Gamepad gamepad)
            {
                // Check for any pressed buttons on gamepad
                foreach (var control in gamepad.allControls)
                {
                    if (control is ButtonControl button && button.isPressed)
                    {
                        if (!cancelAction.Contains(button))
                        {
                            return true;
                        }
                    }
                }
            }
            else if (device is Keyboard keyboard)
            {
                // Check for any pressed keys
                foreach (var control in keyboard.allControls)
                {
                    if (control is KeyControl key && key.isPressed)
                    {
                        if (!cancelAction.Contains(key))
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        public PlayerControllerAssignment FindPlayerAssignment(string playerTag)
        {
            foreach (var assignment in playerAssignments)
            {
                if (assignment.playerTag == playerTag && assignment.isAssigned)
                {
                    return assignment;
                }
            }
            return null;
        }

        /// <summary>
        /// Activate or deactivate battle controls given specific playerTag
        /// </summary>
        /// <param name="playerTag"></param>
        /// <param name="active"></param>
        public void SetBattleControlsActive(bool active, string playerTag)
        {
            if (IsPlayerAssigned(playerTag))
            {
                var i = playerAssignments.Find(x => x.playerTag == playerTag);
                if (active)
                {
                    i.playerInput.actions.FindActionMap("BattleControls").Enable();
                    i.playerInput.actions.FindActionMap("UI").Disable();
                    i.playerInput.SwitchCurrentActionMap("BattleControls");
                }
                else
                {
                    i.playerInput.actions.FindActionMap("BattleControls").Disable();
                    i.playerInput.actions.FindActionMap("UI").Enable();
                    i.playerInput.SwitchCurrentActionMap("UI");
                }
            }
            else Debug.LogError($"ControllerManager: Failed to activate or deactive battle controls because {playerTag}" +
                $" does not have an assigned input.");
        }

        /// <summary>
        /// Activate or deactivate all battle controls
        /// </summary>
        /// <param name="active"></param>
        public void SetBattleControlsActive(bool active)
        {
            foreach (var i in playerAssignments)
            {
                if (i.isAssigned && i.playerInput != null)
                {
                    var input = i.playerInput;
                    if (active)
                    {
                        input.actions.FindActionMap("BattleControls").Enable();
                        input.actions.FindActionMap("UI").Disable();
                        input.SwitchCurrentActionMap("BattleControls");
                    }
                    else
                    {
                        input.actions.FindActionMap("BattleControls").Disable();
                        input.actions.FindActionMap("UI").Enable();
                        input.SwitchCurrentActionMap("UI");
                    }
                }
                else Debug.LogError($"ControllerManager: Failed to activate or deactive battle controls because {i.playerTag}" +
                $" does not have an assigned input.");
            }
        }

        // Helper function to convert ControlScheme into strings that can be read by input system
        // Expand this later if we need to map more ControlSchemes
        public static string GetControlSchemeName(ControlScheme scheme)
        {
            switch (scheme)
            {
                case ControlScheme.XINPUT:
                    return "Gamepad";
                case ControlScheme.KEYBOARD:
                    return "KeyboardMouse";
                default:
                    throw new System.Exception("Unknown control scheme");
            }
        }

    }

    // Maybe somehow I can assign the playerInput field in each player based on scene
}