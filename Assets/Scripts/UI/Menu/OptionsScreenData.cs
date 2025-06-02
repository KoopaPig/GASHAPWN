using GASHAPWN.Audio;
using GASHAPWN.Utility;
using UnityEngine;
using UnityEngine.EventSystems;

namespace GASHAPWN.UI
{
    /// <summary>
    /// Controller for Data-related options
    /// </summary>
    public class OptionsScreenData : MonoBehaviour
    {
        [Tooltip("Confirmation Window for sensitive options")]
        [SerializeField] private ConfirmationWindow confirmWindow;

        private GameObject lastSelectedObject = null;


        /// PRIVATE METHODS ///

        private void OnEnable()
        {
            confirmWindow.gameObject.SetActive(false);
        }

        // Actions when opening Confirmation Window
        private void OpenConfirmationWindow(string message)
        {
            confirmWindow.gameObject.SetActive(true);
            confirmWindow.yesButton.onClick.AddListener(ClickYes);
            confirmWindow.noButton.onClick.AddListener(ClickNo);
            confirmWindow.messageText.text = message;
            UI_SFXManager.Instance.Play_WarningPopup();
        }

        // Actions when clicking "Yes" in Confirmation Window
        private void ClickYes()
        {
            GameManager.Instance.DeleteSaveData(); // delete save data
            CloseConfirmationWindow();
        }

        // Actions when clicking "No" in Confirmation Window

        private void ClickNo()
        {
            CloseConfirmationWindow();
        }

        // Actions when closing Confirmation Window
        private void CloseConfirmationWindow()
        {
            EventSystemSelectHelper.SetSelectedGameObject(lastSelectedObject);
            confirmWindow.yesButton.onClick.RemoveListener(ClickYes);
            confirmWindow.noButton.onClick.RemoveListener(ClickNo);
            confirmWindow.gameObject.SetActive(false);
        }


        /// PUBLIC METHODS ///

        /// <summary>
        /// Handle what happens when pressing "Delete Save Data" in Options
        /// </summary>
        public void OnDeleteDataGUI()
        {
            OpenConfirmationWindow("Are you sure you want to delete save data?");
            lastSelectedObject = EventSystem.current.currentSelectedGameObject;
            EventSystemSelectHelper.SetSelectedGameObject(confirmWindow.noButton.gameObject);
        }
    }
}