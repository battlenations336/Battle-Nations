using BNR;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NanoPanelCtrl : MonoBehaviour
{
    public Text NanoPanel;

    private Player player = new Player();

    public void Init(Player _player)
    {
        player = _player;
        player.Storage.OnChangeNano += BankSystem_OnNanoChanged;
    }

    public void Start()
    {
        Init(GameData.Player);
        UpdatePanel();
    }

    private void OnDestroy()
    {
        if (player.Storage != null && player.Storage.OnChangeNano != null)
            player.Storage.OnChangeNano -= BankSystem_OnNanoChanged;
    }

    private void BankSystem_OnNanoChanged(object sender, System.EventArgs e)
    {
        UpdatePanel();
    }

    void UpdatePanel()
    {
        NanoPanel.text = GameData.Player.Storage.GetNanoAmnt().ToString();
    }
}
