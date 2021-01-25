
using BNR;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class BattleMapController : MonoBehaviour
{
    private List<GameObject> weaponGO = new List<GameObject>();
    public Camera MainCamera;
    public static BattleMapController instance;
    public GameObject TopObject;
    public SpriteRenderer BattleMap;
    private ModalPanel_BattleResult modalPanel_BattleResult;
    private UnitPanel unitPanel;
    private UnityAction resultOKAction;
    private BattleResult battleResult;
    private const float lineWidth = 0.01f;
    private BattleGrid playerGrid;
    private BattleGrid enemyGrid;
    private int maxOffenceUnits;
    private BattleMapType mapType;
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
    private List<AnimCont> targetList;
    private int currentHomeSelection;
    private float trueScale;
    private float scaleRatio;
    private GameObject prefab;
    public DebugTextController debugWindow;
    public PlayerBadgeController pBadgeController;
    public EnemyBadgeController eBadgeController;
    public UnitBadgeCtrl UnitBadgeCtrl;
    public GameObject WeaponList;
    private MapSize mapSize;
    private string mapName;
    private bool dragAOE;
    private int targetCount;
    private int delay;

    private void Awake()
    {
        if ((UnityEngine.Object)BattleMapController.instance == (UnityEngine.Object)null)
            BattleMapController.instance = this;
        else if ((UnityEngine.Object)BattleMapController.instance != (UnityEngine.Object)this)
            UnityEngine.Object.Destroy((UnityEngine.Object)this.gameObject);
        this.modalPanel_BattleResult = ModalPanel_BattleResult.Instance();
        this.unitPanel = UnitPanel.Instance();
        this.resultOKAction = new UnityAction(this.ExitScreen);
        this.debugWindow = GameObject.Find("DebugText").GetComponent<DebugTextController>();
        this.pBadgeController = GameObject.Find("PlayerBadge").GetComponent<PlayerBadgeController>();
        this.eBadgeController = GameObject.Find("EnemyBadge").GetComponent<EnemyBadgeController>();
    }

    private void init()
    {
        this.pBadgeController.Init(GameData.Player);
        if (BattleConfig.TestId != "tutorial-occupation-NEW")
            this.eBadgeController.Init("Captain Bad Guy", 10);
        else
            this.eBadgeController.gameObject.SetActive(false);
        this.TopObject = GameObject.Find("Background");
        this.UIObject = GameObject.Find("Canvas");
        this.playerGrid = new BattleGrid();
        this.enemyGrid = new BattleGrid();
        this.targetList = new List<AnimCont>();
        this.vertexList = new List<GameObject>();
        this.MainCamera = Camera.main;
        this.centre = this.MainCamera.ViewportToScreenPoint((Vector3)this.MainCamera.rect.center);
        this.scaleRatio = this.TopObject.GetComponent<BackgroundController>().GetRatio();
        this.gridOffset_e = new Vector3(50f, 10f) * this.scaleRatio;
        this.gridOffset_e = new Vector3(1f, 1.5f, 0.0f);
        this.gridOffset_p = new Vector3(-80f, -60f) * this.scaleRatio;
        this.gridOffset_p = new Vector3(-0.5f, 0.75f, 0.0f);
        this.mapType = BattleMapType.ThreeByFive;
        this.maxOffenceUnits = 7;
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
        this.trueScale = 1028f / (float)Screen.width;
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
        this.BuildEnemyGrid();
        this.BuildPlayerGrid();
        this.AddCells();
        if (this.debugDots)
            this.drawDebugDots();
        this.CreateArmy();
        foreach (ArmyUnit armyUnit in GameData.Player.Army.Values)
            this.unitPanel.AddButton(armyUnit.Name, Path.GetFileNameWithoutExtension(armyUnit.GetBattleUnit().icon), armyUnit.AvailableQty(), armyUnit.total, GameData.IsUnitPremium(armyUnit.Name));
        this.battleMode = BattleMode.PlayerSetup;
        this.unitPanel.unitEvent.AddListener(new UnityAction<string>(this.OnUnitSelected));
        this.debugWindow.ShowDebug(true);
    }

    private void CreateArmy()
    {
        if (BattleConfig.MapName != null && BattleConfig.MapName != string.Empty)
            this.BattleMap.sprite = UnityEngine.Resources.Load<Sprite>("Maps/" + BattleConfig.MapName);
        string testId = BattleConfig.TestId;
        if (!(testId == "tutorial-occupation-NEW"))
        {
            if (!(testId == "Test1"))
            {
                if (!(testId == "AC"))
                    return;
                this.addEnemySprite(7, "hero_ancient_robot");
                this.addEnemySprite(4, "def_natural_rocks_small");
                this.addEnemySprite(5, "def_natural_rocks_large");
                this.enemyGrid.AttackingCell = 7;
                this.enemyGrid.Attacker().Unit.CurrentWeapon = "primary";
                this.enemyGrid.Attacker().Unit.CurrentAbility = 0;
                this.eBadgeController.EnemyName.text = "Mutant Killer Dustbin";
            }
            else
            {
                this.addEnemySprite(0, "s_raider_bombadier");
                this.addEnemySprite(1, "s_raider_infantry");
                this.addEnemySprite(2, "s_raider_brawler");
                this.addEnemySprite(3, "s_raider_infantry");
                this.addEnemySprite(4, "s_raider_bombadier");
                this.enemyGrid.AttackingCell = 2;
                this.enemyGrid.Attacker().Unit.CurrentWeapon = "primary";
                this.enemyGrid.Attacker().Unit.CurrentAbility = 0;
                this.eBadgeController.EnemyName.text = "Captain Bad Guy";
            }
        }
        else
            this.loadScriptDriver();
    }

    private void loadScriptDriver()
    {
        int cellNo1 = 0;
        BN_FirstBattleScriptDriver2 battleScriptDriver2 = GameData.LoadBattleScript();
        foreach (UnitLayout unit in battleScriptDriver2.layout.units)
        {
            this.addEnemySprite(cellNo1, unit.unitName);
            ++cellNo1;
        }
        int cellNo2 = 0;
        foreach (UnitLayout unitLayout in battleScriptDriver2.layout.playerUnitLayout)
        {
            this.addPlayerSprite(cellNo2, unitLayout.unitName);
            ++cellNo2;
        }
    }

    private void ShowBattleResult(BattleResult _battleResult)
    {
        this.modalPanel_BattleResult.Show(_battleResult, this.resultOKAction, BattleConfig.EncounterId, 0);
    }

    private void OnUnitSelected(string unitname)
    {
        if (!GameData.Player.CanDeploy(unitname))
            return;
        int nextCell = this.playerGrid.GetNextCell();
        if (nextCell < 0)
            return;
        this.addPlayerSprite(nextCell, unitname);
        GameData.Player.DeployUnit(unitname);
        this.unitPanel.UpdateButton(unitname, GameData.Player.Army[unitname].AvailableQty().ToString());
    }

    private void ExitScreen()
    {
        if (this.battleResult == BattleResult.Victory && BattleConfig.OccupationId != string.Empty)
            GameData.Player.DefeatedOccupations.Add(BattleConfig.OccupationId);
        if (this.battleResult == BattleResult.Victory && BattleConfig.Encounter != null)
            MapConfig.encounterComplete = BattleConfig.Encounter;
        this.playerGrid.ReleaseUnits();
        if (GameData.Player.HomeMap() == "MyLand")
        {
            SceneManager.LoadScene("PlayerMap");
        }
        else
        {
            MapConfig.InitHome();
            SceneManager.LoadScene("WorldMap");
        }
    }

    private void Update()
    {
        switch (this.battleMode)
        {
            case BattleMode.PlayerSetup:
                this.Update_PlayerSetup();
                break;
            case BattleMode.PlayerAttack:
                if (Input.GetKeyUp(KeyCode.W))
                {
                    this.battleResult = BattleResult.Victory;
                    this.ExitScreen();
                    break;
                }
                this.Update_PlayerAttack();
                break;
            case BattleMode.EnemyAttack:
                this.Update_EnemyAttack();
                break;
            case BattleMode.Complete:
                this.Update_Complete();
                break;
            default:
                if (Input.GetKeyUp(KeyCode.Escape))
                {
                    this.ExitScreen();
                    break;
                }
                break;
        }
        if (Input.GetKeyUp(KeyCode.Equals))
            GameData.spriteScale += 0.01f;
        if (!Input.GetKeyUp(KeyCode.Minus))
            return;
        GameData.spriteScale -= 0.01f;
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
        Vector3 zero = Vector3.zero;
        if (Input.GetMouseButtonDown(0) && this.playerGrid.GetActiveUnitCount() < this.maxOffenceUnits)
        {
            int index = this.playerGrid.IsCellClicked(Input.mousePosition, this.battleTileWidth / 100f, this.battleTileHeight / 100f);
            if (index >= 0)
            {
                this.playerGrid.ClearHighlights();
                this.playerGrid.ResetAttack();
                this.playerGrid.Cells[index].LightUp = HighlightType.Hit;
            }
        }
        if (!Input.GetKeyUp(KeyCode.Escape))
            return;
        this.ExitScreen();
    }

    private void Update_PlayerAttack()
    {
        Vector3 zero = Vector3.zero;
        if (Input.GetMouseButtonUp(0))
        {
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
                this.enemyGrid.SetPossibleTargets(index1, this.playerGrid.Attacker());
                if (this.playerGrid.Attacker().Unit.IsAOE())
                    this.AoE_Reticule = this.createReticule(index1);
            }
            if (this.playerGrid.Attacker() != null)
            {
                int index2 = this.enemyGrid.IsCellClicked(worldPoint, this.battleTileWidth / 100f, this.battleTileHeight / 100f);
                if (index2 >= 0 && this.enemyGrid.Cells[index2].UnitAlive() && (this.enemyGrid.Cells[index2].LightUp == HighlightType.Selected || this.enemyGrid.Cells[index2].LightUp == HighlightType.Damage))
                {
                    this.enemyGrid.SetTarget(index2);
                    this.enemyGrid.ClearHighlights();
                    if ((UnityEngine.Object)this.AoE_Reticule != (UnityEngine.Object)null)
                        UnityEngine.Object.Destroy((UnityEngine.Object)this.AoE_Reticule);
                    this.playerGrid.Attacker().UnitSprite.GetComponent<SpriteAnim>().PlayAnimation(this.playerGrid.Attacker().Unit.CurrentWeapon, false);
                    this.SearchForTargets();
                    this.battleMode = BattleMode.PlayerPostAttack;
                    this.playerGrid.Attacker().Unit.ApplyCooldown();
                    this.RefreshWeaponTabs();
                }
                else if (index2 >= 0)
                {
                    if ((UnityEngine.Object)this.AoE_Reticule != (UnityEngine.Object)null)
                        UnityEngine.Object.Destroy((UnityEngine.Object)this.AoE_Reticule);
                    if (this.playerGrid.Attacker().Unit.IsAOE())
                    {
                        this.enemyGrid.ClearHighlights();
                        this.enemyGrid.ResetAttack();
                        this.enemyGrid.SetPossibleTargets(index2, this.playerGrid.Attacker());
                        this.AoE_Reticule = this.createReticule(index2);
                    }
                }
            }
        }
        if (!Input.GetKeyUp(KeyCode.Escape))
            return;
        this.ExitScreen();
    }

    private void SelectAttacker(int _cellNo)
    {
        foreach (UnityEngine.Object @object in this.weaponGO)
            UnityEngine.Object.Destroy(@object);
        this.weaponGO = new List<GameObject>();
        this.playerGrid.SetAttacker(_cellNo);
        UnitEntity unit = this.playerGrid.Cells[_cellNo].Unit;
        this.UnitBadgeCtrl.Text.text = GameData.GetText(unit.BattleUnit.name);
        if (unit.BattleUnit.weapons != null)
        {
            foreach (string key in unit.BattleUnit.weapons.Keys)
            {
                Weaponry weapon = unit.BattleUnit.weapons[key];
                if (weapon.abilities != null && weapon.abilities.Length != 0)
                {
                    if (weapon.abilities.Length == 1)
                        this.weaponGO.Add(this.CreateWeaponTab("WeaponTabSingle", weapon, key));
                    if (weapon.abilities.Length == 2)
                        this.weaponGO.Add(this.CreateWeaponTabTwo("WeaponTabTwo", weapon, key));
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
        int num = 0;
        int length = weapon.abilities.Length;
        GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>((GameObject)UnityEngine.Resources.Load(tabName), new Vector3(0.0f, 0.0f, 0.0f), Quaternion.identity);
        TabSingleCtrl component = gameObject.GetComponent<TabSingleCtrl>();
        gameObject.transform.SetParent(this.WeaponList.transform);
        component.WeaponName = weaponName;
        component.OnSelect += new EventHandler<WeaponSelectEventArgs>(this.WeaponTab_OnSelect);
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
        this.enemyGrid.SetPossibleTargets(this.playerGrid.AttackingCell, this.playerGrid.Attacker());
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

    private void SearchForTargets()
    {
        this.targetCount = 0;
        this.delay = this.playerGrid.Attacker().Unit.GetSelectedWeapon().damageAnimationDelay + this.playerGrid.Attacker().Unit.GetSelectedWeapon().firesoundFrame;
        this.StopCoroutine("ScheduleEnemyDamage");
        if (!this.playerGrid.Attacker().Unit.HasDamageAnimation())
            this.delay += 5;
        this.StartCoroutine("ScheduleEnemyDamage");
    }

    private void SearchForPlayerTargets()
    {
        this.targetCount = 0;
        this.delay = this.enemyGrid.Attacker().Unit.GetSelectedWeapon().damageAnimationDelay + this.enemyGrid.Attacker().Unit.GetSelectedWeapon().firesoundFrame;
        this.StopCoroutine("SchedulePlayerDamage");
        if (!this.enemyGrid.Attacker().Unit.HasDamageAnimation())
            this.delay += 5;
        this.StartCoroutine("SchedulePlayerDamage");
    }

    private IEnumerator ScheduleEnemyDamage()
    {
        BattleMapController battleMapController = this;
        yield return (object)new WaitForSeconds(0.04f * (float)battleMapController.delay);
        if (battleMapController.playerGrid.Attacker().Unit.GetSelectedAbility().stats.targetArea == null)
        {
            Debug.Log((object)string.Format("Damaging {0} {1}", (object)battleMapController.enemyGrid.TargetCell, (object)battleMapController.delay));
            battleMapController.enemyGrid.Cells[battleMapController.enemyGrid.TargetCell].LightUp = HighlightType.Hit;
            battleMapController.PlayDamageToEnemy(battleMapController.enemyGrid.TargetCell, true);
        }
        else if (battleMapController.enemyGrid.TargetList.Count<int>() > 0 && battleMapController.targetCount < battleMapController.enemyGrid.TargetList.Count<int>())
        {
            Debug.Log((object)string.Format("Damaging {0} {1}", (object)battleMapController.enemyGrid.TargetList[battleMapController.targetCount], (object)battleMapController.delay));
            battleMapController.enemyGrid.Cells[battleMapController.enemyGrid.TargetList[battleMapController.targetCount]].LightUp = HighlightType.Hit;
            battleMapController.PlayDamageToEnemy(battleMapController.enemyGrid.TargetList[battleMapController.targetCount], true);
            ++battleMapController.targetCount;
            battleMapController.delay = (int)((double)battleMapController.playerGrid.Attacker().Unit.GetSelectedAbility().stats.targetArea.aoeOrderDelay * 20.0);
            battleMapController.StartCoroutine(nameof(ScheduleEnemyDamage));
        }
        if (battleMapController.playerGrid.Attacker().Unit.GetSelectedAbility().stats.damageArea != null)
        {
            foreach (int splash in battleMapController.enemyGrid.SplashList)
            {
                battleMapController.enemyGrid.Cells[splash].LightUp = HighlightType.Splashed;
                battleMapController.PlayDamageToEnemy(splash, false);
            }
        }
    }

    private IEnumerator SchedulePlayerDamage()
    {
        BattleMapController battleMapController = this;
        yield return (object)new WaitForSeconds(0.04f * (float)battleMapController.delay);
        if (battleMapController.enemyGrid.Attacker().Unit.GetSelectedAbility().stats.targetArea == null)
        {
            Debug.Log((object)string.Format("Damaging {0} {1}", (object)battleMapController.playerGrid.TargetCell, (object)battleMapController.delay));
            battleMapController.playerGrid.Cells[battleMapController.playerGrid.TargetCell].LightUp = HighlightType.Hit;
            battleMapController.PlayDamageToPlayer(battleMapController.playerGrid.TargetCell, true);
        }
        else if (battleMapController.playerGrid.TargetList.Count<int>() > 0 && battleMapController.targetCount < battleMapController.playerGrid.TargetList.Count<int>())
        {
            battleMapController.playerGrid.Cells[battleMapController.playerGrid.TargetList[battleMapController.targetCount]].LightUp = HighlightType.Hit;
            battleMapController.PlayDamageToPlayer(battleMapController.playerGrid.TargetList[battleMapController.targetCount], true);
            ++battleMapController.targetCount;
            battleMapController.delay = (int)((double)battleMapController.enemyGrid.Attacker().Unit.GetSelectedAbility().stats.targetArea.aoeOrderDelay * 20.0);
            battleMapController.StartCoroutine(nameof(SchedulePlayerDamage));
        }
        if (battleMapController.enemyGrid.Attacker().Unit.GetSelectedAbility().stats.damageArea != null)
        {
            foreach (int splash in battleMapController.playerGrid.SplashList)
            {
                battleMapController.playerGrid.Cells[splash].LightUp = HighlightType.Splashed;
                battleMapController.PlayDamageToPlayer(splash, false);
            }
        }
    }

    private IEnumerator SchedulePlayerDamageOld()
    {
        int num = this.enemyGrid.Attacker().Unit.GetSelectedWeapon().damageAnimationDelay + this.enemyGrid.Attacker().Unit.GetSelectedWeapon().firesoundFrame;
        if (!this.enemyGrid.Attacker().Unit.HasDamageAnimation())
            num += 15;
        yield return (object)new WaitForSeconds(0.03f * (float)num);
        this.PlayDamageToPlayer(0, false);
    }

    private void PlayDamageToEnemy(int _target, bool hasSound)
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
        component.AttackerId = this.playerGrid.AttackingCell;
        component.AttackComplete += new EventHandler(this.enemyCBT);
        gameObject.GetComponent<SpriteRenderer>().sortingOrder = 3;
        gameObject.transform.SetParent(this.TopObject.transform, false);
        gameObject.GetComponent<SpriteAnim>().PlayAnimation("Damage", false);
    }

    private void PlayDamageToPlayer(int _target, bool hasSound)
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
        component.AttackerId = this.enemyGrid.AttackingCell;
        component.AttackComplete += new EventHandler(this.playerCBT);
        gameObject.GetComponent<SpriteRenderer>().sortingOrder = 3;
        gameObject.transform.SetParent(this.TopObject.transform, false);
        gameObject.GetComponent<SpriteAnim>().PlayAnimation("Damage", false);
        UnityEngine.Object.Destroy((UnityEngine.Object)gameObject, 3f);
    }

    public void enemyCBT(object sender, EventArgs args)
    {
        SpriteAnim spriteAnim = sender as SpriteAnim;
        GameObject gameObject1 = new GameObject();
        if (!this.enemyGrid.Cells[spriteAnim.CellNo].UnitAlive())
            return;
        int _damage = this.playerGrid.Cells[spriteAnim.AttackerId].Unit.BaseDamageAmount();
        this.enemyGrid.Cells[spriteAnim.CellNo].Unit.DamageUnit(spriteAnim.CellNo, _damage);
        this.prefab = (GameObject)UnityEngine.Resources.Load("DMT");
        this.prefab.name = "Enemy Damage";
        GameObject gameObject2 = UnityEngine.Object.Instantiate<GameObject>(this.prefab, this.enemyGrid.Cells[spriteAnim.CellNo].Position - new Vector3(25f, 0.0f, 0.0f), Quaternion.identity);
        FloatingNote component = gameObject2.GetComponent<FloatingNote>();
        component.OnComplete += new EventHandler(this.StartEnemyAttack);
        gameObject2.transform.SetParent(this.TopObject.transform);
        gameObject2.transform.position = this.enemyGrid.Cells[spriteAnim.CellNo].Position + new Vector3(0.0f, 0.4f, 0.0f);
        component.SetText("-" + _damage.ToString());
        component.SetColour(Color.red);
        UnityEngine.Object.Destroy((UnityEngine.Object)gameObject2, 3f);
    }

    public void playerCBT(object sender, EventArgs args)
    {
        SpriteAnim spriteAnim = sender as SpriteAnim;
        if (!this.playerGrid.Cells[spriteAnim.CellNo].UnitAlive())
            return;
        int _damage = this.enemyGrid.Attacker().Unit.BaseDamageAmount();
        this.playerGrid.Cells[spriteAnim.CellNo].Unit.DamageUnit(spriteAnim.CellNo, _damage);
        this.prefab = (GameObject)UnityEngine.Resources.Load("DMT");
        this.prefab.name = "Enemy Damage";
        GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(this.prefab, this.playerGrid.Cells[spriteAnim.CellNo].Position - new Vector3(25f, 0.0f, 0.0f), Quaternion.identity);
        FloatingNote component = gameObject.GetComponent<FloatingNote>();
        component.OnComplete += new EventHandler(this.StartPlayerAttack);
        gameObject.transform.SetParent(this.TopObject.transform);
        gameObject.transform.position = this.playerGrid.Cells[spriteAnim.CellNo].Position + new Vector3(0.0f, 0.4f, 0.0f);
        component.SetText("-" + _damage.ToString());
        component.SetColour(Color.red);
        double num = (double)((RectTransform)gameObject.transform).rect.width / 2.0;
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
        if (this.targetList != null && this.targetList.Count<AnimCont>() > 0)
        {
            AnimCont animCont = sender as AnimCont;
            Debug.Log((object)string.Format("Cleaning {0}", (object)animCont.cellNo));
            this.targetList.Remove(animCont);
            if (this.targetList.Count<AnimCont>() > 0)
                return;
        }
        this.targetList = new List<AnimCont>();
        this.playerGrid.ClearHighlights();
        this.enemyGrid.ClearHighlights();
        this.enemyGrid.ResetAttack();
        if (this.enemyGrid.GetActiveUnitCount() == 0)
        {
            this.battleResult = BattleResult.Victory;
            this.battleMode = BattleMode.Complete;
        }
        else
            this.battleMode = BattleMode.EnemyAttack;
    }

    public void StartPlayerAttack(object sender, EventArgs e)
    {
        this.playerGrid.UpdateCooldowns();
        this.RefreshWeaponTabs();
        this.targetList = new List<AnimCont>();
        this.playerGrid.ClearHighlights();
        this.playerGrid.ResetAttack();
        this.enemyGrid.ClearHighlights();
        this.enemyGrid.ResetAttack();
        if (this.playerGrid.GetActiveUnitCount() == 0)
        {
            this.battleResult = BattleResult.Defeat;
            this.battleMode = BattleMode.Complete;
        }
        else
            this.battleMode = BattleMode.PlayerAttack;
    }

    private void Update_EnemyAttack()
    {
        this.enemyGrid.SelectAttacker_Seq();
        this.playerGrid.SetPossibleTargets(this.enemyGrid.AttackingCell, this.enemyGrid.Attacker());
        this.playerGrid.SelectTarget();
        if (this.enemyGrid.Attacker() != null)
        {
            this.enemyGrid.Attacker().Unit.CurrentWeapon = "primary";
            this.enemyGrid.Attacker().Unit.CurrentAbility = 0;
            this.playerGrid.SetTarget(this.playerGrid.TargetCell);
            this.playerGrid.ClearHighlights();
            this.enemyGrid.Attacker().UnitSprite.GetComponent<SpriteAnim>().PlayAnimation("Attack", false);
            this.SearchForPlayerTargets();
            this.playerGrid.Attacker().Unit.ApplyCooldown();
            this.battleMode = BattleMode.EnemyPostAttack;
        }
        else
            this.battleMode = BattleMode.Complete;
    }

    public void EnemyAttackComplete(object sender, EventArgs e)
    {
        GameObject gameObject1 = new GameObject();
        int _damage = this.enemyGrid.Attacker().Unit.BaseDamageAmount();
        this.battleMode = BattleMode.EnemyPostAttack;
        this.playerGrid.Cells[this.playerGrid.TargetCell].Unit.DamageUnit(this.playerGrid.TargetCell, _damage);
        this.prefab = (GameObject)UnityEngine.Resources.Load("CBT");
        this.prefab.name = "Player Damage";
        GameObject gameObject2 = UnityEngine.Object.Instantiate<GameObject>(this.prefab, this.playerGrid.Cells[this.playerGrid.TargetCell].Position - new Vector3(25f, 0.0f, 0.0f), Quaternion.identity);
        gameObject2.GetComponent<AnimCont>().OnComplete += new EventHandler(this.StartPlayerAttack);
        gameObject1.transform.SetParent(this.UIObject.transform, false);
        gameObject1.transform.position = this.playerGrid.Cells[this.playerGrid.TargetCell].Position + new Vector3(-25f, 80f, 0.0f);
        gameObject2.transform.SetParent(gameObject1.transform);
        gameObject2.GetComponent<Text>().text = "-" + _damage.ToString();
        gameObject2.GetComponent<Animator>().SetTrigger("Hit");
    }

    public void EnemyDeathComplete(object sender, EventArgs e)
    {
        SpriteAnim spriteAnim = sender as SpriteAnim;
        UnityEngine.Object.Destroy((UnityEngine.Object)this.enemyGrid.Cells[spriteAnim.CellNo].UnitSprite);
        UnityEngine.Object.Destroy((UnityEngine.Object)this.enemyGrid.Cells[spriteAnim.CellNo].HealthSprite);
    }

    public void StartBattle()
    {
        if (this.battleMode != BattleMode.PlayerSetup)
            return;
        this.playerGrid.ClearHighlights();
        this.playerGrid.ResetAttack();
        this.enemyGrid.ClearHighlights();
        this.enemyGrid.ResetAttack();
        this.battleMode = BattleMode.PlayerAttack;
        AudioSource component = GameObject.Find("BattleMusic").GetComponent<AudioSource>();
        if (!(bool)(UnityEngine.Object)component)
            return;
        component.Play();
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

    private void addPlayerSprite(int cellNo, string _unitName)
    {
        this.prefab = (GameObject)UnityEngine.Resources.Load("UnitSprite");
        this.prefab.name = _unitName;
        UnitEntity unit = new UnitEntity(_unitName);
        unit.CurrentWeapon = "primary";
        unit.CurrentAbility = 0;
        GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(this.prefab, Vector3.zero, Quaternion.identity);
        SpriteAnim spriteAnim = gameObject.GetComponent<SpriteAnim>();
        spriteAnim.CellNo = cellNo;
        spriteAnim.LoadAnimation(this.playerGrid.Cells[cellNo].Position, "Idle", unit.BattleUnit.backIdleAnimation.ToLower(), true, SpriteType.Idle);
        foreach (string key in unit.BattleUnit.weapons.Keys)
        {
            Weaponry weapon = unit.BattleUnit.weapons[key];
            spriteAnim.LoadAnimation(this.playerGrid.Cells[cellNo].Position, key, weapon.backAttackAnimation.ToLower(), false, SpriteType.Attack, weapon.firesoundFrame - 1, weapon.firesound);
        }
        spriteAnim.LoadAnimation(this.playerGrid.Cells[cellNo].Position, "Death", "troopdeath", false, SpriteType.Death, 0, string.Empty);
        spriteAnim.DeathComplete += new EventHandler(this.PlayerDeathComplete);
        gameObject.GetComponent<SpriteRenderer>().sortingOrder = 2;
        gameObject.transform.SetParent(this.TopObject.transform, false);
        gameObject.GetComponent<SpriteAnim>().PlayAnimation("Idle", true);
        this.playerGrid.Cells[cellNo].Unit = unit;
        this.playerGrid.Cells[cellNo].Unit.OnFirstDamage += (EventHandler<UnitEventArgs>)((_param1, _param2) =>
        {
            this.prefab = (GameObject)UnityEngine.Resources.Load("HealthBar_Unit");
            this.playerGrid.Cells[cellNo].HealthSprite = UnityEngine.Object.Instantiate<GameObject>(this.prefab, Vector3.zero, Quaternion.identity);
            this.playerGrid.Cells[cellNo].HealthSprite.transform.SetParent(this.TopObject.transform, false);
            this.playerGrid.Cells[cellNo].HealthSprite.GetComponent<HealthBar>().Init(unit);
            double num = (double)this.playerGrid.Cells[cellNo].HealthSprite.transform.GetComponentInChildren<SpriteRenderer>().bounds.size.x / 2.0;
            this.playerGrid.Cells[cellNo].HealthSprite.transform.position = new Vector3(this.playerGrid.Cells[cellNo].Position.x, this.playerGrid.Cells[cellNo].Position.y - 0.4f);
        });
        this.playerGrid.Cells[cellNo].Unit.OnDeath += (EventHandler<UnitEventArgs>)((_param1, _param2) => spriteAnim.PlayAnimation("Death", false));
        this.playerGrid.Cells[cellNo].UnitSprite = gameObject;
    }

    private void addEnemySprite(int cellNo, string _unitName)
    {
        this.prefab = (GameObject)UnityEngine.Resources.Load("UnitSprite");
        this.prefab.name = _unitName;
        UnitEntity unit = new UnitEntity(_unitName);
        unit.CurrentWeapon = "primary";
        unit.CurrentAbility = 0;
        GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(this.prefab, Vector3.zero, Quaternion.identity);
        SpriteAnim spriteAnim = gameObject.GetComponent<SpriteAnim>();
        spriteAnim.CellNo = cellNo;
        spriteAnim.LoadAnimation(this.enemyGrid.Cells[cellNo].Position, "Idle", unit.BattleUnit.frontIdleAnimation.ToLower(), true, SpriteType.Idle);
        if (!unit.IsDefensive())
            spriteAnim.LoadAnimation(this.enemyGrid.Cells[cellNo].Position, "Attack", unit.GetSelectedWeapon().frontAttackAnimation.ToLower(), false, SpriteType.Attack, unit.GetSelectedWeapon().firesoundFrame, unit.GetSelectedWeapon().firesound);
        spriteAnim.LoadAnimation(this.enemyGrid.Cells[cellNo].Position, "Death", "troopdeath", false, SpriteType.Death, 8, "");
        spriteAnim.DeathComplete += new EventHandler(this.EnemyDeathComplete);
        gameObject.GetComponent<SpriteRenderer>().sortingOrder = 2;
        gameObject.transform.SetParent(this.TopObject.transform, true);
        gameObject.GetComponent<SpriteAnim>().PlayAnimation("Idle", true);
        this.enemyGrid.Cells[cellNo].Unit = unit;
        this.enemyGrid.Cells[cellNo].Unit.OnFirstDamage += (EventHandler<UnitEventArgs>)((_param1, _param2) =>
        {
            this.prefab = (GameObject)UnityEngine.Resources.Load("HealthBar_Unit");
            this.enemyGrid.Cells[cellNo].HealthSprite = UnityEngine.Object.Instantiate<GameObject>(this.prefab, Vector3.zero, Quaternion.identity);
            this.enemyGrid.Cells[cellNo].HealthSprite.transform.SetParent(this.TopObject.transform, false);
            this.enemyGrid.Cells[cellNo].HealthSprite.GetComponent<HealthBar>().Init(unit);
            double num = (double)this.enemyGrid.Cells[cellNo].HealthSprite.transform.GetComponentInChildren<SpriteRenderer>().bounds.size.x / 2.0;
            this.enemyGrid.Cells[cellNo].HealthSprite.transform.position = new Vector3(this.enemyGrid.Cells[cellNo].Position.x, this.enemyGrid.Cells[cellNo].Position.y - 0.4f);
        });
        this.enemyGrid.Cells[cellNo].Unit.OnDeath += (EventHandler<UnitEventArgs>)((_param1, _param2) => spriteAnim.PlayAnimation("Death", false));
        this.enemyGrid.Cells[cellNo].UnitSprite = gameObject;
    }

    private GameObject highlightCell(Vector3 pos)
    {
        this.prefab = (GameObject)UnityEngine.Resources.Load("squareSelection");
        this.prefab.name = "Highlight";
        Vector3 vector3_1 = new Vector3(this.battleTileWidth / 2f, this.battleTileHeight / 2f, 10f);
        GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(this.prefab, Vector3.zero, Quaternion.identity);
        Vector3 vector3_2 = pos;
        gameObject.transform.SetParent(this.TopObject.transform, false);
        gameObject.transform.localScale = new Vector3(1f / this.TopObject.transform.localScale.x, 1f / this.TopObject.transform.localScale.y, 1f);
        gameObject.transform.localScale = new Vector3(this.trueScale, this.trueScale, 1f);
        vector3_2.z = -10f;
        Vector3 worldPoint = this.MainCamera.ScreenToWorldPoint(pos);
        worldPoint.z = 0.0f;
        gameObject.transform.position = worldPoint;
        return gameObject;
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

    private void BuildEnemyGrid()
    {
        switch (this.mapType)
        {
            case BattleMapType.ThreeByFive:
                this.vertexList.Add(this.createVertexEnemy(-1.5f, 1f, 1f, -1.5f));
                this.vertexList.Add(this.createVertexEnemy(-1.5f, 1f, -0.5f, 2f));
                this.vertexList.Add(this.createVertexEnemy(-1f, 0.5f, 0.5f, 2f));
                this.vertexList.Add(this.createVertexEnemy(0.5f, -1f, 2f, 0.5f));
                this.vertexList.Add(this.createVertexEnemy(1f, -1.5f, 2f, -0.5f));
                this.vertexList.Add(this.createVertexEnemy(-1f, 1.5f, 1.5f, -1f));
                this.vertexList.Add(this.createVertexEnemy(-0.5f, 0.0f, 1f, 1.5f));
                this.vertexList.Add(this.createVertexEnemy(0.0f, -0.5f, 1.5f, 1f));
                this.vertexList.Add(this.createVertexEnemy(-0.5f, 2f, 2f, -0.5f));
                this.vertexList.Add(this.createVertexEnemy(0.5f, 2f, 2f, 0.5f));
                this.enemyGrid.Cells[0].Position = this.calculatePosition(-1f, 1f, this.gridOffset_e);
                this.enemyGrid.Cells[1].Position = this.calculatePosition(-0.5f, 0.5f, this.gridOffset_e);
                this.enemyGrid.Cells[2].Position = this.calculatePosition(0.0f, 0.0f, this.gridOffset_e);
                this.enemyGrid.Cells[3].Position = this.calculatePosition(0.5f, -0.5f, this.gridOffset_e);
                this.enemyGrid.Cells[4].Position = this.calculatePosition(1f, -1f, this.gridOffset_e);
                this.enemyGrid.Cells[5].Position = this.calculatePosition(-0.5f, 1.5f, this.gridOffset_e);
                this.enemyGrid.Cells[6].Position = this.calculatePosition(0.0f, 1f, this.gridOffset_e);
                this.enemyGrid.Cells[7].Position = this.calculatePosition(0.5f, 0.5f, this.gridOffset_e);
                this.enemyGrid.Cells[8].Position = this.calculatePosition(1f, 0.0f, this.gridOffset_e);
                this.enemyGrid.Cells[9].Position = this.calculatePosition(1.5f, -0.5f, this.gridOffset_e);
                this.enemyGrid.Cells[10].Position = this.calculatePosition(0.5f, 1.5f, this.gridOffset_e);
                this.enemyGrid.Cells[11].Position = this.calculatePosition(1f, 1f, this.gridOffset_e);
                this.enemyGrid.Cells[12].Position = this.calculatePosition(1.5f, 0.5f, this.gridOffset_e);
                break;
        }
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

    private void BuildPlayerGrid()
    {
        switch (this.mapType)
        {
            case BattleMapType.ThreeByFive:
                this.vertexList.Add(this.createVertexPlayer(-1f, 1.5f, 1.5f, -1f));
                this.vertexList.Add(this.createVertexPlayer(-1f, 1.5f, -2f, 0.5f));
                this.vertexList.Add(this.createVertexPlayer(-0.5f, 1f, -2f, -0.5f));
                this.vertexList.Add(this.createVertexPlayer(1f, -0.5f, -0.5f, -2f));
                this.vertexList.Add(this.createVertexPlayer(1.5f, -1f, 0.5f, -2f));
                this.vertexList.Add(this.createVertexPlayer(-1.5f, 1f, 1f, -1.5f));
                this.vertexList.Add(this.createVertexPlayer(0.0f, 0.5f, -1.5f, -1f));
                this.vertexList.Add(this.createVertexPlayer(0.5f, 0.0f, -1f, -1.5f));
                this.vertexList.Add(this.createVertexPlayer(-2f, 0.5f, 0.5f, -2f));
                this.vertexList.Add(this.createVertexPlayer(-2f, -0.5f, -0.5f, -2f));
                this.playerGrid.Cells[0].Position = this.calculatePosition(-1f, 1f, this.gridOffset_p);
                this.playerGrid.Cells[1].Position = this.calculatePosition(-0.5f, 0.5f, this.gridOffset_p);
                this.playerGrid.Cells[2].Position = this.calculatePosition(0.0f, 0.0f, this.gridOffset_p);
                this.playerGrid.Cells[3].Position = this.calculatePosition(0.5f, -0.5f, this.gridOffset_p);
                this.playerGrid.Cells[4].Position = this.calculatePosition(1f, -1f, this.gridOffset_p);
                this.playerGrid.Cells[5].Position = this.calculatePosition(-1.5f, 0.5f, this.gridOffset_p);
                this.playerGrid.Cells[6].Position = this.calculatePosition(-1f, 0.0f, this.gridOffset_p);
                this.playerGrid.Cells[7].Position = this.calculatePosition(-0.5f, -0.5f, this.gridOffset_p);
                this.playerGrid.Cells[8].Position = this.calculatePosition(0.0f, -1f, this.gridOffset_p);
                this.playerGrid.Cells[9].Position = this.calculatePosition(0.5f, -1.5f, this.gridOffset_p);
                this.playerGrid.Cells[10].Position = this.calculatePosition(-1.5f, -0.5f, this.gridOffset_p);
                this.playerGrid.Cells[11].Position = this.calculatePosition(-1f, -1f, this.gridOffset_p);
                this.playerGrid.Cells[12].Position = this.calculatePosition(-0.5f, -1.5f, this.gridOffset_p);
                break;
        }
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
}
