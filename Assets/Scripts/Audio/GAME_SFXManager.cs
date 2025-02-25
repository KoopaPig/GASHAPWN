using UnityEngine;

namespace GASHAPWN.Audio {
    public class GAME_SFXManager : MonoBehaviour
    {
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


