using UnityEngine;
using UnityEngine.EventSystems;

namespace GASHAPWN.UI
{
    public class OptionsScreenData : MonoBehaviour
    {
        [SerializeField] private ConfirmationWindow confirmWindow;

        GameObject lastSelectedObject = null;
        public void OnDeleteDataGUI()
        {
            OpenConfirmationWindow("Are you sure you want to delete save data?");
            lastSelectedObject = EventSystem.current.currentSelectedGameObject;
            EventSystem.current.SetSelectedGameObject(confirmWindow.noButton.gameObject);
        }


        private void OnEnable()
        {
            confirmWindow.gameObject.SetActive(false);
        }

        private void OpenConfirmationWindow(string message)
        {
            confirmWindow.gameObject.SetActive(true);
            confirmWindow.yesButton.onClick.AddListener(ClickYes);
            confirmWindow.noButton.onClick.AddListener(ClickNo);
            confirmWindow.messageText.text = message;
        }

        private void ClickYes()
        {
            CloseConfirmationWindow();
        }

        private void ClickNo()
        {
            CloseConfirmationWindow();
        }

        private void CloseConfirmationWindow()
        {
            EventSystem.current.SetSelectedGameObject(lastSelectedObject);
            confirmWindow.yesButton.onClick.RemoveListener(ClickYes);
            confirmWindow.noButton.onClick.RemoveListener(ClickNo);
            confirmWindow.gameObject.SetActive(false);
        }
    }
}

