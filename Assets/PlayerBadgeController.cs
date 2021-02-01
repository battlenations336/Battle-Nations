using BNR;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerBadgeController : MonoBehaviour
{
    public Text PlayerName;
    public Text LevelText;
    public Text XP_Text;
    public Image ProgressBackground;
    public Image Avatar;
    public GameObject ProgressBar;

    private Player player;

    public void Start()
    {
        Init(GameData.Player);
    }

    private void OnDestroy()
    {
        if (player != null)
            player.OnChangeXP -= PlayerSystem_OnXPChanged;
    }

    public void Init(Player _player)
    {
        player = _player;
        PlayerName.text = _player.Name;
        LevelText.text = string.Format("Lvl. {0}", _player.Level);
        XP_Text.text = _player.XP.ToString();

        ProgressBackground.sprite = Resources.Load<Sprite>("UI/progress_bar_int@2x") as Sprite;
        player.OnChangeXP += PlayerSystem_OnXPChanged;
        UpdateXPBar();
    }

    private void PlayerSystem_OnXPChanged(object sender, System.EventArgs e)
    {
        UpdateXPBar();
    }

    private void UpdateXPBar()
    {
        if (LevelText != null)
            LevelText.text = string.Format("Lvl. {0}", GameData.Player.Level);
        if (ProgressBar != null)
        {
            ProgressBar.transform.localScale = new Vector3(1 - player.GetXPPercent(), 1);
            XP_Text.text = player.XP.ToString();
        }
    }
}
