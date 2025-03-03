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

namespace GASHAPWN
{
    public class BattleManager : MonoBehaviour
    {
        public static BattleManager Instance { get; private set; }
        public BattleState State { get; private set; }

        public InputActionAsset controls;

        InputActionMap battleControls;

        // New figure to add to collection or
        // Figure is already in collection
        public bool newFigure = false;

        // New Figure screen
        public bool showFigure = false;

        // Victory screen
        private bool showVictory = false;

        // Controls battle end
        public bool playerHasDied = false;

        GameObject playerThatDied = null;

        // Time limit of battle (seconds)
        public float battleTime = 0;

        // Seconds of countdown (should be 6, 1 buffer + 5 countdown)
        [NonSerialized] public float countDownTime = 6;

        // Used so timer starts only when countdown ends
        private bool trackTime = false;

        // Debug Bool: Turn off if you want to skip the countdown
        // Note: only checked in Start()
        public bool isCountDownOn = true;

        // Player figures generated before battle
        public Figure player1Figure, player2Figure;

        public BattleGUIController player1BattleGUI, player2BattleGUI;

        [Header("Events to Trigger")]
        // Triggers when countdown initiates
        public UnityEvent<BattleState> ChangeToCountdown = new();

        // Triggers after the countdown has finished
        public UnityEvent<BattleState> ChangeToBattle = new();

        // Triggers when Sudden Death entered
        public UnityEvent<BattleState> ChangeToSuddenDeath = new();

        // Triggers when the battle has concluded
        public UnityEvent<BattleState> ChangeToVictory = new();

        // TODO: Triggers if new figure wins
        public UnityEvent<BattleState> ChangeToNewFigure = new();

        public UnityEvent<string, Figure> OnWinningFigure = new();

        private void Awake()
        {
            // Check for other instances
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                return;
            }
            Instance = this;
            
            battleTime = GameManager.Instance.currentBattleTime;

            // Generate Figures
            player1Figure = FigureManager.instance.GetRandomFigure();
            player2Figure = FigureManager.instance.GetRandomFigure();

            // Check for null figures
            if(player1Figure == null || player2Figure == null)
            if (player1Figure == null || player2Figure == null)
            {
                Debug.Log($"Figures failed to generated: 1) {player1Figure.name}, 2) {player2Figure.name}");
                return;
            }
        }

        private void Start()
        {
            // Starts dorment and awakes when a battle is initiated
            State = BattleState.Sleep;

            // Link generated figures to BattleGUI
            player1BattleGUI.SetFigureName(player1Figure.name);
            player1BattleGUI.SetFigureIcon(player1Figure.Icon);
            player2BattleGUI.SetFigureName(player2Figure.name);
            player2BattleGUI.SetFigureIcon(player2Figure.Icon);

            //battleControls = controls.FindActionMap("Player");
            if (isCountDownOn) ChangeStateCountdown();
            else ChangeStateBattle();
        }

        // Changes the battle state to countdown
        // Only works when the last state was sleep
        public void ChangeStateCountdown()
        {
            if (State == BattleState.Sleep)
            {
                State = BattleState.CountDown;
                //battleControls.Disable();
                Debug.Log("Inputs disabled");
                ChangeToCountdown.Invoke(State);
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
                BattleStartActions();
                
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
            }
            else Debug.Log("Can not change battle state to sudden death");
        }

        // Changes the battle state to VictoryScreen
        // Works if the current battle state is Battle or SuddenDeath
        public void ChangeStateVictoryScreen()
        {
            if (State == BattleState.Battle || State == BattleState.SuddenDeath)
            {
                State = BattleState.VictoryScreen;
                ChangeToVictory.Invoke(State);
                BattleEndActions();
            }
            else Debug.Log("Can not change battle state to victory");
        }

        public void ChangeStateNewFigureScreen()
        {
            if (State == BattleState.VictoryScreen)
            {
                State = BattleState.NewFigureScreen;
                ChangeToNewFigure.Invoke(State);
            }
            else Debug.Log("BattleManager: Can not change battle state to newFigureScreen");
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
                if (playerHasDied) { ChangeStateVictoryScreen(); }
                else if (battleTime <= 0)
                {
                    ChangeStateSuddenDeath();
                }
            
            if (State == BattleState.SuddenDeath)
            {
                // display victory screen if player died during sudden death
                if (playerHasDied) { ChangeStateVictoryScreen(); }

            }

            // Check to exit victory screen
            if (State == BattleState.VictoryScreen && showVictory) 
            {
                // Check for inputs from the UI actionmap
                controls.FindActionMap("UI").actionTriggered += End;
            }
            
        }

        public void OnPlayerDeath(GameObject player)
        {
            playerHasDied = true;
            playerThatDied = player;
            if (player.CompareTag("Player1")) OnWinningFigure.Invoke(player.tag, player2Figure);
            else OnWinningFigure.Invoke(player.tag, player1Figure);
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

                if (newFigure) showFigure = true;
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

