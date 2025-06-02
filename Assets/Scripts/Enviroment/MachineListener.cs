using UnityEngine;
using System.Collections;

namespace GASHAPWN.Environment {
    /// <summary>
    /// Perform certain actions on GashaMachine in response to events
    /// </summary>
    public class MachineListener : MonoBehaviour
    {
        private Animator animator;

        private void Awake() 
        {
            animator = GetComponent<Animator>();
        }

        private void OnEnable() 
        {
            BattleManager.Instance.ChangeToNewFigure.AddListener(HandleNewFigureScreen);
        }

        private IEnumerator WaitToOpenDoor(float waitDuration) {
            yield return new WaitForSeconds(waitDuration);
            animator.SetTrigger("door-open");
        }

        private void HandleNewFigureScreen(BattleState state)
        {
            StartCoroutine(WaitToOpenDoor(1f));
        }
    }
}