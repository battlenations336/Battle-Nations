using GameCommon.Functions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BNR
{
    public class JobPrereqCheck
    {
        Composition building = null;
        Dictionary<int, JobPrereq> reqs = null;
        string jobName;

        public JobPrereqCheck(string _job)
        {
            reqs = GameData.JobInfo.jobs[_job].prereq;
            jobName = _job;
        }

        public bool IsValid()
        {
            Dictionary<int, JobPrereq> prereqConfigs = GetPrereqConfig();
            bool result = true;

            foreach (JobPrereq prereq in prereqConfigs.Values)
            {
                switch (prereq._t)
                {
                    case "LevelPrereqConfig":
                        if (GetPlayerLevel() < prereq.level)
                            result = false;
                        break;
                    case "ActiveMissionPrereqConfig":
                        bool found = false;
                        foreach (string missionId in prereq.missionIds)
                        {
                            if (GameData.Player.CurrentMissions.ContainsKey(missionId))
                            {
                                found = true;
                            }
                        }
                        if (!found)
                        {
                            if (prereq.missionActive)
                                result = false;                            
                        }
                        else
                        {
                            if (!prereq.missionActive)
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
            }
            return (result);
        }

        public int GetPlayerLevel()
        {
            return (GameData.Player.Level);
        }

        public Dictionary<int, JobPrereq> GetPrereqConfig()
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
