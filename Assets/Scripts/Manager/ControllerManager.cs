using System.Collections.Generic;
using System.Linq;
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
            public string controlScheme;  // Use "Keyboard" or "Controller" to match your action maps
            public bool isAssigned = false;

            public PlayerControllerAssignment(string tag)
            {
                playerTag = tag;
            }
        }

        [SerializeField] private InputActionAsset inputActions;

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
            if (playerAssignments.Count == 0)
            {
                playerAssignments.Add(new PlayerControllerAssignment("Player1"));
                playerAssignments.Add(new PlayerControllerAssignment("Player2"));
            }

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
                targetAssignment.controlScheme = "Controller";

                // Remove from available gamepads list
                availableGamepads.Remove(device as Gamepad);

                Debug.Log($"Assigned {device.name} (Controller) to {playerTag}");
            }
            else if (device is Keyboard)
            {
                targetAssignment.controlScheme = "Keyboard";

                // Remove from available keyboards list
                availableKeyboards.Remove(device);

                Debug.Log($"Assigned {device.name} (Keyboard) to {playerTag}");
            }

            assignedDevices.Add(device);

            return true;
        }

        public void SetupPlayerInput(GameObject playerObject)
        {
            if (playerObject == null) return;

            string playerTag = playerObject.tag;

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

            if (targetAssignment == null || !targetAssignment.isAssigned)
                return;

            // Get or add PlayerInput component
            PlayerInput playerInput = playerObject.GetComponent<PlayerInput>();
            if (playerInput == null)
            {
                playerInput = playerObject.AddComponent<PlayerInput>();
            }

            if (inputActions != null)
            {
                playerInput.actions = inputActions;
            }

            // Method 1: Using InputUser directly
            try
            {
                // Create a new InputUser and pair it with the device
                InputUser user = InputUser.CreateUserWithoutPairedDevices();
                user = InputUser.PerformPairingWithDevice(targetAssignment.device, user);

                // Assign the player input to the user
                user.AssociateActionsWithUser(playerInput.actions);

                // Switch to the correct action map immediately
                playerInput.SwitchCurrentActionMap(targetAssignment.controlScheme);

                // Ensure player never auto switches schemes
                playerInput.neverAutoSwitchControlSchemes = true;

                // Store references
                targetAssignment.playerInput = playerInput;
                targetAssignment.user = user;

                Debug.Log($"Set up player input for {playerTag} with {targetAssignment.device.name} using InputUser");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to set up InputUser: {e.Message}. Falling back to alternative method.");

                // Method 2: Alternative approach using PlayerInput directly
                try
                {
                    // Set the action map directly
                    playerInput.defaultActionMap = targetAssignment.controlScheme;
                    playerInput.neverAutoSwitchControlSchemes = true;

                    // Store reference to the player input
                    targetAssignment.playerInput = playerInput;

                    Debug.Log($"Set up player input for {playerTag} with {targetAssignment.device.name} using direct PlayerInput");
                }
                catch (System.Exception ex)
                {
                    Debug.LogError($"Failed to set up direct PlayerInput: {ex.Message}");
                }
            }
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

        public GASHAPWN.UI.ControlScheme GetPlayerControlScheme(string playerTag, System.Action onFallback = null)
        {
            foreach (var assignment in playerAssignments)
            {
                if (assignment.playerTag == playerTag && assignment.isAssigned)
                {
                    if (assignment.device is Gamepad)
                    {
                        return GASHAPWN.UI.ControlScheme.XINPUT;
                    }
                    else
                    {
                        return GASHAPWN.UI.ControlScheme.KEYBOARD;
                    }
                }
            }
            onFallback?.Invoke(); // Invoke fallback action if default condition is reached
            return GASHAPWN.UI.ControlScheme.KEYBOARD; // Default
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

            // Make an exception for cancel action

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
        public void SetBattleControlsActive(string playerTag, bool active)
        {
            if (IsPlayerAssigned(playerTag))
            {
                var i = playerAssignments.Find(x => x.playerTag == playerTag);
                if (active)
                {
                    i.playerInput.actions.FindActionMap(i.controlScheme).Enable();
                }
                else
                {
                    i.playerInput.actions.FindActionMap(i.controlScheme).Disable();
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
                        input.actions.FindActionMap(i.controlScheme).Enable();
                    }
                    else
                    {
                        input.actions.FindActionMap(i.controlScheme).Disable();
                    }
                }
                else Debug.LogError($"ControllerManager: Failed to activate or deactive battle controls because {i.playerTag}" +
                $" does not have an assigned input.");
            }
        }
    }
}