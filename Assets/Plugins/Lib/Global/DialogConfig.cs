using System.Collections.Generic;
using System.Linq;

namespace BNR
{
    public static class DialogConfig
    {
        public class ScriptList
        {
            public string MissionId { get; set; }
            public string NextScript { get; set; }
        }

        private static List<ScriptList> scriptList = new List<ScriptList>();

        private static string MissionId { get; set; }

        public static bool LevelledUp { get; set; }

        public static void AddScript(string _missionId, string _scriptId)
        {
            ScriptList newScript = new ScriptList();

            newScript.MissionId = _missionId;
            newScript.NextScript = _scriptId;

            scriptList.Add(newScript);
        }

        public static void RemoveTopScript()
        {
            if (scriptList != null || scriptList.Count > 0)
            {
                scriptList.RemoveAt(0);
            }
        }

        public static string GetNextScript()
        {
            string result = null;

            if (scriptList != null || scriptList.Count > 0)
            {
                result = scriptList.First().NextScript;
            }

            return (result);
        }

        public static string GetNextMissionId()
        {
            string result = null;

            if (scriptList != null || scriptList.Count > 0)
            {
                result = scriptList.First().MissionId;
            }

            return (result);
        }

        public static void Clear()
        {
            scriptList = new List<ScriptList>();
            MissionId = string.Empty;
            LevelledUp = false;
        }

        public static bool IsScriptWaiting()
        {
            bool waiting = false;

            if (scriptList != null && scriptList.Count > 0)
                waiting = true;

            return (waiting);
        }

        public static bool IsScriptWaiting(string missionId)
        {
            bool waiting = false;

            var srchScript = scriptList.Where(x => x.MissionId == missionId).FirstOrDefault();

            if (srchScript != null)
                waiting = true;

            return (waiting);
        }

    }
}
