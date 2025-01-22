using UnityEngine;
using UnityEngine.Events;

public class BattleManager : MonoBehaviour
{
    public static BattleManager Instance { get; private set; }

    public enum BattleState { CountDown, Battle, VictoryScreen }

    public BattleState State { get; private set; }

    // Tracks if the battle state has changed
    public UnityEvent BattleStateChanged;

    private void Awake()
    {
        // Check for other instances
        if(Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }
        Instance = this;

        // Initial state
        State = BattleState.CountDown;
    }

    // Changes battle state to Battle
    // Works if the current battle state is CountDown
    public void ChangeStateBattle()
    {
        if (State == BattleState.CountDown)
        {
            State = BattleState.Battle;
            BattleStateChanged.Invoke();
        }
    }

    // Changes battle state to VictoryScreen
    // Works if the current battle state is Battle
    public void ChangeStateVictoryScreen()
    {
        if (State == BattleState.Battle)
        {
            State = BattleState.VictoryScreen;
            BattleStateChanged.Invoke();
        }
    }
}
