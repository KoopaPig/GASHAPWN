using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;
using System.Collections;
using System.Collections.Generic;
using static GASHAPWN.PlayerInputAssigner;
using UnityEngine.InputSystem.UI;
using Unity.VisualScripting;
using UnityEngine.EventSystems;

namespace GASHAPWN
{
    /// <summary>
    /// Initializes PlayerInput and PlayerData on Battle scene startup
    /// </summary>
    public class BattleInitializer : MonoBehaviour
    {
        private void Start()
        {
            Debug.Log("BattleInitializer: Setting up player controls from controller assignments...");
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

                // Find matching playerObject in scene if it exists
                GameObject playerObj = GameObject.FindWithTag(assignment.playerTag);

                assignment.playerInput.defaultActionMap = "BattleControls";
                assignment.playerInput.SwitchCurrentActionMap("BattleControls");

                assignment.playerInput.neverAutoSwitchControlSchemes = true;
                assignment.playerInput.uiInputModule = FindFirstObjectByType<InputSystemUIInputModule>();
                assignment.playerInput.camera = Camera.main;
                assignment.playerInput.ActivateInput();

                // Initialize playerData component now that input is initialized and we are in battle scene
                playerObj.GetComponent<PlayerData>().InitializePlayerData();
            }
        }
    }
}