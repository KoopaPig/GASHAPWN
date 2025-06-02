using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace GASHAPWN.UI {
    /// <summary>
    /// Highlights panel if currently selected
    /// </summary>
    [RequireComponent(typeof(Image))]
    public class PanelHighlighter : MonoBehaviour
    {
        [Tooltip("Non-highlighted color")]
        [SerializeField] private Color defaultColor = Color.white;
        [Tooltip("Highlighted color")]
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