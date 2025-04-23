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
using UnityEngine.ProBuilder.MeshOperations;

namespace GASHAPWN
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        // Debug mode for game manager
        public bool DebugMode { get; private set; } = false;

        // Tracks the current game state
        [SerializeField] public GameState State { get; private set; }

        public static event Action<GameState> OnGameStateChanged;

        [System.Serializable]
        public class CollectedFigure
        {
            public Figure figure;
            public int amount;

            public CollectedFigure() { figure = null; amount = 0; }
            public CollectedFigure(Figure _figure) { figure = _figure; amount = 1; }
        };

        [System.Serializable]
        public class Data
        {
            [SerializeReference]
            public List<string> collectedFigureIDs;
            [SerializeReference]
            public List<int> collectedFigureCounts;

            public Data()
            {
                collectedFigureIDs = new();
                collectedFigureCounts = new();
            }
        }

        public Data Player1Data;
        public Data Player2Data;
        public Data TestData;

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

        // Whenever a level is selected, this field should be populated
        public Level currentLevel = null;

        public int numPlayers = 2;

        /// PUBLIC METHODS ///

        // Call this function to set a new GameState
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
            Debug.Log($"GameManager: GameState: {State.ToString()}");
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

        // Save the collection to a specific player
        public void Save(string playerName, Data playerData)
        {
            FileManager.Save(playerName, playerData);
        }

        // Load the collection from a specific player
        public List<CollectedFigure> Load(string playerName) 
        {
            List<CollectedFigure> Collection = new();
            Data playerData = FileManager.Load<Data>(playerName);
            foreach(string ID in playerData.collectedFigureIDs)
            {
                CollectedFigure newCollectedFigure = new CollectedFigure(FigureManager.instance.GetFigureByID(ID));
                Collection.Add(newCollectedFigure);
            }
            int index = 0;
            foreach(CollectedFigure collectedFigure in Collection)
            {
                collectedFigure.amount = playerData.collectedFigureCounts[index];
                Debug.Log("Amount added to " + collectedFigure.figure.name + " = " + collectedFigure.amount);
                index++;
            }
            return Collection;
        }


        /// PRIVATE METHODS ///

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                return;
            }
            Instance = this;

            DontDestroyOnLoad(this);

            // Testing Section: Adding random data to test data and saving it
            if (DebugMode)
            {
                for (int i = 0; i < 10; i++)
                {
                    // Make a new figure
                    Figure testFigure = FigureManager.instance.GetRandomFigure();

                    // Check if the figure already exists in the test data
                    if (TestData.collectedFigureIDs.Contains(testFigure.GetID())) continue;

                    // If it doesn't, add it to the test data
                    else
                    {
                        TestData.collectedFigureIDs.Add(testFigure.GetID());
                        TestData.collectedFigureCounts.Add(UnityEngine.Random.Range(0, 10));
                    }
                }

                Save("test", TestData);
            }
            
        }

        private void Start()
        {
            // Initial state
            UpdateGameState(GameState.Title);

            // Load data file or create a new one
            if (File.Exists(Path.Combine(Application.persistentDataPath, "data.json")))
            {
                Debug.Log("Found player data");
                if (Player1Collection != null && Player1Collection.Count > 0) Player1Collection.Clear();
                Player1Collection = Load("data");
            }
            else
            {
                Debug.Log("Did not find player data");
                File.Create(Path.Combine(Application.persistentDataPath, "data.json"));
                Player1Collection = new();
            }

            // Testing section: Loads in the testing data
            if (DebugMode)
            {
                Debug.Log(Application.persistentDataPath);
                // Load Test Data
                if (File.Exists(Path.Combine(Application.persistentDataPath, "test.json")))
                {
                    Debug.Log("Found test data, loading...");
                    if (Player1Collection != null && Player1Collection.Count > 0) Player1Collection.Clear();
                    Player1Collection = Load("test");
                    Debug.Log("Data loaded");
                    Debug.Log("Player1Collection count: " + Player1Collection.Count);
                }
                else Debug.LogError("Test data could not be found");
            }

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
