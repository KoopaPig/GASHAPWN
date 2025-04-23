using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;
using EasyTransition;
using GASHAPWN.Audio;

namespace GASHAPWN.UI {
    public class ConfirmationWindow : MonoBehaviour
    {
        public Button yesButton;
        public Button noButton;
        public TextMeshProUGUI messageText;
        private InputAction cancelAction;


        //private void OnEnable()
        //{
        //    var inputActionAsset = GetComponentInParent<PlayerInput>().actions;
        //    cancelAction = inputActionAsset["Cancel"];

        //    cancelAction.performed += HandleCancel;
        //    if (!cancelAction.enabled) { cancelAction.Enable(); }
        //}

        //private void HandleCancel(InputAction.CallbackContext context)
        //{
        //    this.gameObject.SetActive(false);
        //}

        //private void OnDisable()
        //{
        //    cancelAction.performed -= HandleCancel;
        //    cancelAction.Disable();
        //}
    }
}



