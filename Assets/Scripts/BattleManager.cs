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

    

    // Tracks if the battle state has changed
    // public UnityEvent BattleStateChanged;
    public UnityEvent<BattleState> ChangetoCountdown = new UnityEvent<BattleState>();
    public UnityEvent<BattleState> ChangetoBattle = new UnityEvent<BattleState>();
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

        State = BattleState.Sleep;

        ChangeStateCountdown();
        ChangeStateBattle();
        ChangeStateVictoryScreen();
    }

    

    public void ChangeStateCountdown()
    {
        if (State == BattleState.Sleep)
        {
            State = BattleState.CountDown;
            ChangetoCountdown.Invoke(State);
        }
        else Debug.Log("Can not change battle state to countdown");
    }

    // Changes battle state to Battle
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

    // Changes battle state to VictoryScreen
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

    private void BattleStartActions()
    {
        
    }

    public void BattleEndActions()
    {
        
    }
}
