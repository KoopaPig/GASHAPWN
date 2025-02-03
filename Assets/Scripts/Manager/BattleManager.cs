using JetBrains.Annotations;
using System;
using System.Diagnostics.Tracing;
using System.Timers;
using UnityEngine;
using UnityEngine.Events;

namespace GASHAPWN
{
    public class BattleManager : MonoBehaviour
    {
        public static BattleManager Instance { get; private set; }
        public BattleState State { get; private set; }

        // Time limit of battle (seconds)
        public float battleTime = 0;

        // Seconds of countdown (should be 5)
        [NonSerialized] public float countDownTime = 5;

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

        }

        private void Start()
        {
            // Starts dorment and awakes when a battle is initiated
            State = BattleState.Sleep;
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

        // Changes the battle state to VictoryScreen
        // Works if the current battle state is Battle
        public void ChangeStateVictoryScreen()
        {
            if (State == BattleState.Battle)
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
            Debug.Log("Battle Start!");
        }
        private void Update()
        {
            if (State == BattleState.CountDown)
            {
                countDownTime -= Time.deltaTime;
                if (countDownTime <= 0)
                {
                    // TODO: Enable controls here
                    ChangeStateBattle();
                }
            }
            if (trackTime)
            {
                battleTime -= Time.deltaTime;
            }
            if (State == BattleState.Battle && battleTime <= 0) ChangeStateVictoryScreen();

            // TODO: Checks to exit victory screen, and checks if "new figure screen" should pop up
        }

        // Performs actions required when the battle ends
        public void BattleEndActions()
        {
            trackTime = false;
            Debug.Log("Battle End!");
        }
    }

    public enum BattleState
    {
        Sleep,
        CountDown,
        Battle,
        VictoryScreen,
        NewFigureScreen
    }
}

