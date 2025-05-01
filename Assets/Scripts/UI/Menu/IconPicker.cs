using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.UI;

namespace GASHAPWN.UI {
    public class IconPicker : MonoBehaviour
    {
        [SerializeField] private GameObject xInputHelp;
        [SerializeField] private GameObject KeyboardHelp;
        [SerializeField] private ControlScheme controlScheme;
        [SerializeField] public bool automaticUpdate = true;

        public void ManualSetControlScheme(ControlScheme cs)
        {
            if (!automaticUpdate)
            {
                controlScheme = cs;
                UpdateControlScheme();
            }
            else Debug.Log($"{this.name}.IconPicker is set to automaticUpdate, cannot manually set.");
        }


        // TODO: Adjust how this automatic update functions with new system
        private void Update()
        {
            if (!automaticUpdate) return;

            ControlScheme detectedScheme;

            // Try to Get ControlScheme from Player1 first;
            // if not, try to get any control scheme from first PlayerInput object in scene
            if (PlayerInputAssigner.Instance.TryGetPlayerControlScheme("Player1", out detectedScheme) ||
                PlayerInputAssigner.Instance.TryGetAnyControlScheme(out detectedScheme))
            {
                if (detectedScheme != controlScheme)
                {
                    controlScheme = detectedScheme;
                    UpdateControlScheme();
                }
            }
        }

        private void OnValidate()
        {
            UpdateControlScheme();
        }

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