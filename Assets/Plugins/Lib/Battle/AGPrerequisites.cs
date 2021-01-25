using System.Collections.Generic;

namespace BNR
{
    public class AGPrerequisites
    {
        public static AGPrerequisites create(Dictionary<string, PrereqConfig> json)
        {
            return (json == null || json.Count == 0) ? null
                    : new AGPrerequisites(json);
        }

        private int minLevel, minRank;

        protected AGPrerequisites(Dictionary<string, PrereqConfig> json)
        {
            foreach (PrereqConfig prereq in json.Values)
            {
                switch (prereq._t)
                {
                    case "LevelPrereqConfig":
                        minLevel = (int)prereq.level;
                        break;
                    case "UnitLevelPrereqConfig":
                        minRank = (int)prereq.level;
                        break;
                }
            }
        }

        public int getMinLevel()
        {
            return minLevel;
        }

        public int getMinRank()
        {
            return minRank;
        }
    }
}
