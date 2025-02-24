using System.Collections.Generic;
using UnityEngine;

namespace GASHAPWN.Audio {

    [System.Serializable]
    public class SFXGroup
    {
        public string groupTag;
        public List<AudioClip> audioClips = new List<AudioClip>();
        private int previousIndex = -1;

        public AudioClip GetRandomAudioClip()
        {
            int i = UnityEngine.Random.Range(0, audioClips.Count);
            while (i == previousIndex)
            {
                i = UnityEngine.Random.Range(0, audioClips.Count);
            }
            var selectedClip = audioClips[i];
            previousIndex = i;
            return selectedClip;
        }
    }

    public class SFXManager : MonoBehaviour
    {
        public static SFXManager Instance { get; private set; }

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
