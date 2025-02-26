using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using Febucci.UI;
using UnityEngine.Events;

[RequireComponent(typeof(Button))]
public class MenuButtonControl : GeneralButtonControl
{
    [SerializeField] private TextAnimator_TMP textAnimator;
    [SerializeField] private Image buttonOutline;

    private void Awake()
    {
        textAnimator.SetBehaviorsActive(false);
    }

    protected override void Update()
    {
        base.Update();
        if (isHighlightDesired)
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
