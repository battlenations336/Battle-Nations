using BNR;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MissionDetailDialogCtrl : MonoBehaviour
{
    public EventHandler OnClose { get; set; }
    public EventHandler<ButtonEventArgs> OnGoButtonClicked { get; set; }

    Dictionary<string, GameObject> MenuItems = new Dictionary<string, GameObject>();
    public Text Title;
    public GameObject MissonGrid;
    public Button PortraitBtn;
    public Image Portrait;
    public Image HintAlertIcon;
    public GameObject DialogBubble;
    public Text HintText;

    string currentNPCId = string.Empty;
    string missionId = string.Empty;
    int count = 0;

    public void InitFromMission(string _missionId, string NPCId)
    {
        missionId = _missionId;
        currentNPCId = NPCId;

        if (missionId != string.Empty && GameData.Missions.ContainsKey(_missionId))
        {
            Title.text = GameData.GetText(GameData.Missions[_missionId].title);
            BuildList();
        }

        if (GameData.Missions[_missionId].description != null)
        {
            MissionScript scr = GameData.LoadScript(GameData.Missions[_missionId].description)[0];
            var sprite = Resources.Load<Sprite>("NPC/" + GameData.GetCharacter(scr.speaker).largeIcon + "@2x");
            Portrait.GetComponent<Image>().sprite = sprite;
            HintText.text = SetNextTextBlock(scr);

            Portrait.gameObject.SetActive(true);
            PortraitBtn.gameObject.SetActive(true);
            HintAlertIcon.gameObject.SetActive(true);
            DialogBubble.gameObject.SetActive(false);
        }
        else
        {
            Portrait.gameObject.SetActive(false);
            PortraitBtn.gameObject.SetActive(false);
            HintAlertIcon.gameObject.SetActive(false);
            DialogBubble.gameObject.SetActive(false);
        }
    }

    public string SetNextTextBlock(MissionScript scr)
    {
        string textBlock;

        textBlock = string.Empty;

        if (scr.text[0].body != null)
            textBlock += GameData.GetTitle(scr.text[0].body.ToLower());

        return (textBlock);
    }

    public void BuildList()
    {
        foreach (string key in MenuItems.Keys)
        {
            GameObject GO = MenuItems[key];
            Destroy(GO);
        }

        MenuItems = new Dictionary<string, GameObject>();
        count = 0;
        foreach (string objKey in GameData.Missions[missionId].objectives.Keys)
        {
            Missions.Objective objData = GameData.Missions[missionId].objectives[objKey];

            AddMenuItem(objKey, objData, getStepCount(objData, objKey), isComplete(objData, objKey));
        }
    }

    public void AddMenuItem(string seq, Missions.Objective objData, string countText, bool complete)
    {
        GameObject prefab = (GameObject)Resources.Load("ObjectiveItem");

        GameObject temp = (GameObject)Instantiate(prefab, new Vector3(0, 0, 0), Quaternion.identity);
        
        if (MissonGrid != null)
            temp.transform.SetParent(MissonGrid.transform);

        ObjectiveItemCtrl itemCtrl = temp.GetComponent<ObjectiveItemCtrl>();

        Sprite sprite = null;

        if (objData.icon != null && objData.icon != string.Empty)
            sprite = GameData.GetIcon(objData.icon);

        if (sprite != null)
            itemCtrl.Icon.sprite = sprite;

        itemCtrl.Summary.text = GameData.GetText(objData.prereq.objectiveText);
        itemCtrl.Count.text = countText;
        itemCtrl.CheckMark.gameObject.SetActive(complete);
        if (countText != string.Empty && !complete)
        {
            itemCtrl.Count.gameObject.SetActive(true);
        }
        if (string.IsNullOrEmpty(countText) && objData.prereq._t == "EnterOpponentLandPrereqConfig" && currentNPCId != objData.prereq.opponentId)
        {
            itemCtrl.Count.gameObject.SetActive(false);
            itemCtrl.CheckMark.gameObject.SetActive(false);
            itemCtrl.GoButton.gameObject.SetActive(true);
            itemCtrl.GoButtonText.text = "GO";
            itemCtrl.GoButton.onClick.AddListener(() => GoButton_OnClick(objData.prereq.opponentId));
        }
        if (objData.prereq._t == "TurnInPrereqConfig" && !complete)
        {
            itemCtrl.Count.gameObject.SetActive(false);
            itemCtrl.CheckMark.gameObject.SetActive(false);
            itemCtrl.GoButtonText.text = "Give";
            itemCtrl.GoButton.gameObject.SetActive(true);
            if (!GameData.Player.Affordable(objData.prereq.toll.amount))
                itemCtrl.GoButton.interactable = false;
            else
                itemCtrl.GoButton.onClick.AddListener(() => GiveButton_OnClick(objData.prereq.toll.amount, seq));
        }
        itemCtrl.gameObject.SetActive(true);
        //var button = itemCtrl.GetComponent<Button>() as Button;
        //button.onClick.AddListener(delegate { ExecuteButton(name); });
        count++;
        MenuItems.Add(count.ToString(), temp);
    }

    string getStepCount(Missions.Objective objData, string seq)
    {
        string count = string.Empty;

        if (GameData.Player.CurrentMissions[missionId].Steps != null && GameData.Player.CurrentMissions[missionId].Steps.ContainsKey(seq))
        {
            var step = GameData.Player.CurrentMissions[missionId].Steps[seq];
            if (step.Goal > 1)
                count = string.Format("{0}/{1}", step.Count, step.Goal);
        }

        return (count);
    }

    bool isComplete(Missions.Objective objData, string seq)
    {
        bool result = false;

        if (GameData.Player.CurrentMissions[missionId].Steps != null && GameData.Player.CurrentMissions[missionId].Steps.ContainsKey(seq))
        {
            var step = GameData.Player.CurrentMissions[missionId].Steps[seq];
            result = step.Complete;
        }
        else
        {
            MissionScriptEngine missionScriptEngine = new MissionScriptEngine();
            result = missionScriptEngine.CheckObjective(objData, seq, missionId, string.Empty);
        }

        return (result);
    }

    public void ToggleHintText()
    {
        if (HintAlertIcon.gameObject.activeSelf == true)
        {
            HintAlertIcon.gameObject.SetActive(false);
            DialogBubble.gameObject.SetActive(true);
        }
        else
        {
            HintAlertIcon.gameObject.SetActive(true);
            DialogBubble.gameObject.SetActive(false);
        }
    }

    public void GiveButton_OnClick(Cost cost, string seq)
    {
        GameData.Player.Storage.DebitStorage(cost);
        GameData.Player.CurrentMissions[missionId].Steps[seq].Complete = true;
        BuildList();
    }

    public void GoButton_OnClick(string opponentId)
    {
        if (OnGoButtonClicked != null)
            OnGoButtonClicked(this, new ButtonEventArgs(ButtonValue.OK, opponentId));
    }

    public void ReplayButton_OnClick()
    {
        if (GameData.Player.CurrentMissions[missionId].HasBattleStep())
        {
            if (!GameData.Player.CurrentMissions[missionId].HasEncounterSpawn())
            {
                GameData.Player.CurrentMissions[missionId].State = GameCommon.MissionState.Initialising;
                if (GameData.Missions[missionId].startScript != null)
                    GameData.Player.CurrentMissions[missionId].State = GameCommon.MissionState.StartDialog;
                else
                    GameData.Player.CurrentMissions[missionId].State = GameCommon.MissionState.Initialising;
                //MissionScriptEngine missionScriptEngine = new MissionScriptEngine();
                //missionScriptEngine.CheckStartEffects(missionId);
            }
            CloseDialog();
        }
    }

    public void CloseDialog()
    {
        if (OnClose != null)
            OnClose(this, new EventArgs());
    }
}
