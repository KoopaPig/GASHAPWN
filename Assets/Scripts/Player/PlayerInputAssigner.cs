using MyBox;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace GASHAPWN
{
    [DefaultExecutionOrder(-5)]
    [RequireComponent(typeof(PlayerInputManager))]
    public class PlayerInputAssigner : MonoBehaviour
    {
        public static PlayerInputAssigner Instance { get; private set; }

        // Get reference to a single PlayerInput prefab that should be used in all scenarios outside the battle
        [SerializeField] private GameObject uiPlayerInputPrefab;

        // PlayerInputManager is no longer a singleton.
        // It is permanently connected to PlayerInputAssigner which is a wrapper singleton.
        [HideInInspector] public PlayerInputManager playerInputManager;

        [Space(20)]
        [Tooltip("If true, controls are auto-assigned")]
        public bool IsAutoAssignDebug = false;

        [Tooltip("If true, use 2 gamepads for auto-assigning. If false, defaults to P1 = Keyboard & P2 = Gamepad")]
        [ConditionalField("IsAutoAssignDebug")]
        [SerializeField] private bool isTwoGamepad = false; 

        /// <summary>
        /// PlayerControllerAssignment class holds data linking together player and playerInput
        /// </summary>
        [System.Serializable]
        public class PlayerControllerAssignment
        {
            public string playerTag;
            public PlayerInput playerInput;
            public ControlScheme controlScheme;
            public bool isAssigned = false;

            public PlayerControllerAssignment(string tag)
            {
                playerTag = tag;
            }
        }

        // List of PlayerControllerAssignments
        public static List<PlayerControllerAssignment> playerAssignments = new();

        private List<Transform> cachedSpawnPoints;

        private void Awake()
        {
            // Singleton enforcement BEFORE PlayerInputManager initializes
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject); // Avoid PlayerInputManager duplicate error
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            playerInputManager = GetComponent<PlayerInputManager>();
            if (playerInputManager != null)
            {
                playerInputManager.onPlayerJoined += OnPlayerJoined;
            }
            else
            {
                Debug.LogError("PlayerInputManager component missing!");
            }
        }

        private void Start()
        {
            if (IsAutoAssignDebug) Debug_Create2Players(isTwoGamepad);
        }

        // Fires when PlayerInputManager.onPlayerJoined fires
        private void OnPlayerJoined(PlayerInput input)
        {
            // If not LevelSelect, ignore the body of this function
            if (SceneManager.GetActiveScene().name != "LevelSelect") return;

            AssignPlayer(input);
        }

        // Setup player given PlayerInput
        private void AssignPlayer(PlayerInput input)
        {
            // Find player spawn points, cache them
            if (cachedSpawnPoints != null) cachedSpawnPoints.Clear();
            if (cachedSpawnPoints == null || cachedSpawnPoints.Count == 0)
            {
                GameObject[] spawns = GameObject.FindGameObjectsWithTag("PlayerSpawn");
                cachedSpawnPoints = spawns
                    .OrderBy(go => go.name)
                    .Select(go => go.transform)
                    .ToList();
            }

            int index = playerAssignments.Count;
            string playerTag = $"Player{index + 1}"; // Construct playerTag

            var controlScheme = GetControlSchemeFromInput(input);

            var assignment = new PlayerControllerAssignment(playerTag)
            {
                playerInput = input,
                controlScheme = controlScheme,
                isAssigned = true
            };

            playerAssignments.Add(assignment);
            Debug.Log($"{nameof(PlayerInputAssigner)}: Assigned {playerTag} with {input.devices[0].displayName} input.");
            input.gameObject.tag = playerTag;
            input.gameObject.name = playerTag;

            // set input position to cached spawn point
            if (index < cachedSpawnPoints.Count)
            {
                Transform spawnPoint = cachedSpawnPoints[index];
                input.transform.position = spawnPoint.position;
            }
            else Debug.LogWarning($"{nameof(PlayerInputAssigner)}: No spawn point available for {playerTag}. " +
                $"Total available: {cachedSpawnPoints.Count}");

            input.DeactivateInput(); // deactivate until battle
        }

        // Setup 2 players with auto-binded controls for debug purposes
        private void Debug_Create2Players(bool twoGamepad)
        {
            if (Gamepad.all.Count == 0) { Debug.LogError($"{nameof(PlayerInputAssigner)}: Gamepad is not connected. Players not created."); return; }

            DisableJoining(); // Make sure auto-joining is disabled

            var prefab = PlayerInputManager.instance.playerPrefab;
            if (prefab == null)
            {
                Debug.LogError($"{nameof(PlayerInputAssigner)}: No playerPrefab on PlayerInputManager.");
                return;
            }

            var p1 = PlayerInput.Instantiate(prefab, playerIndex: 0, controlScheme: "KeyboardMouse",
                        splitScreenIndex: -1, pairWithDevice: (Gamepad.all.Count > 1 && twoGamepad) ? Gamepad.all[0] : Keyboard.current);
            var p2 = PlayerInput.Instantiate(prefab, playerIndex: 1, controlScheme: "Gamepad",
                        splitScreenIndex: -1, pairWithDevice: (Gamepad.all.Count > 1 && twoGamepad) ? Gamepad.all[1] : Gamepad.all[0]);

            AssignPlayer(p1);
            AssignPlayer(p2);
                
            SetPlayerInputsPersistent();
        }

        private void OnDisable()
        {
            if (PlayerInputManager.instance != null)
            {
                PlayerInputManager.instance.onPlayerJoined -= OnPlayerJoined;
            }
        }


        ///// UTILITY AND HELPER FUNCTIONS /////

        // Mark PlayerInputs in playerAssignments as persistent, only call this when transitioning to battle scene
        public void SetPlayerInputsPersistent()
        {
            if (SceneManager.GetActiveScene().name == "LevelSelect" || IsAutoAssignDebug)
            {
                foreach (var assignment in playerAssignments)
                {
                    DontDestroyOnLoad(assignment.playerInput);
                }
            }
            else Debug.LogError($"{nameof(PlayerInputAssigner)}: Tried to set PlayerInputs as persistent outside the LevelSelect scene or outside debug mode.");
        }
        
        // Returns true if IsAssgined = true for PlayerControllerAssingment corresponding to playerTag
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

        // Returns PlayerControllerAssignment corresponding to playerTag
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
        public void SetBattleControlsActive(bool active, string playerTag)
        {
            if (IsPlayerAssigned(playerTag))
            {
                var i = playerAssignments.Find(x => x.playerTag == playerTag);
                var input = i.playerInput;

                if (active)
                    input.actions.FindActionMap("BattleControls").Enable();
                else
                    input.actions.FindActionMap("BattleControls").Disable();
            }
            else Debug.LogError($"{nameof(PlayerInputAssigner)}: Failed to activate or deactive battle controls because {playerTag}" +
                $" does not have an assigned input.");
        }

        /// <summary>
        /// Activate or deactivate all battle controls
        /// </summary>
        public void SetBattleControlsActive(bool active)
        {
            foreach (var i in playerAssignments)
            {
                if (i.isAssigned && i.playerInput != null)
                {
                    var input = i.playerInput;

                    if (active)
                        input.actions.FindActionMap("BattleControls").Enable();
                    else
                        input.actions.FindActionMap("BattleControls").Disable();
                }
                else Debug.LogError($"{nameof(PlayerInputAssigner)}: Failed to activate or deactive battle controls because {i.playerTag}" +
                $" does not have an assigned input.");
            }
        }

        public static string ControlSchemeToString(ControlScheme scheme)
        {
            return scheme switch
            {
                ControlScheme.KEYBOARD => "KeyboardMouse",
                ControlScheme.XINPUT => "Gamepad",
                _ => throw new System.Exception("PlayerInputAssigner: Unknown string, could not convert to ControlScheme")
            };
        }

        //public static ControlScheme StringToControlScheme(string str)
        //{
        //    return str switch
        //    {
        //        "KeyboardMouse" => ControlScheme.KEYBOARD,
        //        "Gamepad" => ControlScheme.XINPUT,
        //        _ => throw new System.Exception($"PlayerInputAssigner: Unknown string \"{str}\", could not convert to ControlScheme")
        //    };
        //}

        public static ControlScheme GetControlSchemeFromInput(PlayerInput input)
        {
            var dev = input.devices[0];
            if (dev is Gamepad)
                return ControlScheme.XINPUT;
            else if (dev is Keyboard)
                return ControlScheme.KEYBOARD;
            else throw new System.Exception($"{nameof(PlayerInputAssigner)}: Unknown device \"{dev.displayName}\"; could not convert to ControlScheme");
        }

        // Returns true if found a ControlScheme corresponding to playerTag in playerAssignments
        public bool TryGetPlayerControlScheme(string playerTag, out ControlScheme controlScheme)
        {
            var assignment = FindPlayerAssignment(playerTag);
            if (assignment != null)
            {
                controlScheme = assignment.controlScheme;
                return true;
            }

            controlScheme = ControlScheme.KEYBOARD; // fallback
            return false;
        }

        // Returns true if found a ControlScheme from first PlayerInput object in scene
        public bool TryGetAnyControlScheme(out ControlScheme scheme)
        {
            var input = FindFirstObjectByType<PlayerInput>();
            if (input.isActiveAndEnabled)
            {
                try
                {
                    scheme = GetControlSchemeFromInput(input);
                    return true;
                }
                catch { }
            }

            scheme = ControlScheme.KEYBOARD; // fallback
            return false;
        }

        public void EnableJoining() { if (playerInputManager != null) playerInputManager.EnableJoining(); }

        public void DisableJoining() { if (playerInputManager != null) playerInputManager.DisableJoining(); }

        // Clear playerAssignments and destroy persistent PlayerInputs
        public void ClearAssignments()
        {
            foreach (var assignment in playerAssignments)
            {
                if (!string.IsNullOrEmpty(assignment.playerTag))
                {
                    GameObject obj = GameObject.FindWithTag(assignment.playerTag);
                    if (obj != null) Destroy(obj);
                }
            }
            playerAssignments.Clear();
        }

        // Instantiate new PlayerInput for UI navigation
        public void ConsolidatePlayerInput()
        {
            // Destroy PlayerInput components of players
            foreach (var assignment in playerAssignments)
            {
                Destroy(assignment.playerInput);
            }

            // Spawn new PlayerInput for UI
            if (uiPlayerInputPrefab != null)
            {
                GameObject uiInputObj = Instantiate(uiPlayerInputPrefab);
                PlayerInput uiInput = uiInputObj.GetComponent<PlayerInput>();

                SetUIControlsActive(true, uiInput);
            }
            else
            {
                Debug.LogWarning($"{nameof(PlayerInputAssigner)}: UI PlayerInput prefab not assigned.");
            }
        }

        // Activate UI Controls for given PlayerInput
        public void SetUIControlsActive(bool active, PlayerInput input)
        {
            if (active)
            {
                var battleMap = input.actions.FindActionMap("BattleControls");
                if (battleMap != null) battleMap.Disable();    

                input.SwitchCurrentActionMap("UI");
                var uiModule = FindFirstObjectByType<InputSystemUIInputModule>();
                if (uiModule != null) uiModule.actionsAsset = input.actions;
                input.camera = Camera.main;
                if (!input.inputIsActive) input.ActivateInput();
            }
            else
            {
                var battleMap = input.actions.FindActionMap("BattleControls");
                if (battleMap != null)
                {
                    input.SwitchCurrentActionMap("BattleControls");
                    battleMap.Enable();
                }
            }
        }

        /// <summary>
        /// Reset state of all actions for given PlayerInput
        /// </summary>
        public void ClearAllInputs(PlayerInput playerInput)
        {
            foreach (var action in playerInput.actions) action.Reset();
        }
    }
}