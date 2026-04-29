using UnityEngine;
using System.Collections;

namespace GASHAPWN.Environment {
    /// <summary>
    /// Primary controller on parent object of Gasha Machine
    /// </summary>
    public class MachineController : MonoBehaviour
    {
        // This can handle spawning in level
        // Eventually, levels might need custom initialization, in which case need to make a new interface + derived classes

        [SerializeField] private Transform innerLevelContainer;
        [SerializeField] private Level defaultLevel;

        private GameObject currentLevelInstance;
        private Animator animator;

        public Transform Player1SpawnPos { get; set; }
        public Transform Player2SpawnPos { get; set; }


        /// PRIVATE METHODS ///

        private void Awake() 
        {
            animator = GetComponent<Animator>();
        }

        private void OnEnable() 
        {
            BattleManager.Instance.ChangeToNewFigure.AddListener(HandleNewFigureScreen);
            SpawnLevel();
        }

        private IEnumerator WaitToOpenDoor(float waitDuration) {
            yield return new WaitForSeconds(waitDuration);
            animator.SetTrigger(AnimationStrings.machineDoorOpen);
        }

        private void HandleNewFigureScreen(BattleState state)
        {
            StartCoroutine(WaitToOpenDoor(1f));
        }


        /// PUBLIC METHODS ///

        public void SpawnLevel()
        {
            var lvl = GameManager.Instance.currentLevel;

            if (lvl == null)
            {
                Debug.LogWarning("MACHINE LISTENER: No level selected! Falling back to default level.");
                lvl = defaultLevel;
            }

            // Cleanup previous
            if (currentLevelInstance != null)
            {
                Destroy(currentLevelInstance);
            }

            // Spawn new level and initialize
            currentLevelInstance = Instantiate(lvl.levelPrefab, innerLevelContainer.position, lvl.levelPrefab.transform.rotation, innerLevelContainer);
            var lvlData = currentLevelInstance.GetComponent<LevelData>();
            lvlData.Initialize(this);
        }
    }
}