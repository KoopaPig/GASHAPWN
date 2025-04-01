using System.Collections;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;
using EasyTransition;
using UnityEngine.InputSystem;

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

        [Header("Transition Settings")]
        [SerializeField] public TransitionSettings collectionTransition;
        [SerializeField] public string mainMenuSceneName;

        private InputAction cancelAction;


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

        private void OnEnable()
        {
            var inputActionAsset = GetComponent<PlayerInput>().actions;
            cancelAction = inputActionAsset["Cancel"];

            cancelAction.performed += HandleCancel;
            if (!cancelAction.enabled) { cancelAction.Enable(); }
        }

        private void HandleCancel(InputAction.CallbackContext context)
        {
            TransitionManager.Instance().Transition(mainMenuSceneName, collectionTransition, 0);
            GameManager.Instance.UpdateGameState(GameState.Title);
        }

        private void OnDisable()
        {
            cancelAction.performed -= HandleCancel;
            cancelAction.Disable();
        }

    }
}

