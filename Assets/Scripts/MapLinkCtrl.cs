using BNR;
using System;
using TMPro;
using UnityEngine;

public class MapLinkCtrl : MonoBehaviour
{
    public EventHandler<ButtonEventArgs> OnClick { get; set; }

    public SpriteRenderer spriteRenderer;
    public string npcId;
    public TextMeshPro LinkName;

    BNR.AnimationInfo info;

    public void Init(BuildingJ building, Vector3 position, string _npcId)
    {
        string animationName;

        info = new BNR.AnimationInfo();
        npcId = _npcId;
        if (building.type == "map_outpost")
            animationName = string.Empty;

        animationName = GameData.Compositions[building.type].componentConfigs.Animation.animations.Default;
        if (animationName == null)
            animationName = string.Empty;
        name = building.type;
        if (animationName != string.Empty && !GameData.AnimationInfo.ContainsKey(animationName))
        {
            animationName = animationName.ToLower();
            if (animationName != string.Empty && !GameData.AnimationInfo.ContainsKey(animationName))
            {
                Debug.Log(string.Format("{0} animation {1} missing", "Default", animationName));
                spriteRenderer.sprite = null;
            }            
        }
        if (animationName != string.Empty && GameData.AnimationInfo.ContainsKey(animationName))
        {
            if (GameData.AnimationInfo.ContainsKey(animationName))
                info = GameData.AnimationInfo[animationName];
            spriteRenderer.sprite = GameData.GetSprite(string.Format("Animations/Buildings/Idle/{0}_0", GameData.Compositions[building.type].componentConfigs.Animation.animations.Default));
        }
        if (info == null)
        {
            info = new BNR.AnimationInfo();
        }

        spriteRenderer.sortingOrder = 144 + 3;
        //        transform.position = position;

        BoxCollider2D boxcol;
        boxcol = GetComponent<BoxCollider2D>();
        boxcol.size = new Vector2(info.width / 100.0f, info.height / 100.0f);
        // GameData.Compositions[building.type].componentConfigs
        SetPosition(position);


        Debug.Log(string.Format("Loading link {0}", npcId));
        if (npcId != null && npcId != string.Empty && GameData.NPCs.ContainsKey(npcId) && GameData.NPCs[npcId].name != null)
            LinkName.text = GameData.GetText(GameData.NPCs[npcId].name);
        else
            LinkName.text = string.Empty;

        if (npcId == "player")
            LinkName.text = "Outpost";

        LinkName.renderer.sortingOrder = 144 + 4;
    }

    public void SetPosition(Vector3 newPos)
    {
        var pivotOffset = setPivot_Link(info);
        //pivotOffset = Vector3.zero;
        transform.position = new Vector3(newPos.x + pivotOffset.x - 0.12f/* - 3.0f*/, newPos.y + pivotOffset.y - 0.06f, 0);        
    }

    Vector2 setPivot_Link(BNR.AnimationInfo _info)
    {
        Vector2 pivot = new Vector2(-(_info.width - Math.Abs(_info.left))/2, _info.height) / 100;
        return (pivot);
    }

    Vector2 setPivot_B(BNR.AnimationInfo _info)
    {
        Vector3 delta = new Vector3(_info.left, (_info.top * -1) - (_info.height), 0);
        delta.y = _info.height - Math.Abs(_info.top);
        Vector2 pivot = new Vector3(Math.Abs(delta.x) / _info.width, (Math.Abs(delta.y) / _info.height));

        return (pivot);
    }

    Vector2 setPivot_L(BNR.AnimationInfo _info)
    {
        Vector3 delta = new Vector3(_info.left, (_info.top * -1) - (_info.height), 0);
        delta.y = _info.top;
        Vector2 pivot = new Vector3(Math.Abs(delta.x) / _info.width, (Math.Abs(delta.y) / _info.height));

        pivot = new Vector2(_info.width, _info.height) / 100;
        return (pivot);
    }


    void OnMouseUp()
    {
        if (OnClick != null)
            OnClick(this, new ButtonEventArgs(ButtonValue.String, npcId));
    }

}

