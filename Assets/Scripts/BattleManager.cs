using JetBrains.Annotations;
using System.Diagnostics.Tracing;
using System.Timers;
using UnityEngine;
using UnityEngine.Events;

public class BattleManager : MonoBehaviour
{
    public static BattleManager Instance { get; private set; }

    public enum BattleState { Sleep, CountDown, Battle, VictoryScreen }
    public BattleState State { get; private set; }

    

    // Triggers when countdown initiates
    public UnityEvent<BattleState> ChangetoCountdown = new UnityEvent<BattleState>();

    // Triggers after the countdown has finished
    public UnityEvent<BattleState> ChangetoBattle = new UnityEvent<BattleState>();

    // Triggers when the battle has concluded
    public UnityEvent<BattleState> ChangetoVictory = new UnityEvent<BattleState>();

    private void Awake()
    {
        // Check for other instances
        if(Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }
        Instance = this;

        // Starts dorment and awakes when a battle is initiated
        State = BattleState.Sleep;

    }

    
    // Changes the battle state to countdown
    // Only works when the last state was sleep
    public void ChangeStateCountdown()
    {
        if (State == BattleState.Sleep)
        {
            State = BattleState.CountDown;
            ChangetoCountdown.Invoke(State);
        }
        else Debug.Log("Can not change battle state to countdown");
    }

    // Changes the battle state to Battle
    // Works if the current battle state is CountDown
    public void ChangeStateBattle()
    {
        if (State == BattleState.CountDown)
        {
            State = BattleState.Battle;
            ChangetoBattle.Invoke(State);
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
            State = BattleState.VictoryScreen;
            ChangetoVictory.Invoke(State);
        }
        else Debug.Log("Can not change battle state to victory");
    }

    // Performs actions required when the battle begins
    private void BattleStartActions()
    {
        
    }

    // Performs actions required when the battle ends
    public void BattleEndActions()
    {
        
    }
}
