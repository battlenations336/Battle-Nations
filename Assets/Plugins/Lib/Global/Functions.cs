using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using UnityEngine;

namespace BNR
{
    public class Functions
    {
        public static Vector3 CalculateCoord(int _x, int _y)
        {
            float x = _x;
            float y = _y;

            float dmapx = (x - y) * 60;
            float dmapy = (x + y) * 30;
            float xoff = 30 * (0) + 60;
            float yoff = 15 * (96 + 96);

            dmapx = xoff - 20 * (x - y);
            dmapy = yoff - 10 * (x + y);
            dmapy = 10 * (x + y);

            Vector3 accuPoint = new Vector3((int)(dmapx) - 3000, (int)((dmapy)), 0);
            accuPoint = new Vector3(dmapx, dmapy, 0);

            Vector3 newPos = new Vector3(accuPoint.x / 100f, accuPoint.y / 100f, 0);

            return (newPos);
        }

        public static Color GetColor(float R, float G, float B)
        {
            return new Color(R / 255, G / 255, B / 255);
        }

        public static Color GetColor(float R, float G, float B, float A)
        {
            return new Color(R / 255f, G / 255f, B / 255f, A / 255f);
        }

        public static string GetTaskTime(int totalSec, float completedSec)
        {
            string result = string.Empty;
            int hours = 0, mins = 0, secs = 0;
            float timeLeft = totalSec - completedSec;

            if (timeLeft > 0)
            {
                hours = (int)(timeLeft / 3600);
                mins = (int)((timeLeft % 3600) / 60);
                secs = (int)(timeLeft - (hours * 3600) - (mins * 60));
            }
            return (string.Format("{0}h {1}m {2}s", hours, mins, secs));
        }

        public static string GetShortTaskTime(int totalSec, float completedSec)
        {
            string result = string.Empty;
            int hours = 0, mins = 0, secs = 0;
            float timeLeft = totalSec - completedSec;

            if (timeLeft > 0)
            {
                hours = (int)(timeLeft / 3600);
                mins = (int)((timeLeft % 3600) / 60);
                secs = (int)(timeLeft - (hours * 3600) - (mins * 60));

                if (secs > 0)
                    result = string.Format("{0}s", secs);

                if (mins > 0)
                    result = string.Format("{0}m {1}", mins, result);

                if (hours > 0 || string.IsNullOrEmpty(result))
                    result = string.Format("{0}h {1}", hours, result);
            }
            return (result.TrimEnd(' '));
        }

        public static int GetHours(float totalSec, float completedSec)
        {
            int result = 1;
            float hours = 0;
            float timeLeft = totalSec - completedSec;

            if (timeLeft > 0)
            {
                hours = (timeLeft / 3600) + 1;

                result = (int)hours;
            }

            return (result);
        }

        public static string ResourceToSpriteName(string resource)
        {
            if (resource == "pvp_energy")
                return ("UI/resource_pvp_energy@2x");

            return (Functions.ResourceToSpriteName(ResourceNameToEnum(resource)));
        }

        public static string ResourceToSpriteName(Resource resource)
        {
            string result = "Icons/BarSinister@2x";

            switch (resource)
            {
                case Resource.bars:
                    result = "UI/resource_bars@2x";
                    break;
                case Resource.chem:
                    result = "UI/resource_chem@2x";
                    break;
                case Resource.coal:
                    result = "UI/resource_coal@2x";
                    break;
                case Resource.concrete:
                    result = "UI/resource_concrete@2x";
                    break;
                case Resource.gear:
                    result = "UI/resource_gear@2x";
                    break;
                case Resource.heart:
                    result = "UI/resource_heart@2x";
                    break;
                case Resource.iron:
                    result = "UI/resource_iron@2x";
                    break;
                case Resource.lumber:
                    result = "UI/resource_lumber@2x";
                    break;
                case Resource.oil:
                    result = "UI/resource_oil@2x";
                    break;
                case Resource.sbars:
                    result = "UI/resource_sbars@2x";
                    break;
                case Resource.sgear:
                    result = "UI/resource_sgear@2x";
                    break;
                case Resource.skull:
                    result = "UI/resource_skull@2x";
                    break;
                case Resource.sskull:
                    result = "UI/resource_sskull@2x";
                    break;
                case Resource.star:
                    result = "UI/resource_star@2x";
                    break;
                case Resource.steel:
                    result = "UI/resource_steel@2x";
                    break;
                case Resource.stone:
                    result = "UI/resource_stone@2x";
                    break;
                case Resource.stooth:
                    result = "UI/resource_stooth@2x";
                    break;
                case Resource.tooth:
                    result = "UI/resource_tooth@2x";
                    break;
                case Resource.wood:
                    result = "UI/resource_wood@2x";
                    break;

            }

            return (result);
        }

        public static string ResourceEnumToName(Resource _resource)
        {
            string result = string.Empty;

            switch (_resource)
            {
                case Resource.bars:
                    result = "bars";
                    break;
                case Resource.chem:
                    result = "chem";
                    break;
                case Resource.coal:
                    result = "coal";
                    break;
                case Resource.concrete:
                    result = "concrete";
                    break;
                case Resource.gear:
                    result = "gear";
                    break;
                case Resource.heart:
                    result = "heart";
                    break;
                case Resource.iron:
                    result = "iron";
                    break;
                case Resource.lumber:
                    result = "lumber";
                    break;
                case Resource.oil:
                    result = "oil";
                    break;
                case Resource.sbars:
                    result = "sbars";
                    break;
                case Resource.sgear:
                    result = "sgear";
                    break;
                case Resource.skull:
                    result = "skull";
                    break;
                case Resource.sskull:
                    result = "sskull";
                    break;
                case Resource.star:
                    result = "star";
                    break;
                case Resource.steel:
                    result = "steel";
                    break;
                case Resource.stone:
                    result = "stone";
                    break;
                case Resource.stooth:
                    result = "stooth";
                    break;
                case Resource.tooth:
                    result = "tooth";
                    break;
                case Resource.wood:
                    result = "wood";
                    break;
            }
            return (result);
        }

        public static Resource ResourceNameToEnum(string _name)
        {
            Resource result = Resource.wood;

            switch (_name)
            {
                case "bars":
                    result = Resource.bars;
                    break;
                case "chem":
                    result = Resource.chem;
                    break;
                case "coal":
                    result = Resource.coal;
                    break;
                case "concrete":
                    result = Resource.concrete;
                    break;
                case "gear":
                    result = Resource.gear;
                    break;
                case "heart":
                    result = Resource.heart;
                    break;
                case "iron":
                    result = Resource.iron;
                    break;
                case "lumber":
                    result = Resource.lumber;
                    break;
                case "oil":
                    result = Resource.oil;
                    break;
                case "sbars":
                    result = Resource.sbars;
                    break;
                case "sgear":
                    result = Resource.sgear;
                    break;
                case "skull":
                    result = Resource.skull;
                    break;
                case "sskull":
                    result = Resource.sskull;
                    break;
                case "star":
                    result = Resource.star;
                    break;
                case "steel":
                    result = Resource.steel;
                    break;
                case "stone":
                    result = Resource.stone;
                    break;
                case "stooth":
                    result = Resource.stooth;
                    break;
                case "tooth":
                    result = Resource.tooth;
                    break;
                case "wood":
                    result = Resource.wood;
                    break;
            }

            return (result);
        }

        public static string[] ResourceNames()
        {
            string[] names = new string[] { "bars", "chem", "coal", "concrete",
                            "gear", "heart", "iron", "lumber", "oil",
                            "sbars", "sgear", "skull", "sskull",
                            "star", "steel", "stone", "stooth",
                            "tooth", "wood"};

            return (names);
        }

        public static object GetPropertyValue(object instance, string strPropertyName)
        {
            Type type = instance.GetType();
            System.Reflection.PropertyInfo propertyInfo = type.GetProperty(strPropertyName);
            if (propertyInfo == null)
                return (null);
            return propertyInfo.GetValue(instance, null);
        }

        public static Tuple<string, int> GetTollValue(object instance)
        {
            string item = string.Empty;
            int quantity = 0;

            foreach (string resource in Functions.ResourceNames())
            {
                int value = 0;
                value = (int)Functions.GetPropertyValue(instance, resource);
                if (value > 0)
                {
                    item = resource;
                    quantity = value;
                }
            }

            return (Tuple.Create(item, quantity));
        }

        public static void SaveAccountList()
        {
            string datapath = Path.Combine(Application.persistentDataPath, "Accounts.json");
            string json = JsonConvert.SerializeObject(GameData.AccountSelectionList);
            using (StreamWriter streamWriter = File.CreateText(datapath))
            {
                streamWriter.Write(json);
            }
        }

        public static int ExpansionCount()
        {
            int result = 0;

            result = GameData.Player.WorldMaps[Constants.Outpost].Expansions.Count - 4;

            if (ExpansionInProgress())
            {
                result++;
            }

            return (result);
        }

        public static bool ExpansionInProgress()
        {
            bool result = false;

            var inProgress = GameData.Player.WorldMaps[Constants.Outpost].Buildings.Where(x => x.Name.StartsWith("LandExpand")).FirstOrDefault();
            if (inProgress != null)
            {
                result = true;
            }

            return (result);
        }

        public static bool ExpansionAllowed()
        {
            bool result = false;
            int expansionCount = ExpansionCount();

            if (GameData.ExpandLandCosts.Length > expansionCount && GameData.ExpandLandCosts[expansionCount + 1].prereq["1"].level <= GameData.Player.Level)
                result = true;

            return (result);
        }

        public static bool UpgradeInProgress()
        {
            bool result = false;

            var inProgress = GameData.Player.WorldMaps[Constants.Outpost].Buildings.Where(x => x.State == GameCommon.BuildingState.Upgrading || 
            x.State == GameCommon.BuildingState.UpgradeComplete).FirstOrDefault();
            if (inProgress != null)
            {
                result = true;
            }

            return (result);
        }

        public static bool IsStorageResource(string outputType)
        {
            bool result = false;

            switch (outputType)
            {
                case "resource_stone":
                case "resource_wood":
                case "resource_coal":
                case "resource_oil":
                case "resource_iron":
                    result = true;
                    break;
            }
            return (result);
        }

        public static Resource OutputTypeToEnum(string outputType)
        {
            Resource result = Resource.stone;

            if (outputType.Contains("_"))
                outputType = outputType.Substring(outputType.IndexOf("_") + 1);
            switch(outputType)
            {
                case "stone":
                    result = Resource.stone;
                    break;
                case "wood":
                    result = Resource.wood;
                    break;
                case "coal":
                    result = Resource.coal;
                    break;
                case "oil":
                    result = Resource.oil;
                    break;
                case "iron":
                    result = Resource.iron;
                    break;
            }

            return (result);
        }
    }
}