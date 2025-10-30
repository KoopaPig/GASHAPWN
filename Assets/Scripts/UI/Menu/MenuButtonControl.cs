using Febucci.UI;
using MyBox;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;
using UnityEngine.UI;

namespace GASHAPWN.UI {
    /// <summary>
    /// Extends visual status of menu buttons, inherits from GeneralButtonControl
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class MenuButtonControl : GeneralButtonControl
    {
        [Tooltip("Text Animator within button")]
        [SerializeField] private TextAnimator_TMP textAnimator;

        [Tooltip("Button Outline Component")]
        [SerializeField] private Image buttonOutline;

        [SerializeField] private bool _hasIcon = false;

        [Tooltip("Icon within button that appears on hover")]
        [ConditionalField("_hasIcon")][SerializeField] private Image buttonIcon;

        private void Awake()
        {
            textAnimator.SetBehaviorsActive(false);
            if (_hasIcon)
            {
                buttonIcon.gameObject.SetActive(false);
            }
        }

        protected override void Update()
        {
            base.Update();

            if (isHighlightDesired && GetComponent<Button>().enabled)
            {
                buttonOutline.enabled = true;
                textAnimator.SetBehaviorsActive(true);
                if (_hasIcon)
                {
                    buttonIcon.gameObject.SetActive(true);
                    buttonIcon.color = textHighlightedColor;
                }
            }
            else
            {
                buttonOutline.enabled = false;
                textAnimator.SetBehaviorsActive(false);
                if (_hasIcon)
                {
                    buttonIcon.color = textNormalColor;
                    buttonIcon.gameObject.SetActive(false);
                }
            }
        }
    }
}