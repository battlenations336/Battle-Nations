
using GameCommon;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BNR
{
    public class MapData
    {
        public List<BuildingEntity> Buildings;
        public List<Occupation> Occupations;
        public List<EncounterArmy> Encounters;
        public List<int> Expansions;
        public DateTime NextSpawnCheck;
        public DateTime LastAssist;

        public MapData()
        {
            this.Buildings = new List<BuildingEntity>();
            this.Occupations = new List<Occupation>();
            this.Encounters = new List<EncounterArmy>();
            this.Expansions = new List<int>();
        }

        public bool ContainsDepot()
        {
            bool flag = false;
            IEnumerable<BuildingEntity> source = this.Buildings.Where<BuildingEntity>((Func<BuildingEntity, bool>)(x =>
            {
                if (!x.Name.StartsWith("comp_res_resourcedepot"))
                    return false;
                return x.State == BuildingState.Inactive || x.State == BuildingState.Full;
            }));
            if (source != null && source.Count<BuildingEntity>() > 0)
                flag = true;
            return flag;
        }

        public void RemoveMissionEncounters(string missionId)
        {
            IEnumerable<EncounterArmy> encounterArmies = this.Encounters.Where<EncounterArmy>((Func<EncounterArmy, bool>)(x => x.EventId == missionId));
            if (encounterArmies == null)
                return;
            foreach (EncounterArmy encounterArmy in encounterArmies)
                this.Encounters.Remove(encounterArmy);
        }

        public int FirstFreeEncounterInstance()
        {
            int result = 1;
            for (EncounterArmy encounterArmy = this.Encounters.Where<EncounterArmy>((Func<EncounterArmy, bool>)(x => x.InstanceId == result)).FirstOrDefault<EncounterArmy>(); encounterArmy != null && encounterArmy.InstanceId == result; encounterArmy = this.Encounters.Where<EncounterArmy>((Func<EncounterArmy, bool>)(x => x.InstanceId == result)).FirstOrDefault<EncounterArmy>())
                result++;
            return result;
        }
    }
}
