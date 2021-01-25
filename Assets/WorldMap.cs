
using BNR;
using GameCommon;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

public class WorldMap : MapBase, IPointerDownHandler, IEventSystemHandler
{
    public static WorldMap instance;
    public ScreenShotCtrl ScreenShotCtrl;
    public GameObject WorldViewAnchor;
    public NPCBadgeCtrl NPCBadgeCtrl;

    private void Awake()
    {
        if ((UnityEngine.Object)WorldMap.instance == (UnityEngine.Object)null)
            WorldMap.instance = this;
        else if ((UnityEngine.Object)WorldMap.instance != (UnityEngine.Object)this)
            UnityEngine.Object.Destroy((UnityEngine.Object)this.gameObject);
        this.fingerId = 0;
    }

    private new void Start()
    {
        this.TopObject = GameObject.Find("Map");
        GameData.Player.OnMissionUpdated += new EventHandler<MissionEventArgs>(this.OnMissionUpdated);
        base.Start();
        this.InitIcons();
        this.DisplayBadge();
        if ((UnityEngine.Object)this.MissionDialogCtrl != (UnityEngine.Object)null)
            this.MissionDialogCtrl.OnFinish += new EventHandler(((MapBase)this).CloseNPCDialog);
        GameData.Player.OnEncounterUpdated += new EventHandler<EncounterEventArgs>(this.OnEncounterUpdated);
        this.UpdateMissionStatus(string.Empty);
        this.LoadEncounters();
        GameData.Player.CheckForEncounterSpawn(MapConfig.npcId);
        if (MapConfig.encounterComplete == null)
            return;
        this.RemoveEncounter();
    }

    public void DisplayBadge()
    {
        if (string.IsNullOrEmpty(MapConfig.npcId))
            return;
        this.WorldViewAnchor.SetActive(MapConfig.npcId == "WORLD_MAP");
        this.NPCBadgeCtrl.gameObject.SetActive(MapConfig.npcId != "WORLD_MAP");
        if (!(MapConfig.npcId != "WORLD_MAP"))
            return;
        NPCs npC = GameData.NPCs[MapConfig.npcId];
        this.NPCBadgeCtrl.Avatar.sprite = GameData.GetIcon(npC.icon);
        this.NPCBadgeCtrl.Title.text = GameData.GetText(npC.name);
        this.NPCBadgeCtrl.Level.text = string.Format("Lvl. {0}", (object)npC.level.ToString());
    }

    public void CreateEncounter(EncounterArmy encounter)
    {
        Vector3 position = Vector3.zero;
        BattleEncounterArmy battleEncounter = (BattleEncounterArmy)null;
        if (encounter == null || !GameData.BattleEncounters.armies.ContainsKey(encounter.Name))
            return;
        battleEncounter = GameData.BattleEncounters.armies[encounter.Name];
        string t = battleEncounter.placement._t;
        if (!(t == "BNPlacementBuilding"))
        {
            if (!(t == "BNPlacementRandom"))
            {
                if (t == "BNPlacementLocation")
                    position = Functions.CalculateCoord(battleEncounter.placement.pos.x, battleEncounter.placement.pos.y);
            }
            else
            {
                Vector3 zero = Vector3.zero;
                int num1 = UnityEngine.Random.Range(1, 20) - 10;
                int num2 = UnityEngine.Random.Range(1, 20) - 10;
                float x = (float)((double)num1 * 57.5999984741211 / 144.0);
                float y = (float)((double)num2 * 28.7999992370605 / 144.0);
                position = zero + new Vector3(x, y, 0.0f);
            }
        }
        else
        {
            position = Vector3.zero;
            BuildingEntity buildingEntity = GameData.Player.WorldMaps[this.NPCId()].Buildings.Where<BuildingEntity>((Func<BuildingEntity, bool>)(x => x.Name == battleEncounter.placement.compositionName && x.State != BuildingState.Offline && x.State != BuildingState.RibbonTime)).FirstOrDefault<BuildingEntity>();
            if (buildingEntity != null)
                position = buildingEntity.entitySprite.GetComponent<SpriteAnim_b>().GetMarkerPosition();
        }
        encounter.X = position.x;
        encounter.Y = position.y;
        encounter.Marker = this.DeployOccupation(battleEncounter.icon, Functions.GetColor((float)byte.MaxValue, 0.0f, 0.0f), encounter, position, BattleMapType.ThreeByFive, BattleType.Encounter);
        this.WorldView.UpdateEncounter(encounter, this.NPCId(), 0);
    }

    private void OnDestroy()
    {
        GameData.Player.OnMissionUpdated -= new EventHandler<MissionEventArgs>(this.OnMissionUpdated);
        GameData.Player.OnEncounterUpdated -= new EventHandler<EncounterEventArgs>(this.OnEncounterUpdated);
        if (!((UnityEngine.Object)this.MissionDialogCtrl != (UnityEngine.Object)null))
            return;
        this.MissionDialogCtrl.OnFinish -= new EventHandler(((MapBase)this).CloseNPCDialog);
    }

    private new void Update()
    {
        if (Input.GetMouseButtonDown(0) && !this.DialogPanel.activeSelf)
        {
            GameObject clickedObject = this.GetClickedObject();
            if ((UnityEngine.Object)clickedObject != (UnityEngine.Object)null && clickedObject.name == "Map")
                this.mode = WorldMode.Dragging;
        }
        if (Input.GetMouseButtonUp(0))
            this.mode = WorldMode.None;
        float deltaTime = Time.deltaTime;
        foreach (BuildingEntity building in GameData.Player.WorldMaps["MyLand"].Buildings)
            building.UpdateExternal(deltaTime);
        base.Update();
    }

    private void LateUpdate()
    {
        switch (this.mode)
        {
            case WorldMode.Dragging:
                this.Difference = Camera.main.ScreenToWorldPoint(Input.mousePosition) - Camera.main.transform.position;
                if (!this.Drag)
                {
                    this.Drag = true;
                    this.Origin = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                    goto case WorldMode.Moving;
                }
                else
                    goto case WorldMode.Moving;
            case WorldMode.Moving:
                if (this.Drag)
                {
                    float orthographicSize = Camera.main.orthographicSize;
                    float num = orthographicSize * (float)Screen.width / (float)Screen.height;
                    Vector3 min = this.Map.bounds.min;
                    Vector3 max = this.Map.bounds.max;
                    Vector3 vector3 = this.Origin - this.Difference;
                    if ((double)vector3.x < (double)min.x + (double)num)
                        vector3.x = min.x + num;
                    if ((double)vector3.x > (double)max.x - (double)num)
                        vector3.x = max.x - num;
                    if ((double)vector3.y < (double)min.y + (double)orthographicSize)
                        vector3.y = min.y + orthographicSize;
                    if ((double)vector3.y > (double)max.y - (double)orthographicSize)
                        vector3.y = max.y - orthographicSize;
                    Camera.main.transform.position = vector3;
                    GameData.Player.cameraPos = Camera.main.transform.position;
                }
                Input.GetMouseButton(1);
                break;
            default:
                this.Drag = false;
                goto case WorldMode.Moving;
        }
    }

    public new void OnPointerDown(PointerEventData eventData)
    {
        GameObject clickedObject = this.GetClickedObject();
        if (!((UnityEngine.Object)clickedObject != (UnityEngine.Object)null) || !(clickedObject.name == "Map"))
            return;
        Point tileIndex = this.tileGrid.GetTileIndex(Camera.main.ScreenToWorldPoint(Input.mousePosition));
        Debug.Log((object)string.Format("Clicked tile index {0}, {1}", (object)tileIndex.X, (object)tileIndex.Y));
        if (this.tileGrid.GetOccupant(this.MainCamera.ScreenToWorldPoint(Input.mousePosition), true) != null)
            return;
        this.mode = WorldMode.Dragging;
    }

    public new void HomeIcon_OnClick()
    {
        this.GotoHomeMap();
    }

    public void ScreenShot()
    {
        AudioSource audioSource = (AudioSource)null;
        if ((UnityEngine.Object)audioSource != (UnityEngine.Object)null)
        {
            AudioClip clip = Resources.Load<AudioClip>("Audio/camera-shutter-click-01");
            audioSource.PlayOneShot(clip);
        }
        this.ScreenShotCtrl.Shoot();
    }

    public void OnMissionUpdated(object sender, MissionEventArgs args)
    {
        if (!((UnityEngine.Object)this.WorldView != (UnityEngine.Object)null))
            return;
        this.WorldView.UpdateMission(args.MissionId, args.State);
        if (args.State != MissionState.Completed)
            return;
        this.UpdateMissionStatus(string.Empty);
    }

    public void OnEncounterUpdated(object sender, EncounterEventArgs args)
    {
        switch (args.Mode)
        {
            case TransMode.Create:
                this.CreateEncounter(args.Encounter);
                break;
            case TransMode.Delete:
                this.RemoveEncounter(args.Encounter.Name);
                break;
        }
    }

    public void RemoveEncounter(string Id)
    {
        GameObject gameObject1 = (GameObject)null;
        GameObject gameObject2 = GameObject.Find("Map/" + Id);
        if (!((UnityEngine.Object)gameObject2 != (UnityEngine.Object)null))
            return;
        UnityEngine.Object.Destroy((UnityEngine.Object)gameObject2);
        gameObject1 = (GameObject)null;
    }

    private void Building_OnStateChange(object sender, BuildingEventArgs args)
    {
        if (!(sender is BuildingEntity entity))
            return;
        this.SetBuildingState(entity);
    }

    public new void SetBuildingState(BuildingEntity entity)
    {
        if (!entity.IsResourceProducer())
            return;
        GameObject original = (GameObject)Resources.Load("ResourceBubble");
        original.name = "ResourceBubble";
        if ((UnityEngine.Object)entity.bubble != (UnityEngine.Object)null)
        {
            UnityEngine.Object.Destroy((UnityEngine.Object)entity.bubble);
            entity.bubble = (GameObject)null;
        }
        if (entity.State != BuildingState.Open)
            return;
        GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(original, Vector3.zero, Quaternion.identity);
        ResourceBubbleCtrl component = gameObject.GetComponent<ResourceBubbleCtrl>();
        if (entity.State != BuildingState.Inactive)
            component.OnClick += new EventHandler(((MapBase)this).CollectResource);
        component.Entity = entity;
        entity.bubble = component.gameObject;
        if (entity.State == BuildingState.Open)
        {
            component.Icon.sprite = Resources.Load<Sprite>("UI/resource_time@2x");
            component.Icon.transform.localScale = new Vector3(0.8f, 0.8f, 0.0f);
        }
        gameObject.transform.SetParent(entity.entitySprite.transform);
        float num = (float)entity.entitySprite.GetComponent<SpriteAnim_b>().GetAnimation("Idle_B").info.height / 100f;
        gameObject.transform.localPosition = new Vector3(0.0f, num + 0.2f, 0.0f);
    }
}
