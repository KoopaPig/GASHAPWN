using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

// adapted from x4000 on Unity Forum

[RequireComponent(typeof(Button))]
public class GeneralButtonControl : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler, ISelectHandler, IDeselectHandler
{
    protected TextMeshProUGUI txt;
    private Button btn;

    public Color textNormalColor;
    public Color textDisabledColor;
    public Color textPressedColor;
    public Color textHighlightedColor;

    void Start()
    {
        txt = GetComponentInChildren<TextMeshProUGUI>();
        btn = gameObject.GetComponent<Button>();
    }

    protected ButtonStatus lastButtonStatus = ButtonStatus.Normal;
    
    protected bool isHighlightDesired = false;
    protected bool isPressedDesired = false;

    protected virtual void Update()
    {
        ButtonStatus desiredButtonStatus = ButtonStatus.Normal;
        if (!btn.interactable)
            desiredButtonStatus = ButtonStatus.Disabled;
        else
        {
            if (isHighlightDesired)
                desiredButtonStatus = ButtonStatus.Highlighted;
            if (isPressedDesired)
                desiredButtonStatus = ButtonStatus.Pressed;
        }

        if (desiredButtonStatus != this.lastButtonStatus)
        {
            this.lastButtonStatus = desiredButtonStatus;
            switch (this.lastButtonStatus)
            {
                case ButtonStatus.Normal:
                    txt.color = textNormalColor;
                    break;
                case ButtonStatus.Disabled:
                    txt.color = textDisabledColor;
                    break;
                case ButtonStatus.Pressed:
                    txt.color = textPressedColor;
                    break;
                case ButtonStatus.Highlighted:
                    txt.color = textHighlightedColor;
                    break;
            }
        }
    }

    public virtual void OnPointerEnter(PointerEventData eventData)
    {
        if (btn.interactable) isHighlightDesired = true;
    }

    public virtual void OnPointerDown(PointerEventData eventData)
    {
        if (btn.interactable) isPressedDesired = true;
    }

    public virtual void OnPointerUp(PointerEventData eventData)
    {
        if (btn.interactable) isPressedDesired = false;
    }

    public virtual void OnPointerExit(PointerEventData eventData)
    {
        if (btn.interactable) isHighlightDesired = false;
    }

    public virtual void OnSelect(BaseEventData eventData)
    {
        if (btn != null && btn.interactable)
        {
            isHighlightDesired = true;
        }
    }

    public virtual void OnDeselect(BaseEventData eventData)
    {
        if (btn != null && btn.interactable)
        {
            isHighlightDesired = false;
        }
    }

    public enum ButtonStatus
    {
        Normal,
        Disabled,
        Highlighted,
        Pressed
    }
}
