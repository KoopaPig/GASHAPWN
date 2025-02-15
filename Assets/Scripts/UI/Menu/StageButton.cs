using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace GASHAPWN.UI {
    public class StageButton : MonoBehaviour
    {
        [SerializeField] private Image levelIcon;
        public GameObject highlightBorder;

        public void SetLevel(Level l)
        {
            levelIcon.sprite = l.levelPreviewIcon;
        }

        private void Awake()
        {
            highlightBorder.SetActive(false);
        }
    }
}


