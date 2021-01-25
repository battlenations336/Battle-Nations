using GameCommon.Functions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BNR
{
    public class AchievementPrereqCheck
    {
        Composition building = null;
        Missions.StartRulePrereq reqs = null;
        string achievementId;

        public AchievementPrereqCheck(string _achievementId)
        {
            reqs = GameData.Achievements[_achievementId].objective.prereq;
            achievementId = _achievementId;
        }

        public bool IsValid()
        {
            Missions.StartRulePrereq prereq = GetPrereqConfig();
            bool result = true;

            switch (prereq._t)
            {
                case "LevelPrereqConfig":
                    if (GetPlayerLevel() < prereq.level)
                        result = false;
                    break;
                case "HasAnyCompositionsPrereqConfig":
                    var buildingB = GameData.Player.WorldMaps[Constants.Outpost].Buildings.Where(x => x.State != GameCommon.BuildingState.Offline && x.State != GameCommon.BuildingState.UnderConstruction1
                    && x.State != GameCommon.BuildingState.UnderConstruction2 && x.State != GameCommon.BuildingState.RibbonTime).ToList();
                    if (buildingB == null || buildingB.Count() < prereq.count)
                    {
                        result = false;
                    }
                    break;
                case "UnitLevelPrereqConfig":
                    //if (GetUnitlevel(GetUnitName()) < prereqConfig.level)
                    //    result = false;
                    break;
                case "CompleteMissionPrereqConfig":
                    //if (!IsMissionCompleted(prereqConfig.missionId))
                    //    result = false;
                    break;
                case "CompleteAnyMissionPrereqConfig":
                    //if (!IsAnyMissionCompleted(prereqConfig.missionIds))
                    //    result = false;
                    break;
                case "HasCompositionPrereqConfig":
                    //if (!IsBuildingActive(prereqConfig.compositionName))
                    //    result = false;
                    break;
                case "EnoughPopulationLimitPrereqConfig":
                    //if (PopulationSpace() < prereqConfig.additionalPopulation)
                    //    result = false;
                    break;
                case "SingleEntityPrereqConfig":
                    //if (!IsSingleEntity(prereqConfig.entityId))
                    //    result = false;
                    break;
                case "MissionStatusPrereqConfig":
                    //if (!IsAnyMissionCompleted(prereqConfig.completeMissionIds))
                    //    result = false;
                    //if (!IsAnyMissionActive(prereqConfig.activeMissionIds))
                    //    result = false;
                    //if (!IsAnyMissionIncomplete(prereqConfig.incompleteMissionIds))
                    //    result = false;
                    break;
                default:
                    result = false;
                    break;
            }

            return (result);
        }

        public int GetPlayerLevel()
        {
            return (GameData.Player.Level);
        }

        public Missions.StartRulePrereq GetPrereqConfig()
        {
            return (reqs);
        }

        public int GetUnitlevel(string unitName)
        {
            throw new NotImplementedException();
        }

        public string GetUnitName()
        {
            throw new NotImplementedException();
        }

        public bool IsAnyMissionActive(string[] missionIds)
        {
            throw new NotImplementedException();
        }

        public bool IsAnyMissionCompleted(string[] missionIds)
        {
            //foreach (string missionId in missionIds)
            //{
            //    if (GameData.Player.CompletedMissions.ContainsKey(missionId))
            //        return (true);
            //}

            //if (configName == "comp_mil_hospital")
            //    if (GameData.Player.CurrentMissions.ContainsKey("p01_NEW2INTRO_144_HaveHospital"))
            //        return (true);

            return (false);
        }

        public bool IsAnyMissionIncomplete(string[] missionIds)
        {
            //throw new NotImplementedException();
            return (false);
        }
    }
}
