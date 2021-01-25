using BNR;
using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemDetailCtrl : MonoBehaviour
{
    public EventHandler<ButtonEventArgs> OnBuildClick { get; set; }

    public Image Border;
    public Text Description;
    public Button BuildButton;
    public GameObject DataPanel;

    private MessageBoxCtrl messageBoxCtrl;

    Composition composition;
    GameObject prefab;
    GameObject DP1;
    GameObject DP2;
    GameObject DP3;
    GameObject DP4;
    GameObject DP5;
    string config;

    List<GameObject> DataPanes;

    public void BuildButton_OnClick()
    {
        ButtonEventArgs buttonEventArgs = new ButtonEventArgs(ButtonValue.String, config);

        if (GameData.IsBuildingLocked(config))
            return;

        if (GameData.LevelRequirement_Config(config) > GameData.Player.Level)
        {
            messageBoxCtrl.Show(string.Format("You need to be level {0} to unlock this item. Create goods to level up.", GameData.LevelRequirement_Config(config)));
            return;
        }
        if (!IsAffordable())
        {
            messageBoxCtrl.Show(string.Format("Not enough resources to begin construction."));
            return;
        }

        if (!ValidateBuild())
            return;

        if (OnBuildClick != null)
            OnBuildClick(this, buttonEventArgs);
    }

    bool ValidateBuild()
    {
        bool result = true;

        if (composition.componentConfigs.Expansion != null)
        {
            if (Functions.ExpansionInProgress())
            {
                messageBoxCtrl.Show("Expansion already in progress; wait or Hurry to build another.");
                result = false;
            }

            if (result && !Functions.ExpansionAllowed())
            {
                if (Functions.ExpansionCount() == Constants.MaxExpansions)
                {
                    messageBoxCtrl.Show("Your Outpost is fully expanded now.");
                }
                else
                {
                    messageBoxCtrl.Show(string.Format("You must be level {0} to add more expansions - complete missions or collect jobs to continue.", GameData.Player.Level + 1));
                }
                result = false;
            }
        }
        else
        {
            if (composition.componentConfigs.PopulationCapacity != null && !GameData.Player.PopulationValid_Contribution(composition.componentConfigs.PopulationCapacity.contribution))
            {
                messageBoxCtrl.Show(string.Format("Population limit of {0} reached. Level up to build more houses.", GameData.Player.PopulationLimit));
                result = false;
            }
            if (composition.componentConfigs.RequireWorkers != null && !GameData.Player.PopulationValid_Demand(composition.componentConfigs.RequireWorkers.workers))
            {
                messageBoxCtrl.Show(string.Format("Not enough available workers. Build more houses or turn buildings off to free up workers.", GameData.Player.PopulationLimit));
                result = false;
            }
        }

        return (result);
    }

    public void InitFromComposition(string _config, Composition _comp)
    {
        int resourceValue = 0;
        int resourceCount = 0;
        fieldName = "Cost";
        rowFull = true;
        DataPanes = new List<GameObject>();

        Sprite sprite = Resources.Load<Sprite>("UI/resource_moneyicon_0");
        composition = _comp;
        config = _config;

        if (GameData.IsBuildingPremium(config))
        {
            //Border.color = new Color(61f / 255f, 159f / 255f, 11f / 255f, 1);
            Border.color = Functions.GetColor(61f, 159f, 11f);
        }
        else
        {
            Border.color = Functions.GetColor(207f, 79f, 7f, 204f);
        }

        resourceCount = CostItemCount();
        if (DataPanel != null)
        {
            resourceValue = BuildingEntity.GetBuildCost_Money(composition.componentConfigs);
            if (resourceValue > 0)
            {
                AddCost("UI/resource_moneyicon_0", resourceValue.ToString(),
                    (resourceValue <= GameData.Player.Storage.GetGoldAmnt()));
            }
            resourceValue = BuildingEntity.GetBuildCost_Currency(composition.componentConfigs);
            if (resourceValue > 0)
            {
                AddCost("UI/resource_currency@2x", resourceValue.ToString(),
                    (resourceValue <= GameData.Player.Storage.GetNanoAmnt()));
            }
            resourceValue = BuildingEntity.GetBuildCost_Z2Points(composition.componentConfigs);
            if (resourceValue > 0)
            {
                AddCost("UI/community_icon_Z2Live", resourceValue.ToString(),
                    (resourceValue <= GameData.Player.Storage.GetZ2points()));
            }
            if (composition.componentConfigs.Expansion == null)
            {
                foreach (string resource in Functions.ResourceNames())
                {
                    int value = 0;
                    value = (int)Functions.GetPropertyValue(composition.componentConfigs.StructureMenu.cost.resources, resource);
                    if (value > 0)
                    {
                        AddCost(Functions.ResourceToSpriteName(resource), value.ToString(), (value <= GameData.Player.Storage.GetResource(Functions.ResourceNameToEnum(resource))));
                    }
                }
            }

            if (BuildTime() > 0)
            {
                rowFull = true;
                AddCost("UI/resource_time@2x", Functions.GetTaskTime(BuildTime(), 0), true);
            }

            if (PopulationReq() > 0)
            {
                rowFull = true;
                AddCost("UI/resource_population@2x", PopulationReq().ToString(), GameData.Player.PopulationValid_Demand(PopulationReq()));
            }
        }

        if (composition.componentConfigs.StructureMenu.description != null)
        {
            Description.text = string.Format("\n {0}", GameData.GetText(composition.componentConfigs.StructureMenu.description));
        }

        messageBoxCtrl = MessageBoxCtrl.Instance();
    }

    string fieldName;
    bool rowFull = true;

    void AddCost(string spriteName, string value, bool affordable)
    {
        DataPanelCtrl dataCtrl = null;
        Sprite sprite = null;

        sprite = GameData.GetSprite(spriteName);
        if (rowFull)
        {
            GameObject panel = AddPanel();
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
        if (DataPanes != null)
            foreach (GameObject pane in DataPanes)
            {
                Destroy(pane);
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
        return (BuildingEntity.GetBuildTime(composition.componentConfigs));
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

    bool IsAffordable()
    {
        int resourceValue = 0;
        if (composition.componentConfigs.Expansion == null)
        {
            if (!GameData.Player.Affordable(composition.componentConfigs.StructureMenu.cost))
            {
                return (false);
            }
        }
        else
        {
            resourceValue = BuildingEntity.GetBuildCost_Money(composition.componentConfigs);
            if (resourceValue > 0 && resourceValue > GameData.Player.Storage.GetGoldAmnt())
            {
                return (false);
            }
            resourceValue = BuildingEntity.GetBuildCost_Currency(composition.componentConfigs);
            if (resourceValue > 0 && resourceValue > GameData.Player.Storage.GetNanoAmnt())
            {
                return (false);
            }
        }

        return (true);
    }
}
