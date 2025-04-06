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

namespace GASHAPWN
{
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

        private List<GameObject> activePlayers = new();

        public List<(GameObject player, bool isWinner)> pendingPlayerResults = new();

        [Header("Controls Reference")]
        public InputActionAsset controls;

        InputActionMap battleControls;

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

        public UnityEvent<string, Figure> OnWinningFigure = new();

        public UnityEvent<string, Figure> OnLosingFigure = new();

        
        /// PRIVATE METHODS ///
        
        private void Awake()
        {
            // Check for other instances
            // if (Instance != null && Instance != this)
            // {
            //     Destroy(this);
            //     return;
            // }
            // _instance = this;
            
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
                        paf.SetFigureInCapsule(FigureManager.instance.GetRandomFigureWeighted());
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

            //battleControls = controls.FindActionMap("Player");
            if (isCountDownOn) ChangeStateCountdown();
            else ChangeStateBattle();
        }

        private void Update()
        {
            if (State == BattleState.CountDown)
            {
                countDownTime -= Time.deltaTime;
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
                    if (isDebug) OnPlayerDeathDebug();
                }
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
                    if (isDebug) OnPlayerDeathDebug();

                }

            }

            // Check to exit victory screen
            if (State == BattleState.VictoryScreen && showVictory)
            {
                // Check for inputs from the UI actionmap
                controls.FindActionMap("UI").actionTriggered += End;
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
            // Enable controls here
            //battleControls.Enable();
            //Debug.Log("Inputs enabled");
            Debug.Log("Battle Start!");
        }

        private void SuddenDeathActions()
        {
            // timer should be disabled
            Debug.Log("Entered Sudden Death!");
        }

        private void OnPlayerDeathDebug()
        {
            Debug.Log("BattleManager: OnPlayerDeathDebug is disabled.");
            //OnWinningFigure.Invoke("Player2", player2Figure);
            //OnLosingFigure.Invoke("Player1", player1Figure);
        }

        /// PUBLIC METHODS ///

        // Changes the battle state to countdown
        // Only works when the last state was sleep
        public void ChangeStateCountdown()
        {
            if (State == BattleState.Sleep)
            {
                State = BattleState.CountDown;
                //battleControls.Disable();
                //Debug.Log("Inputs disabled");
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
                OnWinningFigure.Invoke(activePlayers[0].tag, 
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
            // Disable battle controls
            //battleControls.Disable();
            Debug.Log("Battle End!");
        }

        public void FigureCheck(string WinningTag, Figure PlayerFigure)
        {
            //if (!Collection.Contains(PlayerFigure))
            //{
            //    Collection.Add(PlayerFigure);
            //    newFigure = true;
            //}
            //else newFigure = false;

            GameManager.CollectedFigure potentialNewFigure = new(PlayerFigure);

            if(WinningTag == "Player1")
            {
                if (!GameManager.Instance.Player1Collection.Contains(potentialNewFigure))
                {
                    GameManager.Instance.Player1Collection.Add(potentialNewFigure);
                    newFigure = true;
                }
                else
                {
                    int index = GameManager.Instance.Player1Collection.FindIndex(x => x == potentialNewFigure);
                    GameManager.Instance.Player1Collection[index].amount += 1;
                    newFigure = false;
                }
            }
            else
            {
                if (!GameManager.Instance.Player2Collection.Contains(potentialNewFigure))
                {
                    GameManager.Instance.Player2Collection.Add(potentialNewFigure);
                    newFigure = true;
                }
                else
                {
                    int index = GameManager.Instance.Player2Collection.FindIndex(x => x == potentialNewFigure);
                    GameManager.Instance.Player2Collection[index].amount += 1;
                    newFigure = false;
                }
            }
        }

        public void End(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                showVictory = false;
                // Determine if new figure screen should pop up

                //if (playerThatDied.CompareTag("Player1")) FigureCheck(GameManager.Instance.Player1Collection, player2Figure);
                //else if (playerThatDied.CompareTag("Player2")) FigureCheck(GameManager.Instance.Player2Collection, player1Figure);
                //else
                //{
                //    Debug.LogError("End executed without player that died");
                //}

                controls.FindActionMap("UI").actionTriggered -= End;
            }
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

