using GASHAPWN.UI;
using UnityEngine;
using UnityEngine.Audio;

namespace GASHAPWN.Audio {
    public class UI_SFXManager : MonoBehaviour
    {
        public static UI_SFXManager Instance { get; private set; }

        [SerializeField] private AudioMixerGroup soundMixer;

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

        public void Play_GameEnd()
        {
            AudioManager.Instance.PlaySound("SFX_Ref_Whistle");
        }

        public void Play_SuddenDeath()
        {
            AudioManager.Instance.PlaySound("SFX_Lightning_Instant_Cast_Spell_C");
        }


    }
}
