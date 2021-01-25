using BNR;
using System;
using UnityEngine;

public class ResourceBubbleCtrl : MonoBehaviour
{
    public BuildingEntity Entity;
    public SpriteRenderer Icon;
    public SpriteRenderer TopIcon;

    EventHandler onClick;
    public EventHandler OnClick
    {
        get { return onClick; }
        set { onClick = value; }
    }

    void OnMouseUp()
    {
        if (OnClick != null)
            OnClick(this, new EventArgs());
    }

    public void SetSortingOrder(Point index)
    {
        int order = (144) + index.Y;

        SpriteRenderer SR = GetComponent<SpriteRenderer>();
        SR.sortingOrder = order; // (index.Y) + 144;

        Icon.sortingOrder = order + 1;
    }
}
