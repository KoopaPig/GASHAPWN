using GASHAPWN.UI;
using UnityEngine;
using UnityEngine.Audio;

namespace GASHAPWN.Audio {
    public class UI_SFXManager : MonoBehaviour
    {
        public static UI_SFXManager Instance { get; private set; }

        [SerializeField] private AudioMixerGroup soundMixer;

        [SerializeField] private SFXGroup menuButtonSelectionGroup;

        [SerializeField] private SFXGroup generalButtonSelectionGroup;

        [SerializeField] private SFXGroup generalSelectionGroup;

        [SerializeField] private SFXGroup buttonDinkyGroup;

        [SerializeField] private SFXGroup buttonFancyGroup;

        [SerializeField] private SFXGroup capsuleShakeGroup;

        [SerializeField] private SFXGroup staticGroup;

        [SerializeField] private SFXGroup infoCardGroup;

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
            AudioManager.Instance.PlayRandomSound(menuButtonSelectionGroup);
        }

        public void Play_GeneralButtonSelection()
        {
            AudioManager.Instance.PlayRandomSound(generalButtonSelectionGroup);
        }

        public void Play_GeneralObjectSelection()
        {
            AudioManager.Instance.PlayRandomSound(generalSelectionGroup);
        }

        public void Play_GeneralButton()
        {
            AudioManager.Instance.PlaySound("SFX_UI_Click_Open_Cute");
        }

        public void Play_FancyButton()
        {
            AudioManager.Instance.PlayRandomSound(buttonFancyGroup);
        }

        public void Play_GameEnd()
        {
            AudioManager.Instance.PlaySound("SFX_Ref_Whistle");
        }

        public void Play_SuddenDeath()
        {
            AudioManager.Instance.PlaySound("SFX_Lightning_Instant_Cast_Spell_C");
        }

        public void Play_LeftRightButtonSelection()
        {
            AudioManager.Instance.PlayRandomSound(buttonDinkyGroup);
        }

        public void Play_CapsuleShake() {
            AudioManager.Instance.PlayRandomSound(capsuleShakeGroup);
        }

        public void Play_StaticGroup() {
            AudioManager.Instance.PlayRandomSound(staticGroup);
        }

        public void Play_ScreenWoosh()
        {
            AudioManager.Instance.PlaySound("SFX_UI_Swoosh_Generic_2");
        }

        public void Play_InfoCardGroup()
        {
            AudioManager.Instance.PlayRandomSound(infoCardGroup);
        }
    }
}
