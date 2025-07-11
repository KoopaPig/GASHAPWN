using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace GASHAPWN.UI {
    public class OverlayGUI : MonoBehaviour
    {
        [SerializeField] private GameObject countdownGUI;
        [SerializeField] private GameObject battleEndGUI;
        [SerializeField] private GameObject suddenDeathGUI;
        [SerializeField] private GameObject victoryScreenSubPanel;

        private void Awake()
        {
            countdownGUI.SetActive(false);
            countdownGUI.GetComponent<Animator>().enabled = false;
            battleEndGUI.SetActive(false);
            battleEndGUI.GetComponent<Animator>().enabled = false;
            suddenDeathGUI.SetActive(false);
            suddenDeathGUI.GetComponent<Animator>().enabled = false;
            victoryScreenSubPanel.SetActive(false);
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

        public void BattleEndGUI()
        {
            battleEndGUI.SetActive(true);
            battleEndGUI.GetComponent<Animator>().enabled = true;
            victoryScreenSubPanel.SetActive(true);
            StartCoroutine(victoryScreenSubPanel.GetComponentInParent<ResultsScreenGUI>().SlideInVictoryScreen(3f));
        }

        public void SuddenDeathGUI()
        {
            suddenDeathGUI.SetActive(true);
            suddenDeathGUI.GetComponent<Animator>().enabled = true;
        }
    }
}
