
using GameCommon;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace BNR
{
    public class BuildingEntity
    {
        public int Uid;
        public string Name;
        public int Level;
        public Vector3 Position;
        private BuildingState state;
        public BuildingMode Mode;
        public bool Flip;
        [JsonIgnore]
        public GameObject entitySprite;
        [JsonIgnore]
        public GameObject bubble;
        [JsonIgnore]
        public Composition composition;
        public float timer;
        public DateTime eventStartTime;
        public int eventCompleteTime;
        public int buildTime;
        public float buildSpeed;
        private string idleAnimationName;
        private string busyAnimationName;
        private BuildingEntity.AutoCollectMode autoCollect;
        private float autoCollectPause;
        public string JobName;

        public EventHandler TaskComplete { get; set; }

        public EventHandler<BuildingEventArgs> OnStateChange { get; set; }

        public BuildingEntity(string building_type)
        {
            this.Name = building_type;
            this.composition = GameData.Compositions[building_type];
            if (this.IsTaxBuilding())
                this.eventCompleteTime = this.composition.componentConfigs.Taxes.paymentInterval * 60;
            if (this.IsResourceProducer())
                this.eventCompleteTime = 216000;
            if (this.composition.componentConfigs.Construction != null)
                this.buildTime = this.composition.componentConfigs.Construction.buildTime;
            if (this.composition.componentConfigs.Animation != null)
            {
                this.idleAnimationName = this.composition.componentConfigs.Animation.animations.Idle;
                if (this.idleAnimationName == null)
                    this.idleAnimationName = this.composition.componentConfigs.Animation.animations.Default;
                this.busyAnimationName = this.composition.componentConfigs.Animation.animations.Idle;
            }
            this.Level = 1;
            this.JobName = string.Empty;
            this.buildSpeed = 1f;
        }

        public override string ToString()
        {
            return string.Format("{0} {1} {2}", (object)this.Name, (object)this.Level, (object)this.State);
        }

        public int Width()
        {
            return !this.IsExpansion() ? this.composition.componentConfigs.Placeable.width : 12;
        }

        public int Height()
        {
            return !this.IsExpansion() ? this.composition.componentConfigs.Placeable.height : 12;
        }

        public BuildingState State
        {
            get
            {
                return this.state;
            }
            set
            {
                BuildingState state = this.state;
                if (this.state == value)
                    return;
                this.state = value;
                if (state == BuildingState.Offline || this.state == BuildingState.Offline)
                    GameData.Player.RecalculatePopulation();
                if (this.OnStateChange == null || this.state == BuildingState.Initialising)
                    return;
                this.OnStateChange((object)this, new BuildingEventArgs(this.Name, state, this.state));
            }
        }

        public BuildingEntity.AutoCollectMode AutoMode
        {
            get
            {
                return this.autoCollect;
            }
        }

        public bool StateHasBubble()
        {
            return this.State == BuildingState.Inactive && this.IsProducer() || (this.State == BuildingState.Full || this.State == BuildingState.RibbonTime) || (this.State == BuildingState.Upgrading || this.State == BuildingState.UpgradeComplete || this.state == BuildingState.Offline);
        }

        public bool Available()
        {
            return this.State != BuildingState.Broken && this.State != BuildingState.Buying && (this.State != BuildingState.Offline && this.State != BuildingState.Reparing) && (this.State != BuildingState.RibbonTime && this.State != BuildingState.UnderConstruction1 && this.State != BuildingState.UnderConstruction2);
        }

        public static int GetBuildTime(componentConfigs configs)
        {
            if (configs.Expansion != null)
                return GameData.ExpandLandCosts[Functions.ExpansionCount()].buildTime;
            return configs.Construction != null ? configs.Construction.buildTime : 0;
        }

        public void SetExpanionBuildTime()
        {
            if (!this.IsExpansion())
                return;
            this.buildTime = BuildingEntity.GetBuildTime(this.composition.componentConfigs);
        }

        public static int GetBuildCost_Money(componentConfigs configs)
        {
            return configs.Expansion == null ? configs.StructureMenu.cost.money : (configs.StructureMenu.expandLandType != 1 ? GameData.ExpandLandCosts[Functions.ExpansionCount()].currencyCost.money : GameData.ExpandLandCosts[Functions.ExpansionCount()].moneyCost.money);
        }

        public static int GetBuildCost_Currency(componentConfigs configs)
        {
            return configs.Expansion == null ? configs.StructureMenu.cost.currency : (configs.StructureMenu.expandLandType != 1 ? GameData.ExpandLandCosts[Functions.ExpansionCount()].currencyCost.currency : GameData.ExpandLandCosts[Functions.ExpansionCount()].moneyCost.currency);
        }

        public static int GetBuildCost_Z2Points(componentConfigs configs)
        {
            int num = 0;
            return configs.Expansion == null ? configs.StructureMenu.cost.z2points : num;
        }

        public List<string> GetJobList()
        {
            List<string> stringList = new List<string>();
            if (this.composition.componentConfigs.Job != null)
            {
                foreach (string job in this.composition.componentConfigs.JobList.jobs)
                    stringList.Add(job);
            }
            else if (this.composition.componentConfigs.Project != null)
            {
                foreach (string job in this.composition.componentConfigs.ProjectList.jobs)
                    stringList.Add(job);
            }
            return stringList;
        }

        public Vector3 GetBubblePosition()
        {
            Vector3 vector3 = new Vector3();
            AnimationInfo animationInfo = GameData.AnimationInfo[this.idleAnimationName];
            vector3.x = this.Position.x - (float)(animationInfo.width / 2);
            vector3.y = this.Position.y + (float)animationInfo.height;
            vector3.z = 0.0f;
            return vector3;
        }

        public Point GetNewPosition()
        {
            return this.IsExpansion() ? new Point(8, 2) : new Point(105, 35);
        }

        public void BeginUpgrade()
        {
            if (this.IsResourceProducer() || this.IsTaxBuilding())
            {
                if ((this.State == BuildingState.Working || this.State == BuildingState.Full) && (this.Level < 11 && this.composition.componentConfigs.buildingUpgrade != null))
                {
                    this.buildTime = this.composition.componentConfigs.buildingUpgrade.levels[this.Level - 1].upgradeTime;
                    this.State = BuildingState.Upgrading;
                }
            }
            else if (this.State == BuildingState.Inactive && this.Level < 11 && this.composition.componentConfigs.buildingUpgrade != null)
            {
                this.buildTime = this.composition.componentConfigs.buildingUpgrade.levels[this.Level - 1].upgradeTime;
                this.State = BuildingState.Upgrading;
            }
            this.timer = 0.0f;
        }

        public void CompleteUpgrade()
        {
            ++this.Level;
        }

        public void UpdateExternal(float deltaTime)
        {
            if (this.State != BuildingState.Full)
                this.timer += deltaTime * this.buildSpeed;
            switch (this.State)
            {
                case BuildingState.UnderConstruction1:
                    if ((double)this.timer <= (double)(this.buildTime / 2))
                        break;
                    this.State = BuildingState.UnderConstruction2;
                    if (this.TaskComplete == null)
                        break;
                    this.TaskComplete((object)this, new EventArgs());
                    break;
                case BuildingState.UnderConstruction2:
                    if ((double)this.timer <= (double)this.buildTime)
                        break;
                    this.State = BuildingState.RibbonTime;
                    this.buildSpeed = 1f;
                    break;
                case BuildingState.Working:
                    if (this.IsTaxBuilding() || this.IsJob() || this.IsProject())
                    {
                        if ((double)this.timer <= (double)this.eventCompleteTime)
                            break;
                        this.State = BuildingState.Full;
                        break;
                    }
                    if ((double)this.timer <= (double)(this.eventCompleteTime / 60))
                        break;
                    this.State = BuildingState.Full;
                    break;
                case BuildingState.Upgrading:
                    if ((double)this.timer <= (double)this.buildTime)
                        break;
                    this.State = BuildingState.UpgradeComplete;
                    this.buildSpeed = 1f;
                    break;
            }
        }

        public void Update(float deltaTime)
        {
            if (this.State != BuildingState.Full)
                this.timer += deltaTime * this.buildSpeed;
            switch (this.State)
            {
                case BuildingState.Inactive:
                    this.buildSpeed = 1f;
                    if (this.IsResourceProducer() && !GameData.Player.Storage.ResourceFull(Functions.OutputTypeToEnum(this.composition.componentConfigs.ResourceProducer.outputType)))
                    {
                        this.autoCollect = BuildingEntity.AutoCollectMode.Off;
                        this.autoCollectPause = 0.0f;
                        this.State = BuildingState.Working;
                        this.eventStartTime = DateTime.Now;
                        if ((double)this.timer > (double)(this.eventCompleteTime / 60))
                            this.timer = 0.0f;
                    }
                    if (this.IsTaxBuilding())
                    {
                        this.State = BuildingState.Working;
                        this.timer = 0.0f;
                    }
                    if (!this.IsDepot() || !GameData.Player.Storage.ResourceFull())
                        break;
                    this.State = BuildingState.Full;
                    break;
                case BuildingState.Placed:
                    this.timer = 0.0f;
                    this.State = BuildingState.UnderConstruction1;
                    break;
                case BuildingState.UnderConstruction1:
                    if ((double)this.timer <= (double)(this.buildTime / 2))
                        break;
                    this.State = BuildingState.UnderConstruction2;
                    if (this.TaskComplete == null)
                        break;
                    this.TaskComplete((object)this, new EventArgs());
                    break;
                case BuildingState.UnderConstruction2:
                    if ((double)this.timer <= (double)this.buildTime)
                        break;
                    this.State = BuildingState.RibbonTime;
                    this.buildSpeed = 1f;
                    break;
                case BuildingState.Working:
                    if (this.IsTaxBuilding() || this.IsJob() || this.IsProject())
                    {
                        if ((double)this.timer <= (double)this.eventCompleteTime)
                            break;
                        this.State = BuildingState.Full;
                        break;
                    }
                    if ((double)this.timer <= (double)(this.eventCompleteTime / 60))
                        break;
                    this.State = BuildingState.Full;
                    break;
                case BuildingState.Full:
                    if (this.IsResourceProducer())
                    {
                        switch (this.autoCollect)
                        {
                            case BuildingEntity.AutoCollectMode.Off:
                                if (GameData.Player.WorldMaps["MyLand"].ContainsDepot())
                                {
                                    this.autoCollectPause = 3f;
                                    this.autoCollect = BuildingEntity.AutoCollectMode.Waiting;
                                    break;
                                }
                                break;
                            case BuildingEntity.AutoCollectMode.Waiting:
                                this.autoCollectPause -= deltaTime;
                                if ((double)this.autoCollectPause <= 0.0)
                                {
                                    this.autoCollect = BuildingEntity.AutoCollectMode.Done;
                                    if (this.OnStateChange != null)
                                    {
                                        this.OnStateChange((object)this, new BuildingEventArgs(this.Name, this.State, this.State));
                                        break;
                                    }
                                    break;
                                }
                                break;
                            case BuildingEntity.AutoCollectMode.Done:
                                this.autoCollect = BuildingEntity.AutoCollectMode.Off;
                                this.State = BuildingState.Inactive;
                                break;
                            case BuildingEntity.AutoCollectMode.Full:
                                if (!GameData.Player.Storage.ResourceFull(Functions.OutputTypeToEnum(this.composition.componentConfigs.ResourceProducer.outputType)))
                                {
                                    this.autoCollect = BuildingEntity.AutoCollectMode.Off;
                                    break;
                                }
                                break;
                        }
                    }
                    if (!this.IsDepot() || GameData.Player.Storage.ResourceFull())
                        break;
                    this.State = BuildingState.Inactive;
                    break;
                case BuildingState.Upgrading:
                    if ((double)this.timer <= (double)this.buildTime)
                        break;
                    this.State = BuildingState.UpgradeComplete;
                    this.buildSpeed = 1f;
                    break;
            }
        }

        public void StartJob(string _jobName)
        {
            if (this.IsHospital())
            {
                this.timer = 0.0f;
                this.eventCompleteTime = GameData.BattleUnits[_jobName].healTime * 60;
                this.JobName = _jobName;
                GameData.Player.Storage.DebitStorage(GameData.BattleUnits[_jobName].healCost);
                GameData.Player.AddToHealQueue(_jobName);
                this.State = BuildingState.Working;
            }
            if (this.Name == "comp_milUnit_barracks" || this.IsProject())
            {
                this.timer = 0.0f;
                this.eventCompleteTime = GameData.BattleUnits[_jobName].buildTime;
                GameData.Player.Storage.DebitStorage(GameData.BattleUnits[_jobName].cost);
                this.JobName = _jobName;
                this.State = BuildingState.Working;
            }
            if (!this.IsJob())
                return;
            this.timer = 0.0f;
            this.eventCompleteTime = GameData.GetJobinfo(_jobName).buildTime;
            GameData.Player.Storage.DebitStorage(this.JobCost(_jobName));
            this.JobName = _jobName;
            this.State = BuildingState.Working;
        }

        public Cost JobCost(string jobName)
        {
            Cost cost = new Cost();
            cost.money = GameData.GetJobinfo(jobName).cost.money * this.composition.componentConfigs.buildingUpgrade.levels[this.Level - 1].input / 100;
            cost.resources = new ResourceList();
            foreach (string resourceName in Functions.ResourceNames())
            {
                int propertyValue = (int)Functions.GetPropertyValue((object)GameData.GetJobinfo(jobName).cost.resources, resourceName);
                if (propertyValue > 0)
                {
                    string name = resourceName;
                    int num = propertyValue * this.composition.componentConfigs.buildingUpgrade.levels[this.Level - 1].input / 100;
                    PropertyInfo property = cost.resources.GetType().GetProperty(name);
                    if (property != (PropertyInfo)null)
                        property.SetValue((object)cost.resources, (object)num);
                }
            }
            return cost;
        }

        public Cost JobReward(string jobName)
        {
            Cost cost = new Cost();
            cost.money = GameData.GetJobinfo(jobName).rewards.amount.money * this.composition.componentConfigs.buildingUpgrade.levels[this.Level - 1].output / 100;
            cost.resources = new ResourceList();
            if (GameData.GetJobinfo(jobName).rewards.amount.resources != null)
            {
                foreach (string resourceName in Functions.ResourceNames())
                {
                    int num1 = 0;
                    object propertyValue = Functions.GetPropertyValue((object)GameData.GetJobinfo(jobName).rewards.amount.resources, resourceName);
                    if (propertyValue != null)
                        num1 = (int)propertyValue;
                    if (num1 > 0)
                    {
                        string name = resourceName;
                        int num2 = num1 * this.composition.componentConfigs.buildingUpgrade.levels[this.Level - 1].output / 100;
                        PropertyInfo property = cost.resources.GetType().GetProperty(name);
                        if (property != (PropertyInfo)null)
                            property.SetValue((object)cost.resources, (object)num2);
                    }
                }
            }
            return cost;
        }

        public bool ReadyForUpgrade()
        {
            bool flag = true;
            if (this.IsTaxBuilding() || this.IsResourceProducer())
            {
                if (this.State == BuildingState.Offline)
                    flag = false;
            }
            else if (this.state != BuildingState.Inactive)
                flag = false;
            if (this.composition.componentConfigs.buildingUpgrade == null)
                flag = false;
            if (flag && this.Level + 1 > this.composition.componentConfigs.buildingUpgrade.levels.Length)
                flag = false;
            return flag;
        }

        public bool ReadyForOffline()
        {
            bool flag = false;
            if (this.state == BuildingState.Inactive)
                flag = true;
            if (this.IsResourceProducer() && this.state == BuildingState.Working)
                flag = true;
            if (this.state == BuildingState.Offline && GameData.Player.PopulationValid_Demand(this.composition.componentConfigs.RequireWorkers.workers - this.composition.componentConfigs.RequireWorkers.minWorkers))
                flag = true;
            return flag;
        }

        public float GetTaskPercent()
        {
            float num1 = this.timer / (float)this.buildTime;
            float num2;
            if (this.IsTaxBuilding() || this.IsJob() || this.IsProject())
            {
                switch (this.state)
                {
                    case BuildingState.UnderConstruction1:
                    case BuildingState.UnderConstruction2:
                    case BuildingState.Upgrading:
                        num2 = this.timer / (float)this.buildTime;
                        break;
                    case BuildingState.Working:
                        num2 = this.timer / (float)this.eventCompleteTime;
                        break;
                    default:
                        num2 = 1f;
                        break;
                }
            }
            else
            {
                switch (this.state)
                {
                    case BuildingState.UnderConstruction1:
                    case BuildingState.UnderConstruction2:
                    case BuildingState.Upgrading:
                        num2 = this.timer / (float)this.buildTime;
                        break;
                    case BuildingState.Working:
                        num2 = this.timer / (float)(this.eventCompleteTime / 60);
                        break;
                    default:
                        num2 = 1f;
                        break;
                }
            }
            if ((double)num2 > 1.0)
                num2 = 1f;
            return num2;
        }

        public string GetTaskTime()
        {
            if (this.IsTaxBuilding() || this.IsJob() || this.IsProject())
            {
                switch (this.state)
                {
                    case BuildingState.UnderConstruction1:
                    case BuildingState.UnderConstruction2:
                    case BuildingState.Upgrading:
                        return Functions.GetTaskTime(this.buildTime, this.timer);
                    case BuildingState.Working:
                        return Functions.GetTaskTime(this.eventCompleteTime, this.timer);
                    default:
                        return Functions.GetTaskTime(1, 1f);
                }
            }
            else
            {
                switch (this.state)
                {
                    case BuildingState.UnderConstruction1:
                    case BuildingState.UnderConstruction2:
                    case BuildingState.Upgrading:
                        return Functions.GetTaskTime(this.buildTime, this.timer);
                    case BuildingState.Working:
                        return Functions.GetTaskTime(this.eventCompleteTime / 60, this.timer);
                    default:
                        return Functions.GetTaskTime(1, 1f);
                }
            }
        }

        public int GetHurryCost()
        {
            int num = 1;
            switch (this.State)
            {
                case BuildingState.UnderConstruction1:
                case BuildingState.UnderConstruction2:
                case BuildingState.Upgrading:
                    num = Functions.GetHours((float)this.buildTime, this.timer);
                    break;
                case BuildingState.Working:
                    if (this.IsTaxBuilding() || this.IsJob() || this.IsProject())
                    {
                        if (this.state == BuildingState.Working)
                        {
                            num = Functions.GetHours((float)this.eventCompleteTime, this.timer);
                            break;
                        }
                        break;
                    }
                    if (this.state == BuildingState.Working)
                    {
                        num = Functions.GetHours((float)(this.eventCompleteTime / 60), this.timer);
                        break;
                    }
                    break;
            }
            return num;
        }

        public bool IsResourceStorage()
        {
            bool flag = false;
            if (Functions.IsStorageResource(this.composition.componentConfigs.StructureMenu.roleIconName))
                flag = true;
            return flag;
        }

        public bool IsExpansion()
        {
            return this.composition.componentConfigs.Expansion != null;
        }

        public bool IsMoveable()
        {
            return this.composition.componentConfigs.Placeable != null && this.composition.componentConfigs.Placeable.isMoveable;
        }

        public bool IsProducer()
        {
            return this.IsHospital() || this.IsProject() || this.IsJob();
        }

        public bool IsResourceProducer()
        {
            return this.composition != null && !(this.Name == "comp_mil_combatArena") && this.composition.componentConfigs.ResourceProducer != null;
        }

        public bool IsHospital()
        {
            return this.composition != null && this.composition.componentConfigs.Hospital != null;
        }

        public bool IsDepot()
        {
            return this.composition != null && this.composition.componentConfigs.Depot != null;
        }

        public bool IsSupplyDepot()
        {
            return this.composition != null && this.Name == "comp_res_supplydepot";
        }

        public bool IsProject()
        {
            return this.composition != null && this.composition.componentConfigs.Project != null;
        }

        public bool IsJob()
        {
            return this.composition != null && this.composition.componentConfigs.Job != null;
        }

        public bool IsTaxBuilding()
        {
            return this.composition != null && this.composition.componentConfigs.Taxes != null;
        }

        public bool IsStorageBuilding()
        {
            return this.composition != null && this.composition.componentConfigs.ResourceCapacity != null && this.composition.componentConfigs.Taxes == null;
        }

        public bool IsConstructable()
        {
            return this.composition.componentConfigs.Construction != null;
        }

        public bool IsUnderConstruction()
        {
            return this.State == BuildingState.UnderConstruction1 || this.State == BuildingState.UnderConstruction2 || this.State == BuildingState.RibbonTime;
        }

        public bool IsUpgrading()
        {
            return this.State == BuildingState.Upgrading || this.State == BuildingState.UpgradeComplete;
        }

        public bool IsFarmJob()
        {
            bool flag = false;
            if (this.IsJob() && (this.Name == "comp_civJob_farm" || this.Name == "comp_civJob_field" || (this.Name == "comp_civJob_greenhouse" || this.Name == "comp_civJob_plantation")))
                flag = true;
            return flag;
        }

        public int OutputQty()
        {
            int num1 = 0;
            if (this.IsResourceStorage())
            {
                if (this.IsResourceProducer())
                {
                    double outputRate = (double)this.composition.componentConfigs.ResourceProducer.outputRate;
                    double output = (double)this.composition.componentConfigs.buildingUpgrade.levels[this.Level - 1].output;
                    Resource type = Functions.ResourceNameToEnum(this.composition.componentConfigs.ResourceProducer.outputType);
                    double num2 = output;
                    double num3 = outputRate * num2 / 100.0;
                    num1 = Convert.ToInt32(num3 + 0.5);
                    if (GameData.Player.WorldMaps["MyLand"].ContainsDepot())
                    {
                        TimeSpan timeSpan = DateTime.Now - this.eventStartTime;
                        int num4 = 1;
                        if (timeSpan.TotalHours > 1.0)
                            num4 = Convert.ToInt32(timeSpan.TotalHours);
                        int amount = num1 * num4;
                        if (!GameData.Player.Storage.CheckResourceCapacity(type, amount))
                        {
                            int resourceCapacity = GameData.Player.Storage.GetResourceCapacity(type);
                            int num5 = Convert.ToInt32((double)resourceCapacity / num3) * num1;
                            if (num5 < resourceCapacity)
                                num5 += Convert.ToInt32(num3);
                            amount = num5;
                        }
                        num1 = amount;
                    }
                }
                if (this.IsJob())
                {
                    int propertyValue = (int)Functions.GetPropertyValue((object)GameData.JobInfo.jobs[this.JobName].rewards.amount.resources, Functions.ResourceEnumToName(Functions.OutputTypeToEnum(this.composition.componentConfigs.StructureMenu.roleIconName)));
                    if (propertyValue > 0)
                        num1 = propertyValue;
                }
            }
            return num1;
        }

        public bool CanHurry()
        {
            return (this.composition.componentConfigs.Taxes == null || this.IsUnderConstruction()) && (this.State != BuildingState.Full && this.State != BuildingState.Inactive);
        }

        public void Hurry()
        {
            switch (this.State)
            {
                case BuildingState.UnderConstruction1:
                case BuildingState.UnderConstruction2:
                case BuildingState.Upgrading:
                    if ((double)this.buildSpeed != 1.0)
                        break;
                    this.buildSpeed = (float)(((double)this.buildTime - (double)this.timer) / 3.0);
                    GameData.Player.Storage.DebitStorage(new Cost()
                    {
                        currency = this.GetHurryCost()
                    });
                    break;
                case BuildingState.Working:
                    if (this.IsTaxBuilding() || this.IsJob() || this.IsProject())
                    {
                        if (this.state != BuildingState.Working)
                            break;
                        this.buildSpeed = (float)(((double)this.eventCompleteTime - (double)this.timer) / 3.0);
                        GameData.Player.Storage.DebitStorage(new Cost()
                        {
                            currency = this.GetHurryCost()
                        });
                        break;
                    }
                    if (this.state != BuildingState.Working)
                        break;
                    this.buildSpeed = (float)(((double)(this.eventCompleteTime / 60) - (double)this.timer) / 3.0);
                    GameData.Player.Storage.DebitStorage(new Cost()
                    {
                        currency = this.GetHurryCost()
                    });
                    break;
            }
        }

        public void CollectOutput()
        {
            if (this.IsTaxBuilding())
            {
                GameData.Player.Storage.AddMoney(this.TaxAmount());
                if (this.Name == "comp_res_supplydepot")
                {
                    GameData.Player.Storage.AddResource(Resource.stone, this.composition.componentConfigs.Taxes.rewards.amount.resources.stone);
                    GameData.Player.Storage.AddResource(Resource.wood, this.composition.componentConfigs.Taxes.rewards.amount.resources.wood);
                    GameData.Player.Storage.AddResource(Resource.iron, this.composition.componentConfigs.Taxes.rewards.amount.resources.iron);
                    GameData.Player.Storage.AddNanopods(10);
                }
                GameData.Player.IncreaseXP(this.TaxXPAmount());
            }
            if (this.IsResourceProducer())
            {
                GameData.Player.Storage.AddResource(Functions.ResourceNameToEnum(this.composition.componentConfigs.ResourceProducer.nodeType), this.OutputQty());
                TimeSpan timeSpan1 = DateTime.Now - this.eventStartTime;
                TimeSpan timeSpan2 = new TimeSpan(0, 60 - timeSpan1.Minutes, 0);
                this.timer = (float)((60 - timeSpan1.Minutes) * 60);
            }
            if (this.IsHospital())
            {
                GameData.Player.HealUnit(this.JobName);
                this.State = BuildingState.Inactive;
                this.JobName = string.Empty;
            }
            this.IsProject();
            if (this.IsJob())
            {
                Job jobinfo = GameData.GetJobinfo(this.JobName);
                GameData.Player.Storage.AddMoney(this.JobReward(this.JobName).money);
                GameData.Player.IncreaseXP(jobinfo.rewards.XP);
                if (jobinfo.rewards.amount.resources != null)
                    GameData.Player.Storage.CreditStorage(this.JobReward(this.JobName));
                this.State = BuildingState.Inactive;
            }
            if (!this.IsDepot())
                return;
            this.State = BuildingState.Inactive;
        }

        public int TaxAmount()
        {
            float num = 1f;
            float money = (float)this.composition.componentConfigs.Taxes.rewards.amount.money;
            if (this.Name != "comp_res_supplydepot")
                num = (float)this.composition.componentConfigs.buildingUpgrade.levels[this.Level].output / 100f;
            return (int)((double)num * (double)money);
        }

        public int TaxXPAmount()
        {
            float num = 1f;
            float xp = (float)this.composition.componentConfigs.Taxes.rewards.XP;
            if (this.Name != "comp_res_supplydepot")
                num = (float)this.composition.componentConfigs.buildingUpgrade.levels[this.Level].XPoutput / 100f;
            return (int)((double)num * (double)xp);
        }

        public int ResourceOutput(int _level)
        {
            int num = 0;
            if (this.IsResourceProducer())
                num = (int)((double)this.composition.componentConfigs.ResourceProducer.outputRate * (double)this.composition.componentConfigs.buildingUpgrade.levels[_level - 1].output / 100.0 + 0.5);
            return num;
        }

        public int ResourceOutputIncrease()
        {
            int num = 0;
            if (this.IsResourceProducer())
                num = (int)((double)this.composition.componentConfigs.ResourceProducer.outputRate * ((double)this.composition.componentConfigs.buildingUpgrade.levels[this.Level].output - (double)this.composition.componentConfigs.buildingUpgrade.levels[this.Level - 1].output) / 100.0 + 0.5);
            return num;
        }

        public string ActivityText()
        {
            if (this.IsJob())
                return string.Format("{0}", (object)GameData.GetText(GameData.GetJobinfo(this.JobName).name));
            return this.IsTaxBuilding() || this.IsResourceProducer() ? "Ready to collect in :" : "Unknown";
        }

        public string WorkersMod()
        {
            string str = "0";
            if (this.composition.componentConfigs.RequireWorkers != null)
                str = this.State == BuildingState.Offline ? string.Format("-{0}", (object)this.composition.componentConfigs.RequireWorkers.minWorkers.ToString()) : string.Format("-{0}", (object)this.composition.componentConfigs.RequireWorkers.workers.ToString());
            if (this.composition.componentConfigs.PopulationCapacity != null)
                str = string.Format("+{0}", (object)this.composition.componentConfigs.PopulationCapacity.contribution.ToString());
            return str;
        }

        public enum AutoCollectMode
        {
            Off,
            Waiting,
            Done,
            Full,
        }
    }
}
