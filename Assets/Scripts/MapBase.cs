
using Assets.ClientViews;
using BNR;
using ExitGames.Client.Photon;
using GameCommon;
using GameCommon.SerializedObjects;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MapBase : MonoBehaviour, IPointerUpHandler, IEventSystemHandler, IPointerDownHandler, IPointerClickHandler
{
    private static int xmin = 10000;
    private static int xmax = -10000;
    private static int ymin = 10000;
    private static int ymax = -10000;
    internal int fingerId = -1;
    private List<string> missingAnimationsDebug = new List<string>();
    public WorldView WorldView;
    public Camera MainCamera;
    public SpriteRenderer Map;
    public GameObject TopObject;
    public Canvas UICanvas;
    public PlayerBadgeController pBadgeController;
    public GameObject BottomPanel;
    public GameObject ResourcePanel;
    public GameObject IconPanel;
    public GameObject DialogPanel;
    public GameObject UnitPanel;
    public UnitInformationCtrl UnitInformationCtrl;
    public UnitLevelUpCtrl UnitLevelUpCtrl;
    public MissionDialogCtrl MissionDialogCtrl;
    public MissionListCtril missionListCtrl;
    public MissionDetailDialogCtrl missionDetailDialogCtrl;
    public MissionCompleteCtrl missionCompleteCtrl;
    internal TileGrid<GridSpace> tileGrid;
    internal LandLayout mapFile;
    internal GameMap landFile;
    internal WorldMode mode;
    internal Vector3 Difference;
    internal bool Drag;
    internal Vector3 Origin;
    internal GameObject building_prefab;
    private MissionScriptEngine missionScriptEngine;
    private List<MissionScript> scriptQueue;
    private string newMission;
    protected bool WaitingForDialog;
    private float dialogDelay;
    private bool criticalRegion;
    private bool updateRequired;

    public string MapName()
    {
        return MapConfig.mapName == "LandLayout_3" || MapConfig.mapName == "MyLand" ? "MyLand" : MapConfig.mapName;
    }

    public string NPCId()
    {
        return MapConfig.mapName == "LandLayout_3" || MapConfig.mapName == "MyLand" ? "MyLand" : MapConfig.npcId;
    }

    private void Awake()
    {
    }

    public void Start()
    {
        this.MainCamera = Camera.main;
        this.building_prefab = (GameObject)UnityEngine.Resources.Load("BuildingSprite");
        Debug.Log((object)string.Format("Opening map - {0}", (object)MapConfig.mapName));
        this.tileGrid = new TileGrid<GridSpace>(144, 144);
        this.Init();
        GameData.Player.OnPlayerDataUpdated += new EventHandler(this.SaveGame);
        this.UpdateMissionStatus(string.Empty);
    }

    public void DrawMap()
    {
        if (GameData.Player.WorldMaps[this.MapName()].Buildings == null)
            return;
        foreach (BuildingEntity building in GameData.Player.WorldMaps[this.MapName()].Buildings)
        {
            this.addEntitySprite(building);
            if (building.composition.componentConfigs.Assistance != null)
                this.SetBuildingState(building);
        }
    }

    public void SetBuildingState(BuildingEntity entity)
    {
        ResourceBubbleCtrl resourceBubbleCtrl = (ResourceBubbleCtrl)null;
        GameObject gameObject = (GameObject)null;
        if (!entity.IsResourceProducer())
            return;
        GameObject original = (GameObject)UnityEngine.Resources.Load("ResourceBubble");
        original.name = "ResourceBubble";
        if ((UnityEngine.Object)entity.bubble != (UnityEngine.Object)null)
        {
            UnityEngine.Object.Destroy((UnityEngine.Object)entity.bubble);
            entity.bubble = (GameObject)null;
        }
        if (entity.State == BuildingState.Open)
        {
            gameObject = UnityEngine.Object.Instantiate<GameObject>(original, Vector3.zero, Quaternion.identity);
            resourceBubbleCtrl = gameObject.GetComponent<ResourceBubbleCtrl>();
            if (entity.State != BuildingState.Inactive)
                resourceBubbleCtrl.OnClick += new EventHandler(this.CollectResource);
            resourceBubbleCtrl.Entity = entity;
            entity.bubble = resourceBubbleCtrl.gameObject;
            if (entity.State == BuildingState.Open)
            {
                resourceBubbleCtrl.Icon.sprite = UnityEngine.Resources.Load<Sprite>("UI/resource_time@2x");
                resourceBubbleCtrl.Icon.transform.localScale = new Vector3(0.5f, 0.5f, 0.0f);
            }
            gameObject.transform.SetParent(entity.entitySprite.transform);
            float num = (float)entity.entitySprite.GetComponent<SpriteAnim_b>().GetAnimation("Idle_B").info.height / 100f;
            gameObject.transform.localPosition = new Vector3(0.0f, num + 0.2f, 0.0f);
        }
        if (!((UnityEngine.Object)resourceBubbleCtrl != (UnityEngine.Object)null) || !((UnityEngine.Object)resourceBubbleCtrl.Icon != (UnityEngine.Object)null))
            return;
        resourceBubbleCtrl.SetSortingOrder(this.tileGrid.GetTileIndex(gameObject.transform.localPosition));
    }

    public void CollectResource(object sender, EventArgs args)
    {
        ResourceBubbleCtrl resourceBubbleCtrl = sender as ResourceBubbleCtrl;
        BuildingEntity entity = resourceBubbleCtrl.Entity;
        bool flag = true;
        if (entity.composition.componentConfigs.Assistance == null)
            flag = false;
        UnityEngine.Object.Destroy((UnityEngine.Object)resourceBubbleCtrl.transform.gameObject);
        entity.bubble = (GameObject)null;
        if (flag)
        {
            Tuple<string, int> tollValue = Functions.GetTollValue((object)entity.composition.componentConfigs.Assistance.rewards.amount.resources);
            if (entity.composition.componentConfigs.Assistance.rewards.amount.money > 0)
                this.ShowCollectText(entity, entity.composition.componentConfigs.Assistance.rewards.amount.money.ToString(), GameData.GetSprite("UI/resource_moneyicon_0"), 2, 0.0f);
            this.ShowCollectText(entity, tollValue.Item2.ToString(), GameData.GetSprite(Functions.ResourceToSpriteName(tollValue.Item1)), 1, 0.8f);
        }
        if (!flag)
            return;
        this.CollectOutput(entity);
    }

    private void ShowCollectText(
      BuildingEntity entity,
      string quantity,
      Sprite icon,
      int count,
      float shrink = 0.0f)
    {
        GameObject original = (GameObject)UnityEngine.Resources.Load("FloatNotification");
        original.name = "Collection";
        GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(original, Vector3.zero, Quaternion.identity);
        FloatingNote component = gameObject.GetComponent<FloatingNote>();
        gameObject.transform.SetParent(this.TopObject.transform, false);
        this.MainCamera.WorldToScreenPoint(entity.entitySprite.transform.position);
        this.MainCamera.WorldToScreenPoint(this.TopObject.transform.position);
        component.Icon.sprite = icon;
        if ((double)shrink > 0.0)
            component.Icon.transform.localScale = new Vector3(shrink, shrink, 0.0f);
        Vector3 position = entity.entitySprite.transform.position;
        position.z = 0.0f;
        gameObject.transform.position = position + new Vector3(0.1f, (float)(0.699999988079071 + 0.5 * (double)count), 0.0f);
        if (!((UnityEngine.Object)component != (UnityEngine.Object)null))
            return;
        component.SetText(quantity);
        if (entity.IsTaxBuilding())
            component.SetSound("UI02-collecting_taxes.caf");
        if (entity.IsFarmJob())
            component.SetSound("UI05-collecting_farm_jobs.caf");
        if (!entity.IsHospital() && !entity.IsJob() && !entity.IsResourceProducer())
            return;
        component.OnComplete += (EventHandler)((_param1, _param2) => this.SetBuildingState(entity));
    }

    private void CollectOutput(BuildingEntity entity)
    {
        Functions.GetTollValue((object)entity.composition.componentConfigs.Assistance.rewards.amount.resources);
        if (entity.composition.componentConfigs.Assistance.rewards.amount.money > 0)
            this.ShowCollectText(entity, entity.composition.componentConfigs.Assistance.rewards.amount.money.ToString(), GameData.GetSprite("UI/resource_moneyicon_0"), 2, 0.0f);
        entity.State = BuildingState.Closed;
    }

    public void CheckMissions()
    {
        if ((UnityEngine.Object)this.MissionDialogCtrl != (UnityEngine.Object)null)
            this.MissionDialogCtrl.OnFinish += new EventHandler(this.CloseNPCDialog);
        this.missionScriptEngine = new MissionScriptEngine();
        this.scriptQueue = new List<MissionScript>();
        this.missionScriptEngine.loadData();
        foreach (object newMission in this.missionScriptEngine.NewMissions)
            Debug.Log((object)string.Format("Found new mission {0}", newMission));
        List<string> stringList = new List<string>();
        foreach (string key in GameData.Player.CurrentMissions.Keys)
            stringList.Add(key);
        foreach (string str in stringList)
        {
            Debug.Log((object)string.Format("Checking mission - {0}", (object)str));
            if (this.missionScriptEngine.CheckObjectives(str, string.Empty))
            {
                Debug.Log((object)string.Format("Finished mission - {0}", (object)str));
                GameData.Player.FinishMission(str);
            }
        }
    }

    private void OnApplicationQuit()
    {
        Debug.Log((object)("Application ending after " + (object)Time.time + " seconds"));
    }

    public void Init()
    {
        for (int index1 = 0; index1 < 144; ++index1)
        {
            for (int index2 = 0; index2 < 144; ++index2)
            {
                Vector3 vector3 = new Vector3();
                vector3.x = (float)((double)(index1 - index2) * 0.399999976158142 / 2.0);
                vector3.y = (float)((double)(index1 + index2) * 0.199999988079071 / 2.0);
                vector3.y -= 14.4f;
                vector3.y += 0.09999999f;
                vector3.z = -1f;
                GridSpace gridSpace = this.tileGrid[index1, index2];
                gridSpace.Position = (Vector2)vector3;
                gridSpace.TileIndex = 0;
            }
        }
        this.LoadMapData(MapConfig.mapName);
        if (MapConfig.mapName == "map_empty")
        {
            this.BottomPanel.SetActive(false);
            this.IconPanel.SetActive(false);
            this.pBadgeController.gameObject.SetActive(false);
            this.Map.color = Functions.GetColor(0.0f, 0.0f, 0.0f);
        }
        else
        {
            this.LoadMap();
            this.BottomPanel.SetActive(true);
            this.IconPanel.SetActive(true);
            this.pBadgeController.gameObject.SetActive(true);
            this.pBadgeController.Init(GameData.Player);
        }
        if ((UnityEngine.Object)this.missionDetailDialogCtrl != (UnityEngine.Object)null)
        {
            this.missionDetailDialogCtrl.OnClose += new EventHandler(this.CloseMissionDetail);
            this.missionDetailDialogCtrl.OnGoButtonClicked += new EventHandler<ButtonEventArgs>(this.OnGoButtonClicked);
        }
        if ((UnityEngine.Object)this.missionCompleteCtrl != (UnityEngine.Object)null)
            this.missionCompleteCtrl.OnClose += new EventHandler(this.CloseMissionComplete);
        if ((UnityEngine.Object)this.missionListCtrl != (UnityEngine.Object)null)
            this.missionListCtrl.OnMissionClicked += new EventHandler<ButtonEventArgs>(this.OpenMissionDetail);
        GameData.Player.OnMissionsChanged += new EventHandler(this.UpdateMissionList);
    }

    private void LoadAsset(string addressName)
    {
        Addressables.LoadAsset<Texture2D>((object)addressName).Completed += new Action<AsyncOperationHandle<Texture2D>>(this.TextureHandle_Completed);
    }

    private void TextureHandle_Completed(AsyncOperationHandle<Texture2D> handle)
    {
        if (handle.Status != AsyncOperationStatus.Succeeded)
            return;
        Texture2D result = handle.Result;
        this.Map.sprite = Sprite.Create(result, new Rect(0.0f, 0.0f, (float)result.width, (float)result.height), new Vector2(0.5f, 0.5f), 100f);
        this.Map.GetComponent<BoxCollider2D>().size = this.Map.sprite.rect.size / 100f;
    }

    private void LoadMap()
    {
        int index1 = 0;
        int num1 = 0;
        if (MapConfig.mapName == "LandLayout_3" || MapConfig.mapName == "MyLand")
            this.LoadAsset(string.Format("Assets/Maps/LandLayout_1.png", (object[])Array.Empty<object>()));
        else if (MapConfig.mapName == "WORLD_MAP")
            this.LoadAsset(string.Format("Assets/Maps/WORLD_MAP.png", (object[])Array.Empty<object>()));
        else
            this.LoadAsset(string.Format("Assets/Maps/{0}.png", (object)MapConfig.mapName));
        Debug.Log((object)string.Format("Loading map {0}", (object)MapConfig.mapName));
        if (!GameData.Player.WorldMaps.ContainsKey(this.NPCId()))
        {
            GameData.Player.WorldMaps.Add(this.NPCId(), new MapData());
            this.SaveGame();
        }
        this.missingAnimationsDebug = new List<string>();
        for (int index2 = 0; index2 < this.landFile.map.layers[0].height; ++index2)
        {
            for (int index3 = 0; index3 < this.landFile.map.layers[0].width; ++index3)
            {
                if (num1 <= this.landFile.map.layers[0].data.Length)
                {
                    num1 = this.landFile.map.layers[0].data[index1];
                    ++index1;
                    if (num1 != 0)
                    {
                        int num2 = (index3 - index2) * 60 + 59;
                        int num3 = (index3 + index2) * 30 + 29;
                        if (MapBase.xmin > num2)
                            MapBase.xmin = num2;
                        if (MapBase.xmax < num2)
                            MapBase.xmax = num2;
                        if (MapBase.ymin > num3)
                            MapBase.ymin = num3;
                        if (MapBase.ymax < num3)
                            MapBase.ymax = num3;
                    }
                }
            }
        }
        if (MapConfig.mapName == "LandLayout_3" || MapConfig.mapName == "MyLand")
            return;
        foreach (BuildingJ building in this.mapFile.buildings)
        {
            if (GameData.Compositions.ContainsKey(building.type))
            {
                if (GameData.Compositions[building.type].componentConfigs.WorldMapObject != null)
                {
                    LinkPrereqCheck linkPrereqCheck = new LinkPrereqCheck(GameData.Compositions[building.type].componentConfigs.WorldMapObject.npcId);
                    if (GameData.Compositions[building.type].componentConfigs.Animation.animations.Default != null && GameData.Compositions[building.type].componentConfigs.Animation.animations.Default != string.Empty)
                    {
                        if (GameData.NPCs.ContainsKey(GameData.Compositions[building.type].componentConfigs.WorldMapObject.npcId))
                        {
                            NPCPrereqCheck npcPrereqCheck = new NPCPrereqCheck(GameData.Compositions[building.type].componentConfigs.WorldMapObject.npcId);
                            if (GameData.NPCs[GameData.Compositions[building.type].componentConfigs.WorldMapObject.npcId].level <= GameData.Player.Level && npcPrereqCheck.IsValid())
                                this.AddMapLink(building, GameData.Compositions[building.type].componentConfigs.WorldMapObject.npcId);
                        }
                        else
                            this.AddMapLink(building, GameData.Compositions[building.type].componentConfigs.WorldMapObject.npcId);
                    }
                }
                else
                {
                    BuildingEntity entity = new BuildingEntity(building.type);
                    entity.Uid = GameData.Player.WorldMaps[this.NPCId()].Buildings.Count<BuildingEntity>() <= 0 ? 1 : GameData.Player.WorldMaps[this.NPCId()].Buildings.DefaultIfEmpty<BuildingEntity>().Max<BuildingEntity>((Func<BuildingEntity, int>)(bld => bld.Uid)) + 1;
                    entity.Flip = building.flip;
                    entity.State = MapConfig.NPC() == null || !MapConfig.NPC().isHelpable || GameData.Compositions[building.type].componentConfigs.Assistance == null ? BuildingState.Closed : BuildingState.Open;
                    entity.Position = this.GetPosition_Tile(building.x, building.y);
                    entity.Level = 1;
                    this.addEntitySprite(entity);
                    this.SetBuildingState(entity);
                    if (MapConfig.mapName == "LandLayout_3")
                        GameData.Player.WorldMaps["MyLand"].Buildings.Add(entity);
                }
            }
        }
        foreach (object obj in this.missingAnimationsDebug)
            Debug.Log((object)string.Format("Missing animation -> {0}", obj));
    }

    public void LoadEncounters()
    {
        if (!GameData.Player.WorldMaps.ContainsKey(this.NPCId()) || GameData.Player.WorldMaps[this.NPCId()].Encounters == null)
            return;
        foreach (EncounterArmy encounter in GameData.Player.WorldMaps[this.NPCId()].Encounters)
        {
            if (encounter.Status != EncounterStatus.Complete)
                this.DeployEncounter(encounter);
        }
    }

    public void DeployEncounter(EncounterArmy encounter)
    {
        Vector3 position = Vector3.zero;
        Debug.Log((object)string.Format("Deploying {0}", (object)encounter.Name));
        if (encounter == null || !GameData.BattleEncounters.armies.ContainsKey(encounter.Name))
            return;
        BattleEncounterArmy army = GameData.BattleEncounters.armies[encounter.Name];
        position = new Vector3(encounter.X, encounter.Y, 0.0f);
        encounter.Marker = this.DeployOccupation(army.icon, Functions.GetColor((float)byte.MaxValue, 0.0f, 0.0f), encounter, position, BattleMapType.ThreeByFive, BattleType.Encounter);
    }

    public GameObject DeployOccupation(
      string icon,
      Color color,
      EncounterArmy encounter,
      Vector3 position,
      BattleMapType layout,
      BattleType type)
    {
        GameObject gameObject1 = GameObject.Find(string.Format("{0}-{1}", (object)encounter.Name, (object)encounter.InstanceId));
        Debug.Log((object)string.Format("Creating {0}", (object)encounter.Name));
        if ((UnityEngine.Object)gameObject1 != (UnityEngine.Object)null)
            return (GameObject)null;
        GameObject gameObject2 = UnityEngine.Object.Instantiate<GameObject>((GameObject)UnityEngine.Resources.Load("Occupation"), new Vector3(0.0f, 0.0f, 0.0f), Quaternion.identity);
        gameObject2.name = string.Format("{0}-{1}", (object)encounter.Name, (object)encounter.InstanceId);
        SpriteRenderer component1 = gameObject2.GetComponent<SpriteRenderer>();
        Point tileIndex = this.tileGrid.GetTileIndex(position);
        component1.sortingOrder = 144 - tileIndex.Y + 144;
        if ((UnityEngine.Object)this.TopObject != (UnityEngine.Object)null)
            gameObject2.transform.SetParent(this.TopObject.transform);
        gameObject2.transform.position = position;
        OccupationController component2 = gameObject2.GetComponent<OccupationController>();
        component2.OnClick += new EventHandler(this.Occupation_OnClick);
        component2.Type = type;
        component2.SetIcon(icon, component1.sortingOrder + 1);
        component2.occupationIcon.color = color;
        component2.Encounter = encounter;
        component2.MapLayout = layout;
        component2.MaxDefence = GameData.BattleEncounters.armies[encounter.Name].attackerDefenseSlots;
        component2.MaxOffence = GameData.BattleEncounters.armies[encounter.Name].attackerSlots;
        return gameObject2;
    }

    public void Occupation_OnClick(object sender, EventArgs args)
    {
        if (this.DialogsOpen())
            return;
        OccupationController occupationController = sender as OccupationController;
        if (sender == null)
            return;
        if (occupationController.Type == BattleType.Encounter)
        {
            BattleConfig.InitFromEncounter(occupationController.Encounter);
        }
        else
        {
            BattleConfig.Type = BattleType.Occupation;
            BattleConfig.Encounter = occupationController.Encounter;
            BattleConfig.OccupationId = string.Empty;
            BattleConfig.MapName = string.Empty;
            BattleConfig.MapLayout = occupationController.MapLayout;
            BattleConfig.MaxOffence = occupationController.MaxOffence;
            BattleConfig.MaxDefence = occupationController.MaxDefence;
            BattleConfig.ForceStart = false;
        }
        SceneManager.LoadScene("BattleMap");
    }

    private Vector3 GetPosition_Sprite(int x, int y)
    {
        return new Vector3((float)x / 1f, (float)y / 1f, 0.0f);
    }

    private Vector3 GetPosition_Tile(int _x, int _y)
    {
        float num1 = (float)_x;
        float num2 = (float)_y;
        float num3 = (float)(((double)num1 - (double)num2) * 60.0);
        float num4 = (float)(((double)num1 + (double)num2) * 30.0);
        float num5 = 2880f;
        float x = (float)(60.0 - 20.0 * ((double)num1 - (double)num2));
        num4 = num5 - (float)(10.0 * ((double)num1 + (double)num2));
        float y = (float)(10.0 * ((double)num1 + (double)num2));
        Vector3 vector3 = new Vector3((float)((int)((double)x - (double)MapBase.xmin) - 3000), (float)((int)((double)y - (double)MapBase.ymin) - 50), 0.0f);
        vector3 = new Vector3(x, y, 0.0f);
        return new Vector3((float)((double)vector3.x / 100.0 - 0.180000007152557), (float)((double)vector3.y / 100.0 + 0.0599999986588955), 0.0f);
    }

    private void OnDestroy()
    {
        GameData.Player.OnMissionsChanged -= new EventHandler(this.UpdateMissionList);
        if ((UnityEngine.Object)this.missionDetailDialogCtrl != (UnityEngine.Object)null)
        {
            this.missionDetailDialogCtrl.OnClose -= new EventHandler(this.CloseMissionDetail);
            this.missionDetailDialogCtrl.OnGoButtonClicked -= new EventHandler<ButtonEventArgs>(this.OnGoButtonClicked);
        }
        if ((UnityEngine.Object)this.missionCompleteCtrl != (UnityEngine.Object)null)
            this.missionCompleteCtrl.OnClose -= new EventHandler(this.CloseMissionComplete);
        if ((UnityEngine.Object)this.missionListCtrl != (UnityEngine.Object)null)
            this.missionListCtrl.OnMissionClicked -= new EventHandler<ButtonEventArgs>(this.OpenMissionDetail);
        if (!((UnityEngine.Object)this.MissionDialogCtrl != (UnityEngine.Object)null))
            return;
        this.MissionDialogCtrl.OnFinish -= new EventHandler(this.CloseNPCDialog);
    }

    public void Update()
    {
        if (Input.GetKeyUp(KeyCode.Escape))
            SceneManager.LoadScene("Menu");
        if (DialogConfig.IsScriptWaiting() && !this.DialogsOpen())
        {
            this.dialogDelay += Time.deltaTime;
            if ((double)this.dialogDelay > 1.0)
            {
                this.dialogDelay = 0.0f;
                this.OpenNPCDialog(DialogConfig.GetNextScript());
            }
        }
        else
            this.dialogDelay = 0.0f;
        if (DialogConfig.IsScriptWaiting() || !this.updateRequired || this.criticalRegion)
            return;
        this.updateRequired = false;
        this.UpdateMissionStatus(string.Empty);
    }

    public void CloseDialogPanel()
    {
        this.CloseDialogs();
        if (this.DialogsOpen())
            return;
        this.DialogPanel.SetActive(false);
    }

    public virtual bool DialogsOpen()
    {
        bool flag = false;
        if (this.MissionDialogCtrl.gameObject.activeSelf)
            flag = true;
        if ((UnityEngine.Object)this.missionDetailDialogCtrl != (UnityEngine.Object)null && this.missionDetailDialogCtrl.gameObject.activeSelf)
            flag = true;
        if ((UnityEngine.Object)this.missionCompleteCtrl != (UnityEngine.Object)null && this.missionCompleteCtrl.gameObject.activeSelf)
            flag = true;
        return flag;
    }

    public void UpdateMissionList(object sender, EventArgs args)
    {
        if (!((UnityEngine.Object)this.missionListCtrl != (UnityEngine.Object)null))
            return;
        this.missionListCtrl.BuildList();
    }

    public void UpdateMissionStatus(string newItem)
    {
        if (this.criticalRegion)
        {
            Debug.Log((object)"Critical region violation");
            this.updateRequired = true;
        }
        else
        {
            this.criticalRegion = true;
            this.missionScriptEngine = new MissionScriptEngine();
            this.scriptQueue = new List<MissionScript>();
            foreach (string index in GameData.Player.CurrentMissions.Keys.ToList<string>())
            {
                if (GameData.Player.CurrentMissions[index].State == MissionState.Finished || this.missionScriptEngine.CheckObjectives(index, newItem))
                {
                    if (GameData.Player.CurrentMissions[index].State != MissionState.Finished && !DialogConfig.IsScriptWaiting(index))
                        GameData.Player.FinishMission(index);
                    if (GameData.Player.CurrentMissions[index].State == MissionState.Finished && (index != "p01_NEW2INTRO_025_LandSetup" || SceneManager.GetActiveScene().name == "PlayerMap"))
                    {
                        Debug.Log((object)string.Format("Completion 1: mission - {0}", (object)index));
                        GameData.Player.CompleteMission(index);
                        if (GameData.Missions[index].finishScript == null && (GameData.Missions[index].hideIcon != 1 || GameData.Missions[index].completeScript != null) && GameData.Missions[index].rewards != null)
                            this.OpenMissionComplete(index);
                    }
                }
            }
            this.missionScriptEngine.loadData();
            foreach (string newMission in this.missionScriptEngine.NewMissions)
            {
                GameData.Player.InitMission(newMission);
                Debug.Log((object)string.Format("Found new mission {0}", (object)newMission));
            }
            List<string> stringList = new List<string>();
            foreach (string key in GameData.Player.CurrentMissions.Keys)
                stringList.Add(key);
            foreach (string index in stringList)
            {
                if (GameData.Player.CurrentMissions[index].State == MissionState.Initialising)
                    this.missionScriptEngine.CheckStartEffects(index);
                if (GameData.Player.CurrentMissions[index].State == MissionState.StartDialog)
                    GameData.Player.StartMissionDialog(index);
                if (GameData.Player.CurrentMissions[index].State != MissionState.Initialising && GameData.Player.CurrentMissions[index].State != MissionState.Started)
                {
                    int state = (int)GameData.Player.CurrentMissions[index].State;
                }
            }
            foreach (string index in GameData.Player.CurrentMissions.Keys.ToList<string>())
            {
                if (GameData.Player.CurrentMissions[index].State == MissionState.Finished || this.missionScriptEngine.CheckObjectives(index, newItem))
                {
                    if (GameData.Player.CurrentMissions[index].State != MissionState.Finished && !DialogConfig.IsScriptWaiting(index))
                        GameData.Player.FinishMission(index);
                    if (GameData.Player.CurrentMissions[index].State == MissionState.Finished && (index != "p01_NEW2INTRO_025_LandSetup" || SceneManager.GetActiveScene().name == "PlayerMap"))
                    {
                        Debug.Log((object)string.Format("Completion 2: mission - {0}", (object)index));
                        GameData.Player.CompleteMission(index);
                        if (GameData.Missions[index].finishScript == null && (GameData.Missions[index].hideIcon != 1 || GameData.Missions[index].completeScript != null) && GameData.Missions[index].rewards != null)
                            this.OpenMissionComplete(index);
                    }
                }
            }
            this.missionScriptEngine.loadData();
            this.criticalRegion = false;
        }
    }

    public void WorldIcon_OnClick()
    {
        this.GotoWorldMap();
    }

    public void HomeIcon_OnClick()
    {
        this.GotoHomeMap();
    }

    public void GotoBattleMap()
    {
        SceneManager.LoadScene("BattleMap");
    }

    public void GotoWorldMap()
    {
        MapConfig.InitWorldMap();
        SceneManager.LoadScene("WorldMap");
    }

    public void GotoHomeMap()
    {
        MapConfig.InitHome();
        SceneManager.LoadScene("PlayerMap");
    }

    public void LoadMapData(string mapname)
    {
        TextAsset textAsset1 = !(mapname == "WORLD_MAP") ? (!(mapname == "MyLand") ? UnityEngine.Resources.Load("JSON/" + mapname) as TextAsset : UnityEngine.Resources.Load("JSON/LandLayout_3") as TextAsset) : UnityEngine.Resources.Load("JSON/WORLD_MAP") as TextAsset;
        if ((UnityEngine.Object)textAsset1 != (UnityEngine.Object)null)
        {
            this.mapFile = JObject.Parse(textAsset1.ToString()).ToObject<LandLayout>();
            TextAsset textAsset2 = UnityEngine.Resources.Load("JSON/" + Path.GetFileNameWithoutExtension(this.mapFile.tileLayoutFile)) as TextAsset;
            if ((UnityEngine.Object)textAsset2 != (UnityEngine.Object)null)
            {
                this.landFile = JObject.Parse(textAsset2.ToString()).ToObject<GameMap>();
            }
            else
            {
                this.landFile = new GameMap();
                Debug.Log((object)string.Format("Land file not found {0}", (object)this.mapFile.tileLayoutFile));
            }
        }
        else
        {
            this.mapFile = new LandLayout();
            this.landFile = new GameMap();
            Debug.Log((object)string.Format("Map file not found {0}", (object)mapname));
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        throw new NotImplementedException();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!Input.GetMouseButtonDown(0) || this.DialogPanel.activeSelf)
            return;
        GameObject clickedObject = this.GetClickedObject();
        if (!((UnityEngine.Object)clickedObject != (UnityEngine.Object)null) || !(clickedObject.name == "Map"))
            return;
        this.mode = WorldMode.Dragging;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        throw new NotImplementedException();
    }

    internal GameObject GetClickedObject()
    {
        GameObject gameObject = (GameObject)null;
        if (EventSystem.current.IsPointerOverGameObject(this.fingerId))
            return (GameObject)null;
        Physics2D.Raycast((Vector2)Input.mousePosition, -Vector2.up);
        RaycastHit2D raycastHit2D = Physics2D.Raycast((Vector2)Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);
        if ((UnityEngine.Object)raycastHit2D.collider != (UnityEngine.Object)null)
        {
            gameObject = raycastHit2D.collider.gameObject;
            Debug.Log((object)raycastHit2D.collider.gameObject.name);
        }
        return gameObject;
    }

    private void addEntitySprite(BuildingEntity entity)
    {
        string empty = string.Empty;
        string _animationName1 = string.Empty;
        string _animationName2 = string.Empty;
        Composition composition = GameData.Compositions[entity.Name];
        if (composition.componentConfigs.Animation != null)
        {
            empty = composition.componentConfigs.Animation.animations.Default;
            _animationName1 = composition.componentConfigs.Animation.animations.Idle;
            if (_animationName1 == null || _animationName1 == string.Empty)
                _animationName1 = composition.componentConfigs.Animation.animations.Default;
            if (_animationName1 == null || _animationName1 == string.Empty)
                _animationName1 = composition.componentConfigs.Animation.animations.Active;
            _animationName2 = composition.componentConfigs.Animation.animations.Active;
            if (_animationName2 == null || _animationName2 == string.Empty)
                _animationName2 = composition.componentConfigs.Animation.animations.Default;
            if (_animationName2 == null || _animationName2 == string.Empty)
                _animationName2 = composition.componentConfigs.Animation.animations.Idle;
        }
        else if (entity.IsExpansion())
            _animationName1 = "LandExpansion_idle";
        this.building_prefab.name = entity.Name;
        GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(this.building_prefab, Vector3.zero, Quaternion.identity);
        SpriteAnim_b component = gameObject.GetComponent<SpriteAnim_b>();
        component.LoadAnimation(entity.Position, "Idle_B", _animationName1, true, SpriteType.Idle_Building);
        if (!string.IsNullOrEmpty(_animationName2))
            component.LoadAnimation(entity.Position, "Busy_B", _animationName2, true, SpriteType.Busy);
        if (!string.IsNullOrEmpty(empty))
            component.LoadAnimation(entity.Position, "Def_B", empty, true, SpriteType.Default);
        component.flip = entity.Flip;
        gameObject.GetComponent<SpriteRenderer>().sortingOrder = 144 - this.tileGrid.GetTileIndex(entity.Position).Y;
        gameObject.transform.SetParent(this.TopObject.transform, false);
        if (_animationName1 != "Replace Me")
        {
            if (!gameObject.GetComponent<SpriteAnim_b>().PlayAnimation("Idle_B", true) && !this.missingAnimationsDebug.Contains(entity.Name))
                this.missingAnimationsDebug.Add(entity.Name);
        }
        else
            gameObject.GetComponent<SpriteAnim_b>().PlayAnimation("Def_B", true);
        gameObject.GetComponent<SpriteAnim_b>().Shift();
        entity.entitySprite = gameObject;
    }

    public void RemoveEncounter()
    {
        UnityEngine.Object.Destroy((UnityEngine.Object)MapConfig.encounterComplete.Marker);
        MissionScriptEngine.UpdateBattleMissions(MapConfig.encounterComplete.Name);
        this.ExplodeEncounter(new Vector3(MapConfig.encounterComplete.X, MapConfig.encounterComplete.Y, 0.0f));
    }

    public void ExplodeEncounter(Vector3 position)
    {
        if ((bool)(UnityEngine.Object)MapConfig.encounterComplete.Marker)
        {
            UnityEngine.Object.Destroy((UnityEngine.Object)MapConfig.encounterComplete.Marker);
            MapConfig.encounterComplete.Marker = (GameObject)null;
        }
        GameObject original = (GameObject)UnityEngine.Resources.Load("UnitSprite");
        original.name = "EncounterExplosion";
        GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(original, Vector3.zero, Quaternion.identity);
        SpriteAnim component = gameObject.GetComponent<SpriteAnim>();
        component.CellNo = 0;
        component.LoadAnimation(position, "Encounter", "encounterexplosion", true, SpriteType.Encounter, 1, string.Empty);
        gameObject.GetComponent<SpriteRenderer>().sortingOrder = 145;
        gameObject.transform.SetParent(this.TopObject.transform, false);
        gameObject.GetComponent<SpriteAnim>().PlayAnimation("Encounter", false);
        component.DeathComplete += new EventHandler(this.RemoveEncounterComplete);
    }

    private void RemoveEncounterComplete(object sender, EventArgs args)
    {
        if (!(sender is SpriteAnim))
            return;
        SpriteAnim spriteAnim = sender as SpriteAnim;
        this.ShowVictoryText(spriteAnim.gameObject.transform.position);
        UnityEngine.Object.Destroy((UnityEngine.Object)spriteAnim.gameObject);
        MapConfig.encounterComplete.Status = EncounterStatus.Complete;
        this.WorldView.UpdateEncounter(MapConfig.encounterComplete, this.NPCId(), BattleConfig.AwardMoney);
        MapConfig.encounterComplete = (EncounterArmy)null;
        this.UpdateMissionStatus(string.Empty);
    }

    private void ShowVictoryText(Vector3 position)
    {
        GameObject original = (GameObject)UnityEngine.Resources.Load("ReadyNotification");
        original.name = "Victory";
        GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(original, Vector3.zero, Quaternion.identity);
        FloatingNote component = gameObject.GetComponent<FloatingNote>();
        gameObject.transform.SetParent(this.TopObject.transform, false);
        this.MainCamera.WorldToScreenPoint(position);
        Vector3 vector3 = position;
        vector3.z = 0.0f;
        gameObject.transform.position = vector3 + new Vector3(0.1f, 0.0f, 0.0f);
        if (!((UnityEngine.Object)component != (UnityEngine.Object)null))
            return;
        component.SetText("Victory!", 20);
    }

    private void AddMapLink(BuildingJ building, string npcId)
    {
        GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>((GameObject)UnityEngine.Resources.Load("MapLink"), new Vector3(0.0f, 0.0f, 0.0f), Quaternion.identity);
        MapLinkCtrl component = gameObject.GetComponent<MapLinkCtrl>();
        if ((UnityEngine.Object)this.TopObject != (UnityEngine.Object)null)
            gameObject.transform.SetParent(this.TopObject.transform);
        float x1 = (float)building.x;
        float y1 = (float)building.y;
        float num1 = (float)(((double)x1 - (double)y1) * 60.0);
        float num2 = (float)(((double)x1 + (double)y1) * 30.0);
        float num3 = 2880f;
        float x2 = (float)(60.0 - 20.0 * ((double)x1 - (double)y1));
        num2 = num3 - (float)(10.0 * ((double)x1 + (double)y1));
        float y2 = (float)(10.0 * ((double)x1 + (double)y1));
        Vector3 vector3 = new Vector3((float)((int)((double)x2 - (double)MapBase.xmin) - 3000), (float)((int)((double)y2 - (double)MapBase.ymin) - 50), 0.0f);
        vector3 = new Vector3(x2, y2, 0.0f);
        Vector3 position = new Vector3(vector3.x / 100f, vector3.y / 100f, 0.0f);
        component.Init(building, position, npcId);
        component.OnClick += new EventHandler<ButtonEventArgs>(this.GotoLink);
    }

    private void GotoLink(object sender, ButtonEventArgs args)
    {
        MapLinkCtrl mapLinkCtrl = sender as MapLinkCtrl;
        if (args.StringValue == "player")
        {
            this.GotoHomeMap();
        }
        else
        {
            MapConfig.InitFromNPC(args.StringValue);
            if ((UnityEngine.Object)mapLinkCtrl != (UnityEngine.Object)null)
                Debug.Log((object)string.Format("Going to {0}-{1}", (object)MapConfig.mapName, (object)mapLinkCtrl.npcId));
            SceneManager.LoadScene("WorldMap");
        }
    }

    public void InitIcons()
    {
        this.AddMapButton("Home", "home@2x");
        this.AddMapButton("World", "mapicon@2x");
    }

    private void AddMapButton(string buttonName, string iconName)
    {
        Sprite sprite = UnityEngine.Resources.Load<Sprite>("UI/" + iconName);
        GameObject original = (GameObject)UnityEngine.Resources.Load("IconButton");
        original.name = "IconButton";
        GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(original);
        Button component = gameObject.GetComponent<Button>();
        gameObject.GetComponent<IconButtonCtrl>().Icon.sprite = sprite;
        component.onClick.AddListener((UnityAction)(() => this.ExecuteButton(buttonName)));
        gameObject.transform.SetParent(this.IconPanel.transform, true);
    }

    private void ExecuteButton(string name)
    {
        if (this.mode == WorldMode.Select)
            return;
        if (!(name == "World"))
        {
            if (!(name == "Home"))
                return;
            this.GotoHomeMap();
        }
        else
            this.GotoWorldMap();
    }

    private void OpenNPCDialog(string _missionScript)
    {
        this.CloseDialogs();
        Debug.Log((object)string.Format("Opening NPC mission dialog for {0}", (object)DialogConfig.GetNextMissionId()));
        this.CloseDialogs();
        this.WaitingForDialog = true;
        this.DialogPanel.GetComponent<BringToFront>().SetAlphaLevel(0.6f);
        this.DialogPanel.SetActive(true);
        this.MissionDialogCtrl.gameObject.SetActive(true);
        this.MissionDialogCtrl.Play(_missionScript);
    }

    public void OpenMissionDetail(string _missionId)
    {
        if ((UnityEngine.Object)this.missionDetailDialogCtrl == (UnityEngine.Object)null)
            return;
        this.WaitingForDialog = true;
        this.DialogPanel.GetComponent<BringToFront>().SetAlphaLevel(0.0f);
        this.DialogPanel.SetActive(true);
        this.missionDetailDialogCtrl.gameObject.SetActive(true);
        this.missionDetailDialogCtrl.InitFromMission(_missionId, this.NPCId());
    }

    public void OpenMissionComplete(string _missionId)
    {
        if ((UnityEngine.Object)this.missionCompleteCtrl == (UnityEngine.Object)null)
            return;
        this.CloseDialogs();
        Debug.Log((object)string.Format("Opening complete mission dialog for {0}", (object)_missionId));
        this.DialogPanel.GetComponent<BringToFront>().SetAlphaLevel(0.0f);
        this.DialogPanel.SetActive(true);
        this.WaitingForDialog = true;
        this.missionCompleteCtrl.gameObject.SetActive(true);
        this.missionCompleteCtrl.InitFromMission(_missionId);
    }

    public void OpenMissionDetail(object sender, ButtonEventArgs args)
    {
        this.OpenMissionDetail(args.StringValue);
    }

    public void OnGoButtonClicked(object sender, ButtonEventArgs args)
    {
        this.GotoLink(sender, args);
    }

    public void CloseMissionDetail(object sender, EventArgs args)
    {
        this.missionDetailDialogCtrl.gameObject.SetActive(false);
        this.CloseDialogPanel();
        this.WaitingForDialog = false;
        this.UpdateMissionStatus(string.Empty);
    }

    public void CloseMissionComplete(object sender, EventArgs args)
    {
        this.missionCompleteCtrl.gameObject.SetActive(false);
        this.CloseDialogPanel();
        this.WaitingForDialog = false;
        this.UpdateMissionStatus(string.Empty);
    }

    public void CloseDialogs()
    {
    }

    public void CloseNPCDialog(object sender, EventArgs args)
    {
        string nextMissionId = DialogConfig.GetNextMissionId();
        this.DialogPanel.GetComponent<BringToFront>().SetAlphaLevel(0.0f);
        this.MissionDialogCtrl.gameObject.SetActive(false);
        this.CloseDialogPanel();
        this.WaitingForDialog = false;
        DialogConfig.RemoveTopScript();
        GameData.Player.AdvanceMissionPostDialog(nextMissionId);
        if (GameData.Player.CurrentMissions[nextMissionId].State == MissionState.Initialising || GameData.Player.CurrentMissions[nextMissionId].State == MissionState.Finished)
        {
            if (nextMissionId == "p01_NEW2INTRO_020_OpeningBattle" && GameData.Player.CurrentMissions[nextMissionId].State == MissionState.Finished)
            {
                VideoConfig.VideoName = "long_pan";
                SceneManager.LoadScene("Video");
            }
            else if (GameData.Missions[nextMissionId].rewards != null && GameData.Player.CurrentMissions[nextMissionId].State == MissionState.Finished)
                this.OpenMissionComplete(nextMissionId);
            else
                this.UpdateMissionStatus(string.Empty);
        }
        if (!(nextMissionId != null & nextMissionId != string.Empty) || !GameData.Player.CurrentMissions.ContainsKey(nextMissionId) || (GameData.Player.CurrentMissions[nextMissionId].State != MissionState.Started || GameData.Missions[nextMissionId].hideIcon != 0))
            return;
        this.OpenMissionDetail(nextMissionId);
    }

    public void SaveGame(object sender, EventArgs args)
    {
        this.SaveGame();
    }

    public void SaveGame()
    {
        OperationRequest request = new OperationRequest()
        {
            OperationCode = 1,
            Parameters = new Dictionary<byte, object>()
      {
        {
          PhotonEngine.instance.SubCodeParameterCode,
          (object) MessageSubCode.SaveProfile
        },
        {
          (byte) 5,
          MessageSerializerService.SerializeObjectOfType<Profile>(GameData.Player.GetProfile())
        }
      }
        };
        PhotonEngine.instance.SendRequest(request);
    }
}
