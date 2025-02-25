using GASHAPWN.UI;
using UnityEngine;

namespace GASHAPWN.Audio {
    public class UI_SFXManager : MonoBehaviour
    {
        public static UI_SFXManager Instance { get; private set; }

        [SerializeField] private SFXGroup buttonSelectionGroup;

        private void Awake()
        {
            // Check for other instances
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                return;
            }
            Instance = this;
        }

        public void Play_PlayButton()
        {
            AudioManager.Instance.PlaySound("SFX_UI_Swipe_Screen_1");
        }

        public void Play_MainMenuButtonSelection()
        {
            AudioManager.Instance.PlayRandomSound(buttonSelectionGroup);
        }

        public void Play_GeneralButtonSelection()
        {
            AudioManager.Instance.PlaySound("SFX_UI_Click_Close");
        }

        public void Play_GeneralButton()
        {
            AudioManager.Instance.PlaySound("SFX_UI_Click_Open_Cute");
        }

    }
}
