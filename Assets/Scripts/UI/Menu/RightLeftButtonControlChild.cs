using Unity.Burst.Intrinsics;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;

namespace GASHAPWN.UI {
    public class RightLeftButtonControlChild : MonoBehaviour
    {
        private void Awake()
        {
            var button = GetComponent<Button>();
            if (button != null)
            {
                button.interactable = true;
                button.navigation = new Navigation { mode = Navigation.Mode.Automatic };
            }

        }
    }
}