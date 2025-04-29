using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;
using System.Collections;
using System.Collections.Generic;
using static GASHAPWN.ControllerManager;
using UnityEngine.InputSystem.UI;
using Unity.VisualScripting;
using static UnityEditor.Searcher.SearcherWindow;

namespace GASHAPWN
{
    public class BattleInitializer : MonoBehaviour
    {
        [SerializeField] private GameObject player1Object;
        [SerializeField] private GameObject player2Object;

        // Force-set specific control schemes (useful for debugging)

        // NOTE: DEBUG IS NOT FUNCTIONAL RIGHT NOW
        [Header("Debug Options")]
        [SerializeField] private bool useDebugControllers = false;
        [SerializeField] private string player1ControlScheme = "Gamepad";
        [SerializeField] private string player2ControlScheme = "KeyboardMouse";

        private void Start()
        {
            Debug.Log("BattleInitializer: Setting up player controls from controller assignments...");
            SetupPlayersWithAssignedControllers();
        }

        private void SetupPlayersWithAssignedControllers()
        {
            foreach (var assignment in ControllerManager.Instance.playerAssignments)
            {
                // find player object via playerTag
                GameObject playerObj = GameObject.FindWithTag(assignment.playerTag);
                if (playerObj == null)
                {
                    Debug.LogError($"BattleInitializer: No player object found with tag {assignment.playerTag}");
                    break;
                }

                // Get PlayerInput component from player game object
                PlayerInput playerInput = playerObj.GetComponent<PlayerInput>();

                // Add component if it doesn't exist
                if (playerInput == null) playerInput = playerObj.AddComponent<PlayerInput>();


                if (!useDebugControllers)
                {
                    SetupPlayerWithDevice(playerInput, assignment);
                }
                else { StartCoroutine(WaitToAddDebugDevices(playerInput, assignment)); }
            }
        }

        private IEnumerator WaitToAddDebugDevices(PlayerInput playerInput, PlayerControllerAssignment assignment)
        {
            yield return null;
            if (assignment.playerTag == "Player1") { SetupPlayerWithDevice(playerInput, assignment, player1ControlScheme); }
            if (assignment.playerTag == "Player2") { SetupPlayerWithDevice(playerInput, assignment, player2ControlScheme); }
            else Debug.LogError("Should not be here.");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="playerInput"></param>
        /// <param name="assignment"></param>
        private void SetupPlayerWithDevice(PlayerInput playerInput, PlayerControllerAssignment assignment, string controlScheme = null)
        {
            Debug.Log($"BattleInitializer: Setting up {assignment.playerTag} with device {assignment.device.displayName}");

            playerInput.actions = ControllerManager.Instance.inputActions;

            if (controlScheme == null) { playerInput.defaultControlScheme = GetControlSchemeName(assignment.controlScheme); }
            else { playerInput.defaultControlScheme = controlScheme; }

            playerInput.defaultActionMap = "BattleControls";
            playerInput.SwitchCurrentActionMap("BattleControls");

            playerInput.neverAutoSwitchControlSchemes = true;

            // Unpair any previous devices and user
            if (playerInput.user.valid)
            {
                playerInput.user.UnpairDevicesAndRemoveUser();
            }

            // Pair this device and assign it to this player
            var pairedUser = InputUser.PerformPairingWithDevice(assignment.device, assignment.user);
            pairedUser.AssociateActionsWithUser(playerInput.actions);

            // Save to assignment 
            assignment.playerInput = playerInput;
            assignment.user = pairedUser;

            playerInput.ActivateInput();
        }

    }
}