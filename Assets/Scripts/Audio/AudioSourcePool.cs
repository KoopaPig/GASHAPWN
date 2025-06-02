using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

namespace GASHAPWN.Audio
{
    public class AudioSourcePool : MonoBehaviour
    {
        public static AudioSourcePool Instance { get; private set; }

        [Tooltip("Initial amount of AudioSources in pool")]
        public int poolSize = 10;

        [Tooltip("AudioSource prefab  to instantiate")]
        public AudioSource audioSourcePrefab;

        private Queue<AudioSource> _availableSources;


        /// PRIVATE METHODS ///
        
        private void Awake()
        {
            // Ensure Singleton
            if (Instance == null)
            {
                Instance = this;
                InitializePool();
            }
            else
            {
                Destroy(gameObject);
            }
            // Persistance
            DontDestroyOnLoad(gameObject);  
        }

        // Initialize AudioSources, fill queue with them
        private void InitializePool()
        {
            _availableSources = new Queue<AudioSource>();

            for (int i = 0; i < poolSize; i++)
            {
                _availableSources.Enqueue(CreateNewAudioSource());
            }
        }


        /// PUBLIC METHODS ///

        // return AudioSource from front of queue
        public AudioSource GetAudioSource()
        {
            AudioSource source;
            if (_availableSources.Count > 0)
            {
                source = _availableSources.Dequeue();
            }
            else
            {
                source = CreateNewAudioSource();
            }
            source.gameObject.SetActive(true);
            return source;
        }

        // Stop audio source then put at back of queue
        public void ReturnAudioSource(AudioSource source)
        {
            source.Stop();
            source.clip = null;
            source.gameObject.SetActive(false);
            _availableSources.Enqueue(source);
        }

        // returns new AudioSource
        private AudioSource CreateNewAudioSource()
        {
            AudioSource newSource = Instantiate(audioSourcePrefab, transform);
            newSource.gameObject.SetActive(false); // disable initially
            return newSource;
        }

        // Return AudioSource to pool after it is done playing
        public void ReturnToPool(AudioSource source)
        {
            source.Stop();
            source.clip = null;
            source.gameObject.SetActive(false);
            _availableSources.Enqueue(source);
        }

        // Return AudioSource to pool immediately
        public void ReturnToPoolImmediate(AudioSource source)
        {
            source.Stop();
            source.clip = null;
            source.gameObject.SetActive(false);
            _availableSources.Enqueue(source);
        }
    }
}