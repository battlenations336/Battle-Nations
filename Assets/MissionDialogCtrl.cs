using BNR;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MissionDialogCtrl : MonoBehaviour
{
    public EventHandler OnFinish { get; set; }

    public GameObject LeftPortrait;
    public GameObject RightPortrait;
    public GameObject LeftTail;
    public GameObject RightTail;
    public GameObject NameSays;
    public GameObject TitleText;
    public GameObject DialogText;
    public AudioSource audio;

    List<MissionScript> script;
    MissionScript scriptLines;

    int scriptLineNo;

    Sprite sprite;
    Sprite blankSprite;

    private void Update()
    {
        if (Input.GetKeyUp(KeyCode.Space))
        {
            if (scriptLineNo >= scriptLines.text.Length)
            {
                if (script.Count > 0)
                {
                    InitDialog(script[0]);
                    script.RemoveAt(0);
                }
                else
                {
                    if (OnFinish != null)
                        OnFinish(null, null);
                }
            }
            else
            {
                SetNextTextBlock();
            }
        }
    }

    public void Play(string _scriptId)
    {
        script = new List<MissionScript>();
        var scriptList = GameData.LoadScript(_scriptId);
        if (scriptList == null)
        {
            if (!string.IsNullOrEmpty(_scriptId))
                Debug.Log(string.Format("Script {0} not found", _scriptId));
            if (OnFinish != null)
            {
                OnFinish(this, new EventArgs());
                return;
            }
        }    
        foreach (MissionScript scr in GameData.LoadScript(_scriptId))
        {
            script.Add(scr);
        }

        if (script != null && script.Count > 0)
        {
            InitDialog(script[0]);
            script.RemoveAt(0);
        }
    }

    public void InitDialog(MissionScript _scriptLines)
    {
        scriptLines = _scriptLines;
        scriptLineNo = 0;

        SetNextTextBlock();

        sprite = Resources.Load<Sprite>("NPC/" + GameData.GetCharacter(scriptLines.speaker).largeIcon + "@2x");
        blankSprite = Resources.Load<Sprite>("squareBlank");
        
        if (scriptLines.view == "MissionPopupIconRight")
        {
            LeftPortrait.GetComponent<Image>().sprite = blankSprite;
            RightPortrait.GetComponent<Image>().sprite = sprite;
            LeftTail.SetActive(false);
            RightTail.SetActive(true);
        }
        else
        {
            LeftPortrait.GetComponent<Image>().sprite = sprite;
            RightPortrait.GetComponent<Image>().sprite = blankSprite;
            LeftTail.SetActive(true);
            RightTail.SetActive(false);
        }

        NameSays.GetComponent<Text>().text = string.Format("{0}:", GameData.GetText(GameData.GetCharacter(scriptLines.speaker).name));
    }

    public void SetNextTextBlock()
    {
        string textBlock = string.Empty;
        string textTitle = string.Empty;

        audio.Play();

        if (scriptLines.text[scriptLineNo].title != null)
            textTitle = GameData.GetTitle(scriptLines.text[scriptLineNo].title.ToLower());

        if (scriptLines.text[scriptLineNo].body != null)
            textBlock = formatBlock(GameData.GetTitle(scriptLines.text[scriptLineNo].body.ToLower()));

        if (!string.IsNullOrEmpty(textTitle))
        {
            TitleText.GetComponent<Text>().text = textTitle;
            TitleText.gameObject.SetActive(true);
        }
        else
        {
            TitleText.gameObject.SetActive(false);
        }

        DialogText.GetComponent<Text>().text = textBlock;
        scriptLineNo++;
    }

    public string formatBlock(string block)
    {
        return (block);
    }
}
