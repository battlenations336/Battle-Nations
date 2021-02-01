
using BNR;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ModalPanel_BattleResult : MonoBehaviour
{
    private Dictionary<string, GameObject> MenuItems = new Dictionary<string, GameObject>();
    public Text ResultText;
    public Button ButtonOK;
    public GameObject ModalPanelObject;
    public GameObject ModalDialog;
    public GameObject RewardGrid;
    public int rewardMoney;
    private static ModalPanel_BattleResult modalPanel;
    private int count;

    public static ModalPanel_BattleResult Instance()
    {
        if (!(bool)(Object)ModalPanel_BattleResult.modalPanel)
        {
            ModalPanel_BattleResult.modalPanel = Object.FindObjectOfType<ModalPanel_BattleResult>();
            if (!(bool)(Object)ModalPanel_BattleResult.modalPanel)
                Debug.Log((object)"No active modal panel found");
        }
        return ModalPanel_BattleResult.modalPanel;
    }

    public void Show(
      BattleResult battleResult,
      UnityAction OKEvent,
      string encounterId,
      int _rewardMoney)
    {
        this.rewardMoney = _rewardMoney;
        this.ModalPanelObject.SetActive(true);
        this.ModalDialog.SetActive(true);
        this.ButtonOK.onClick.RemoveAllListeners();
        this.ButtonOK.onClick.AddListener(new UnityAction(this.ClosePanel));
        this.ButtonOK.onClick.AddListener(OKEvent);
        switch (battleResult)
        {
            case BattleResult.Victory:
                this.ResultText.text = "Victory!";
                break;
            case BattleResult.Defeat:
                this.ResultText.text = "Defeated!";
                break;
            case BattleResult.Surrender:
                this.ResultText.text = "Surrendered!";
                break;
            default:
                this.ResultText.text = "No idea!";
                break;
        }
        if (string.IsNullOrEmpty(encounterId) || battleResult != BattleResult.Victory)
            return;
        this.BuildList(encounterId);
    }

    public void BuildList(string encounterId)
    {
        BattleEncounterArmy army = GameData.BattleEncounters.armies[encounterId];
        foreach (string key in this.MenuItems.Keys)
            Object.Destroy((Object)this.MenuItems[key]);
        this.MenuItems = new Dictionary<string, GameObject>();
        this.count = 0;
        if (army.rewards == null)
            return;
        if (army.rewards != null && army.rewards.units != null)
        {
            foreach (string key in army.rewards.units.Keys)
            {
                int unit = army.rewards.units[key];
                if (unit > 0)
                    this.AddMenuItem(GameData.GetIcon(GameData.BattleUnits[key].icon), unit.ToString());
            }
        }
        if (army.rewards.amount.money + this.rewardMoney > 0)
            this.AddMenuItem(GameData.GetSprite("UI/resource_moneyicon_0"), (army.rewards.amount.money + this.rewardMoney).ToString());
        if (army.rewards.amount.currency > 0)
            this.AddMenuItem(GameData.GetSprite("UI/resource_currency@2x"), army.rewards.amount.currency.ToString());
        if (army.rewards.amount.resources == null)
            return;
        foreach (string resourceName in Functions.ResourceNames())
        {
            int num = 0;
            object propertyValue = Functions.GetPropertyValue((object)army.rewards.amount.resources, resourceName);
            if (propertyValue != null)
            {
                num = (int)propertyValue;
                if (num > 0)
                    this.AddMenuItem(GameData.GetSprite(Functions.ResourceToSpriteName(resourceName)), num.ToString());
            }
        }
    }

    public void AddMenuItem(Sprite iconSprite, string qty)
    {
        GameObject gameObject = Object.Instantiate<GameObject>((GameObject)UnityEngine.Resources.Load("RewardItem"), new Vector3(0.0f, 0.0f, 0.0f), Quaternion.identity);
        if ((Object)this.RewardGrid != (Object)null)
            gameObject.transform.SetParent(this.RewardGrid.transform);
        RewardItemCtrl component = gameObject.GetComponent<RewardItemCtrl>();
        if ((Object)iconSprite != (Object)null)
            component.Icon.sprite = iconSprite;
        component.Quantity.text = qty;
        ++this.count;
        this.MenuItems.Add(this.count.ToString(), gameObject);
    }

    public void ClosePanel()
    {
        this.ModalDialog.SetActive(false);
        this.ModalPanelObject.SetActive(false);
    }
}
