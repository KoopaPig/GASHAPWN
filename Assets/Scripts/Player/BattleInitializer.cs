using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;
using System.Collections;
using System.Collections.Generic;
using static GASHAPWN.PlayerInputAssigner;
using UnityEngine.InputSystem.UI;
using Unity.VisualScripting;
using static UnityEditor.Searcher.SearcherWindow;
using UnityEngine.EventSystems;

namespace GASHAPWN
{
    public class BattleInitializer : MonoBehaviour
    {
        private void Start()
        {
            Debug.Log("BattleInitializer: Setting up player controls from controller assignments...");
            //SetupPlayersWithAssignedControllers();
            InitializePlayers();
        }

        private void InitializePlayers()
        {
            foreach (var assignment in PlayerInputAssigner.playerAssignments)
            {
                if (assignment == null || assignment.playerInput == null)
                {
                    Debug.LogWarning("BattleInitializer: Skipping unassigned player.");
                    continue;
                }

                // Find the matching player object in the battle scene
                GameObject playerObj = GameObject.FindWithTag(assignment.playerTag);
                if (playerObj == null)
                {
                    Debug.LogError($"BattleInitializer: Could not find player GameObject with tag {assignment.playerTag}");
                    continue;
                }

                // Remove any existing PlayerInput on the scene object
                var existingInput = playerObj.GetComponent<PlayerInput>();
                if (existingInput != null && existingInput != assignment.playerInput)
                {
                    Destroy(existingInput);
                }

                // Reparent the persistent PlayerInput to the scene player
                assignment.playerInput.transform.SetParent(playerObj.transform, false);
                assignment.playerInput.transform.localPosition = Vector3.zero;

                // Optionally transfer ownership
                assignment.playerInput.gameObject.name = playerObj.name + "_Input";


                assignment.playerInput.defaultActionMap = "BattleControls";
                assignment.playerInput.SwitchCurrentActionMap("BattleControls");

                assignment.playerInput.neverAutoSwitchControlSchemes = true;
                assignment.playerInput.uiInputModule = FindFirstObjectByType<InputSystemUIInputModule>();
                assignment.playerInput.camera = Camera.main;
                assignment.playerInput.ActivateInput();
            }
        }



        private void SetupPlayersWithAssignedControllers()
        {
            foreach (var assignment in PlayerInputAssigner.playerAssignments)
            {
                // find player object via playerTag
                GameObject playerObj = GameObject.FindWithTag(assignment.playerTag);
                if (playerObj == null)
                {
                    Debug.LogError($"BattleInitializer: No player object found with tag {assignment.playerTag}");
                    break;
                }

                PlayerInput input = playerObj.GetComponent<PlayerInput>();
                input = assignment.playerInput;
            }
        }

        private IEnumerator WaitToAddDebugDevices(PlayerInput playerInput, PlayerControllerAssignment assignment)
        {
            yield return null;
            //if (assignment.playerTag == "Player1") { SetupPlayerWithDevice(playerInput, assignment, player1ControlScheme); }
            //if (assignment.playerTag == "Player2") { SetupPlayerWithDevice(playerInput, assignment, player2ControlScheme); }
            //else Debug.LogError("Should not be here.");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="playerInput"></param>
        /// <param name="assignment"></param>
        //private void SetupPlayerWithDevice(PlayerInput playerInput, PlayerControllerAssignment assignment, string controlScheme = null)
        //{
        //    Debug.Log($"BattleInitializer: Setting up {assignment.playerTag} with device {assignment.device.displayName}");

        //    playerInput.actions = ControllerManager.Instance.inputActions;

        //    if (controlScheme == null) { playerInput.defaultControlScheme = PlayerInputAssigner.ControlSchemeToString(assignment.controlScheme); }
        //    else { playerInput.defaultControlScheme = controlScheme; }

        //    playerInput.defaultActionMap = "BattleControls";
        //    playerInput.SwitchCurrentActionMap("BattleControls");

        //    playerInput.neverAutoSwitchControlSchemes = true;

        //    // Unpair any previous devices and user
        //    if (playerInput.user.valid)
        //    {
        //        playerInput.user.UnpairDevicesAndRemoveUser();
        //    }

        //    // Pair this device and assign it to this player
        //    var pairedUser = InputUser.PerformPairingWithDevice(assignment.device, assignment.user);
        //    pairedUser.AssociateActionsWithUser(playerInput.actions);

        //    // Save to assignment 
        //    assignment.playerInput = playerInput;
        //    assignment.user = pairedUser;

        //    playerInput.ActivateInput();
        //}

    }
}