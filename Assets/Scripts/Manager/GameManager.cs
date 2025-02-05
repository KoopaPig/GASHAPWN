using JetBrains.Annotations;
using System;
using System.Data;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Events;

namespace GASHAPWN
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        public GameState State;

        public static event Action<GameState> OnGameStateChanged;

        [Header("Events to Trigger")]
        // Triggers when the player switches to the title screen
        public UnityEvent<GameState> ChangeToTitle = new UnityEvent<GameState>();

        // Triggers when the player switches to the level select screen
        public UnityEvent<GameState> ChangeToLevelSelect = new UnityEvent<GameState>();

        // Triggers when the player initiates a battle
        public UnityEvent<GameState> ChangeToBattle = new UnityEvent<GameState>();

        // Triggers when the Collection scene is entered
        public UnityEvent<GameState> ChangeToCollection = new UnityEvent<GameState>();
        public void UpdateGameState(GameState newState)
        {
            switch (newState)
            {
                case GameState.Title:
                    ChangeToTitle.Invoke(newState);
                    break;
                case GameState.LevelSelect:
                    ChangeToLevelSelect.Invoke(newState);
                    break;
                case GameState.Battle:
                    ChangeToBattle.Invoke(newState);
                    break;
                case GameState.Collection:
                    ChangeToCollection.Invoke(newState);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(newState), newState, null);
            }

            OnGameStateChanged?.Invoke(newState);
        }


        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                return;
            }
            Instance = this;

            DontDestroyOnLoad(this);
        }

        private void Start()
        {
            // Initial state
            UpdateGameState(GameState.Title);
        }

        public enum GameState
        {
            Title,
            LevelSelect,
            Battle,
            Collection
        }
    }
}
