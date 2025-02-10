using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Adapted from gamesplusjames via YouTube

[System.Serializable]
public class ResItem
{
    public int horizontal, vertical;
}

public class OptionsScreenGraphics : MonoBehaviour
{
    // Get reference to fullscreenTog and VsyncTog
    public Toggle fullscreenTog, VsyncTog;

    // New list of ResItems
    public List<ResItem> resolutions = new List<ResItem>();

    // Index of currently selected resolution
    private int selectedResolution;

    // Get reference to text of selected resolution
    public TMP_Text resolutionLabel;

    void Start()
    {
        fullscreenTog.isOn = Screen.fullScreen;

        if (QualitySettings.vSyncCount == 0)
        {
            VsyncTog.isOn = false;
        } else
        {
            VsyncTog.isOn = true;
        }

        // set current resolution to corresponding resolution in list
        bool foundRes = false;
        for (int i = 0; i < resolutions.Count; i++) {
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

    public void ResLeft()
    {
        selectedResolution--;
        if (selectedResolution < 0)
        {
            selectedResolution = 0;
        }
        UpdateResLabel();
    }

    public void ResRight()
    {
        selectedResolution++;
        if (selectedResolution > resolutions.Count - 1) 
        { 
            selectedResolution = resolutions.Count - 1;
        }
        UpdateResLabel();
    }

    public void UpdateResLabel()
    {
        resolutionLabel.text = resolutions[selectedResolution].horizontal.ToString() + " x " + resolutions[selectedResolution].vertical.ToString();
    }
    public void ApplyGraphics()
    {
        if (VsyncTog.isOn)
        {
            QualitySettings.vSyncCount = 1;
        } else
        {
            QualitySettings.vSyncCount = 0;
        }
        Screen.SetResolution(resolutions[selectedResolution].horizontal, resolutions[selectedResolution].vertical, fullscreenTog.isOn);
    }

    // TODO: Might have to disable settings for WebGL build
}