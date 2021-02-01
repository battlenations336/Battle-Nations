using BNR;
using System;
using UnityEngine;
using UnityEngine.UI;

public class BuildingUpgradeDialogCtrl : MonoBehaviour
{
    public EventHandler OnClose { get; set; }
    public GameObject CurrentPanel;
    public GameObject LevelPanel;
    public GameObject AdvancedPanel;
    public GameObject FactoryPanel;
    public Button LevelBtn;
    public Text LevelBtnText;
    public Button AdvancedBtn;
    public Text AdvancedBtnText;
    public Text Title;

    private MessageBoxCtrl messageBoxCtrl;

    UpgradeCurrentCtrl upgradeCurrentCtrl;
    UpgradeFactoryCtrl upgradeFactoryCtrl;
    BuildingEntity entity;

    public void InitFromInstance(BuildingEntity _entity, string config)
    {
        Title.text = string.Format("Upgrade to Level {0}", _entity.Level + 1);
        LevelBtnText.text = string.Format("Level {0}", _entity.Level + 1);
        messageBoxCtrl = MessageBoxCtrl.Instance();

        entity = _entity;
        upgradeCurrentCtrl = CurrentPanel.GetComponent<UpgradeCurrentCtrl>();

        if (upgradeCurrentCtrl != null)
            upgradeCurrentCtrl.InitFromInstance(entity, config);


        upgradeFactoryCtrl = FactoryPanel.GetComponent<UpgradeFactoryCtrl>();

        if (upgradeFactoryCtrl != null)
            upgradeFactoryCtrl.InitFromInstance(entity, config);

    }

    bool ValidateBuild()
    {
        bool result = true;

        if (result && Functions.UpgradeInProgress())
        {
            messageBoxCtrl.Show("Upgrade already in progress; wait or Hurry to build another.");
            result = false;
        }

        return (result);
    }

    public void ItemDetail_OnUpgradeLevel()
    {
        if (!ValidateBuild())
            return;

        entity.BeginUpgrade();
        ItemDetail_OnClose();
    }

    public void ItemDetail_OnClose()
    {
        //UpgradeFactoryCtrl upgradeFactoryCtrl = FactoryPanel.GetComponent<UpgradeFactoryCtrl>();
        upgradeFactoryCtrl.ResetPanel();
        //itemDetailCtrl.OnBuildClick -= ItemDetail_OnClick;
        //GameObject.SetActive(false);

        if (OnClose != null)
            OnClose(this, new EventArgs());
    }

}
