// Adapted from EasyTransition, modified

using GASHAPWN;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace EasyTransition
{
    public class TransitionManager : MonoBehaviour
    {
        private static TransitionManager instance;

        public static TransitionManager Instance()
        {
            if (instance == null)
                Debug.LogError("You tried to access the instance before it exists.");

            return instance;
        }

        [SerializeField] private GameObject transitionTemplate;

        [Tooltip("Global list of transitionPairs (sceneName, TransitionSettings")]
        public List<TransitionPair> transitionPairs = new();

        // flag if currently playing transition
        private bool runningTransition;

        public UnityAction onTransitionBegin;
        public UnityAction onTransitionCutPointReached;
        public UnityAction onTransitionEnd;

        //public Image Fillbar;
        //public GameObject LoadingScreen;

        private void Awake()
        {
            instance = this;
        }

        /// <summary>
        /// Starts a transition without loading a new level.
        /// </summary>
        /// <param name="transition">The settings of the transition you want to use.</param>
        /// <param name="startDelay">The delay before the transition starts.</param>
        public void Transition(TransitionSettings transition, float startDelay)
        {
            if (transition == null || runningTransition)
            {
                Debug.LogError("You have to assing a transition.");
                return;
            }

            runningTransition = true;
            StartCoroutine(Timer(startDelay, transition));
        }

        /// <summary>
        /// Loads the new Scene with a transition.
        /// </summary>
        /// <param name="sceneName">The name of the scene you want to load.</param>
        /// <param name="transition">The settings of the transition you want to use to load you new scene.</param>
        /// <param name="startDelay">The delay before the transition starts.</param>
        public void Transition(string sceneName, TransitionSettings transition, float startDelay)
        {
            if (transition == null || runningTransition)
            {
                Debug.LogError("You have to assing a transition.");
                return;
            }

            runningTransition = true;
            StartCoroutine(Timer(sceneName, startDelay, transition));
        }

        /// <summary>
        /// Loads the new Scene with a transition.
        /// </summary>
        /// <param name="sceneIndex">The index of the scene you want to load.</param>
        /// <param name="transition">The settings of the transition you want to use to load you new scene.</param>
        /// <param name="startDelay">The delay before the transition starts.</param>
        public void Transition(int sceneIndex, TransitionSettings transition, float startDelay)
        {
            if (transition == null || runningTransition)
            {
                Debug.LogError("You have to assing a transition.");
                return;
            }

            runningTransition = true;
            StartCoroutine(Timer(sceneIndex, startDelay, transition));
        }

        /// <summary>
        /// Loads the new Scene given TransitionPair
        /// </summary>
        /// <param name="transition"></param>
        /// <param name="startDelay"></param>
        public void Transition(TransitionPair transition, float startDelay)
        {
            Transition(transition.sceneName, transition.settings, startDelay);
        }

        /// <summary>
        /// Loads the new Scene given Level
        /// </summary>
        /// <param name="level"></param>
        /// <param name="startDelay"></param>
        public void Transition(Level level, float startDelay)
        {
            TransitionPair matchingTransition = transitionPairs.Find(t => t.sceneName == level.levelSceneName);
            if (matchingTransition != null)
            {
                Transition(matchingTransition, startDelay);
            }
            else Debug.LogError($"TransitionManager: Invalid level scene name: {level.levelSceneName}");
        }

        /// <summary>
        /// Loads the new Scene given just sceneName
        /// </summary>
        /// <param name="sceneName"></param>
        /// <param name="startDelay"></param>
        public void Transition(string sceneName, float startDelay)
        {
            // make sure sceneName is in transitionPairs
            TransitionPair matchingTransition = transitionPairs.Find(t => t.sceneName == sceneName);
            if (matchingTransition != null)
            {
                Transition(matchingTransition, startDelay);
            }
            else Debug.LogError($"TransitionManager: Invalid scene name: {sceneName}");
        }

        /// <summary>
        /// Gets the index of a scene from its name.
        /// </summary>
        /// <param name="sceneName">The name of the scene you want to get the index of.</param>
        int GetSceneIndex(string sceneName)
        {
            return SceneManager.GetSceneByName(sceneName).buildIndex;
        }

        IEnumerator Timer(string sceneName, float startDelay, TransitionSettings transitionSettings)
        {
            yield return new WaitForSecondsRealtime(startDelay);

            onTransitionBegin?.Invoke();

            GameObject template = Instantiate(transitionTemplate) as GameObject;
            template.GetComponent<Transition>().transitionSettings = transitionSettings;

            float transitionTime = transitionSettings.transitionTime;
            if (transitionSettings.autoAdjustTransitionTime)
                transitionTime = transitionTime / transitionSettings.transitionSpeed;

            yield return new WaitForSecondsRealtime(transitionTime);

            onTransitionCutPointReached?.Invoke();


            SceneManager.LoadScene(sceneName);
            //StartCoroutine(LoadScene(sceneName));
            yield return new WaitForSecondsRealtime(transitionSettings.destroyTime);

            onTransitionEnd?.Invoke();
        }

        IEnumerator Timer(int sceneIndex, float startDelay, TransitionSettings transitionSettings)
        {
            yield return new WaitForSecondsRealtime(startDelay);

            onTransitionBegin?.Invoke();

            GameObject template = Instantiate(transitionTemplate) as GameObject;
            template.GetComponent<Transition>().transitionSettings = transitionSettings;

            float transitionTime = transitionSettings.transitionTime;
            if (transitionSettings.autoAdjustTransitionTime)
                transitionTime = transitionTime / transitionSettings.transitionSpeed;

            yield return new WaitForSecondsRealtime(transitionTime);

            onTransitionCutPointReached?.Invoke();

            SceneManager.LoadScene(sceneIndex);
            //StartCoroutine(LoadScene(sceneIndex));

            yield return new WaitForSecondsRealtime(transitionSettings.destroyTime);

            onTransitionEnd?.Invoke();
        }

        IEnumerator Timer(float delay, TransitionSettings transitionSettings)
        {
            yield return new WaitForSecondsRealtime(delay);

            onTransitionBegin?.Invoke();

            GameObject template = Instantiate(transitionTemplate) as GameObject;
            template.GetComponent<Transition>().transitionSettings = transitionSettings;

            float transitionTime = transitionSettings.transitionTime;
            if (transitionSettings.autoAdjustTransitionTime)
                transitionTime = transitionTime / transitionSettings.transitionSpeed;

            yield return new WaitForSecondsRealtime(transitionTime);

            onTransitionCutPointReached?.Invoke();

            template.GetComponent<Transition>().OnSceneLoad(SceneManager.GetActiveScene(), LoadSceneMode.Single);

            yield return new WaitForSecondsRealtime(transitionSettings.destroyTime);

            onTransitionEnd?.Invoke();

            runningTransition = false;
        }

        private IEnumerator Start()
        {
            while (this.gameObject.activeInHierarchy)
            {
                //Check for multiple instances of the Transition Manager component
                var managerCount = GameObject.FindObjectsOfType<TransitionManager>(true).Length;
                if (managerCount > 1)
                    Debug.LogError($"There are {managerCount.ToString()} Transition Managers in your scene. Please ensure there is only one Transition Manager in your scene or overlapping transitions may occur.");

                yield return new WaitForSecondsRealtime(1f);
            }
        }

        //public IEnumerator LoadScene(int sceneIndex)
        //{
        //    //Instantiate(LoadingScreen);
        //    LoadingScreen.SetActive(true);

        //    AsyncOperation operation = SceneManager.LoadSceneAsync(sceneIndex);

        //    while (!operation.isDone)
        //    {
        //        float progress = Mathf.Clamp01(operation.progress / 0.9f);
        //        Fillbar.fillAmount = progress;
        //    }

        //    yield return null;
        //}

        //public IEnumerator LoadScene(string sceneName)
        //{
        //    //Instantiate(LoadingScreen);
        //    LoadingScreen.SetActive(true);

        //    AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);

        //    while (!operation.isDone)
        //    {
        //        float progress = Mathf.Clamp01(operation.progress / 0.9f);
        //        Fillbar.fillAmount = progress;
        //    }

        //    yield return null;
        //}
    }

    /// <summary>
    /// TransitionPair is a pair of (sceneName, TransitionSettings)
    /// </summary>
    [System.Serializable]
    public class TransitionPair
    {
        public string sceneName = null;
        public TransitionSettings settings;
    }
}
