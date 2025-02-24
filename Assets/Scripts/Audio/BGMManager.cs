using UnityEngine;
using UnityEngine.Audio;

namespace GASHAPWN.Audio {
    public class BGMManager : MonoBehaviour
    {
        public static BGMManager Instance { get; private set; }

        [SerializeField] private AudioClip battleMusic;
        [SerializeField] private AudioClip titleMusic;
        [SerializeField] private AudioClip levelSelectMusic;
        [SerializeField] private AudioClip collectionMusic;

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
    }
}


