using BNR;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MissionListCtril : MonoBehaviour
{
    public EventHandler<ButtonEventArgs> OnMissionClicked { get; set; }

    Dictionary<string, GameObject> MenuItems = new Dictionary<string, GameObject>();
    public GameObject MissonGrid;
    MissionScriptEngine scriptEngine = new MissionScriptEngine();
    // Start is called before the first frame update
    void Start()
    {
        BuildList();
    }

    public void BuildList()
    {
        foreach (string key in MenuItems.Keys)
        {
            GameObject GO = MenuItems[key];
            Destroy(GO);
        }

        MenuItems = new Dictionary<string, GameObject>();

        foreach (string mission in GameData.Player.CurrentMissions.Keys)
        {
            if (GameData.Player.CurrentMissions[mission].State != GameCommon.MissionState.Completed)
                if (GameData.Missions.ContainsKey(mission))
                    if (GameData.Missions[mission].hideIcon == 0)
                        AddMenuItem(mission);
        }
    }

    public void AddMenuItem(string name)
    {
        GameObject prefab = (GameObject)Resources.Load("MissionIconBtn");

        GameObject temp = (GameObject)Instantiate(prefab, new Vector3(0, 0, 0), Quaternion.identity);
        //temp.transform.position = new Vector3(8.8f, 0.9f, 0f);

        if (MissonGrid != null)
            temp.transform.SetParent(MissonGrid.transform);

        IconButtonCtrl itemCtrl = temp.GetComponent<IconButtonCtrl>();

        string giver = GameData.Missions[name].giver;
        string characterIcon = string.Empty;
        Sprite sprite = null;

        string icon = scriptEngine.GetCurrentIcon(name, GameData.Missions[name].objectives, string.Empty);
        if (icon == null)
        {
            if (giver != null && giver != string.Empty)
                characterIcon = GameData.Characters[giver].smallIcon;
            if (characterIcon != null && characterIcon != string.Empty)
                sprite = GameData.GetIcon(characterIcon);
        }
        else
        {
            sprite = GameData.GetIcon(icon);
        }

        if (sprite != null)
            itemCtrl.Icon.sprite = sprite;

        var button = itemCtrl.GetComponent<Button>() as Button;
        button.onClick.AddListener(delegate { ExecuteButton(name); });

        MenuItems.Add(name, temp);
    }

    void ExecuteButton(string name)
    {
        if (OnMissionClicked != null)
            OnMissionClicked(this, new ButtonEventArgs(ButtonValue.OK, name));
    }
}
