using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

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
            public string ID;
            public int amount;

            public CollectedFigure() { ID = null; amount = 0; }
            public CollectedFigure(Figure _figure) { ID = _figure.GetID(); amount = 1; }
        };

        [System.Serializable]
        public class CollectionData
        {
            public List<CollectedFigure> collection1;
            public CollectionData() { new List<CollectedFigure>(); }

            public bool IsEmpty() { return collection1.Count == 0; }
        }

        public CollectionData PlayerData;
        public CollectionData TestData;

        public List<CollectedFigure> Player1Collection = new();

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
        public void Save(string playerName, CollectionData playerData)
        {
            if (playerData.IsEmpty())
            {
                Debug.Log($"No data to save, will not overwrite {playerName}.");
                return;
            }
            FileManager.Save(playerName, playerData);
        }

        // Load the collection from a specific player
        public void Load(string playerName) 
        {
            CollectionData playerData = FileManager.Load<CollectionData>(playerName);
            List<CollectedFigure> Collection = new();
            foreach (var colFig in playerData.collection1)
            {
                CollectedFigure newCollectedFigure = new CollectedFigure(FigureManager.Instance.GetFigureByID(colFig.ID));
                Collection.Add(newCollectedFigure);
            }
            int index = 0;
            foreach (CollectedFigure collectedFigure in Collection)
            {
                collectedFigure.amount = playerData.collection1[index].amount;
                Debug.Log("Amount added to " + FigureManager.Instance.GetFigureByID(collectedFigure.ID) + " = " + collectedFigure.amount);
                index++;
            }
            Player1Collection = Collection;
        }

        // Debug function: Removes all save data
        public void DeleteSaveData()
        {
            if (!DebugMode)
            {
                PlayerData.collection1.Clear();
                Player1Collection.Clear();
                Save("data", PlayerData);
            }
            else
            {
                TestData.collection1.Clear();
                Player1Collection.Clear();
                Save("test", TestData);
            }

        }

        public void LoadRandomSaveData(int amountOfFigures)
        {
            // Create a new collection and a checking list for already added figures
            List<GameManager.CollectedFigure> randomCollection = new();
            List<Figure> randomFigures = new();

            // Create a set amount of random figures
            for (int j = 0; j < amountOfFigures; j++)
            {
                GameManager.CollectedFigure randomCollectedFigure = new();
                Figure newRandomFigure = FigureManager.Instance.GetRandomFigure();

                // Check the checking list for duplicate figures
                if (randomFigures.Contains(newRandomFigure)) continue;
                else
                {
                    randomFigures.Add(newRandomFigure);
                    randomCollectedFigure.ID = newRandomFigure.GetID();
                    // Generate a random amount collected
                    randomCollectedFigure.amount = UnityEngine.Random.Range(0, 10);
                    randomCollection.Add(randomCollectedFigure);
                }
            }
            Player1Collection = randomCollection;
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
                foreach (CollectedFigure selectedFigure in Player1Collection)
                {
                    TestData.collection1.Add(selectedFigure);
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
                    Load("data");
                }
                else
                {
                    Debug.Log("Did not find player data; Creating new save data");
                    Save("data",PlayerData);
                }
            }
        }

        private void OnApplicationQuit()
        {
            if (!DebugMode)
            {
                // Store the collected figures in player data
                foreach (CollectedFigure colFig in Player1Collection)
                {
                    PlayerData.collection1.Add(colFig);
                }

                // Save the data
                Save("data", PlayerData);
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
