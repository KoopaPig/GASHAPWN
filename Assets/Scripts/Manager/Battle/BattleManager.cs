using GASHAPWN.UI;
using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using Unity.VisualScripting;
using System.Collections;
using System.Linq;

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

        [Header("Booleans")]
            // New figure to add to collection or figure is already in collection
            [NonSerialized] public bool newFigure = false;
            // If isWinner, then battle end
            [NonSerialized] public bool isWinner = false;
            // Flag for when players spawn
            private bool isPlayerSpawn = false;

        [Header("Time")]
            // Time limit of battle (seconds)
            public float battleTime = 0;
            // Seconds of countdown (should be 6, 1 buffer + 5 countdown)
            [NonSerialized] public float countDownTime = 6;
            // Debug Bool: Turn off if you want to skip the countdown
            // Note: only checked in Start()
            [SerializeField] private bool isCountDownOn = true;
            // Used so timer starts only when countdown ends
            private bool trackTime = false;

        [Header("Player Reference")]
            [Tooltip("Player 1 BattleGUI")]
            public BattleGUIController player1BattleGUI;

            [Tooltip("Player 2 BattleGUI")]
            public BattleGUIController player2BattleGUI;

            [Tooltip("Player 1 spawn position")]
            public Transform player1SpawnPos;

            [Tooltip("Player 2 spawn position")]
            public Transform player2SpawnPos;

        // Queue of defeated players plus winner at top
        public Queue<(GameObject player, bool isWinner)> pendingPlayerResults = new();

        // List to track current active players (removed when defeated / game end)
        private List<GameObject> activePlayers = new();

        #region Battle State Events
        [Header("Events to Trigger")]
            // Triggers when countdown initiates
            public UnityEvent<BattleState> ChangeToCountdown = new();

            // Triggers after the countdown has finished
            public UnityEvent<BattleState> ChangeToBattle = new();

            // Triggers when Sudden Death entered
            public UnityEvent<BattleState> ChangeToSuddenDeath = new();

            // Triggers when the battle has concluded
            public UnityEvent<BattleState> ChangeToResults = new();
            
            // Triggers when changing to new figure screen
            public UnityEvent<BattleState> ChangeToNewFigure = new();

            // Triggers when players are spawned into the arena
            public UnityEvent<BattleState> OnPlayerSpawn = new();

            // Triggers when winner has been declared
            public UnityEvent<GameObject, string, Figure> OnWinner = new();
        #endregion

        // Tracks the current game state
        public BattleState State { get; private set; }
        public static event Action<BattleState> OnBattleStateChanged;


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
                    ChangeStateResultsScreen();
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
                    ChangeStateResultsScreen();
                }
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

        // Performs actions required when sudden death
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

        // Set given player as the winner
        private bool IsPlayerWin(GameObject player) {
            if (activePlayers.Count == 1) { return true; }
            else { 
                activePlayers.Remove(player);
                pendingPlayerResults.Enqueue((player, false));
                return false;
            }
        }

        // Wait one frame before clearing playerPendingResults
        private IEnumerator WaitToClearResults()
        {
            yield return new WaitForNextFrameUnit();
            pendingPlayerResults.Clear();
        }


        /// PUBLIC METHODS ///

        /// <summary>
        /// Changes the State to Countdown (only when State == Sleep)
        /// </summary>
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

        /// <summary>
        /// Changes the State to Battle (only when State == Countdown)
        /// </summary>
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

        /// <summary>
        /// Changes the State to Sudden Death (only when State == Battle)
        /// </summary>
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

        /// <summary>
        /// Changes the State to ResultsScreen (only when State == Battle || SuddenDeath)
        /// </summary>
        public void ChangeStateResultsScreen()
        {
            if (State == BattleState.Battle || State == BattleState.SuddenDeath)
            {
                State = BattleState.ResultsScreen;
                ChangeToResults.Invoke(State);
                BattleEndActions();
                OnBattleStateChanged?.Invoke(State);
                Debug.Log($"BattleManager: BattleState: {State.ToString()}");
            }
            else Debug.Log("Can not change battle state to victory");
        }

        /// <summary>
        /// Changes the State to NewFigureScreen (only when State == ResultsScreen)
        /// </summary>
        public void ChangeStateNewFigureScreen()
        {
            if (State == BattleState.ResultsScreen)
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
            pendingPlayerResults.Enqueue((player, isWinner));

            // if activePlayers is down to 1 after checking if principle player died, 
            // then we know the remaining player has won
            if (activePlayers.Count == 1) {
                isWinner = IsPlayerWin(activePlayers[0]);
                // Store results for winning player
                pendingPlayerResults.Enqueue((activePlayers[0], isWinner));
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

        // Performs actions required when players spawn
        public void OnPlayerSpawnActions(BattleState state)
        {
            OnPlayerSpawn.Invoke(state);
            ResetToSpawn();
            PlayerInputAssigner.Instance.SetBattleControlsActive(false);
        }

        // Reset active players to spawn points
        public void ResetToSpawn()
        {
            foreach (var player in activePlayers)
            {
                if (player.tag == "Player1") player.gameObject.transform.position = player1SpawnPos.position;
                else if (player.tag == "Player2") player.gameObject.transform.position = player2SpawnPos.position;
                else Debug.LogError("BattleManager: Invalid Player tag. Could not set spawn positions.");
            }
        }

        // Returns activePlayers
        public List<GameObject> GetActivePlayers()
        {
            if (activePlayers.Count > 0) { return activePlayers; }
            else return null;
        }

        // Set newFigure according to if given figure is in Collection
        public void FigureCheck(string WinningTag, Figure PlayerFigure)
        {
            // see if figure already exists in collection
            var existingFigure = GameManager.Instance.currPlayerCollectionData.collection
            .FirstOrDefault(f => f.ID == PlayerFigure.GetID());

            // if it doesn't, mark it as new and add to collection
            if (existingFigure == null)
            {
                GameManager.Instance.currPlayerCollectionData.Add(new CollectedFigure(PlayerFigure));
                newFigure = true;
            }
            // if it does, increment amount
            else
            {
                existingFigure.amount += 1;
                newFigure = false;
            }
        }
    }

    public enum BattleState
    {
        Sleep,
        CountDown,
        Battle,
        SuddenDeath,
        ResultsScreen,
        NewFigureScreen
    }
}