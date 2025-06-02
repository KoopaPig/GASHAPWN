using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace GASHAPWN.UI {

    /// <summary>
    /// ControlsItem ties a title to a parent GameObject of UI elements for ControlsScreen
    /// </summary>
    [System.Serializable]
    public class ControlsItem
    {
        public GameObject obj;
        public string title;
    }

    /// <summary>
    /// Manages UI elements on ControlsScreen
    /// </summary>
    public class ControlsScreen : MonoBehaviour
    {
        [Tooltip("List of containers with controls info")]
        public List<ControlsItem> controlsItems;

        // Get reference to text of selected controls
        [SerializeField] private TextMeshProUGUI controlsLabel;

        // Index of currently selected controls
        private int selectedControls = 0;


        /// PRIVATE METHODS ///

        private void Start()
        {
            UpdateControlsLabel();
        }

        // Update controlsLabel based on currently selected ControlsItem
        private void UpdateControlsLabel()
        {
            foreach (ControlsItem i in controlsItems)
            {
                i.obj.SetActive(false);
            }
            controlsItems[selectedControls].obj.SetActive(true);
            controlsLabel.text = controlsItems[selectedControls].title;
        }


        /// PUBLIC METHODS ///

        /// <summary>
        /// Left button pressed actions
        /// </summary>
        public void ControlsLeft()
        {
            selectedControls--;
            if (selectedControls < 0)
            {
                selectedControls = controlsItems.Count - 1;
            }
            UpdateControlsLabel();
        }

        /// <summary>
        /// Right button pressed actions
        /// </summary>
        public void ControlsRight()
        {
            selectedControls++;
            if (selectedControls > controlsItems.Count - 1)
            {
                selectedControls = 0;
            }
            UpdateControlsLabel();
        }
    }
}