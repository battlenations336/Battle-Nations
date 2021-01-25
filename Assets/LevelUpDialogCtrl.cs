using BNR;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelUpDialogCtrl : MonoBehaviour
{
    public EventHandler OnClose { get; set; }

    public Text Title;
    public GameObject RewardGrid;
    public GameObject UnlockedGrid;
    public GameObject Portrait;
    public GameObject RewardDialog;
    public Text RewardText;


    Dictionary<string, GameObject> MenuItems = new Dictionary<string, GameObject>();

    public AudioSource audio;

    int level;
    LevelDialog[] dialog = null;
    int count;

    public void BuildList()
    {
        LevelAward awards = null;
        foreach (string key in MenuItems.Keys)
        {
            GameObject GO = MenuItems[key];
            Destroy(GO);
        }

        MenuItems = new Dictionary<string, GameObject>();
        count = 0;
        if (GameData.Levels[level.ToString()].awards == null)
            return;

        awards = GameData.Levels[level.ToString()].awards;

        if (awards.amount.money > 0)
        {
            AddMenuItem(true, GameData.GetSprite("UI/resource_moneyicon_0"), awards.amount.money.ToString());
        }

        if (awards.amount.currency > 0)
        {
            AddMenuItem(true, GameData.GetSprite("UI/resource_currency@2x"), awards.amount.currency.ToString());
        }

        List<EntityListEntry> unlockedList = new List<EntityListEntry>();

        unlockedList = GameData.GetUnlockedList(level);

        foreach(EntityListEntry unlockedEntry in unlockedList)
        {
            switch(unlockedEntry.Type)
            {
                case EntityType.Unit:
                    AddMenuItem(false, GameData.GetIcon(GameData.BattleUnits[unlockedEntry.Name].icon), string.Empty);
                    break;
                case EntityType.Building:
                    AddMenuItem(false, GameData.GetIcon(GameData.Compositions[unlockedEntry.Name].componentConfigs.StructureMenu.icon), string.Empty);
                    break;
            }
        }

        if (GameData.Levels[level.ToString()].dialog != null)
        {
            LevelDialog scr = GameData.LoadLevelDialog(level)[0];
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

    public void AddMenuItem(bool reward, Sprite iconSprite, string qty)
    {
        GameObject prefab = (GameObject)Resources.Load("RewardItem");

        GameObject temp = (GameObject)Instantiate(prefab, new Vector3(0, 0, 0), Quaternion.identity);

        if (reward)
        {
            if (RewardGrid != null)
                temp.transform.SetParent(RewardGrid.transform);
        }
        else
        {
            if (UnlockedGrid != null)
                temp.transform.SetParent(UnlockedGrid.transform);
        }

        RewardItemCtrl itemCtrl = temp.GetComponent<RewardItemCtrl>();

        if (iconSprite != null)
            itemCtrl.Icon.sprite = iconSprite;

        itemCtrl.Quantity.text = qty;
        //var button = itemCtrl.GetComponent<Button>() as Button;
        //button.onClick.AddListener(delegate { ExecuteButton(name); });
        count++;
        MenuItems.Add(count.ToString(), temp);
    }

    public string SetNextTextBlock(LevelDialog scr)
    {
        string textBlock;

        textBlock = string.Empty;

        if (scr.text[0].body != null)
            textBlock += GameData.GetTitle(scr.text[0].body.ToLower());

        return (textBlock);
    }

    public void SetLevel(int _level)
    {
        level = _level;
        dialog = GameData.LoadLevelDialog(level);
        if (dialog != null)
            Title.text = GameData.GetText(dialog[0].text[0].title);

        BuildList();
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
