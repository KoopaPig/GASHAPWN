using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace GASHAPWN.UI {
    public class LevelSelectGrid : MonoBehaviour
    {
        [SerializeField] private List<StageButton> stageButtons;
        private LevelSelect levelSelect;
        private StageButton selectedButton;
        [SerializeField] private LevelPreviewController levelPreview;

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

        public void SelectLevel(Level level)
        {
            levelSelect.selectedLevel = level;
            levelPreview.SetLevelPreview(level);
            Debug.Log("Selected " + level.levelName);
        }

        // Keeps track of currently selected button
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


