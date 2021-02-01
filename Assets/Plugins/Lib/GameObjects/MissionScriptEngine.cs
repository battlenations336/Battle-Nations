
using GameCommon;
using GameCommon.SerializedObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.SceneManagement;

namespace BNR
{
    public class MissionScriptEngine
    {
        private List<string> newMissions = new List<string>();
        private object world;

        public MissionScriptEngine()
        {
            this.newMissions = new List<string>();
        }

        public List<string> NewMissions
        {
            get
            {
                return this.newMissions;
            }
            set
            {
                this.newMissions = value;
            }
        }

        public bool CheckObjectives(string missionId, string newItem)
        {
            Missions mission = GameData.Missions[missionId];
            int num = missionId == "p01_NEWINTRO_060_StartCabbage" ? 1 : 0;
            bool flag = this.CheckObjectives(missionId, mission.objectives, newItem);
            if (flag && mission.persistenceRules != null)
                flag = false;
            return flag;
        }

        public bool CheckObjectives(
          string missionId,
          Dictionary<string, Missions.Objective> _objectives,
          string newItem)
        {
            bool flag = true;
            foreach (string key in _objectives.Keys)
            {
                if (!this.CheckObjective(_objectives[key], key, missionId, newItem))
                    flag = false;
            }
            return flag;
        }

        public bool CheckObjective(
          Missions.Objective objData,
          string objKey,
          string missionId,
          string newItem)
        {
            bool flag1 = true;
            bool flag2 = false;
            int num1 = 0;
            if (missionId == "p01_NEWINTRO_SQ5_010_SecondStoneQuarry")
                num1 = 0;
            switch (objData.prereq._t)
            {
                case "ActiveMissionPrereqConfig":
                    bool flag3 = false;
                    foreach (string missionId1 in objData.prereq.missionIds)
                    {
                        if (GameData.Player.CurrentMissions.ContainsKey(missionId1))
                            flag3 = true;
                    }
                    if (!flag3)
                    {
                        if (objData.prereq.missionActive)
                        {
                            flag1 = false;
                            break;
                        }
                        break;
                    }
                    if (!objData.prereq.missionActive)
                    {
                        flag1 = false;
                        break;
                    }
                    break;
                case "AttackNPCBuildingPrereqConfig":
                    flag2 = true;
                    GameData.Player.CurrentMissions[missionId].UpdateAttackNPCBuilding(1);
                    if (!GameData.Player.CurrentMissions[missionId].FindStep("AttackNPCBuildingPrereqConfig").Complete)
                    {
                        flag1 = false;
                        break;
                    }
                    break;
                case "BuildingLevelPrereqConfig":
                    bool flag4 = false;
                    foreach (string compositionId in objData.prereq.compositionIds)
                    {
                        string tmpBuilding = compositionId;
                        List<BuildingEntity> list = GameData.Player.WorldMaps["MyLand"].Buildings.Where<BuildingEntity>((Func<BuildingEntity, bool>)(x => x.Name == tmpBuilding && x.Level >= objData.prereq.level)).ToList<BuildingEntity>();
                        if (list != null && list.Count<BuildingEntity>() > 0)
                            flag4 = true;
                    }
                    if (!flag4)
                    {
                        flag1 = false;
                        break;
                    }
                    break;
                case "CollectJobPrereqConfig":
                    flag2 = true;
                    if (!GameData.Player.CurrentMissions[missionId].Steps[objKey].Complete)
                    {
                        flag1 = false;
                        break;
                    }
                    break;
                case "CollectProjectPrereqConfig":
                    flag2 = true;
                    if (!GameData.Player.CurrentMissions[missionId].Steps[objKey].Complete)
                    {
                        flag1 = false;
                        break;
                    }
                    break;
                case "CollectTaxesPrereqConfig":
                    flag2 = true;
                    if (!GameData.Player.CurrentMissions[missionId].FindStep("CollectTaxesPrereqConfig").Complete)
                    {
                        flag1 = false;
                        break;
                    }
                    break;
                case "CompleteAnyMissionPrereqConfig":
                    bool flag5 = false;
                    foreach (string missionId1 in objData.prereq.missionIds)
                    {
                        if (GameData.Player.CompletedMissions.ContainsKey(missionId1))
                            flag5 = true;
                    }
                    if (!flag5)
                    {
                        flag1 = false;
                        break;
                    }
                    break;
                case "CreateStructurePrereqConfig":
                    List<BuildingEntity> list1 = GameData.Player.WorldMaps["MyLand"].Buildings.Where<BuildingEntity>((Func<BuildingEntity, bool>)(x => x.Name == objData.prereq.structureType && x.State == BuildingState.RibbonTime)).ToList<BuildingEntity>();
                    if (list1 == null || list1.Count<BuildingEntity>() < objData.prereq.count)
                    {
                        flag1 = false;
                        break;
                    }
                    break;
                case "DefeatEncounterPrereqConfig":
                    if (!GameData.Player.DefeatedOccupations.Contains(objData.prereq.encounterId))
                    {
                        flag1 = false;
                        break;
                    }
                    break;
                case "DefeatEncounterSetPrereqConfig":
                    flag2 = true;
                    if (!GameData.Player.CurrentMissions[missionId].FindStep("DefeatEncounterSetPrereqConfig").Complete)
                    {
                        flag1 = false;
                        break;
                    }
                    break;
                case "DefeatOccupationPrereqConfig":
                    if (!GameData.Player.DefeatedOccupations.Contains(objData.prereq.occupationId))
                    {
                        flag1 = false;
                        break;
                    }
                    break;
                case "EnterOpponentLandPrereqConfig":
                    if (MapConfig.npcId == objData.prereq.opponentId)
                        GameData.Player.CurrentMissions[missionId].Steps[objKey].Complete = true;
                    if (!GameData.Player.CurrentMissions[missionId].Steps[objKey].Complete)
                    {
                        flag1 = false;
                        break;
                    }
                    break;
                case "EnterStatePrereqConfig":
                    string str = MapConfig.npcId;
                    if (string.IsNullOrEmpty(str))
                        str = "MyLand";
                    if (str != objData.prereq.state)
                    {
                        flag1 = false;
                        break;
                    }
                    break;
                case "FinishBattlePrereqConfig":
                    flag2 = true;
                    if (!GameData.Player.CurrentMissions[missionId].FindStep("FinishBattlePrereqConfig").Complete)
                    {
                        flag1 = false;
                        break;
                    }
                    break;
                case "HasCompositionPrereqConfig":
                    flag2 = true;
                    GameData.Player.CurrentMissions[missionId].UpdateHasComposition();
                    if (!GameData.Player.CurrentMissions[missionId].FindStep("HasCompositionPrereqConfig").Complete)
                    {
                        flag1 = false;
                        break;
                    }
                    break;
                case "HaveAnyOfTheseStructuresPrereqConfig":
                    int num2 = 0;
                    if (objData.prereq.buildings != null)
                    {
                        foreach (string building in objData.prereq.buildings)
                        {
                            string tmpBuilding = building;
                            List<BuildingEntity> list2 = GameData.Player.WorldMaps["MyLand"].Buildings.Where<BuildingEntity>((Func<BuildingEntity, bool>)(x => x.Name == tmpBuilding && x.State != BuildingState.Offline)).ToList<BuildingEntity>();
                            if (list2 != null && list2.Count<BuildingEntity>() >= 1)
                            {
                                foreach (BuildingEntity buildingEntity in list2)
                                {
                                    if (!buildingEntity.IsUnderConstruction())
                                        ++num2;
                                }
                            }
                        }
                        if (num2 < objData.prereq.count)
                        {
                            flag1 = false;
                            break;
                        }
                        break;
                    }
                    break;
                case "HaveLandExpansionsPrereqConfig":
                    MissionScriptEngine.UpdateLandExpansion();
                    flag2 = true;
                    if (!GameData.Player.CurrentMissions[missionId].FindStep("HaveLandExpansionsPrereqConfig").Complete)
                    {
                        flag1 = false;
                        break;
                    }
                    break;
                case "HaveOneOfTheseStructuresPrereqConfig":
                    bool flag6 = false;
                    if (objData.prereq.buildingCounts != null)
                    {
                        foreach (string key in objData.prereq.buildingCounts.Keys)
                        {
                            string tmpBuilding = key;
                            List<BuildingEntity> list2 = GameData.Player.WorldMaps["MyLand"].Buildings.Where<BuildingEntity>((Func<BuildingEntity, bool>)(x => x.Name == tmpBuilding && x.State != BuildingState.Offline)).ToList<BuildingEntity>();
                            if (list2 != null && list2.Count<BuildingEntity>() >= objData.prereq.buildingCounts[tmpBuilding])
                                flag6 = true;
                        }
                    }
                    if (!flag6)
                    {
                        flag1 = false;
                        break;
                    }
                    break;
                case "LevelPrereqConfig":
                    if (GameData.Player.Level < objData.prereq.level)
                    {
                        flag1 = false;
                        break;
                    }
                    break;
                case "MinPopulationCapacityPrereqConfig":
                    if (GameData.Player.Population < objData.prereq.capacity)
                    {
                        flag1 = false;
                        break;
                    }
                    break;
                case "MoveBuildingPrereqConfig":
                    flag2 = true;
                    if (!GameData.Player.CurrentMissions[missionId].FindStep("MoveBuildingPrereqConfig").Complete)
                    {
                        flag1 = false;
                        break;
                    }
                    break;
                case "PanCameraPrereqConfig":
                    if (SceneManager.GetActiveScene().name != "PlayerMap")
                    {
                        flag1 = false;
                        break;
                    }
                    break;
                case "StartJobPrereqConfig":
                    bool flag7 = false;
                    foreach (BuildingEntity building in GameData.Player.WorldMaps["MyLand"].Buildings)
                    {
                        if ((building.State == BuildingState.Working || building.State == BuildingState.Full) && building.JobName == objData.prereq.jobId)
                            flag7 = true;
                    }
                    if (!flag7 && newItem != objData.prereq.projectId)
                    {
                        flag1 = false;
                        break;
                    }
                    break;
                case "StartProjectPrereqConfig":
                    bool flag8 = false;
                    foreach (BuildingEntity building in GameData.Player.WorldMaps["MyLand"].Buildings)
                    {
                        if ((building.State == BuildingState.Working || building.State == BuildingState.Full) && building.JobName == objData.prereq.projectId)
                            flag8 = true;
                    }
                    if (!flag8 && newItem != objData.prereq.projectId)
                    {
                        flag1 = false;
                        break;
                    }
                    break;
                case "TurnInPrereqConfig":
                    flag2 = true;
                    if (!GameData.Player.CurrentMissions[missionId].Steps[objKey].Complete)
                    {
                        flag1 = false;
                        break;
                    }
                    break;
                case "UnitLevelUpStartedPrereqConfig":
                    if (string.IsNullOrEmpty(GameData.Player.Army.Where<KeyValuePair<string, ArmyUnit>>((Func<KeyValuePair<string, ArmyUnit>, bool>)(x => x.Value.Upgrading)).FirstOrDefault<KeyValuePair<string, ArmyUnit>>().Key))
                    {
                        flag1 = false;
                        break;
                    }
                    break;
                case "UnitLevelupCollectedPrereqConfig":
                    if (string.IsNullOrEmpty(GameData.Player.Army.Where<KeyValuePair<string, ArmyUnit>>((Func<KeyValuePair<string, ArmyUnit>, bool>)(x => x.Value.level > 0)).FirstOrDefault<KeyValuePair<string, ArmyUnit>>().Key))
                    {
                        flag1 = false;
                        break;
                    }
                    break;
                default:
                    flag1 = false;
                    break;
            }
            return flag1;
        }

        public string GetCurrentIcon(
          string missionId,
          Dictionary<string, Missions.Objective> _objectives,
          string newItem)
        {
            string str = (string)null;
            string index = (string)null;
            foreach (string key in _objectives.Keys)
            {
                Missions.Objective objective = _objectives[key];
                if (!this.CheckObjective(objective, key, missionId, newItem) && index == null && objective.icon != null)
                    index = key;
            }
            if (index != null)
                str = _objectives[index].icon;
            return str;
        }

        public void CheckStartEffects(string _mission)
        {
            Missions mission = GameData.Missions[_mission];
            string encounterId = string.Empty;
            if (mission == null)
                return;
            if (mission.serverEffects != null)
            {
                foreach (Missions.ServerEffect serverEffect in mission.serverEffects)
                {
                    string t = serverEffect._t;
                    if (!(t == "SpawnNPCEffect"))
                    {
                        if (!(t == "EncounterArmyEffect"))
                        {
                            if (!(t == "NPCOccupyEffect"))
                            {
                                if (!(t == "NPCOccupySetEffect"))
                                {
                                    if (t == "SpawnEncountersEffect")
                                    {
                                        int num = 0;
                                        int index1 = 0;
                                        string worldId = "MyLand";
                                        BattleEncounterTable table = GameData.BattleEncounters.tables[serverEffect.encounterTable];
                                        if (!string.IsNullOrEmpty(table.npcId))
                                            worldId = table.npcId;
                                        KeyValuePair<string, Missions.Objective> keyValuePair = mission.objectives.Where<KeyValuePair<string, Missions.Objective>>((Func<KeyValuePair<string, Missions.Objective>, bool>)(x => x.Value.prereq._t == "DefeatEncounterSetPrereqConfig")).FirstOrDefault<KeyValuePair<string, Missions.Objective>>();
                                        if (keyValuePair.Value != null && keyValuePair.Value.prereq._t == "DefeatEncounterSetPrereqConfig")
                                            num = keyValuePair.Value.prereq.count;
                                        if (num > 0)
                                        {
                                            for (int index2 = 0; index2 < num; ++index2)
                                            {
                                                if (index1 > table.encounters.Length)
                                                    index1 = 0;
                                                GameData.Player.CreateEncounter(worldId, table.encounters[index1].encounterId, EncounterType.Mission, _mission, mission.forceStart == 1);
                                                ++index1;
                                            }
                                        }
                                    }
                                }
                                else if (serverEffect.encounters != null)
                                {
                                    GameData.Player.CheckSpawnWorld(serverEffect.landNPCId);
                                    foreach (Missions.EncounterSet encounter in serverEffect.encounters)
                                        GameData.Player.CreateEncounter(serverEffect.landNPCId, encounter.encounterId, EncounterType.Mission, _mission, mission.forceStart == 1);
                                }
                            }
                            else
                            {
                                GameData.Player.CheckSpawnWorld(serverEffect.landNPCId);
                                GameData.Player.CreateEncounter(serverEffect.landNPCId, serverEffect.encounterId, EncounterType.Mission, _mission, mission.forceStart == 1);
                            }
                        }
                        else if (string.IsNullOrEmpty(serverEffect.worldMapId))
                        {
                            if (MapConfig.mapName == "MyLand")
                                GameData.Player.CreateEncounter("MyLand", serverEffect.encounterId, EncounterType.Mission, _mission, mission.forceStart == 1);
                        }
                        else
                            GameData.Player.CreateEncounter(serverEffect.worldMapId, serverEffect.encounterId, EncounterType.Mission, _mission, mission.forceStart == 1);
                    }
                    else
                    {
                        foreach (string npcId in serverEffect.npcIds)
                            GameData.Player.CheckSpawnWorld(npcId);
                    }
                }
            }
            foreach (Missions.Objective objective in mission.objectives.Values)
            {
                string t = objective.prereq._t;
                if (!(t == "DefeatOccupationPrereqConfig"))
                {
                    if (t == "DefeatEncounterPrereqConfig")
                        encounterId = objective.prereq.encounterId;
                }
                else
                    encounterId = objective.prereq.occupationId;
            }
            foreach (Missions.Objective objective in mission.objectives.Values)
            {
                foreach (Missions.ObjectiveStep step in objective.steps)
                {
                    foreach (Missions.CompleteRule completeRule in step.completeRules.Values)
                    {
                        if (completeRule._t == "EnterStatePrereqConfig" && completeRule.prereq.state == "CombatSetup")
                        {
                            BattleConfig.InitFromEncounter(encounterId);
                            SceneManager.LoadScene("BattleMap");
                        }
                    }
                    foreach (Missions.Effect effect in step.startEffects.Values)
                    {
                        string t = effect._t;
                        if (!(t == "ScriptedBattleEffect"))
                        {
                            if (t == "SwapLandEffect" && SceneManager.GetActiveScene().name != "PlayerMap")
                            {
                                MapConfig.InitHome();
                                SceneManager.LoadScene("PlayerMap");
                            }
                        }
                        else if (!GameData.Player.DefeatedOccupations.Contains(encounterId) && !GameData.Player.Occupations.Contains(encounterId))
                        {
                            GameData.Player.Occupations.Add(encounterId);
                            BattleConfig.TestId = string.Empty;
                            BattleConfig.OccupationId = encounterId;
                            BattleConfig.MapName = "BattleMapJungle";
                            BattleConfig.MapLayout = BattleMapType.ThreeByFive;
                            BattleConfig.MaxDefence = 0;
                            BattleConfig.MaxOffence = 7;
                            BattleConfig.ForceStart = mission.forceStart == 1;
                            SceneManager.LoadScene("BattleMap");
                        }
                    }
                }
            }
            GameData.Player.StartMissionDialog(_mission);
        }

        public void loadData()
        {
            this.newMissions = new List<string>();
            foreach (string key in GameData.Missions.Keys)
            {
                int num1 = key == "p01_INTRO_010_OpeningBattleDialog" ? 1 : 0;
                int num2 = key == "p01_NEW2INTRO_020_OpeningBattle" ? 1 : 0;
                int num3 = key == "p01_NEWINTRO_150_BuildFarm" ? 1 : 0;
                Missions mission = GameData.Missions[key];
                bool flag1 = true;
                bool flag2;
                if (mission.startRules != null)
                {
                    if (mission.persistenceRules != null)
                        flag2 = false;
                    else if (mission.startScript == null && mission.finishScript == null && mission.objectives == null)
                    {
                        flag2 = false;
                    }
                    else
                    {
                        foreach (Missions.StartRule startRule in mission.startRules.Values)
                        {
                            Missions.StartRule rule = startRule;
                            switch (rule.prereq._t)
                            {
                                case "ActiveMissionPrereqConfig":
                                    foreach (string missionId in rule.prereq.missionIds)
                                    {
                                        if (rule.prereq.inverse.HasValue)
                                        {
                                            bool? inverse = rule.prereq.inverse;
                                            bool flag3 = false;
                                            if (!(inverse.GetValueOrDefault() == flag3 & inverse.HasValue))
                                            {
                                                if (GameData.Player.CurrentMissions.ContainsKey(missionId))
                                                {
                                                    flag1 = false;
                                                    continue;
                                                }
                                                continue;
                                            }
                                        }
                                        if (!GameData.Player.CurrentMissions.ContainsKey(missionId))
                                            flag1 = false;
                                    }
                                    continue;
                                case "ActiveTagPrereqConfig":
                                    flag1 = false;
                                    continue;
                                case "BuildingLevelPrereqConfig":
                                    flag1 = false;
                                    continue;
                                case "CollectJobPrereqConfig":
                                    flag1 = false;
                                    continue;
                                case "CollectStructurePrereqConfig":
                                    flag1 = false;
                                    continue;
                                case "CompleteAnyMissionPrereqConfig":
                                    bool flag4 = false;
                                    foreach (string missionId in rule.prereq.missionIds)
                                    {
                                        if (GameData.Player.CompletedMissions.ContainsKey(missionId))
                                            flag4 = true;
                                    }
                                    if (!flag4)
                                    {
                                        flag1 = false;
                                        continue;
                                    }
                                    continue;
                                case "CompleteMissionPrereqConfig":
                                    if (rule.prereq.inverse.HasValue)
                                    {
                                        bool? inverse = rule.prereq.inverse;
                                        bool flag3 = false;
                                        if (!(inverse.GetValueOrDefault() == flag3 & inverse.HasValue))
                                        {
                                            if (GameData.Player.CompletedMissions.ContainsKey(rule.prereq.missionId))
                                            {
                                                flag1 = false;
                                                continue;
                                            }
                                            continue;
                                        }
                                    }
                                    if (!GameData.Player.CompletedMissions.ContainsKey(rule.prereq.missionId))
                                    {
                                        flag1 = false;
                                        continue;
                                    }
                                    continue;
                                case "CreateStructurePrereqConfig":
                                    flag1 = false;
                                    continue;
                                case "EnterOpponentLandPrereqConfig":
                                    flag1 = false;
                                    continue;
                                case "EnterStatePrereqConfig":
                                    if (rule.prereq.state == "MyLand")
                                    {
                                        if (MapConfig.mapName != "MyLand")
                                        {
                                            flag1 = false;
                                            continue;
                                        }
                                        continue;
                                    }
                                    if (MapConfig.mapName != rule.prereq.state)
                                    {
                                        flag1 = false;
                                        continue;
                                    }
                                    continue;
                                case "HasCompositionPrereqConfig":
                                    List<BuildingEntity> list = GameData.Player.WorldMaps["MyLand"].Buildings.Where<BuildingEntity>((Func<BuildingEntity, bool>)(x => x.Name == rule.prereq.compositionName && x.State != BuildingState.Offline && (x.State != BuildingState.UnderConstruction1 && x.State != BuildingState.UnderConstruction2) && x.State != BuildingState.RibbonTime)).ToList<BuildingEntity>();
                                    if (list == null || list.Count<BuildingEntity>() < rule.prereq.count)
                                    {
                                        flag1 = false;
                                        continue;
                                    }
                                    continue;
                                case "HaveAnyOfTheseStructuresPrereqConfig":
                                    flag1 = false;
                                    continue;
                                case "LevelPrereqConfig":
                                    if (rule.prereq.inverse.HasValue)
                                    {
                                        bool? inverse = rule.prereq.inverse;
                                        bool flag3 = false;
                                        if (!(inverse.GetValueOrDefault() == flag3 & inverse.HasValue))
                                        {
                                            if (rule.prereq.level <= GameData.Player.Level)
                                            {
                                                flag1 = false;
                                                continue;
                                            }
                                            continue;
                                        }
                                    }
                                    if (rule.prereq.level > GameData.Player.Level)
                                    {
                                        flag1 = false;
                                        continue;
                                    }
                                    continue;
                                case "UnitReadyToLevelUpPrereqConfig":
                                    if (string.IsNullOrEmpty(GameData.Player.Army.Where<KeyValuePair<string, ArmyUnit>>((Func<KeyValuePair<string, ArmyUnit>, bool>)(x => x.Value.level == 0 && x.Value.ReadyToPromote())).FirstOrDefault<KeyValuePair<string, ArmyUnit>>().Key))
                                    {
                                        flag1 = false;
                                        continue;
                                    }
                                    continue;
                                default:
                                    continue;
                            }
                        }
                        if (flag1 && !GameData.Player.CompletedMissions.ContainsKey(key) && (!GameData.Player.CurrentMissions.ContainsKey(key) && !this.newMissions.Contains(key)))
                            this.newMissions.Add(key);
                    }
                }
            }
        }

        public static Dictionary<string, MissionStep> GetMissionSteps(
          string _missionId)
        {
            Dictionary<string, MissionStep> dictionary = (Dictionary<string, MissionStep>)null;
            if (GameData.Missions.ContainsKey(_missionId))
            {
                Missions mission = GameData.Missions[_missionId];
                foreach (string key in mission.objectives.Keys)
                {
                    Missions.Objective objective = mission.objectives[key];
                    if (MissionScriptEngine.IsTrackable(objective.prereq._t))
                    {
                        if (dictionary == null)
                            dictionary = new Dictionary<string, MissionStep>();
                        MissionStep missionStep = new MissionStep();
                        Tuple<string, int> stepGoal = MissionScriptEngine.GetStepGoal(objective.prereq);
                        missionStep.Complete = false;
                        missionStep.Count = 0;
                        missionStep.Target = stepGoal.Item1;
                        missionStep.Goal = stepGoal.Item2;
                        missionStep.Prereq = objective.prereq._t;
                        missionStep.Seq = key;
                        dictionary.Add(key, missionStep);
                    }
                }
            }
            return dictionary;
        }

        public static bool IsTrackable(string prereq)
        {
            bool flag = false;
            switch (prereq)
            {
                case "AttackNPCBuildingPrereqConfig":
                case "CollectJobPrereqConfig":
                case "CollectProjectPrereqConfig":
                case "CollectTaxesPrereqConfig":
                case "DefeatEncounterSetPrereqConfig":
                case "EnterOpponentLandPrereqConfig":
                case "FinishBattlePrereqConfig":
                case "HasCompositionPrereqConfig":
                case "HaveLandExpansionsPrereqConfig":
                case "MoveBuildingPrereqConfig":
                case "TurnInPrereqConfig":
                    flag = true;
                    break;
            }
            return flag;
        }

        public static Tuple<string, int> GetStepGoal(Missions.StartRulePrereq prereq)
        {
            string empty = string.Empty;
            int num = 0;
            if (prereq.toll != null)
            {
                Tuple<string, int> tollValue = Functions.GetTollValue((object)prereq.toll.amount.resources);
                empty = tollValue.Item1;
                num = tollValue.Item2;
            }
            else if (prereq.count != 0)
                num = prereq.count;
            return Tuple.Create<string, int>(empty, num);
        }

        public static void UpdateTaxCollection(BuildingEntity entity)
        {
            foreach (string key in GameData.Player.CurrentMissions.Keys)
            {
                CurrentMission currentMission = GameData.Player.CurrentMissions[key];
                if (currentMission.HasTaxCollection())
                    currentMission.UpdateTaxCollection();
            }
        }

        public static void UpdateTurnIn(string item, int quantity)
        {
            foreach (string key in GameData.Player.CurrentMissions.Keys)
            {
                CurrentMission currentMission = GameData.Player.CurrentMissions[key];
                if (currentMission.HasTurnIn(item))
                    currentMission.UpdateTurnIn(item, quantity);
            }
        }

        public static void UpdateLandExpansion()
        {
            foreach (string key in GameData.Player.CurrentMissions.Keys)
            {
                CurrentMission currentMission = GameData.Player.CurrentMissions[key];
                if (currentMission.HasLandExpansion())
                    currentMission.UpdateLandExpansionCollection();
            }
        }

        public static bool UpdateMoveBuilding(BuildingEntity _entity)
        {
            foreach (string key in GameData.Player.CurrentMissions.Keys)
            {
                CurrentMission currentMission = GameData.Player.CurrentMissions[key];
                if (currentMission.HasMoveBuilding())
                    return currentMission.UpdateMoveBuilding(_entity);
            }
            return false;
        }

        public static bool UpdateLevelupUnit()
        {
            foreach (string key in GameData.Player.CurrentMissions.Keys)
            {
                CurrentMission currentMission = GameData.Player.CurrentMissions[key];
                if (currentMission.HasLevelupStartedUnit() || currentMission.HasLevelupCompleteUnit())
                    return true;
            }
            return false;
        }

        public static bool UpdateCompositionReq()
        {
            foreach (string key in GameData.Player.CurrentMissions.Keys)
            {
                CurrentMission currentMission = GameData.Player.CurrentMissions[key];
                if (currentMission.HasCompositionReq())
                    return currentMission.UpdateHasComposition();
            }
            return false;
        }

        public static bool UpdateCollectJob(string jobName)
        {
            foreach (string key in GameData.Player.CurrentMissions.Keys)
            {
                CurrentMission currentMission = GameData.Player.CurrentMissions[key];
                if (currentMission.HasCollectJob(jobName))
                    return currentMission.UpdateCollectJob(jobName);
            }
            return false;
        }

        public static bool UpdateCollectProject(string jobName)
        {
            foreach (string key in GameData.Player.CurrentMissions.Keys)
            {
                CurrentMission currentMission = GameData.Player.CurrentMissions[key];
                if (currentMission.HasCollectProject(jobName))
                    return currentMission.UpdateCollectProject(jobName);
            }
            return false;
        }

        public static void UpdateBattleMissions(string encounter)
        {
            MissionScriptEngine.UpdateDefeatEncounterSet(encounter);
            MissionScriptEngine.UpdateFinishBattle(encounter);
        }

        public static bool UpdateDefeatEncounterSet(string encounter)
        {
            foreach (string key in GameData.Player.CurrentMissions.Keys)
            {
                CurrentMission currentMission = GameData.Player.CurrentMissions[key];
                if (currentMission.HasDefeatEncounterSet(encounter))
                    return currentMission.UpdateDefeatEncounterSet(encounter);
            }
            return false;
        }

        public static bool UpdateFinishBattle(string encounter)
        {
            foreach (string key in GameData.Player.CurrentMissions.Keys)
            {
                CurrentMission currentMission = GameData.Player.CurrentMissions[key];
                if (currentMission.HasFinishBattle(encounter))
                    return currentMission.UpdateFinishBattle(encounter);
            }
            return false;
        }
    }
}
