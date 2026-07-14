using System;
using System.Collections.Generic;
using System.IO;
using Unity.AppUI.UI;
using UnityEngine;
using UnityEngine.Events;

namespace GASHAPWN
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("Global Variables")]
            [Tooltip("Maximum number of players")]
            public int numPlayers = 2;

            [Tooltip("Current battle time (in seconds)")]
            // GameManager stores currentBattleTime so LevelSelect can set it
            public float currentBattleTime = 180;

            [Tooltip("Current selected level")]
            // Whenever a level is selected, this field should be populated
            public Level currentLevel = null;

        // IN FUTURE: Collection should be tied to player profile?
        [Header("Collection Data")]
        // CollectionData loaded in GameManager
            [Tooltip("CollectionData of current player")]
            public CollectionData currPlayerCollectionData = new();
            
            // Test CollectionData for debug
            private CollectionData testCollectionData = new();

        // Debug mode for GameManager
        [Tooltip("Toggle whether Test Collection Data is being used")]
        public bool IsUsingTestData = false;

        // Tracks the current game state
        [SerializeField] public GameState State { get; private set; }
        public static event Action<GameState> OnGameStateChanged;

        // Triggers when save data is loaded
        public event Action OnSaveDataLoaded;

        #region Game State Events
        [Header("Events to Trigger")]
            // Triggers when the player switches to the title screen
            public UnityEvent<GameState> ChangeToTitle = new UnityEvent<GameState>();

            // Triggers when the player switches to the level select screen
            public UnityEvent<GameState> ChangeToLevelSelect = new UnityEvent<GameState>();

            // Triggers when the player initiates a battle
            public UnityEvent<GameState> ChangeToBattle = new UnityEvent<GameState>();

            // Triggers when the Collection scene is entered
            public UnityEvent<GameState> ChangeToCollection = new UnityEvent<GameState>();
        #endregion


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

            OnGameStateChanged?.Invoke(newState);
            Debug.Log($"{nameof(GameManager)}: GameState: {State.ToString()}");
        }

        // Save the collection given a filename
        public void Save(string filename, CollectionData data)
        {
            if (data.IsEmpty())
            {
                Debug.Log($"{nameof(GameManager)}: No data to save, will not overwrite {filename}.");
                return;
            }
            FileManager.Save(filename, data);
        }

        // Load the collection given a filename
        public void Load(string filename) 
        {
            currPlayerCollectionData = FileManager.Load<CollectionData>(filename);
            OnSaveDataLoaded?.Invoke();
        }

        // Debug function: Removes all save data
        public void DeleteSaveData()
        {
            if (!IsUsingTestData)
            {
                currPlayerCollectionData.Clear();
                Save("data", currPlayerCollectionData);
            }
            else
            {
                testCollectionData.Clear();
                Save("test", testCollectionData);
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

            if (IsUsingTestData)
            {
                LoadRandomSaveData(10);
                Save("test", currPlayerCollectionData);
            }
        }

        private void Start()
        {
            // Initial state
            UpdateGameState(GameState.Title);

            // Testing section: Loads in the testing data
            if (!IsUsingTestData)
            {
                // Load data file or create a new one
                if (File.Exists(Path.Combine(Application.persistentDataPath, "data.json")))
                {
                    Debug.Log($"{nameof(GameManager)}: Found player data");
                    if (currPlayerCollectionData != null && currPlayerCollectionData.Count() > 0) currPlayerCollectionData.Clear();
                    Load("data");
                }
                else
                {
                    Debug.Log($"{nameof(GameManager)}: Did not find player data; Creating new save data");
                    Save("data", currPlayerCollectionData);
                }
            }
            else
            {
                Debug.Log(Application.persistentDataPath);
                // Load Test Data
                if (File.Exists(Path.Combine(Application.persistentDataPath, "test.json")))
                {
                    Debug.Log($"{nameof(GameManager)}: Found test data, loading...");
                    if (currPlayerCollectionData != null && currPlayerCollectionData.Count() > 0) currPlayerCollectionData.Clear();
                    Load("test");
                    Debug.Log($"{nameof(GameManager)}: Data loaded");
                }
                else Debug.LogError($"{nameof(GameManager)}: Test data could not be found");
            }
        }

        private void OnApplicationQuit()
        {
            // Save the data
            if (!IsUsingTestData) Save("data", currPlayerCollectionData);
        }

        private void LoadRandomSaveData(int amountOfFigures)
        {
            // Create a new collection and a checking list for already added figures
            List<CollectedFigure> randomCollection = new();
            List<Figure> randomFigures = new();

            // Create a set amount of random figures
            for (int j = 0; j < amountOfFigures; j++)
            {
                CollectedFigure randomCollectedFigure = new();
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
            currPlayerCollectionData = new CollectionData(randomCollection);
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