
using BNR;
using GameCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class HospitalDialogCtrl : MonoBehaviour
{
    private Dictionary<string, GameObject> MenuItems = new Dictionary<string, GameObject>();
    private string injuredStr = string.Empty;
    public GameObject EmptyHealingPane;
    public GameObject ActiveHealingPane;
    public Button BackButton;
    public Button LeftButton;
    public Button RightButton;
    public Text RightButtonText;
    public Text HealCostText;
    public Button HurryButton;
    public Button CollectButton;
    public Button UpgradeBtn;
    public GameObject ReadyText;
    public Text Name;
    public GameObject MenuGrid;
    public HealUnitCtrl HealUnitCtrl;
    public HealCostCtrl HealCostCtrl;
    public Text TimeLeftText;
    public Text HurryCostText;
    public GameObject ProgressBar;
    public Image Footer;
    private AudioSource audioSource;
    private AudioClip busyClip;
    private BuildingEntity buildingEntity;
    private MessageBoxCtrl messageBoxCtrl;
    private string selectedUnit;
    private bool dialogOpen;

    public EventHandler OnClose { get; set; }

    public EventHandler<HurryEventArgs> OnHurry { get; set; }

    public EventHandler<CollectEventArgs> OnCollect { get; set; }

    public EventHandler OnUpgrade { get; set; }

    public void InitFromBuildingEntity(BuildingEntity _buildingEntity)
    {
        this.buildingEntity = _buildingEntity;
        this.init();
    }

    public void InitButtons()
    {
        this.UnsetButtons();
        this.HurryButton.GetComponent<Button>().onClick.AddListener(new UnityAction(this.HurryButton_OnClick));
        this.CollectButton.GetComponent<Button>().onClick.AddListener(new UnityAction(this.CollectButton_OnClick));
        this.UpgradeBtn.GetComponent<Button>().onClick.AddListener(new UnityAction(this.UpgradeButton_OnClick));
    }

    private void UnsetButtons()
    {
        this.UpgradeBtn.GetComponent<Button>().onClick.RemoveAllListeners();
        this.CollectButton.GetComponent<Button>().onClick.RemoveAllListeners();
        this.HurryButton.GetComponent<Button>().onClick.RemoveAllListeners();
    }

    private void initFromStateChange(object sender, BuildingEventArgs args)
    {
        this.init();
    }

    public void Refresh()
    {
        this.init();
    }

    public void init()
    {
        this.dialogOpen = true;
        if ((UnityEngine.Object)UnityEngine.Resources.Load<Sprite>(string.Format("Icons/{0}", (object)GameData.NormaliseIconName(this.buildingEntity.composition.componentConfigs.StructureMenu.icon))) == (UnityEngine.Object)null)
            Debug.Log((object)string.Format("Factory sprite {0} not found", (object)GameData.NormaliseIconName(this.buildingEntity.composition.componentConfigs.StructureMenu.icon)));
        this.Name.text = string.Format("{0} Lvl. {1}", (object)GameData.GetText(this.buildingEntity.composition.componentConfigs.StructureMenu.name), (object)this.buildingEntity.Level);
        this.UpgradeBtn.gameObject.SetActive(true);
        if (this.buildingEntity.composition.componentConfigs.Healing != null)
        {
            this.ReadyText.GetComponent<Text>().text = GameData.GetText(this.buildingEntity.composition.componentConfigs.Healing.healUnitStr);
            this.RightButtonText.GetComponent<Text>().text = GameData.GetText(this.buildingEntity.composition.componentConfigs.Healing.healUnitStr);
            this.HealCostText.GetComponent<Text>().text = GameData.GetText(this.buildingEntity.composition.componentConfigs.Healing.healCostStr);
            this.injuredStr = !(this.buildingEntity.composition.componentConfigs.Healing.healTimeStr == "hospitalTime") ? "damaged" : "injured";
        }
        this.BuildMenu();
        this.InitButtons();
        this.selectedUnit = string.Empty;
        this.ShowHealingPane();
        this.messageBoxCtrl = MessageBoxCtrl.Instance();
    }

    public void ResetMenu()
    {
        foreach (string key in this.MenuItems.Keys)
            UnityEngine.Object.Destroy((UnityEngine.Object)this.MenuItems[key]);
    }

    public BuildingEntity GetBuildingEntity()
    {
        return this.buildingEntity;
    }

    public void BuildMenu()
    {
        this.ResetMenu();
        this.MenuItems = new Dictionary<string, GameObject>();
        List<UnitLevel> source = new List<UnitLevel>();
        List<UnitLevel> unitLevelList = new List<UnitLevel>();
        foreach (string key1 in GameData.Player.Army.Keys)
        {
            if (GameData.Player.Army[key1].injured != 0)
            {
                BattleUnit battleUnit = GameData.BattleUnits[key1];
                double level = 99.0;
                if (battleUnit.prereq != null)
                {
                    foreach (string key2 in battleUnit.prereq.Keys)
                    {
                        PrereqConfig prereqConfig = battleUnit.prereq[key2];
                        if (prereqConfig._t == "LevelPrereqConfig")
                            level = prereqConfig.level;
                    }
                }
                source.Add(new UnitLevel(key1, level, battleUnit.cost.money > 0 ? battleUnit.cost.money : battleUnit.cost.currency * 10000));
            }
        }
        foreach (UnitLevel unitLevel in (IEnumerable<UnitLevel>)source.OrderBy<UnitLevel, double>((Func<UnitLevel, double>)(x => x.Level)).ThenBy<UnitLevel, int>((Func<UnitLevel, int>)(x => x.Cost)).ThenBy<UnitLevel, string>((Func<UnitLevel, string>)(x => x.Name)))
        {
            ArmyUnit armyUnit = GameData.Player.Army[unitLevel.Name];
            if (((IEnumerable<string>)GameData.BattleUnits[unitLevel.Name].tags).Contains<string>(this.buildingEntity.composition.componentConfigs.Healing.tag))
                this.AddMenuItem(unitLevel.Name, GameData.Player.Army[unitLevel.Name].injured);
        }
    }

    public void AddMenuItem(string name, int injured)
    {
        GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>((GameObject)UnityEngine.Resources.Load("RepairMenuItem"), new Vector3(0.0f, 0.0f, 0.0f), Quaternion.identity);
        gameObject.transform.position = new Vector3(8.8f, 0.9f, 0.0f);
        if ((UnityEngine.Object)this.MenuGrid != (UnityEngine.Object)null)
            gameObject.transform.SetParent(this.MenuGrid.transform, false);
        RepairItemCtrl itemCtrl = gameObject.GetComponent<RepairItemCtrl>();
        Sprite icon = GameData.GetIcon(GameData.BattleUnits[name].icon);
        itemCtrl.ConfigName = name;
        itemCtrl.Icon.sprite = icon;
        itemCtrl.Name.text = GameData.GetText(GameData.BattleUnits[name].name);
        itemCtrl.Count.text = string.Format("{0} {1}", (object)injured, (object)this.injuredStr);
        itemCtrl.GetComponent<Button>().onClick.AddListener((UnityAction)(() => this.ExecuteButton(itemCtrl)));
        this.MenuItems.Add(name, gameObject);
    }

    private void ExecuteButton(RepairItemCtrl itemCtrl)
    {
        this.SelectionChanged(itemCtrl.ConfigName);
        this.HealUnitCtrl.Name.text = itemCtrl.Name.text;
        this.HealUnitCtrl.Icon.sprite = itemCtrl.Icon.sprite;
    }

    public void UpgradeButton_OnClick()
    {
        if (this.buildingEntity == null || !(this.buildingEntity.JobName == string.Empty) || (!this.IsOpen() || this.OnUpgrade == null))
            return;
        this.OnUpgrade((object)this, new EventArgs());
    }

    public void HealButton_OnClick()
    {
        if (!this.IsOpen())
            return;
        if (!GameData.Player.Affordable(GameData.BattleUnits[this.selectedUnit].healCost))
        {
            this.messageBoxCtrl.Show("Not enough resource/money to start this job");
        }
        else
        {
            if (this.buildingEntity == null || !(this.buildingEntity.JobName == string.Empty) || !this.IsOpen())
                return;
            this.buildingEntity.StartJob(this.selectedUnit);
            this.SetActivePane(this.selectedUnit);
            this.SelectionChanged(string.Empty);
        }
    }

    public void SetActivePane(string activeUnit)
    {
        ActiveHealingPaneCtrl component = this.ActiveHealingPane.GetComponent<ActiveHealingPaneCtrl>();
        component.Icon.sprite = GameData.GetIcon(GameData.BattleUnits[activeUnit].icon);
        component.Name.text = GameData.GetText(GameData.BattleUnits[activeUnit].name);
    }

    public void UpdateProgressBar()
    {
        if ((UnityEngine.Object)this.ProgressBar != (UnityEngine.Object)null)
            this.ProgressBar.transform.localScale = new Vector3(1f - this.buildingEntity.GetTaskPercent(), 1f);
        if ((UnityEngine.Object)this.TimeLeftText != (UnityEngine.Object)null)
            this.TimeLeftText.text = this.buildingEntity.GetTaskTime();
        if (!((UnityEngine.Object)this.HurryCostText != (UnityEngine.Object)null))
            return;
        this.HurryCostText.text = string.Format("{0}", (object)this.buildingEntity.GetHurryCost());
    }

    private void SelectionChanged(string _selectedUnit)
    {
        this.selectedUnit = _selectedUnit;
        if (this.selectedUnit == string.Empty)
            this.ShowHealingPane();
        else
            this.ShowCostPane();
    }

    private void ShowHealingPane()
    {
        this.ReadyText.SetActive(true);
        this.EmptyHealingPane.SetActive(this.buildingEntity.State == BuildingState.Inactive);
        this.ActiveHealingPane.SetActive(this.buildingEntity.State != BuildingState.Inactive);
        if (!string.IsNullOrEmpty(this.buildingEntity.JobName) && this.buildingEntity.State != BuildingState.Inactive)
        {
            this.SetActivePane(this.buildingEntity.JobName);
            this.RightButton.gameObject.SetActive(false);
            this.HurryButton.gameObject.SetActive(this.buildingEntity.State != BuildingState.Full);
            this.CollectButton.gameObject.SetActive(this.buildingEntity.State == BuildingState.Full);
        }
        else
        {
            this.RightButton.gameObject.SetActive(true);
            this.HurryButton.gameObject.SetActive(false);
            this.CollectButton.gameObject.SetActive(false);
        }
        this.HealUnitCtrl.gameObject.SetActive(false);
        this.HealCostCtrl.gameObject.SetActive(false);
        this.LeftButton.gameObject.SetActive(false);
        this.Footer.sprite = GameData.GetSprite("UI/MilitaryBuildingBottom@2x");
    }

    private void ShowCostPane()
    {
        this.ReadyText.SetActive(false);
        this.EmptyHealingPane.SetActive(false);
        this.ActiveHealingPane.SetActive(false);
        this.HealUnitCtrl.gameObject.SetActive(true);
        this.HealCostCtrl.gameObject.SetActive(true);
        this.HealCostCtrl.SetUnit(this.selectedUnit, true);
        this.LeftButton.gameObject.SetActive(true);
        this.RightButton.gameObject.SetActive(true);
        this.HurryButton.gameObject.SetActive(false);
        this.CollectButton.gameObject.SetActive(false);
        this.Footer.sprite = GameData.GetSprite("UI/HospitalBuildingBottom@2x");
    }

    public void BackBtn_OnClick()
    {
        if (!this.IsOpen())
            return;
        if (this.selectedUnit != string.Empty)
            this.SelectionChanged(string.Empty);
        else
            this.CloseDialog();
    }

    public void HurryButton_OnClick()
    {
        if (!GameData.Player.Affordable(0, this.buildingEntity.GetHurryCost(), (ResourceList)null, 0))
        {
            this.messageBoxCtrl.Show("Not enough nanopods to hurry this job");
        }
        else
        {
            if (this.OnHurry == null)
                return;
            this.OnHurry((object)this, new HurryEventArgs(this.buildingEntity));
        }
    }

    public void CollectButton_OnClick()
    {
        if (this.OnCollect == null)
            return;
        this.OnCollect((object)this, new CollectEventArgs(this.buildingEntity));
    }

    public void CloseDialog()
    {
        this.UnsetButtons();
        this.ResetMenu();
        this.dialogOpen = false;
        if (this.OnClose == null)
            return;
        this.OnClose((object)this, new EventArgs());
    }

    public bool IsOpen()
    {
        return this.dialogOpen;
    }
}
