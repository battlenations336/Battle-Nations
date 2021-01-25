
using GameCommon;
using GameCommon.SerializedObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BNR
{
    public class Player
    {
        private Dictionary<string, ArmyUnit> army = new Dictionary<string, ArmyUnit>();
        public string Name;
        public Vector3 cameraPos;
        private int level;
        private int xp;
        private int maxXP;
        private Storage storage;
        private int population;
        private int populationActive;
        private int populationLimit;
        public Dictionary<string, MapData> WorldMaps;
        public Dictionary<string, BNR.AchievementProgress> AchievementProgress;
        private Dictionary<string, CompletedMission> completedMissions;
        private Dictionary<string, CurrentMission> currentMissions;
        private List<string> defeatedOccupations;
        private List<string> occupations;

        public EventHandler OnChangeXP { get; set; }

        public EventHandler OnChangeLevel { get; set; }

        public EventHandler<ButtonEventArgs> OnAchievementUpdated { get; set; }

        public EventHandler OnChangePopulation { get; set; }

        public EventHandler OnMissionsChanged { get; set; }

        public EventHandler<MissionEventArgs> OnMissionUpdated { get; set; }

        public EventHandler<EncounterEventArgs> OnEncounterUpdated { get; set; }

        public EventHandler OnPlayerDataUpdated { get; set; }

        public bool Intro { get; set; }

        public Player()
        {
            this.storage = new Storage();
            this.WorldMaps = new Dictionary<string, MapData>();
            this.completedMissions = new Dictionary<string, CompletedMission>();
            this.currentMissions = new Dictionary<string, CurrentMission>();
            this.defeatedOccupations = new List<string>();
            this.occupations = new List<string>();
        }

        public int Population
        {
            get
            {
                return this.population;
            }
        }

        public int PopulationActive
        {
            get
            {
                return this.populationActive;
            }
        }

        public int PopulationLimit
        {
            get
            {
                return this.populationLimit;
            }
        }

        public List<string> DefeatedOccupations
        {
            get
            {
                return this.defeatedOccupations;
            }
        }

        public void UpdateStorageLimit()
        {
            foreach (Resource storageResource in this.storage.StorageResources)
                this.storage.SetMaxResource(storageResource, 0);
            foreach (BuildingEntity building in this.WorldMaps["MyLand"].Buildings)
            {
                if (!building.IsUnderConstruction() && building.composition.componentConfigs.ResourceCapacity != null)
                {
                    foreach (Resource storageResource in this.storage.StorageResources)
                    {
                        int propertyValue = (int)Functions.GetPropertyValue((object)building.composition.componentConfigs.ResourceCapacity.contribution.resources, Functions.ResourceEnumToName(storageResource));
                        if (propertyValue > 0)
                            this.storage.AddMaxResource(storageResource, propertyValue);
                    }
                }
            }
        }

        public List<string> Occupations
        {
            get
            {
                return this.occupations;
            }
        }

        public Dictionary<string, CompletedMission> CompletedMissions
        {
            get
            {
                return this.completedMissions;
            }
        }

        public Dictionary<string, CurrentMission> CurrentMissions
        {
            get
            {
                return this.currentMissions;
            }
        }

        public bool WorldInitialised(string name)
        {
            bool flag = false;
            if (this.WorldMaps.ContainsKey(name) && this.WorldMaps[name].Buildings.Count<BuildingEntity>() > 0)
                flag = true;
            return flag;
        }

        public void CheckSpawnWorld(string WorldId)
        {
            if (GameData.Player.WorldMaps.ContainsKey(WorldId))
                return;
            this.WorldMaps.Add(WorldId, new MapData());
            this.PlayerDataUpdated();
        }

        public void InitMission(string missionId)
        {
            if (this.completedMissions.ContainsKey(missionId))
                Debug.Log((object)string.Format("Tried to repeat mission {0}", (object)missionId));
            else if (this.CurrentMissions.ContainsKey(missionId))
            {
                Debug.Log((object)string.Format("Tried to restart mission {0}", (object)missionId));
            }
            else
            {
                CurrentMission currentMission = new CurrentMission();
                currentMission.MissionId = missionId;
                currentMission.State = GameData.Missions[missionId].startScript == null ? MissionState.Initialising : MissionState.StartDialog;
                currentMission.Steps = MissionScriptEngine.GetMissionSteps(missionId);
                GameData.Player.CurrentMissions.Add(missionId, currentMission);
                Debug.Log((object)string.Format("Added mission {0}", (object)missionId));
                if (this.OnMissionsChanged != null)
                    this.OnMissionsChanged((object)this, new EventArgs());
                if (this.OnMissionUpdated == null)
                    return;
                this.OnMissionUpdated((object)this, new MissionEventArgs(missionId, currentMission.State));
            }
        }

        public void StartMissionDialog(string missionId)
        {
            if (GameData.Missions[missionId].startScript != null && GameData.Player.CurrentMissions[missionId].State == MissionState.StartDialog)
            {
                Debug.Log((object)string.Format("Start dialog mission {0}, {1}", (object)missionId, (object)this.currentMissions[missionId].State));
                DialogConfig.AddScript(missionId, GameData.Missions[missionId].startScript.scriptId);
                GameData.Player.CurrentMissions[missionId].State = MissionState.WaitForStartDialog;
            }
            else
                this.StartMission(missionId);
        }

        public void StartMission(string missionId)
        {
            GameData.Player.currentMissions[missionId].State = MissionState.Started;
            if (this.OnMissionUpdated == null)
                return;
            this.OnMissionUpdated((object)this, new MissionEventArgs(missionId, GameData.Player.CurrentMissions[missionId].State));
        }

        public void AdvanceMissionPostDialog(string _missionId)
        {
            if (!this.currentMissions.ContainsKey(_missionId))
                return;
            Debug.Log((object)string.Format("Advancing mission {0}, {1}", (object)_missionId, (object)this.currentMissions[_missionId].State));
            MissionState state = this.currentMissions[_missionId].State;
            if (this.currentMissions[_missionId].State == MissionState.WaitForStartDialog)
                this.currentMissions[_missionId].State = MissionState.Initialising;
            if (this.currentMissions[_missionId].State == MissionState.Ready)
                this.currentMissions[_missionId].State = MissionState.Started;
            if (this.currentMissions[_missionId].State == MissionState.ObjectivesDone && !DialogConfig.IsScriptWaiting(_missionId))
                this.currentMissions[_missionId].State = MissionState.Finished;
            if (this.currentMissions[_missionId].State == state || this.OnMissionUpdated == null)
                return;
            this.OnMissionUpdated((object)this, new MissionEventArgs(_missionId, this.currentMissions[_missionId].State));
        }

        public void UpdateMissionProgress(string _missionId)
        {
            if (this.OnMissionUpdated == null)
                return;
            this.OnMissionUpdated((object)this, new MissionEventArgs(_missionId, this.currentMissions[_missionId].State));
        }

        public void FinishMission(string _missionId)
        {
            if (!this.currentMissions.ContainsKey(_missionId))
                return;
            this.currentMissions[_missionId].State = MissionState.ObjectivesDone;
            if (GameData.Missions[_missionId].finishScript != null)
            {
                Debug.Log((object)string.Format("End dialog mission {0}, {1}", (object)_missionId, (object)this.currentMissions[_missionId].State));
                if (GameData.Missions[_missionId].finishScript == null)
                    return;
                DialogConfig.AddScript(_missionId, GameData.Missions[_missionId].finishScript.scriptId);
            }
            else
                this.currentMissions[_missionId].State = MissionState.Finished;
        }

        public void CompleteMission(string _missionId)
        {
            if (this.currentMissions.ContainsKey(_missionId))
                this.currentMissions.Remove(_missionId);
            if (this.completedMissions.ContainsKey(_missionId))
                return;
            this.completedMissions.Add(_missionId, new CompletedMission()
            {
                MissionId = _missionId
            });
            foreach (MapData mapData in this.WorldMaps.Values)
                ;
            Missions mission = GameData.Missions[_missionId];
            if (mission != null && mission.rewards != null)
            {
                if (mission.rewards.units != null)
                {
                    foreach (string key in mission.rewards.units.Keys)
                    {
                        int unit = mission.rewards.units[key];
                        if (unit > 0)
                            this.AddUnit(key, unit);
                    }
                }
                if (mission.rewards.XP > 0)
                    this.IncreaseXP(mission.rewards.XP);
                if (mission.rewards.amount != null)
                {
                    if (mission.rewards.amount.money > 0)
                        this.storage.AddMoney(mission.rewards.amount.money);
                    if (mission.rewards.amount.currency > 0)
                        this.storage.AddNanopods(mission.rewards.amount.currency);
                }
                if (mission.rewards.amount.resources != null)
                {
                    foreach (string resourceName in Functions.ResourceNames())
                    {
                        int propertyValue = (int)Functions.GetPropertyValue((object)mission.rewards.amount.resources, resourceName);
                        if (propertyValue > 0)
                            this.storage.AddResource(Functions.ResourceNameToEnum(resourceName), propertyValue);
                    }
                }
            }
            if (this.OnMissionsChanged != null)
                this.OnMissionsChanged((object)this, new EventArgs());
            if (this.OnMissionUpdated != null)
                this.OnMissionUpdated((object)this, new MissionEventArgs(_missionId, MissionState.Completed));
            Debug.Log((object)string.Format("Completed mission {0}", (object)_missionId));
        }

        public void CompleteEncounter(string WorldId, EncounterArmy _encounter, int rewardMoney)
        {
            this.CompleteEncounter(_encounter.Name, rewardMoney);
            if (this.OnEncounterUpdated == null)
                return;
            this.OnEncounterUpdated((object)this, new EncounterEventArgs(WorldId, _encounter, TransMode.Delete, false));
        }

        public void CompleteEncounter(string _encounterId, int rewardMoney)
        {
            if (string.IsNullOrEmpty(_encounterId) || !GameData.BattleEncounters.armies.ContainsKey(_encounterId))
                return;
            BattleEncounterArmy army = GameData.BattleEncounters.armies[_encounterId];
            if (army != null && army.rewards != null)
            {
                if (army.rewards.units != null)
                {
                    foreach (string key in army.rewards.units.Keys)
                    {
                        int unit = army.rewards.units[key];
                        if (unit > 0)
                            this.AddUnit(key, unit);
                    }
                }
                if (army.rewards.amount != null && army.rewards.amount.money > 0)
                    this.storage.AddMoney(army.rewards.amount.money);
                if (rewardMoney > 0)
                    this.storage.AddMoney(rewardMoney);
            }
            Debug.Log((object)string.Format("Completed encounter {0}", (object)_encounterId));
        }

        public void CreateEncounter(
          string worldId,
          string encounterId,
          EncounterType type,
          string eventId,
          bool forceStart)
        {
            this.CheckSpawnWorld(worldId);
            EncounterArmy Encounter = new EncounterArmy();
            Encounter.InstanceId = this.WorldMaps[worldId].FirstFreeEncounterInstance();
            Encounter.Name = encounterId;
            Encounter.Type = type;
            Encounter.EventId = eventId;
            Encounter.Status = EncounterStatus.Ready;
            Encounter.Created = DateTime.Now;
            this.WorldMaps[worldId].Encounters.Add(Encounter);
            if (this.OnEncounterUpdated == null)
                return;
            this.OnEncounterUpdated((object)this, new EncounterEventArgs(worldId, Encounter, TransMode.Create, forceStart));
        }

        public void CheckForEncounterSpawn(string npcId)
        {
            int num1 = 0;
            string encounterId = string.Empty;
            if (string.IsNullOrEmpty(npcId))
                return;
            if (this.WorldMaps[npcId].NextSpawnCheck > DateTime.Now)
            {
                TimeSpan timeSpan = this.WorldMaps[npcId].NextSpawnCheck.Subtract(DateTime.Now);
                Debug.Log((object)string.Format("Too early for spawn, {0} - {1}", (object)npcId, (object)timeSpan.ToString()));
            }
            else
            {
                KeyValuePair<string, BattleEncounterTable> battleEncounterTable = GameData.BattleEncounters.tables.Where<KeyValuePair<string, BattleEncounterTable>>((Func<KeyValuePair<string, BattleEncounterTable>, bool>)(x => x.Value.npcId == npcId && x.Value.levels.min <= this.level && x.Value.levels.max >= this.level)).FirstOrDefault<KeyValuePair<string, BattleEncounterTable>>();
                if (string.IsNullOrEmpty(battleEncounterTable.Key))
                    return;
                IEnumerable<EncounterArmy> source = this.WorldMaps[npcId].Encounters.Where<EncounterArmy>((Func<EncounterArmy, bool>)(x => x.Type == EncounterType.Random && x.EventId == battleEncounterTable.Key));
                if (source == null || source.Count<EncounterArmy>() < battleEncounterTable.Value.maxEncounters)
                {
                    int num2 = UnityEngine.Random.Range(1, 100);
                    foreach (EncounterDef encounter in battleEncounterTable.Value.encounters)
                    {
                        if (num2 >= num1 && num2 <= num1 + encounter.spawnPercent)
                            encounterId = encounter.encounterId;
                        else
                            num1 += encounter.spawnPercent;
                    }
                    if (!string.IsNullOrEmpty(encounterId))
                    {
                        this.CreateEncounter(npcId, encounterId, EncounterType.Random, battleEncounterTable.Key, false);
                        DateTime dateTime = DateTime.Now.AddMinutes((double)UnityEngine.Random.Range((float)battleEncounterTable.Value.spawnIntervalMinutes.min, (float)battleEncounterTable.Value.spawnIntervalMinutes.max));
                        this.WorldMaps[npcId].NextSpawnCheck = dateTime;
                        Debug.Log((object)string.Format("Random spawn {0} - {1}", (object)npcId, (object)encounterId));
                    }
                    else
                        Debug.Log((object)string.Format("No spawn found for {0}", (object)npcId));
                }
                else
                {
                    if (source.Count<EncounterArmy>() < battleEncounterTable.Value.maxEncounters)
                        return;
                    Debug.Log((object)string.Format("Found {0} encounters in {1}", (object)source.Count<EncounterArmy>(), (object)npcId));
                }
            }
        }

        public void RemoveEncounter(string WorldId, EncounterArmy encounter)
        {
            if (this.OnEncounterUpdated == null)
                return;
            this.OnEncounterUpdated((object)this, new EncounterEventArgs(WorldId, encounter, TransMode.Delete, false));
        }

        public void DefeatOccupation(string _occId)
        {
            if (!this.defeatedOccupations.Contains(_occId))
                this.defeatedOccupations.Add(_occId);
            if (!this.occupations.Contains(_occId))
                return;
            this.occupations.Remove(_occId);
        }

        private void createTestArmy()
        {
            this.AddUnit("s_trooper");
            this.AddUnit("s_trooper");
            this.AddUnit("s_trooper");
            this.AddUnit("s_shock");
            this.AddUnit("s_shock");
            this.AddUnit("s_shotgunner");
            this.AddUnit("s_officer");
            this.AddUnit("s_raider_infantry");
            this.AddUnit("s_mortar");
            this.AddUnit("s_grenadier");
            this.AddUnit("s_gunner");
            this.AddUnit("s_arsonist");
            this.AddUnit("s_ninja");
            this.AddUnit("s_arctic_trooper");
            this.AddUnit("s_trooper_underdog");
        }

        public void LoadFromProfile(Profile profile)
        {
            this.xp = profile.XP;
            this.level = profile.Level;
            this.population = profile.Population;
            this.Intro = profile.Intro;
            this.cameraPos = new Vector3(profile.CameraPosX, profile.CameraPosY, profile.CameraPosZ);
            Levels level = GameData.Levels[this.level.ToString()];
            if (level != null)
                this.maxXP = level.nextLevelXp;
            if (profile.Worlds != null)
            {
                foreach (World world in profile.Worlds)
                {
                    MapData mapData = new MapData();
                    mapData.NextSpawnCheck = world.NextSpawnCheck;
                    mapData.LastAssist = world.LastAssist;
                    if (world.Buildings != null)
                    {
                        foreach (Building building in (IEnumerable<Building>)world.Buildings)
                        {
                            BuildingEntity buildingEntity = new BuildingEntity(building.Name);
                            buildingEntity.Uid = building.Uid;
                            buildingEntity.Level = building.Level;
                            buildingEntity.Mode = building.Mode;
                            buildingEntity.Flip = building.Flip;
                            buildingEntity.JobName = building.JobName;
                            if (buildingEntity.JobName == null)
                                buildingEntity.JobName = string.Empty;
                            buildingEntity.timer = building.Timer;
                            buildingEntity.eventCompleteTime = building.EventCompleteTime;
                            buildingEntity.eventStartTime = building.EventStartTime;
                            buildingEntity.Position = new Vector3(building.PositionX, building.PositionY, 0.0f);
                            if (buildingEntity.IsResourceProducer())
                            {
                                TimeSpan timeSpan = DateTime.Now - buildingEntity.eventStartTime;
                                buildingEntity.State = building.State != BuildingState.Working || timeSpan.TotalSeconds <= (double)(buildingEntity.eventCompleteTime / 60) ? building.State : BuildingState.Full;
                            }
                            else
                                buildingEntity.State = building.State;
                            buildingEntity.buildTime = building.BuildTime;
                            mapData.Buildings.Add(buildingEntity);
                        }
                    }
                    if (world.Expansions != null)
                    {
                        foreach (GameCommon.SerializedObjects.Expansion expansion in (IEnumerable<GameCommon.SerializedObjects.Expansion>)world.Expansions)
                            mapData.Expansions.Add(expansion.Index);
                    }
                    if (world.Encounters != null)
                    {
                        foreach (Encounter encounter in (IEnumerable<Encounter>)world.Encounters)
                            mapData.Encounters.Add(new EncounterArmy()
                            {
                                InstanceId = encounter.InstanceId,
                                Name = encounter.Name,
                                EventId = encounter.EventId,
                                X = encounter.X,
                                Y = encounter.Y,
                                Status = encounter.Status,
                                Type = encounter.Type,
                                Created = encounter.Created
                            });
                    }
                    this.WorldMaps.Add(world.Name, mapData);
                }
                if (!this.WorldMaps.ContainsKey("MyLand"))
                    this.WorldMaps.Add("MyLand", new MapData());
                this.populationLimit = level.populationLimit;
                this.RecalculatePopulation();
            }
            if (profile.Units != null)
            {
                foreach (Unit unit in profile.Units)
                    this.army.Add(unit.Name, new ArmyUnit()
                    {
                        Id = unit.Id,
                        Name = unit.Name,
                        level = unit.Level,
                        total = unit.Total,
                        busy = unit.Busy,
                        repairing = unit.Repairing,
                        injured = unit.Injured,
                        xp = unit.XP,
                        Upgrading = unit.Upgrading,
                        UpgradeTimer = unit.UpgradeTimer,
                        UpgradeStart = unit.UpgradeStart
                    });
            }
            if (profile.MissionsCompleted != null)
            {
                foreach (MissionsCompleted missionsCompleted in profile.MissionsCompleted)
                {
                    CompletedMission completedMission = new CompletedMission();
                    if (!this.completedMissions.ContainsKey(missionsCompleted.Name))
                    {
                        completedMission.Id = missionsCompleted.Id;
                        completedMission.MissionId = missionsCompleted.Name;
                        this.completedMissions.Add(missionsCompleted.Name, completedMission);
                    }
                }
            }
            if (profile.CurrentMission != null)
            {
                foreach (GameCommon.SerializedObjects.CurrentMission currentMission1 in profile.CurrentMission)
                {
                    CurrentMission currentMission2 = new CurrentMission();
                    currentMission2.Id = currentMission1.Id;
                    currentMission2.MissionId = currentMission1.Name;
                    currentMission2.State = currentMission1.Status;
                    if (currentMission1.Steps != null)
                    {
                        currentMission2.Steps = new Dictionary<string, MissionStep>();
                        foreach (string key in currentMission1.Steps.Keys)
                        {
                            MissionStep step = currentMission1.Steps[key];
                            currentMission2.Steps.Add(key, new MissionStep()
                            {
                                Complete = step.Complete,
                                Count = step.Count,
                                Goal = step.Goal,
                                Prereq = step.Prereq,
                                Seq = step.Seq
                            });
                        }
                    }
                    this.currentMissions.Add(currentMission1.Name, currentMission2);
                    if (currentMission2.State == MissionState.Ready || currentMission2.State == MissionState.ObjectivesDone)
                        this.AdvanceMissionPostDialog(currentMission2.MissionId);
                }
            }
            this.storage.SetMoney(profile.Storage.Money);
            this.storage.SetNanopods(profile.Storage.Currency);
            this.storage.SetResource(Resource.bars, profile.Storage.Bars);
            this.storage.SetResource(Resource.chem, profile.Storage.Chem);
            this.storage.SetResource(Resource.coal, profile.Storage.Coal);
            this.storage.SetResource(Resource.concrete, profile.Storage.Concrete);
            this.storage.SetResource(Resource.gear, profile.Storage.Gear);
            this.storage.SetResource(Resource.heart, profile.Storage.Heart);
            this.storage.SetResource(Resource.iron, profile.Storage.Iron);
            this.storage.SetResource(Resource.lumber, profile.Storage.Lumber);
            this.storage.SetResource(Resource.oil, profile.Storage.Oil);
            this.storage.SetResource(Resource.sbars, profile.Storage.Sbars);
            this.storage.SetResource(Resource.sgear, profile.Storage.Sgear);
            this.storage.SetResource(Resource.skull, profile.Storage.Skull);
            this.storage.SetResource(Resource.sskull, profile.Storage.Sskull);
            this.storage.SetResource(Resource.star, profile.Storage.Star);
            this.storage.SetResource(Resource.steel, profile.Storage.Steel);
            this.storage.SetResource(Resource.stone, profile.Storage.Stone);
            this.storage.SetResource(Resource.stooth, profile.Storage.Stooth);
            this.storage.SetResource(Resource.tooth, profile.Storage.Tooth);
            this.storage.SetResource(Resource.wood, profile.Storage.Wood);
            if (profile.Achievements != null)
            {
                this.AchievementProgress = new Dictionary<string, BNR.AchievementProgress>();
                foreach (Achievement achievement in profile.Achievements)
                    this.AchievementProgress.Add(achievement.AchievementId, new BNR.AchievementProgress(achievement));
            }
            this.UpdateStorageLimit();
            if (this.WorldMaps.Count == 0)
                this.WorldMaps.Add("map_empty", new MapData());
            if (this.OnChangeLevel == null)
                return;
            this.OnChangeLevel((object)this, new EventArgs());
        }

        public void UpdateStorage(GameCommon.SerializedObjects.Storage _storage)
        {
            this.storage.SetMoney(_storage.Money);
            this.storage.SetNanopods(_storage.Currency);
            this.storage.SetResource(Resource.bars, _storage.Bars);
            this.storage.SetResource(Resource.chem, _storage.Chem);
            this.storage.SetResource(Resource.coal, _storage.Coal);
            this.storage.SetResource(Resource.concrete, _storage.Concrete);
            this.storage.SetResource(Resource.gear, _storage.Gear);
            this.storage.SetResource(Resource.heart, _storage.Heart);
            this.storage.SetResource(Resource.iron, _storage.Iron);
            this.storage.SetResource(Resource.lumber, _storage.Lumber);
            this.storage.SetResource(Resource.oil, _storage.Oil);
            this.storage.SetResource(Resource.sbars, _storage.Sbars);
            this.storage.SetResource(Resource.sgear, _storage.Sgear);
            this.storage.SetResource(Resource.skull, _storage.Skull);
            this.storage.SetResource(Resource.sskull, _storage.Sskull);
            this.storage.SetResource(Resource.star, _storage.Star);
            this.storage.SetResource(Resource.steel, _storage.Steel);
            this.storage.SetResource(Resource.stone, _storage.Stone);
            this.storage.SetResource(Resource.stooth, _storage.Stooth);
            this.storage.SetResource(Resource.tooth, _storage.Tooth);
            this.storage.SetResource(Resource.wood, _storage.Wood);
            if (this.storage.OnChangeResource == null)
                return;
            this.storage.OnChangeResource((object)this, new EventArgs());
        }

        public Profile GetProfile()
        {
            Profile profile = new Profile()
            {
                Level = this.level,
                XP = this.xp,
                Population = this.population,
                Intro = this.Intro,
                CameraPosX = this.cameraPos.x,
                CameraPosY = this.cameraPos.y,
                CameraPosZ = this.cameraPos.z,
                Storage = new GameCommon.SerializedObjects.Storage()
            };
            profile.Storage.Money = this.storage.GetGoldAmnt();
            profile.Storage.Currency = this.storage.GetNanoAmnt();
            profile.Worlds = new List<World>();
            if (this.WorldMaps != null)
            {
                foreach (string key in this.WorldMaps.Keys)
                {
                    World world = new World();
                    world.Buildings = (IList<Building>)new List<Building>();
                    world.Name = key;
                    MapData worldMap = this.WorldMaps[key];
                    List<int> expansions = worldMap.Expansions;
                    world.LastAssist = worldMap.LastAssist;
                    world.NextSpawnCheck = worldMap.NextSpawnCheck;
                    profile.Worlds.Add(world);
                }
            }
            if (this.army != null)
            {
                profile.Units = new List<Unit>();
                foreach (string key in this.army.Keys)
                {
                    ArmyUnit armyUnit = this.army[key];
                    profile.Units.Add(new Unit()
                    {
                        Name = armyUnit.Name,
                        Level = armyUnit.level,
                        Busy = armyUnit.busy,
                        Repairing = armyUnit.repairing,
                        Injured = armyUnit.injured,
                        XP = armyUnit.xp,
                        Total = armyUnit.total,
                        UpgradeTimer = armyUnit.UpgradeTimer,
                        Upgrading = armyUnit.Upgrading,
                        UpgradeStart = armyUnit.UpgradeStart
                    });
                }
            }
            if (this.completedMissions != null)
            {
                profile.MissionsCompleted = new List<MissionsCompleted>();
                foreach (string key in this.completedMissions.Keys)
                {
                    CompletedMission completedMission = this.CompletedMissions[key];
                    profile.MissionsCompleted.Add(new MissionsCompleted()
                    {
                        Name = key,
                        Id = completedMission.Id
                    });
                }
            }
            if (this.currentMissions != null)
            {
                profile.CurrentMission = new List<GameCommon.SerializedObjects.CurrentMission>();
                foreach (string key in this.currentMissions.Keys)
                {
                    CurrentMission currentMission = this.CurrentMissions[key];
                    profile.CurrentMission.Add(new GameCommon.SerializedObjects.CurrentMission()
                    {
                        Id = currentMission.Id,
                        Name = currentMission.MissionId,
                        Status = currentMission.State
                    });
                }
            }
            if (this.AchievementProgress != null)
            {
                profile.Achievements = new List<Achievement>();
                foreach (string key in this.AchievementProgress.Keys)
                {
                    BNR.AchievementProgress achievementProgress = this.AchievementProgress[key];
                    profile.Achievements.Add(new Achievement()
                    {
                        AchievementId = key,
                        Count = achievementProgress.Count,
                        Complete = achievementProgress.Complete,
                        CompleteDate = achievementProgress.CompleteDate
                    });
                }
            }
            return profile;
        }

        public void LoadPlayer(string name)
        {
            this.xp = 50;
            this.maxXP = 100;
            this.level = 9;
            this.Name = name;
            this.storage.AddMoney(3000);
            this.storage.AddNanopods(5);
            this.population = 12;
            this.populationActive = 15;
            this.createTestArmy();
            this.WorldMaps.Add("MyLand", new MapData());
        }

        public Dictionary<string, ArmyUnit> Army
        {
            get
            {
                return this.army;
            }
            set
            {
                this.army = value;
            }
        }

        public int XP
        {
            get
            {
                return this.xp;
            }
        }

        public int Level
        {
            get
            {
                return this.level;
            }
            set
            {
                this.level = value;
            }
        }

        public Storage Storage
        {
            get
            {
                return this.storage;
            }
        }

        public void RecalculatePopulation()
        {
            int num1 = 0;
            int num2 = 0;
            bool flag = false;
            if (this.WorldMaps == null || !this.WorldMaps.ContainsKey("MyLand"))
                return;
            foreach (BuildingEntity building in this.WorldMaps["MyLand"].Buildings)
            {
                if (!building.IsUnderConstruction() && building.State != BuildingState.Sold)
                {
                    if (building.composition.componentConfigs.PopulationCapacity != null)
                        num1 += building.composition.componentConfigs.PopulationCapacity.contribution;
                    if (building.composition.componentConfigs.RequireWorkers != null)
                    {
                        if (building.State == BuildingState.Offline)
                            num2 += building.composition.componentConfigs.RequireWorkers.minWorkers;
                        else
                            num2 += building.composition.componentConfigs.RequireWorkers.workers;
                    }
                }
            }
            if (num1 != this.Population)
            {
                this.population = num1;
                flag = true;
            }
            if (num2 != this.populationActive)
            {
                this.populationActive = num2;
                flag = true;
            }
            if (!flag || this.OnChangePopulation == null)
                return;
            this.OnChangePopulation((object)this, new EventArgs());
        }

        public bool Affordable(Cost _cost)
        {
            return this.Affordable(_cost.money, _cost.currency, _cost.resources, _cost.z2points);
        }

        public bool Affordable(int money, int currency, ResourceList resources, int z2points)
        {
            bool flag = true;
            if (money > 0 && money > this.storage.GetGoldAmnt())
                flag = false;
            if (currency > 0 && currency > this.storage.GetNanoAmnt() || z2points > this.storage.GetZ2points())
                return false;
            if (resources != null)
            {
                foreach (string resourceName in Functions.ResourceNames())
                {
                    int resource = this.storage.GetResource(Functions.ResourceNameToEnum(resourceName));
                    int propertyValue = (int)Functions.GetPropertyValue((object)resources, resourceName);
                    if (propertyValue > 0 && propertyValue > resource)
                        flag = false;
                }
            }
            return flag;
        }

        public bool PopulationValid_Contribution(int contribution)
        {
            int num = 0;
            foreach (BuildingEntity building in this.WorldMaps["MyLand"].Buildings)
            {
                if (building.composition.componentConfigs.PopulationCapacity != null && building.IsUnderConstruction())
                    num += building.composition.componentConfigs.PopulationCapacity.contribution;
            }
            return this.population + contribution + num <= this.populationLimit;
        }

        public bool PopulationValid_Demand(int demand)
        {
            return this.populationActive + demand <= this.population;
        }

        public void IncreaseXP(int amount)
        {
            this.xp += amount;
            if (this.xp >= this.maxXP)
                this.LevelUp();
            if (this.OnChangeXP == null)
                return;
            this.OnChangeXP((object)this, new EventArgs());
        }

        private void LevelUp()
        {
            if (this.level >= 29)
                return;
            ++this.level;
            Levels level1 = GameData.Levels[this.level.ToString()];
            this.xp -= this.maxXP;
            if (this.xp < 0)
                this.xp = 0;
            if (level1 != null && level1.awards != null)
            {
                if (level1.awards.amount.money > 0)
                    this.storage.AddMoney(level1.awards.amount.money);
                if (level1.awards.amount.currency > 0)
                    this.storage.AddNanopods(level1.awards.amount.currency);
            }
            Levels level2 = GameData.Levels[this.level.ToString()];
            if (level2 != null)
            {
                this.maxXP = level2.nextLevelXp;
                this.populationActive = level2.populationLimit;
                this.populationLimit = level2.populationLimit;
                this.RecalculatePopulation();
            }
            if (this.OnChangeLevel == null)
                return;
            this.OnChangeLevel((object)this, new EventArgs());
        }

        public float GetXPPercent()
        {
            return this.maxXP > 0 ? (float)this.xp / (float)this.maxXP : 0.0f;
        }

        public void DeployUnit(string unit)
        {
            if (!(unit != string.Empty) || !this.army.ContainsKey(unit))
                return;
            ++this.army[unit].busy;
        }

        public void ReleaseUnit(string unit)
        {
            if (!(unit != string.Empty) || !this.army.ContainsKey(unit) || this.army[unit].busy <= 0)
                return;
            --this.army[unit].busy;
        }

        public void InjureUnit(string unit)
        {
            if (!(unit != string.Empty) || !this.army.ContainsKey(unit))
                return;
            ++this.army[unit].injured;
            this.ReleaseUnit(unit);
        }

        public void HealUnit(string unit)
        {
            if (!(unit != string.Empty) || !this.army.ContainsKey(unit) || this.army[unit].repairing <= 0)
                return;
            --this.army[unit].repairing;
        }

        public void AddToHealQueue(string unit)
        {
            if (!(unit != string.Empty) || !this.army.ContainsKey(unit))
                return;
            ++this.army[unit].repairing;
            if (this.army[unit].injured <= 0)
                return;
            --this.army[unit].injured;
        }

        public bool CanDeploy(string unit)
        {
            bool flag = false;
            if (unit != string.Empty && this.army.ContainsKey(unit) && this.army[unit].AvailableQty() > 0)
                flag = true;
            return flag;
        }

        public ArmyUnit GetUnitStats(string unit)
        {
            ArmyUnit armyUnit = (ArmyUnit)null;
            if (unit != string.Empty && this.army.ContainsKey(unit))
                armyUnit = this.army[unit];
            return armyUnit;
        }

        public List<string> GetAvailableList()
        {
            List<string> stringList = new List<string>();
            foreach (string key in this.army.Keys)
            {
                ArmyUnit armyUnit = this.army[key];
                stringList.Add(key);
            }
            return stringList;
        }

        public void AddUnit(string name, int qty)
        {
            for (int index = 1; index <= qty; ++index)
                this.AddUnit(name);
        }

        public void AddUnit(string name)
        {
            if (name == null)
                return;
            ArmyUnit armyUnit;
            if (!this.army.ContainsKey(name))
            {
                armyUnit = new ArmyUnit();
                armyUnit.Name = name;
                this.army.Add(name, armyUnit);
            }
            else
                armyUnit = this.army[name];
            ++armyUnit.total;
        }

        public string HomeMap()
        {
            return this.WorldMaps.ContainsKey("MyLand") ? "MyLand" : "map_empty";
        }

        public bool IsBuildingAchievementLocked()
        {
            bool flag = false;
            if (this.AchievementProgress != null)
            {
                foreach (string key in GameData.Achievements.Keys)
                {
                    if (GameData.Achievements[key].objective.prereq._t == "HasAnyCompositionsPrereqConfig" && !this.AchievementProgress[key].Complete)
                        flag = true;
                }
            }
            return flag;
        }

        public bool IsUnitAchievementLocked()
        {
            bool flag = false;
            if (this.AchievementProgress != null)
            {
                foreach (string key in GameData.Achievements.Keys)
                {
                    if (GameData.Achievements[key].objective.prereq._t == "HasAnyUnitsPrereqConfig" && !this.AchievementProgress[key].Complete)
                        flag = true;
                }
            }
            return flag;
        }

        public void UpdateAchievementProgress_Building()
        {
            foreach (string key in this.AchievementProgress.Keys)
            {
                BNR.AchievementProgress achievementProgress = this.AchievementProgress[key];
                if (GameData.Achievements[key].objective.prereq._t == "HasAnyCompositionsPrereqConfig")
                {
                    ++achievementProgress.Count;
                    if (achievementProgress.Count >= GameData.Achievements[key].objective.prereq.count)
                    {
                        achievementProgress.Complete = true;
                        achievementProgress.CompleteDate = DateTime.Now;
                        this.storage.AddNanopods(GameData.Achievements[key].reward.currency);
                    }
                    if (this.OnAchievementUpdated != null)
                        this.OnAchievementUpdated((object)this, new ButtonEventArgs(ButtonValue.OK, key));
                }
            }
        }

        public void UpdateAchievementProgress_Unit()
        {
            foreach (string key in this.AchievementProgress.Keys)
            {
                BNR.AchievementProgress achievementProgress = this.AchievementProgress[key];
                if (GameData.Achievements[key].objective.prereq._t == "HasAnyUnitsPrereqConfig")
                {
                    ++achievementProgress.Count;
                    if (achievementProgress.Count >= GameData.Achievements[key].objective.prereq.count)
                    {
                        achievementProgress.Complete = true;
                        this.storage.AddNanopods(GameData.Achievements[key].reward.currency);
                    }
                    if (this.OnAchievementUpdated != null)
                        this.OnAchievementUpdated((object)this, new ButtonEventArgs(ButtonValue.OK, key));
                }
            }
        }

        public void PlayerDataUpdated()
        {
            if (this.OnPlayerDataUpdated == null)
                return;
            this.OnPlayerDataUpdated((object)this, new EventArgs());
        }

        public void TestBoost()
        {
            GameData.Player.IncreaseXP(1000);
        }
    }
}
