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
        private bool _uiStickInUse = false;

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
            Vector2 dir = context.ReadValue<Vector2>();

            // Deadzone threshold
            const float threshold = 0.5f;

            if (dir.magnitude < threshold)
            {
                _uiStickInUse = false;
                return;
            }

            if (_uiStickInUse)
                return;

            if (dir.x <= -threshold)
            {
                if (this.gameObject == EventSystem.current.currentSelectedGameObject)
                {
                    leftButton.onClick.Invoke();
                }
                _uiStickInUse = true;
            }
            else if (dir.x >= threshold)
            {
                if (this.gameObject == EventSystem.current.currentSelectedGameObject)
                {
                    rightButton.onClick.Invoke();
                }
                _uiStickInUse = true;
            }
        }
    }
}