using GameCommon.Functions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BNR
{
    public class NPCPrereqCheck
    {
        Composition building = null;
        Dictionary<string, PrereqConfig> reqs = null;
        string npcId;

        public NPCPrereqCheck(string _NPC)
        {
            reqs = GameData.NPCs[_NPC].spawnPrereqs;
            npcId = _NPC;
        }

        public bool IsValid()
        {
            Dictionary<string, PrereqConfig> prereqConfigs = GetPrereqConfig();
            bool result = true;

            foreach (PrereqConfig prereq in prereqConfigs.Values)
            {
                switch (prereq._t)
                {
                    case "LevelPrereqConfig":
                        if (GetPlayerLevel() < prereq.level)
                            result = false;
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
                        if (!IsAnyMissionCompleted(prereq.completeMissionIds))
                            result = false;
                        if (!IsAnyMissionActive(prereq.activeMissionIds))
                            result = false;
                        if (!AreMissionsIncomplete(prereq.incompleteMissionIds))
                            result = false;
                        break;
                    default:
                        result = false;
                        break;
                }
            }
            return (result);
        }

        public int GetPlayerLevel()
        {
            return (GameData.Player.Level);
        }

        public Dictionary<string, PrereqConfig> GetPrereqConfig()
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
            if (missionIds != null)
                foreach (string missionId in missionIds)
                {
                    if (GameData.Player.CurrentMissions.ContainsKey(missionId))
                        return (true);
                }

            return (false);
        }

        public bool IsAnyMissionCompleted(string[] missionIds)
        {
            if (missionIds != null)
                foreach (string missionId in missionIds)
                {
                    if (GameData.Player.CompletedMissions.ContainsKey(missionId))
                        return (true);
                }

            //if (configName == "comp_mil_hospital")
            //    if (GameData.Player.CurrentMissions.ContainsKey("p01_NEW2INTRO_144_HaveHospital"))
            //        return (true);

            return (false);
        }

        public bool AreMissionsIncomplete(string[] missionIds)
        {
            if (missionIds != null)
                foreach (string missionId in missionIds)
                {
                    if (GameData.Player.CompletedMissions.ContainsKey(missionId))
                        return (false);
                }

            return (true);
        }
    }
}
