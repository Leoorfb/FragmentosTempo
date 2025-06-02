using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Joystick : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    private RectTransform joystickBackground;
    private RectTransform joystickHandle;
    private Vector2 inputDirection = Vector2.zero;
    public float handleLimit = 1.0f;

    public Vector2 InputDirection => inputDirection;

    public void OnPointerDown(PointerEventData eventData)
    {
        OnDrag(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        Vector2 position = Vector2.zero;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(joystickBackground, eventData.position, eventData.pressEventCamera, out position);

        position = position / (joystickBackground.sizeDelta / 2);

        inputDirection = Vector2.ClampMagnitude(position, handleLimit);

        joystickHandle.anchoredPosition = inputDirection * (joystickBackground.sizeDelta / 2);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        inputDirection = Vector2.zero;
        joystickHandle.anchoredPosition = Vector2.zero;
    }
}
