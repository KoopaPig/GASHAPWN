using UnityEngine;
using UnityEngine.Audio;

namespace GASHAPWN.Audio {
    public class GAME_SFXManager : MonoBehaviour
    {

        [SerializeField] private AudioMixerGroup soundMixer;

        public static GAME_SFXManager Instance { get; private set; }

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


    }
}


