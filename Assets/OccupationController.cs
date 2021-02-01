
using BNR;
using System;
using UnityEngine;

public class OccupationController : MonoBehaviour
{
    public SpriteRenderer occupationIcon;
    public SpriteRenderer identityIcon;
    public BattleMapType MapLayout;
    public EncounterArmy Encounter;

    public EventHandler OnClick { get; set; }

    public int MaxOffence { get; set; }

    public int MaxDefence { get; set; }

    public BattleType Type { get; set; }

    public void Init(int x, int y, string icon)
    {
        this.SetIcon(icon, 146);
        this.transform.position = Functions.CalculateCoord(x, y);
    }

    public void SetIcon(string icon, int sortOrder)
    {
        Sprite icon1 = GameData.GetIcon(icon);
        this.identityIcon.sprite = icon1;
        this.identityIcon.transform.localScale *= Math.Min(50f / icon1.rect.size.x, 50f / icon1.rect.size.y);
        this.identityIcon.sortingOrder = sortOrder;
    }

    private void OnMouseUp()
    {
        if (this.OnClick == null)
            return;
        this.OnClick((object)this, new EventArgs());
    }
}
