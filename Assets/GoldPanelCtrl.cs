using BNR;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GoldPanelCtrl : MonoBehaviour
{
    public Text GoldPanel;

    private Player player = new Player();

    public void Init(Player _player)
    {
        player = _player;
        player.Storage.OnChangeGold += BankSystem_OnGoldChanged;
    }

    public void Start()
    {
        Init(GameData.Player);
        UpdatePanel();
    }

    private void OnDestroy()
    {
        if (player.Storage != null && player.Storage.OnChangeGold != null)
            player.Storage.OnChangeGold -= BankSystem_OnGoldChanged;
    }

    private void BankSystem_OnGoldChanged(object sender, System.EventArgs e)
    {
        UpdatePanel();
    }

    void UpdatePanel()
    {
        GoldPanel.text = GameData.Player.Storage.GetGoldAmnt().ToString();
    }
}
