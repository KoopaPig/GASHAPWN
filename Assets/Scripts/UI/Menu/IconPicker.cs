using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.UI;

namespace GASHAPWN.UI {
    public class IconPicker : MonoBehaviour
    {
        [Tooltip("XInput ControlsHelp Object")]
        [SerializeField] private GameObject xInputHelp;
        [Tooltip("Keyboard ControlsHelp Object")]
        [SerializeField] private GameObject KeyboardHelp;
        [Tooltip("Current ControlScheme of IconPicker")]
        [SerializeField] private ControlScheme controlScheme;
        [Tooltip("Toggle whether IconPicker should automatically update its visual state")]
        public bool IsAutomaticUpdate = true;

        /// <summary>
        /// Manually set ControlScheme
        /// </summary>
        /// <param name="cs"></param>
        public void ManualSetControlScheme(ControlScheme cs)
        {
            if (!IsAutomaticUpdate)
            {
                controlScheme = cs;
                UpdateControlScheme();
            }
            else Debug.Log($"{this.name}.IconPicker is set to automaticUpdate, cannot manually set.");
        }

        private void Update()
        {
            if (!IsAutomaticUpdate) return;

            ControlScheme detectedScheme;

            // Try to Get ControlScheme from Player1 first;
            // if not, try to get any control scheme from first PlayerInput object in scene
            if (PlayerInputAssigner.Instance.TryGetPlayerControlScheme("Player1", out detectedScheme) ||
                PlayerInputAssigner.Instance.TryGetAnyControlScheme(out detectedScheme))
            {
                controlScheme = detectedScheme;
                UpdateControlScheme();
            }
        }

        /// <summary>
        /// Update controller icon according to controlScheme
        /// </summary>
        private void UpdateControlScheme()
        {
            switch (controlScheme)
            {
                case ControlScheme.XINPUT:
                    xInputHelp.SetActive(true);
                    KeyboardHelp.SetActive(false);
                    break;
                case ControlScheme.KEYBOARD:
                    xInputHelp.SetActive(false);
                    KeyboardHelp.SetActive(true);
                    break;
            }
        }
    }
}