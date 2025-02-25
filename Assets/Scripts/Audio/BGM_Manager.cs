using UnityEngine;
using UnityEngine.Audio;

namespace GASHAPWN.Audio {
    public class BGM_Manager : MonoBehaviour
    {
        public static BGM_Manager Instance { get; private set; }

        [SerializeField] private AudioSource mainAudioSource;

        private void Awake()
        {
            // Check for other instances
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                return;
            }
            Instance = this;

            // Check both game and battle manager states and change music accordingly

            GameManager.Instance.ChangeToBattle.AddListener(SetMusicBattle);
            GameManager.Instance.ChangeToTitle.AddListener(SetMusicMenu);
            GameManager.Instance.ChangeToLevelSelect.AddListener(SetMusicLevelSelect);
            GameManager.Instance.ChangeToCollection.AddListener(SetMusicCollection);

        }

        // Update is called once per frame
        void Update()
        {

        }

        private void OnDisable()
        {
            GameManager.Instance.ChangeToBattle.RemoveListener(SetMusicBattle);
            GameManager.Instance.ChangeToTitle.RemoveListener(SetMusicMenu);
            GameManager.Instance.ChangeToLevelSelect.RemoveListener(SetMusicLevelSelect);
            GameManager.Instance.ChangeToCollection.RemoveListener(SetMusicCollection);
        }

        private void SetMusicBattle(GameManager.GameState state)
        {

        }

        private void SetMusicMenu(GameManager.GameState state)
        {

        }

        private void SetMusicLevelSelect(GameManager.GameState state)
        {

        }

        private void SetMusicCollection(GameManager.GameState state)
        {

        }
    }
}


