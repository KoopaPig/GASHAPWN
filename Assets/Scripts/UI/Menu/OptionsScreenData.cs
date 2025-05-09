using GASHAPWN.Audio;
using GASHAPWN.Utility;
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
            EventSystemSelectHelper.SetSelectedGameObject(confirmWindow.noButton.gameObject);
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
            UI_SFXManager.Instance.Play_WarningPopup();
        }

        private void ClickYes()
        {
            GameManager.Instance.DeleteSaveData(); // delete save data
            CloseConfirmationWindow();
        }

        private void ClickNo()
        {
            CloseConfirmationWindow();
        }

        private void CloseConfirmationWindow()
        {
            EventSystemSelectHelper.SetSelectedGameObject(lastSelectedObject);
            confirmWindow.yesButton.onClick.RemoveListener(ClickYes);
            confirmWindow.noButton.onClick.RemoveListener(ClickNo);
            confirmWindow.gameObject.SetActive(false);
        }
    }
}

