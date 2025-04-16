using GASHAPWN.UI;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;

namespace GASHAPWN {
    public class CollectionManager : MonoBehaviour
    {
        public static CollectionManager Instance;

        [Header("Camera References & Settings")]
        [SerializeField] private Camera mainCamera;
        [SerializeField] private Animator mainCameraAnimator;
        [SerializeField] private float cameraMoveDuration = 0.8f;
        [SerializeField] private AnimationCurve cameraMoveCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

        [Header("Scene References")]
        [SerializeField] private Animator doorAnimator;
        [SerializeField] public bool isCamIntro = true;
        [SerializeField] private Transform nodesParent;

        [Header("UI References")]
        [SerializeField] private CollectionGUI collectionGUI;

        [Header("Input")]
        public InputActionReference navigateAction;
        public InputActionReference rotateAction;
        public InputActionReference selectAction;

        [Header("Rotation Settings")]
        [SerializeField] private float rotationSpeed = 100f;

        // Node management
        private CollectionNode currentNode;
        private List<CollectionNode> collectionNodes = new List<CollectionNode>();
        private Coroutine cameraMoveCoroutine;
        private bool isMoving = false;
        private float currentRotationInput = 0f;

        // Whether the collection system is initialized and ready
        private bool initialized = false;

        /// PUBLIC METHODS ///

        // Initialize the collection with player data
        public void InitializeCollection(List<GameManager.CollectedFigure> collectedFigures)
        {
            if (initialized)
                return;

            // Find all collection nodes in the scene
            FindAllNodes();

            // Update nodes based on collected figures
            UpdateNodesFromCollection(collectedFigures);

            // Select the first collected node
            SelectFirstCollectedNode();

            initialized = true;
        }

        // Selects a specific collection node
        public void SelectNode(CollectionNode node, bool animate = true)
        {
            if (node == null || isMoving)
                return;

            // Update node visual states
            if (currentNode != null)
                currentNode.UpdateVisualState(false);

            // Store the new current node
            currentNode = node;
            currentNode.UpdateVisualState(true);

            // Move camera to the node
            if (animate)
            {
                if (cameraMoveCoroutine != null)
                    StopCoroutine(cameraMoveCoroutine);

                cameraMoveCoroutine = StartCoroutine(MoveCamera(currentNode.transform));
            }
            else
            {
                // Instantly move camera
                mainCamera.transform.position = currentNode.transform.position;
                mainCamera.transform.rotation = currentNode.transform.rotation;
            }

            // Display the figure
            currentNode.DisplayFigure();

            // Update UI with figure information
            if (collectionGUI != null && currentNode.isCollected)
            {
                collectionGUI.SwitchFigureGUI(currentNode.associatedFigure);
            }
        }

        // Navigate to the next node
        public void NavigateNext()
        {
            if (currentNode != null && currentNode.nextNode != null)
            {
                Audio.UI_SFXManager.Instance.Play_LeftRightButtonSelection();
                SelectNode(currentNode.nextNode);
            }
        }

        // Navigate to the previous node
        public void NavigatePrevious()
        {
            if (currentNode != null && currentNode.previousNode != null)
            {
                Audio.UI_SFXManager.Instance.Play_LeftRightButtonSelection();
                SelectNode(currentNode.previousNode);
            }
        }

        // Rotate the current figure
        public void RotateFigure(float amount)
        {
            if (currentNode != null)
            {
                currentNode.RotateFigure(amount * rotationSpeed * Time.deltaTime);
            }
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

            // Find references if not set
            if (mainCamera == null)
                mainCamera = Camera.main;

            if (collectionGUI == null)
                collectionGUI = FindFirstObjectByType<CollectionGUI>();
        }

        private void Start()
        {
            // Start with UI inactive
            collectionGUI.CollectionGUISetActive(false);

            // Handle Camera Intro
            if (isCamIntro) {
                StartCoroutine(PlayIntroSequence());
            } else {
                InitializeFromGameManager();
                collectionGUI.CollectionGUISetActive(true);
            }
        }

        private void OnEnable()
        {
            // Set up input actions
            if (navigateAction != null)
            {
                navigateAction.action.performed += OnNavigate;
                navigateAction.action.Enable();
            }

            if (rotateAction != null)
            {
                rotateAction.action.performed += OnRotate;
                rotateAction.action.canceled += OnRotateCanceled;
                rotateAction.action.Enable();
            }

            if (selectAction != null)
            {
                selectAction.action.performed += OnSelect;
                selectAction.action.Enable();
            }
        }

        private void OnDisable()
        {
            // Clean up input actions
            if (navigateAction != null)
                navigateAction.action.performed -= OnNavigate;

            if (rotateAction != null)
            {
                rotateAction.action.performed -= OnRotate;
                rotateAction.action.canceled -= OnRotateCanceled;
            }

            if (selectAction != null)
                selectAction.action.performed -= OnSelect;
        }

        // Brute force testing
        private void Update()
        {
            // Handle continuous rotation input
            if (currentRotationInput != 0 && currentNode != null)
            {
                currentNode.RotateFigure(currentRotationInput * rotationSpeed * Time.deltaTime);
            }
            
            // Direct keyboard navigation
            if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
            {
                Debug.Log("Left key pressed - Navigate Previous");
                NavigatePrevious();
            }
            else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
            {
                Debug.Log("Right key pressed - Navigate Next");
                NavigateNext();
            }
            
            // Direct rotation controls
            if (Input.GetKey(KeyCode.Q))
            {
                RotateFigure(-1);
            }
            else if (Input.GetKey(KeyCode.E))
            {
                RotateFigure(1);
            }
        }

        #region Input Handlers

        private void OnNavigate(InputAction.CallbackContext context)
        {
            if (isMoving || currentNode == null)
                return;

            // Read the input value (expected to be a Vector2)
            Vector2 input = context.ReadValue<Vector2>();
            
            // Determine direction based on input
            if (input.x > 0.5f)
            {
                NavigateNext();
            }
            else if (input.x < -0.5f)
            {
                NavigatePrevious();
            }
        }

        private void OnRotate(InputAction.CallbackContext context)
        {
            // Get rotation input
            currentRotationInput = context.ReadValue<float>();
        }

        private void OnRotateCanceled(InputAction.CallbackContext context)
        {
            // Reset rotation when input is released
            currentRotationInput = 0f;
        }

        private void OnSelect(InputAction.CallbackContext context)
        {
            // Handle selection (if needed)
            if (currentNode != null && currentNode.isCollected)
            {
                Audio.UI_SFXManager.Instance.Play_GeneralButtonSelection();
            }
        }

        #endregion

        // Plays intro animation sequence
        private IEnumerator PlayIntroSequence()
        {
            // Enable camera animator
            if (mainCameraAnimator != null) {
                mainCameraAnimator.enabled = true;
                mainCameraAnimator.SetBool("isCamIntro", true);
            }
            
            // Open door
            if (doorAnimator != null) {
                doorAnimator.SetTrigger("openDoor");
            }

            // Wait for animations to progress
            yield return new WaitForSeconds(2.5f);

            // Initialize collection while animations are still playing
            InitializeFromGameManager();

            // Wait a bit more for animations to finish
            yield return new WaitForSeconds(1.5f);

            // Activate UI
            collectionGUI.CollectionGUISetActive(true);
        }

        // Initialize collection data from GameManager
        private void InitializeFromGameManager()
        {
            if (GameManager.Instance != null)
            {
                // Choose either Player1 or Player2 collection
                List<GameManager.CollectedFigure> playerCollection = GameManager.Instance.Player1Collection;
                InitializeCollection(playerCollection);
            }
            else
            {
                Debug.LogError("CollectionManager: GameManager not found!");
            }
        }

        // Coroutine to smoothly move the camera to a target transform
        private IEnumerator MoveCamera(Transform target)
        {
            isMoving = true;
            
            Vector3 startPosition = mainCamera.transform.position;
            Quaternion startRotation = mainCamera.transform.rotation;
            
            Vector3 targetPosition = target.position;
            Quaternion targetRotation = target.rotation;
            
            float elapsed = 0f;
            
            while (elapsed < cameraMoveDuration)
            {
                elapsed += Time.deltaTime;
                float normalizedTime = elapsed / cameraMoveDuration;
                float curveValue = cameraMoveCurve.Evaluate(normalizedTime);
                
                // Move and rotate camera
                mainCamera.transform.position = Vector3.Lerp(startPosition, targetPosition, curveValue);
                mainCamera.transform.rotation = Quaternion.Slerp(startRotation, targetRotation, curveValue);
                
                yield return null;
            }
            
            // Ensure we end at exactly the target
            mainCamera.transform.position = targetPosition;
            mainCamera.transform.rotation = targetRotation;
            
            isMoving = false;
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
                CollectionNode[] nodes = FindObjectsOfType<CollectionNode>();
                collectionNodes.AddRange(nodes);
            }

            Debug.Log($"CollectionManager: Found {collectionNodes.Count} collection nodes");
        }

        // Update nodes based on the player's collection
        private void UpdateNodesFromCollection(List<GameManager.CollectedFigure> collectedFigures)
        {
            // Create a lookup for faster access
            Dictionary<string, GameManager.CollectedFigure> collectedLookup = new Dictionary<string, GameManager.CollectedFigure>();
            foreach (var item in collectedFigures)
            {
                if (item.figure != null)
                {
                    collectedLookup[item.figure.GetID()] = item;
                }
            }

            // Update each node
            foreach (CollectionNode node in collectionNodes)
            {
                if (node.associatedFigure != null)
                {
                    string figureId = node.associatedFigure.GetID();
                    node.isCollected = collectedLookup.ContainsKey(figureId);
                    node.UpdateVisualState(false);
                }
            }
        }

        // Select the first collected node
        private void SelectFirstCollectedNode()
        {
            // Try to find a collected node
            foreach (CollectionNode node in collectionNodes)
            {
                if (node.isCollected)
                {
                    SelectNode(node, false);
                    return;
                }
            }

            // If no collected nodes found, select the first node
            if (collectionNodes.Count > 0)
            {
                SelectNode(collectionNodes[0], false);
            }
        }
    }
}