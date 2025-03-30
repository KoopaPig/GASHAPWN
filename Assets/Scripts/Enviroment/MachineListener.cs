using UnityEngine;
using System.Collections;

namespace GASHAPWN {
    public class MachineListener : MonoBehaviour
    {
        Animator animator;

        private void Awake() 
        {
            animator = GetComponent<Animator>();
        }

        public void HandleNewFigureScreen(BattleState state) 
        {
            StartCoroutine(WaitToOpenDoor(1f));
        }

        private void OnEnable() 
        {
            BattleManager.Instance.ChangeToNewFigure.AddListener(HandleNewFigureScreen);
        }

        private IEnumerator WaitToOpenDoor(float waitDuration) {
            yield return new WaitForSeconds(waitDuration);
            animator.SetTrigger("door-open");
        }
    }
}