
using GameCommon.SerializedObjects;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace BNR
{
    public class GameData
    {
        public static float spriteScale = 1.27f;
        public static Dictionary<string, BNR.AnimationInfo> AnimationInfo = new Dictionary<string, BNR.AnimationInfo>();
        public static Dictionary<string, BNR.Characters> Characters = new Dictionary<string, BNR.Characters>();
        public static Dictionary<string, Composition> Compositions = new Dictionary<string, Composition>();
        public static Dictionary<string, BNR.BattleAbilities> BattleAbilities = new Dictionary<string, BNR.BattleAbilities>();
        public static Dictionary<string, BattleUnit> BattleUnits = new Dictionary<string, BattleUnit>();
        public static BattleEncounter BattleEncounters = new BattleEncounter();
        public static Dictionary<string, BNR.DamageAnimConfig> DamageAnimConfig = new Dictionary<string, BNR.DamageAnimConfig>();
        public static Dictionary<string, BNR.Cutscenes> Cutscenes = new Dictionary<string, BNR.Cutscenes>();
        public static JobInfo JobInfo = new JobInfo();
        public static Dictionary<string, BNR.Levels> Levels = new Dictionary<string, BNR.Levels>();
        public static Dictionary<string, BNR.Missions> Missions = new Dictionary<string, BNR.Missions>();
        public static Dictionary<string, string> TextFile = new Dictionary<string, string>();
        public static List<BNR.AccountSelectionList> AccountSelectionList = new List<BNR.AccountSelectionList>();
        public static Dictionary<string, BNR.NPCs> NPCs = new Dictionary<string, BNR.NPCs>();
        public static Dictionary<string, BNAchievement> Achievements = new Dictionary<string, BNAchievement>();
        public static Player Player = new Player();
        public static bool GameLoaded = false;
        public static BNR.StructureMenu[] StructureMenu;
        public static BNR.ExpandLandCosts[] ExpandLandCosts;

        public static void LoadAccounList()
        {
            string str = string.Empty;
            string path = Path.Combine(Application.persistentDataPath, "Accounts.json");
            GameData.AccountSelectionList = new List<BNR.AccountSelectionList>();
            if (File.Exists(path))
            {
                using (StreamReader streamReader = File.OpenText(path))
                    str = streamReader.ReadToEnd();
            }
            else
                Functions.SaveAccountList();
            if (str == null || !(str != string.Empty))
                return;
            GameData.AccountSelectionList = JsonConvert.DeserializeObject<List<BNR.AccountSelectionList>>(str);
        }

        public static void LoadAccounList2()
        {
            TextAsset textAsset = Resources.Load("Settings/Accounts") as TextAsset;
            if ((UnityEngine.Object)textAsset != (UnityEngine.Object)null && (GameData.AccountSelectionList == null || GameData.AccountSelectionList.Count < 1))
            {
                GameData.AccountSelectionList = JsonConvert.DeserializeObject<List<BNR.AccountSelectionList>>(textAsset.ToString());
            }
            else
            {
                GameData.AccountSelectionList = new List<BNR.AccountSelectionList>();
                if (!((UnityEngine.Object)textAsset == (UnityEngine.Object)null))
                    return;
                Functions.SaveAccountList();
            }
        }

        public static BN_FirstBattleScriptDriver2 LoadBattleScript()
        {
            return JsonConvert.DeserializeObject<BN_FirstBattleScriptDriver2>((Resources.Load("JSON/BN_FirstBattleScriptDriver2") as TextAsset).ToString());
        }

        public static void InitSettings()
        {
            Settings.Volume_Background = 1f;
        }

        public static void LoadGameData()
        {
            GameData.TextFile = JObject.Parse((Resources.Load("JSON/BattleNations_en") as TextAsset).ToString()).ToObject<Dictionary<string, string>>();
            GameData.AnimationInfo = JObject.Parse((Resources.Load("JSON/AnimationInfo") as TextAsset).ToString()).ToObject<Dictionary<string, BNR.AnimationInfo>>();
            GameData.JobInfo = JObject.Parse((Resources.Load("JSON/JobInfo") as TextAsset).ToString()).ToObject<JobInfo>();
            JArray jarray1 = JArray.Parse((Resources.Load("JSON/StructureMenu") as TextAsset).ToString());
            int index1 = 0;
            GameData.StructureMenu = new BNR.StructureMenu[8];
            foreach (JObject jobject in jarray1)
            {
                GameData.StructureMenu[index1] = jobject.ToObject<BNR.StructureMenu>();
                ++index1;
            }
            JArray jarray2 = JArray.Parse((Resources.Load("JSON/ExpandLandCosts") as TextAsset).ToString());
            int index2 = 0;
            GameData.ExpandLandCosts = new BNR.ExpandLandCosts[144];
            foreach (JObject jobject in jarray2)
            {
                GameData.ExpandLandCosts[index2] = jobject.ToObject<BNR.ExpandLandCosts>();
                ++index2;
            }
            GameData.Compositions = JObject.Parse((Resources.Load("JSON/Compositions") as TextAsset).ToString()).ToObject<Dictionary<string, Composition>>();
            GameData.TextFile = JObject.Parse((Resources.Load("JSON/BattleNations_en") as TextAsset).ToString()).ToObject<Dictionary<string, string>>();
            GameData.BattleAbilities = JObject.Parse((Resources.Load("JSON/BattleAbilities") as TextAsset).ToString()).ToObject<Dictionary<string, BNR.BattleAbilities>>();
            GameData.DamageAnimConfig = JObject.Parse((Resources.Load("JSON/DamageAnimConfig") as TextAsset).ToString()).ToObject<Dictionary<string, BNR.DamageAnimConfig>>();
            GameData.BattleUnits = JObject.Parse((Resources.Load("JSON/BattleUnits") as TextAsset).ToString()).ToObject<Dictionary<string, BattleUnit>>();
            GameData.BattleEncounters = JObject.Parse((Resources.Load("JSON/BattleEncounters") as TextAsset).ToString()).ToObject<BattleEncounter>();
            GameData.Levels = JObject.Parse((Resources.Load("JSON/Levels") as TextAsset).ToString()).ToObject<Dictionary<string, BNR.Levels>>();
            GameData.Characters = JObject.Parse((Resources.Load("JSON/Characters") as TextAsset).ToString()).ToObject<Dictionary<string, BNR.Characters>>();
            GameData.Cutscenes = JObject.Parse((Resources.Load("JSON/Cutscenes") as TextAsset).ToString()).ToObject<Dictionary<string, BNR.Cutscenes>>();
            GameData.Missions = JObject.Parse((Resources.Load("JSON/Missions") as TextAsset).ToString()).ToObject<Dictionary<string, BNR.Missions>>();
            GameData.NPCs = JObject.Parse((Resources.Load("JSON/NPCs") as TextAsset).ToString()).ToObject<Dictionary<string, BNR.NPCs>>();
            GameData.Achievements = JObject.Parse((Resources.Load("JSON/BNAchievements") as TextAsset).ToString()).ToObject<Dictionary<string, BNAchievement>>();
            GameData.UpdateGameData();
            AssetCache.InitCache();
        }

        public static void UpdateGameData()
        {
            if (GameData.AnimationInfo.ContainsKey("garden_empty"))
            {
                BNR.AnimationInfo animationInfo = GameData.AnimationInfo["garden_empty"];
                animationInfo.animationName = "LandExpansion_idle";
                GameData.AnimationInfo.Add("LandExpansion_idle", animationInfo);
            }
            if (GameData.Missions.ContainsKey("p01_UPGRADE_010_Farm"))
                GameData.Missions["p01_UPGRADE_010_Farm"].objectives["0"].prereq.level = 2;
            if (GameData.Missions.ContainsKey("p01_UPGRADE_020_ToolShop"))
                GameData.Missions["p01_UPGRADE_020_ToolShop"].objectives["0"].prereq.level = 2;
            if (GameData.Missions.ContainsKey("p01_UPGRADE_030_Hovel"))
                GameData.Missions["p01_UPGRADE_030_Hovel"].objectives["0"].prereq.level = 2;
            if (GameData.Missions.ContainsKey("p01_HEALING_UpgradeHospital"))
                GameData.Missions["p01_HEALING_UpgradeHospital"].objectives["0"].prereq.level = 2;
            if (!GameData.Missions.ContainsKey("p01_NEWINTRO_SQ1_020_GrenadierFight"))
                return;
            GameData.Missions["p01_NEWINTRO_SQ1_020_GrenadierFight"].objectives["0"].prereq._t = "EnterOpponentLandPrereqConfig";
            GameData.Missions["p01_NEWINTRO_SQ1_020_GrenadierFight"].objectives["0"].prereq.opponentId = "WORLD_MAP";
            GameData.Missions["p01_NEWINTRO_SQ1_020_GrenadierFight"].objectives["0"].icon = "mapicon";
            BNR.Missions.Objective objective = new BNR.Missions.Objective()
            {
                prereq = new BNR.Missions.StartRulePrereq()
            };
            objective.prereq._t = "DefeatEncounterPrereqConfig";
            objective.prereq.encounterId = "mis_p01_NEWINTRO_SQ1_010_TrainGrenadier_enc1";
            objective.prereq.objectiveText = "p01_newintro_sq1_020_grenadierfight_obj_1";
            objective.steps = new BNR.Missions.ObjectiveStep[0];
            GameData.Missions["p01_NEWINTRO_SQ1_020_GrenadierFight"].objectives.Add("1", objective);
            GameData.Missions["p01_NEWINTRO_SQ1_020_GrenadierFight"].objectives["1"].icon = "Grenadier_icon";
        }

        public static MissionScript[] LoadScript(string _name)
        {
            MissionScript[] missionScriptArray = (MissionScript[])null;
            if (_name != null && _name != string.Empty)
            {
                if (!_name.StartsWith("mis_"))
                    _name = string.Format("mis_{0}", (object)_name);
                TextAsset textAsset = Resources.Load("JSON/Missions/" + _name) as TextAsset;
                if ((UnityEngine.Object)textAsset != (UnityEngine.Object)null)
                    missionScriptArray = JArray.Parse(textAsset.ToString()).ToObject<MissionScript[]>();
            }
            return missionScriptArray;
        }

        public static LevelDialog[] LoadLevelDialog(int level)
        {
            LevelDialog[] levelDialogArray = (LevelDialog[])null;
            TextAsset textAsset = Resources.Load(string.Format("JSON/LevelUp/LevelUp_{0}", (object)level)) as TextAsset;
            if ((UnityEngine.Object)textAsset != (UnityEngine.Object)null)
                levelDialogArray = JArray.Parse(textAsset.ToString()).ToObject<LevelDialog[]>();
            return levelDialogArray;
        }

        public static void InitPlayer(string name, Profile profile)
        {
            GameData.Player.LoadFromProfile(profile);
            GameData.GameLoaded = true;
        }

        public static Job GetJobinfo(string _config)
        {
            Job job = new Job();
            if (GameData.JobInfo.jobs.ContainsKey(_config))
                job = GameData.JobInfo.jobs[_config];
            return job;
        }

        public static bool IsUnitPremium(string _unitName)
        {
            bool flag = false;
            if (GameData.BattleUnits.ContainsKey(_unitName) && GameData.BattleUnits[_unitName].cost.currency != 0)
                flag = true;
            return flag;
        }

        public static bool IsBuildingPremium(string _config)
        {
            bool flag = false;
            if (GameData.Compositions[_config].componentConfigs.Expansion != null)
            {
                if (_config == "LandExpandFast")
                    flag = true;
            }
            else if (GameData.Compositions[_config].componentConfigs.StructureMenu.cost.currency != 0)
                flag = true;
            return flag;
        }

        public static string BMCatToString(Category _category)
        {
            string str = string.Empty;
            switch (_category)
            {
                case Category.bmCat_houses:
                    str = "Houses";
                    break;
                case Category.bmCat_shops:
                    str = "Shops";
                    break;
                case Category.bmCat_military:
                    str = "Military";
                    break;
                case Category.bmCat_boosts:
                    str = "Boosts";
                    break;
                case Category.bmCat_resources:
                    str = "Resources";
                    break;
                case Category.bmCat_store:
                    str = "Nanopods";
                    break;
                case Category.bmCat_decorations:
                    str = "Decorations";
                    break;
                case Category.bmCat_expansion:
                    str = "Expansions";
                    break;
            }
            return str;
        }

        public static string BMCatToBackground(Category _category)
        {
            string str = string.Empty;
            switch (_category)
            {
                case Category.bmCat_houses:
                    str = "UI/housingBackground@2x";
                    break;
                case Category.bmCat_shops:
                    str = "UI/goodsBackground@2x";
                    break;
                case Category.bmCat_military:
                    str = "UI/militaryBackground@2x";
                    break;
                case Category.bmCat_boosts:
                    str = "UI/boostsBackground@2x";
                    break;
                case Category.bmCat_resources:
                    str = "UI/resourcesBackground@2x";
                    break;
                case Category.bmCat_store:
                    str = "UI/nanopodBackground@2x";
                    break;
                case Category.bmCat_decorations:
                    str = "UI/decorationBackground@2x";
                    break;
                case Category.bmCat_expansion:
                    str = "UI/expansionBackground@2x";
                    break;
            }
            return str;
        }

        public static string GetText(string textId)
        {
            string str = textId;
            if (textId != null && textId != string.Empty)
            {
                if (GameData.TextFile.ContainsKey(textId))
                    str = GameData.TextFile[textId];
                else if (GameData.TextFile.ContainsKey(textId.ToLower()))
                    str = GameData.TextFile[textId.ToLower()];
            }
            return str;
        }

        public static string NormaliseIconName(string _iconName)
        {
            if (_iconName == null || _iconName == string.Empty)
                return string.Empty;
            string str = Path.GetFileNameWithoutExtension(_iconName);
            if (!str.EndsWith("@2x"))
                str = string.Format("{0}@2x", (object)str);
            return str;
        }

        public static Sprite GetIcon(string iconName)
        {
            Sprite sprite = GameData.GetSprite("Icons/" + Path.GetFileNameWithoutExtension(iconName));
            if ((UnityEngine.Object)sprite == (UnityEngine.Object)null)
                sprite = GameData.GetSprite("UI/" + Path.GetFileNameWithoutExtension(iconName));
            return sprite;
        }

        public static Sprite GetSprite(string spriteName)
        {
            string path = spriteName;
            Sprite sprite = Resources.Load<Sprite>(path);
            if ((UnityEngine.Object)sprite == (UnityEngine.Object)null && !path.EndsWith("@2x"))
            {
                path = string.Format("{0}@2x", (object)path);
                sprite = Resources.Load<Sprite>(path);
            }
            if ((UnityEngine.Object)sprite == (UnityEngine.Object)null)
                Debug.Log((object)string.Format("Sprite missing - {0}", (object)path));
            return sprite;
        }

        public static string GetTitle(string _title)
        {
            string empty = string.Empty;
            if (GameData.TextFile.ContainsKey(_title))
                empty = GameData.TextFile[_title];
            return empty;
        }

        public static BNR.Characters GetCharacter(string _name)
        {
            BNR.Characters characters = new BNR.Characters();
            if (GameData.Characters.ContainsKey(_name))
                characters = GameData.Characters[_name];
            return characters;
        }

        public static bool IsBuildingLocked(string _config)
        {
            bool flag = false;
            if (!GameData.Compositions.ContainsKey(_config))
                flag = true;
            else if (GameData.Compositions[_config].componentConfigs.StructureMenu.prereq != null && !new BuildingPrereqCheck(_config).IsValid())
                flag = true;
            return flag;
        }

        public static int LevelRequirement_Config(string _config)
        {
            int num = 0;
            if (GameData.Compositions.ContainsKey(_config) && GameData.Compositions[_config].componentConfigs.StructureMenu.prereq != null)
            {
                foreach (PrereqConfig prereqConfig in GameData.Compositions[_config].componentConfigs.StructureMenu.prereq.Values)
                {
                    if (prereqConfig._t == "LevelPrereqConfig")
                        num = Convert.ToInt32(prereqConfig.level);
                }
            }
            return num;
        }

        public static int LevelRequirement_Unit(string _unit)
        {
            int num = 0;
            if (GameData.BattleUnits.ContainsKey(_unit))
            {
                foreach (PrereqConfig prereqConfig in GameData.BattleUnits[_unit].prereq.Values)
                {
                    if (prereqConfig._t == "LevelPrereqConfig")
                        num = Convert.ToInt32(prereqConfig.level);
                }
            }
            return num;
        }

        public static bool IsUnitLocked(string name)
        {
            bool flag = false;
            if (!GameData.BattleUnits.ContainsKey(name))
                flag = true;
            else if (GameData.BattleUnits[name].prereq != null && !new UnitPrereqCheck(name).IsValid())
                flag = true;
            return flag;
        }

        public static List<EntityListEntry> GetUnlockedList(int level)
        {
            List<EntityListEntry> entityListEntryList = new List<EntityListEntry>();
            if (level < 0 || level > 70)
                return (List<EntityListEntry>)null;
            foreach (string key1 in GameData.BattleUnits.Keys)
            {
                BattleUnit battleUnit = GameData.BattleUnits[key1];
                if (battleUnit.prereq != null)
                {
                    foreach (string key2 in battleUnit.prereq.Keys)
                    {
                        if (battleUnit.prereq[key2].level == (double)level)
                            entityListEntryList.Add(new EntityListEntry(EntityType.Unit, key1));
                    }
                }
            }
            foreach (string key1 in GameData.Compositions.Keys)
            {
                Composition composition = GameData.Compositions[key1];
                if (composition.componentConfigs != null && composition.componentConfigs.StructureMenu != null && composition.componentConfigs.StructureMenu.prereq != null)
                {
                    foreach (string key2 in composition.componentConfigs.StructureMenu.prereq.Keys)
                    {
                        if (composition.componentConfigs.StructureMenu.prereq[key2].level == (double)level && composition.componentConfigs.StructureMenu.icon != null && composition.componentConfigs.StructureMenu.icon != string.Empty)
                            entityListEntryList.Add(new EntityListEntry(EntityType.Building, key1));
                    }
                }
            }
            return entityListEntryList;
        }
    }
}
