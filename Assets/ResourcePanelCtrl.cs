
using BNR;
using System;
using UnityEngine;
using UnityEngine.UI;

public class ResourcePanelCtrl : MonoBehaviour
{
    private Player player = new Player();
    public Text T1_1Txt;
    public Text T1_2Txt;
    public Text T1_3Txt;
    public Text T1_4Txt;
    public Text T1_5Txt;
    public Text T2_1Txt;
    public Text T2_2Txt;
    public Text T2_3Txt;
    public Text T3_1Txt;
    public Text T3_2Txt;
    public Text T3_3Txt;
    public Text T3_4Txt;
    public Text T3_5Txt;
    public Text T4_1Txt;
    public Text T4_2Txt;
    public Text T4_3Txt;
    public Text T4_4Txt;
    public Text T4_5Txt;
    public Text T5_1Txt;
    public Text T5_2Txt;
    public Text T5_3Txt;

    public void Init(Player _player)
    {
        this.player = _player;
        this.player.Storage.OnChangeResource += new EventHandler(this.BankSystem_OnResourceChanged);
    }

    public void Start()
    {
        this.Init(GameData.Player);
        this.UpdatePanel();
    }

    private void OnDestroy()
    {
        if (this.player.Storage == null || this.player.Storage.OnChangeResource == null)
            return;
        this.player.Storage.OnChangeResource -= new EventHandler(this.BankSystem_OnResourceChanged);
    }

    private void BankSystem_OnResourceChanged(object sender, EventArgs e)
    {
        this.UpdatePanel();
    }

    private void UpdatePanel()
    {
        this.UpdateResourceIndicator(this.T1_1Txt, Resource.stone);
        this.UpdateResourceIndicator(this.T1_2Txt, Resource.wood);
        this.UpdateResourceIndicator(this.T1_3Txt, Resource.iron);
        this.UpdateResourceIndicator(this.T1_4Txt, Resource.coal);
        this.UpdateResourceIndicator(this.T1_5Txt, Resource.oil);
        this.T2_1Txt.text = GameData.Player.Storage.GetResource(Resource.concrete).ToString();
        this.T2_2Txt.text = GameData.Player.Storage.GetResource(Resource.lumber).ToString();
        this.T2_3Txt.text = GameData.Player.Storage.GetResource(Resource.steel).ToString();
        this.T3_1Txt.text = GameData.Player.Storage.GetResource(Resource.gear).ToString();
        this.T3_2Txt.text = GameData.Player.Storage.GetResource(Resource.bars).ToString();
        this.T3_3Txt.text = GameData.Player.Storage.GetResource(Resource.skull).ToString();
        this.T3_4Txt.text = GameData.Player.Storage.GetResource(Resource.tooth).ToString();
        this.T3_5Txt.text = GameData.Player.Storage.GetResource(Resource.chem).ToString();
        this.T4_1Txt.text = GameData.Player.Storage.GetResource(Resource.sgear).ToString();
        this.T4_2Txt.text = GameData.Player.Storage.GetResource(Resource.sbars).ToString();
        this.T4_3Txt.text = GameData.Player.Storage.GetResource(Resource.stooth).ToString();
        this.T4_4Txt.text = GameData.Player.Storage.GetResource(Resource.sskull).ToString();
        this.T5_1Txt.text = GameData.Player.Storage.GetResource(Resource.star).ToString();
        this.T5_2Txt.text = GameData.Player.Storage.GetResource(Resource.heart).ToString();
    }

    private void UpdateResourceIndicator(Text text, Resource type)
    {
        text.text = GameData.Player.Storage.GetResource(type).ToString();
        if (GameData.Player.Storage.ResourceFull(type))
            text.color = Color.red;
        else
            text.color = Functions.GetColor(144f, 134f, 134f);
    }
}
