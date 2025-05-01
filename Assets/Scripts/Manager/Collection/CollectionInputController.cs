using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using EasyTransition;
using UnityEngine.InputSystem.Composites;

namespace GASHAPWN
{
    [RequireComponent(typeof(CollectionManager))]
    public class CollectionInputController : MonoBehaviour
    {
        [Header("Debug")]
        [SerializeField] private bool debugLog = true;

        private CollectionManager collectionManager;
        private InputAction cancelAction;

        private bool isInitialized = false;
        private float rotationValue = 0f;
        private float navigateCooldown = 0.2f;
        private float navigateTimer = 0f;
        
        private void Awake()
        {
            collectionManager = GetComponent<CollectionManager>();
        }

        private void OnEnable()
        {
            var inputActionAsset = GetComponent<PlayerInput>().actions;
            cancelAction = inputActionAsset["Cancel"];

            cancelAction.performed += HandleCancel;
            if (!cancelAction.enabled) { cancelAction.Enable(); }
        }


        private void Update()
        {
            //if (!isInitialized) return;
            
            // Update navigation cooldown timer
            if (navigateTimer > 0)
                navigateTimer -= Time.deltaTime;
                
            // Apply continuous rotation if value is non-zero
            if (Mathf.Abs(rotationValue) > 0.01f && collectionManager != null)
            {
                collectionManager.RotateFigure(rotationValue * Time.deltaTime);
            }
        }
        
        public void OnNavigate(InputAction.CallbackContext context)
        {

            //if (!isInitialized || collectionManager == null) return;
            
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
        
        // Rotation handler - used to rotate the figure
        public void OnRotate(InputAction.CallbackContext context)
        {
            // Get the raw rotation value from the input device
            rotationValue = context.ReadValue<float>() * 100f;

            // Debug.Log to verify we're getting both positive and negative values
            DebugLog($"Rotation value: {rotationValue}");
        }
        
        private void OnRotateCanceled(InputAction.CallbackContext context)
        {
            rotationValue = 0f;
        }
        
        public void OnSubmit(InputAction.CallbackContext context)
        {
            //if (!isInitialized || collectionManager == null) return;
            
            DebugLog("Selection performed");
            Audio.UI_SFXManager.Instance.Play_GeneralButtonSelection();
        }

        private void DebugLog(string message)
        {
            if (debugLog)
                Debug.Log($"CollectionInputController: {message}");
        }

        private void HandleCancel(InputAction.CallbackContext context)
        {
            TransitionManager.Instance().Transition(collectionManager.mainMenuSceneName, collectionManager.collectionTransition, 0);
            GameManager.Instance.UpdateGameState(GameState.Title);
        }

        private void OnDisable()
        {
            cancelAction.performed -= HandleCancel;
            cancelAction.Disable();
        }
    }
}