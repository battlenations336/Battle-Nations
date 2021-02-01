using BNR;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PvPTurnCtrl : MonoBehaviour
{
    public GameObject BannerDialog;
    public Image Banner;
    public Text Message;

    private static PvPTurnCtrl panel;

    float waitTime;
    float maxTime = 5.0f;

    private void Update()
    {
        float delta = Time.deltaTime;

        waitTime += delta;
        if (waitTime > maxTime)
            ClosePanel();
    }

    public static PvPTurnCtrl Instance()
    {
        if (!panel)
        {
            panel = FindObjectOfType<PvPTurnCtrl>() as PvPTurnCtrl;
            if (!panel)
                Debug.Log("No active panel found");
        }

        return (panel);
    }

    public void Show(bool opponent)
    {
        BannerDialog.SetActive(true);

        if (opponent)
        {
            Banner.sprite = GameData.GetSprite("UI/" + "Pvp_textBGopponent@2x");
            Message.text = "Opponents turn";
            Message.color = Functions.GetColor(255, 0, 0);
        }
        else
        {
            Banner.sprite = GameData.GetSprite("UI/" + "Pvp_textBGyou@2x");
            Message.text = "Your turn";
            Message.color = Functions.GetColor(0, 255, 0);

        }

        waitTime = 0.0f;
    }

    public void ClosePanel()
    {
        BannerDialog.SetActive(false);
    }
}
