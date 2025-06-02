using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace GASHAPWN.UI {
    /// <summary>
    /// Controller for group of StageButtons on Stage Select Screen
    /// </summary>
    public class LevelSelectGrid : MonoBehaviour
    {
        [Tooltip("Level Preview Controller")]
        [SerializeField] private LevelPreviewController levelPreview;

        [Tooltip("List of StageButtons")]
        [SerializeField] private List<StageButton> stageButtons;

        // currently selected StageButton
        private StageButton selectedButton;

        // Reference to LevelSelect
        private LevelSelect levelSelect;

        private void Awake()
        {
            levelSelect = FindFirstObjectByType<LevelSelect>();
            if (stageButtons.Count == levelSelect.levels.Count)
            {
                // populate the buttons with corresponding level data
                for (int i = 0; i < stageButtons.Count; i++)
                {
                    int index = i;
                    stageButtons[i].SetLevel(levelSelect.levels[index]);
                    stageButtons[i].GetComponent<Button>().onClick.AddListener(delegate { SelectLevel(levelSelect.levels[index]); });
                    stageButtons[i].GetComponent<Button>().onClick.AddListener(() => OnButtonSelected(stageButtons[index]));
                }
                // default selection
                SelectLevel(levelSelect.levels[0]);
                selectedButton = stageButtons[0];
            }
            else Debug.LogError("Number of Stage Buttons and Levels does not match.");
        }

        /// <summary>
        /// Handle Level Selected
        /// </summary>
        /// <param name="level"></param>
        public void SelectLevel(Level level)
        {
            levelSelect.selectedLevel = level;
            levelPreview.SetLevelPreview(level);
            Debug.Log("Selected " + level.levelName);
        }

        /// <summary>
        /// Update and keep track of selected StageButton
        /// </summary>
        /// <param name="button"></param>
        public void OnButtonSelected(StageButton button)
        {
            if (selectedButton != null && selectedButton != button)
            {
                selectedButton.highlightBorder.SetActive(false);
            }
            selectedButton = button;
            selectedButton.highlightBorder.SetActive(true);
        }
    }
}