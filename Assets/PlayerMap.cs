
using BNR;
using GameCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayerMap : MapBase
{
    public static PlayerMap instance;
    private GameObject prefab;
    public GameObject DepotDialog;
    public GameObject UIObject;
    public GameObject Starburst;
    public LevelUpDialogCtrl LevelUpDialogCtrl;
    public FactoryDialogCtrl FactoryDialogCtrl;
    public HospitalDialogCtrl HospitalDialogCtrl;
    public BarracksDialogCtrl BarracksDialogCtrl;
    public BuildingUpgradeDialogCtrl BuildingUpgradeDialogCtrl;
    public AchievementUnlockedCtrl AchievementUnlockedCtrl;
    public StorageWindowCtrl StorageWindowCtrl;
    public GameObject PvPMenu;
    public ExpansionGrid expansionGrid;
    public GameObject ExpansionMesh;
    public GameObject MoveBar;
    public GameObject DoneBtn;
    public GameObject MoveText;
    public ScreenShotCtrl ScreenShotCtrl;
    public AudioSource SoundFX;
    private MessageBoxCtrl messageBoxCtrl;
    private DebugWindowCtrl debugWindowCtrl;
    private Vector3 LastPosition;
    private GameObject objectMoving;
    private BuildingEntity newBuilding;
    private Vector3 ResetCamera;
    private BuildingEntity selectedEntity;
    private int testCount;

    private void Awake()
    {
        if ((UnityEngine.Object)PlayerMap.instance == (UnityEngine.Object)null)
            PlayerMap.instance = this;
        else if ((UnityEngine.Object)PlayerMap.instance != (UnityEngine.Object)this)
            UnityEngine.Object.Destroy((UnityEngine.Object)this.gameObject);
        this.fingerId = 0;
    }

    private new void Start()
    {
        this.TopObject = GameObject.Find("Map");
        this.UIObject = GameObject.Find("Canvas");
        GameData.Player.OnChangeLevel += new EventHandler(this.OpenLevelUpDialog);
        GameData.Player.OnAchievementUpdated += new EventHandler<ButtonEventArgs>(this.AchievementUpdated);
        GameData.Player.OnMissionUpdated += new EventHandler<MissionEventArgs>(this.OnMissionUpdated);
        GameData.Player.OnEncounterUpdated += new EventHandler<EncounterEventArgs>(this.OnEncounterUpdated);
        this.UpdateVolume();
        base.Start();
        this.MainCamera = Camera.main;
        this.ResetCamera = Camera.main.transform.position;
        this.DepotDialog.SetActive(false);
        this.DepotDialog.GetComponentInChildren<CategoryPanelCtrl>().ExitHost.GetComponentInChildren<Button>().onClick.AddListener((UnityAction)(() => this.CloseDepot()));
        CategoryPanelCtrl componentInChildren = this.DepotDialog.GetComponentInChildren<CategoryPanelCtrl>();
        if ((UnityEngine.Object)componentInChildren != (UnityEngine.Object)null)
        {
            componentInChildren.OnButtonClick -= new EventHandler<PurchaseEventArgs>(this.OnPurchase);
            componentInChildren.OnButtonClick += new EventHandler<PurchaseEventArgs>(this.OnPurchase);
        }
        if ((UnityEngine.Object)this.HospitalDialogCtrl != (UnityEngine.Object)null)
        {
            this.HospitalDialogCtrl.OnClose += new EventHandler(this.CloseHospital);
            this.HospitalDialogCtrl.OnUpgrade += new EventHandler(this.UpgradeHospital);
            this.HospitalDialogCtrl.OnHurry += new EventHandler<HurryEventArgs>(this.HurryButton_OnClick);
            this.HospitalDialogCtrl.OnCollect += new EventHandler<CollectEventArgs>(this.CollectButton_OnClick);
        }
        if ((UnityEngine.Object)this.BuildingUpgradeDialogCtrl != (UnityEngine.Object)null)
            this.BuildingUpgradeDialogCtrl.OnClose += new EventHandler(this.CloseBuildingUpgrade);
        if ((UnityEngine.Object)this.BarracksDialogCtrl != (UnityEngine.Object)null)
        {
            this.BarracksDialogCtrl.OnClose += new EventHandler(this.CloseBarracks);
            this.BarracksDialogCtrl.OnUpgrade += new EventHandler(this.UpgradeBarracks);
            this.BarracksDialogCtrl.OnHurry += new EventHandler<HurryEventArgs>(this.HurryButton_OnClick);
            this.BarracksDialogCtrl.OnCollect += new EventHandler<CollectEventArgs>(this.CollectButton_OnClick);
        }
        if ((UnityEngine.Object)this.FactoryDialogCtrl != (UnityEngine.Object)null)
        {
            this.FactoryDialogCtrl.OnClose += new EventHandler(this.CloseFactoryDialog);
            this.FactoryDialogCtrl.OnUpgrade += new EventHandler(this.UpgradeFactory);
            this.FactoryDialogCtrl.OnHurry += new EventHandler<HurryEventArgs>(this.HurryButton_OnClick);
            this.FactoryDialogCtrl.OnCollect += new EventHandler<CollectEventArgs>(this.CollectButton_OnClick);
        }
        if ((UnityEngine.Object)this.LevelUpDialogCtrl != (UnityEngine.Object)null)
            this.LevelUpDialogCtrl.OnClose += new EventHandler(this.CloseLevelUpDialog);
        if ((UnityEngine.Object)this.AchievementUnlockedCtrl != (UnityEngine.Object)null)
            this.AchievementUnlockedCtrl.OnClose += new EventHandler(this.CloseAchievementDialog);
        if ((UnityEngine.Object)this.UnitInformationCtrl != (UnityEngine.Object)null)
        {
            this.UnitInformationCtrl.OnClose += new EventHandler(this.CloseUnitInformationDialog);
            this.UnitInformationCtrl.OnPromote += new EventHandler<ButtonEventArgs>(this.OpenUnitLevelUpDialog);
        }
        if ((UnityEngine.Object)this.UnitLevelUpCtrl != (UnityEngine.Object)null)
        {
            this.UnitLevelUpCtrl.OnHurry += new EventHandler<HurryEventArgs>(this.HurryButton_OnClick);
            this.UnitLevelUpCtrl.OnPromote += new EventHandler<ButtonEventArgs>(this.PromoteButton_OnClick);
            this.UnitLevelUpCtrl.OnCollect += new EventHandler<CollectEventArgs>(this.CollectButton_OnClick);
        }
        if ((UnityEngine.Object)this.DialogPanel != (UnityEngine.Object)null)
            this.DialogPanel.GetComponent<BringToFront>().OnClose += new EventHandler(this.CloseDialogs);
        this.messageBoxCtrl = MessageBoxCtrl.Instance();
        this.debugWindowCtrl = DebugWindowCtrl.Instance();
        this.expansionGrid.Init();
        this.InitIcons();
        this.LoadMapData();
        Camera.main.transform.position = GameData.Player.cameraPos + new Vector3(0.0f, 0.0f, -10f);
        if ((UnityEngine.Object)this.MissionDialogCtrl != (UnityEngine.Object)null)
            this.MissionDialogCtrl.OnFinish += new EventHandler(((MapBase)this).CloseNPCDialog);
        this.UpdateMissionStatus(string.Empty);
        this.LoadEncounters();
        if (MapConfig.encounterComplete == null)
            return;
        this.RemoveEncounter();
    }

    private void OnDestroy()
    {
        GameData.Player.OnChangeLevel -= new EventHandler(this.OpenLevelUpDialog);
        GameData.Player.OnAchievementUpdated -= new EventHandler<ButtonEventArgs>(this.AchievementUpdated);
        GameData.Player.OnMissionUpdated -= new EventHandler<MissionEventArgs>(this.OnMissionUpdated);
        GameData.Player.OnEncounterUpdated -= new EventHandler<EncounterEventArgs>(this.OnEncounterUpdated);
        if ((UnityEngine.Object)this.MissionDialogCtrl != (UnityEngine.Object)null)
            this.MissionDialogCtrl.OnFinish -= new EventHandler(((MapBase)this).CloseNPCDialog);
        if ((UnityEngine.Object)this.FactoryDialogCtrl != (UnityEngine.Object)null)
        {
            this.FactoryDialogCtrl.OnClose -= new EventHandler(this.CloseFactoryDialog);
            this.FactoryDialogCtrl.OnUpgrade -= new EventHandler(this.UpgradeFactory);
            this.FactoryDialogCtrl.OnHurry -= new EventHandler<HurryEventArgs>(this.HurryButton_OnClick);
            this.FactoryDialogCtrl.OnCollect -= new EventHandler<CollectEventArgs>(this.CollectButton_OnClick);
        }
        if ((UnityEngine.Object)this.UnitInformationCtrl != (UnityEngine.Object)null)
            this.UnitInformationCtrl.OnClose -= new EventHandler(this.CloseUnitInformationDialog);
        foreach (BuildingEntity building in GameData.Player.WorldMaps["MyLand"].Buildings)
            building.OnStateChange -= new EventHandler<BuildingEventArgs>(this.Building_OnStateChange);
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
                    break;
                }
                break;
            case WorldMode.Moving:
                if ((UnityEngine.Object)this.objectMoving != (UnityEngine.Object)null)
                {
                    MeshGenerator component = this.objectMoving.GetComponent<MeshGenerator>();
                    Vector3 worldPoint = this.MainCamera.ScreenToWorldPoint(Input.mousePosition);
                    if ((double)this.LastPosition.x != 0.0 && (double)this.LastPosition.y != 0.0)
                        this.Difference = worldPoint - this.LastPosition;
                    else
                        this.Difference = Vector3.zero;
                    this.Difference.z = 0.0f;
                    if ((UnityEngine.Object)component != (UnityEngine.Object)null && this.Difference != Vector3.zero)
                    {
                        if (this.newBuilding.IsExpansion())
                            component.Move(this.Difference, worldPoint, this.newBuilding.IsExpansion());
                        else
                            component.Move(this.Difference, this.newBuilding.entitySprite.transform.position, this.newBuilding.IsExpansion());
                        this.CheckPlacement(this.objectMoving, worldPoint);
                    }
                    if (this.newBuilding != null)
                        this.newBuilding.entitySprite.transform.position += this.Difference;
                    this.LastPosition = worldPoint;
                    break;
                }
                break;
            default:
                this.Drag = false;
                break;
        }
        if (this.Drag)
        {
            float orthographicSize = this.MainCamera.orthographicSize;
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
    }

    private void UpdateVolume()
    {
        AudioSource component = GameObject.Find("Audio Source").GetComponent<AudioSource>();
        if (!(bool)(UnityEngine.Object)component)
            return;
        component.volume = Settings.Volume_Background;
    }

    private void UpdateOpenDialogs()
    {
        if (this.FactoryDialogCtrl.gameObject.activeSelf)
            this.FactoryDialogCtrl.Refresh();
        if (this.HospitalDialogCtrl.IsOpen())
            this.HospitalDialogCtrl.Refresh();
        if (!this.BarracksDialogCtrl.IsOpen())
            return;
        this.BarracksDialogCtrl.Refresh();
    }

    private new void Update()
    {
        if (this.FactoryDialogCtrl.gameObject.activeSelf)
            this.FactoryDialogCtrl.UpdateProgressBar();
        if (this.HospitalDialogCtrl.IsOpen())
            this.HospitalDialogCtrl.UpdateProgressBar();
        if (this.BarracksDialogCtrl.IsOpen())
            this.BarracksDialogCtrl.UpdateProgressBar();
        if (this.UnitLevelUpCtrl.gameObject.activeSelf)
            this.UnitLevelUpCtrl.UpdateProgressBar();
        switch (this.mode)
        {
            case WorldMode.None:
                if (Input.GetKeyUp(KeyCode.M))
                {
                    Settings.Volume_Background = 0.0f;
                    this.UpdateVolume();
                    break;
                }
                break;
        }
        float deltaTime = Time.deltaTime;
        foreach (BuildingEntity building in GameData.Player.WorldMaps["MyLand"].Buildings)
            building.Update(deltaTime);
        foreach (ArmyUnit armyUnit in GameData.Player.Army.Values.Where<ArmyUnit>((Func<ArmyUnit, bool>)(x => x.Upgrading)))
            armyUnit.Update(deltaTime);
        if (Input.GetMouseButtonDown(0))
        {
            GameObject clickedObject = this.GetClickedObject();
            if ((UnityEngine.Object)clickedObject != (UnityEngine.Object)null && (clickedObject.name == "Map" || clickedObject.name == "MeshHighlight"))
            {
                BuildingEntity occupant = this.tileGrid.GetOccupant(this.MainCamera.ScreenToWorldPoint(Input.mousePosition), false);
                if (occupant == null)
                {
                    Debug.Log((object)"Map");
                    this.mode = WorldMode.Dragging;
                }
                else
                {
                    Debug.Log((object)string.Format("{0}", (object)occupant.Name));
                    if (this.mode == WorldMode.Select)
                    {
                        if (this.tileGrid.CheckPlacement(occupant.entitySprite.transform.position, occupant) && occupant.IsMoveable())
                            this.SelectObjectToMove(occupant);
                    }
                    else
                        this.Sprite_OnClick(occupant);
                }
            }
            else if ((UnityEngine.Object)clickedObject == (UnityEngine.Object)this.ExpansionMesh)
            {
                this.mode = WorldMode.Moving;
                this.objectMoving = clickedObject;
                this.LastPosition = this.MainCamera.ScreenToWorldPoint(Input.mousePosition);
            }
            else if (this.mode == WorldMode.Select && this.selectedEntity == null)
            {
                Point tileIndex = this.tileGrid.GetTileIndex(Camera.main.ScreenToWorldPoint(Input.mousePosition));
                if (tileIndex != null && this.tileGrid.IsInBounds(tileIndex) && (this.tileGrid[tileIndex].Occupant != null && this.tileGrid[tileIndex].Occupant.IsMoveable()))
                {
                    this.newBuilding = this.tileGrid[tileIndex].Occupant;
                    Debug.Log((object)string.Format("Selected {0}", (object)this.newBuilding.ToString()));
                    this.SelectObjectToMove(this.newBuilding);
                }
            }
        }
        if (Input.GetMouseButtonUp(0))
        {
            if (this.newBuilding != null)
            {
                Transform transform = this.ExpansionMesh.transform;
                int num = this.newBuilding.entitySprite.GetComponent<SpriteAnim_b>().GetAnimation("Idle_B").info.height / 100;
                this.newBuilding.entitySprite.GetComponent<SpriteAnim_b>().SetPosition(this.ExpansionMesh.transform.position);
            }
            else
                this.tileGrid.GetTileIndex(this.MainCamera.ScreenToWorldPoint(Input.mousePosition));
            this.LastPosition = Vector3.zero;
            if (this.mode != WorldMode.Select)
            {
                if (this.DoneBtn.activeSelf)
                    this.mode = WorldMode.Select;
                else
                    this.mode = WorldMode.None;
            }
        }
        base.Update();
    }

    public new void OnPointerUp(PointerEventData eventData)
    {
    }

    public new void OnPointerDown(PointerEventData eventData)
    {
        GameObject clickedObject = this.GetClickedObject();
        if ((UnityEngine.Object)clickedObject != (UnityEngine.Object)null && clickedObject.name == "Map")
        {
            Point tileIndex = this.tileGrid.GetTileIndex(Camera.main.ScreenToWorldPoint(Input.mousePosition));
            Debug.Log((object)string.Format("Clicked tile index {0}, {1}", (object)tileIndex.X, (object)tileIndex.Y));
            BuildingEntity occupant = this.tileGrid.GetOccupant(this.MainCamera.ScreenToWorldPoint(Input.mousePosition), true);
            if (occupant == null)
                this.mode = WorldMode.Dragging;
            else
                this.Sprite_OnClick(occupant);
        }
        else
        {
            if (!((UnityEngine.Object)clickedObject == (UnityEngine.Object)this.newBuilding.entitySprite))
                return;
            this.mode = WorldMode.Moving;
            this.objectMoving = clickedObject;
            this.LastPosition = this.MainCamera.ScreenToWorldPoint(Input.mousePosition);
        }
    }

    public new void OnPointerClick(PointerEventData eventData)
    {
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

    private bool CheckPlacement(GameObject objectMoving, Vector3 position)
    {
        MeshGenerator component = objectMoving.GetComponent<MeshGenerator>();
        bool placementValid;
        if (this.newBuilding.IsExpansion())
        {
            placementValid = this.expansionGrid.CheckPlacement(this.expansionGrid.GetTileIndex(position), 12, 12);
        }
        else
        {
            position = component.transform.position - new Vector3(0.4f, 0.0f, 0.0f);
            placementValid = this.tileGrid.CheckPlacement(position, this.selectedEntity != null ? this.selectedEntity : this.newBuilding);
        }
        component.SetColour(placementValid);
        return placementValid;
    }

    private void LoadMapData()
    {
        foreach (BuildingEntity entity in (IEnumerable<BuildingEntity>)GameData.Player.WorldMaps["MyLand"].Buildings.OrderByDescending<BuildingEntity, float>((Func<BuildingEntity, float>)(x => x.Position.y)))
        {
            if ((!entity.IsExpansion() || entity.State != BuildingState.Inactive) && entity.State != BuildingState.Hidden)
            {
                this.addEntitySprite(entity);
                this.SetBuildingState(entity);
            }
        }
    }

    private new GameObject GetClickedObject()
    {
        GameObject gameObject = (GameObject)null;
        if (EventSystem.current.IsPointerOverGameObject(this.fingerId))
            return (GameObject)null;
        Physics2D.Raycast((Vector2)Input.mousePosition, -Vector2.up);
        RaycastHit2D raycastHit2D = Physics2D.Raycast((Vector2)Camera.main.ScreenToWorldPoint(Input.mousePosition + new Vector3(1f, 1f, 0.0f)), Vector2.zero);
        if ((UnityEngine.Object)raycastHit2D.collider != (UnityEngine.Object)null)
        {
            gameObject = raycastHit2D.collider.gameObject;
            Debug.Log((object)raycastHit2D.collider.gameObject.name);
        }
        return gameObject;
    }

    public override bool DialogsOpen()
    {
        bool flag = base.DialogsOpen();
        if (this.DepotDialog.activeSelf || this.FactoryDialogCtrl.gameObject.activeSelf || (this.PvPMenu.gameObject.activeSelf || this.LevelUpDialogCtrl.gameObject.activeSelf) || (this.missionCompleteCtrl.gameObject.activeSelf || this.BarracksDialogCtrl.gameObject.activeSelf || (this.HospitalDialogCtrl.gameObject.activeSelf || !this.UnitPanel.GetComponent<SlideCtrl>().IsHidden())) || this.AchievementUnlockedCtrl.gameObject.activeSelf)
            flag = true;
        return flag;
    }

    private void CountSomething()
    {
        int num1 = 0;
        int num2 = 0;
        string str1 = string.Empty;
        string str2 = string.Empty;
        foreach (BattleUnit battleUnit in GameData.BattleUnits.Values)
        {
            int num3 = 0;
            if (battleUnit.weapons != null)
            {
                foreach (Weaponry weaponry in battleUnit.weapons.Values)
                {
                    if (weaponry.abilities != null)
                    {
                        if (weaponry.abilities.Length > num1)
                        {
                            num1 = weaponry.abilities.Length;
                            str1 = battleUnit.name;
                        }
                        num3 += weaponry.abilities.Length;
                    }
                }
            }
            if (num3 > num2)
            {
                num2 = num3;
                str2 = battleUnit.name;
            }
        }
        Debug.Log((object)string.Format("Max number ablities {1}:- {0}", (object)num1, (object)str1));
        Debug.Log((object)string.Format("Max total number ablities {1}:- {0}", (object)num2, (object)str2));
    }

    public new void InitIcons()
    {
        this.AddMapButton("Screenshot", "Camera");
        this.AddMapButton("Army", "army@2x");
        this.AddMapButton("PvP", "pvpbutton@2x");
        this.AddMapButton("Move", "move@2x");
        this.AddMapButton("Build", "build@2x");
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
        if (!(name == "Army"))
        {
            if (!(name == "World"))
            {
                if (!(name == "Build"))
                {
                    if (!(name == "Move"))
                    {
                        if (!(name == "PvP"))
                        {
                            if (!(name == "Screenshot"))
                                return;
                            if ((UnityEngine.Object)this.SoundFX != (UnityEngine.Object)null)
                                this.SoundFX.PlayOneShot(UnityEngine.Resources.Load<AudioClip>("Audio/camera-shutter-click-01"));
                            string str = this.ScreenShotCtrl.Shoot();
                            if (!string.IsNullOrEmpty(str))
                                this.messageBoxCtrl.Show(string.Format("Map exported to {0}", (object)str));
                            else
                                this.messageBoxCtrl.Show("Export of game map failed.");
                        }
                        else
                            this.OpenPvP();
                    }
                    else
                        this.StartMoveMode();
                }
                else
                    this.OpenDepot();
            }
            else
                this.GotoWorldMap();
        }
        else
            this.ShowArmy();
    }

    private void StartMoveMode()
    {
        this.mode = WorldMode.Select;
        if ((UnityEngine.Object)this.BottomPanel != (UnityEngine.Object)null)
        {
            SlideCtrl component = this.BottomPanel.GetComponent<SlideCtrl>();
            if ((UnityEngine.Object)component != (UnityEngine.Object)null)
                component.Hide();
        }
        if ((UnityEngine.Object)this.IconPanel != (UnityEngine.Object)null)
            this.IconPanel.GetComponent<SlideCtrl>().Hide();
        if ((UnityEngine.Object)this.pBadgeController != (UnityEngine.Object)null)
            this.pBadgeController.GetComponent<SlideCtrl>().Hide();
        if ((UnityEngine.Object)this.missionListCtrl != (UnityEngine.Object)null)
            this.missionListCtrl.gameObject.GetComponent<SlideCtrl>().Hide();
        this.MoveText.gameObject.SetActive(true);
        this.DoneBtn.gameObject.SetActive(true);
    }

    public void ExitMoveMode()
    {
        this.mode = WorldMode.None;
        if ((UnityEngine.Object)this.BottomPanel != (UnityEngine.Object)null)
        {
            SlideCtrl component = this.BottomPanel.GetComponent<SlideCtrl>();
            if ((UnityEngine.Object)component != (UnityEngine.Object)null)
                component.Show();
        }
        if ((UnityEngine.Object)this.IconPanel != (UnityEngine.Object)null)
            this.IconPanel.GetComponent<SlideCtrl>().Show();
        if ((UnityEngine.Object)this.pBadgeController != (UnityEngine.Object)null)
            this.pBadgeController.GetComponent<SlideCtrl>().Show();
        if ((UnityEngine.Object)this.missionListCtrl != (UnityEngine.Object)null)
            this.missionListCtrl.gameObject.GetComponent<SlideCtrl>().Show();
        this.MoveText.gameObject.SetActive(false);
        this.DoneBtn.gameObject.SetActive(false);
        if (this.selectedEntity == null)
            return;
        this.CancelMove((object)this, new ButtonEventArgs(ButtonValue.Cancel, string.Empty));
    }

    private void MoveObject(BuildingEntity entity)
    {
        if (!this.CheckPlacement(this.ExpansionMesh, this.newBuilding.entitySprite.transform.position))
            return;
        this.ExpansionMesh.GetComponent<MeshGenerator>().OnOKButtonClick = (EventHandler)null;
        this.ExpansionMesh.GetComponent<MeshGenerator>().OnCancelButtonClick = (EventHandler<ButtonEventArgs>)null;
        this.ExpansionMesh.GetComponent<MeshGenerator>().OnFlipButtonClick = (EventHandler<ButtonEventArgs>)null;
        this.ExpansionMesh.GetComponent<MeshGenerator>().OnSellButtonClick = (EventHandler<ButtonEventArgs>)null;
        this.ExpansionMesh.GetComponent<MeshGenerator>().Hide();
        this.tileGrid.UpdatePlacement(this.selectedEntity, false);
        this.selectedEntity.entitySprite.SetActive(true);
        this.selectedEntity.entitySprite.transform.position = this.newBuilding.entitySprite.transform.position;
        this.selectedEntity.Position = this.ExpansionMesh.transform.position;
        this.selectedEntity.entitySprite.GetComponent<SpriteAnim_b>().Move(this.selectedEntity.Position);
        this.tileGrid.UpdatePlacement(this.selectedEntity, true);
        this.selectedEntity.entitySprite.GetComponent<SpriteRenderer>().sortingOrder = 144 - this.tileGrid.GetTileIndex(entity.Position).Y;
        this.objectMoving = (GameObject)null;
        if ((UnityEngine.Object)this.newBuilding.entitySprite != (UnityEngine.Object)null)
            UnityEngine.Object.Destroy((UnityEngine.Object)this.newBuilding.entitySprite);
        this.newBuilding = (BuildingEntity)null;
        this.selectedEntity = (BuildingEntity)null;
        if (MissionScriptEngine.UpdateMoveBuilding(entity))
            this.UpdateMissionStatus(string.Empty);
        this.WorldView.ChangeBuildingState(entity.Uid, entity.State, entity.State, entity, 0);
        this.MoveText.SetActive(true);
    }

    private void SelectObjectToMove(BuildingEntity entity)
    {
        Vector3 zero = Vector3.zero;
        if (this.selectedEntity != null)
            return;
        this.MoveText.gameObject.SetActive(false);
        Point tileIndex = this.tileGrid.GetTileIndex(entity.entitySprite.transform.position);
        Vector3 position = (Vector3)this.tileGrid[tileIndex.X, tileIndex.Y].Position;
        this.selectedEntity = entity;
        this.selectedEntity.entitySprite.SetActive(false);
        this.newBuilding = this.addMovingEntitySprite(entity.Position, this.selectedEntity.Name, false, this.selectedEntity.State);
        this.ExpansionMesh.GetComponent<MeshGenerator>().DrawBorder = true;
        this.ExpansionMesh.GetComponent<MeshGenerator>().Init(entity.Position, (float)this.newBuilding.Width(), (float)this.newBuilding.Height(), entity.composition.componentConfigs.Sellable != null);
        this.ExpansionMesh.GetComponent<MeshGenerator>().Show();
        this.ExpansionMesh.GetComponent<MeshGenerator>().OnOKButtonClick += (EventHandler)((_param1, _param2) => this.MoveObject(entity));
        this.ExpansionMesh.GetComponent<MeshGenerator>().OnCancelButtonClick += new EventHandler<ButtonEventArgs>(this.CancelMove);
        this.ExpansionMesh.GetComponent<MeshGenerator>().OnSellButtonClick += new EventHandler<ButtonEventArgs>(this.SellBuilding);
        this.ExpansionMesh.GetComponent<MeshGenerator>().OnFlipButtonClick += new EventHandler<ButtonEventArgs>(this.FlipEntity);
        this.CheckPlacement(this.ExpansionMesh, entity.Position + new Vector3(0.0f, 0.2f, 0.0f));
        this.objectMoving = this.ExpansionMesh;
    }

    private void CancelMove(object sender, ButtonEventArgs args)
    {
        this.ExpansionMesh.GetComponent<MeshGenerator>().OnOKButtonClick = (EventHandler)null;
        this.ExpansionMesh.GetComponent<MeshGenerator>().OnCancelButtonClick = (EventHandler<ButtonEventArgs>)null;
        this.ExpansionMesh.GetComponent<MeshGenerator>().OnFlipButtonClick = (EventHandler<ButtonEventArgs>)null;
        this.ExpansionMesh.GetComponent<MeshGenerator>().OnSellButtonClick = (EventHandler<ButtonEventArgs>)null;
        this.ExpansionMesh.GetComponent<MeshGenerator>().Hide();
        this.selectedEntity.entitySprite.SetActive(true);
        this.objectMoving = (GameObject)null;
        if ((UnityEngine.Object)this.newBuilding.entitySprite != (UnityEngine.Object)null)
            UnityEngine.Object.Destroy((UnityEngine.Object)this.newBuilding.entitySprite);
        this.newBuilding = (BuildingEntity)null;
        this.selectedEntity = (BuildingEntity)null;
        this.MoveText.SetActive(true);
    }

    private void SellBuilding(object sender, ButtonEventArgs args)
    {
        BuildingEntity selectedEntity = this.selectedEntity;
        this.CancelMove((object)this, new ButtonEventArgs(ButtonValue.Cancel, string.Empty));
        this.tileGrid.UpdatePlacement(selectedEntity, false);
        if (selectedEntity.composition.componentConfigs.Sellable == null)
            return;
        if (selectedEntity.composition.componentConfigs.Sellable.amount.money > 0)
        {
            this.ShowCollectText(selectedEntity, selectedEntity.composition.componentConfigs.Sellable.amount.money.ToString(), GameData.GetSprite("UI/resource_moneyicon_0"), 1, 0.0f);
            GameData.Player.Storage.AddMoney(selectedEntity.composition.componentConfigs.Sellable.amount.money);
        }
        GameData.Player.WorldMaps["MyLand"].Buildings.Remove(selectedEntity);
        UnityEngine.Object.Destroy((UnityEngine.Object)selectedEntity.entitySprite);
        selectedEntity.State = BuildingState.Sold;
        GameData.Player.RecalculatePopulation();
    }

    private BuildingEntity addMovingEntitySprite(
      Vector3 pos,
      string _unitName,
      bool flip,
      BuildingState state)
    {
        string _animationName = string.Empty;
        Composition composition = GameData.Compositions[_unitName];
        BuildingEntity buildingEntity = new BuildingEntity(_unitName);
        if (composition.componentConfigs.Animation != null)
        {
            _animationName = composition.componentConfigs.Animation.animations.Idle;
            if (_animationName == null || _animationName == string.Empty)
                _animationName = composition.componentConfigs.Animation.animations.Default;
            if (_animationName == null || _animationName == string.Empty)
                _animationName = composition.componentConfigs.Animation.animations.Active;
        }
        else if (buildingEntity.IsExpansion())
            _animationName = "LandExpansion_idle";
        this.prefab = (GameObject)UnityEngine.Resources.Load("BuildingSprite");
        this.prefab.name = _unitName;
        GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(this.prefab, Vector3.zero, Quaternion.identity);
        SpriteAnim_b component = gameObject.GetComponent<SpriteAnim_b>();
        component.LoadAnimation(pos, "Idle_B", _animationName, true, SpriteType.Idle_Building);
        component.flip = flip;
        gameObject.GetComponent<SpriteRenderer>().sortingOrder = 153;
        gameObject.transform.SetParent(this.TopObject.transform, false);
        gameObject.GetComponent<SpriteAnim_b>().PlayAnimation("Idle_B", true);
        buildingEntity.entitySprite = gameObject;
        buildingEntity.Position = pos;
        return buildingEntity;
    }

    private void addEntitySprite(
      Vector3 pos,
      string _unitName,
      string IN,
      bool flip,
      BuildingState state)
    {
        Vector3 _position = new Vector3(pos.x / 100f, pos.y / 100f, 0.0f);
        this.prefab = (GameObject)UnityEngine.Resources.Load("BuildingSprite");
        this.prefab.name = _unitName;
        BuildingEntity buildingEntity = new BuildingEntity(_unitName);
        buildingEntity.Flip = flip;
        buildingEntity.State = state;
        GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(this.prefab, Vector3.zero, Quaternion.identity);
        SpriteAnim_b component = gameObject.GetComponent<SpriteAnim_b>();
        component.LoadAnimation(_position, "Idle_B", IN, true, SpriteType.Idle_Building);
        component.flip = flip;
        gameObject.GetComponent<SpriteRenderer>().sortingOrder = 2;
        gameObject.transform.SetParent(this.TopObject.transform, false);
        gameObject.GetComponent<SpriteAnim_b>().PlayAnimation("Idle_B", true);
        buildingEntity.TaskComplete += new EventHandler(this.TaskComplete);
        buildingEntity.entitySprite = gameObject;
        buildingEntity.Position = pos;
        GameData.Player.WorldMaps["MyLand"].Buildings.Add(buildingEntity);
    }

    private void addNewEntitySprite(
      Vector3 pos,
      string _unitName,
      string IIN,
      bool flip,
      BuildingState state,
      int Uid)
    {
        BuildingEntity entity = new BuildingEntity(_unitName);
        entity.Uid = Uid;
        entity.Flip = flip;
        entity.State = state;
        entity.Position = pos;
        entity.Level = 1;
        if (entity.IsExpansion())
            entity.SetExpanionBuildTime();
        this.addEntitySprite(entity);
        GameData.Player.WorldMaps["MyLand"].Buildings.Add(entity);
    }

    private void addEntitySprite(BuildingEntity entity)
    {
        string _animationName1 = string.Empty;
        string _animationName2 = string.Empty;
        string _animationName3 = string.Empty;
        string _animationName4 = string.Empty;
        Composition composition = GameData.Compositions[entity.Name];
        if (composition.componentConfigs.Animation != null)
        {
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
            if (composition.componentConfigs.Construction != null)
            {
                _animationName3 = composition.componentConfigs.Animation.animations.UnderConstruction;
                _animationName4 = composition.componentConfigs.Animation.animations.UnderConstruction2;
            }
        }
        else if (entity.IsExpansion())
        {
            _animationName1 = "LandExpansion_idle";
            _animationName3 = "contructionvehicle_idlebackr";
            _animationName4 = "contructionvehicle_idlebackr";
        }
        this.building_prefab.name = entity.Name;
        entity.OnStateChange += new EventHandler<BuildingEventArgs>(this.Building_OnStateChange);
        GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(this.building_prefab, Vector3.zero, Quaternion.identity);
        SpriteAnim_b component = gameObject.GetComponent<SpriteAnim_b>();
        component.LoadAnimation(entity.Position, "Idle_B", _animationName1, true, SpriteType.Idle_Building);
        component.LoadAnimation(entity.Position, "Con1", _animationName3, true, SpriteType.Construction1, 1, "bd02-building_in_progress.caf");
        component.LoadAnimation(entity.Position, "Con2", _animationName4, true, SpriteType.Construction2);
        if (_animationName2 != string.Empty && _animationName2 != null)
            component.LoadAnimation(entity.Position, "Busy_B", _animationName2, true, SpriteType.Busy);
        component.flip = entity.Flip;
        component.OnClick += (EventHandler)((_param1, _param2) => this.Sprite_OnClick(entity));
        gameObject.GetComponent<SpriteRenderer>().sortingOrder = 144 - this.tileGrid.GetTileIndex(entity.Position).Y;
        gameObject.transform.SetParent(this.TopObject.transform, false);
        if (entity.State == BuildingState.Placed)
            gameObject.GetComponent<SpriteAnim_b>().PlayAnimation("Con1", true);
        else
            gameObject.GetComponent<SpriteAnim_b>().PlayAnimation("Idle_B", true);
        entity.entitySprite = gameObject;
        Vector3 vector3_1 = new Vector3(0.4f, 0.0f, 0.0f);
        if (!entity.IsExpansion())
        {
            this.tileGrid.UpdatePlacement(entity, entity.Position - vector3_1, true);
        }
        else
        {
            vector3_1 = new Vector3(0.2f, 0.2f, 0.0f);
            this.tileGrid.UpdatePlacement(entity, entity.entitySprite.transform.position - vector3_1, true);
            Vector3 vector3_2 = new Vector3(0.4f, 0.2f, 0.0f);
            this.expansionGrid.ShowBorder(entity.Position - vector3_2, this.expansionGrid.GetTileIndex(component.transform.position), entity.State != BuildingState.Inactive);
        }
    }

    public void PromoteButton_OnClick(object sender, ButtonEventArgs args)
    {
        if (args.StringValue == null)
            return;
        GameData.Player.Army[args.StringValue].StartPromotion();
        this.UnitLevelUpCtrl.Show(GameData.Player.Army[args.StringValue]);
        this.WorldView.UnitPromotion(args.StringValue, true);
        if (!MissionScriptEngine.UpdateLevelupUnit())
            return;
        this.UpdateMissionStatus(string.Empty);
    }

    public void HurryButton_OnClick(object sender, HurryEventArgs args)
    {
        if (args.Building != null)
        {
            args.Building.Hurry();
            this.WorldView.HurryTask(HurryTaskType.Building, args.Building.Uid, args.Building.Name);
        }
        else
        {
            if (args.ArmyUnit == null)
                return;
            args.ArmyUnit.Hurry();
            this.WorldView.HurryTask(HurryTaskType.UnitPromotion, 0, args.ArmyUnit.Name);
        }
    }

    public void CollectButton_OnClick(object sender, CollectEventArgs args)
    {
        if (args.Building != null)
        {
            this.CollectOutput(args.Building);
        }
        else
        {
            if (args.ArmyUnit == null)
                return;
            args.ArmyUnit.Collect();
            this.WorldView.UnitPromotion(args.ArmyUnit.Name, false);
            this.CloseUnitLevelUp((object)this, new EventArgs());
            this.OpenUnitInformation(args.ArmyUnit.Name);
            if ((UnityEngine.Object)this.SoundFX != (UnityEngine.Object)null)
                this.SoundFX.PlayOneShot(UnityEngine.Resources.Load<AudioClip>("Audio/BN_achievement.caf"));
            if (!MissionScriptEngine.UpdateLevelupUnit())
                return;
            this.UpdateMissionStatus(string.Empty);
        }
    }

    private void Sprite_OnClick(BuildingEntity building)
    {
        if (this.DialogsOpen() || this.mode != WorldMode.None)
            return;
        if (building.IsUnderConstruction() || building.IsTaxBuilding() || (building.IsResourceProducer() || building.IsUpgrading()))
        {
            this.WaitingForDialog = true;
            this.DialogPanel.SetActive(true);
            this.FactoryDialogCtrl.gameObject.SetActive(true);
            this.FactoryDialogCtrl.Content.SetActive(true);
            this.FactoryDialogCtrl.InitFromBuildingEntity(building);
        }
        else if (building.IsStorageBuilding())
        {
            this.WaitingForDialog = true;
            this.DialogPanel.SetActive(true);
            this.StorageWindowCtrl.gameObject.SetActive(true);
            this.StorageWindowCtrl.Show(building);
        }
        else if (building.IsHospital())
        {
            this.WaitingForDialog = true;
            this.DialogPanel.SetActive(true);
            this.HospitalDialogCtrl.gameObject.SetActive(true);
            this.HospitalDialogCtrl.InitFromBuildingEntity(building);
        }
        else if (building.Name == "comp_milUnit_barracks" || building.IsProject())
        {
            this.WaitingForDialog = true;
            this.DialogPanel.SetActive(true);
            this.BarracksDialogCtrl.gameObject.SetActive(true);
            this.BarracksDialogCtrl.InitFromBuildingEntity(building);
        }
        else
        {
            if (!building.IsJob())
                return;
            this.WaitingForDialog = true;
            this.DialogPanel.SetActive(true);
            this.FactoryDialogCtrl.gameObject.SetActive(true);
            this.FactoryDialogCtrl.InitFromBuildingEntity(building);
        }
    }

    private void TaskComplete(object sender, EventArgs args)
    {
        if (!(sender is BuildingEntity entity))
            return;
        this.SetBuildingState(entity);
    }

    private void Building_OnStateChange(object sender, BuildingEventArgs args)
    {
        if (sender is BuildingEntity buildingEntity)
        {
            Debug.LogFormat("Building {0} changing {1} -> {2}", (object)buildingEntity.Name, (object)args.OldState, (object)args.NewState);
            this.SetBuildingState(buildingEntity);
            if (args.OldState == BuildingState.RibbonTime)
            {
                GameData.Player.RecalculatePopulation();
                this.UpdateMissionStatus(args.BuildingType);
                if (!buildingEntity.IsExpansion() && GameData.Player.IsBuildingAchievementLocked())
                {
                    GameData.Player.UpdateAchievementProgress_Building();
                    this.SaveGame();
                }
                if (buildingEntity.composition.componentConfigs.defenseStructure != null && !string.IsNullOrEmpty(buildingEntity.composition.componentConfigs.defenseStructure.unitId))
                    GameData.Player.AddUnit(buildingEntity.composition.componentConfigs.defenseStructure.unitId);
            }
            if (args.NewState == BuildingState.RibbonTime)
                this.UpdateMissionStatus(string.Empty);
            if (args.NewState == BuildingState.Working || args.OldState == BuildingState.UpgradeComplete)
                this.UpdateMissionStatus(string.Empty);
            if (args.NewState == BuildingState.Inactive && args.OldState == BuildingState.Full && !string.IsNullOrEmpty(buildingEntity.JobName))
            {
                if (buildingEntity.IsProject())
                {
                    MissionScriptEngine.UpdateCollectProject(buildingEntity.JobName);
                    if (GameData.Player.IsUnitAchievementLocked())
                        GameData.Player.UpdateAchievementProgress_Unit();
                }
                else
                    MissionScriptEngine.UpdateCollectJob(buildingEntity.JobName);
            }
            if (args.NewState == BuildingState.Full && args.OldState == BuildingState.Full && buildingEntity.IsResourceProducer())
            {
                ResourceBubbleCtrl component = buildingEntity.bubble.GetComponent<ResourceBubbleCtrl>();
                if ((UnityEngine.Object)component != (UnityEngine.Object)null)
                    this.CollectResource((object)component, new EventArgs());
            }
            int index = 0;
            if (buildingEntity.IsExpansion())
                index = this.expansionGrid.GetIndex(buildingEntity.Position + new Vector3(0.0f, 0.2f));
            this.WorldView.ChangeBuildingState(buildingEntity.Uid, args.OldState, args.NewState, buildingEntity, index);
        }
        this.UpdateOpenDialogs();
    }

    private new void SetBuildingState(BuildingEntity entity)
    {
        ResourceBubbleCtrl resourceBubbleCtrl = (ResourceBubbleCtrl)null;
        GameObject gameObject = (GameObject)null;
        this.prefab = (GameObject)UnityEngine.Resources.Load("ResourceBubble");
        this.prefab.name = "ResourceBubble";
        if ((UnityEngine.Object)entity.bubble != (UnityEngine.Object)null)
        {
            UnityEngine.Object.Destroy((UnityEngine.Object)entity.bubble);
            entity.bubble = (GameObject)null;
        }
        if (entity.StateHasBubble())
        {
            gameObject = UnityEngine.Object.Instantiate<GameObject>(this.prefab, Vector3.zero, Quaternion.identity);
            resourceBubbleCtrl = gameObject.GetComponent<ResourceBubbleCtrl>();
            if (entity.State != BuildingState.Inactive)
                resourceBubbleCtrl.OnClick += new EventHandler(this.CollectResource);
            resourceBubbleCtrl.Entity = entity;
            entity.bubble = resourceBubbleCtrl.gameObject;
            if (entity.Name == "comp_res_supplydepot")
            {
                resourceBubbleCtrl.Icon.sprite = UnityEngine.Resources.Load<Sprite>("UI/goods_0");
                resourceBubbleCtrl.Icon.transform.localScale = new Vector3(0.6f, 0.6f, 0.0f);
            }
            gameObject.transform.SetParent(entity.entitySprite.transform);
            float num = (float)entity.entitySprite.GetComponent<SpriteAnim_b>().GetAnimation("Idle_B").info.height / 100f;
            gameObject.transform.localPosition = new Vector3(0.0f, num + 0.2f, 0.0f);
        }
        else if ((UnityEngine.Object)entity.bubble != (UnityEngine.Object)null)
        {
            UnityEngine.Object.Destroy((UnityEngine.Object)entity.bubble);
            entity.bubble = (GameObject)null;
        }
        switch (entity.State)
        {
            case BuildingState.Inactive:
                if (entity.StateHasBubble())
                    resourceBubbleCtrl.Icon.sprite = UnityEngine.Resources.Load<Sprite>("UI/inactive_0");
                entity.entitySprite.GetComponent<SpriteAnim_b>().PlayAnimation("Idle_B", true);
                break;
            case BuildingState.Offline:
                resourceBubbleCtrl.Icon.sprite = UnityEngine.Resources.Load<Sprite>("Icons/PowerIcon@2x");
                resourceBubbleCtrl.Icon.transform.localScale = new Vector3(0.35f, 0.35f, 0.0f);
                entity.entitySprite.GetComponent<SpriteAnim_b>().PlayAnimation("Idle_B", true);
                break;
            case BuildingState.UnderConstruction1:
                entity.entitySprite.GetComponent<SpriteAnim_b>().PlayAnimation("Con1", true);
                break;
            case BuildingState.UnderConstruction2:
                entity.entitySprite.GetComponent<SpriteAnim_b>().PlayAnimation("Con2", true);
                break;
            case BuildingState.Working:
                entity.entitySprite.GetComponent<SpriteAnim_b>().PlayAnimation("Busy_B", true);
                break;
            case BuildingState.Full:
                if ((entity.IsProject() || entity.IsHospital()) && GameData.BattleUnits.ContainsKey(entity.JobName))
                {
                    resourceBubbleCtrl.Icon.sprite = GameData.GetIcon(GameData.BattleUnits[entity.JobName].icon);
                    resourceBubbleCtrl.Icon.transform.localScale = new Vector3(0.5f, 0.5f, 0.0f);
                }
                if (entity.IsJob())
                {
                    resourceBubbleCtrl.Icon.sprite = GameData.GetIcon(GameData.GetJobinfo(entity.JobName).icon);
                    resourceBubbleCtrl.Icon.transform.localScale = new Vector3(0.5f, 0.5f, 0.0f);
                }
                if (entity.IsTaxBuilding() && entity.Name != "comp_res_supplydepot")
                {
                    if (entity.composition.componentConfigs.Taxes.taxesReadyOverrideIcon != null && entity.composition.componentConfigs.Taxes.taxesReadyOverrideIcon != string.Empty)
                    {
                        resourceBubbleCtrl.Icon.sprite = GameData.GetSprite("UI/" + entity.composition.componentConfigs.Taxes.taxesReadyOverrideIcon);
                        resourceBubbleCtrl.Icon.transform.localScale = new Vector3(0.4f, 0.4f, 0.0f);
                    }
                    else
                        resourceBubbleCtrl.Icon.sprite = GameData.GetSprite("UI/resource_moneyicon_0");
                }
                if (entity.IsResourceProducer())
                {
                    Resource resource = Functions.ResourceNameToEnum(entity.composition.componentConfigs.ResourceProducer.outputType);
                    resourceBubbleCtrl.Icon.sprite = GameData.GetSprite(Functions.ResourceToSpriteName(resource));
                    resourceBubbleCtrl.Icon.transform.localScale = new Vector3(0.4f, 0.4f, 0.0f);
                }
                if (entity.IsDepot())
                {
                    resourceBubbleCtrl.Icon.sprite = UnityEngine.Resources.Load<Sprite>("UI/construction_0");
                    resourceBubbleCtrl.Icon.transform.localScale = new Vector3(0.9f, 0.9f, 0.0f);
                    break;
                }
                break;
            case BuildingState.RibbonTime:
                resourceBubbleCtrl.Icon.sprite = UnityEngine.Resources.Load<Sprite>("UI/buildingFinish_0");
                resourceBubbleCtrl.Icon.transform.localScale = new Vector3(0.8f, 0.8f, 0.0f);
                entity.entitySprite.GetComponent<SpriteAnim_b>().PlayAnimation("Idle_B", true);
                break;
            case BuildingState.Upgrading:
                resourceBubbleCtrl.Icon.sprite = UnityEngine.Resources.Load<Sprite>("UI/BN_alertIconUpgrade@2x");
                entity.entitySprite.GetComponent<SpriteAnim_b>().PlayAnimation("Idle_B", true);
                break;
            case BuildingState.UpgradeComplete:
                resourceBubbleCtrl.Icon.sprite = UnityEngine.Resources.Load<Sprite>("UI/BN_alertIconUpgrade_done@2x");
                entity.entitySprite.GetComponent<SpriteAnim_b>().PlayAnimation("Idle_B", true);
                break;
        }
        if (!((UnityEngine.Object)resourceBubbleCtrl != (UnityEngine.Object)null) || !((UnityEngine.Object)resourceBubbleCtrl.Icon != (UnityEngine.Object)null))
            return;
        resourceBubbleCtrl.SetSortingOrder(this.tileGrid.GetTileIndex(gameObject.transform.localPosition));
    }

    private new void CollectResource(object sender, EventArgs args)
    {
        ResourceBubbleCtrl resourceBubbleCtrl = sender as ResourceBubbleCtrl;
        BuildingEntity entity1 = resourceBubbleCtrl.Entity;
        if (entity1.State != BuildingState.UpgradeComplete && entity1.State != BuildingState.RibbonTime && (entity1.IsResourceStorage() && !GameData.Player.Storage.CheckResourceCapacity(Functions.OutputTypeToEnum(entity1.composition.componentConfigs.StructureMenu.roleIconName))))
        {
            if (entity1.AutoMode == BuildingEntity.AutoCollectMode.Done)
                return;
            this.messageBoxCtrl.Show("Resource storage full; use resources or buy more warehouses to collect.");
        }
        else
        {
            if (this.DialogsOpen() && entity1.AutoMode != BuildingEntity.AutoCollectMode.Done || (entity1.State == BuildingState.Upgrading || entity1.State == BuildingState.Offline) || entity1.IsDepot() && entity1.State != BuildingState.RibbonTime)
                return;
            bool flag = true;
            if (entity1.State == BuildingState.RibbonTime || entity1.State == BuildingState.UpgradeComplete)
                flag = false;
            UnityEngine.Object.Destroy((UnityEngine.Object)resourceBubbleCtrl.transform.gameObject);
            entity1.bubble = (GameObject)null;
            if (flag)
            {
                int num1;
                if (entity1.IsTaxBuilding())
                {
                    BuildingEntity entity2 = entity1;
                    num1 = entity1.TaxAmount();
                    string quantity1 = num1.ToString();
                    Sprite sprite1 = GameData.GetSprite("UI/resource_moneyicon_0");
                    this.ShowCollectText(entity2, quantity1, sprite1, 1, 0.0f);
                    if (entity1.Name == "comp_res_supplydepot")
                    {
                        TaxResources resources = entity1.composition.componentConfigs.Taxes.rewards.amount.resources;
                        if (resources.iron > 0)
                        {
                            BuildingEntity entity3 = entity1;
                            num1 = resources.iron;
                            string quantity2 = num1.ToString();
                            Sprite sprite2 = GameData.GetSprite("UI/resource_ironicon_0");
                            this.ShowCollectText(entity3, quantity2, sprite2, 2, 2.5f);
                        }
                        if (resources.wood > 0)
                        {
                            BuildingEntity entity3 = entity1;
                            num1 = resources.wood;
                            string quantity2 = num1.ToString();
                            Sprite sprite2 = GameData.GetSprite("UI/resource_woodicon_0");
                            this.ShowCollectText(entity3, quantity2, sprite2, 3, 2.5f);
                        }
                        if (resources.stone > 0)
                        {
                            BuildingEntity entity3 = entity1;
                            num1 = resources.stone;
                            string quantity2 = num1.ToString();
                            Sprite sprite2 = GameData.GetSprite("UI/resource_stoneicon_0");
                            this.ShowCollectText(entity3, quantity2, sprite2, 4, 2.5f);
                        }
                        BuildingEntity entity4 = entity1;
                        num1 = 10;
                        string quantity3 = num1.ToString();
                        Sprite sprite3 = GameData.GetSprite("UI/resource_currency@2x");
                        this.ShowCollectText(entity4, quantity3, sprite3, 5, 1.5f);
                    }
                    else if (entity1.composition.componentConfigs.Taxes.rewards.XP > 0)
                    {
                        BuildingEntity entity3 = entity1;
                        num1 = entity1.composition.componentConfigs.Taxes.rewards.XP;
                        string quantity2 = num1.ToString();
                        Sprite sprite2 = GameData.GetSprite("UI/resource_xp@2x");
                        this.ShowCollectText(entity3, quantity2, sprite2, 2, 0.8f);
                    }
                }
                if (entity1.IsProject() || entity1.IsHospital())
                    this.ShowCollectText(entity1, string.Empty, GameData.GetIcon(GameData.BattleUnits[entity1.JobName].icon), 1, 0.0f);
                if (entity1.IsJob())
                {
                    Job jobinfo = GameData.GetJobinfo(entity1.JobName);
                    if (jobinfo.rewards.XP > 0)
                    {
                        BuildingEntity entity2 = entity1;
                        num1 = jobinfo.rewards.XP;
                        string quantity = num1.ToString();
                        Sprite sprite = GameData.GetSprite("UI/resource_xp@2x");
                        this.ShowCollectText(entity2, quantity, sprite, 1, 0.8f);
                    }
                    if (entity1.JobReward(entity1.JobName).money > 0)
                    {
                        BuildingEntity entity2 = entity1;
                        num1 = entity1.JobReward(entity1.JobName).money;
                        string quantity = num1.ToString();
                        Sprite sprite = GameData.GetSprite("UI/resource_moneyicon_0");
                        this.ShowCollectText(entity2, quantity, sprite, 2, 0.0f);
                    }
                    if (entity1.JobReward(entity1.JobName).resources != null)
                    {
                        Tuple<string, int> tollValue = Functions.GetTollValue((object)entity1.JobReward(entity1.JobName).resources);
                        if (tollValue.Item2 > 0)
                        {
                            BuildingEntity entity2 = entity1;
                            num1 = tollValue.Item2;
                            string quantity = num1.ToString();
                            Sprite sprite = GameData.GetSprite(Functions.ResourceToSpriteName(tollValue.Item1));
                            this.ShowCollectText(entity2, quantity, sprite, 2, 0.0f);
                        }
                    }
                }
                if (entity1.IsResourceProducer())
                {
                    double outputRate = (double)entity1.composition.componentConfigs.ResourceProducer.outputRate;
                    double output = (double)entity1.composition.componentConfigs.buildingUpgrade.levels[entity1.Level - 1].output;
                    Resource resource = Functions.ResourceNameToEnum(entity1.composition.componentConfigs.ResourceProducer.outputType);
                    double num2 = output;
                    double num3 = outputRate * num2 / 100.0;
                    double a = (double)entity1.OutputQty();
                    this.ShowCollectText(entity1, Math.Ceiling(a).ToString(), GameData.GetSprite(Functions.ResourceToSpriteName(resource)), 1, 0.8f);
                }
            }
            else
            {
                this.ShowReadyText(entity1);
                if (entity1.IsExpansion())
                    entity1.State = BuildingState.Inactive;
            }
            if (!flag)
                return;
            this.CollectOutput(entity1);
        }
    }

    private void CollectOutput(BuildingEntity entity)
    {
        string jobName = entity.JobName;
        if (entity.IsTaxBuilding() || entity.IsResourceProducer())
        {
            if (entity.IsTaxBuilding())
            {
                MissionScriptEngine.UpdateTaxCollection(entity);
                this.UpdateMissionStatus("");
            }
            entity.CollectOutput();
            this.WorldView.CollectOutput(entity.Uid, this.MapName(), string.Empty);
            entity.State = BuildingState.Inactive;
        }
        else if (entity.IsProject())
        {
            this.WorldView.PurchaseEntity(entity.JobName, Vector3.zero, false, 0, false);
            GameData.Player.AddUnit(entity.JobName);
            entity.State = BuildingState.Inactive;
            if (jobName != null)
                this.UpdateMissionStatus(jobName);
            entity.JobName = string.Empty;
        }
        else
        {
            entity.CollectOutput();
            if (jobName == null)
                return;
            if (entity.IsJob())
            {
                this.WorldView.CollectOutput(entity.Uid, this.MapName(), jobName);
                entity.JobName = string.Empty;
            }
            this.UpdateMissionStatus(string.Empty);
        }
    }

    private void ShowReadyText(BuildingEntity entity)
    {
        this.prefab = (GameObject)UnityEngine.Resources.Load("ReadyNotification");
        this.prefab.name = "Ready";
        GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(this.prefab, Vector3.zero, Quaternion.identity);
        FloatingNote component = gameObject.GetComponent<FloatingNote>();
        gameObject.transform.SetParent(this.TopObject.transform, false);
        this.MainCamera.WorldToScreenPoint(entity.entitySprite.transform.position);
        Vector3 position = entity.entitySprite.transform.position;
        position.z = 0.0f;
        gameObject.transform.position = position + new Vector3(0.1f, 0.9f, 0.0f);
        if (!((UnityEngine.Object)component != (UnityEngine.Object)null))
            return;
        component.SetText("Ready!");
        component.SetSound("bd03-finalize_building.caf");
        if (entity.State == BuildingState.UpgradeComplete)
            entity.CompleteUpgrade();
        entity.State = BuildingState.Inactive;
        GameData.Player.RecalculatePopulation();
        if (entity.composition.componentConfigs.ResourceCapacity != null)
            GameData.Player.UpdateStorageLimit();
        if (entity.IsHospital())
            component.OnComplete += (EventHandler)((_param1, _param2) => this.SetBuildingState(entity));
        if (!entity.IsExpansion())
            return;
        Vector3 vector3 = new Vector3(0.0f, 0.2f, 0.0f);
        this.expansionGrid.RemoveBorder(this.expansionGrid.GetTileIndex(entity.entitySprite.transform.position));
        vector3 = new Vector3(0.8f, 0.2f, 0.0f);
        this.tileGrid.UpdatePlacement(entity, entity.Position - vector3, false);
        vector3 = new Vector3(0.0f, 0.2f, 0.0f);
        this.expansionGrid.OpenExpansion(this.expansionGrid.GetTilePos(entity.Position + vector3));
        UnityEngine.Object.Destroy((UnityEngine.Object)entity.entitySprite);
        entity.entitySprite = (GameObject)null;
        GameData.Player.WorldMaps["MyLand"].Buildings.Remove(entity);
        this.ExpansionMesh.GetComponent<MeshGenerator>().Hide();
        this.UpdateMissionStatus(string.Empty);
    }

    private void ShowCollectText(
      BuildingEntity entity,
      string quantity,
      Sprite icon,
      int count,
      float shrink = 0.0f)
    {
        this.prefab = (GameObject)UnityEngine.Resources.Load("FloatNotification");
        this.prefab.name = "Collection";
        GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(this.prefab, Vector3.zero, Quaternion.identity);
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

    private void TestSetup()
    {
    }

    public void OnEncounterUpdated(object sender, EncounterEventArgs args)
    {
        this.CreateEncounter(args.WorldId, args.Encounter);
    }

    public void CreateEncounter(string WorldId, EncounterArmy encounter)
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
            else if (WorldId == "MyLand")
            {
                position = (Vector3)this.tileGrid[this.expansionGrid.GetRandomTile()].Position;
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
            BuildingEntity buildingEntity = GameData.Player.WorldMaps[this.NPCId()].Buildings.Where<BuildingEntity>((Func<BuildingEntity, bool>)(x => x.Name == battleEncounter.placement.compositionName && x.State != BuildingState.Offline && x.State != BuildingState.RibbonTime)).FirstOrDefault<BuildingEntity>();
            if (buildingEntity != null)
                position = buildingEntity.entitySprite.GetComponent<SpriteAnim_b>().GetMarkerPosition();
        }
        encounter.X = position.x;
        encounter.Y = position.y;
        encounter.Marker = !(WorldId == MapConfig.mapName) ? (GameObject)null : this.CreateOccupation(battleEncounter.icon, Functions.GetColor((float)byte.MaxValue, 0.0f, 0.0f), encounter, position, BattleMapType.ThreeByFive, BattleType.Encounter);
        this.WorldView.UpdateEncounter(encounter, WorldId, BattleConfig.AwardMoney);
    }

    public GameObject CreateOccupation(
      string icon,
      Color color,
      EncounterArmy encounter,
      Vector3 position,
      BattleMapType layout,
      BattleType type)
    {
        this.prefab = (GameObject)UnityEngine.Resources.Load("Occupation");
        GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(this.prefab, new Vector3(0.0f, 0.0f, 0.0f), Quaternion.identity);
        gameObject.transform.position = position;
        gameObject.name = encounter.Name;
        SpriteRenderer component1 = gameObject.GetComponent<SpriteRenderer>();
        Point tileIndex = this.tileGrid.GetTileIndex(position);
        component1.sortingOrder = 144 - tileIndex.Y + 144;
        if ((UnityEngine.Object)this.TopObject != (UnityEngine.Object)null)
            gameObject.transform.SetParent(this.TopObject.transform);
        OccupationController component2 = gameObject.GetComponent<OccupationController>();
        component2.OnClick += new EventHandler(this.Occupation_OnClick);
        component2.Type = type;
        component2.SetIcon(icon, component1.sortingOrder + 1);
        component2.occupationIcon.color = color;
        component2.Encounter = encounter;
        component2.MapLayout = layout;
        component2.MaxDefence = GameData.BattleEncounters.armies[encounter.Name].attackerDefenseSlots;
        component2.MaxOffence = GameData.BattleEncounters.armies[encounter.Name].attackerSlots;
        return gameObject;
    }

    public new void WorldIcon_OnClick()
    {
        PlayerMap.instance = (PlayerMap)null;
        this.GotoWorldMap();
    }

    public void PvPIcon_OnClick()
    {
        PlayerMap.instance = (PlayerMap)null;
        this.OpenPvP();
    }

    private void ShowArmy()
    {
        this.DialogPanel.SetActive(true);
        if (!((UnityEngine.Object)this.UnitPanel != (UnityEngine.Object)null))
            return;
        this.UnitPanel.GetComponent<UnitPanel>().Build();
        this.UnitPanel.GetComponent<UnitPanel>().unitEvent.AddListener(new UnityAction<string>(this.OpenUnitInformation));
        SlideCtrl component = this.UnitPanel.GetComponent<SlideCtrl>();
        if (!((UnityEngine.Object)component != (UnityEngine.Object)null))
            return;
        component.Show();
    }

    public void OpenDepot()
    {
        AudioSource component = GameObject.Find("Audio Source").GetComponent<AudioSource>();
        if ((bool)(UnityEngine.Object)component)
            component.Pause();
        this.DepotDialog.transform.position = new Vector3((float)Screen.width / 2f, (float)-Screen.height);
        this.DialogPanel.SetActive(true);
        this.DepotDialog.SetActive(true);
        CategoryPanelCtrl componentInChildren = this.DepotDialog.GetComponentInChildren<CategoryPanelCtrl>();
        if (!((UnityEngine.Object)componentInChildren != (UnityEngine.Object)null))
            return;
        componentInChildren.StartTransition();
    }

    public void OpenUnitInformation(string unitName)
    {
        this.DialogPanel.SetActive(true);
        this.UnitInformationCtrl.gameObject.SetActive(true);
        this.UnitInformationCtrl.Show(unitName);
    }

    private void OnPurchase(object sender, PurchaseEventArgs args)
    {
        Vector3 zero = Vector3.zero;
        Vector3 position = !args.Name.StartsWith("LandExpand") ? (Vector3)this.tileGrid[105, 35].Position : this.expansionGrid.GetTilePos((Vector3)this.expansionGrid.grid[8][2].Position);
        this.CloseDepot();
        this.ExpansionMesh.GetComponent<MeshGenerator>().OnOKButtonClick += (EventHandler)((_param1, _param2) => this.BuyBuilding(args.Name));
        this.ExpansionMesh.GetComponent<MeshGenerator>().OnCancelButtonClick += new EventHandler<ButtonEventArgs>(this.CancelPurchase);
        this.ExpansionMesh.GetComponent<MeshGenerator>().OnFlipButtonClick += new EventHandler<ButtonEventArgs>(this.CancelPurchase);
        this.newBuilding = this.addMovingEntitySprite(this.ExpansionMesh.transform.position, args.Name, false, BuildingState.Buying);
        this.ExpansionMesh.GetComponent<MeshGenerator>().OnFlipButtonClick += new EventHandler<ButtonEventArgs>(this.FlipEntity);
        this.ExpansionMesh.GetComponent<MeshGenerator>().DrawBorder = true;
        this.ExpansionMesh.GetComponent<MeshGenerator>().Init(position, (float)this.newBuilding.Width(), (float)this.newBuilding.Height(), false);
        this.ExpansionMesh.GetComponent<MeshGenerator>().Show();
        this.CheckPlacement(this.ExpansionMesh, position + new Vector3(0.0f, 0.2f, 0.0f));
    }

    private void BuyBuilding(string name)
    {
        Vector3 zero = Vector3.zero;
        Vector3 vector3 = new Vector3(0.0f, 0.0f, 0.0f);
        bool flip = false;
        if (!this.CheckPlacement(this.ExpansionMesh, this.newBuilding.entitySprite.transform.position))
            return;
        SpriteAnim_b component = this.newBuilding.entitySprite.GetComponent<SpriteAnim_b>();
        if ((UnityEngine.Object)component != (UnityEngine.Object)null)
            flip = component.flip;
        if (this.newBuilding.IsExpansion())
            vector3 = new Vector3(0.4f, 0.2f, 0.0f);
        Vector3 position = this.ExpansionMesh.transform.position + vector3;
        int num = GameData.Player.WorldMaps["MyLand"].Buildings.Max<BuildingEntity>((Func<BuildingEntity, int>)(x => x.Uid)) + 1;
        this.WorldView.PurchaseEntity(name, position, flip, num, false);
        if (this.newBuilding.IsExpansion())
        {
            GameData.Player.Storage.AddMoney(-BuildingEntity.GetBuildCost_Money(this.newBuilding.composition.componentConfigs));
            GameData.Player.Storage.AddNanopods(-BuildingEntity.GetBuildCost_Currency(this.newBuilding.composition.componentConfigs));
        }
        else
            GameData.Player.Storage.DebitStorage(this.newBuilding.composition.componentConfigs.StructureMenu.cost);
        this.ExpansionMesh.GetComponent<MeshGenerator>().OnOKButtonClick = (EventHandler)null;
        this.ExpansionMesh.GetComponent<MeshGenerator>().OnCancelButtonClick = (EventHandler<ButtonEventArgs>)null;
        this.ExpansionMesh.GetComponent<MeshGenerator>().OnFlipButtonClick = (EventHandler<ButtonEventArgs>)null;
        this.ExpansionMesh.GetComponent<MeshGenerator>().OnSellButtonClick = (EventHandler<ButtonEventArgs>)null;
        this.ExpansionMesh.GetComponent<MeshGenerator>().Hide();
        this.objectMoving = (GameObject)null;
        if (this.newBuilding.IsResourceProducer())
        {
            BuildingEntity building = this.tileGrid.UpdatePlacement(this.newBuilding, true);
            if (building != null)
            {
                UnityEngine.Object.Destroy((UnityEngine.Object)building.entitySprite);
                this.WorldView.ChangeBuildingState(building.Uid, BuildingState.Inactive, BuildingState.Hidden, building, 0);
            }
        }
        UnityEngine.Object.Destroy((UnityEngine.Object)this.newBuilding.entitySprite);
        PlayerMap.instance.CompletePurchase(name, num, position, flip, false);
        this.newBuilding = (BuildingEntity)null;
    }

    private void FlipEntity(object sender, ButtonEventArgs args)
    {
        if (!((UnityEngine.Object)this.newBuilding.entitySprite != (UnityEngine.Object)null))
            return;
        SpriteAnim_b component = this.newBuilding.entitySprite.GetComponent<SpriteAnim_b>();
        component.flip = !component.flip;
    }

    private void CancelPurchase(object sender, ButtonEventArgs args)
    {
        this.ExpansionMesh.GetComponent<MeshGenerator>().OnOKButtonClick = (EventHandler)null;
        this.ExpansionMesh.GetComponent<MeshGenerator>().OnCancelButtonClick = (EventHandler<ButtonEventArgs>)null;
        this.ExpansionMesh.GetComponent<MeshGenerator>().OnSellButtonClick = (EventHandler<ButtonEventArgs>)null;
        this.ExpansionMesh.GetComponent<MeshGenerator>().OnFlipButtonClick = (EventHandler<ButtonEventArgs>)null;
        this.ExpansionMesh.GetComponent<MeshGenerator>().Hide();
        this.objectMoving = (GameObject)null;
        if ((UnityEngine.Object)this.newBuilding.entitySprite != (UnityEngine.Object)null)
            UnityEngine.Object.Destroy((UnityEngine.Object)this.newBuilding.entitySprite);
        this.newBuilding = (BuildingEntity)null;
    }

    public void CompletePurchase(string name, int newId, Vector3 position, bool flip, bool init)
    {
        Debug.Log((object)string.Format("Buying {0}", (object)name));
        if (!init && GameData.Compositions[name].componentConfigs.Construction != null)
            this.addNewEntitySprite(position, name, string.Empty, flip, BuildingState.Placed, newId);
        else
            this.addNewEntitySprite(position, name, string.Empty, flip, BuildingState.Inactive, newId);
    }

    public void CloseDialogs(object sender, EventArgs args)
    {
        this.CloseDialogs();
        if (this.UnitPanel.GetComponent<SlideCtrl>().IsHidden())
            return;
        this.UnitPanel.GetComponent<SlideCtrl>().OnComplete += new EventHandler(this.OnTransitionComplete);
        this.UnitPanel.GetComponent<SlideCtrl>().Hide();
    }

    public new void CloseDialogs()
    {
        this.CloseFactoryDialog((object)this, new EventArgs());
        this.CloseHospital((object)this, new EventArgs());
        this.CloseBarracks((object)this, new EventArgs());
        this.CloseBuildingUpgrade((object)this, new EventArgs());
        this.CloseStorageWindow((object)this, new EventArgs());
        this.CloseUnitLevelUp((object)this, new EventArgs());
        this.CloseUnitInformationDialog((object)this, new EventArgs());
    }

    private void OnTransitionComplete(object sender, EventArgs args)
    {
        this.UnitPanel.GetComponent<SlideCtrl>().OnComplete -= new EventHandler(this.OnTransitionComplete);
        this.CloseDialogPanel();
    }

    public void CloseBuildingUpgrade(object sender, EventArgs args)
    {
        this.BuildingUpgradeDialogCtrl.gameObject.SetActive(false);
        this.CloseDialogPanel();
        this.WaitingForDialog = false;
    }

    public void CloseDepot()
    {
        AudioSource component = GameObject.Find("Audio Source").GetComponent<AudioSource>();
        this.DepotDialog.GetComponent<CategoryPanelCtrl>().ResetPanel();
        this.DepotDialog.SetActive(false);
        this.CloseDialogPanel();
        if ((bool)(UnityEngine.Object)component)
            component.UnPause();
        this.WaitingForDialog = false;
    }

    public void CloseHospital(object sender, EventArgs args)
    {
        this.HospitalDialogCtrl.gameObject.SetActive(false);
        this.CloseDialogPanel();
        this.WaitingForDialog = false;
    }

    public void UpgradeHospital(object sender, EventArgs args)
    {
        BuildingEntity buildingEntity = this.HospitalDialogCtrl.GetBuildingEntity();
        this.HospitalDialogCtrl.CloseDialog();
        this.WaitingForDialog = true;
        this.DialogPanel.SetActive(true);
        this.BuildingUpgradeDialogCtrl.gameObject.SetActive(true);
        this.BuildingUpgradeDialogCtrl.InitFromInstance(buildingEntity, buildingEntity.Name);
    }

    public void UpgradeFactory(object sender, EventArgs args)
    {
        BuildingEntity buildingEntity = this.FactoryDialogCtrl.GetBuildingEntity();
        this.CloseFactoryDialog(sender, args);
        this.WaitingForDialog = true;
        this.DialogPanel.SetActive(true);
        this.BuildingUpgradeDialogCtrl.gameObject.SetActive(true);
        this.BuildingUpgradeDialogCtrl.InitFromInstance(buildingEntity, buildingEntity.Name);
    }

    public void UpgradeBarracks(object sender, EventArgs args)
    {
        BuildingEntity buildingEntity = this.BarracksDialogCtrl.GetBuildingEntity();
        this.BarracksDialogCtrl.CloseDialog();
        this.WaitingForDialog = true;
        this.DialogPanel.SetActive(true);
        this.BuildingUpgradeDialogCtrl.gameObject.SetActive(true);
        this.BuildingUpgradeDialogCtrl.InitFromInstance(buildingEntity, buildingEntity.Name);
    }

    public void CloseBarracks(object sender, EventArgs args)
    {
        this.BarracksDialogCtrl.gameObject.SetActive(false);
        this.CloseDialogPanel();
        this.WaitingForDialog = false;
    }

    public void OpenPvP()
    {
        this.WaitingForDialog = true;
        this.PvPMenu.gameObject.SetActive(true);
        this.DialogPanel.SetActive(true);
    }

    public void OpenRandomBattle()
    {
        this.ClosePvP();
        BattleConfig.Type = BattleType.VsRandom;
        BattleConfig.OccupationId = string.Empty;
        BattleConfig.TestId = string.Empty;
        BattleConfig.MapName = "BattleMapMarin";
        BattleConfig.MapLayout = BattleMapType.ThreeByFive;
        BattleConfig.MaxDefence = 0;
        BattleConfig.MaxOffence = 7;
        BattleConfig.ForceStart = false;
        SceneManager.LoadScene("BattleMap");
    }

    public void ClosePvP()
    {
        this.PvPMenu.gameObject.SetActive(false);
        this.CloseDialogPanel();
        this.WaitingForDialog = false;
    }

    public void CloseFactoryDialog(object sender, EventArgs args)
    {
        this.FactoryDialogCtrl.gameObject.SetActive(false);
        this.FactoryDialogCtrl.Content.SetActive(false);
        this.FactoryDialogCtrl.ContentShop.SetActive(false);
        this.CloseDialogPanel();
    }

    public void CloseLevelUpDialog(object sender, EventArgs args)
    {
        this.SaveGame();
        this.LevelUpDialogCtrl.gameObject.SetActive(false);
        this.Starburst.SetActive(false);
        this.CloseDialogPanel();
        this.WaitingForDialog = false;
        this.UpdateMissionStatus(string.Empty);
    }

    public void CloseAchievementDialog(object sender, EventArgs args)
    {
        this.SaveGame();
        this.AchievementUnlockedCtrl.gameObject.SetActive(false);
        this.Starburst.SetActive(false);
        this.CloseDialogPanel();
        this.WaitingForDialog = false;
    }

    public void CloseUnitInformationDialog(object sender, EventArgs args)
    {
        this.UnitInformationCtrl.gameObject.SetActive(false);
        this.CloseDialogPanel();
        this.WaitingForDialog = false;
    }

    public void CloseUnitLevelUp(object sender, EventArgs args)
    {
        this.UnitLevelUpCtrl.gameObject.SetActive(false);
        if (this.UnitInformationCtrl.gameObject.activeSelf)
            return;
        this.CloseDialogPanel();
        this.WaitingForDialog = false;
    }

    public void CloseStorageWindow(object sender, EventArgs args)
    {
        this.StorageWindowCtrl.gameObject.SetActive(false);
        this.CloseDialogPanel();
        this.WaitingForDialog = false;
    }

    public void OpenLevelUpDialog(object sender, EventArgs args)
    {
        Debug.Log((object)string.Format("Opening level up dialog for {0}", (object)GameData.Player.Level));
        this.CloseDialogs();
        this.WaitingForDialog = true;
        this.DialogPanel.SetActive(true);
        this.LevelUpDialogCtrl.gameObject.SetActive(true);
        this.Starburst.SetActive(true);
        this.Starburst.GetComponent<RotateCtrl>().InitBurst(true);
        this.Starburst.GetComponent<RotateCtrl>().Play();
        this.LevelUpDialogCtrl.SetLevel(GameData.Player.Level);
    }

    public void OpenUnitLevelUpDialog(object sender, ButtonEventArgs args)
    {
        Debug.Log((object)string.Format("Opening unit level up dialog for {0}", (object)GameData.Player.Level));
        this.WaitingForDialog = true;
        this.DialogPanel.SetActive(true);
        this.UnitLevelUpCtrl.gameObject.SetActive(true);
        this.UnitLevelUpCtrl.Show(GameData.Player.Army[args.StringValue]);
    }

    public void OpenAchievementDialog(string achievementId)
    {
        Debug.Log((object)string.Format("Opening achievement dialog", (object[])Array.Empty<object>()));
        this.CloseDialogs();
        this.WaitingForDialog = true;
        this.DialogPanel.SetActive(true);
        this.AchievementUnlockedCtrl.gameObject.SetActive(true);
        this.Starburst.SetActive(true);
        this.Starburst.GetComponent<RotateCtrl>().InitBurst(false);
        this.Starburst.GetComponent<RotateCtrl>().Play();
        this.AchievementUnlockedCtrl.InitFromAchievement(achievementId);
    }

    public void AchievementUpdated(object sender, ButtonEventArgs args)
    {
        if (GameData.Player.AchievementProgress[args.StringValue].Complete)
            this.OpenAchievementDialog(args.StringValue);
        this.WorldView.UpdateAchievement(args.StringValue, 1, GameData.Player.AchievementProgress[args.StringValue].Complete);
    }

    public new void Occupation_OnClick(object sender, EventArgs args)
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
            BattleConfig.TestId = occupationController.Encounter.Name;
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
}
