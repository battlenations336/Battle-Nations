using GameCommon.Functions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BNR
{
    public class UnitPrereqCheck : CheckPrereq
    {
        ArmyUnit armyUnit = null;
        Dictionary<string, PrereqConfig> reqs = null;
        string unitName;

        public UnitPrereqCheck(string name)
        {
            if (GameData.Player.Army.ContainsKey(name))
                armyUnit = GameData.Player.Army[name];
            reqs = GameData.BattleUnits[name].prereq;
            unitName = name;
        }

        public override int GetPlayerLevel()
        {
            return(GameData.Player.Level);
        }

        public override Dictionary<string, PrereqConfig> GetPrereqConfig()
        {
            return(reqs);
        }

        public override int GetUnitlevel(string unitName)
        {
            if (armyUnit != null)
                return armyUnit.level;

            return (0);
        }

        public override string GetUnitName()
        {
            return (unitName);
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
            throw new NotImplementedException();
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
