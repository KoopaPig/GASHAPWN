using UnityEngine;
using UnityEngine.UI;

namespace GASHAPWN.UI {
    /// <summary>
    /// Ensures child buttons of LeftRightButton are set up correctly
    /// </summary>
    public class LeftRightButtonControlChild : MonoBehaviour
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