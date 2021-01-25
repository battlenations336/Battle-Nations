
namespace BNR
{
    public static class BattleConfig
    {
        public static BattleType Type { get; set; }

        public static string MapName { get; set; }

        public static string OccupationId { get; set; }

        public static string TestId { get; set; }

        public static BattleMapType MapLayout { get; set; }

        public static int MaxOffence { get; set; }

        public static int MaxDefence { get; set; }

        public static bool ForceStart { get; set; }

        public static string EncounterId { get; set; }

        public static EncounterArmy Encounter { get; set; }

        public static int AwardMoney { get; set; }

        public static void InitFromEncounter(EncounterArmy _encounter)
        {
            BattleConfig.InitFromEncounter(_encounter.Name);
            BattleConfig.Encounter = _encounter;
        }

        public static void InitFromEncounter(string encounterId)
        {
            BattleConfig.Reset();
            BattleConfig.Type = BattleType.Encounter;
            if (!(encounterId != string.Empty) || !GameData.BattleEncounters.armies.ContainsKey(encounterId))
                return;
            BattleConfig.Type = BattleType.Encounter;
            BattleConfig.EncounterId = encounterId;
            BattleEncounterArmy army = GameData.BattleEncounters.armies[encounterId];
            string layoutId = army.layoutId;
            if (!(layoutId == "equal_5x3"))
            {
                if (layoutId == "equal_3x3")
                    BattleConfig.MapLayout = BattleMapType.ThreeByThree;
            }
            else
                BattleConfig.MapLayout = BattleMapType.ThreeByFive;
            BattleConfig.MapName = "BattleMap";
            BattleConfig.MaxDefence = army.attackerDefenseSlots;
            BattleConfig.MaxOffence = army.attackerSlots;
        }

        public static bool IsPvP()
        {
            return BattleConfig.Type == BattleType.VsFriend || BattleConfig.Type == BattleType.VsRandom;
        }

        public static void Reset()
        {
            BattleConfig.MapName = string.Empty;
            BattleConfig.Type = BattleType.Encounter;
            BattleConfig.OccupationId = string.Empty;
            BattleConfig.TestId = string.Empty;
            BattleConfig.MapLayout = BattleMapType.ThreeByFive;
            BattleConfig.MaxOffence = 7;
            BattleConfig.MaxDefence = 0;
            BattleConfig.ForceStart = false;
            BattleConfig.EncounterId = string.Empty;
            BattleConfig.Encounter = (EncounterArmy)null;
            BattleConfig.AwardMoney = 0;
        }
    }
}
