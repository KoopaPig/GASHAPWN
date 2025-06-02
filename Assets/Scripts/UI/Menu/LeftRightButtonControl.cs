using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace GASHAPWN.UI
{
    /// <summary>
    /// Defines a left/right composite button
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class LeftRightButtonControl : MonoBehaviour
    {
        [Tooltip("Left Arrow of LeftRightButton")]
        [SerializeField] private Button leftButton;
        [Tooltip("Right Arrow of LeftRightButton")]
        [SerializeField] private Button rightButton;

        private InputAction navigateAction;

        private void OnEnable()
        {
            // Have to get inputActions in parent first
            var inputActions = GetComponentInParent<PlayerInput>().actions;

            var uiActionMap = inputActions.FindActionMap("UI");
            navigateAction = uiActionMap.FindAction("Navigate");

            navigateAction.performed += OnNavigate;
            if (!navigateAction.enabled) navigateAction.Enable();
        }

        private void OnDisable()
        {
            navigateAction.performed -= OnNavigate;
        }

        // Handle navigation
        // ISSUE: This does not work very well with joystick
        private void OnNavigate(InputAction.CallbackContext context)
        {
            Vector2 input = context.ReadValue<Vector2>();

            if (input.x < 0)
            {
                if (this.gameObject == EventSystem.current.currentSelectedGameObject)
                {
                    leftButton.onClick.Invoke();
                }
            }
            else if (input.x > 0)
            {
                if (this.gameObject == EventSystem.current.currentSelectedGameObject)
                {
                    rightButton.onClick.Invoke();
                }
            }
        }
    }
}