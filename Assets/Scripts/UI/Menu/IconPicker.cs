using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace GASHAPWN.UI {
    public class IconPicker : MonoBehaviour
    {
        [SerializeField] private GameObject xInputHelp;
        [SerializeField] private GameObject KeyboardHelp;
        private ControlScheme controlScheme;
        [SerializeField] public bool automaticUpdate = true;

        public void ManualSetControlScheme(ControlScheme cs) {
            if (!automaticUpdate) { 
                controlScheme = cs;
                UpdateControlScheme();
            }
            else Debug.Log($"{this.name}.IconPicker is set to automaticUpdate, cannot manually set.");
        }

        private void Start()
        {
            UpdateControlScheme();
        }

        private void OnValidate()
        {
            UpdateControlScheme();
        }

        void Update()
        {
            // Update buttons automatically according to control scheme
            if (automaticUpdate)
            {
                controlScheme = ControllerManager.Instance.GetPlayerControlScheme("Player1");
                UpdateControlScheme();
            }
        }

        private void UpdateControlScheme()
        {
            xInputHelp.SetActive(false);
            KeyboardHelp.SetActive(false);
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