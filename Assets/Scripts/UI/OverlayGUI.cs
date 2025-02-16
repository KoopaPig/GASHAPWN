using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace GASHAPWN.UI {
    public class OverlayGUI : MonoBehaviour
    {
        [SerializeField] private GameObject countdownGUI;
        //[SerializeField] private GameObject victoryOverlayGUI;
        //[SerializeField] private GameObject battleEndGUI;

        private void Awake()
        {
            countdownGUI.SetActive(false);
            countdownGUI.GetComponent<Animator>().enabled = false;
        }

        public void StartCountdownGUI()
        {
            countdownGUI.SetActive(true);
            countdownGUI.GetComponent<Animator>().enabled = true;
        }

        public void EndCountdownGUI()
        {
           StartCoroutine(WaitToEndCountdownGUI());
        }

        private IEnumerator WaitToEndCountdownGUI()
        {
            yield return new WaitForSeconds(2f);
            countdownGUI.SetActive(false);
            countdownGUI.GetComponent<Animator>().enabled = false;
        }
    }
}
