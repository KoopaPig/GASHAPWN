using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace GASHAPWN.UI {
    [RequireComponent(typeof(Image))]
    public class PanelHighlighter : MonoBehaviour
    {
        [SerializeField] private Color defaultColor = Color.white;
        [SerializeField] private Color highlightedColor = Color.cyan;

        private Image panelImage;

        private void Start()
        {
            panelImage = GetComponent<Image>();
        }

        private void Update()
        {
            GameObject selectedObject = EventSystem.current.currentSelectedGameObject;

            if (selectedObject != null && selectedObject.transform.IsChildOf(this.transform))
            {
                panelImage.color = highlightedColor;
            } else panelImage.color = defaultColor;
        }
    }
}


