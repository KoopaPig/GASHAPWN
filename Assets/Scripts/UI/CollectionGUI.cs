using UnityEngine;
using GASHAPWN.Audio;

namespace GASHAPWN.UI
{
    /// <summary>
    /// Controller for Collection Scene GUI
    /// </summary>
    [RequireComponent(typeof(GraphicsFaderCanvas))]
    public class CollectionGUI : MonoBehaviour
    {
        [Header("CollectionGUI Elements")]
            [Tooltip("InfoCard Object")]
            [SerializeField] private GameObject infoCard;

            // Reference to infoCardGUI from infoCard
            private InfoCardGUI infoCardGUI;

            [Tooltip("Navigation Arrows Object")]
            [SerializeField] private GameObject navigationArrows;

            [Tooltip("Footer Object")]
            [SerializeField] private GameObject footer;


        /// PUBLIC METHODS ///

        /// <summary>
        /// If value == true, fade in CollectionGUI. If false, fade out.
        /// </summary> 
        public void CollectionGUISetActive(bool value)
        {
            if (!value) { 
                GetComponent<GraphicsFaderCanvas>().FadeTurnOff(false); 
            }
            else
            {
                GetComponent<GraphicsFaderCanvas>().FadeTurnOn(false);
            }
        }

        /// <summary>
        /// Given Figure, boolean isCollected, and amount, set data on InfoCard and slide it in
        /// </summary>
        public void SwitchFigureGUI(Figure figure, bool isCollected, int amount)
        {
            if (isCollected)
            {
                infoCardGUI.SlideIn();
                infoCardGUI.SetFigureInfoCard(figure, amount);
                UI_SFXManager.Instance.Play_InfoCardGroup();
            } else if (infoCardGUI.isVisible)
            {
                infoCardGUI.SlideOut();
            }
        }


        /// PRIVATE METHODS ///

        private void Awake()
        {
            infoCardGUI = infoCard.GetComponent<InfoCardGUI>();
        }
    }
}