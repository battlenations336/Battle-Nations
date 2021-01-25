using BNR;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PopPanelCtrl : MonoBehaviour
{
    public Text PopPanel;
    Player player = new Player();

    public void Init(Player _player)
    {
        player = _player;
        player.OnChangePopulation += Player_OnPopChanged;
    }

    public void Start()
    {
        Init(GameData.Player);
        UpdatePanel();
    }

    private void OnDestroy()
    {
        if (player.OnChangePopulation != null)
            player.OnChangePopulation -= Player_OnPopChanged;
    }

    void Player_OnPopChanged(object sender, System.EventArgs e)
    {
        UpdatePanel();
    }

    void UpdatePanel()
    {
        PopPanel.text = string.Format("{0}/{1}", GameData.Player.PopulationActive, GameData.Player.Population);
    }    
}
