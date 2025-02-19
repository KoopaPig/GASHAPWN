using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GASHAPWN.UI {
    public class LevelPreviewController : MonoBehaviour
    {
        [SerializeField] private Image levelIcon;
        [SerializeField] private TextMeshProUGUI levelTitle;
        //[SerializeField] private Image levelBannerBG;
        [SerializeField] private GameObject staticOverlay;

        private void Awake()
        {
            staticOverlay.SetActive(false);
        }

        public void SetLevelPreview(Level selectedLevel)
        {
            // could add more flashiness to this
            StartCoroutine(SetStatic());
            levelIcon.sprite = selectedLevel.levelPreviewIcon;
            levelTitle.text = selectedLevel.levelName;
        }

        private IEnumerator SetStatic()
        {
            staticOverlay.SetActive(true);
            yield return new WaitForSeconds(0.1f);
            staticOverlay.SetActive(false);
        }
    }
}