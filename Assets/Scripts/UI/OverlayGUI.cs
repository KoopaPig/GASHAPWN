using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace GASHAPWN.UI {
    public class OverlayGUI : MonoBehaviour
    {
        [SerializeField] private GameObject countdownGUI;
        [SerializeField] private GameObject battleEndGUI;
        [SerializeField] private GameObject suddenDeathGUI;
        [SerializeField] private VictoryScreenGUI victoryScreenGUI;

        private void Awake()
        {
            countdownGUI.SetActive(false);
            countdownGUI.GetComponent<Animator>().enabled = false;
            battleEndGUI.SetActive(false);
            battleEndGUI.GetComponent<Animator>().enabled = false;
            suddenDeathGUI.SetActive(false);
            suddenDeathGUI.GetComponent<Animator>().enabled = false;
            victoryScreenGUI.gameObject.SetActive(false);
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
            victoryScreenGUI.gameObject.SetActive(true);
            StartCoroutine(victoryScreenGUI.SlideInVictoryScreen(3f));
        }

        public void SuddenDeathGUI()
        {
            suddenDeathGUI.SetActive(true);
            suddenDeathGUI.GetComponent<Animator>().enabled = true;
        }
    }
}
