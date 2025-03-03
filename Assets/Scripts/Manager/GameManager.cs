using JetBrains.Annotations;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using System;
using System.Data;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;
using System.Collections.Generic;
using Unity.VisualScripting;
using System.IO;

namespace GASHAPWN
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        // Tracks the current game state
        public GameState State;

        public static event Action<GameState> OnGameStateChanged;

        [System.Serializable]
        public class CollectedFigure
        {
            public Figure figure;
            public int amount;

            public CollectedFigure() { figure = null; amount = 0; }
            public CollectedFigure(Figure _figure) { figure = _figure; amount = 1; }
        };

        public List<CollectedFigure> Player1Collection = new();
        public List<CollectedFigure> Player2Collection = new();

        [Header("Events to Trigger")]
        // Triggers when the player switches to the title screen
        public UnityEvent<GameState> ChangeToTitle = new UnityEvent<GameState>();

        // Triggers when the player switches to the level select screen
        public UnityEvent<GameState> ChangeToLevelSelect = new UnityEvent<GameState>();

        // Triggers when the player initiates a battle
        public UnityEvent<GameState> ChangeToBattle = new UnityEvent<GameState>();

        // Triggers when the Collection scene is entered
        public UnityEvent<GameState> ChangeToCollection = new UnityEvent<GameState>();

        // GameManager stores currentBattleTime so LevelSelect can set it
        public float currentBattleTime = 180;

        public void UpdateGameState(GameState newState)
        {
            State = newState;
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

            // Control Scheme Detection at level selection
            if (newState == GameState.LevelSelect) InputSystem.onDeviceChange += DeviceChange;
            else InputSystem.onDeviceChange -= DeviceChange;

            OnGameStateChanged?.Invoke(newState);
        }
        public void DeviceChange(InputDevice device, InputDeviceChange change)
        {
            switch (change)
            {
                case InputDeviceChange.Added:
                    Debug.Log(device.layout + " added");
                    break;
                case InputDeviceChange.ConfigurationChanged:
                    Debug.Log(device.layout + " config changed");
                    break;
                default:
                    break;
            }
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
            //UpdateGameState(GameState.Title);

        }
        // Save the collection to a specific player
        public void Save(string playerName, List<CollectedFigure> Collection)
        {
            FileManager.Save(playerName + ".json", Collection);
        }

        // Load the collection from a specific player
        public List<CollectedFigure> Load(string playerName, List<CollectedFigure> Collection) 
        {
            Collection = FileManager.Load<List<CollectedFigure>>(playerName + ".json");
            return Collection;
        }
    }
    public enum GameState
    {
        Title,
        LevelSelect,
        Battle,
        Collection
    }
}
