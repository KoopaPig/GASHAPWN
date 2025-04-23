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
        public bool DebugMode = false;

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
            public List<string> collection1CollectedFigureIDs;

            [SerializeReference]
            public List<string> collection2CollectedFigureIDs;

            [SerializeReference]
            public List<int> collection1CollectedFigureCounts;

            [SerializeReference]
            public List<int> collection2CollectedFigureCounts;

            public Data()
            {
                collection1CollectedFigureIDs = new();
                collection2CollectedFigureIDs = new();
                collection1CollectedFigureCounts = new();
                collection2CollectedFigureCounts = new();
            }
        }

        public Data PlayerData;
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
        public void Load(string playerName) 
        {
            Data playerData = FileManager.Load<Data>(playerName);
            for(int i = 0; i < 2; i++)
            {
                List<CollectedFigure> Collection = new();
                foreach (string ID in playerData.collection1CollectedFigureIDs)
                {
                    CollectedFigure newCollectedFigure = new CollectedFigure(FigureManager.instance.GetFigureByID(ID));
                    Collection.Add(newCollectedFigure);
                }
                int index = 0;
                foreach (CollectedFigure collectedFigure in Collection)
                {
                    collectedFigure.amount = playerData.collection1CollectedFigureCounts[index];
                    Debug.Log("Amount added to " + collectedFigure.figure.name + " = " + collectedFigure.amount);
                    index++;
                }
                if (i == 0) Player1Collection = Collection;
                else Player2Collection = Collection;
            }
        }

        // Debug function: Removes all save data in test
        public void DeleteSaveData()
        {
            TestData.collection1CollectedFigureIDs.Clear();
            TestData.collection2CollectedFigureIDs.Clear();
            TestData.collection1CollectedFigureCounts.Clear();
            TestData.collection2CollectedFigureCounts.Clear();
            Save("test", TestData);
        }

        public void LoadRandomSaveData(int amountOfFigures)
        {
            // Create a new collection and a checking list for already added figures
            List<GameManager.CollectedFigure> randomCollection = new();
            List<Figure> randomFigures = new();

            // Create a set amount of random figures
            for(int i = 0; i < 2; i++)
            {
                for (int j = 0; j < amountOfFigures; j++)
                {
                    GameManager.CollectedFigure randomCollectedFigure = new();
                    Figure newRandomFigure = FigureManager.instance.GetRandomFigure();

                    // Check the checking list for duplicate figures
                    if (randomFigures.Contains(newRandomFigure)) continue;
                    else
                    {
                        randomFigures.Add(newRandomFigure);
                        randomCollectedFigure.figure = newRandomFigure;
                        // Generate a random amount collected
                        randomCollectedFigure.amount = UnityEngine.Random.Range(0, 10);
                        randomCollection.Add(randomCollectedFigure);
                    }
                }
                if (i == 0) Player1Collection = randomCollection;
                else Player2Collection = randomCollection;
            }
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
                LoadRandomSaveData(10);
                for(int i = 0; i < 2; i++)
                {
                    if (i == 0)
                    {
                        foreach (CollectedFigure selectedFigure in Player1Collection)
                        {
                            TestData.collection1CollectedFigureIDs.Add(selectedFigure.figure.GetID());
                            TestData.collection1CollectedFigureCounts.Add(selectedFigure.amount);
                        }
                    }
                    else
                    {
                        foreach (CollectedFigure selectedFigure in Player2Collection)
                        {
                            TestData.collection2CollectedFigureIDs.Add(selectedFigure.figure.GetID());
                            TestData.collection2CollectedFigureCounts.Add(selectedFigure.amount);
                        }
                    }
                }
                Save("test", TestData);
            }
            
        }

        private void Start()
        {
            // Initial state
            UpdateGameState(GameState.Title);

            // Testing section: Loads in the testing data
            if (DebugMode)
            {
                Debug.Log(Application.persistentDataPath);
                // Load Test Data
                if (File.Exists(Path.Combine(Application.persistentDataPath, "test.json")))
                {
                    Debug.Log("Found test data, loading...");
                    if (Player1Collection != null && Player1Collection.Count > 0) Player1Collection.Clear();
                    Load("test");
                    Debug.Log("Data loaded");
                }
                else Debug.LogError("Test data could not be found");
            }
            else
            {
                // Load data file or create a new one
                if (File.Exists(Path.Combine(Application.persistentDataPath, "data.json")))
                {
                    Debug.Log("Found player data");
                    if (Player1Collection != null && Player1Collection.Count > 0) Player1Collection.Clear();
                    if (Player2Collection != null && Player2Collection.Count > 0) Player2Collection.Clear();
                    Load("data");
                }
                else
                {
                    Debug.Log("Did not find player data; Creating new save data");
                    Save("data",PlayerData);
                }
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
