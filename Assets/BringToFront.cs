using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BringToFront : MonoBehaviour, IPointerUpHandler, IPointerDownHandler, IPointerClickHandler
{
    public EventHandler OnClose { get; set; }

    public Image Background;

    public void SetAlphaLevel(float _level)
    {
        if (Background != null)
        {
            Color color = Background.color;
            color.a = _level;
            Background.color = color;
        }
    }

    private void OnDisable()
    {
        SetAlphaLevel(0.0f);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (OnClose != null)
            OnClose(this, new EventArgs());
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        //throw new NotImplementedException();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        //throw new NotImplementedException();
    }
}
