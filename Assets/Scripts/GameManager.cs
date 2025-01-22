using System.Data;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Events;



public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public enum GameState { Title, LevelSelect, Battle }

    public GameState State { get; private set; }

    // Tracks when game states change
    public UnityEvent GameStateChanged;

    private void Awake()
    {
        // Check if there are other instances
        if(Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }
        Instance = this;

        // Initial state
        State = GameState.Title;

        // Persistant Singleton
        DontDestroyOnLoad(this);
    }

    // Changes the game state to Title
    // Works if the current game state is LevelSelect
    public void ChangeStateTitle()
    {
        if(State == GameState.LevelSelect)
        {
            State = GameState.Title;
            GameStateChanged.Invoke();
        }
    }

    // Changes the game state to LevelSelect
    public void ChangeStateLevelSelect()
    {
        if(State == GameState.Title || State == GameState.Battle)
        {
            State = GameState.LevelSelect;
            GameStateChanged.Invoke();
        }
    }

    // Changes the game state to Battle
    // Works if the current game state is LevelSelect
    public void ChangeStateBattle()
    {
        if(State == GameState.LevelSelect)
        {
            State = GameState.Battle;
            GameStateChanged.Invoke();
        }
    }

}