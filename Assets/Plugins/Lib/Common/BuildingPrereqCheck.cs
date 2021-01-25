using GameCommon.Functions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BNR
{
    public class BuildingPrereqCheck : CheckPrereq
    {
        Composition building = null;
        Dictionary<string, PrereqConfig> reqs = null;
        string configName;

        public BuildingPrereqCheck(string _config)
        {
            reqs = GameData.Compositions[_config].componentConfigs.StructureMenu.prereq;
            configName = _config;
        }

        public override int GetPlayerLevel()
        {
            return (GameData.Player.Level);
        }

        public override Dictionary<string, PrereqConfig> GetPrereqConfig()
        {
            return (reqs);
        }

        public override int GetUnitlevel(string unitName)
        {
            throw new NotImplementedException();
        }

        public override string GetUnitName()
        {
            throw new NotImplementedException();
        }

        public override bool IsAnyMissionActive(string[] missionIds)
        {
            throw new NotImplementedException();
        }

        public override bool IsAnyMissionCompleted(string[] missionIds)
        {
            foreach (string missionId in missionIds)
            {                
                if (GameData.Player.CompletedMissions.ContainsKey(missionId))
                    return (true);                
            }

            if (configName == "comp_mil_hospital")
                if (GameData.Player.CurrentMissions.ContainsKey("p01_NEW2INTRO_144_HaveHospital"))
                    return (true);

            return (false);
        }

        public override bool IsAnyMissionIncomplete(string[] missionIds)
        {
            throw new NotImplementedException();
        }

        public override bool IsBuildingActive(string buildingType)
        {
            bool result = true;

            if (GameData.Player.WorldMaps[Constants.Outpost].Buildings == null)
                return (false);

            var building = GameData.Player.WorldMaps[Constants.Outpost].Buildings.Where(x => x.Name == buildingType).FirstOrDefault();
            if (building == null || building.Name != buildingType || building.Available() == false)
                result = false;

            return (result);
        }

        public override bool IsMissionCompleted(string missionId)
        {
            if (GameData.Player.CompletedMissions.ContainsKey(missionId))
                return (true);

            return (false);
        }

        public override bool IsSingleEntity(string buildingType)
        {
            bool result = true;

            if (GameData.Player.WorldMaps[Constants.Outpost].Buildings == null)
                return (true);

            var building = GameData.Player.WorldMaps[Constants.Outpost].Buildings.Where(x => x.Name == buildingType).FirstOrDefault();
            if (building != null && building.Name == buildingType)
                result = false;

            return (result);
        }

        public override int PopulationSurplus()
        {
            return (GameData.Player.Population - GameData.Player.PopulationActive);
        }

        public override int PopulationSpace()
        {
            return (GameData.Player.PopulationLimit - GameData.Player.Population);
        }
    }
}
