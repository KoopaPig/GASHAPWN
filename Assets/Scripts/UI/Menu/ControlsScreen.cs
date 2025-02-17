using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


namespace GASHAPWN.UI {

    [System.Serializable]
    public class ControlsItem
    {
        public GameObject obj;
        public string title;
    }

    public class ControlsScreen : MonoBehaviour
    {
        // Get reference to containers with controls info
        public List<ControlsItem> controlsItems;

        // Get reference to text of selected controls
        [SerializeField] private TMP_Text controlsLabel;

        // Index of currently selected controls
        private int selectedControls = 0;

        void Start()
        {
            UpdateControlsLabel();
        }

        public void ControlsLeft()
        {
            selectedControls--;
            if (selectedControls < 0)
            {
                selectedControls = controlsItems.Count - 1;
            }
            UpdateControlsLabel();
        }

        public void ControlsRight()
        {
            selectedControls++;
            if (selectedControls > controlsItems.Count - 1)
            {
                selectedControls = 0;
            }
            UpdateControlsLabel();
        }

        private void UpdateControlsLabel()
        {
            foreach (ControlsItem i in controlsItems)
            {
                i.obj.SetActive(false);
            }
            controlsItems[selectedControls].obj.SetActive(true);
            controlsLabel.text = controlsItems[selectedControls].title;
        }
    }
}
