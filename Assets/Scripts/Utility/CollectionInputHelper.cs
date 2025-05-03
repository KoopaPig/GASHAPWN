//using UnityEngine;
//using UnityEngine.InputSystem;

//namespace GASHAPWN
//{
//    /// <summary>
//    /// Helper class to set up input references for the collection scene
//    /// Integrates with existing ControllerManager
//    /// </summary>
//    public class CollectionInputHelper : MonoBehaviour
//    {
//        [Header("References")]
//        [SerializeField] private CollectionManager collectionManager;
//        [SerializeField] private PlayerInput playerInput;

//        [Header("Action References")]
//        [SerializeField] private InputActionReference navigateAction;
//        [SerializeField] private InputActionReference rotateAction;
//        [SerializeField] private InputActionReference selectAction;
//        [SerializeField] private InputActionReference cancelAction;

//        private void Awake()
//        {
//            // Find references if not set
//            if (collectionManager == null)
//                collectionManager = FindFirstObjectByType<CollectionManager>();

//            if (playerInput == null)
//                playerInput = GetComponent<PlayerInput>();

//            if (playerInput == null)
//                playerInput = gameObject.AddComponent<PlayerInput>();

//            Debug.Log("CollectionInputHelper: Awake complete");
//        }

//        private void Start()
//        {
//            Debug.Log("CollectionInputHelper: Starting setup");

//            // Set up input references based on ControllerManager if possible
//            if (ControllerManager.Instance != null)
//            {
//                SetupInputFromControllerManager();
//            }
//            else
//            {
//                Debug.LogWarning("CollectionInputHelper: ControllerManager not found, using default setup");
//                SetupDefaultInput();
//            }

//            // Assign actions to collection manager - direct field assignment
//            if (collectionManager != null)
//            {
//                // Check if the fields are public
//                var navigateField = collectionManager.GetType().GetField("navigateAction");
//                var rotateField = collectionManager.GetType().GetField("rotateAction");
//                var selectField = collectionManager.GetType().GetField("selectAction");

//                if (navigateField != null && rotateField != null && selectField != null)
//                {
//                    // Fields are public, we can assign directly
//                    Debug.Log("CollectionInputHelper: Fields are public, assigning directly");

//                    collectionManager.navigateAction = navigateAction;
//                    collectionManager.rotateAction = rotateAction;
//                    collectionManager.selectAction = selectAction;
//                }
//                else
//                {
//                    // Fields are private, we need to use reflection
//                    Debug.Log("CollectionInputHelper: Fields are private, using reflection");

//                    navigateField = collectionManager.GetType().GetField("navigateAction",
//                        System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);

//                    rotateField = collectionManager.GetType().GetField("rotateAction",
//                        System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);

//                    selectField = collectionManager.GetType().GetField("selectAction",
//                        System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);

//                    if (navigateField != null) navigateField.SetValue(collectionManager, navigateAction);
//                    if (rotateField != null) rotateField.SetValue(collectionManager, rotateAction);
//                    if (selectField != null) selectField.SetValue(collectionManager, selectAction);
//                }

//                // Add direct field assignment as backup
//                // Find navigate action
//                var navigate = playerInput.actions.FindAction("Navigate");
//                if (navigate != null)
//                {
//                    var navigateRef = InputActionReference.Create(navigate);
//                    navigateField?.SetValue(collectionManager, navigateRef);
//                    Debug.Log("Navigate action assigned directly: " + navigate.name);
//                }

//                // Find rotate action
//                var rotate = playerInput.actions.FindAction("Rotate") ??
//                            playerInput.actions.FindAction("RotateChargeDirection");
//                if (rotate != null)
//                {
//                    var rotateRef = InputActionReference.Create(rotate);
//                    rotateField?.SetValue(collectionManager, rotateRef);
//                    Debug.Log("Rotate action assigned directly: " + rotate.name);
//                }

//                // Find select action
//                var select = playerInput.actions.FindAction("Submit") ??
//                            playerInput.actions.FindAction("Jump");
//                if (select != null)
//                {
//                    var selectRef = InputActionReference.Create(select);
//                    selectField?.SetValue(collectionManager, selectRef);
//                    Debug.Log("Select action assigned directly: " + select.name);
//                }

//                Debug.Log($"Collection Manager configured with input actions");
//            }
//            else
//            {
//                Debug.LogError("CollectionInputHelper: Collection Manager not found!");
//            }

//            // Debug information
//            Debug.Log($"Navigate action assigned: {navigateAction != null}");
//            Debug.Log($"Rotate action assigned: {rotateAction != null}");
//            Debug.Log($"Select action assigned: {selectAction != null}");
//            Debug.Log($"Current action map: {playerInput.currentActionMap?.name}");
//        }

//        /// <summary>
//        /// Set up input based on existing ControllerManager input settings
//        /// </summary>
//        private void SetupInputFromControllerManager()
//        {
//            // Get the input action asset from ControllerManager
//            var assignment = PlayerInputAssigner.Instance.FindPlayerAssignment("Player1");
//            if (assignment != null && assignment.isAssigned)
//            {
//                Debug.Log("CollectionInputHelper: Found Player1 assignment");

//                // Use the same input action asset
//                playerInput.actions = assignment.playerInput.actions;

//                // Switch to the same action map
//                playerInput.SwitchCurrentActionMap(PlayerInputAssigner.ControlSchemeToString(assignment.controlScheme));

//                // Get references to the actions
//                navigateAction = InputActionReference.Create(playerInput.actions.FindAction("Navigate"));

//                // For rotation, try different possible names
//                var rotateActionObj = playerInput.actions.FindAction("Rotate", true) ??
//                                     playerInput.actions.FindAction("RotateChargeDirection");

//                if (rotateActionObj == null)
//                {
//                    // If no rotation action exists, create one
//                    rotateActionObj = CreateRotateAction(playerInput.currentActionMap);
//                }

//                rotateAction = InputActionReference.Create(rotateActionObj);

//                // For selection, try different possible names
//                selectAction = InputActionReference.Create(
//                    playerInput.actions.FindAction("Submit") ??
//                    playerInput.actions.FindAction("Jump") ??
//                    playerInput.actions.FindAction("Select")
//                );

//                cancelAction = InputActionReference.Create(playerInput.actions.FindAction("Cancel"));

//                Debug.Log("CollectionInputHelper: Set up input from ControllerManager");
//                Debug.Log($"Navigate action: {navigateAction?.action?.name}");
//                Debug.Log($"Rotate action: {rotateAction?.action?.name}");
//                Debug.Log($"Select action: {selectAction?.action?.name}");
//            }
//            else
//            {
//                Debug.LogWarning("CollectionInputHelper: No Player1 assignment found in ControllerManager");
//                SetupDefaultInput();
//            }
//        }

//        /// <summary>
//        /// Creates a rotation action if it doesn't exist
//        /// </summary>
//        private InputAction CreateRotateAction(InputActionMap actionMap)
//        {
//            if (actionMap == null)
//                return null;

//            Debug.Log($"Creating Rotate action in map: {actionMap.name}");

//            // Create new Rotate action if it doesn't exist
//            var rotateAction = actionMap.AddAction("Rotate", InputActionType.Value);

//            // Add bindings based on action map
//            if (actionMap.name.Contains("Keyboard"))
//            {
//                rotateAction.AddBinding("<Keyboard>/q").WithProcessor("scale(-1)");
//                rotateAction.AddBinding("<Keyboard>/e");
//                rotateAction.AddBinding("<Mouse>/rightButton").WithProcessor("scale(1)");
//                rotateAction.AddBinding("<Mouse>/leftButton").WithProcessor("scale(-1)");
//            }
//            else if (actionMap.name.Contains("Gamepad"))
//            {
//                rotateAction.AddBinding("<Gamepad>/leftTrigger").WithProcessor("scale(-1)");
//                rotateAction.AddBinding("<Gamepad>/rightTrigger");
//            }

//            rotateAction.Enable();
//            Debug.Log($"Created and enabled Rotate action with {rotateAction.bindings.Count} bindings");

//            return rotateAction;
//        }

//        /// <summary>
//        /// Set up default input if ControllerManager is not available
//        /// </summary>
//        private void SetupDefaultInput()
//        {
//            Debug.Log("CollectionInputHelper: Setting up default input");

//            // Create a new input actions asset
//            var inputActions = ScriptableObject.CreateInstance<InputActionAsset>();

//            // Create keyboard and controller action maps
//            var keyboardMap = new InputActionMap("Keyboard");
//            var controllerMap = new InputActionMap("Gamepad");

//            inputActions.AddActionMap(keyboardMap);
//            inputActions.AddActionMap(controllerMap);

//            // Create keyboard navigation action
//            var navigateKeyboard = keyboardMap.AddAction("Navigate", InputActionType.Value);
//            navigateKeyboard.AddCompositeBinding("2DVector")
//                .With("Up", "<Keyboard>/w")
//                .With("Down", "<Keyboard>/s")
//                .With("Left", "<Keyboard>/a")
//                .With("Right", "<Keyboard>/d");

//            // Create controller navigation action
//            var navigateController = controllerMap.AddAction("Navigate", InputActionType.Value);
//            navigateController.AddBinding("<Gamepad>/leftStick");
//            navigateController.AddBinding("<Gamepad>/dpad");

//            // Create rotation actions
//            var rotateKeyboard = keyboardMap.AddAction("Rotate", InputActionType.Value);
//            rotateKeyboard.AddBinding("<Keyboard>/q").WithProcessor("scale(-1)");
//            rotateKeyboard.AddBinding("<Keyboard>/e");
//            rotateKeyboard.AddBinding("<Mouse>/rightButton").WithProcessor("scale(1)");
//            rotateKeyboard.AddBinding("<Mouse>/leftButton").WithProcessor("scale(-1)");

//            var rotateController = controllerMap.AddAction("Rotate", InputActionType.Value);
//            rotateController.AddBinding("<Gamepad>/leftTrigger").WithProcessor("scale(-1)");
//            rotateController.AddBinding("<Gamepad>/rightTrigger");

//            // Create select actions
//            var selectKeyboard = keyboardMap.AddAction("Submit", InputActionType.Button);
//            selectKeyboard.AddBinding("<Keyboard>/space");
//            selectKeyboard.AddBinding("<Keyboard>/enter");

//            var selectController = controllerMap.AddAction("Submit", InputActionType.Button);
//            selectController.AddBinding("<Gamepad>/buttonSouth");

//            // Create cancel actions
//            var cancelKeyboard = keyboardMap.AddAction("Cancel", InputActionType.Button);
//            cancelKeyboard.AddBinding("<Keyboard>/escape");

//            var cancelController = controllerMap.AddAction("Cancel", InputActionType.Button);
//            cancelController.AddBinding("<Gamepad>/buttonEast");

//            // Assign to player input
//            playerInput.actions = inputActions;

//            // Set action map based on connected devices
//            playerInput.SwitchCurrentActionMap(Gamepad.current != null ? "Gamepad" : "Keyboard");

//            // Enable the action maps
//            keyboardMap.Enable();
//            controllerMap.Enable();

//            // Create action references
//            navigateAction = InputActionReference.Create(playerInput.actions.FindAction("Navigate"));
//            rotateAction = InputActionReference.Create(playerInput.actions.FindAction("Rotate"));
//            selectAction = InputActionReference.Create(playerInput.actions.FindAction("Submit"));
//            cancelAction = InputActionReference.Create(playerInput.actions.FindAction("Cancel"));

//            Debug.Log("CollectionInputHelper: Default input setup complete");
//            Debug.Log($"Navigate action created: {navigateAction?.action?.name}");
//            Debug.Log($"Rotate action created: {rotateAction?.action?.name}");
//        }
//    }
//}