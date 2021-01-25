
using BNR;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class OnUnitSelected : UnityEvent<string>
{
}

public class UnitPanel : MonoBehaviour
{
    public OnUnitSelected unitEvent;
    public GameObject UnitPanelObject;
    public GameObject Canvas;
    public GameObject TabBar;
    private Dictionary<string, Button> unitButtons;
    private static UnitPanel unitPanel;

    private void Awake()
    {
        if (this.unitEvent == null)
            this.unitEvent = new OnUnitSelected();
        this.unitButtons = new Dictionary<string, Button>();
    }

    public static UnitPanel Instance()
    {
        if (!(bool)(Object)UnitPanel.unitPanel)
        {
            UnitPanel.unitPanel = Object.FindObjectOfType<UnitPanel>();
            if (!(bool)(Object)UnitPanel.unitPanel)
                Debug.Log((object)"No active unit panel found");
        }
        return UnitPanel.unitPanel;
    }

    private void Start()
    {
        this.Build();
    }

    public void Build()
    {
        foreach (Component component in this.unitButtons.Values)
            Object.Destroy((Object)component.gameObject);
        this.unitButtons = new Dictionary<string, Button>();
        foreach (ArmyUnit armyUnit in GameData.Player.Army.Values)
            this.AddButton(armyUnit.Name, Path.GetFileNameWithoutExtension(armyUnit.GetBattleUnit().icon), armyUnit.AvailableQty(), armyUnit.total, GameData.IsUnitPremium(armyUnit.Name));
    }

    public void AddButton(string unitName, string iconName, int available, int total, bool premium)
    {
        GameObject original = (GameObject)UnityEngine.Resources.Load("UnitButton");
        original.name = unitName;
        GameObject gameObject = Object.Instantiate<GameObject>(original, Vector3.zero, Quaternion.identity);
        gameObject.transform.SetParent(this.UnitPanelObject.transform);
        Button component1 = gameObject.GetComponent<Button>();
        UnitButton component2 = gameObject.GetComponent<UnitButton>();
        component1.onClick.RemoveAllListeners();
        component1.onClick.AddListener((UnityAction)(() => this.SelectUnit(unitName)));
        if (premium)
        {
            Sprite _sprite = (Sprite)UnityEngine.Resources.Load("UI/starburst@2x", typeof(Sprite));
            component2.SetBackgroundImage(_sprite);
        }
        component2.Available.text = available.ToString();
        component2.Total.text = total.ToString();
        string path = "Icons/" + iconName;
        Sprite _sprite1 = UnityEngine.Resources.Load<Sprite>(path);
        if ((Object)_sprite1 == (Object)null && !path.EndsWith("@2x"))
        {
            path = string.Format("{0}@2x", (object)path);
            _sprite1 = UnityEngine.Resources.Load<Sprite>(path);
        }
        if ((Object)_sprite1 == (Object)null)
            Debug.Log((object)string.Format("Unit icon missing - {0}", (object)path));
        else
            component2.SetUnitImage(_sprite1);
        if (GameData.Player.Army[unitName].Upgrading || GameData.Player.Army[unitName].ReadyToPromote())
            component2.Promote.gameObject.SetActive(true);
        else
            component2.Promote.gameObject.SetActive(false);
        this.unitButtons.Add(unitName, component1);
    }

    public void UpdateButton(string name, string available)
    {
        Button unitButton = this.unitButtons[name];
        if (!((Object)unitButton != (Object)null))
            return;
        unitButton.GetComponent<UnitButton>().Available.text = available;
    }

    public void SelectUnit(string UnitName)
    {
        if (this.unitEvent == null)
            return;
        this.unitEvent.Invoke(UnitName);
    }

    public void ClosePanel()
    {
        this.UnitPanelObject.SetActive(false);
    }
}
