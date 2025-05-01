using System.Collections;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;
using EasyTransition;
using UnityEngine.InputSystem;
using GASHAPWN.Audio;

namespace GASHAPWN.UI
{
    [RequireComponent(typeof(GraphicsFaderCanvas))]
    public class CollectionGUI : MonoBehaviour
    {
        [Header("CollectionGUI Elements")]
        [SerializeField] private GameObject infoCard;
        private InfoCardGUI infoCardGUI;
        [SerializeField] private GameObject navigationArrows;
        [SerializeField] private GameObject footer;

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
        public void SwitchFigureGUI(Figure figure, bool isCollected)
        {
            if (isCollected)
            {
                StartCoroutine(infoCardGUI.SlideCardIn());
                infoCardGUI.SetFigureInfoCard(figure);
                UI_SFXManager.Instance.Play_InfoCardGroup();
            } else if (infoCardGUI.isVisible)
            {
                UI_SFXManager.Instance.Play_InfoCardGroup();
                StartCoroutine(infoCardGUI.SlideCardOut());
            }
        }

        /// PRIVATE METHODS ///

        private void Awake()
        {
            infoCardGUI = infoCard.GetComponent<InfoCardGUI>();
        }
    }
}

