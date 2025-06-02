using UnityEngine;
using UnityEngine.UI;
using Febucci.UI;

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

        private void Awake()
        {
            textAnimator.SetBehaviorsActive(false);
        }

        protected override void Update()
        {
            base.Update();

            if (isHighlightDesired && GetComponent<Button>().enabled)
            {
                //buttonOutline.GetComponent<Animator>().enabled = true;
                buttonOutline.enabled = true;
                textAnimator.SetBehaviorsActive(true);
            }
            else
            {
                buttonOutline.enabled = false;
                //buttonOutline.GetComponent<Animator>().enabled = false;
                textAnimator.SetBehaviorsActive(false);
            }
        }
    }
}