
using System;
using UnityEngine;

public class SpriteButtonCtrl : MonoBehaviour
{
    public SpriteRenderer SpriteRenderer;

    public EventHandler OnClick { get; set; }

    public void Start()
    {
        this.SpriteRenderer.sortingOrder = 296;
    }

    private void OnMouseUp()
    {
        if (this.OnClick == null)
            return;
        this.OnClick((object)this, new EventArgs());
    }
}
