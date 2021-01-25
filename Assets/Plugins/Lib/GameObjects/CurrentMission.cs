
using GameCommon;
using GameCommon.SerializedObjects;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BNR
{
    public class CurrentMission
    {
        private string name;

        public int Id { get; set; }

        public string MissionId { get; set; }

        public MissionState State { get; set; }

        public Dictionary<string, MissionStep> Steps { get; set; }

        public Missions.StartRulePrereq GetObjectivePrereq(string seq)
        {
            return GameData.Missions[this.MissionId].objectives[seq].prereq;
        }

        public bool StepsComplete()
        {
            bool flag = true;
            if (this.Steps != null)
            {
                foreach (string key in this.Steps.Keys)
                {
                    MissionStep step = this.Steps[key];
                    if (step.Count < step.Goal)
                        flag = false;
                }
            }
            return flag;
        }

        public bool HasBattleStep()
        {
            bool flag = false;
            foreach (Missions.ServerEffect serverEffect in GameData.Missions[this.MissionId].serverEffects)
            {
                string t = serverEffect._t;
                if (t == "SpawnNPCEffect" || t == "EncounterArmyEffect" || (t == "NPCOccupyEffect" || t == "NPCOccupySetEffect") || t == "SpawnEncountersEffect")
                    flag = true;
            }
            return flag;
        }

        public bool HasEncounterSpawn()
        {
            bool flag = false;
            foreach (MapData mapData in GameData.Player.WorldMaps.Values)
            {
                EncounterArmy encounterArmy = mapData.Encounters.Where<EncounterArmy>((Func<EncounterArmy, bool>)(x => x.EventId == this.MissionId)).FirstOrDefault<EncounterArmy>();
                if (encounterArmy != null && !string.IsNullOrEmpty(encounterArmy.EventId))
                    flag = true;
            }
            return flag;
        }

        public MissionStep FindStep(string name)
        {
            MissionStep missionStep = (MissionStep)null;
            if (this.Steps != null)
                missionStep = this.Steps.Values.Where<MissionStep>((Func<MissionStep, bool>)(x => x.Prereq == name)).FirstOrDefault<MissionStep>();
            return missionStep;
        }

        public MissionStep FindJobStep(string name, string jobName)
        {
            MissionStep missionStep = (MissionStep)null;
            Missions mission = GameData.Missions[this.MissionId];
            if (mission.objectives != null)
            {
                foreach (string key in mission.objectives.Keys)
                {
                    Missions.Objective objective = mission.objectives[key];
                    if (objective.prereq._t == name && objective.prereq.jobId == jobName)
                        missionStep = this.Steps[key];
                }
            }
            return missionStep;
        }

        public MissionStep FindProjectStep(string name, string projectName)
        {
            MissionStep missionStep = (MissionStep)null;
            Missions mission = GameData.Missions[this.MissionId];
            if (mission.objectives != null)
            {
                foreach (string key in mission.objectives.Keys)
                {
                    Missions.Objective objective = mission.objectives[key];
                    if (objective.prereq._t == name && objective.prereq.projectId == projectName)
                        missionStep = this.Steps[key];
                }
            }
            return missionStep;
        }

        public Missions.StartRulePrereq FindUntrackedStep(string name)
        {
            Missions.StartRulePrereq startRulePrereq = (Missions.StartRulePrereq)null;
            Missions mission = GameData.Missions[this.MissionId];
            if (mission.objectives != null)
            {
                foreach (string key in mission.objectives.Keys)
                {
                    Missions.Objective objective = mission.objectives[key];
                    if (objective.prereq._t == name)
                        startRulePrereq = objective.prereq;
                }
            }
            return startRulePrereq;
        }

        public MissionStep FindEncounterStep(string name, string encounter)
        {
            MissionStep missionStep = (MissionStep)null;
            Missions mission = GameData.Missions[this.MissionId];
            if (mission.objectives != null)
            {
                foreach (string key in mission.objectives.Keys)
                {
                    Missions.Objective objective = mission.objectives[key];
                    if (objective.prereq._t == name)
                    {
                        if (objective.prereq.encounterIds != null && ((IEnumerable<string>)objective.prereq.encounterIds).Contains<string>(encounter))
                            missionStep = this.Steps[key];
                        else if (objective.prereq.encounterId != null && objective.prereq.encounterId == encounter)
                            missionStep = this.Steps[key];
                    }
                }
            }
            return missionStep;
        }

        public MissionStep FindResourceStep(string name, string item)
        {
            MissionStep missionStep = (MissionStep)null;
            Missions mission = GameData.Missions[this.MissionId];
            if (mission.objectives != null)
            {
                foreach (string key in mission.objectives.Keys)
                {
                    Missions.Objective objective = mission.objectives[key];
                    if (objective.prereq._t == name)
                    {
                        Tuple<string, int> tollValue = Functions.GetTollValue((object)objective.prereq.toll.amount.resources);
                        if (!(tollValue.Item1 == string.Empty) && !(tollValue.Item1 != item) && objective.prereq._t == name)
                            missionStep = this.Steps[key];
                    }
                }
            }
            return missionStep;
        }

        public bool HasDefeatEncounterSet(string encounter)
        {
            bool flag = false;
            MissionStep encounterStep = this.FindEncounterStep("DefeatEncounterSetPrereqConfig", encounter);
            if (encounterStep != null && encounterStep.Prereq != string.Empty)
                flag = true;
            return flag;
        }

        public bool HasFinishBattle(string encounter)
        {
            bool flag = false;
            MissionStep encounterStep = this.FindEncounterStep("FinishBattlePrereqConfig", encounter);
            if (encounterStep != null && encounterStep.Prereq != string.Empty)
                flag = true;
            return flag;
        }

        public bool HasTaxCollection()
        {
            bool flag = false;
            MissionStep step = this.FindStep("CollectTaxesPrereqConfig");
            if (step != null && step.Prereq != string.Empty)
                flag = true;
            return flag;
        }

        public bool HasLandExpansion()
        {
            bool flag = false;
            MissionStep step = this.FindStep("HaveLandExpansionsPrereqConfig");
            if (step != null && step.Prereq != string.Empty)
                flag = true;
            return flag;
        }

        public bool HasMoveBuilding()
        {
            bool flag = false;
            MissionStep step = this.FindStep("MoveBuildingPrereqConfig");
            if (step != null && step.Prereq != string.Empty)
                flag = true;
            return flag;
        }

        public bool HasLevelupStartedUnit()
        {
            bool flag = false;
            Missions.StartRulePrereq untrackedStep = this.FindUntrackedStep("UnitLevelUpStartedPrereqConfig");
            if (untrackedStep != null && untrackedStep._t != string.Empty)
                flag = true;
            return flag;
        }

        public bool HasLevelupCompleteUnit()
        {
            bool flag = false;
            Missions.StartRulePrereq untrackedStep = this.FindUntrackedStep("UnitLevelupCollectedPrereqConfig");
            if (untrackedStep != null && untrackedStep._t != string.Empty)
                flag = true;
            return flag;
        }

        public bool HasCompositionReq()
        {
            bool flag = false;
            MissionStep step = this.FindStep("HasCompositionPrereqConfig");
            if (step != null && step.Prereq != string.Empty)
                flag = true;
            return flag;
        }

        public bool HasCollectJob(string jobName)
        {
            bool flag = false;
            MissionStep jobStep = this.FindJobStep("CollectJobPrereqConfig", jobName);
            if (jobStep != null && jobStep.Prereq != string.Empty)
                flag = true;
            return flag;
        }

        public bool HasCollectProject(string projectName)
        {
            bool flag = false;
            MissionStep projectStep = this.FindProjectStep("CollectProjectPrereqConfig", projectName);
            if (projectStep != null && projectStep.Prereq != string.Empty)
                flag = true;
            return flag;
        }

        public bool HasTurnIn(string item)
        {
            bool flag = false;
            MissionStep resourceStep = this.FindResourceStep("TurnInPrereqConfig", item);
            if (resourceStep != null && resourceStep.Prereq != string.Empty)
                flag = true;
            return flag;
        }

        public bool UpdateDefeatEncounterSet(string encounter)
        {
            return ((IEnumerable<string>)this.GetObjectivePrereq(this.FindEncounterStep("DefeatEncounterSetPrereqConfig", encounter).Seq).encounterIds).Contains<string>(encounter) && this.UpdateEncounterStep("DefeatEncounterSetPrereqConfig", encounter, 1, false);
        }

        public bool UpdateFinishBattle(string encounter)
        {
            return this.GetObjectivePrereq(this.FindEncounterStep("FinishBattlePrereqConfig", encounter).Seq).encounterId == encounter && this.UpdateEncounterStep("FinishBattlePrereqConfig", encounter, 1, false);
        }

        public bool UpdateTaxCollection()
        {
            return this.UpdateStep("CollectTaxesPrereqConfig", 1, false);
        }

        public bool UpdateLandExpansionCollection()
        {
            return this.UpdateStep("HaveLandExpansionsPrereqConfig", Functions.ExpansionCount(), true);
        }

        public bool UpdateMoveBuilding(BuildingEntity _entity)
        {
            return this.GetObjectivePrereq(this.FindStep("MoveBuildingPrereqConfig").Seq).structureType == _entity.Name && this.UpdateStep("MoveBuildingPrereqConfig", 1, false);
        }

        public bool UpdateHasComposition()
        {
            MissionStep step = this.FindStep("HasCompositionPrereqConfig");
            if (step == null)
                return false;
            Missions.StartRulePrereq objData = this.GetObjectivePrereq(step.Seq);
            List<BuildingEntity> list = GameData.Player.WorldMaps["MyLand"].Buildings.Where<BuildingEntity>((Func<BuildingEntity, bool>)(x => x.Name == objData.compositionName && x.State != BuildingState.Offline && (x.State != BuildingState.UnderConstruction1 && x.State != BuildingState.UnderConstruction2) && x.State != BuildingState.RibbonTime && x.State != BuildingState.Placed)).ToList<BuildingEntity>();
            return list != null && list.Count<BuildingEntity>() > 0 && this.UpdateStep("HasCompositionPrereqConfig", list.Count<BuildingEntity>(), true);
        }

        public bool UpdateCollectJob(string jobName)
        {
            return this.GetObjectivePrereq(this.FindJobStep("CollectJobPrereqConfig", jobName).Seq).jobId == jobName && this.UpdateJobStep("CollectJobPrereqConfig", jobName, 1, false);
        }

        public bool UpdateCollectProject(string projectName)
        {
            return this.GetObjectivePrereq(this.FindProjectStep("CollectProjectPrereqConfig", projectName).Seq).projectId == projectName && this.UpdateProjectStep("CollectProjectPrereqConfig", projectName, 1, false);
        }

        public bool UpdateTurnIn(string item, int quantity)
        {
            return this.FindResourceStep("TurnInPrereqConfig", item) != null && this.UpdateResourceStep("TurnInPrereqConfig", item, quantity, false);
        }

        public bool UpdateAttackNPCBuilding(int value)
        {
            this.FindStep("AttackNPCBuildingPrereqConfig");
            return this.UpdateStep("AttackNPCBuildingPrereqConfig", value, true);
        }

        public bool UpdateStep(string name, int value = 1, bool overwrite = false)
        {
            bool flag = false;
            MissionStep step = this.FindStep(name);
            if (step != null && step.Prereq != string.Empty)
            {
                if (overwrite)
                    step.Count = value;
                else
                    step.Count += value;
                if (step.Count >= step.Goal)
                {
                    step.Count = step.Goal;
                    step.Complete = true;
                    flag = true;
                }
                GameData.Player.UpdateMissionProgress(this.MissionId);
            }
            return flag;
        }

        public bool UpdateJobStep(string name, string jobName, int value = 1, bool overwrite = false)
        {
            bool flag = false;
            MissionStep jobStep = this.FindJobStep(name, jobName);
            if (jobStep != null && jobStep.Prereq != string.Empty)
            {
                if (overwrite)
                    jobStep.Count = value;
                else
                    jobStep.Count += value;
                if (jobStep.Count >= jobStep.Goal)
                {
                    jobStep.Count = jobStep.Goal;
                    jobStep.Complete = true;
                    flag = true;
                }
                GameData.Player.UpdateMissionProgress(this.MissionId);
            }
            return flag;
        }

        public bool UpdateProjectStep(string name, string projectName, int value = 1, bool overwrite = false)
        {
            bool flag = false;
            MissionStep projectStep = this.FindProjectStep(name, projectName);
            if (projectStep != null && projectStep.Prereq != string.Empty)
            {
                if (overwrite)
                    projectStep.Count = value;
                else
                    projectStep.Count += value;
                if (projectStep.Count >= projectStep.Goal)
                {
                    projectStep.Count = projectStep.Goal;
                    projectStep.Complete = true;
                    flag = true;
                }
                GameData.Player.UpdateMissionProgress(this.MissionId);
            }
            return flag;
        }

        public bool UpdateEncounterStep(string name, string encounter, int value = 1, bool overwrite = false)
        {
            bool flag = false;
            MissionStep encounterStep = this.FindEncounterStep(name, encounter);
            if (encounterStep != null && encounterStep.Prereq != string.Empty)
            {
                if (overwrite)
                    encounterStep.Count = value;
                else
                    encounterStep.Count += value;
                if (encounterStep.Count >= encounterStep.Goal)
                {
                    encounterStep.Count = encounterStep.Goal;
                    encounterStep.Complete = true;
                    flag = true;
                }
                GameData.Player.UpdateMissionProgress(this.MissionId);
            }
            return flag;
        }

        public bool UpdateResourceStep(string name, string item, int value = 1, bool overwrite = false)
        {
            bool flag = false;
            MissionStep resourceStep = this.FindResourceStep(name, item);
            if (resourceStep != null && resourceStep.Prereq != string.Empty)
            {
                if (overwrite)
                    resourceStep.Count = value;
                else
                    resourceStep.Count += value;
                if (resourceStep.Count >= resourceStep.Goal)
                {
                    resourceStep.Count = resourceStep.Goal;
                    resourceStep.Complete = true;
                    flag = true;
                }
                GameData.Player.UpdateMissionProgress(this.MissionId);
            }
            return flag;
        }
    }
}
