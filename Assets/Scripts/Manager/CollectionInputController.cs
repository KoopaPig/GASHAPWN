using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

namespace GASHAPWN
{
    public class CollectionInputController : MonoBehaviour
    {
        [Header("Dependencies")]
        [SerializeField] private CollectionManager collectionManager;
        [SerializeField] private PlayerInput playerInput;
        [SerializeField] private InputActionAsset inputActions;
        
        [Header("Debug")]
        [SerializeField] private bool debugLog = true;
        
        private bool isInitialized = false;
        private float rotationValue = 0f;
        private float navigateCooldown = 0.2f;
        private float navigateTimer = 0f;
        
        private void Awake()
        {
            if (collectionManager == null)
                collectionManager = FindFirstObjectByType<CollectionManager>();
                
            if (playerInput == null)
                playerInput = GetComponent<PlayerInput>();
            
            if (playerInput == null)
            {
                Debug.LogError("CollectionInputController: No PlayerInput component found! Adding one...");
                playerInput = gameObject.AddComponent<PlayerInput>();
            }
        }
        
        private void Start()
        {
            StartCoroutine(InitializeInputs());
        }
        
        private IEnumerator InitializeInputs()
        {
            // Wait for ControllerManager to be fully initialized
            yield return new WaitForSeconds(0.5f);
            
            try
            {
                // Set up input actions
                if (playerInput.actions == null)
                {
                    // Try to get input actions from the serialized field first
                    if (inputActions != null)
                    {
                        DebugLog("Using serialized input actions");
                        playerInput.actions = inputActions;
                    }
                    // Then try to find a Player Input Actions asset in Resources
                    else
                    {
                        DebugLog("Looking for input actions in Resources");
                        InputActionAsset resourceActions = Resources.Load<InputActionAsset>("Player");
                        if (resourceActions != null)
                        {
                            playerInput.actions = resourceActions;
                            DebugLog("Found input actions in Resources");
                        }
                        else
                        {
                            Debug.LogWarning("Could not find input actions. Please assign in inspector.");
                        }
                    }
                }
                
                if (playerInput.actions != null)
                {
                    // Determine action map to use
                    string actionMapName = "Keyboard"; // Default
                    if (Gamepad.current != null)
                    {
                        actionMapName = "Gamepad";
                        DebugLog($"Controller detected, using {actionMapName} action map");
                    }
                    
                    // Switch to appropriate action map
                    playerInput.SwitchCurrentActionMap(actionMapName);
                    DebugLog($"Switched to action map: {playerInput.currentActionMap?.name}");
                    
                    // Set up action callbacks
                    SetupActionCallbacks();
                }
                else
                {
                    Debug.LogError("No input actions available - input will not work!");
                }
                
                isInitialized = true;
                DebugLog("Initialization complete");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error initializing input: {e.Message}\n{e.StackTrace}");
            }
        }
        
        private void SetupActionCallbacks()
        {
            // Find relevant actions
            var moveAction = playerInput.actions.FindAction("Movement");
            var rotateAction = playerInput.actions.FindAction("RotateChargeDirection");
            var jumpAction = playerInput.actions.FindAction("Jump");
            
            // Unbind existing callbacks
            if (moveAction != null)
            {
                moveAction.performed -= OnMove;
                moveAction.canceled -= OnMoveCanceled;
                
                // Bind new callbacks
                moveAction.performed += OnMove;
                moveAction.canceled += OnMoveCanceled;
                moveAction.Enable();
                DebugLog($"Movement action bound: {moveAction.name}");
            }
            else
            {
                Debug.LogWarning("Movement action not found");
            }
            
            if (rotateAction != null)
            {
                rotateAction.performed -= OnRotate;
                rotateAction.canceled -= OnRotateCanceled;
                
                // Bind new callbacks
                rotateAction.performed += OnRotate;
                rotateAction.canceled += OnRotateCanceled;
                rotateAction.Enable();
                DebugLog($"Rotate action bound: {rotateAction.name}");
            }
            else
            {
                Debug.LogWarning("Rotate action not found");
            }
            
            if (jumpAction != null)
            {
                jumpAction.performed -= OnJump;
                
                // Bind new callbacks
                jumpAction.performed += OnJump;
                jumpAction.Enable();
                DebugLog($"Jump action bound: {jumpAction.name}");
            }
            else
            {
                Debug.LogWarning("Jump action not found");
            }
        }
        
        private void Update()
        {
            if (!isInitialized) return;
            
            // Update navigation cooldown timer
            if (navigateTimer > 0)
                navigateTimer -= Time.deltaTime;
                
            // Apply continuous rotation if value is non-zero
            if (Mathf.Abs(rotationValue) > 0.01f && collectionManager != null)
            {
                collectionManager.RotateFigure(rotationValue * Time.deltaTime);
            }
        }
        
        // Movement handler - used for navigation between nodes
        private void OnMove(InputAction.CallbackContext context)
        {
            if (!isInitialized || collectionManager == null) return;
            
            Vector2 input = context.ReadValue<Vector2>();
            
            // Only navigate when input exceeds threshold and cooldown is complete
            if (navigateTimer <= 0)
            {
                if (Mathf.Abs(input.x) > 0.5f)
                {
                    if (input.x > 0)
                        collectionManager.NavigateNext();
                    else
                        collectionManager.NavigatePrevious();
                    
                    // Set cooldown to prevent rapid navigation
                    navigateTimer = navigateCooldown;
                    DebugLog($"Navigation: {(input.x > 0 ? "Next" : "Previous")}");
                }
            }
        }
        
        private void OnMoveCanceled(InputAction.CallbackContext context)
        {
            // Movement input stopped - nothing to do
        }
        
        // Rotation handler - used to rotate the figure
        private void OnRotate(InputAction.CallbackContext context)
        {
            if (!isInitialized || collectionManager == null) return;
            
            // Read input value and process based on context
            Vector2 input = context.ReadValue<Vector2>();
            
            // Different handling based on action map
            if (playerInput.currentActionMap.name == "Gamepad")
            {
                // For controller, we'll use the X-axis directly
                rotationValue = input.x * 100f; // Scale factor for rotation speed
            }
            else
            {
                // For keyboard, use the X-axis
                rotationValue = input.x * 100f;
            }
            
            DebugLog($"Rotation value: {rotationValue}");
        }
        
        private void OnRotateCanceled(InputAction.CallbackContext context)
        {
            rotationValue = 0f;
        }
        
        // Jump handler - used for selection
        private void OnJump(InputAction.CallbackContext context)
        {
            if (!isInitialized || collectionManager == null) return;
            
            DebugLog("Selection performed");
            Audio.UI_SFXManager.Instance.Play_GeneralButtonSelection();
        }
        
        private void OnDisable()
        {
            // Clean up any callbacks
            if (playerInput != null && playerInput.actions != null)
            {
                var moveAction = playerInput.actions.FindAction("Movement");
                if (moveAction != null)
                {
                    moveAction.performed -= OnMove;
                    moveAction.canceled -= OnMoveCanceled;
                }
                
                var rotateAction = playerInput.actions.FindAction("RotateChargeDirection");
                if (rotateAction != null)
                {
                    rotateAction.performed -= OnRotate;
                    rotateAction.canceled -= OnRotateCanceled;
                }
                
                var jumpAction = playerInput.actions.FindAction("Jump");
                if (jumpAction != null)
                {
                    jumpAction.performed -= OnJump;
                }
            }
        }
        
        private void DebugLog(string message)
        {
            if (debugLog)
                Debug.Log($"CollectionInputController: {message}");
        }
    }
}