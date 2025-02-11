using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace GASHAPWN.UI {
    public class IconPicker : MonoBehaviour
    {
        [SerializeField] private GameObject xInputHelp;
        [SerializeField] private GameObject KeyboardHelp;

        [SerializeField] private ControlScheme controlScheme;

        private void Start()
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

        void Update()
        {
            // TODO: Update buttons automatically according to control scheme
        }
    }
}