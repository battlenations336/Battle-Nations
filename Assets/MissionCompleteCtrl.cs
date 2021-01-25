using BNR;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MissionCompleteCtrl : MonoBehaviour
{
    public Text Title;
    public GameObject MissonGrid;
    public GameObject Portrait;
    public GameObject RewardDialog;
    public Text RewardText;

    public EventHandler OnClose { get; set; }

    string missionId = string.Empty;
    Dictionary<string, GameObject> MenuItems = new Dictionary<string, GameObject>();

    public AudioSource audio;

    int level;
    LevelDialog[] dialog = null;
    int count;

    public void InitFromMission(string _missionId)
    {
        missionId = _missionId;

        if (missionId != string.Empty && GameData.Missions.ContainsKey(_missionId))
        {
            Title.text = GameData.GetText(GameData.Missions[_missionId].title);
            BuildList();

            if (GameData.Missions[_missionId].completeScript != null)
            {
                MissionScript scr = GameData.LoadScript(GameData.Missions[_missionId].completeScript.scriptId)[0];
                var sprite = Resources.Load<Sprite>("NPC/" + GameData.GetCharacter(scr.speaker).largeIcon + "@2x");
                Portrait.SetActive(true);
                Portrait.GetComponent<Image>().sprite = sprite;
                RewardDialog.SetActive(true);
                RewardText.text = SetNextTextBlock(scr);
            }
            else
            {
                Portrait.SetActive(false);
                RewardDialog.SetActive(false);
            }
        }
    }

    public string SetNextTextBlock(MissionScript scr)
    {
        string textBlock;
        
        textBlock = string.Empty;
        //if (scriptLines.text[scriptLineNo].title != null)
        //    textBlock = GameData.GetTitle(scriptLines.text[scriptLineNo].title.ToLower()) + "\n";
        //else
        //    textBlock = "\n";

        if (scr.text[0].body != null)
            textBlock += GameData.GetTitle(scr.text[0].body.ToLower());

        return (textBlock);
    }

    public void BuildList()
    {
        Missions.MissionRewards rewards = null;
        foreach (string key in MenuItems.Keys)
        {
            GameObject GO = MenuItems[key];
            Destroy(GO);
        }

        MenuItems = new Dictionary<string, GameObject>();
        count = 0;
        if (GameData.Missions[missionId].rewards == null)
            return;

        rewards = GameData.Missions[missionId].rewards;
        if (rewards != null && rewards.units != null)
        {
            foreach (string unit in rewards.units.Keys)
            {
                int qty = rewards.units[unit];
                if (qty > 0)
                    AddMenuItem(GameData.GetIcon(GameData.BattleUnits[unit].icon), qty.ToString());
            }
        }

        if (rewards.XP > 0)
        {
            AddMenuItem(GameData.GetSprite("UI/resource_xp@2x"), rewards.XP.ToString());
        }

        if (rewards.amount.money > 0)
        {
            AddMenuItem(GameData.GetSprite("UI/resource_moneyicon_0"), rewards.amount.money.ToString());
        }

        if (rewards.amount.currency > 0)
        {
            AddMenuItem(GameData.GetSprite("UI/resource_currency@2x"), rewards.amount.currency.ToString());
        }

        if (rewards.amount.resources != null)
        {
            foreach (string resource in Functions.ResourceNames())
            {
                int value = 0;
                value = (int)Functions.GetPropertyValue(rewards.amount.resources, resource);
                if (value > 0)
                {
                    
                    AddMenuItem(GameData.GetSprite(Functions.ResourceToSpriteName(resource)), value.ToString());
                }
            }
        }
    }

    public void AddMenuItem(Sprite iconSprite, string qty)
    {
        GameObject prefab = (GameObject)Resources.Load("RewardItem");

        GameObject temp = (GameObject)Instantiate(prefab, new Vector3(0, 0, 0), Quaternion.identity);

        if (MissonGrid != null)
            temp.transform.SetParent(MissonGrid.transform);

        RewardItemCtrl itemCtrl = temp.GetComponent<RewardItemCtrl>();

        if (iconSprite != null)
            itemCtrl.Icon.sprite = iconSprite;

        itemCtrl.Quantity.text = qty;
        //var button = itemCtrl.GetComponent<Button>() as Button;
        //button.onClick.AddListener(delegate { ExecuteButton(name); });
        count++;
        MenuItems.Add(count.ToString(), temp);
    }

    public void SetLevel(int _level)
    {
        level = _level;
        dialog = GameData.LoadLevelDialog(level);
        if (dialog != null)
            Title.text = GameData.GetText(dialog[0].text[0].title);
    }

    public void SetSound(string soundfile)
    {
        AudioClip clip;

        if (audio)
        {
            clip = (AudioClip)Resources.Load<AudioClip>("Audio/" + soundfile);
            if (clip != null)
                audio.PlayOneShot(clip, 0.7F);
        }
    }

    public void CloseDialog()
    {
        if (OnClose != null)
            OnClose(this, new EventArgs());
    }
}
