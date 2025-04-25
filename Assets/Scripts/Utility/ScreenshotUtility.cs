using System;
using NUnit.Framework.Constraints;
using Unity.Cinemachine;
using UnityEngine;

namespace GASHAPWN.Utility
{
    public class ScreenshotUtility : MonoBehaviour
    {
        [SerializeField] private CinemachineCamera frontCam;
        [SerializeField] private CinemachineCamera topDownCam;

        public bool useTopDown = false;

        private void Update()
        {
            SwitchCamera();
            // Press '0' to take screenshot
            if (Input.GetKeyDown(KeyCode.Alpha0))
            {
                string filename = "levelScreenshot-" + DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss") + ".png";
                ScreenCapture.CaptureScreenshot(filename, 4);
                Debug.Log("Screenshot taken: " + filename);
            }
        }

        // if useTopDown, switch to topDownCam
        private void SwitchCamera()
        {
            if (useTopDown)
            {
                topDownCam.Priority = 20;
                frontCam.Priority = 10;
            }
            else
            {
                frontCam.Priority = 20;
                topDownCam.Priority = 10;
            }
        }

        private void OnValidate()
        {
            SwitchCamera();
        }
    }
}