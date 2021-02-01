
using BNR;
using GameCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class BarracksDialogCtrl : MonoBehaviour
{
    private Dictionary<string, GameObject> MenuItems = new Dictionary<string, GameObject>();
    public GameObject EmptyHealingPane;
    public GameObject ActiveHealingPane;
    public Button BackButton;
    public Button LeftButton;
    public Button RightButton;
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
    public Text ButtonBuild;
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

    public void InitFromBuildingEntity(BuildingEntity _buildingEntity)
    {
        this.buildingEntity = _buildingEntity;
        this.init();
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
        this.UpgradeBtn.gameObject.SetActive(false);
        if (!this.buildingEntity.IsUnderConstruction() && this.buildingEntity.State != BuildingState.Upgrading && this.buildingEntity.State != BuildingState.Working)
            this.UpgradeBtn.gameObject.SetActive(true);
        this.ButtonBuild.text = "Train";
        this.ReadyText.GetComponent<Text>().text = "Now training:";
        GameObject gameObject = GameObject.Find("MedIcon");
        if ((UnityEngine.Object)gameObject != (UnityEngine.Object)null)
            gameObject.SetActive(false);
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

    public void BuildMenu()
    {
        this.ResetMenu();
        this.MenuItems = new Dictionary<string, GameObject>();
        List<UnitLevel> source = new List<UnitLevel>();
        List<UnitLevel> unitLevelList = new List<UnitLevel>();
        foreach (string job in this.buildingEntity.GetJobList())
        {
            BattleUnit battleUnit = GameData.BattleUnits[job];
            double level = 99.0;
            if (battleUnit.prereq != null)
            {
                foreach (string key in battleUnit.prereq.Keys)
                {
                    PrereqConfig prereqConfig = battleUnit.prereq[key];
                    if (prereqConfig._t == "LevelPrereqConfig")
                        level = prereqConfig.level;
                }
            }
            source.Add(new UnitLevel(job, level, battleUnit.cost.money > 0 ? battleUnit.cost.money : battleUnit.cost.currency * 10000));
        }
        foreach (UnitLevel unitLevel in (IEnumerable<UnitLevel>)source.OrderBy<UnitLevel, double>((Func<UnitLevel, double>)(x => x.Level)).ThenBy<UnitLevel, int>((Func<UnitLevel, int>)(x => x.Cost)).ThenBy<UnitLevel, string>((Func<UnitLevel, string>)(x => x.Name)))
            this.AddMenuItem(unitLevel.Name, 0);
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
        itemCtrl.Name.text = string.IsNullOrEmpty(GameData.BattleUnits[name].shortName) ? GameData.GetText(GameData.BattleUnits[name].name) : GameData.GetText(GameData.BattleUnits[name].shortName);
        itemCtrl.Count.text = string.Empty;
        itemCtrl.Count.gameObject.transform.parent.gameObject.SetActive(false);
        if (this.IsUnitLocked(name))
            itemCtrl.LockItem();
        itemCtrl.GetComponent<Button>().onClick.AddListener((UnityAction)(() => this.ExecuteButton(itemCtrl)));
        this.MenuItems.Add(name, gameObject);
    }

    public BuildingEntity GetBuildingEntity()
    {
        return this.buildingEntity;
    }

    private void ExecuteButton(RepairItemCtrl itemCtrl)
    {
        this.SelectionChanged(itemCtrl.ConfigName);
        this.HealUnitCtrl.Name.text = itemCtrl.Name.text;
        this.HealUnitCtrl.Icon.sprite = itemCtrl.Icon.sprite;
    }

    private bool IsUnitLocked(string name)
    {
        bool flag = false;
        if (GameData.IsUnitLocked(name))
            flag = true;
        return flag;
    }

    public void HealButton_OnClick()
    {
        if (!this.IsOpen() || string.IsNullOrEmpty(this.selectedUnit))
            return;
        if (GameData.LevelRequirement_Unit(this.selectedUnit) > GameData.Player.Level)
            this.messageBoxCtrl.Show(string.Format("You need to be level {0} to unlock this unit. Create goods to level up.", (object)GameData.LevelRequirement_Unit(this.selectedUnit)));
        else if (!GameData.Player.Affordable(GameData.BattleUnits[this.selectedUnit].cost))
        {
            this.messageBoxCtrl.Show("Not enough resource/money to build this unit");
        }
        else
        {
            if (this.buildingEntity == null || !(this.buildingEntity.JobName == string.Empty) || (string.IsNullOrEmpty(this.selectedUnit) || !this.IsOpen()) || this.IsUnitLocked(this.selectedUnit))
                return;
            this.SetSoundClip("UI04-assigning_jobs.caf");
            this.SetActivePane(this.selectedUnit);
            this.buildingEntity.StartJob(this.selectedUnit);
            this.SelectionChanged(string.Empty);
        }
    }

    public void SetSoundClip(string _audioClip)
    {
        this.audioSource = this.GetComponent<AudioSource>();
        this.busyClip = UnityEngine.Resources.Load<AudioClip>("Audio/" + _audioClip);
        if (!((UnityEngine.Object)this.audioSource != (UnityEngine.Object)null) || !((UnityEngine.Object)this.busyClip != (UnityEngine.Object)null))
            return;
        this.audioSource.clip = this.busyClip;
        this.audioSource.loop = false;
        this.audioSource.Play();
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
        this.HealCostCtrl.SetUnit(this.selectedUnit, false);
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

    public void UpgradeButton_OnClick()
    {
        if (this.buildingEntity == null || !(this.buildingEntity.JobName == string.Empty) || (!this.IsOpen() || this.OnUpgrade == null))
            return;
        this.OnUpgrade((object)this, new EventArgs());
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
