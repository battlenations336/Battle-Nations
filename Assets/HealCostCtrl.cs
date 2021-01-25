using BNR;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealCostCtrl : MonoBehaviour
{
    public CostItemCtrl CostItem1;
    public CostItemCtrl CostItem2;
    public CostItemCtrl CostItem3;
    public CostItemCtrl CostItem4;

    CostItemCtrl[] costItems = null;
    bool hospital;

    public void Init()
    {
        costItems = new CostItemCtrl[4];

        costItems[0] = CostItem1;
        costItems[1] = CostItem3;
        costItems[2] = CostItem2;
        costItems[3] = CostItem4;
    }

    public void SetUnit(string unitName, bool _hospital)
    {
        hospital = _hospital;

        if (costItems == null)
            Init();

        ClearAll();

        SetItems(unitName);
    }

    void SetItems(string unitName)
    {
        Cost cost = null;
        BattleUnit battleUnit = GameData.BattleUnits[unitName];
        int count = 0;

        costItems[0].Icon.sprite = GameData.GetSprite("UI/" + "resource_time@2x");
        if (hospital == true)
            costItems[0].Text.text = Functions.GetTaskTime(battleUnit.healTime, 0);
        else
            costItems[0].Text.text = Functions.GetTaskTime(battleUnit.buildTime, 0);

        costItems[0].gameObject.SetActive(true);
        count++;

        if (hospital == true)
            cost = battleUnit.healCost;
        else
            cost = battleUnit.cost;

        if (cost.currency > 0)
        {
            costItems[count].Icon.sprite = GameData.GetSprite("UI/" + "resource_currency@2x");
            costItems[count].Text.text = cost.currency.ToString();
            costItems[count].gameObject.SetActive(true);
            count++;
        }

        if (cost.money > 0)
        {
            costItems[count].Icon.sprite = GameData.GetSprite("UI/" + "resource_moneyicon_0");
            costItems[count].Text.text = cost.money.ToString();
            costItems[count].gameObject.SetActive(true);
            count++;
        }

        foreach (string resource in Functions.ResourceNames())
        {
            int value = 0;
            bool warningIssued = false;

            value = (int)Functions.GetPropertyValue(cost.resources, resource);
            if (value > 0)
            {
                if (count < 4)
                {
                    costItems[count].Icon.sprite = GameData.GetSprite(Functions.ResourceToSpriteName(Functions.ResourceNameToEnum(resource)));
                    costItems[count].Text.text = value.ToString();
                    costItems[count].gameObject.SetActive(true);
                    count++;
                }
                else
                {
                    if (!warningIssued)
                    {
                        Debug.Log(string.Format("Heal Cost overflow for unit {0}", unitName));
                        warningIssued = true;
                    }
                }
            }
        }
    }

    void ClearAll()
    {
        foreach (CostItemCtrl ctrl in costItems)
        {
            ctrl.gameObject.SetActive(false);
        }
    }
}
