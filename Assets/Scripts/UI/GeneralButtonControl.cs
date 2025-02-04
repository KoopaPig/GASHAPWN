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

    private ButtonStatus lastButtonStatus = ButtonStatus.Normal;
    
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

    public void OnPointerEnter(PointerEventData eventData)
    {
        isHighlightDesired = true;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        isPressedDesired = true;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isPressedDesired = false;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHighlightDesired = false;
    }

    public void OnSelect(BaseEventData eventData)
    {
        ((ISelectHandler)btn).OnSelect(eventData);
        isHighlightDesired = true;
    }

    public void OnDeselect(BaseEventData eventData)
    {
        ((IDeselectHandler)btn).OnDeselect(eventData);
        isHighlightDesired = false;
    }

    public enum ButtonStatus
    {
        Normal,
        Disabled,
        Highlighted,
        Pressed
    }
}
