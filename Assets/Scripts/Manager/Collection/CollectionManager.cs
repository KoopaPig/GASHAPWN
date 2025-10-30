using GASHAPWN.UI;
using System.Collections;
using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;


namespace GASHAPWN {
    /// <summary>
    /// Manages collection nodes and cameras in Collection scene
    /// </summary>
    public class CollectionManager : MonoBehaviour
    {
        public static CollectionManager Instance;

        [Header("Camera References & Settings")]
            [Tooltip("Cinemachine Cam for viewing Figure nodes")]
            [SerializeField] private CinemachineCamera nodeCam;

            [Tooltip("Cinemachine Cam for intro animation")]
            [SerializeField] private CinemachineCamera introCam;

            private Animator introCamAnimator;

        [Header("Scene References")]
            [Tooltip("Toggle for intro animation")]
            [SerializeField] public bool isIntroAnimation = true;

            [Tooltip("Reference to animator for door")]
            [SerializeField] private Animator doorAnimator;

            [Tooltip("Nodes parent transform")]
            [SerializeField] private Transform nodesParent;

        [Header("UI References")]
            [SerializeField] private CollectionGUI collectionGUI;

        [Header("Rotation Settings")]
            [SerializeField] private float rotationSpeed = 100f;

        // NODE MANAGEMENT
        private CollectionNode currentNode;
        private List<CollectionNode> collectionNodes = new List<CollectionNode>();
        private float currentRotationInput = 0f;

        // Whether the collection system is initialized and ready
        private bool initialized = false;


        /// PUBLIC METHODS ///

        /// <summary>
        /// Initialize the collection with player data
        /// </summary>
        public void InitializeCollection(List<CollectedFigure> collectedFigures)
        {
            if (initialized)
                return;

            // Find all collection nodes in the scene
            FindAllNodes();

            // Verify that nodes are properly linked
            VerifyNodeConnections();

            // Update nodes based on collected figures
            UpdateNodesFromCollection(collectedFigures);

            // Display all Figures
            DisplayAllFigures();


            initialized = true;
        }

        /// <summary>
        /// Select a specific collection node
        /// </summary>
        public void SelectNode(CollectionNode node)
        {
            if (node == null) return;

            // Make sure introCam animator is disabled when node is selected
            if (introCamAnimator != null && introCamAnimator.enabled)
                introCamAnimator.enabled = false;

            // Deactivate visual state of last node
            if (currentNode != null) { currentNode.UpdateVisualState(false); }

            // Set new node and activate visual state
            currentNode = node;
            currentNode.UpdateVisualState(true);

            // Enable NodeCam priority and set its target
            if (nodeCam != null)
            {
                nodeCam.Target.TrackingTarget = currentNode.cameraPosition;
                nodeCam.LookAt = currentNode.transform;
                // Update Priorities
                nodeCam.Priority = 20;
                introCam.Priority = 0;
            }

            // Display the figure
            currentNode.DisplayFigure();

            // Update UI with figure information
            if (collectionGUI != null)
            {
                collectionGUI.SwitchFigureGUI(currentNode.associatedFigure, currentNode.isCollected, currentNode.amount);
            }
        }

        /// <summary>
        /// Select a collection node by index
        /// </summary>
        public void SelectNode(int nodeIndex)
        {
            if (nodeIndex >= 0 && nodeIndex < collectionNodes.Count)
            {
                // Disable animator
                // Make sure introCam animator is disabled when node is selected
                if (introCamAnimator != null && introCamAnimator.enabled)
                    introCamAnimator.enabled = false;

                // Get the node
                CollectionNode node = collectionNodes[nodeIndex];

                // Deactivate visual state of last node
                if (currentNode != null) { currentNode.UpdateVisualState(false); }

                // Set new node and activate visual state
                currentNode = node;
                currentNode.UpdateVisualState(true);

                // Enable NodeCam priority and set its target
                if (nodeCam != null)
                {
                    nodeCam.Target.TrackingTarget = currentNode.cameraPosition;
                    nodeCam.LookAt = currentNode.transform;
                    // Update Priorities
                    nodeCam.Priority = 20;
                    introCam.Priority = 0;
                }

                // Display figure
                currentNode.DisplayFigure();

                // Update UI
                if (collectionGUI != null)
                {
                    collectionGUI.SwitchFigureGUI(currentNode.associatedFigure, currentNode.isCollected, currentNode.amount);
                }
            }
        }

        /// <summary>
        /// Navigate to the next node
        /// </summary>
        public void NavigateNext()
        {
            if (currentNode != null && currentNode.nextNode != null)
            {
                int nextIndex = collectionNodes.IndexOf(currentNode.nextNode);
                if (nextIndex >= 0)
                {
                    SelectNode(nextIndex);
                }
                else
                {
                    // Fallback to node-based select if index isn't found
                    SelectNode(currentNode.nextNode);
                }
                Audio.UI_SFXManager.Instance.Play_LeftRightButtonSelection();
            }
        }

        /// <summary>
        /// Navigate to the previous node
        /// </summary>
        public void NavigatePrevious()
        {
            if (currentNode != null && currentNode.previousNode != null)
            {
                int prevIndex = collectionNodes.IndexOf(currentNode.previousNode);
                if (prevIndex >= 0)
                {
                    SelectNode(prevIndex);
                }
                else
                {
                    // Fallback to node-based select if index isn't found
                    SelectNode(currentNode.previousNode);
                }
                Audio.UI_SFXManager.Instance.Play_LeftRightButtonSelection();
            }
        }

        /// <summary>
        /// Rotate the current figure given amount
        /// </summary>
        public void RotateFigure(float amount)
        {
            if (currentNode != null)
            {
                currentNode.RotateFigure(amount * rotationSpeed * Time.deltaTime);
            }
        }

        /// <summary>
        /// Calls DisplayFigure() on all nodes
        /// </summary>
        public void DisplayAllFigures()
        {
            foreach (var node in collectionNodes) node.DisplayFigure();
        }

        /// PRIVATE METHODS ///

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(this);
                return;
            }

            introCamAnimator = introCam.GetComponent<Animator>();

            if (collectionGUI == null)
                collectionGUI = FindFirstObjectByType<CollectionGUI>();

            // Set camera priorities depending on whether intro is toggled
            if (isIntroAnimation) {
                introCam.Priority = 20;
                nodeCam.Priority = 0;
            } else
            {
                introCam.Priority = 0;
                nodeCam.Priority = 20;
            }
        }

        private void OnEnable()
        {
            GameManager.Instance.OnSaveDataLoaded += InitializeFromGameManager;
        }

        private void Start()
        {
            // Start with UI inactive
            collectionGUI.CollectionGUISetActive(false);

            // Handle Intro Animation, then initialize
            if (isIntroAnimation) {
                StartCoroutine(PlayIntroSequence());
            } 
            // Just initialize
            else {
                InitializeFromGameManager();
                SelectFirstCollectedNode();
                collectionGUI.CollectionGUISetActive(true);
            }
            // If still not initialized, failsafe
            if (!initialized)
            {
                FailsafeInitialize();
            }
        }

        private void Update()
        {
            // Handle continuous rotation input
            if (currentRotationInput != 0 && currentNode != null)
            {
                currentNode.RotateFigure(currentRotationInput * rotationSpeed * UnityEngine.Time.deltaTime);
            }
        }

        private void OnDisable()
        {
            GameManager.Instance.OnSaveDataLoaded -= InitializeFromGameManager;
        }

        // Initialize collection data from GameManager
        private void InitializeFromGameManager()
        {
            if (GameManager.Instance != null)
            {
                // IN FUTURE: Select corresponding CollectionData corresponding to profile
                InitializeCollection(GameManager.Instance.currPlayerCollectionData.collection);
            }
            else
            {
                Debug.LogError("CollectionManager: GameManager not found!");
            }
        }

        // If collection did not initialize, failsafe method
        private void FailsafeInitialize()
        {     
            // Only force select if no node is currently selected or if we're not on the correct first node
            if (currentNode == null || 
                (collectionNodes.Count > 0 && currentNode != collectionNodes[0]))
            {
                Debug.Log("CollectionManager: Failsafe: Forcing selection of first node");
                SelectNode(0);  // Always select first node
            }
        }

        // Find all collection nodes in the scene
        private void FindAllNodes()
        {
            collectionNodes.Clear();

            if (nodesParent != null)
            {
                // Find nodes under the specified parent
                CollectionNode[] nodes = nodesParent.GetComponentsInChildren<CollectionNode>();
                collectionNodes.AddRange(nodes);
            }
            else
            {
                // Find all nodes in the scene
                var nodes = FindObjectsByType<CollectionNode>(FindObjectsSortMode.InstanceID);
                collectionNodes.AddRange(nodes);
            }

            // Sort nodes by series and then by number in series
            collectionNodes.Sort((a, b) => {
                // First compare series names alphabetically
                int seriesComparison = string.Compare(
                    a.associatedFigure?.GetSeries()?.SeriesName ?? "", 
                    b.associatedFigure?.GetSeries()?.SeriesName ?? ""
                );
                
                if (seriesComparison != 0)
                    return seriesComparison;
                    
                // Then compare by number in series
                return a.associatedFigure?.GetNumberInSeries() ?? 0 - 
                    b.associatedFigure?.GetNumberInSeries() ?? 0;
            });
        }
        
        // Update nodes based on the player's collection
        private void UpdateNodesFromCollection(List<CollectedFigure> collectedFigures)
        {
            if(collectedFigures.Count == 0)
            {
                foreach(CollectionNode node in collectionNodes)
                {
                    node.isCollected = false;
                    node.UpdateVisualState(false);
                }
            }
            else
            {
                // Create a lookup for faster access
                Dictionary<string, CollectedFigure> collectedLookup = new Dictionary<string, CollectedFigure>();
                foreach (var item in collectedFigures)
                {
                    if (item.ID != null)
                    {
                        collectedLookup[item.ID] = item;
                    }
                }

                // Update each node
                foreach (CollectionNode node in collectionNodes)
                {
                    if (node.associatedFigure != null)
                    {
                        string figureId = node.associatedFigure.GetID();
                        if (collectedLookup.ContainsKey(figureId))
                        {
                            node.isCollected = true;
                            node.amount = collectedLookup[figureId].amount;
                        }
                        node.UpdateVisualState(false);
                    }
                }
            }
        }

        // Select the first collected node
        private void SelectFirstCollectedNode()
        {
            // Try to find a collected node in the first series
            string firstSeriesName = null;
            CollectionNode firstCollectedNode = null;
            
            foreach (CollectionNode node in collectionNodes)
            {
                if (node.associatedFigure == null || node.associatedFigure.GetSeries() == null)
                    continue;
                    
                string seriesName = node.associatedFigure.GetSeries().SeriesName;
                
                // Set the first series name if not already set
                if (firstSeriesName == null)
                    firstSeriesName = seriesName;
                    
                // If we're still in the first series and this node is collected
                if (seriesName == firstSeriesName && node.isCollected)
                {
                    firstCollectedNode = node;
                    break;  // Found our node, break out
                }
            }
            
            // If we found a collected node in the first series, select it
            if (firstCollectedNode != null)
            {
                SelectNode(firstCollectedNode);
                return;
            }
            
            // If no collected nodes found in first series, 
            // fall back to the first node regardless of collection status
            if (collectionNodes.Count > 0)
            {
                SelectNode(collectionNodes[0]);
            }
        }
        
        // Verify node connections to ensure proper navigation
        private void VerifyNodeConnections()
        {
            foreach (var node in collectionNodes)
            {
                // Check prev/next links
                if (node.nextNode != null && node.nextNode.previousNode != node)
                {
                    node.nextNode.previousNode = node;
                }
                
                if (node.previousNode != null && node.previousNode.nextNode != node)
                {
                    node.previousNode.nextNode = node;
                }
            }
        }

        // Plays intro animation sequence then initializes collection
        private IEnumerator PlayIntroSequence()
        {
            // Enable camera animator
            if (introCamAnimator != null)
            {
                introCamAnimator.enabled = true;
                introCamAnimator.SetBool("isCamIntro", true);
            }

            // Open door
            if (doorAnimator != null)
            {
                doorAnimator.SetTrigger("openDoor");
            }

            // Initialize collection while animations are still playing
            InitializeFromGameManager();

            // TODO: Wait until animation complete instead
            // Wait for animations to progress
            yield return new WaitForSeconds(6f);

            SelectFirstCollectedNode();

            // Activate UI
            collectionGUI.CollectionGUISetActive(true);

            // Disable animator after intro to prevent it from interfering with camera control
            if (introCamAnimator != null)
            {
                introCamAnimator.enabled = false;
            }
        }


        /// DEBUG ///

        // Debug function: Adds a collection of random figures and updates the nodes
        public void AddRandomCollection(int amountOfFigures)
        {
            // Create a new collection and a checking list for already added figures
            List<CollectedFigure> randomCollection = new();
            List<Figure> randomFigures = new();

            // Create a set amount of random figures
            for (int i = 0; i < amountOfFigures; i++)
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
            UpdateNodesFromCollection(randomCollection);
        }

        // Debug function: Removes the current collection from the nodes
        public void RemoveCollection()
        {
            foreach (CollectionNode node in collectionNodes)
            {
                if (node.associatedFigure != null)
                {
                    node.associatedFigure = null;
                    node.isCollected = false;
                    node.UpdateVisualState(false);
                }
            }
        }
    }
}