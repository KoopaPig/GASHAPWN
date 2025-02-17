using JetBrains.Annotations;
using System;
using System.Diagnostics.Tracing;
using System.Timers;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

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

        // Time limit of battle (seconds)
        public float battleTime = 0;

        // Seconds of countdown (should be 6, 1 buffer + 5 countdown)
        [NonSerialized] public float countDownTime = 6;

        // Used so timer starts only when countdown ends
        private bool trackTime = false;

        // Debug Bool: Turn off if you want to skip the countdown
        // Note: only checked in Start()
        public bool isCountDownOn = true;

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
        }

        private void Start()
        {
            // Starts dorment and awakes when a battle is initiated
            State = BattleState.Sleep;
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
                BattleEndActions();
                State = BattleState.VictoryScreen;
                ChangeToVictory.Invoke(State);
            }
            else Debug.Log("Can not change battle state to victory");
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
            if (State == BattleState.VictoryScreen && !showVictory) 
            {
                // Show the victory screen
                showVictory = true;

                // Check for inputs from the UI actionmap
                controls.FindActionMap("UI").actionTriggered += End;
            }
            
        }

        public void OnPlayerDeath()
        {
            playerHasDied = true;
        }

        // Performs actions required when the battle ends
        public void BattleEndActions()
        {
            trackTime = false;
            // Disable battle controls
            //battleControls.Disable();
            Debug.Log("Battle End!");
        }

        public void End(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                showVictory = false;
                // Determine if new figure screen should pop up
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

