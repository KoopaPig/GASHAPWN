using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.InputSystem;

namespace GASHAPWN.UI {
    public class ButtonSelectionHandler : MonoBehaviour
    {
        public UnityEvent OnMenuButtonSelected = new UnityEvent();
        public UnityEvent OnGeneralButtonSelected = new UnityEvent();
        public UnityEvent OnGeneralObjectSelected = new UnityEvent();

        private GameObject lastSelected;

        private void Update()
        {

            GameObject currentSelected = EventSystem.current.currentSelectedGameObject;

            if (currentSelected != lastSelected && currentSelected != null)
            {
                // Invoke for menu buttons
                if (currentSelected != null && currentSelected.GetComponent<MenuButtonControl>() != null)
                {
                    OnMenuButtonSelected?.Invoke();
                }

                // Invoke for general buttons
                if (currentSelected != null && currentSelected.GetComponent<GeneralButtonControl>() != null)
                {
                    OnGeneralButtonSelected?.Invoke();
                }

                // Invoke for general selections
                if (currentSelected != null)
                {
                    OnGeneralObjectSelected?.Invoke();
                }

                lastSelected = currentSelected;
            }
        }
    }
}


