using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace GASHAPWN.UI {
    // Adapted from gamesplusjames via YouTube

    /// <summary>
    /// Pair of axes of resolution
    /// </summary>
    [System.Serializable]
    public class ResItem
    {
        public int horizontal, vertical;
    }

    /// <summary>
    /// Controller for Graphics-related options
    /// </summary>
    public class OptionsScreenGraphics : MonoBehaviour
    {
        [Tooltip("Toggle for fullscreen")]
        public Toggle fullscreenTog;

        [Tooltip("Toggle for V-Sync")]
        public Toggle VsyncTog;

        [Tooltip("List of Resolutions")]
        public List<ResItem> resolutions = new List<ResItem>();

        [Tooltip("Label for selected resolution")]
        public TMP_Text resolutionLabel;

        // Index of currently selected resolution
        private int selectedResolution;


        /// PRIVATE METHODS ///

        private void Start()
        {
            fullscreenTog.isOn = Screen.fullScreen;

            if (QualitySettings.vSyncCount == 0)
            {
                VsyncTog.isOn = false;
            }
            else
            {
                VsyncTog.isOn = true;
            }

            // set current resolution to corresponding resolution in list
            bool foundRes = false;
            for (int i = 0; i < resolutions.Count; i++)
            {
                if (Screen.width == resolutions[i].horizontal && Screen.height == resolutions[i].vertical)
                {
                    foundRes = true;
                    selectedResolution = i;

                    UpdateResLabel();
                }
            }

            // add current resolution to list if not found
            if (!foundRes)
            {
                ResItem newRes = new ResItem();
                newRes.horizontal = Screen.width;
                newRes.vertical = Screen.height;

                resolutions.Add(newRes);
                selectedResolution = resolutions.Count - 1;
                UpdateResLabel();
            }
        }

        private void UpdateResLabel()
        {
            resolutionLabel.text = resolutions[selectedResolution].horizontal.ToString() + " x " + resolutions[selectedResolution].vertical.ToString();
        }


        /// PUBLIC METHODS ///

        /// <summary>
        /// Left resolution button pressed actions
        /// </summary>
        public void ResLeft()
        {
            selectedResolution--;
            if (selectedResolution < 0)
            {
                selectedResolution = 0;
            }
            UpdateResLabel();
        }

        /// <summary>
        /// Right resolution button pressed actions
        /// </summary>
        public void ResRight()
        {
            selectedResolution++;
            if (selectedResolution > resolutions.Count - 1)
            {
                selectedResolution = resolutions.Count - 1;
            }
            UpdateResLabel();
        }

        public void ApplyGraphics()
        {
            if (VsyncTog.isOn)
            {
                QualitySettings.vSyncCount = 1;
            }
            else
            {
                QualitySettings.vSyncCount = 0;
            }
            Screen.SetResolution(resolutions[selectedResolution].horizontal, resolutions[selectedResolution].vertical, fullscreenTog.isOn);
        }

        // TODO: Might have to disable settings for WebGL build
    }
}