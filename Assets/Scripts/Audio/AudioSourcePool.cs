using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

namespace GASHAPWN.Audio
{
    // Grabbed from HapticMC
    public class AudioSourcePool : MonoBehaviour
    {
        // AudioSourcePool is a Singleton
        public static AudioSourcePool Instance { get; private set; }

        public int poolSize = 10;
        public AudioSource audioSourcePrefab;

        private Queue<AudioSource> availableSources;


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
            availableSources = new Queue<AudioSource>();

            for (int i = 0; i < poolSize; i++)
            {
                availableSources.Enqueue(CreateNewAudioSource());
            }
        }

        /// PUBLIC METHODS ///

        // return AudioSource from front of queue
        public AudioSource GetAudioSource()
        {
            AudioSource source;
            if (availableSources.Count > 0)
            {
                source = availableSources.Dequeue();
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
            availableSources.Enqueue(source);
        }

        // returns new AudioSource
        private AudioSource CreateNewAudioSource()
        {
            AudioSource newSource = Instantiate(audioSourcePrefab, transform);
            newSource.gameObject.SetActive(false); // disable initially
            return newSource;
        }

        // Return audio source to pool after it is done playing
        public void ReturnToPool(AudioSource source)
        {
            source.Stop();
            source.clip = null;
            source.gameObject.SetActive(false);
            availableSources.Enqueue(source);
        }

        // Return audio soruce to pool immediately
        public void ReturnToPoolImmediate(AudioSource source)
        {
            source.Stop();
            source.clip = null;
            source.gameObject.SetActive(false);
            availableSources.Enqueue(source);
        }
    }
}