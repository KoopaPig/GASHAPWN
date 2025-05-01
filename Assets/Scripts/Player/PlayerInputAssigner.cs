using System.Collections.Generic;
using System.Linq;
using GASHAPWN;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.InputSystem.Users;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using static UnityEditor.Searcher.SearcherWindow;

namespace GASHAPWN
{
    [DefaultExecutionOrder(-5)]
    [RequireComponent(typeof(PlayerInputManager))]
    public class PlayerInputAssigner : MonoBehaviour
    {
        public static PlayerInputAssigner Instance { get; private set; }

        // PlayerInputManager is no longer a singleton.
        // It is permanently connected to PlayerInputAssigner which is a wrapper singleton.
        [HideInInspector] public PlayerInputManager playerInputManager;

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

        // Get reference to a single PlayerInput prefab that should be used in all scenarios outside the battle
        [SerializeField] private GameObject uiPlayerInputPrefab;

        // NEED TO ADD A DEBUG FUNCTION BACK IN HERE

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

        private void OnPlayerJoined(PlayerInput input)
        {
            // If not LevelSelect, ignore the body of this function
            if (SceneManager.GetActiveScene().name != "LevelSelect") return;

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

            var controlScheme = StringToControlScheme(input.currentControlScheme);

            var assignment = new PlayerControllerAssignment(playerTag)
            {
                playerInput = input,
                controlScheme = controlScheme,
                isAssigned = true
            };

            playerAssignments.Add(assignment);
            Debug.Log($"PlayerInputAssigner: Assigned {playerTag} with {input.devices[0].displayName} input.");
            input.gameObject.tag = playerTag;
            input.gameObject.name = playerTag;
            
            // set input position to cached spawn point
            if (index < cachedSpawnPoints.Count)
            {
                Transform spawnPoint = cachedSpawnPoints[index];
                input.transform.position = spawnPoint.position;
            }
            else Debug.LogWarning($"PlayerInputAssigner: No spawn point available for {playerTag}. " +
                $"Total available: {cachedSpawnPoints.Count}");

            input.DeactivateInput(); // deactivate until battle
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
            if (SceneManager.GetActiveScene().name == "LevelSelect")
            {
                foreach (var assignment in playerAssignments)
                {
                    DontDestroyOnLoad(assignment.playerInput);
                }
            }
            else Debug.LogError("PlayerInputAssigner: Tried to set PlayerInputs as persistent outside the LevelSelect scene.");
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

        public static string ControlSchemeToString(ControlScheme scheme)
        {
            return scheme switch
            {
                ControlScheme.KEYBOARD => "KeyboardMouse",
                ControlScheme.XINPUT => "Gamepad",
                _ => throw new System.Exception("PlayerInputAssigner: Unknown string, could not convert to ControlScheme")
            };
        }

        public static ControlScheme StringToControlScheme(string str)
        {
            return str switch
            {
                "KeyboardMouse" => ControlScheme.KEYBOARD,
                "Gamepad" => ControlScheme.XINPUT,
                _ => throw new System.Exception("PlayerInputAssigner: Unknown string, could not convert to ControlScheme")
            };
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
                    scheme = StringToControlScheme(input.currentControlScheme);
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

                uiInput.SwitchCurrentActionMap("UI");
                uiInput.camera = Camera.main;
                uiInput.uiInputModule = FindFirstObjectByType<InputSystemUIInputModule>();
                uiInput.ActivateInput();
            }
            else
            {
                Debug.LogWarning("UI PlayerInput prefab not assigned.");
            }
        }
    }
}