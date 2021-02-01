using BNR;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UpgradeFactoryCtrl : MonoBehaviour
{
    public GameObject DataPanel;
    public GameObject Line1;
    public GameObject Line2;
    public GameObject Line3;
    public GameObject Line4;

    Composition composition;
    GameObject prefab;

    string fieldName;
    bool rowFull = true;
    List<GameObject> DataPanes;
    List<GameObject> DataPanels;
    UpgradeLevel upgradeLevel;

    public void InitFromInstance(BuildingEntity entity, string config)
    {
        int resourceCount = 0;
        fieldName = "Cost";

        composition = entity.composition;
        upgradeLevel = composition.componentConfigs.buildingUpgrade.levels[entity.Level - 1];
        rowFull = true;
        DataPanes = new List<GameObject>();
        DataPanels = new List<GameObject>();

        resourceCount = CostItemCount();
        if (DataPanel != null)
        {
            if (upgradeLevel.upgradeCost.money > 0)
            {
                AddCost("UI/resource_moneyicon_0", upgradeLevel.upgradeCost.money.ToString(),
                    (upgradeLevel.upgradeCost.money <= GameData.Player.Storage.GetGoldAmnt()));
            }
            //if (composition.componentConfigs.StructureMenu.cost.currency > 0)
            //{
            //    AddCost("UI/resource_currency@2x", upgradeLevel.upgradeCost.currency.ToString(),
            //        (upgradeLevel.upgradeCost.currency <= GameData.Player.Storage.GetNanoAmnt()));
            //}
            //if (composition.componentConfigs.StructureMenu.cost.z2points > 0)
            //{
            //    AddCost("UI/community_icon_Z2Live", composition.componentConfigs.StructureMenu.cost.z2points.ToString(),
            //        (composition.componentConfigs.StructureMenu.cost.z2points <= GameData.Player.Storage.GetZ2points()));
            //}
            if (upgradeLevel.upgradeCost.resources != null)
            {
                foreach (string resource in Functions.ResourceNames())
                {
                    int value = 0;
                    object result;
                    result = Functions.GetPropertyValue(upgradeLevel.upgradeCost.resources, resource);
                    if (result != null)
                    {
                        value = (int)result;
                        if (value > 0)
                        {
                            AddCost(Functions.ResourceToSpriteName(resource), value.ToString(), (value <= GameData.Player.Storage.GetResource(Functions.ResourceNameToEnum(resource))));
                        }
                    }
                }
            }

            if (BuildTime() > 0)
            {
                fieldName = "Time";
                rowFull = true;
                AddCost("UI/resource_time@2x", Functions.GetTaskTime(BuildTime(), 0), true);
            }
        }
        upgradeLevel = composition.componentConfigs.buildingUpgrade.levels[entity.Level];

        if (entity.IsHospital())
        {
            PopulateCostLine(Line1, "Cost Reduction", string.Format("-{0}%", (int)(upgradeLevel.input / 150 * 100) - Input(upgradeLevel.input)), string.Empty);
            PopulateCostLine(Line2, "Total of", string.Format("{0}%", (int)(upgradeLevel.input / 150 * 100)), string.Empty);
            PopulateCostLine(Line3, "Time Reduction", string.Format("-{0}%", 100 - (int)upgradeLevel.time), string.Empty);
            PopulateCostLine(Line4, "Total of", string.Format("{0}%", (100 - (100 - (int)upgradeLevel.time))), string.Empty);
        } else if (entity.IsResourceProducer())
        {
            string outputType = Functions.ResourceToSpriteName(composition.componentConfigs.ResourceProducer.outputType);
            PopulateCostLine(Line1, "Production", string.Format("+{0}/hr", entity.ResourceOutputIncrease().ToString()), outputType);
            PopulateCostLine(Line2, "Total of", string.Format("{0}/hr", entity.ResourceOutput(entity.Level + 1).ToString()), outputType);
            PopulateCostLine(Line3, string.Empty, string.Empty, string.Empty);
            PopulateCostLine(Line4, string.Empty, string.Empty, string.Empty);
        }
    }

    int Input(float input)
    {
        return ((int)(100 * (1 - input / 150) + 0.5));
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

    void AddCost(string spriteName, string value, bool affordable)
    {
        DataPanelCtrl dataCtrl = null;
        Sprite sprite = null;

        sprite = GameData.GetSprite(spriteName);
        if (rowFull)
        {
            GameObject panel = AddPanel();
            DataPanels.Add(panel);
            dataCtrl = panel.GetComponent<DataPanelCtrl>() as DataPanelCtrl;
            dataCtrl.Fieldname.text = fieldName;
            dataCtrl.Icon1.sprite = sprite;
            dataCtrl.Text1.text = value;
            if (!affordable)
                dataCtrl.Text1.color = Color.red;
            else
                dataCtrl.Text1.color = Color.white;
            DataPanes.Add(panel);
            rowFull = false;
            fieldName = string.Empty;
        }
        else
        {
            dataCtrl = DataPanes.Last().GetComponent<DataPanelCtrl>();
            dataCtrl.Icon2.sprite = sprite;
            dataCtrl.Text2.text = value;
            if (!affordable)
                dataCtrl.Text2.color = Color.red;
            else
                dataCtrl.Text2.color = Color.white;
            rowFull = true;
        }
    }

    GameObject AddPanel()
    {
        prefab = (GameObject)Resources.Load("DataPanel");
        prefab.name = "ItemDetail";

        GameObject temp = (GameObject)Instantiate(prefab, Vector3.zero, Quaternion.identity);
        //temp = GameObject.Find("DataPanel");
        //DataPanelCtrl data = temp.GetComponent<DataPanelCtrl>() as DataPanelCtrl;

        temp.transform.SetParent(DataPanel.transform, false);
        /*
                data.Fieldname.text = name;
                data.Icon1.sprite = sprite;
                int cost = composition.componentConfigs.StructureMenu.cost.money;
                data.Text1.text = cost.ToString();
                if (cost > GameData.Player.Storage.GetGoldAmnt())
                    data.Text1.color = Color.red;
                else
                    data.Text1.color = Color.white;
        */
        return (temp);
    }

    public void ResetPanel()
    {
        foreach (GameObject pane in DataPanes)
        {
            Destroy(pane);
        }
        DataPanes = null;
        foreach (GameObject panel in DataPanels)
        {
            Destroy(panel);
        }
        DataPanels = null;
        foreach (Transform child in DataPanel.transform)
        {
            GameObject.Destroy(child.gameObject);
        }

    }

    void ResetData(GameObject _data)
    {
        DataPanelCtrl data = _data.GetComponent<DataPanelCtrl>() as DataPanelCtrl;
        if (data != null)
        {
            data.Fieldname.text = string.Empty;
            data.Icon1.sprite = null;
            data.Text1.text = string.Empty;
            data.Icon2.sprite = null;
            data.Text2.text = string.Empty;
        }
    }

    int CostItemCount()
    {
        int result = 0;

        if (composition.componentConfigs.StructureMenu.cost.currency > 0)
            result++;
        if (composition.componentConfigs.StructureMenu.cost.money > 0)
            result++;
        foreach (string resource in Functions.ResourceNames())
        {
            int value = 0;
            value = (int)Functions.GetPropertyValue(composition.componentConfigs.StructureMenu.cost.resources, resource);
            if (value > 0)
            {
                result++;
            }
        }

        return (result);
    }

    int BuildTime()
    {
        if (composition.componentConfigs.buildingUpgrade != null)
            return (upgradeLevel.upgradeTime);

        return (0);
    }

    int PopulationReq()
    {
        int result = 0;

        if (composition.componentConfigs.RequireWorkers != null)
        {
            result = composition.componentConfigs.RequireWorkers.workers;
        }

        return (result);
    }
}
