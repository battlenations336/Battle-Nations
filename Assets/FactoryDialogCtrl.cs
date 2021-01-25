
using BNR;
using GameCommon;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class FactoryDialogCtrl : MonoBehaviour
{
    private Dictionary<string, GameObject> MenuItems = new Dictionary<string, GameObject>();
    public Image Icon;
    public Button UpgradeBtn;
    public Button HurryBtn;
    public Button CollectBtn;
    public Text Name;
    public Text Level;
    public GameObject PopulationIcon;
    public Text Workers;
    public Image TypeIcon;
    public Text TimeLeftTitle;
    public Text TimeLeftText;
    public Text HurryCostText;
    public Text ActivityText;
    public Toggle ActiveBtn;
    public GameObject ProgressBar;
    public GameObject ProgressBarObject;
    public GameObject Grid;
    public GameObject Content;
    public GameObject ContentShop;
    private AudioSource audioSource;
    private AudioClip busyClip;
    private BuildingEntity buildingEntity;
    private MessageBoxCtrl messageBoxCtrl;
    private bool isOpen;
    private Sprite UpgradeActive;
    private Sprite UpgradeInactive;

    public EventHandler OnClose { get; set; }

    public EventHandler<CollectEventArgs> OnCollect { get; set; }

    public EventHandler<HurryEventArgs> OnHurry { get; set; }

    public EventHandler OnUpgrade { get; set; }

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

    private void init()
    {
        Sprite sprite = UnityEngine.Resources.Load<Sprite>(string.Format("Icons/{0}", (object)GameData.NormaliseIconName(this.buildingEntity.composition.componentConfigs.StructureMenu.icon)));
        if ((UnityEngine.Object)sprite == (UnityEngine.Object)null)
            Debug.Log((object)string.Format("Factory sprite {0} not found", (object)GameData.NormaliseIconName(this.buildingEntity.composition.componentConfigs.StructureMenu.icon)));
        this.UpgradeActive = UnityEngine.Resources.Load<Sprite>("UI/btn-upgrade-smlMod@2x");
        this.UpgradeInactive = UnityEngine.Resources.Load<Sprite>("UI/btn-upgrade-smlMod-gray@2x");
        this.Icon.sprite = sprite;
        this.Name.text = GameData.GetText(this.buildingEntity.composition.componentConfigs.StructureMenu.name);
        this.Level.text = string.Format("Level {0}", (object)this.buildingEntity.Level);
        if (this.buildingEntity.composition.componentConfigs.RequireWorkers != null)
        {
            this.Workers.text = string.Format("-{0}", (object)this.buildingEntity.composition.componentConfigs.RequireWorkers.workers.ToString());
        }
        else
        {
            this.Workers.text = string.Empty;
            this.PopulationIcon.SetActive(false);
        }
        if (this.buildingEntity.IsUnderConstruction() || this.buildingEntity.State == BuildingState.Upgrading)
        {
            this.UpgradeBtn.gameObject.SetActive(false);
            this.ActiveBtn.gameObject.SetActive(false);
        }
        else
        {
            if (this.buildingEntity.composition.componentConfigs.buildingUpgrade != null)
            {
                this.UpgradeBtn.gameObject.SetActive(true);
                this.UpgradeBtn.interactable = false;
                if (this.buildingEntity.ReadyForUpgrade())
                {
                    this.UpgradeBtn.image.sprite = this.UpgradeActive;
                    this.UpgradeBtn.interactable = true;
                }
            }
            else
                this.UpgradeBtn.gameObject.SetActive(false);
            this.ActiveBtn.gameObject.SetActive(true);
            this.ActiveBtn.interactable = false;
            this.ActiveBtn.isOn = this.buildingEntity.State != BuildingState.Offline;
            if (this.buildingEntity.ReadyForOffline())
                this.ActiveBtn.interactable = true;
            else if (this.buildingEntity.IsHospital() || this.buildingEntity.IsTaxBuilding())
                this.ActiveBtn.gameObject.SetActive(false);
        }
        this.CollectBtn.gameObject.SetActive(false);
        this.TimeLeftTitle.gameObject.SetActive(false);
        this.TimeLeftText.gameObject.SetActive(false);
        this.ProgressBarObject.SetActive(false);
        this.ContentShop.SetActive(false);
        this.Content.SetActive(false);
        if (this.buildingEntity.IsUnderConstruction())
        {
            this.ActivityText.text = "Under Contruction";
            this.TimeLeftTitle.gameObject.SetActive(true);
            this.TimeLeftText.gameObject.SetActive(true);
            this.ProgressBarObject.SetActive(true);
            this.HurryBtn.gameObject.SetActive(true);
            this.ContentShop.SetActive(false);
            this.Content.SetActive(true);
        }
        if (this.buildingEntity.State == BuildingState.Upgrading)
        {
            this.ActivityText.text = "Upgrading";
            this.TimeLeftTitle.gameObject.SetActive(true);
            this.TimeLeftText.gameObject.SetActive(true);
            this.ProgressBarObject.SetActive(true);
            this.HurryBtn.gameObject.SetActive(true);
            this.ContentShop.SetActive(false);
            this.Content.SetActive(true);
        }
        if (this.buildingEntity.State == BuildingState.UpgradeComplete)
        {
            this.ActivityText.text = "Upgrade complete!";
            this.TimeLeftTitle.gameObject.SetActive(false);
            this.TimeLeftText.gameObject.SetActive(false);
            this.ProgressBarObject.SetActive(false);
            this.HurryBtn.gameObject.SetActive(false);
            this.ContentShop.SetActive(false);
            this.Content.SetActive(true);
        }
        if (this.buildingEntity.State == BuildingState.Working)
        {
            this.ActivityText.text = this.buildingEntity.ActivityText();
            this.TimeLeftTitle.gameObject.SetActive(true);
            this.TimeLeftText.gameObject.SetActive(true);
            this.ProgressBarObject.SetActive(true);
            this.HurryBtn.gameObject.SetActive(true);
            this.ContentShop.SetActive(false);
            this.Content.SetActive(true);
        }
        if (this.buildingEntity.State == BuildingState.Full)
        {
            if (this.buildingEntity.IsResourceProducer())
            {
                if (GameData.Player.Storage.ResourceFull(Functions.OutputTypeToEnum(this.buildingEntity.composition.componentConfigs.ResourceProducer.outputType)))
                {
                    this.ActivityText.text = "Storage is full!";
                    this.HurryBtn.gameObject.SetActive(false);
                }
                else if (GameData.Player.WorldMaps["MyLand"].ContainsDepot())
                {
                    this.ActivityText.text = "Waiting for collection";
                    this.HurryBtn.gameObject.SetActive(false);
                }
                else
                {
                    this.ActivityText.text = "Ready to collect";
                    this.HurryBtn.gameObject.SetActive(false);
                    this.CollectBtn.gameObject.SetActive(true);
                }
            }
            if (this.buildingEntity.IsJob())
            {
                this.ActivityText.text = "Ready to collect";
                this.HurryBtn.gameObject.SetActive(false);
                this.CollectBtn.gameObject.SetActive(true);
            }
            this.ContentShop.SetActive(false);
            this.Content.SetActive(true);
        }
        if (this.buildingEntity.State == BuildingState.Inactive)
        {
            if (this.buildingEntity.IsJob())
            {
                this.ContentShop.SetActive(true);
                this.Content.SetActive(false);
                this.BuildMenu();
            }
            else if (this.buildingEntity.IsResourceProducer() && GameData.Player.Storage.ResourceFull(Functions.OutputTypeToEnum(this.buildingEntity.composition.componentConfigs.ResourceProducer.outputType)))
            {
                this.ActivityText.text = "Storage is full!";
                this.HurryBtn.gameObject.SetActive(false);
            }
            else
            {
                this.ContentShop.SetActive(false);
                this.Content.SetActive(true);
            }
        }
        this.isOpen = true;
        this.messageBoxCtrl = MessageBoxCtrl.Instance();
    }

    public void UpgradeButton_OnClick()
    {
        if (this.buildingEntity == null || !(this.buildingEntity.JobName == string.Empty) && !this.buildingEntity.IsResourceProducer() || (!this.IsOpen() || this.OnUpgrade == null))
            return;
        this.OnUpgrade((object)this, new EventArgs());
    }

    public void ToggleBuildingOn()
    {
        if (this.ActiveBtn.isOn)
        {
            if (this.buildingEntity.State == BuildingState.Offline)
            {
                this.buildingEntity.State = BuildingState.Inactive;
                this.Workers.text = this.buildingEntity.WorkersMod();
                if (this.ShowShop())
                    this.ContentShop.gameObject.SetActive(true);
                else
                    this.Content.gameObject.SetActive(true);
                this.UpgradeBtn.image.sprite = this.UpgradeActive;
                this.UpgradeBtn.interactable = true;
            }
        }
        else if (this.buildingEntity.ReadyForOffline())
        {
            this.buildingEntity.State = BuildingState.Offline;
            this.Workers.text = this.buildingEntity.WorkersMod();
            if (this.ShowShop())
                this.ContentShop.gameObject.SetActive(false);
            else
                this.Content.gameObject.SetActive(false);
            this.UpgradeBtn.image.sprite = this.UpgradeInactive;
            this.UpgradeBtn.interactable = false;
        }
        GameData.Player.RecalculatePopulation();
    }

    public BuildingEntity GetBuildingEntity()
    {
        return this.buildingEntity;
    }

    private bool ShowShop()
    {
        bool flag = true;
        if (this.buildingEntity.State == BuildingState.Working || this.buildingEntity.IsTaxBuilding() || this.buildingEntity.IsResourceProducer())
            flag = false;
        return flag;
    }

    public void BuildMenu()
    {
        foreach (string key in this.MenuItems.Keys)
            UnityEngine.Object.Destroy((UnityEngine.Object)this.MenuItems[key]);
        this.MenuItems = new Dictionary<string, GameObject>();
        foreach (string job in this.buildingEntity.GetJobList())
        {
            if (new JobPrereqCheck(job).IsValid())
                this.AddMenuItem(job, 0);
        }
    }

    public bool IsOpen()
    {
        return this.isOpen;
    }

    public void AddMenuItem(string jobName, int injured)
    {
        GameObject original = (GameObject)UnityEngine.Resources.Load("JobMenuItem");
        Job jobinfo = GameData.GetJobinfo(jobName);
        if (jobinfo == null || jobinfo.name == null || jobinfo.name == string.Empty)
            return;
        GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(original, new Vector3(0.0f, 0.0f, 0.0f), Quaternion.identity);
        gameObject.transform.position = new Vector3(8.8f, 0.9f, 0.0f);
        if ((UnityEngine.Object)this.Grid != (UnityEngine.Object)null)
            gameObject.transform.SetParent(this.Grid.transform, false);
        JobMenuItemCtrl component = gameObject.GetComponent<JobMenuItemCtrl>();
        Sprite icon = GameData.GetIcon(jobinfo.icon);
        component.Name.text = GameData.GetText(jobinfo.name);
        component.Icon.sprite = icon;
        component.Button.onClick.AddListener((UnityAction)(() => this.StartJob_OnClick(jobName)));
        int num;
        if (this.buildingEntity.JobCost(jobName).money > 0)
        {
            GameObject cost1 = component.Cost1;
            num = this.buildingEntity.JobCost(jobName).money;
            string _text = num.ToString();
            this.SetItemDetails(cost1, (Sprite)null, _text);
        }
        else
        {
            Tuple<string, int> tollValue = Functions.GetTollValue((object)this.buildingEntity.JobCost(jobName).resources);
            if (tollValue.Item2 > 0)
            {
                this.SetItemDetails(component.Cost1, GameData.GetSprite(Functions.ResourceToSpriteName(tollValue.Item1)), tollValue.Item2.ToString());
            }
            else
            {
                GameObject cost1 = component.Cost1;
                num = this.buildingEntity.JobCost(jobName).money;
                string _text = num.ToString();
                this.SetItemDetails(cost1, (Sprite)null, _text);
            }
        }
        component.Cost2.SetActive(false);
        this.SetItemDetails(component.Time, (Sprite)null, Functions.GetShortTaskTime(jobinfo.buildTime, 0.0f));
        if (this.buildingEntity.JobReward(jobName).money > 0)
        {
            GameObject reward1 = component.Reward1;
            num = this.buildingEntity.JobReward(jobName).money;
            string _text = num.ToString();
            this.SetItemDetails(reward1, (Sprite)null, _text);
        }
        else
        {
            Tuple<string, int> tollValue = Functions.GetTollValue((object)this.buildingEntity.JobReward(jobName).resources);
            if (tollValue.Item2 > 0)
            {
                GameObject reward1 = component.Reward1;
                Sprite sprite = GameData.GetSprite(Functions.ResourceToSpriteName(tollValue.Item1));
                num = tollValue.Item2;
                string _text = num.ToString();
                this.SetItemDetails(reward1, sprite, _text);
            }
        }
        if (jobinfo.rewards.XP > 0)
        {
            GameObject reward2 = component.Reward2;
            num = jobinfo.rewards.XP;
            string _text = num.ToString();
            this.SetItemDetails(reward2, (Sprite)null, _text);
        }
        else
            component.Reward2.SetActive(false);
        this.MenuItems.Add(jobName, gameObject);
    }

    private void SetItemDetails(GameObject _gameObject, Sprite _sprite, string _text)
    {
        if ((UnityEngine.Object)_sprite != (UnityEngine.Object)null)
            _gameObject.GetComponentInChildren<Image>().sprite = _sprite;
        _gameObject.GetComponentInChildren<Text>().text = _text;
    }

    public void StartJob_OnClick(string name)
    {
        Cost cost = new Cost()
        {
            currency = GameData.GetJobinfo(name).cost.currency,
            money = this.buildingEntity.JobCost(name).money,
            resources = GameData.GetJobinfo(name).cost.resources,
            z2points = GameData.GetJobinfo(name).cost.z2points
        };
        Cost _cost = this.buildingEntity.JobCost(name);
        if (!GameData.Player.Affordable(_cost))
        {
            this.messageBoxCtrl.Show("Not enough resource/money to start this job");
        }
        else
        {
            if (this.buildingEntity != null && this.buildingEntity.JobName == string.Empty)
            {
                this.SetSoundClip("UI04-assigning_jobs.caf");
                this.buildingEntity.StartJob(name);
            }
            this.CloseDialog();
        }
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

    public void CloseDialog()
    {
        this.isOpen = false;
        if (this.OnClose == null)
            return;
        this.OnClose((object)this, new EventArgs());
    }

    public void CollectButton_OnClick()
    {
        if (this.OnCollect == null)
            return;
        this.OnCollect((object)this, new CollectEventArgs(this.buildingEntity));
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
}
