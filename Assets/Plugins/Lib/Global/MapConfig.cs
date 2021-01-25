
namespace BNR
{
    public static class MapConfig
    {
        public static string mapName { get; set; }

        public static string npcId { get; set; }

        public static EncounterArmy encounterComplete { get; set; }

        public static void InitFromNPC(string npcId)
        {
            MapConfig.Reset();
            MapConfig.mapName = GameData.NPCs[npcId].landLayout;
            MapConfig.npcId = npcId;
        }

        public static void InitHome()
        {
            MapConfig.Reset();
            MapConfig.mapName = GameData.Player.HomeMap();
        }

        public static void InitEmpty()
        {
            MapConfig.Reset();
            MapConfig.mapName = "map_empty";
        }

        public static void InitWorldMap()
        {
            MapConfig.Reset();
            MapConfig.mapName = "WORLD_MAP";
            MapConfig.npcId = "WORLD_MAP";
        }

        public static void Reset()
        {
            MapConfig.mapName = string.Empty;
            MapConfig.npcId = string.Empty;
            MapConfig.encounterComplete = (EncounterArmy)null;
        }

        public static NPCs NPC()
        {
            return GameData.NPCs.ContainsKey(MapConfig.npcId) ? GameData.NPCs[MapConfig.npcId] : (NPCs)null;
        }
    }
}
