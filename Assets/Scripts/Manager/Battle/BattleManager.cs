using GASHAPWN.UI;
using JetBrains.Annotations;
using NUnit.Framework;
using System;
using System.Diagnostics.Tracing;
using System.Timers;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using Unity.VisualScripting;
using Unity.Mathematics;
using System.Collections;
using System.Linq;
using static GASHAPWN.GameManager;

namespace GASHAPWN
{
    [RequireComponent (typeof(BattleInitializer))]
    public class BattleManager : MonoBehaviour
    {
        public static BattleManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindFirstObjectByType<BattleManager>();
                }
                return _instance;
            }
        }

        private static BattleManager _instance;

        // Tracks the current game state
        public BattleState State { get; private set; }

        public static event Action<BattleState> OnBattleStateChanged;

        [Header("Booleans")]
        // New figure to add to collection or figure is already in collection
        public bool newFigure = false;

        // Victory screen
        private bool showVictory = false;

        private bool isPlayerSpawn = false;

        // If isWinner, then battle end
        [NonSerialized] public bool isWinner = false;

        // Temporary debug bool: if toggled, ignore playerData calling playerDeath
        [SerializeField] private bool isDebug = false;

        [Header("Time")]
        // Time limit of battle (seconds)
        public float battleTime = 0;

        // Seconds of countdown (should be 6, 1 buffer + 5 countdown)
        [NonSerialized] public float countDownTime = 6;

        // Used so timer starts only when countdown ends
        private bool trackTime = false;

        // Debug Bool: Turn off if you want to skip the countdown
        // Note: only checked in Start()
        public bool isCountDownOn = true;

        [Header("Player Reference")]

        public BattleGUIController player1BattleGUI;
        public BattleGUIController player2BattleGUI;
        public Transform player1SpawnPos;
        public Transform player2SpawnPos;

        private List<GameObject> activePlayers = new();

        public List<(GameObject player, bool isWinner)> pendingPlayerResults = new();

        [Header("Controls Reference")]
        public InputActionAsset controls;

        [Header("Events to Trigger")]
        // Triggers when countdown initiates
        public UnityEvent<BattleState> ChangeToCountdown = new();

        // Triggers after the countdown has finished
        public UnityEvent<BattleState> ChangeToBattle = new();

        // Triggers when Sudden Death entered
        public UnityEvent<BattleState> ChangeToSuddenDeath = new();

        // Triggers when the battle has concluded
        public UnityEvent<BattleState> ChangeToVictory = new();

        public UnityEvent<BattleState> ChangeToNewFigure = new();

        // Triggers when players are spawned into the arena
        public UnityEvent<BattleState> OnPlayerSpawn = new();

        public UnityEvent<GameObject, string, Figure> OnWinner = new();

        
        /// PRIVATE METHODS ///
        
        private void Awake()
        {
            battleTime = GameManager.Instance.currentBattleTime;

            // Find all active players
            PlayerData[] allPlayers = FindObjectsByType<PlayerData>(FindObjectsSortMode.InstanceID);
            foreach (PlayerData player in allPlayers)
            {
                if (player != null)
                {
                    // Add player to activePlayers
                    activePlayers.Add(player.gameObject);
                    // Generate figures
                    var paf = player.GetComponent<PlayerAttachedFigure>();
                        paf.SetFigureInCapsule(FigureManager.Instance.GetRandomFigureWeighted());
                    // Link generated figures to BattleGUI
                    if (player.tag == "Player1") {
                        player1BattleGUI.SetFigureName(paf.GetAttachedFigure().name);
                        player1BattleGUI.SetFigureIcon(paf.GetAttachedFigure().Icon);
                    } else if (player.tag == "Player2") {
                        player2BattleGUI.SetFigureName(paf.GetAttachedFigure().name);
                        player2BattleGUI.SetFigureIcon(paf.GetAttachedFigure().Icon);
                    } else {
                        Debug.LogError("BattleManager: Invalid Player tag. Could not set BattleGUI.");
                    }
                    
                }
            }
            if (activePlayers.Count > GameManager.Instance.numPlayers) {
                Debug.LogError("BattleManager: Number of active players does not match total number of players.");
            }
        }

        private void Start()
        {
            // Starts dorment and awakes when a battle is initiated
            State = BattleState.Sleep;

            if (isCountDownOn) ChangeStateCountdown();
            else ChangeStateBattle();
        }

        private void Update()
        {
            if (State == BattleState.CountDown)
            {
                countDownTime -= Time.deltaTime;
                // Time player spawn actions here
                if (countDownTime <= 5) {
                    if (!isPlayerSpawn) { 
                        OnPlayerSpawnActions(State);
                        isPlayerSpawn = true;
                    }
                }
                // When countdown over, change to Battle state
                if (countDownTime <= 0)
                {
                    ChangeStateBattle();
                }
            }

            if (trackTime)
            {
                battleTime -= Time.deltaTime;
            }

            if (State == BattleState.Battle)
                // display victory screen if player died during battle
                if (isWinner)
                {
                    ChangeStateVictoryScreen();
                }
                // else enter sudden death
                else if (battleTime <= 0)
                {
                    ChangeStateSuddenDeath();
                }

            if (State == BattleState.SuddenDeath)
            {
                // display victory screen if player died during sudden death
                if (isWinner)
                {
                    ChangeStateVictoryScreen();
                }
            }
        }

        private bool IsPlayerWin(GameObject player) {
            if (activePlayers.Count == 1) { return true; }
            else { 
                activePlayers.Remove(player);
                pendingPlayerResults.Add((player, false));
                return false;
            }
        }

        private void OnDisable()
        {
            RemoveAllListeners();
            PlayerInputAssigner.Instance.ClearAssignments();
        }

        private void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
            }
        }

        // Removes all listeners from object before it is destroyed
        private void RemoveAllListeners()
        {
            // Check if there are any listeners before removing
            if (OnBattleStateChanged != null)
            {
                foreach (var d in OnBattleStateChanged.GetInvocationList())
                {
                    OnBattleStateChanged -= (Action<BattleState>)d;
                }
            }
        }

        // Performs actions required when the battle begins
        private void BattleStartActions()
        {
            trackTime = true;
            PlayerInputAssigner.Instance.SetBattleControlsActive(true); // activate controls
            Debug.Log("Battle Start!");
        }

        private void SuddenDeathActions()
        {
            Debug.Log("Entered Sudden Death!");
            ResetToSpawn(); // set players back to spawn points
            // Set HP of all active players to 1
            foreach (var player in activePlayers)
            {
                player.GetComponent<PlayerData>().SetHP(1);
            }
        }

        private IEnumerator WaitToClearResults()
        {
            yield return new WaitForNextFrameUnit();
            pendingPlayerResults.Clear();
        }

        /// PUBLIC METHODS ///

        // Changes the battle state to countdown
        // Only works when the last state was sleep
        public void ChangeStateCountdown()
        {
            if (State == BattleState.Sleep)
            {
                State = BattleState.CountDown;
                ChangeToCountdown.Invoke(State);
                OnBattleStateChanged?.Invoke(State);
                Debug.Log($"BattleManager: BattleState: {State.ToString()}");
                Debug.Log("Countdown from " + countDownTime + " begins");
            }
            else Debug.Log("Can not change battle state to countdown");
        }

        // Changes the battle state to Battle
        // Works if the current battle state is CountDown
        public void ChangeStateBattle()
        {
            if (State == BattleState.CountDown || 
                ((State == BattleState.Sleep) && !isCountDownOn))
            {
                State = BattleState.Battle;
                ChangeToBattle.Invoke(State);
                OnBattleStateChanged?.Invoke(State);
                BattleStartActions();
                Debug.Log($"BattleManager: BattleState: {State.ToString()}");
            }
            else Debug.Log("Can not change battle state to battle");
        }

        public void ChangeStateSuddenDeath()
        {
            if (State == BattleState.Battle)
            {
                State = BattleState.SuddenDeath;
                ChangeToSuddenDeath.Invoke(State);
                SuddenDeathActions();
                OnBattleStateChanged?.Invoke(State);
                Debug.Log($"BattleManager: BattleState: {State.ToString()}");
            }
            else Debug.Log("Can not change battle state to sudden death");
        }

        // Changes the BattleState to VictoryScreen
        // Works if the current battle state is Battle or SuddenDeath
        public void ChangeStateVictoryScreen()
        {
            if (State == BattleState.Battle || State == BattleState.SuddenDeath)
            {
                State = BattleState.VictoryScreen;
                ChangeToVictory.Invoke(State);
                BattleEndActions();
                OnBattleStateChanged?.Invoke(State);
                Debug.Log($"BattleManager: BattleState: {State.ToString()}");
            }
            else Debug.Log("Can not change battle state to victory");
        }

        // Changes the BattleState to NewFigureScreen
        // Works if current BattleState is VictoryScreen
        public void ChangeStateNewFigureScreen()
        {
            if (State == BattleState.VictoryScreen)
            {
                State = BattleState.NewFigureScreen;
                ChangeToNewFigure.Invoke(State);
                OnBattleStateChanged?.Invoke(State);
                Debug.Log($"BattleManager: BattleState: {State.ToString()}");
            }
            else Debug.Log("BattleManager: Can not change battle state to newFigureScreen");
        }

        // Set winner and loser whenever player dies
        public void OnPlayerDeath(GameObject player)
        {   
            isWinner = IsPlayerWin(player);

            // Store the player results instead of invoking the events immediately
            pendingPlayerResults.Add((player, isWinner));

            // if activePlayers is down to 1 after checking if principle player died, 
            // then we know the remaining player has won
            if (activePlayers.Count == 1) {
                isWinner = IsPlayerWin(activePlayers[0]);
                // Store results for winning player
                pendingPlayerResults.Add((activePlayers[0], isWinner));
                // OnWinningFigure called here
                OnWinner.Invoke(activePlayers[0], activePlayers[0].tag, 
                    activePlayers[0].GetComponent<PlayerAttachedFigure>().GetAttachedFigure());
                activePlayers.Clear();
            } else {
                Debug.LogError($"BattleManager: Remaining Players: {activePlayers.Count}");
            }
        }

        // Performs actions required when the battle ends
        public void BattleEndActions()
        {
            trackTime = false;
            PlayerInputAssigner.Instance.SetBattleControlsActive(false);
            PlayerInputAssigner.Instance.ConsolidatePlayerInput();
            Debug.Log("Battle End!");
            foreach (var i in pendingPlayerResults)
            {
                if (i.isWinner)
                {
                    FigureCheck(i.player.tag, i.player.GetComponent<PlayerAttachedFigure>().GetAttachedFigure());
                    break;
                }
            }
            StartCoroutine(WaitToClearResults());
        }

        public void OnPlayerSpawnActions(BattleState state)
        {
            OnPlayerSpawn.Invoke(state);
            ResetToSpawn();
            PlayerInputAssigner.Instance.SetBattleControlsActive(false);
        }

        public void ResetToSpawn()
        {
            foreach (var player in activePlayers)
            {
                if (player.tag == "Player1") player.gameObject.transform.position = player1SpawnPos.position;
                else if (player.tag == "Player2") player.gameObject.transform.position = player2SpawnPos.position;
                else Debug.LogError("BattleManager: Invalid Player tag. Could not set spawn positions.");
            }
        }

        public void FigureCheck(string WinningTag, Figure PlayerFigure)
        {
            // see if figure already exists in collection
            var existingFigure = GameManager.Instance.Player1Collection
            .FirstOrDefault(f => f.ID == PlayerFigure.GetID());

            // if it doesn't, mark it as new and add to collection
            if (existingFigure == null)
            {
                GameManager.Instance.Player1Collection.Add(new CollectedFigure(PlayerFigure));
                newFigure = true;
            }
            // if it does, increment amount
            else
            {
                existingFigure.amount += 1;
                newFigure = false;
            }
        }

        public List<GameObject> GetActivePlayers()
        {
            if (activePlayers.Count > 0) { return activePlayers; }
            else return null;
        }
    }

    public enum BattleState
    {
        Sleep,
        CountDown,
        Battle,
        SuddenDeath,
        VictoryScreen,
        NewFigureScreen
    }
}

