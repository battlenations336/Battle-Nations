
using Assets.ClientViews;
using BNR;
using ExitGames.Client.Photon;
using GameCommon;
using GameCommon.SerializedObjects;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class BattleMapCtrl : MonoBehaviour
{
    private int draggingCell = -1;
    private Vector3 originPos = Vector3.zero;
    private List<GameObject> weaponGO = new List<GameObject>();
    public Camera MainCamera;
    public static BattleMapCtrl instance;
    public BattleView BattleView;
    public GameObject TopObject;
    public SpriteRenderer BattleMap;
    public GameObject EnemyGridLayout;
    public GameObject PlayerGridLayout;
    public Text AttackUnitLimitText;
    public Text DefenseUnitLimitText;
    public Button PassButton;
    public Button RetreatButton;
    private ModalPanel_BattleResult modalPanel_BattleResult;
    private SPPanelCtrl skillPoints;
    private WaitForMatchCtrl waitForMatchCtrl;
    private MessageBoxCtrl messageBoxCtrl;
    private PvPTurnCtrl PvPTurnCtrl;
    private UnitPanel unitPanel;
    private UnityAction resultOKAction;
    private UnityAction spOKAction;
    private BattleResult battleResult;
    private int spTotal;
    private int moneyTotal;
    private const float lineWidth = 0.01f;
    private GridLayoutCtrl_Base enemyGrid;
    private GridLayoutCtrl_Base playerGrid;
    private int targetCount;
    private int targetListIndex;
    private BattleMode battleMode;
    private bool debugDots;
    private List<GameObject> vertexList;
    private Material mat;
    private Vector3 centre;
    private Vector3 gridOffset_e;
    private Vector3 gridOffset_p;
    private const float defBattleTileWidth = 180f;
    private const float defBattleTileHeight = 90f;
    private float battleTileWidth;
    private float battleTileHeight;
    private float mapScale;
    private Sprite[] array;
    private Texture2D testSprite;
    private Sprite screenSprite;
    private GameObject spriteHolder;
    private GameObject UIObject;
    private GameObject AoE_Reticule;
    private int currentHomeSelection;
    private int battleTurns;
    private float scaleRatio;
    private GameObject prefab;
    public DebugTextController debugWindow;
    public PlayerBadgeController pBadgeController;
    public EnemyBadgeController eBadgeController;
    public GameObject UnitDetail;
    public UnitBadgeCtrl UnitBadgeCtrl;
    public GameObject WeaponList;
    public FightButtonController fightButtonCtrl;
    private MapSize mapSize;
    private string mapName;
    private int[] deployOrder;
    private bool dragAOE;
    private int delay;

    private void Awake()
    {
        if ((UnityEngine.Object)BattleMapCtrl.instance == (UnityEngine.Object)null)
            BattleMapCtrl.instance = this;
        else if ((UnityEngine.Object)BattleMapCtrl.instance != (UnityEngine.Object)this)
            UnityEngine.Object.Destroy((UnityEngine.Object)this.gameObject);
        this.modalPanel_BattleResult = ModalPanel_BattleResult.Instance();
        this.skillPoints = SPPanelCtrl.Instance();
        this.waitForMatchCtrl = WaitForMatchCtrl.Instance();
        this.messageBoxCtrl = MessageBoxCtrl.Instance();
        this.PvPTurnCtrl = PvPTurnCtrl.Instance();
        this.waitForMatchCtrl.OnCancel += new EventHandler(this.WaitForMatch_OnCancel);
        this.waitForMatchCtrl.OnTimeout += new EventHandler(this.WaitForMatch_OnTimeout);
        this.unitPanel = UnitPanel.Instance();
        this.spOKAction = new UnityAction(this.ExitScreen);
        this.resultOKAction = new UnityAction(this.ShowSPDialog);
        this.debugWindow = GameObject.Find("DebugText").GetComponent<DebugTextController>();
        this.pBadgeController = GameObject.Find("PlayerBadge").GetComponent<PlayerBadgeController>();
        this.eBadgeController = GameObject.Find("EnemyBadge").GetComponent<EnemyBadgeController>();
    }

    private void init()
    {
        this.pBadgeController.Init(GameData.Player);
        if (BattleConfig.OccupationId == "tutorial-occupation-NEW")
        {
            this.eBadgeController.gameObject.SetActive(false);
            this.eBadgeController.Init(string.Empty, 0);
        }
        if (!string.IsNullOrEmpty(BattleConfig.EncounterId))
        {
            this.eBadgeController.gameObject.SetActive(true);
            this.eBadgeController.Init(BattleConfig.EncounterId);
        }
        this.TopObject = GameObject.Find("Background");
        this.UIObject = GameObject.Find("Canvas");
        this.vertexList = new List<GameObject>();
        this.MainCamera = Camera.main;
        this.centre = this.MainCamera.ViewportToScreenPoint((Vector3)this.MainCamera.rect.center);
        this.scaleRatio = this.TopObject.GetComponent<BackgroundController>().GetRatio() * 0.65f;
        this.gridOffset_e = new Vector3(50f, 10f) * this.scaleRatio;
        this.gridOffset_e = new Vector3(1f, 1.5f, 0.0f);
        this.gridOffset_p = new Vector3(-80f, -60f) * this.scaleRatio;
        this.gridOffset_p = new Vector3(-0.5f, 0.75f, 0.0f);
        if (!(bool)(UnityEngine.Object)this.mat)
        {
            Shader.Find("Hidden/Internal-Colored");
            this.mat = this.GetComponent<LineRenderer>().material;
            this.mat.hideFlags = HideFlags.HideAndDontSave;
            this.mat.SetInt("_Cull", 0);
            this.mat.SetInt("_ZWrite", 0);
            this.mat.SetInt("_ZTest", 8);
        }
        this.debugSettings();
        this.mapScale = 1f;
        this.battleTileWidth = 180f * this.mapScale;
        this.battleTileHeight = 90f * this.mapScale;
        this.battleTurns = 0;
    }

    private void debugSettings()
    {
        Resolution currentResolution = Screen.currentResolution;
        float orthographicSize = Camera.main.orthographicSize;
        double aspect = (double)Camera.main.aspect;
        int width = Screen.width;
        int height = Screen.height;
    }

    private void Start()
    {
        this.init();
        this.UpdateVolume();
        GameObject _parent = GameObject.Find("Background");
        if (BattleConfig.MapLayout == BattleMapType.ThreeByFive)
        {
            this.enemyGrid = (GridLayoutCtrl_Base)this.EnemyGridLayout.AddComponent<GridLayoutCtrl_3x5>();
            this.playerGrid = (GridLayoutCtrl_Base)this.PlayerGridLayout.AddComponent<GridLayoutCtrl_3x5>();
        }
        else
        {
            this.enemyGrid = (GridLayoutCtrl_Base)this.EnemyGridLayout.AddComponent<GridLayoutCtrl_3x3>();
            this.playerGrid = (GridLayoutCtrl_Base)this.PlayerGridLayout.AddComponent<GridLayoutCtrl_3x3>();
        }
        this.deployOrder = this.playerGrid.DeployOrder();
        this.enemyGrid.OnCollapseComplete += (EventHandler)((_param1, _param2) =>
        {
            Debug.LogFormat("collapse complete", (object[])Array.Empty<object>());
            if (this.enemyGrid.IsFrontRowEmpty())
                this.enemyGrid.Collapse();
            else if (BattleConfig.IsPvP())
                this.battleMode = BattleMode.WaitForEnemyAttack;
            else
                this.battleMode = BattleMode.EnemyAttack;
        });
        this.playerGrid.OnCollapseComplete += (EventHandler)((_param1, _param2) =>
        {
            if (this.playerGrid.IsFrontRowEmpty())
                this.playerGrid.Collapse();
            else
                this.battleMode = BattleMode.PlayerAttack;
        });
        this.enemyGrid.BuildGrid(_parent, this.mat, false);
        this.playerGrid.BuildGrid(_parent, this.mat, true);
        this.AddCells();
        if (this.debugDots)
            this.drawDebugDots();
        this.CreateArmy();
        this.unitPanel.unitEvent.AddListener(new UnityAction<string>(this.OnUnitSelected));
        if (BattleConfig.ForceStart)
        {
            this.fightButtonCtrl.SetPressed();
            this.StartBattle();
        }
        else
            this.battleMode = BattleMode.PlayerSetup;
    }

    private void OnApplicationQuit()
    {
        Debug.Log((object)("Application ending after " + (object)Time.time + " seconds"));
    }

    private void SetButtons(bool active)
    {
        this.PassButton.interactable = active;
        this.RetreatButton.interactable = active;
        this.RetreatButton.interactable = true;
    }

    public void DisplayBattleLimits()
    {
        if (BattleConfig.MaxDefence > 0)
        {
            this.DefenseUnitLimitText.gameObject.SetActive(true);
            this.DefenseUnitLimitText.text = string.Format("{0}/{1} Defenses", (object)this.playerGrid.GetDefenseUnitCount(), (object)BattleConfig.MaxDefence);
        }
        this.AttackUnitLimitText.gameObject.SetActive(true);
        this.AttackUnitLimitText.text = string.Format("{0}/{1} Units", (object)this.playerGrid.GetAttackUnitCount(), (object)BattleConfig.MaxOffence);
    }

    public void OpponentPassed()
    {
        if (BattleConfig.IsPvP())
            this.PvPTurnCtrl.Show(false);
        this.battleMode = BattleMode.PlayerAttack;
    }

    public void OpponentSurrendered()
    {
        this.battleResult = BattleResult.Victory;
        this.battleMode = BattleMode.Complete;
    }

    public void SurrenderButton_OnClick()
    {
        this.BattleView.Surrender();
        this.battleMode = BattleMode.Surrender;
    }

    public void PassButton_OnClick()
    {
        if (this.battleMode != BattleMode.PlayerAttack)
            return;
        this.playerGrid.UpdateCooldowns();
        if (BattleConfig.IsPvP())
        {
            this.BattleView.Pass();
            this.PvPTurnCtrl.Show(true);
        }
        this.battleMode = BattleMode.WaitForEnemyAttack;
    }

    public void SetOpponent(Opponent opponent, bool firstTurn)
    {
        int _cellNo = 0;
        if (this.battleMode == BattleMode.Queued)
            this.waitForMatchCtrl.ClosePanel();
        this.eBadgeController.Init(opponent.Name, opponent.Level);
        foreach (GridUnit gridUnit in opponent.Army)
        {
            if (gridUnit != null && gridUnit.Unit != null && gridUnit.Unit != string.Empty)
                this.addEnemySprite(_cellNo, gridUnit.Unit, 0);
            ++_cellNo;
        }
        this.PvPTurnCtrl.Show(!firstTurn);
        if (firstTurn)
            this.battleMode = BattleMode.PlayerAttack;
        else
            this.battleMode = BattleMode.WaitForEnemyAttack;
    }

    private void CreateArmy()
    {
        if (BattleConfig.MapName != null && BattleConfig.MapName != string.Empty)
            this.BattleMap.sprite = UnityEngine.Resources.Load<Sprite>("Maps/" + BattleConfig.MapName);
        if (!string.IsNullOrEmpty(BattleConfig.TestId))
            this.LoadTestEncounter(BattleConfig.TestId);
        if (!string.IsNullOrEmpty(BattleConfig.OccupationId))
            this.LoadTestEncounter(BattleConfig.OccupationId);
        if (string.IsNullOrEmpty(BattleConfig.EncounterId))
            return;
        this.LoadEncounter(BattleConfig.EncounterId);
    }

    private void LoadEncounter(string encounterId)
    {
        BattleEncounterArmy encounter = (BattleEncounterArmy)null;
        if (encounterId == string.Empty || !GameData.BattleEncounters.armies.ContainsKey(encounterId))
            return;
        encounter = GameData.BattleEncounters.armies[encounterId];
        int? gridId;
        if (encounter.units != null)
        {
            foreach (EncLayout unit in encounter.units)
            {
                gridId = unit.gridId;
                if (gridId.HasValue)
                {
                    gridId = unit.gridId;
                    this.addEnemySprite(gridId.Value, unit.unitId, 0);
                }
            }
            foreach (EncLayout unit in encounter.units)
            {
                gridId = unit.gridId;
                if (!gridId.HasValue)
                    this.addEnemySprite(-1, unit.unitId, 0);
            }
        }
        if (encounter.playerUnits != null)
        {
            foreach (EncLayout playerUnit in encounter.playerUnits)
            {
                gridId = playerUnit.gridId;
                if (gridId.HasValue)
                {
                    gridId = playerUnit.gridId;
                    this.addPlayerSprite(gridId.Value, playerUnit.unitId, 0);
                }
            }
        }
        if (!(encounter.placement._t == "BNPlacementBuilding") || GameData.Player.WorldMaps["MyLand"].Buildings.Where<BuildingEntity>((Func<BuildingEntity, bool>)(x => x.Name == encounter.placement.compositionName && x.State != BuildingState.Offline && x.State != BuildingState.RibbonTime)).FirstOrDefault<BuildingEntity>() == null || !GameData.Compositions.ContainsKey(encounter.placement.compositionName))
            return;
        Composition composition = GameData.Compositions[encounter.placement.compositionName];
        if (composition.componentConfigs.defenseStructure == null)
            return;
        this.addPlayerSprite(2, composition.componentConfigs.defenseStructure.unitId, 0);
    }

    private void LoadTestEncounter(string testId)
    {
        if (!(testId == "tutorial-occupation-NEW"))
        {
            if (!(testId == "Test2"))
            {
                if (!(testId == "Test1"))
                {
                    if (!(testId == "AC"))
                        return;
                    this.addEnemySprite(7, "hero_ancient_robot", 0);
                    this.addEnemySprite(4, "def_natural_rocks_small", 0);
                    this.addEnemySprite(5, "def_natural_rocks_large", 0);
                    this.enemyGrid.AttackingCell = 7;
                    this.enemyGrid.Attacker().Unit.CurrentWeapon = "primary";
                    this.enemyGrid.Attacker().Unit.CurrentAbility = 0;
                    this.eBadgeController.EnemyName.text = "Mutant Killer Dustbin";
                }
                else
                {
                    this.addEnemySprite(5, "s_raider_bombadier", 0);
                    this.addEnemySprite(6, "s_raider_infantry", 0);
                    this.addEnemySprite(7, "s_raider_brawler", 0);
                    this.addEnemySprite(8, "s_raider_infantry", 0);
                    this.addEnemySprite(9, "s_raider_bombadier", 0);
                    this.enemyGrid.AttackingCell = 7;
                    this.enemyGrid.Attacker().Unit.CurrentWeapon = "primary";
                    this.enemyGrid.Attacker().Unit.CurrentAbility = 0;
                    this.eBadgeController.EnemyName.text = "Captain Bad Guy";
                }
            }
            else
            {
                this.addEnemySprite(1, "s_raptor_weak", 0);
                this.addEnemySprite(4, "s_raptor_weak", 0);
                this.addEnemySprite(7, "s_raptor_weak", 0);
                this.enemyGrid.AttackingCell = 1;
                this.eBadgeController.EnemyName.text = "Raptors!!!";
            }
        }
        else
            this.loadScriptDriver();
    }

    private void loadScriptDriver()
    {
        int num = 0;
        BN_FirstBattleScriptDriver2 battleScriptDriver2 = GameData.LoadBattleScript();
        foreach (UnitLayout unit in battleScriptDriver2.layout.units)
        {
            this.addEnemySprite(unit.gridId, unit.unitName, unit.hp);
            ++num;
        }
        int _cellNo = 0;
        foreach (UnitLayout unitLayout in battleScriptDriver2.layout.playerUnitLayout)
        {
            this.addPlayerSprite(_cellNo, unitLayout.unitName, unitLayout.hp);
            ++_cellNo;
        }
    }

    private void ShowBattleResult(BattleResult _battleResult)
    {
        this.modalPanel_BattleResult.Show(_battleResult, this.resultOKAction, BattleConfig.EncounterId, this.moneyTotal);
    }

    private void ShowSPDialog()
    {
        if (this.battleResult == BattleResult.Victory && BattleConfig.OccupationId != "tutorial-occupation-NEW")
        {
            this.modalPanel_BattleResult.gameObject.SetActive(false);
            this.skillPoints.Show(this.playerGrid, this.spOKAction, this.spTotal);
        }
        else
            this.ExitScreen();
    }

    private void OnUnitSelected(string unitname)
    {
        if (this.battleMode != BattleMode.PlayerSetup || !GameData.Player.CanDeploy(unitname))
            return;
        if (!GameData.Player.Army[unitname].IsDefence() && this.playerGrid.GetAttackUnitCount() >= BattleConfig.MaxOffence)
            this.messageBoxCtrl.Show(string.Format("You may only place {0} units for this attack type!", (object)BattleConfig.MaxOffence));
        else if (GameData.Player.Army[unitname].IsDefence() && BattleConfig.MaxDefence > 0 && this.playerGrid.GetDefenseUnitCount() >= BattleConfig.MaxDefence)
        {
            this.messageBoxCtrl.Show(string.Format("You may only place {0} units for this defense type!", (object)BattleConfig.MaxDefence));
        }
        else
        {
            if (!GameData.Player.CanDeploy(unitname) || this.playerGrid.GetActiveUnitCount() >= BattleConfig.MaxOffence + BattleConfig.MaxDefence)
                return;
            int nextCell = this.playerGrid.GetNextCell();
            if (nextCell < 0)
                return;
            this.addPlayerSprite(nextCell, unitname, 0);
            GameData.Player.DeployUnit(unitname);
            this.unitPanel.UpdateButton(unitname, GameData.Player.Army[unitname].AvailableQty().ToString());
        }
    }

    private void ExitScreen()
    {
        if (this.battleResult == BattleResult.Victory && (BattleConfig.OccupationId != string.Empty || BattleConfig.EncounterId != string.Empty))
        {
            GameData.Player.DefeatedOccupations.Add(!string.IsNullOrEmpty(BattleConfig.EncounterId) ? BattleConfig.EncounterId : BattleConfig.OccupationId);
            if (!string.IsNullOrEmpty(BattleConfig.EncounterId))
            {
                BattleConfig.AwardMoney = this.moneyTotal;
                if (BattleConfig.Encounter == null)
                {
                    GameData.Player.CompleteEncounter(BattleConfig.EncounterId, this.moneyTotal);
                }
                else
                {
                    GameData.Player.CompleteEncounter(BattleConfig.Encounter.Name, this.moneyTotal);
                    MapConfig.encounterComplete = BattleConfig.Encounter;
                }
            }
        }
        this.playerGrid.ReleaseUnits();
        this.SaveGame();
        if (MapConfig.mapName == "MyLand")
            SceneManager.LoadScene("PlayerMap");
        else
            SceneManager.LoadScene("WorldMap");
    }

    private void UpdateVolume()
    {
        AudioSource component = GameObject.Find("BattleMusic").GetComponent<AudioSource>();
        if (!(bool)(UnityEngine.Object)component)
            return;
        component.volume = Settings.Volume_Background;
    }

    private void Update()
    {
        switch (this.battleMode)
        {
            case BattleMode.PlayerSetup:
                this.Update_PlayerSetup();
                break;
            case BattleMode.PlayerAttack:
                this.SetButtons(true);
                if (!Input.GetKeyUp(KeyCode.W))
                {
                    this.Update_PlayerAttack();
                    break;
                }
                break;
            case BattleMode.WaitForPlayerAttack:
                if (!BattleConfig.IsPvP())
                {
                    this.battleMode = BattleMode.PlayerAttack;
                    break;
                }
                break;
            case BattleMode.WaitForEnemyAttack:
                if (!BattleConfig.IsPvP())
                {
                    this.battleMode = BattleMode.EnemyAttack;
                    break;
                }
                break;
            case BattleMode.EnemyAttack:
                Debug.LogFormat("Enemy attack!", (object[])Array.Empty<object>());
                this.Update_EnemyAttack();
                break;
            case BattleMode.Surrender:
                this.battleResult = BattleResult.Surrender;
                this.battleMode = BattleMode.Complete;
                break;
            case BattleMode.Complete:
                this.Update_Complete();
                break;
        }
        if (Input.GetKeyUp(KeyCode.Equals))
            GameData.spriteScale += 0.01f;
        if (Input.GetKeyUp(KeyCode.Minus))
            GameData.spriteScale -= 0.01f;
        if (Input.GetKeyUp(KeyCode.F))
        {
            foreach (BattleGrid.GridCell cell in this.enemyGrid.Cells)
            {
                int num1 = 1;
                if (cell.UnitAlive())
                {
                    int num2;
                    if (cell.Unit.BattleUnit.stats[0].rewards.amount.money > 0)
                    {
                        BattleGrid.GridCell entity = cell;
                        num2 = cell.Unit.BattleUnit.stats[0].rewards.amount.money;
                        string quantity = num2.ToString();
                        Sprite sprite = GameData.GetSprite("UI/resource_moneyicon_0");
                        int count = num1;
                        this.ShowCollectText(entity, quantity, sprite, count, 0.0f);
                        ++num1;
                    }
                    if (cell.Unit.BattleUnit.stats[0].rewards.SP > 0)
                    {
                        BattleGrid.GridCell entity = cell;
                        num2 = cell.Unit.BattleUnit.stats[0].rewards.SP;
                        string quantity = num2.ToString();
                        Sprite sprite = GameData.GetSprite("UI/resource_sp@2x");
                        int count = num1;
                        this.ShowCollectText(entity, quantity, sprite, count, 0.8f);
                        int num3 = num1 + 1;
                    }
                }
            }
        }
        if (Input.GetKeyUp(KeyCode.D))
            this.ShowBattleResult(BattleResult.Surrender);
        if (!Input.GetKeyUp(KeyCode.M))
            return;
        Settings.Volume_Background = 0.0f;
        this.UpdateVolume();
    }

    private void UpdateDebugWindow()
    {
        this.debugWindow.DebugText.text = string.Format("Ratio {0}", (object)(this.MainCamera.orthographicSize * 2f / (float)Screen.height * (float)Screen.width));
        this.debugWindow.DebugText.text = string.Format("Width {0}", (object)Screen.width);
        this.debugWindow.DebugText.text += string.Format("\nHeight {0}", (object)Screen.height);
        this.debugWindow.DebugText.text += string.Format("\nScale {0}", (object)GameData.spriteScale.ToString());
    }

    private void Update_Complete()
    {
        this.ShowBattleResult(this.battleResult);
    }

    private void Update_PlayerSetup()
    {
        Vector3 zero1 = Vector3.zero;
        Vector3 zero2 = Vector3.zero;
        if (this.draggingCell > -1)
        {
            Vector3 mousePosition = Input.mousePosition;
            mousePosition.z = 10f;
            Vector3 worldPoint = Camera.main.ScreenToWorldPoint(mousePosition);
            this.playerGrid.ClearHighlights();
            this.playerGrid.ResetAttack();
            int index = this.playerGrid.IsCellClicked(worldPoint, this.battleTileWidth / 100f, this.battleTileHeight / 100f);
            if (index >= 0)
                this.playerGrid.Cells[index].LightUp = !this.playerGrid.Cells[index].UnitAlive() ? HighlightType.Selected : HighlightType.Hit;
            this.playerGrid.Cells[this.draggingCell].UnitSprite.GetComponent<SpriteAnim>().Move(worldPoint);
        }
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mousePosition = Input.mousePosition;
            mousePosition.z = 10f;
            int index = this.playerGrid.IsCellClicked(Camera.main.ScreenToWorldPoint(mousePosition), this.battleTileWidth / 100f, this.battleTileHeight / 100f);
            if (index >= 0 && this.playerGrid.Cells[index].UnitAlive())
            {
                this.playerGrid.ClearHighlights();
                this.playerGrid.ResetAttack();
                this.playerGrid.Cells[index].LightUp = HighlightType.Selected;
                this.draggingCell = index;
                this.originPos = this.playerGrid.Cells[index].Position;
                Debug.Log((object)string.Format("Grabbed cell {0}", (object)index));
            }
        }
        if (Input.GetMouseButtonUp(0) && this.draggingCell > -1)
        {
            Vector3 mousePosition = Input.mousePosition;
            mousePosition.z = 10f;
            int to = this.playerGrid.IsCellClicked(Camera.main.ScreenToWorldPoint(mousePosition), this.battleTileWidth / 100f, this.battleTileHeight / 100f);
            if (to >= 0)
            {
                if (this.playerGrid.Cells[to].UnitAlive())
                {
                    this.playerGrid.Cells[this.draggingCell].UnitSprite.GetComponent<SpriteAnim>().Move(this.originPos);
                }
                else
                {
                    this.playerGrid.Cells[this.draggingCell].UnitSprite.GetComponent<SpriteAnim>().NewCellPosition(this.playerGrid.Cells[to].Position);
                    this.playerGrid.MoveCellUnit(this.draggingCell, to);
                }
            }
            else
            {
                string name = this.playerGrid.Cells[this.draggingCell].Unit.Name;
                GameData.Player.ReleaseUnit(name);
                this.unitPanel.UpdateButton(name, GameData.Player.Army[name].AvailableQty().ToString());
                this.playerGrid.Cells[this.draggingCell].UnitSprite.GetComponent<SpriteAnim>().DeathComplete -= new EventHandler(this.PlayerDeathComplete);
                this.playerGrid.Cells[this.draggingCell].Unit.OnDeath = (EventHandler<UnitEventArgs>)null;
                this.playerGrid.Cells[this.draggingCell].Unit.OnFirstDamage = (EventHandler<UnitEventArgs>)null;
                this.playerGrid.Cells[this.draggingCell].Unit = (UnitEntity)null;
                this.playerGrid.Cells[this.draggingCell].HealthSprite = (GameObject)null;
                UnityEngine.Object.Destroy((UnityEngine.Object)this.playerGrid.Cells[this.draggingCell].UnitSprite);
                this.playerGrid.Cells[this.draggingCell].UnitSprite = (GameObject)null;
            }
            this.playerGrid.ClearHighlights();
            this.playerGrid.ResetAttack();
            this.draggingCell = -1;
            this.originPos = Vector3.zero;
        }
        this.DisplayBattleLimits();
        if (!Input.GetKeyUp(KeyCode.Escape))
            return;
        this.ExitScreen();
    }

    private void Update_PlayerAttack()
    {
        Vector3 zero = Vector3.zero;
        if (!Input.GetMouseButtonUp(0))
            return;
        Vector3 mousePosition = Input.mousePosition;
        mousePosition.z = 10f;
        Vector3 worldPoint = Camera.main.ScreenToWorldPoint(mousePosition);
        int index1 = this.playerGrid.IsCellClicked(worldPoint, this.battleTileWidth / 100f, this.battleTileHeight / 100f);
        if (index1 >= 0 && this.playerGrid.Cells[index1].UnitAlive())
        {
            this.playerGrid.ClearHighlights();
            this.playerGrid.ResetAttack();
            this.enemyGrid.ClearHighlights();
            this.enemyGrid.ResetAttack();
            this.SelectAttacker(index1);
            if ((UnityEngine.Object)this.AoE_Reticule != (UnityEngine.Object)null)
                UnityEngine.Object.Destroy((UnityEngine.Object)this.AoE_Reticule);
            this.enemyGrid.SetPossibleTargets(index1, this.playerGrid.Attacker(), index1);
            if (this.playerGrid.Attacker().Unit.IsAOE())
                this.AoE_Reticule = this.createReticule(index1);
        }
        if (this.playerGrid.Attacker() == null)
            return;
        int index2 = this.enemyGrid.IsCellClicked(worldPoint, this.battleTileWidth / 100f, this.battleTileHeight / 100f);
        if (index2 >= 0 && this.playerGrid.Attacker().Unit.IsOnCooldown())
            this.messageBoxCtrl.Show(string.Format("{0} cannot use that weapon for {1} more turns", (object)GameData.GetText(this.playerGrid.Attacker().Unit.BattleUnit.name), (object)this.playerGrid.Attacker().Unit.Cooldown()));
        else if (index2 >= 0 && this.enemyGrid.Cells[index2].UnitAlive() && (this.GetReticuleCell() == index2 || this.GetReticuleCell() == -1) && (this.enemyGrid.Cells[index2].LightUp == HighlightType.Selected || this.enemyGrid.Cells[index2].LightUp == HighlightType.Damage))
        {
            this.enemyGrid.SetTarget(this.playerGrid.Attacker(), index2, (UnityEngine.Object)this.AoE_Reticule != (UnityEngine.Object)null);
            this.enemyGrid.ClearHighlights();
            if ((UnityEngine.Object)this.AoE_Reticule != (UnityEngine.Object)null)
                UnityEngine.Object.Destroy((UnityEngine.Object)this.AoE_Reticule);
            this.playerGrid.Attacker().UnitSprite.GetComponent<SpriteAnim>().PlayAnimation(this.playerGrid.Attacker().Unit.CurrentWeapon, false);
            this.SearchForTargets();
            this.battleMode = BattleMode.PlayerPostAttack;
            this.SetButtons(false);
            if (BattleConfig.IsPvP())
                this.BattleView.LaunchAttack(this.playerGrid, this.enemyGrid);
            this.playerGrid.UpdateCooldowns();
            this.playerGrid.Attacker().Unit.ApplyCooldown();
            this.RefreshWeaponTabs();
        }
        else
        {
            if (index2 < 0)
                return;
            if ((UnityEngine.Object)this.AoE_Reticule != (UnityEngine.Object)null)
                UnityEngine.Object.Destroy((UnityEngine.Object)this.AoE_Reticule);
            if (!this.playerGrid.Attacker().Unit.IsAOE())
                return;
            this.enemyGrid.ClearHighlights();
            this.enemyGrid.ResetAttack();
            this.enemyGrid.SetPossibleTargets(index2, this.playerGrid.Attacker(), this.playerGrid.AttackingCell);
            this.AoE_Reticule = this.createReticule(index2);
        }
    }

    public void Update_PlayerAttack(AttackDamage attackDamage)
    {
        this.playerGrid.AttackDamage = attackDamage;
        this.playerGrid.AttackDamageBase = attackDamage.BaseDamage;
        this.battleMode = BattleMode.PlayerAttack;
    }

    private void SelectAttacker(int _cellNo)
    {
        foreach (UnityEngine.Object @object in this.weaponGO)
            UnityEngine.Object.Destroy(@object);
        this.weaponGO = new List<GameObject>();
        this.playerGrid.SetAttacker(_cellNo);
        UnitEntity unit = this.playerGrid.Cells[_cellNo].Unit;
        this.UnitDetail.gameObject.SetActive(true);
        this.UnitBadgeCtrl.Text.text = GameData.GetText(unit.BattleUnit.name);
        this.UnitBadgeCtrl.HealthBar.UpdateBars(unit);
        if (unit.BattleUnit.weapons != null)
        {
            using (Dictionary<string, Weaponry>.KeyCollection.Enumerator enumerator = unit.BattleUnit.weapons.Keys.GetEnumerator())
            {
                if (enumerator.MoveNext())
                {
                    string current = enumerator.Current;
                    Weaponry weapon = unit.BattleUnit.weapons[current];
                    if (weapon.abilities != null)
                    {
                        if (weapon.abilities.Length != 0)
                            this.weaponGO.Add(this.CreateWeaponTab("WeaponTabSingle", weapon, current));
                    }
                }
            }
        }
        this.RefreshWeaponTabs();
    }

    private void RefreshWeaponTabs()
    {
        if (this.playerGrid.Attacker() == null)
            return;
        foreach (GameObject gameObject in this.weaponGO)
        {
            TabSingleCtrl component1 = gameObject.GetComponent<TabSingleCtrl>();
            if ((UnityEngine.Object)component1 != (UnityEngine.Object)null)
                component1.UpdateTabs(this.playerGrid.Attacker().Unit);
            TabTwoCtrl component2 = gameObject.GetComponent<TabTwoCtrl>();
            if ((UnityEngine.Object)component2 != (UnityEngine.Object)null)
                component2.UpdateTabs(this.playerGrid.Attacker().Unit);
        }
    }

    private GameObject CreateWeaponTab(string tabName, Weaponry weapon, string weaponName)
    {
        int num1 = 0;
        int length = weapon.abilities.Length;
        GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>((GameObject)UnityEngine.Resources.Load(tabName), new Vector3(0.0f, 0.0f, 0.0f), Quaternion.identity);
        TabSingleCtrl component = gameObject.GetComponent<TabSingleCtrl>();
        gameObject.transform.SetParent(this.WeaponList.transform);
        component.WeaponName = weaponName;
        component.OnSelect += new EventHandler<WeaponSelectEventArgs>(this.WeaponTab_OnSelect);
        string[] abilities = weapon.abilities;
        int index1 = 0;
        if (index1 < abilities.Length)
        {
            string index2 = abilities[index1];
            BattleAbilities battleAbility = GameData.BattleAbilities[index2];
            Sprite sprite = UnityEngine.Resources.Load<Sprite>("Icons/" + GameData.NormaliseIconName(battleAbility.icon));
            if ((UnityEngine.Object)sprite == (UnityEngine.Object)null)
                Debug.Log((object)string.Format("Ability icon {0} not found", (object)GameData.NormaliseIconName(battleAbility.icon)));
            component.GetAbilityDetail(num1 + 1).Button.GetComponent<Image>().sprite = sprite;
            if (this.playerGrid.Attacker().Unit.CurrentWeapon == weaponName && this.playerGrid.Attacker().Unit.GetSelectedWeapon().abilities[this.playerGrid.Attacker().Unit.CurrentAbility] == index2)
                component.GetAbilityDetail(num1 + 1).ActiveIcon.SetActive(true);
            else
                component.GetAbilityDetail(num1 + 1).ActiveIcon.SetActive(false);
            component.GetAbilityDetail(num1 + 1).AbilityName = index2;
            int num2 = num1 + 1;
        }
        return gameObject;
    }

    private GameObject CreateWeaponTabTwo(
      string tabName,
      Weaponry weapon,
      string weaponName)
    {
        int num = 0;
        int length = weapon.abilities.Length;
        GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>((GameObject)UnityEngine.Resources.Load(tabName), new Vector3(0.0f, 0.0f, 0.0f), Quaternion.identity);
        TabTwoCtrl component = gameObject.GetComponent<TabTwoCtrl>();
        component.OnSelect += new EventHandler<WeaponSelectEventArgs>(this.WeaponTab_OnSelect);
        gameObject.transform.SetParent(this.WeaponList.transform);
        component.WeaponName = weaponName;
        foreach (string ability in weapon.abilities)
        {
            BattleAbilities battleAbility = GameData.BattleAbilities[ability];
            Sprite sprite = UnityEngine.Resources.Load<Sprite>("Icons/" + GameData.NormaliseIconName(battleAbility.icon));
            if ((UnityEngine.Object)sprite == (UnityEngine.Object)null)
                Debug.Log((object)string.Format("Ability icon {0} not found", (object)GameData.NormaliseIconName(battleAbility.icon)));
            component.GetAbilityDetail(num + 1).Button.GetComponent<Image>().sprite = sprite;
            if (this.playerGrid.Attacker().Unit.CurrentWeapon == weaponName && this.playerGrid.Attacker().Unit.GetSelectedWeapon().abilities[this.playerGrid.Attacker().Unit.CurrentAbility] == ability)
                component.GetAbilityDetail(num + 1).ActiveIcon.SetActive(true);
            else
                component.GetAbilityDetail(num + 1).ActiveIcon.SetActive(false);
            component.GetAbilityDetail(num + 1).AbilityName = ability;
            ++num;
        }
        return gameObject;
    }

    public void WeaponTab_OnSelect(object sender, WeaponSelectEventArgs e)
    {
        this.playerGrid.Attacker().Unit.CurrentWeapon = e.Weapon;
        this.playerGrid.Attacker().Unit.CurrentAbility = Array.IndexOf<string>(this.playerGrid.Attacker().Unit.BattleUnit.weapons[e.Weapon].abilities, e.Ability);
        foreach (GameObject gameObject in this.weaponGO)
        {
            TabSingleCtrl component1 = gameObject.GetComponent<TabSingleCtrl>();
            if ((UnityEngine.Object)component1 != (UnityEngine.Object)null)
            {
                if (component1.WeaponName == e.Weapon && component1.AbilityDetail1.AbilityName == e.Ability)
                    component1.AbilityDetail1.ActiveIcon.SetActive(true);
                else
                    component1.AbilityDetail1.ActiveIcon.SetActive(false);
            }
            TabTwoCtrl component2 = gameObject.GetComponent<TabTwoCtrl>();
            if ((UnityEngine.Object)component2 != (UnityEngine.Object)null)
            {
                if (component2.WeaponName == e.Weapon && component2.AbilityDetail1.AbilityName == e.Ability)
                    component2.AbilityDetail1.ActiveIcon.SetActive(true);
                else
                    component2.AbilityDetail1.ActiveIcon.SetActive(false);
                if (component2.WeaponName == e.Weapon && component2.AbilityDetail2.AbilityName == e.Ability)
                    component2.AbilityDetail2.ActiveIcon.SetActive(true);
                else
                    component2.AbilityDetail2.ActiveIcon.SetActive(false);
            }
        }
        int attackingCell = this.playerGrid.AttackingCell;
        this.playerGrid.ClearHighlights();
        this.playerGrid.ResetAttack();
        this.enemyGrid.ClearHighlights();
        this.enemyGrid.ResetAttack();
        this.playerGrid.SetAttacker(attackingCell);
        this.enemyGrid.SetPossibleTargets(this.playerGrid.AttackingCell, this.playerGrid.Attacker(), attackingCell);
    }

    private GameObject createReticule(int cellNo)
    {
        this.prefab = (GameObject)UnityEngine.Resources.Load("UnitSprite");
        this.prefab.name = "AoE";
        GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(this.prefab, Vector3.zero, Quaternion.identity);
        SpriteAnim component = gameObject.GetComponent<SpriteAnim>();
        component.CellNo = cellNo;
        component.LoadAnimation(this.enemyGrid.Cells[cellNo].Position, "AOE", "aoe_target", true, SpriteType.AOE);
        gameObject.GetComponent<SpriteRenderer>().sortingOrder = 1;
        gameObject.transform.SetParent(this.TopObject.transform, false);
        gameObject.GetComponent<SpriteAnim>().PlayAnimation("AOE", true);
        return gameObject;
    }

    private int GetReticuleCell()
    {
        int num = -1;
        if ((UnityEngine.Object)this.AoE_Reticule != (UnityEngine.Object)null)
            num = this.AoE_Reticule.GetComponent<SpriteAnim>().CellNo;
        return num;
    }

    private void SearchForTargets()
    {
        this.targetCount = 0;
        this.targetListIndex = 0;
        this.delay = this.playerGrid.Attacker().Unit.GetSelectedWeapon().damageAnimationDelay + this.playerGrid.Attacker().Unit.GetSelectedWeapon().firesoundFrame;
        this.playerGrid.Attacker().Unit.HasAttacked = true;
        this.StopCoroutine("ScheduleEnemyDamage");
        if (!this.playerGrid.Attacker().Unit.HasDamageAnimation())
            this.delay += 5;
        this.StartCoroutine("ScheduleEnemyDamage");
    }

    private void SearchForPlayerTargets()
    {
        this.targetCount = 0;
        this.targetListIndex = 0;
        this.delay = this.enemyGrid.Attacker().Unit.GetSelectedWeapon().damageAnimationDelay + this.enemyGrid.Attacker().Unit.GetSelectedWeapon().firesoundFrame;
        this.StopCoroutine("SchedulePlayerDamage");
        if (!this.enemyGrid.Attacker().Unit.HasDamageAnimation())
            this.delay += 5;
        this.StartCoroutine("SchedulePlayerDamage");
    }

    private void EnemyAirstrike()
    {
        this.enemyGrid.Airstrike();
        this.delay = 0;
        this.targetCount = 0;
        this.targetListIndex = 0;
        this.StartCoroutine("ScheduleAirstrike");
    }

    private IEnumerator ScheduleAirstrike()
    {
        BattleMapCtrl battleMapCtrl = this;
        yield return (object)new WaitForSeconds(0.04f * (float)battleMapCtrl.delay);
        if (battleMapCtrl.enemyGrid.TargetList.Count<int>() > 0 && battleMapCtrl.targetCount < battleMapCtrl.enemyGrid.TargetList.Count<int>())
        {
            battleMapCtrl.enemyGrid.Cells[battleMapCtrl.enemyGrid.TargetList[battleMapCtrl.targetCount]].LightUp = HighlightType.Hit;
            battleMapCtrl.PlayAirstrike(battleMapCtrl.enemyGrid.TargetList[battleMapCtrl.targetCount], true);
            ++battleMapCtrl.targetCount;
            battleMapCtrl.delay = battleMapCtrl.targetCount >= battleMapCtrl.enemyGrid.TargetList.Count<int>() ? 0 : (battleMapCtrl.enemyGrid.Cell2Column(battleMapCtrl.enemyGrid.TargetList[battleMapCtrl.targetCount - 1]) == battleMapCtrl.enemyGrid.Cell2Column(battleMapCtrl.enemyGrid.TargetList[battleMapCtrl.targetCount]) ? 0 : 8);
            battleMapCtrl.StartCoroutine(nameof(ScheduleAirstrike));
        }
    }

    private IEnumerator ScheduleEnemyDamage()
    {
        BattleMapCtrl battleMapCtrl = this;
        yield return (object)new WaitForSeconds(0.04f * (float)battleMapCtrl.delay);
        battleMapCtrl.delay = 0;
        AGAbility.TargetSquare[] array = battleMapCtrl.enemyGrid.damagePattern.getOrderedTargetList().ToArray<AGAbility.TargetSquare>();
        if (array.Length != 0 && battleMapCtrl.targetListIndex < array.Length)
        {
            AGAbility.TargetSquare targetSquare = array[battleMapCtrl.targetListIndex];
            int index = !battleMapCtrl.playerGrid.Attacker().Unit.IsDirectFire() ? (!battleMapCtrl.playerGrid.Attacker().Unit.IsAOE() ? (!battleMapCtrl.playerGrid.Attacker().Unit.IsTargetAttack() ? battleMapCtrl.enemyGrid.Relative2Cell(battleMapCtrl.playerGrid.AttackingCell * -1, targetSquare.getX(), targetSquare.getY()) : battleMapCtrl.enemyGrid.Relative2Cell(battleMapCtrl.enemyGrid.GetAbilityTargetCell(battleMapCtrl.playerGrid.AttackingCell), targetSquare.getX(), targetSquare.getY())) : battleMapCtrl.enemyGrid.Relative2Cell(battleMapCtrl.enemyGrid.TargetCell, targetSquare.getX(), targetSquare.getY())) : battleMapCtrl.enemyGrid.TargetCell;
            Debug.Log((object)string.Format("Coord {0},{1} --> {2}", (object)targetSquare.getX(), (object)targetSquare.getY(), (object)index));
            if (index != -1 && battleMapCtrl.enemyGrid.Cells[index].UnitAlive() && (!battleMapCtrl.enemyGrid.IsBlocked(index) || battleMapCtrl.playerGrid.Attacker().Unit.GetSelectedAbility().stats.lineOfFire == 3))
            {
                battleMapCtrl.PlayDamageToEnemy(battleMapCtrl.targetListIndex, index, true);
                if (battleMapCtrl.playerGrid.Attacker().Unit.GetSelectedAbility().stats.targetArea != null)
                    battleMapCtrl.delay = (int)((double)battleMapCtrl.playerGrid.Attacker().Unit.GetSelectedAbility().stats.targetArea.aoeOrderDelay * 20.0);
                ++battleMapCtrl.targetCount;
            }
            ++battleMapCtrl.targetListIndex;
            if (index != -1 && (!battleMapCtrl.enemyGrid.IsBlocked(index) || battleMapCtrl.playerGrid.Attacker().Unit.GetSelectedAbility().stats.lineOfFire == 3))
                battleMapCtrl.enemyGrid.Cells[index].LightUp = HighlightType.Hit;
            battleMapCtrl.StartCoroutine(nameof(ScheduleEnemyDamage));
        }
        else if (battleMapCtrl.targetCount == 0)
            battleMapCtrl.battleMode = BattleMode.WaitForEnemyAttack;
    }

    private IEnumerator SchedulePlayerDamage()
    {
        BattleMapCtrl battleMapCtrl = this;
        yield return (object)new WaitForSeconds(0.04f * (float)battleMapCtrl.delay);
        battleMapCtrl.delay = 0;
        AGAbility.TargetSquare[] array = battleMapCtrl.playerGrid.damagePattern.getOrderedTargetList().ToArray<AGAbility.TargetSquare>();
        if (array.Length != 0 && battleMapCtrl.targetListIndex < array.Length)
        {
            AGAbility.TargetSquare targetSquare = array[battleMapCtrl.targetListIndex];
            int index = !battleMapCtrl.enemyGrid.Attacker().Unit.IsDirectFire() ? (!battleMapCtrl.enemyGrid.Attacker().Unit.IsAOE() ? (!battleMapCtrl.enemyGrid.Attacker().Unit.IsTargetAttack() ? battleMapCtrl.playerGrid.Relative2Cell(battleMapCtrl.enemyGrid.AttackingCell * -1, targetSquare.getX(), targetSquare.getY()) : battleMapCtrl.playerGrid.Relative2Cell(battleMapCtrl.playerGrid.GetAbilityTargetCell(battleMapCtrl.enemyGrid.AttackingCell), targetSquare.getX(), targetSquare.getY())) : battleMapCtrl.playerGrid.Relative2Cell(battleMapCtrl.playerGrid.TargetCell, targetSquare.getX(), targetSquare.getY())) : battleMapCtrl.playerGrid.TargetCell;
            if (index != -1 && battleMapCtrl.playerGrid.Cells[index].UnitAlive() && (!battleMapCtrl.playerGrid.IsBlocked(index) || battleMapCtrl.enemyGrid.Attacker().Unit.GetSelectedAbility().stats.lineOfFire == 3))
            {
                battleMapCtrl.playerGrid.Cells[index].Unit.WasAttacked = true;
                battleMapCtrl.PlayDamageToPlayer(battleMapCtrl.targetListIndex, index, true);
                if (battleMapCtrl.enemyGrid.Attacker().Unit.GetSelectedAbility().stats.targetArea != null)
                    battleMapCtrl.delay = (int)((double)battleMapCtrl.enemyGrid.Attacker().Unit.GetSelectedAbility().stats.targetArea.aoeOrderDelay * 20.0);
                ++battleMapCtrl.targetCount;
            }
            ++battleMapCtrl.targetListIndex;
            if (index != -1 && battleMapCtrl.playerGrid.Cells[index].UnitAlive() && (!battleMapCtrl.playerGrid.IsBlocked(index) || battleMapCtrl.enemyGrid.Attacker().Unit.GetSelectedAbility().stats.lineOfFire == 3))
                battleMapCtrl.playerGrid.Cells[index].LightUp = HighlightType.Hit;
            battleMapCtrl.StartCoroutine(nameof(SchedulePlayerDamage));
        }
        else if (battleMapCtrl.targetCount == 0)
            battleMapCtrl.battleMode = BattleMode.WaitForPlayerAttack;
    }

    private IEnumerator SchedulePlayerDamageOld()
    {
        int num = this.enemyGrid.Attacker().Unit.GetSelectedWeapon().damageAnimationDelay + this.enemyGrid.Attacker().Unit.GetSelectedWeapon().firesoundFrame;
        if (!this.enemyGrid.Attacker().Unit.HasDamageAnimation())
            num += 15;
        yield return (object)new WaitForSeconds(0.03f * (float)num);
        this.PlayDamageToPlayer(0, 0, false);
    }

    private void PlayDamageToEnemy(int damageIndex, int _target, bool hasSound)
    {
        string _animationName = string.Empty;
        string _sound = string.Empty;
        this.prefab = (GameObject)UnityEngine.Resources.Load("UnitSprite");
        this.prefab.name = "Damage sprite";
        GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(this.prefab, Vector3.zero, Quaternion.identity);
        SpriteAnim component = gameObject.GetComponent<SpriteAnim>();
        if (this.playerGrid.Attacker().Unit.HasDamageAnimation())
            _animationName = this.playerGrid.Attacker().Unit.GetFrontDamageAnimation();
        if (hasSound)
            _sound = this.playerGrid.Attacker().Unit.GetSelectedAbility().inf_hitsound;
        component.LoadAnimation(this.enemyGrid.Cells[_target].Position, "Damage", _animationName, false, SpriteType.Damage, 0, _sound);
        component.CellNo = _target;
        component.DamageIndex = damageIndex;
        component.AttackerId = this.playerGrid.AttackingCell;
        component.AttackComplete += new EventHandler(this.enemyCBT);
        this.enemyGrid.Cells[_target].UnitSprite.GetComponent<SpriteAnim>().HitAnimationOn();
        gameObject.GetComponent<SpriteRenderer>().sortingOrder = 3;
        gameObject.transform.SetParent(this.TopObject.transform, false);
        gameObject.GetComponent<SpriteAnim>().PlayAnimation("Damage", false);
    }

    private void PlayAirstrike(int _target, bool hasSound)
    {
        string empty1 = string.Empty;
        string empty2 = string.Empty;
        this.prefab = (GameObject)UnityEngine.Resources.Load("UnitSprite");
        this.prefab.name = "Damage sprite";
        GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(this.prefab, Vector3.zero, Quaternion.identity);
        SpriteAnim component = gameObject.GetComponent<SpriteAnim>();
        string _animationName = "explosion";
        string _sound = "explosionLargeHit.caf";
        component.LoadAnimation(this.enemyGrid.Cells[_target].Position, "Damage", _animationName, false, SpriteType.Damage, 0, _sound);
        component.CellNo = _target;
        component.AttackerId = 0;
        component.AttackComplete += new EventHandler(this.airstrikeCBT);
        gameObject.GetComponent<SpriteRenderer>().sortingOrder = 3;
        gameObject.transform.SetParent(this.TopObject.transform, false);
        gameObject.GetComponent<SpriteAnim>().PlayAnimation("Damage", false);
    }

    public void airstrikeCBT(object sender, EventArgs args)
    {
        SpriteAnim spriteAnim = sender as SpriteAnim;
        GameObject gameObject1 = new GameObject();
        bool flag = false;
        if (!this.enemyGrid.Cells[spriteAnim.CellNo].UnitAlive())
            return;
        Debug.Log((object)string.Format("Airstrike CBT {0}", (object)spriteAnim));
        int _damage = 200;
        if ((double)UnityEngine.Random.Range(1f, 100f) < 5.0)
        {
            flag = true;
            _damage *= 2;
        }
        this.enemyGrid.Cells[spriteAnim.CellNo].Unit.DamageUnit(spriteAnim.CellNo, _damage);
        this.prefab = (GameObject)UnityEngine.Resources.Load("DMT");
        this.prefab.name = "Enemy Damage";
        GameObject gameObject2 = UnityEngine.Object.Instantiate<GameObject>(this.prefab, this.enemyGrid.Cells[spriteAnim.CellNo].Position - new Vector3(25f, 0.0f, 0.0f), Quaternion.identity);
        FloatingNote component = gameObject2.GetComponent<FloatingNote>();
        component.OnComplete += new EventHandler(this.StartEnemyAttack);
        gameObject2.transform.SetParent(this.TopObject.transform);
        gameObject2.transform.position = this.enemyGrid.Cells[spriteAnim.CellNo].Position;
        component.SetText("-" + _damage.ToString());
        if (flag)
        {
            component.SetColour(Color.white);
            component.SetSize(18);
        }
        else
            component.SetColour(Color.red);
        UnityEngine.Object.Destroy((UnityEngine.Object)gameObject2, 3f);
    }

    private void PlayDamageToPlayer(int damageIndex, int _target, bool hasSound)
    {
        string _animationName = string.Empty;
        string _sound = string.Empty;
        this.prefab = (GameObject)UnityEngine.Resources.Load("UnitSprite");
        this.prefab.name = "Damage sprite";
        GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(this.prefab, Vector3.zero, Quaternion.identity);
        SpriteAnim component = gameObject.GetComponent<SpriteAnim>();
        if (this.enemyGrid.Attacker().Unit.HasDamageAnimation())
            _animationName = this.enemyGrid.Attacker().Unit.GetBackDamageAnimation();
        if (hasSound)
            _sound = this.enemyGrid.Attacker().Unit.GetSelectedAbility().inf_hitsound;
        component.LoadAnimation(this.playerGrid.Cells[_target].Position, "Damage", _animationName, false, SpriteType.Damage, 0, _sound);
        component.CellNo = _target;
        component.DamageIndex = damageIndex;
        component.AttackerId = this.enemyGrid.AttackingCell;
        component.AttackComplete += new EventHandler(this.playerCBT);
        this.playerGrid.Cells[_target].UnitSprite.GetComponent<SpriteAnim>().HitAnimationOn();
        gameObject.GetComponent<SpriteRenderer>().sortingOrder = 3;
        gameObject.transform.SetParent(this.TopObject.transform, false);
        gameObject.GetComponent<SpriteAnim>().PlayAnimation("Damage", false);
        UnityEngine.Object.Destroy((UnityEngine.Object)gameObject, 3f);
    }

    public void enemyCBT(object sender, EventArgs args)
    {
        SpriteAnim targetSprite = sender as SpriteAnim;
        int damage = 0;
        bool multipleShots = false;
        TextPosition _textPosition = TextPosition.Centre;
        int num = this.playerGrid.Attacker().Unit.GetSelectedAbility().stats.shotsPerAttack;
        if (num <= 0 || this.enemyGrid.Cells[targetSprite.CellNo].Unit.IsImmune())
            num = 1;
        if (num > 1)
            multipleShots = true;
        for (int index = 1; index <= num; ++index)
        {
            int row = index / 3;
            this.enemyCBT(targetSprite, damage, _textPosition, row, multipleShots, index == num);
            switch (_textPosition)
            {
                case TextPosition.Centre:
                    _textPosition = TextPosition.Right;
                    break;
                case TextPosition.Left:
                    _textPosition = TextPosition.Centre;
                    break;
                case TextPosition.Right:
                    _textPosition = TextPosition.Left;
                    break;
            }
        }
    }

    public void enemyCBT(
      SpriteAnim targetSprite,
      int damage,
      TextPosition _textPosition,
      int row,
      bool multipleShots,
      bool finalShot)
    {
        GameObject gameObject1 = new GameObject();
        bool flag = false;
        if (this.enemyGrid.Cells[targetSprite.CellNo].Unit.IsImmune())
        {
            damage = 0;
        }
        else
        {
            damage = this.playerGrid.AttackDamageBase;
            AGDamagePattern damagePattern = this.enemyGrid.damagePattern;
            if (this.playerGrid.Attacker().Unit.IsAOE() || this.playerGrid.Attacker().Unit.IsDirectFire())
            {
                damage = damagePattern.getTargetSquares()[targetSprite.DamageIndex].getDamage();
            }
            else
            {
                damage = this.playerGrid.Attacker().Unit.BaseDamageAmount();
                damage = new AGAbility(this.playerGrid.Attacker().Unit.Name, this.playerGrid.Attacker().Unit.GetSelectedAbility()).adjustDamage(damage, this.playerGrid.Attacker().Unit.Power());
            }
        }
        if (BattleConfig.IsPvP())
        {
            if (this.playerGrid.AttackDamage.Damage[targetSprite.CellNo].IsCrit)
            {
                flag = true;
                damage *= 2;
            }
        }
        else if ((double)UnityEngine.Random.Range(1f, 100f) < 5.0)
        {
            flag = true;
            damage *= 2;
        }
        this.enemyGrid.Cells[targetSprite.CellNo].Unit.DamageUnit(targetSprite.CellNo, damage);
        this.prefab = (GameObject)UnityEngine.Resources.Load("DMT");
        this.prefab.name = "Enemy Damage";
        GameObject gameObject2 = UnityEngine.Object.Instantiate<GameObject>(this.prefab, this.enemyGrid.Cells[targetSprite.CellNo].Position, Quaternion.identity);
        FloatingNote component = gameObject2.GetComponent<FloatingNote>();
        if (finalShot)
            component.OnComplete += new EventHandler(this.StartEnemyAttack);
        gameObject2.transform.SetParent(this.TopObject.transform);
        gameObject2.transform.position = this.enemyGrid.Cells[targetSprite.CellNo].Position;
        if (this.enemyGrid.Cells[targetSprite.CellNo].Unit.IsImmune())
            component.SetText("IMMUNE");
        else
            component.SetText("-" + damage.ToString());
        component.SetSpeed(0.5f);
        component.SetDuration(1.5f);
        component.SetPosition(_textPosition, row);
        if (flag)
        {
            component.SetColour(Color.white);
            if (multipleShots)
                component.SetSize(8);
            else
                component.SetSize(16);
        }
        else if (this.enemyGrid.Cells[targetSprite.CellNo].Unit.IsImmune())
        {
            component.SetColour(Color.white);
            component.SetSize(10);
        }
        else
        {
            component.SetColour(Color.red);
            if (multipleShots)
                component.SetSize(5);
            else
                component.SetSize(10);
        }
        this.enemyGrid.Cells[targetSprite.CellNo].UnitSprite.GetComponent<SpriteAnim>().HitAnimationOff();
    }

    public void playerCBT(object sender, EventArgs args)
    {
        SpriteAnim targetSprite = sender as SpriteAnim;
        int damage = 0;
        bool multipleShots = false;
        TextPosition _textPosition = TextPosition.Centre;
        int num = this.enemyGrid.Attacker().Unit.GetSelectedAbility().stats.shotsPerAttack;
        if (num <= 0 || this.playerGrid.Cells[targetSprite.CellNo].Unit.IsImmune())
            num = 1;
        if (num > 1)
            multipleShots = true;
        for (int index = 1; index <= num; ++index)
        {
            int row = index / 3;
            this.playerCBT(targetSprite, damage, _textPosition, row, multipleShots, index == num);
            switch (_textPosition)
            {
                case TextPosition.Centre:
                    _textPosition = TextPosition.Right;
                    break;
                case TextPosition.Left:
                    _textPosition = TextPosition.Centre;
                    break;
                case TextPosition.Right:
                    _textPosition = TextPosition.Left;
                    break;
            }
        }
    }

    public void playerCBT(
      SpriteAnim targetSprite,
      int damage,
      TextPosition _textPosition,
      int row,
      bool multipleShots,
      bool finalShot)
    {
        GameObject gameObject1 = new GameObject();
        bool flag = false;
        if (this.playerGrid.Cells[targetSprite.CellNo].Unit.IsImmune())
        {
            damage = 0;
        }
        else
        {
            damage = this.enemyGrid.AttackDamageBase;
            AGDamagePattern damagePattern = this.playerGrid.damagePattern;
            if (this.enemyGrid.Attacker().Unit.IsAOE() || this.enemyGrid.Attacker().Unit.IsDirectFire())
            {
                damage = damagePattern.getTargetSquares()[targetSprite.DamageIndex].getDamage();
            }
            else
            {
                damage = this.enemyGrid.Attacker().Unit.BaseDamageAmount();
                damage = new AGAbility(this.enemyGrid.Attacker().Unit.Name, this.enemyGrid.Attacker().Unit.GetSelectedAbility()).adjustDamage(damage, this.enemyGrid.Attacker().Unit.Power());
            }
        }
        if (BattleConfig.IsPvP())
        {
            if (this.enemyGrid.AttackDamage.Damage[targetSprite.CellNo].IsCrit)
            {
                flag = true;
                damage *= 2;
            }
        }
        else if ((double)UnityEngine.Random.Range(1f, 100f) < 5.0)
        {
            flag = true;
            damage *= 2;
        }
        this.playerGrid.Cells[targetSprite.CellNo].Unit.DamageUnit(targetSprite.CellNo, damage);
        this.prefab = (GameObject)UnityEngine.Resources.Load("DMT");
        this.prefab.name = "Player Damage";
        GameObject gameObject2 = UnityEngine.Object.Instantiate<GameObject>(this.prefab, this.playerGrid.Cells[targetSprite.CellNo].Position, Quaternion.identity);
        FloatingNote component = gameObject2.GetComponent<FloatingNote>();
        if (finalShot)
            component.OnComplete += new EventHandler(this.StartPlayerAttack);
        gameObject2.transform.SetParent(this.TopObject.transform);
        gameObject2.transform.position = this.playerGrid.Cells[targetSprite.CellNo].Position;
        if (this.playerGrid.Cells[targetSprite.CellNo].Unit.IsImmune())
            component.SetText("IMMUNE");
        else
            component.SetText("-" + damage.ToString());
        component.SetSpeed(0.5f);
        component.SetDuration(1.5f);
        component.SetPosition(_textPosition, row);
        if (flag)
        {
            component.SetColour(Color.white);
            if (multipleShots)
                component.SetSize(8);
            else
                component.SetSize(16);
        }
        else if (this.playerGrid.Cells[targetSprite.CellNo].Unit.IsImmune())
        {
            component.SetColour(Color.white);
            component.SetSize(10);
        }
        else
        {
            component.SetColour(Color.red);
            if (multipleShots)
                component.SetSize(5);
            else
                component.SetSize(10);
        }
        this.playerGrid.Cells[targetSprite.CellNo].UnitSprite.GetComponent<SpriteAnim>().HitAnimationOff();
    }

    public void playerCBT_OLD(object sender, EventArgs args)
    {
        SpriteAnim spriteAnim = sender as SpriteAnim;
        bool flag = false;
        if (!this.playerGrid.Cells[spriteAnim.CellNo].UnitAlive())
            return;
        int attackDamageBase = this.enemyGrid.AttackDamageBase;
        AGDamagePattern damagePattern = this.enemyGrid.damagePattern;
        int _damage = this.enemyGrid.Attacker().Unit.IsAOE() || this.enemyGrid.Attacker().Unit.IsDirectFire() ? damagePattern.getTargetSquares()[spriteAnim.DamageIndex].getDamage() : new AGAbility(this.enemyGrid.Attacker().Unit.Name, this.enemyGrid.Attacker().Unit.GetSelectedAbility()).adjustDamage(this.enemyGrid.Attacker().Unit.BaseDamageAmount(), this.enemyGrid.Attacker().Unit.Power());
        if (BattleConfig.IsPvP())
        {
            if (this.enemyGrid.AttackDamage.Damage[spriteAnim.CellNo].IsCrit)
            {
                flag = true;
                _damage *= 2;
            }
        }
        else if ((double)UnityEngine.Random.Range(1f, 100f) < 5.0)
        {
            flag = true;
            _damage *= 2;
        }
        this.playerGrid.Cells[spriteAnim.CellNo].Unit.DamageUnit(spriteAnim.CellNo, _damage);
        this.prefab = (GameObject)UnityEngine.Resources.Load("DMT");
        this.prefab.name = "Enemy Damage";
        GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(this.prefab, this.playerGrid.Cells[spriteAnim.CellNo].Position - new Vector3(25f, 0.0f, 0.0f), Quaternion.identity);
        FloatingNote component = gameObject.GetComponent<FloatingNote>();
        component.OnComplete += new EventHandler(this.StartPlayerAttack);
        gameObject.transform.SetParent(this.TopObject.transform);
        gameObject.transform.position = this.playerGrid.Cells[spriteAnim.CellNo].Position;
        component.SetText("-" + _damage.ToString());
        if (flag)
        {
            component.SetColour(Color.white);
            component.SetSize(18);
        }
        else
            component.SetColour(Color.red);
        double num = (double)((RectTransform)gameObject.transform).rect.width / 2.0;
        this.playerGrid.Cells[spriteAnim.CellNo].UnitSprite.GetComponent<SpriteAnim>().HitAnimationOff();
        UnityEngine.Object.Destroy((UnityEngine.Object)gameObject, 3f);
    }

    public void PlayerDeathComplete(object sender, EventArgs e)
    {
        SpriteAnim spriteAnim = sender as SpriteAnim;
        UnityEngine.Object.Destroy((UnityEngine.Object)this.playerGrid.Cells[spriteAnim.CellNo].UnitSprite);
        UnityEngine.Object.Destroy((UnityEngine.Object)this.playerGrid.Cells[spriteAnim.CellNo].HealthSprite);
    }

    public void StartEnemyAttack(object sender, EventArgs e)
    {
        if (this.targetCount > 0)
        {
            --this.targetCount;
            if (this.targetCount > 0)
                return;
        }
        this.playerGrid.ClearHighlights();
        this.enemyGrid.ClearHighlights();
        this.enemyGrid.ResetAttack();
        if (this.enemyGrid.GetActiveUnitCount() == 0)
        {
            this.battleResult = BattleResult.Victory;
            this.battleMode = BattleMode.Complete;
        }
        else
        {
            if (this.enemyGrid.IsFrontRowEmpty())
            {
                Debug.LogFormat("Start collapse", (object[])Array.Empty<object>());
                this.battleMode = BattleMode.Paused;
                this.enemyGrid.Collapse();
            }
            else
                this.battleMode = BattleMode.WaitForEnemyAttack;
            if (!BattleConfig.IsPvP())
                return;
            this.PvPTurnCtrl.Show(true);
        }
    }

    public void StartPlayerAttack(object sender, EventArgs e)
    {
        this.playerGrid.ClearHighlights();
        this.playerGrid.ResetAttack();
        this.enemyGrid.ClearHighlights();
        this.enemyGrid.ResetAttack();
        ++this.battleTurns;
        if (this.playerGrid.GetActiveUnitCount() == 0)
        {
            this.battleResult = !(BattleConfig.OccupationId == "tutorial-occupation-NEW") ? BattleResult.Defeat : BattleResult.Victory;
            this.battleMode = BattleMode.Complete;
        }
        else
        {
            if (this.playerGrid.IsFrontRowEmpty())
            {
                this.battleMode = BattleMode.Paused;
                this.playerGrid.Collapse();
            }
            else
                this.battleMode = BattleMode.PlayerAttack;
            if (BattleConfig.IsPvP())
                this.PvPTurnCtrl.Show(false);
            if (!(BattleConfig.OccupationId == "tutorial-occupation-NEW") || this.battleTurns <= 5)
                return;
            this.EnemyAirstrike();
        }
    }

    private void Update_EnemyAttack()
    {
        this.enemyGrid.SelectAttacker_Seq();
        this.enemyGrid.Attacker().Unit.CurrentWeapon = "primary";
        this.enemyGrid.Attacker().Unit.CurrentAbility = 0;
        this.playerGrid.SetPossibleTargets(this.enemyGrid.AttackingCell, this.enemyGrid.Attacker(), this.enemyGrid.AttackingCell);
        this.playerGrid.SelectTarget(this.enemyGrid.AttackingCell, this.enemyGrid.Attacker());
        if (this.enemyGrid.Attacker() != null)
        {
            this.playerGrid.SetTarget(this.enemyGrid.Attacker(), this.playerGrid.TargetCell, this.enemyGrid.Attacker().Unit.GetSelectedAbility().stats.targetArea != null);
            this.playerGrid.ClearHighlights();
            this.enemyGrid.Attacker().UnitSprite.GetComponent<SpriteAnim>().PlayAnimation("Attack", false);
            this.SearchForPlayerTargets();
            this.battleMode = BattleMode.EnemyPostAttack;
        }
        else
            this.battleMode = BattleMode.Complete;
    }

    public void Update_EnemyAttack(AttackDamage attackDamage)
    {
        this.enemyGrid.SetAttacker(attackDamage.Source);
        this.enemyGrid.AttackDamage = attackDamage;
        this.enemyGrid.AttackDamageBase = attackDamage.BaseDamage;
        for (int index = 0; index <= this.playerGrid.CellCount() - 1; ++index)
        {
            if (this.playerGrid.Cells[index].UnitAlive() && attackDamage.Damage[index] != null)
            {
                this.playerGrid.Cells[index].LightUp = HighlightType.None;
                if (attackDamage.Damage[index].Type == DamageType.Direct)
                    this.playerGrid.Cells[index].LightUp = HighlightType.Damage;
                if (attackDamage.Damage[index].Type == DamageType.Splash)
                    this.playerGrid.Cells[index].LightUp = HighlightType.SplashDamage;
            }
        }
        this.playerGrid.SetTarget1(attackDamage.Target);
        if (this.enemyGrid.Attacker() != null)
        {
            this.enemyGrid.Attacker().Unit.CurrentWeapon = "primary";
            this.enemyGrid.Attacker().Unit.CurrentAbility = 0;
            this.playerGrid.SetTarget(this.enemyGrid.Attacker(), this.playerGrid.TargetCell, this.enemyGrid.Attacker().Unit.GetSelectedAbility().stats.targetArea != null);
            this.playerGrid.ClearHighlights();
            this.enemyGrid.Attacker().UnitSprite.GetComponent<SpriteAnim>().PlayAnimation("Attack", false);
            this.SearchForPlayerTargets();
            this.battleMode = BattleMode.EnemyPostAttack;
        }
        else
            this.battleMode = BattleMode.Complete;
    }

    public void EnemyAttackComplete(object sender, EventArgs e)
    {
        GameObject gameObject1 = new GameObject();
        int attackDamageBase = this.enemyGrid.AttackDamageBase;
        this.battleMode = BattleMode.EnemyPostAttack;
        this.playerGrid.Cells[this.playerGrid.TargetCell].Unit.DamageUnit(this.playerGrid.TargetCell, attackDamageBase);
        this.prefab = (GameObject)UnityEngine.Resources.Load("CBT");
        this.prefab.name = "Player Damage";
        GameObject gameObject2 = UnityEngine.Object.Instantiate<GameObject>(this.prefab, this.playerGrid.Cells[this.playerGrid.TargetCell].Position - new Vector3(25f, 0.0f, 0.0f), Quaternion.identity);
        gameObject2.GetComponent<AnimCont>().OnComplete += new EventHandler(this.StartPlayerAttack);
        gameObject1.transform.SetParent(this.UIObject.transform, false);
        gameObject1.transform.position = this.playerGrid.Cells[this.playerGrid.TargetCell].Position + new Vector3(-25f, 80f, 0.0f);
        gameObject2.transform.SetParent(gameObject1.transform);
        gameObject2.GetComponent<Text>().text = "-" + attackDamageBase.ToString();
        gameObject2.GetComponent<Animator>().SetTrigger("Hit");
    }

    public void EnemyDeathComplete(object sender, EventArgs e)
    {
        int num1 = 1;
        SpriteAnim spriteAnim = sender as SpriteAnim;
        BattleGrid.GridCell cell = this.enemyGrid.Cells[spriteAnim.CellNo];
        UnitEntity unit = cell.Unit;
        int num2;
        if (unit.BattleUnit.stats[0].rewards.amount.money > 0)
        {
            BattleGrid.GridCell entity = cell;
            num2 = unit.BattleUnit.stats[0].rewards.amount.money;
            string quantity = num2.ToString();
            Sprite sprite = GameData.GetSprite("UI/resource_moneyicon_0");
            int count = num1;
            this.ShowCollectText(entity, quantity, sprite, count, 0.0f);
            ++num1;
        }
        if (unit.BattleUnit.stats[0].rewards.SP > 0)
        {
            BattleGrid.GridCell entity = cell;
            num2 = unit.BattleUnit.stats[0].rewards.SP;
            string quantity = num2.ToString();
            Sprite sprite = GameData.GetSprite("UI/resource_sp@2x");
            int count = num1;
            this.ShowCollectText(entity, quantity, sprite, count, 0.8f);
            int num3 = num1 + 1;
            this.spTotal += unit.BattleUnit.stats[0].rewards.SP;
            if (unit.BattleUnit.stats[0].rewards.amount != null)
                this.moneyTotal += unit.BattleUnit.stats[0].rewards.amount.money;
        }
        UnityEngine.Object.Destroy((UnityEngine.Object)this.enemyGrid.Cells[spriteAnim.CellNo].UnitSprite);
        UnityEngine.Object.Destroy((UnityEngine.Object)this.enemyGrid.Cells[spriteAnim.CellNo].HealthSprite);
    }

    private void ShowCollectText(
      BattleGrid.GridCell entity,
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
        this.MainCamera.WorldToScreenPoint(entity.UnitSprite.transform.position);
        this.MainCamera.WorldToScreenPoint(this.TopObject.transform.position);
        component.Icon.sprite = icon;
        if ((double)shrink > 0.0)
            component.Icon.transform.localScale = new Vector3(shrink, shrink, 0.0f);
        Vector3 position = entity.Position;
        position.z = 0.0f;
        gameObject.transform.position = position + new Vector3(0.1f, 0.5f * (float)count, 0.0f);
        if (!((UnityEngine.Object)component != (UnityEngine.Object)null))
            return;
        component.SetText(quantity);
    }

    public void StartBattle()
    {
        SlideCtrl component1 = GameObject.Find("BottomPanel").GetComponent<SlideCtrl>();
        if ((UnityEngine.Object)component1 != (UnityEngine.Object)null)
            component1.Hide();
        if (this.battleMode == BattleMode.PlayerSetup && (BattleConfig.Type == BattleType.Encounter || BattleConfig.Type == BattleType.Occupation || BattleConfig.Type == BattleType.Training) || this.battleMode == BattleMode.Joined)
        {
            this.playerGrid.ClearHighlights();
            this.playerGrid.ResetAttack();
            this.enemyGrid.ClearHighlights();
            this.enemyGrid.ResetAttack();
            this.battleMode = !BattleConfig.IsPvP() ? BattleMode.PlayerAttack : BattleMode.WaitForEnemyAttack;
            AudioSource component2 = GameObject.Find("BattleMusic").GetComponent<AudioSource>();
            if ((bool)(UnityEngine.Object)component2)
            {
                component2.loop = true;
                component2.Play();
            }
        }
        if ((BattleConfig.Type == BattleType.VsFriend || BattleConfig.Type == BattleType.VsRandom) && this.battleMode == BattleMode.PlayerSetup)
            this.BattleView.JoinBattleQueue(this.playerGrid);
        if (BattleConfig.IsPvP())
            return;
        this.enemyGrid.IsFrontRowEmpty();
    }

    public void OpenWaitForMatchDialog()
    {
        this.waitForMatchCtrl.Show();
    }

    public void BattleQueueJoined()
    {
        this.battleMode = BattleMode.Queued;
        this.OpenWaitForMatchDialog();
    }

    private void WaitForMatch_OnCancel(object sender, EventArgs args)
    {
        this.BattleView.LeaveBattleQueue();
        this.battleMode = BattleMode.PlayerSetup;
        this.fightButtonCtrl.Reset();
    }

    private void WaitForMatch_OnTimeout(object sender, EventArgs args)
    {
        this.BattleView.LeaveBattleQueue();
        this.messageBoxCtrl.Show("Could not find a match. Please try\nagain later.");
        this.battleMode = BattleMode.PlayerSetup;
        this.fightButtonCtrl.Reset();
    }

    private Vector3 GetVert(float dx1, float dy1, Vector3 offset)
    {
        dx1 *= this.scaleRatio * 1f;
        dy1 *= this.scaleRatio * 1f;
        return new Vector3(this.battleTileWidth / 100f * dx1, this.battleTileHeight / 100f * dy1, -1f)
        {
            z = -1f
        };
    }

    private void AddCells()
    {
        Vector3[] vectors = new Vector3[4]
        {
      this.GetVert(-0.5f, 0.0f, this.gridOffset_p),
      this.GetVert(0.0f, 0.5f, this.gridOffset_p),
      this.GetVert(0.5f, 0.0f, this.gridOffset_p),
      this.GetVert(0.0f, -0.5f, this.gridOffset_p)
        };
        foreach (BattleGrid.GridCell gridCell in ((IEnumerable<BattleGrid.GridCell>)this.playerGrid.Cells).Where<BattleGrid.GridCell>((Func<BattleGrid.GridCell, bool>)(x => x.Active)))
        {
            BattleGrid.GridCell cell = gridCell;
            cell.CellSprite = this.CellMesh(cell.Position, vectors);
            this.UpdateCellMeshState(cell);
            cell.OnChangeState += (EventHandler)((_param1, _param2) => this.UpdateCellMeshState(cell));
        }
        foreach (BattleGrid.GridCell gridCell in ((IEnumerable<BattleGrid.GridCell>)this.enemyGrid.Cells).Where<BattleGrid.GridCell>((Func<BattleGrid.GridCell, bool>)(x => x.Active)))
        {
            BattleGrid.GridCell cell = gridCell;
            cell.CellSprite = this.CellMesh(cell.Position, vectors);
            this.UpdateCellMeshState(cell);
            cell.OnChangeState += (EventHandler)((_param1, _param2) => this.UpdateCellMeshState(cell));
        }
    }

    private void UpdateCellMeshState(BattleGrid.GridCell cell)
    {
        cell.CellSprite.GetComponent<MeshGenerator>().SetColour(cell.LightUp);
    }

    private void addPlayerSprite(int _cellNo, string _unitName, int health = 0)
    {
        int _cellNo1 = _cellNo;
        this.prefab = (GameObject)UnityEngine.Resources.Load("UnitSprite");
        this.prefab.name = _unitName;
        UnitEntity unitEntity = new UnitEntity(_unitName);
        unitEntity.CurrentWeapon = "primary";
        unitEntity.CurrentAbility = 0;
        GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(this.prefab, Vector3.zero, Quaternion.identity);
        SpriteAnim component = gameObject.GetComponent<SpriteAnim>();
        component.CellNo = _cellNo1;
        component.LoadAnimation(this.playerGrid.Cells[_cellNo1].Position, "Idle", unitEntity.BattleUnit.backIdleAnimation.ToLower(), true, SpriteType.Idle);
        if (unitEntity.BattleUnit.weapons != null)
        {
            foreach (string key in unitEntity.BattleUnit.weapons.Keys)
            {
                Weaponry weapon = unitEntity.BattleUnit.weapons[key];
                component.LoadAnimation(this.playerGrid.Cells[_cellNo1].Position, key, weapon.backAttackAnimation.ToLower(), false, SpriteType.Attack, weapon.firesoundFrame - 1, weapon.firesound);
            }
        }
        component.LoadAnimation(this.playerGrid.Cells[_cellNo1].Position, "Death", "troopdeath", false, SpriteType.Death, 0, string.Empty);
        component.DeathComplete += new EventHandler(this.PlayerDeathComplete);
        gameObject.GetComponent<SpriteRenderer>().sortingOrder = 2;
        gameObject.transform.SetParent(this.TopObject.transform, false);
        gameObject.GetComponent<SpriteAnim>().PlayAnimation("Idle", true);
        this.playerGrid.Cells[_cellNo1].Unit = unitEntity;
        this.playerGrid.Cells[_cellNo1].Unit.OnFirstDamage += new EventHandler<UnitEventArgs>(this.PlayerCell_OnFirstDamage);
        this.playerGrid.Cells[_cellNo1].Unit.OnDeath += new EventHandler<UnitEventArgs>(this.PlayerCell_OnUnitDeath);
        this.playerGrid.Cells[_cellNo1].UnitSprite = gameObject;
        if (health <= 0)
            return;
        this.playerGrid.Cells[_cellNo1].Unit.SetInitialHealth(_cellNo1, health);
    }

    public void PlayerCell_OnUnitDeath(object sender, UnitEventArgs args)
    {
        int cellNo = args.CellNo;
        this.playerGrid.Cells[cellNo].UnitSprite.GetComponent<SpriteAnim>().PlayAnimation("Death", false);
        if (!((UnityEngine.Object)this.playerGrid.Cells[cellNo].HealthSprite != (UnityEngine.Object)null))
            return;
        UnityEngine.Object.Destroy((UnityEngine.Object)this.playerGrid.Cells[cellNo].HealthSprite);
        this.playerGrid.Cells[cellNo].HealthSprite = (GameObject)null;
    }

    public void PlayerCell_OnFirstDamage(object sender, UnitEventArgs args)
    {
        UnitEntity unit = sender as UnitEntity;
        int cellNo = args.CellNo;
        this.prefab = (GameObject)UnityEngine.Resources.Load("HealthBar_Unit");
        this.playerGrid.Cells[cellNo].HealthSprite = UnityEngine.Object.Instantiate<GameObject>(this.prefab, Vector3.zero, Quaternion.identity);
        this.playerGrid.Cells[cellNo].HealthSprite.transform.SetParent(this.TopObject.transform, false);
        this.playerGrid.Cells[cellNo].HealthSprite.GetComponent<HealthBar>().Init(unit);
        double num = (double)this.playerGrid.Cells[cellNo].HealthSprite.transform.GetComponentInChildren<SpriteRenderer>().bounds.size.x / 2.0;
        this.playerGrid.Cells[cellNo].HealthSprite.transform.position = new Vector3(this.playerGrid.Cells[cellNo].Position.x, this.playerGrid.Cells[cellNo].Position.y - 0.4f);
    }

    private void addEnemySprite(int _cellNo, string _unitName, int health = 0)
    {
        int _cellNo1 = _cellNo < 0 ? this.enemyGrid.GetEmptyCell() : this.deployOrder[_cellNo];
        this.prefab = (GameObject)UnityEngine.Resources.Load("UnitSprite");
        this.prefab.name = _unitName;
        UnitEntity unitEntity = new UnitEntity(_unitName);
        unitEntity.CurrentWeapon = "primary";
        unitEntity.CurrentAbility = 0;
        GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(this.prefab, Vector3.zero, Quaternion.identity);
        SpriteAnim component = gameObject.GetComponent<SpriteAnim>();
        component.CellNo = _cellNo1;
        component.LoadAnimation(this.enemyGrid.Cells[_cellNo1].Position, "Idle", unitEntity.BattleUnit.frontIdleAnimation.ToLower(), true, SpriteType.Idle);
        if (!unitEntity.IsDefensive())
            component.LoadAnimation(this.enemyGrid.Cells[_cellNo1].Position, "Attack", unitEntity.GetSelectedWeapon().frontAttackAnimation.ToLower(), false, SpriteType.Attack, unitEntity.GetSelectedWeapon().firesoundFrame, unitEntity.GetSelectedWeapon().firesound);
        component.LoadAnimation(this.enemyGrid.Cells[_cellNo1].Position, "Death", "troopdeath", false, SpriteType.Death, 8, "");
        component.DeathComplete += new EventHandler(this.EnemyDeathComplete);
        gameObject.GetComponent<SpriteRenderer>().sortingOrder = 2;
        gameObject.transform.SetParent(this.TopObject.transform, true);
        gameObject.GetComponent<SpriteAnim>().PlayAnimation("Idle", true);
        this.enemyGrid.Cells[_cellNo1].Unit = unitEntity;
        this.enemyGrid.Cells[_cellNo1].Unit.OnFirstDamage += new EventHandler<UnitEventArgs>(this.EnemyCell_OnFirstDamage);
        this.enemyGrid.Cells[_cellNo1].Unit.OnDeath += new EventHandler<UnitEventArgs>(this.EnemyCell_OnUnitDeath);
        this.enemyGrid.Cells[_cellNo1].UnitSprite = gameObject;
        if (health <= 0)
            return;
        this.enemyGrid.Cells[_cellNo1].Unit.SetInitialHealth(_cellNo1, health);
    }

    public void EnemyCell_OnUnitDeath(object sender, UnitEventArgs args)
    {
        int cellNo = args.CellNo;
        this.enemyGrid.Cells[cellNo].UnitSprite.GetComponent<SpriteAnim>().PlayAnimation("Death", false);
        if (!((UnityEngine.Object)this.enemyGrid.Cells[cellNo].HealthSprite != (UnityEngine.Object)null))
            return;
        UnityEngine.Object.Destroy((UnityEngine.Object)this.enemyGrid.Cells[cellNo].HealthSprite);
        this.enemyGrid.Cells[cellNo].HealthSprite = (GameObject)null;
    }

    public void EnemyCell_OnFirstDamage(object sender, UnitEventArgs args)
    {
        UnitEntity unit = sender as UnitEntity;
        int cellNo = args.CellNo;
        this.prefab = (GameObject)UnityEngine.Resources.Load("HealthBar_Unit");
        this.enemyGrid.Cells[cellNo].HealthSprite = UnityEngine.Object.Instantiate<GameObject>(this.prefab, Vector3.zero, Quaternion.identity);
        this.enemyGrid.Cells[cellNo].HealthSprite.transform.SetParent(this.TopObject.transform, false);
        this.enemyGrid.Cells[cellNo].HealthSprite.GetComponent<HealthBar>().Init(unit);
        double num = (double)this.enemyGrid.Cells[cellNo].HealthSprite.transform.GetComponentInChildren<SpriteRenderer>().bounds.size.x / 2.0;
        this.enemyGrid.Cells[cellNo].HealthSprite.transform.position = new Vector3(this.enemyGrid.Cells[cellNo].Position.x, this.enemyGrid.Cells[cellNo].Position.y - 0.4f);
    }

    private GameObject CellMesh(Vector3 pos, Vector3[] vectors)
    {
        this.prefab = (GameObject)UnityEngine.Resources.Load("MeshHighlight");
        this.prefab.name = "MeshHighlight";
        Vector3 vector3 = new Vector3(this.battleTileWidth / 2f, this.battleTileHeight / 2f, 10f);
        GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(this.prefab, Vector3.zero, Quaternion.identity);
        MeshGenerator component = gameObject.GetComponent<MeshGenerator>();
        gameObject.transform.SetParent(this.TopObject.transform, false);
        Vector3 position = pos;
        position.z = -1f;
        gameObject.SetActive(true);
        component.Init(position, vectors);
        return gameObject;
    }

    private Vector3 calculatePosition(float dx, float dy, Vector3 offset)
    {
        GameObject gameObject = GameObject.Find("Background");
        Vector3 vector3_1 = this.centre + new Vector3(this.battleTileWidth * dx * this.scaleRatio, this.battleTileHeight * dy * this.scaleRatio) + offset;
        RectTransform transform = gameObject.transform as RectTransform;
        vector3_1 = new Vector3(this.battleTileWidth * dx * this.scaleRatio + transform.rect.center.x, this.battleTileHeight * dy * this.scaleRatio + transform.rect.center.x);
        Vector3 vector3_2 = vector3_1 / 100f;
        vector3_2.y += offset.y;
        vector3_2.x += offset.x;
        vector3_2.z = 0.0f;
        return vector3_2;
    }

    private GameObject createVertexEnemy(float dx1, float dy1, float dx2, float dy2)
    {
        GameObject gameObject1 = GameObject.Find("Background");
        float ratio = gameObject1.GetComponent<BackgroundController>().GetRatio();
        GameObject gameObject2 = UnityEngine.Object.Instantiate<GameObject>((GameObject)UnityEngine.Resources.Load("Gridline"), Vector3.zero, Quaternion.identity);
        gameObject2.name = "EnemyGridLine";
        LineRenderer component = gameObject2.GetComponent<LineRenderer>();
        component.material = this.mat;
        component.startWidth = 0.01f;
        component.endWidth = 0.01f;
        component.startColor = Color.black;
        component.endColor = Color.black;
        component.positionCount = 2;
        component.useWorldSpace = false;
        dx1 *= ratio;
        dy1 *= ratio;
        dx2 *= ratio;
        dy2 *= ratio;
        Vector3 vector3_1 = this.MainCamera.ScreenToWorldPoint(this.centre + new Vector3(this.battleTileWidth * dx1, this.battleTileHeight * dy1) + this.gridOffset_e);
        Vector3 vector3_2 = this.MainCamera.ScreenToWorldPoint(this.centre + new Vector3(this.battleTileWidth * dx2, this.battleTileHeight * dy2) + this.gridOffset_e);
        RectTransform transform = gameObject1.transform as RectTransform;
        vector3_1 = new Vector3(this.battleTileWidth * dx1 + transform.rect.center.x, this.battleTileHeight * dy1 + transform.rect.center.y);
        vector3_2 = new Vector3(this.battleTileWidth * dx2 + transform.rect.center.x, this.battleTileHeight * dy2 + transform.rect.center.y);
        Vector3 position1 = vector3_1 / 100f;
        Vector3 position2 = vector3_2 / 100f;
        position1.y += 1.5f;
        position2.y += 1.5f;
        ++position1.x;
        ++position2.x;
        position1.z = -1f;
        position2.z = -1f;
        component.SetPosition(0, position1);
        component.SetPosition(1, position2);
        return gameObject2;
    }

    private GameObject createVertexPlayer(float dx1, float dy1, float dx2, float dy2)
    {
        GameObject gameObject1 = GameObject.Find("Background");
        float ratio = gameObject1.GetComponent<BackgroundController>().GetRatio();
        GameObject gameObject2 = UnityEngine.Object.Instantiate<GameObject>((GameObject)UnityEngine.Resources.Load("Gridline"), Vector3.zero, Quaternion.identity);
        gameObject2.name = "PlayerGridLine";
        gameObject2.transform.SetParent(gameObject1.transform, true);
        LineRenderer component = gameObject2.GetComponent<LineRenderer>();
        component.material = this.mat;
        component.startWidth = 0.01f;
        component.endWidth = 0.01f;
        component.startColor = Color.black;
        component.endColor = Color.black;
        component.positionCount = 2;
        component.useWorldSpace = false;
        dx1 *= ratio;
        dy1 *= ratio;
        dx2 *= ratio;
        dy2 *= ratio;
        Vector3 vector3_1 = this.MainCamera.ScreenToWorldPoint(this.centre + new Vector3(this.battleTileWidth * dx1, this.battleTileHeight * dy1) + this.gridOffset_p);
        Vector3 vector3_2 = this.MainCamera.ScreenToWorldPoint(this.centre + new Vector3(this.battleTileWidth * dx2, this.battleTileHeight * dy2) + this.gridOffset_p);
        RectTransform transform = gameObject1.transform as RectTransform;
        vector3_1 = new Vector3(this.battleTileWidth * dx1 + transform.rect.center.x, this.battleTileHeight * dy1 + transform.rect.center.y);
        vector3_2 = new Vector3(this.battleTileWidth * dx2 + transform.rect.center.x, this.battleTileHeight * dy2 + transform.rect.center.y);
        Vector3 position1 = vector3_1 / 100f;
        Vector3 position2 = vector3_2 / 100f;
        position1.y += 0.75f;
        position2.y += 0.75f;
        position1.x += -0.5f;
        position2.x += -0.5f;
        position1.z = -1f;
        position2.z = -1f;
        component.SetPosition(0, position1);
        component.SetPosition(1, position2);
        return gameObject2;
    }

    private void drawDebugDots()
    {
        foreach (BattleGrid.GridCell gridCell in ((IEnumerable<BattleGrid.GridCell>)this.enemyGrid.Cells).Where<BattleGrid.GridCell>((Func<BattleGrid.GridCell, bool>)(x => x.Active)))
        {
            LineRenderer lineRenderer = new GameObject().AddComponent<LineRenderer>();
            lineRenderer.material = this.mat;
            lineRenderer.startWidth = 0.08f;
            lineRenderer.endWidth = 0.08f;
            lineRenderer.startColor = Color.green;
            lineRenderer.endColor = Color.green;
            lineRenderer.positionCount = 2;
            Vector3 worldPoint = this.MainCamera.ScreenToWorldPoint(new Vector3(gridCell.Position.x, gridCell.Position.y, gridCell.Position.z));
            Vector3 position1 = new Vector3(worldPoint.x - 0.03f, worldPoint.y - 0.03f, 0.0f);
            Vector3 position2 = new Vector3(worldPoint.x + 0.03f, worldPoint.y + 0.03f, 0.0f);
            lineRenderer.SetPosition(0, position1);
            lineRenderer.SetPosition(1, position2);
        }
        foreach (BattleGrid.GridCell gridCell in ((IEnumerable<BattleGrid.GridCell>)this.playerGrid.Cells).Where<BattleGrid.GridCell>((Func<BattleGrid.GridCell, bool>)(x => x.Active)))
        {
            LineRenderer lineRenderer = new GameObject().AddComponent<LineRenderer>();
            lineRenderer.material = this.mat;
            lineRenderer.startWidth = 0.08f;
            lineRenderer.endWidth = 0.08f;
            lineRenderer.startColor = Color.green;
            lineRenderer.endColor = Color.green;
            lineRenderer.positionCount = 2;
            Vector3 worldPoint = this.MainCamera.ScreenToWorldPoint(new Vector3(gridCell.Position.x, gridCell.Position.y, gridCell.Position.z));
            Vector3 position1 = new Vector3(worldPoint.x - 0.03f, worldPoint.y - 0.03f, 0.0f);
            Vector3 position2 = new Vector3(worldPoint.x + 0.03f, worldPoint.y + 0.03f, 0.0f);
            lineRenderer.SetPosition(0, position1);
            lineRenderer.SetPosition(1, position2);
        }
    }

    private void SaveGame()
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
