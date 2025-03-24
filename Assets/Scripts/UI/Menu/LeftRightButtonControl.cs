using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace GASHAPWN.UI
{
    [RequireComponent(typeof(Button))]
    public class LeftRightButtonControl : MonoBehaviour
    {
        [SerializeField] private Button leftButton;
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

        private void OnNavigate(InputAction.CallbackContext context)
        {
            Vector2 input = context.ReadValue<Vector2>();

            // maybe edit this here
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
