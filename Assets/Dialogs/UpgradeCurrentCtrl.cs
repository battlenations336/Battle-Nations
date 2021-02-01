using BNR;
using UnityEngine;
using UnityEngine.UI;

public class UpgradeCurrentCtrl : MonoBehaviour
{
    public Image Border;
    public Image ItemIcon;
    public Image TypeIcon;
    public Text Line1;
    public Text Line2;
    public GameObject CostLine1;
    public GameObject CostLine2;

    Composition comp;
    BuildingEntity entity;

    public void InitFromInstance(BuildingEntity _entity, string _config)
    {
        entity = _entity;
        comp = entity.composition;

        if (GameData.IsBuildingPremium(_config))
        {
            Border.color = new Color(61f / 255f, 159f / 255f, 11f / 255f, 1);
        }
        else
        {
            Border.color = Functions.GetColor(207f, 79f, 7f, 204f);
        }

        if (entity.IsHospital())
            populateHospital();
        else if (entity.IsProject())
            populateProject();
        else if (entity.IsResourceProducer())
            populateResourceProducer();
        else
            populateGeneral();
    }

    void populateGeneral()
    {
        string iconName = GameData.NormaliseIconName(string.Format("{0}", comp.componentConfigs.StructureMenu.icon));
        Sprite icon = Resources.Load<Sprite>("Icons/" + iconName) as Sprite;
        ItemIcon.sprite = icon;

        Line1.text = string.Format("Level {0}", entity.Level);
        Line2.text = GameData.GetText(comp.componentConfigs.StructureMenu.name.ToLower());
        Sprite spr = Resources.Load<Sprite>("UI/" + GameData.NormaliseIconName(comp.componentConfigs.StructureMenu.roleIconName));
        if (spr == null)
        {
            Debug.Log("Symbol " + GameData.NormaliseIconName(comp.componentConfigs.StructureMenu.roleIconName) + " not found");
            spr = Resources.Load<Sprite>("squareBlank");
        }
        TypeIcon.sprite = spr;

        PopulateCostLine(CostLine1, "Cost Reduction", string.Format("{0}%", Input(comp.componentConfigs.buildingUpgrade.levels[entity.Level - 1].input)), string.Empty);
        PopulateCostLine(CostLine2, "Time Reduction", string.Format("{0}%", 100 - comp.componentConfigs.buildingUpgrade.levels[entity.Level - 1].time), string.Empty);
    }

    void populateHospital()
    {    
        string iconName = GameData.NormaliseIconName(string.Format("{0}", comp.componentConfigs.StructureMenu.icon));
        Sprite icon = Resources.Load<Sprite>("Icons/" + iconName) as Sprite;
        ItemIcon.sprite = icon;

        Line1.text = string.Format("Level {0}", entity.Level);
        Line2.text = GameData.GetText(comp.componentConfigs.StructureMenu.name.ToLower());
        Sprite spr = Resources.Load<Sprite>("UI/" + GameData.NormaliseIconName(comp.componentConfigs.StructureMenu.roleIconName));
        if (spr == null)
        {
            Debug.Log("Symbol " + GameData.NormaliseIconName(comp.componentConfigs.StructureMenu.roleIconName) + " not found");
            spr = Resources.Load<Sprite>("squareBlank");
        }
        TypeIcon.sprite = spr;

        PopulateCostLine(CostLine1, "Cost Reduction", string.Format("{0}%", Input(comp.componentConfigs.buildingUpgrade.levels[entity.Level - 1].input)), string.Empty);
        PopulateCostLine(CostLine2, "Time Reduction", string.Format("{0}%", 100 - comp.componentConfigs.buildingUpgrade.levels[entity.Level - 1].time), string.Empty);
    }

    void populateProject()
    {
        string iconName = GameData.NormaliseIconName(string.Format("{0}", comp.componentConfigs.StructureMenu.icon));
        Sprite icon = Resources.Load<Sprite>("Icons/" + iconName) as Sprite;
        ItemIcon.sprite = icon;

        Line1.text = string.Format("Level {0}", entity.Level);
        Line2.text = GameData.GetText(comp.componentConfigs.StructureMenu.name.ToLower());
        Sprite spr = Resources.Load<Sprite>("UI/" + GameData.NormaliseIconName(comp.componentConfigs.StructureMenu.roleIconName));
        if (spr == null)
        {
            Debug.Log("Symbol " + GameData.NormaliseIconName(comp.componentConfigs.StructureMenu.roleIconName) + " not found");
            spr = Resources.Load<Sprite>("squareBlank");
        }
        TypeIcon.sprite = spr;

        PopulateCostLine(CostLine1, "Cost Reduction", string.Format("{0}%", Input(comp.componentConfigs.buildingUpgrade.levels[entity.Level - 1].input)), string.Empty);
        PopulateCostLine(CostLine2, "Time Reduction", string.Format("{0}%", 100 - comp.componentConfigs.buildingUpgrade.levels[entity.Level - 1].time), string.Empty);
    }

    void populateResourceProducer()
    {
        string iconName = GameData.NormaliseIconName(string.Format("{0}", comp.componentConfigs.StructureMenu.icon));
        Sprite icon = Resources.Load<Sprite>("Icons/" + iconName) as Sprite;
        ItemIcon.sprite = icon;

        Line1.text = string.Format("Level {0}", entity.Level);
        Line2.text = GameData.GetText(comp.componentConfigs.StructureMenu.name.ToLower());
        Sprite spr = Resources.Load<Sprite>("UI/" + GameData.NormaliseIconName(comp.componentConfigs.StructureMenu.roleIconName));
        if (spr == null)
        {
            Debug.Log("Symbol " + GameData.NormaliseIconName(comp.componentConfigs.StructureMenu.roleIconName) + " not found");
            spr = Resources.Load<Sprite>("squareBlank");
        }
        TypeIcon.sprite = spr;

        PopulateCostLine(CostLine1, "Production", string.Format("{0}/hr", entity.ResourceOutput(entity.Level).ToString()), Functions.ResourceToSpriteName(comp.componentConfigs.ResourceProducer.outputType));
        PopulateCostLine(CostLine2, "", string.Empty, string.Empty);
    }

    void PopulateCostLine(GameObject gameObject, string label, string value, string icon)
    {
        CostTextCtrl costTextCtrl = gameObject.GetComponent<CostTextCtrl>();

        if (costTextCtrl != null)
        {
            costTextCtrl.Item.text = label;
            costTextCtrl.Cost.text = value;
            if (icon == string.Empty)
                costTextCtrl.Icon.sprite = GameData.GetSprite("squareBlank");
            else
                costTextCtrl.Icon.sprite = GameData.GetSprite(icon);
        }
    }

    int Input(float input)
    {
        return((int)(100 * (1 -input / 150) + 0.5));
    }
}
