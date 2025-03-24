using System.Collections;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;

namespace GASHAPWN.UI
{
    [RequireComponent(typeof(GraphicsFaderCanvas))]
    public class CollectionGUI : MonoBehaviour
    {

        [SerializeField] private GameObject infoCard;
        [SerializeField] private GameObject navigationArrows;
        [SerializeField] private GameObject footer;

        private InfoCardGUI infoCardGUI;

        /// PUBLIC METHODS ///

        // CollectionGUISetActive(): If true, fade in CollectionGUI. If false, fade out.
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

        // SwitchFigureGUI: Sets the figure on InfoCard and slides in
        public void SwitchFigureGUI(Figure figure)
        {
            StartCoroutine(infoCardGUI.SlideCardIn());
            infoCardGUI.SetFigureInfoCard(figure);
        }

        /// PRIVATE METHODS ///

        private void Awake()
        {
            infoCardGUI = infoCard.GetComponent<InfoCardGUI>();
        }

    }
}

