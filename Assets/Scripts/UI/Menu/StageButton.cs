using UnityEngine;
using UnityEngine.UI;

namespace GASHAPWN.UI {
    /// <summary>
    /// Buttons for Stages in LevelSelect
    /// </summary>
    public class StageButton : MonoBehaviour
    {
        [Tooltip("Icon for corresponding level")]
        [SerializeField] private Image levelIcon;

        [Tooltip("Border that appears when highlighted")]
        public GameObject highlightBorder;

        private void Awake()
        {
            highlightBorder.SetActive(false);
        }

        // Set levelIcon given Level

        public void SetLevel(Level l)
        {
            levelIcon.sprite = l.levelPreviewIconA;
        }
    }
}