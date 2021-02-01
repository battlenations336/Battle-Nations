
using BNR;
using GameCommon.SerializedObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class GridLayoutCtrl_Base : MonoBehaviour
{
    protected float Speed = 0.5f;
    private BattleGrid.GridCell[] cells;
    protected List<GridLayoutCtrl_Base.LinePosition> linePositions;
    protected Vector3[] cellPositions;
    protected List<GameObject> vertexList;
    protected float mapScale;
    protected float battleTileWidth;
    protected float battleTileHeight;
    public int AttackingCell;
    public int AttackDamageBase;
    public AttackDamage AttackDamage;
    private int lastAttacker;
    public int TargetCell;
    public List<int> TargetList;
    public List<int> SplashList;
    public List<UnitEntity> RepairList;
    public AGDamagePattern damagePattern;
    protected GameObject parent;
    protected Material mat;
    private Vector3 centre;
    private Vector3 gridOffset_e;
    private Vector3 gridOffset_p;
    private float scaleRatio;
    private const float lineWidth = 0.01f;
    protected int hiddenRows;
    protected bool collapsing;
    protected float TransitionOffset;
    protected float MaxTransitionOffset;
    protected float UnitTransitionOffset;
    protected float MaxUnitTransitionOffset;
    public List<Drawable> DamagePattern;

    public EventHandler OnCollapseComplete { get; set; }

    public bool PlayerGrid { get; set; }

    private void Start()
    {
    }

    private void Update()
    {
        if (!this.collapsing)
            return;
        this.CollapseGrid();
    }

    public abstract int CellCount();

    public abstract void InitCellPositions();

    public abstract void InitLinePositions();

    public abstract bool IsFrontRowEmpty();

    public abstract void CollapseGrid();

    public abstract void BuildGrid(GameObject _parent, Material _mat, bool _playerGrid);

    public abstract int Row(int cellNo);

    public abstract void SetTarget(BattleGrid.GridCell attackingUnit, int cellNo, bool isAoE);

    public abstract void SetTargetArea(int _cell, BattleGrid.GridCell _attacking);

    public abstract void SetInRange(int _cell, BattleGrid.GridCell _attacking, int attackingCell);

    public abstract int AdjustRow(int cell, int offset);

    public abstract int calcNewEnemyRankPos(int _pos, int _rows);

    public abstract List<int> AirstrikeOrder();

    public abstract int Cell2Column(int cellNo);

    public abstract int Cell2Row(int cellNo);

    public abstract int Relative2Cell(int targetCell, int x, int y);

    public abstract int Mirror2Cell(int attackingCell, int x, int y);

    public abstract int[] DeployOrder();

    public abstract bool IsBlocked(int cell);

    public abstract int GetAbilityTargetCell(int currentCell);

    public BattleGrid.GridCell[] Cells
    {
        get
        {
            return this.cells;
        }
        set
        {
            this.cells = value;
        }
    }

    protected void Init()
    {
        this.cells = new BattleGrid.GridCell[this.CellCount()];
        this.cellPositions = new Vector3[this.CellCount()];
        this.TargetList = new List<int>();
        this.SplashList = new List<int>();
        for (int index = 0; index < this.CellCount(); ++index)
            this.cells[index] = new BattleGrid.GridCell();
        this.AttackingCell = -1;
        this.vertexList = new List<GameObject>();
        this.linePositions = new List<GridLayoutCtrl_Base.LinePosition>();
        this.mapScale = 1f;
        this.battleTileWidth = 180f * this.mapScale;
        this.battleTileHeight = 90f * this.mapScale;
        this.centre = BattleMapCtrl.instance.MainCamera.ViewportToScreenPoint((Vector3)BattleMapCtrl.instance.MainCamera.rect.center);
        this.scaleRatio = BattleMapCtrl.instance.TopObject.GetComponent<BackgroundController>().GetRatio() * 0.65f;
        this.gridOffset_e = new Vector3(50f, 10f) * this.scaleRatio;
        this.gridOffset_e = new Vector3(1f, 1.5f, 0.0f);
        this.gridOffset_p = new Vector3(-80f, -60f) * this.scaleRatio;
        this.gridOffset_p = new Vector3(-0.5f, 0.75f, 0.0f);
        int num = (bool)(UnityEngine.Object)this.mat ? 1 : 0;
        this.hiddenRows = 0;
        this.collapsing = false;
        this.InitLinePositions();
        this.InitCellPositions();
    }

    public int CollapseDirection()
    {
        return !this.PlayerGrid ? -1 : 1;
    }

    public Vector3 GridOffset()
    {
        return !this.PlayerGrid ? this.gridOffset_e : this.gridOffset_p;
    }

    public void Collapse()
    {
        if (this.hiddenRows == 0)
        {
            this.TransitionOffset = 0.0f;
            this.MaxTransitionOffset = 0.5f;
            this.UnitTransitionOffset = 0.0f;
            this.MaxUnitTransitionOffset = 0.5f;
            this.collapsing = true;
        }
        if (this.hiddenRows != 1)
            return;
        this.TransitionOffset = 0.5f;
        this.MaxTransitionOffset = 1f;
        this.UnitTransitionOffset = 0.0f;
        this.MaxUnitTransitionOffset = 0.5f;
        this.collapsing = true;
    }

    public int GetEmptyCell()
    {
        int num = -1;
        List<int> source = new List<int>();
        for (int index = 0; index < this.Cells.Length; ++index)
        {
            if (this.Cells[index].Active && this.Cells[index].Unit == null)
                source.Add(index);
        }
        if (source.Count<int>() > 0)
        {
            int index = UnityEngine.Random.Range(0, source.Count<int>());
            num = source[index];
        }
        return num;
    }

    public void MoveCellUnit(int from, int to)
    {
        if (this.Cells[from].UnitAlive())
        {
            this.Cells[to].Unit = this.Cells[from].Unit;
            this.Cells[to].UnitSprite = this.Cells[from].UnitSprite;
            this.Cells[to].HealthSprite = this.Cells[from].HealthSprite;
            this.Cells[to].UnitSprite.GetComponent<SpriteAnim>().CellNo = to;
        }
        this.Cells[from].Unit = (UnitEntity)null;
        this.Cells[from].UnitSprite = (GameObject)null;
        this.Cells[from].HealthSprite = (GameObject)null;
    }

    public void Airstrike()
    {
        for (int index1 = 0; index1 < this.Cells.Length; ++index1)
        {
            int index2 = this.AirstrikeOrder()[index1];
            if (this.Cells[index2].UnitAlive() && this.Cells[index2].UnitImportant())
                this.TargetList.Add(index2);
        }
    }

    public BattleGrid.GridCell Attacker()
    {
        BattleGrid.GridCell gridCell = (BattleGrid.GridCell)null;
        if (this.AttackingCell >= 0)
            gridCell = this.Cells[this.AttackingCell];
        return gridCell;
    }

    public int GetTargetCell(int attackingCell)
    {
        int num = -1;
        if (this.Row(attackingCell) == 2)
            num -= 9;
        if (this.Row(attackingCell) == 1)
            num -= 5;
        if (this.Row(attackingCell) == 0)
            num = attackingCell;
        return num;
    }

    public void UpdateCooldowns()
    {
        for (int index = 0; index <= this.CellCount() - 1; ++index)
        {
            if (this.cells[index].UnitAlive())
            {
                foreach (string key1 in this.cells[index].Unit.WeaponList.Keys)
                {
                    Weapon weapon = this.cells[index].Unit.WeaponList[key1];
                    foreach (string key2 in weapon.AbilityList.Keys)
                    {
                        Ability ability = weapon.AbilityList[key2];
                        if ((index != this.AttackingCell || !(key1 == this.cells[index].Unit.CurrentWeapon) || !(key2 == this.cells[index].Unit.GetSelectedWeapon().abilities[this.cells[index].Unit.CurrentAbility])) && ability.Cooldown > 0)
                            --ability.Cooldown;
                    }
                }
            }
        }
    }

    public BattleGrid.GridCell TargetUnit(int cellNo)
    {
        BattleGrid.GridCell gridCell = (BattleGrid.GridCell)null;
        if (cellNo >= 0 && cellNo <= this.CellCount() - 1)
            gridCell = this.Cells[cellNo];
        return gridCell;
    }

    public BattleGrid.GridCell Target()
    {
        BattleGrid.GridCell gridCell = (BattleGrid.GridCell)null;
        if (this.TargetCell >= 0 && this.TargetCell <= this.CellCount() - 1)
            gridCell = this.Cells[this.TargetCell];
        return gridCell;
    }

    public BattleGrid.GridCell SelectTarget(int _cell, BattleGrid.GridCell _attacking)
    {
        BattleGrid.GridCell gridCell = (BattleGrid.GridCell)null;
        List<int> intList = new List<int>();
        this.TargetCell = -1;
        if (_attacking.Unit.IsAOE())
        {
            for (int cell = 0; cell <= this.CellCount() - 1; ++cell)
            {
                if (this.cells[cell].UnitAlive() && (!this.IsBlocked(cell) || _attacking.Unit.GetSelectedAbility().stats.lineOfFire == 3))
                    intList.Add(cell);
            }
        }
        else
        {
            for (int cell = 0; cell <= this.CellCount() - 1; ++cell)
            {
                if (this.cells[cell].UnitAlive() && this.cells[cell].LightUp == HighlightType.Selected && (!this.IsBlocked(cell) || _attacking.Unit.GetSelectedAbility().stats.lineOfFire == 3))
                    intList.Add(cell);
            }
        }
        if (intList.Count > 0)
        {
            this.TargetCell = intList[UnityEngine.Random.Range(0, intList.Count)];
            gridCell = this.Cells[this.TargetCell];
            gridCell.LightUp = HighlightType.Hit;
            this.ClearHighlights();
            this.SetTarget(_attacking, this.TargetCell, false);
            Debug.Log((object)string.Format("Target {0} selected by enemy {1}", (object)this.TargetCell, (object)_cell));
        }
        else
            Debug.Log((object)"Target not selected");
        return gridCell;
    }

    public void SetTarget1(int target)
    {
        this.TargetCell = target;
        this.Cells[this.TargetCell].LightUp = HighlightType.Hit;
    }

    public void SetPossibleTargets(int _cell, BattleGrid.GridCell _attacking, int _attackingCell)
    {
        if (_attacking.Unit.GetSelectedAbility().stats.damageArea == null && (_attacking.Unit.GetSelectedAbility().stats.targetArea == null || ((IEnumerable<Area>)_attacking.Unit.GetSelectedAbility().stats.targetArea.data).Count<Area>() <= 1))
            this.SetInRange(_cell, _attacking, _attackingCell);
        else
            this.SetTargetArea(_cell, _attacking);
    }

    public BattleGrid.GridCell SelectAttacker_Rnd()
    {
        BattleGrid.GridCell gridCell = (BattleGrid.GridCell)null;
        List<int> intList = new List<int>();
        this.AttackingCell = -1;
        for (int index = 0; index <= this.CellCount() - 1; ++index)
        {
            if (this.cells[index].UnitAlive())
                intList.Add(index);
        }
        if (intList.Count > 0)
        {
            this.AttackingCell = intList[UnityEngine.Random.Range(0, intList.Count)];
            gridCell = this.Cells[this.AttackingCell];
        }
        this.lastAttacker = this.AttackingCell;
        this.AttackDamageBase = this.cells[this.AttackingCell].Unit.BaseDamageAmount();
        return gridCell;
    }

    public BattleGrid.GridCell SelectAttacker_Seq()
    {
        BattleGrid.GridCell gridCell = (BattleGrid.GridCell)null;
        List<int> intList = new List<int>();
        this.AttackingCell = -1;
        int num = this.lastAttacker >= this.CellCount() - 1 ? 0 : this.lastAttacker + 1;
        for (int index = num; index <= this.CellCount() - 1; ++index)
        {
            if (this.cells[index].UnitAlive() && !this.cells[index].Unit.IsDefensive())
            {
                this.AttackingCell = index;
                gridCell = this.Cells[this.AttackingCell];
                break;
            }
        }
        if (this.AttackingCell < 0)
        {
            for (int index = 0; index <= num; ++index)
            {
                if (this.cells[index].UnitAlive() && !this.cells[index].Unit.IsDefensive())
                {
                    this.AttackingCell = index;
                    gridCell = this.Cells[this.AttackingCell];
                    break;
                }
            }
        }
        this.lastAttacker = this.AttackingCell;
        this.AttackDamageBase = this.cells[this.AttackingCell].Unit.BaseDamageAmount();
        return gridCell;
    }

    public void SetAttacker(int cellNo)
    {
        if (!this.cells[cellNo].UnitAlive() || this.cells[cellNo].Unit.IsDefensive())
            return;
        this.AttackingCell = cellNo;
        this.AttackDamageBase = this.cells[cellNo].Unit.BaseDamageAmount();
        this.cells[cellNo].LightUp = HighlightType.Selected;
    }

    public int IsCellClicked(Vector3 mousePos, float cellWidth, float cellHeight)
    {
        int num = -1;
        for (int index = 0; index <= this.CellCount() - 1; ++index)
        {
            Vector3 position = this.cells[index].Position;
            if ((double)this.underOroverTheLine(new Vector2(position.x - cellWidth / 2f, position.y), new Vector2(position.x, position.y - cellHeight * 0.5f), mousePos) >= 0.0 && (double)this.underOroverTheLine(new Vector2(position.x, position.y - cellHeight * 0.5f), new Vector2(position.x + cellWidth / 2f, position.y), mousePos) >= 0.0 && ((double)this.underOroverTheLine(new Vector2(position.x, position.y + cellHeight / 2f), new Vector2(position.x + cellWidth / 2f, position.y), mousePos) <= 0.0 && (double)this.underOroverTheLine(new Vector2(position.x - cellWidth / 2f, position.y), new Vector2(position.x, position.y + cellHeight / 2f), mousePos) <= 0.0))
                num = index;
        }
        return num;
    }

    private float underOroverTheLine(Vector2 lineStart, Vector2 lineEnd, Vector3 point)
    {
        float x1 = lineStart.x;
        float y1 = lineStart.y;
        double x2 = (double)lineEnd.x;
        float y2 = lineEnd.y;
        float x3 = point.x;
        float y3 = point.y;
        double num = (double)x1;
        return (float)((x2 - num) * ((double)y3 - (double)y1) - ((double)y2 - (double)y1) * ((double)x3 - (double)x1));
    }

    public void ClearHighlights()
    {
        for (int index = 0; index <= this.CellCount() - 1; ++index)
            this.cells[index].LightUp = HighlightType.None;
    }

    public void ResetAttack()
    {
        this.AttackingCell = -1;
        this.TargetList = new List<int>();
        this.SplashList = new List<int>();
    }

    public int GetActiveUnitCount()
    {
        int num = 0;
        for (int index = 0; index <= this.CellCount() - 1; ++index)
        {
            if (this.cells[index].UnitAlive() && this.cells[index].UnitImportant() && !this.cells[index].Unit.IsImmune())
                ++num;
        }
        return num;
    }

    public int GetAttackUnitCount()
    {
        int num = 0;
        for (int index = 0; index < this.CellCount(); ++index)
        {
            if (this.cells[index].UnitAlive() && !this.cells[index].SystemUnit && !((IEnumerable<string>)this.cells[index].Unit.BattleUnit.tags).Contains<string>("Defense"))
                ++num;
        }
        return num;
    }

    public int GetDefenseUnitCount()
    {
        int num = 0;
        for (int index = 0; index < this.CellCount(); ++index)
        {
            if (this.cells[index].UnitAlive() && !this.cells[index].SystemUnit && ((IEnumerable<string>)this.cells[index].Unit.BattleUnit.tags).Contains<string>("Defense"))
                ++num;
        }
        return num;
    }

    public int GetNextCell()
    {
        int num = -1;
        for (int index = 0; index < this.CellCount(); ++index)
        {
            if (this.Cells[index].Unit == null)
            {
                num = index;
                break;
            }
        }
        return num;
    }

    public void ReleaseUnits()
    {
        for (int index = 0; index <= this.CellCount() - 1; ++index)
        {
            if (this.Cells[index].UnitAlive())
                GameData.Player.ReleaseUnit(this.Cells[index].Unit.Name);
        }
    }

    protected Vector3 calculatePosition(Vector3 dxdy, Vector3 offset)
    {
        return this.calculatePosition(dxdy.x, dxdy.y, offset);
    }

    protected Vector3 calculatePosition(float dx, float dy, Vector3 offset)
    {
        Vector3 vector3_1 = this.centre + new Vector3(this.battleTileWidth * dx * this.scaleRatio, this.battleTileHeight * dy * this.scaleRatio) + offset;
        RectTransform transform = this.parent.transform as RectTransform;
        vector3_1 = new Vector3(this.battleTileWidth * dx * this.scaleRatio + transform.rect.center.x, this.battleTileHeight * dy * this.scaleRatio + transform.rect.center.x);
        Vector3 vector3_2 = vector3_1 / 100f;
        vector3_2.y += offset.y;
        vector3_2.x += offset.x;
        vector3_2.z = 0.0f;
        return vector3_2;
    }

    public int GetSPFactor(string unitName)
    {
        bool flag1 = false;
        bool flag2 = false;
        bool flag3 = false;
        foreach (BattleGrid.GridCell cell in this.Cells)
        {
            if (cell.Unit == null || !(cell.Unit.Name != unitName))
            {
                if (cell.UnitAlive())
                    flag1 = true;
                if (cell.Active && cell.Unit != null && cell.Unit.HasAttacked)
                    flag2 = true;
                if (cell.Active && cell.Unit != null && cell.Unit.WasAttacked)
                    flag3 = true;
            }
        }
        int num = 25;
        if (flag1 && !flag2 && !flag3)
            num = 33;
        if (flag3 && !flag2)
            num = 50;
        if (flag1 & flag2)
            num = 100;
        return num;
    }

    protected GameObject createVertex(GridLayoutCtrl_Base.LinePosition linePosition)
    {
        GameObject gameObject1 = GameObject.Find("Background");
        float num1 = gameObject1.GetComponent<BackgroundController>().GetRatio() * 0.65f;
        GameObject gameObject2 = UnityEngine.Object.Instantiate<GameObject>((GameObject)Resources.Load("Gridline"), Vector3.zero, Quaternion.identity);
        gameObject2.name = "GridLine";
        LineRenderer component = gameObject2.GetComponent<LineRenderer>();
        component.material = this.mat;
        component.startWidth = 0.01f;
        component.endWidth = 0.01f;
        component.startColor = Color.black;
        component.endColor = Color.black;
        component.positionCount = 2;
        component.useWorldSpace = false;
        float num2 = linePosition.StartX * num1;
        float num3 = linePosition.StartY * num1;
        float num4 = linePosition.EndX * num1;
        float num5 = linePosition.EndY * num1;
        Vector3 vector3_1 = BattleMapCtrl.instance.MainCamera.ScreenToWorldPoint(this.centre + new Vector3(this.battleTileWidth * num2, this.battleTileHeight * num3) + this.GridOffset());
        Vector3 vector3_2 = BattleMapCtrl.instance.MainCamera.ScreenToWorldPoint(this.centre + new Vector3(this.battleTileWidth * num4, this.battleTileHeight * num5) + this.GridOffset());
        RectTransform transform = gameObject1.transform as RectTransform;
        vector3_1 = new Vector3(this.battleTileWidth * num2 + transform.rect.center.x, this.battleTileHeight * num3 + transform.rect.center.y);
        vector3_2 = new Vector3(this.battleTileWidth * num4 + transform.rect.center.x, this.battleTileHeight * num5 + transform.rect.center.y);
        Vector3 position1 = vector3_1 / 100f;
        Vector3 position2 = vector3_2 / 100f;
        if (!this.PlayerGrid)
        {
            position1.y += 1.5f;
            position2.y += 1.5f;
            ++position1.x;
            ++position2.x;
        }
        else
        {
            position1.y += 0.75f;
            position2.y += 0.75f;
            position1.x += -0.5f;
            position2.x += -0.5f;
        }
        position1.z = -1f;
        position2.z = -1f;
        component.SetPosition(0, position1);
        component.SetPosition(1, position2);
        return gameObject2;
    }

    protected void moveVertexVertex(
      GameObject vertex,
      GridLayoutCtrl_Base.LinePosition linePosition,
      float offset,
      bool shrink)
    {
        GameObject gameObject = GameObject.Find("Background");
        float num1 = gameObject.GetComponent<BackgroundController>().GetRatio() * 0.65f;
        LineRenderer component = vertex.GetComponent<LineRenderer>();
        float num2 = (linePosition.StartX + offset) * num1;
        float num3 = (linePosition.StartY + offset) * num1;
        float num4 = (linePosition.EndX + offset) * num1;
        float num5 = (linePosition.EndY + offset) * num1;
        Vector3 vector3_1 = BattleMapCtrl.instance.MainCamera.ScreenToWorldPoint(this.centre + new Vector3(this.battleTileWidth * num2, this.battleTileHeight * num3) + this.GridOffset());
        Vector3 vector3_2 = BattleMapCtrl.instance.MainCamera.ScreenToWorldPoint(this.centre + new Vector3(this.battleTileWidth * num4, this.battleTileHeight * num5) + this.GridOffset());
        RectTransform transform = gameObject.transform as RectTransform;
        vector3_1 = new Vector3(this.battleTileWidth * num2 + transform.rect.center.x, this.battleTileHeight * num3 + transform.rect.center.y);
        vector3_2 = new Vector3(this.battleTileWidth * num4 + transform.rect.center.x, this.battleTileHeight * num5 + transform.rect.center.y);
        Vector3 position1 = vector3_1 / 100f;
        Vector3 position2 = vector3_2 / 100f;
        if (!this.PlayerGrid)
        {
            position1.y += 1.5f;
            position2.y += 1.5f;
            ++position1.x;
            ++position2.x;
        }
        else
        {
            position1.y += 0.75f;
            position2.y += 0.75f;
            position1.x += -0.5f;
            position2.x += -0.5f;
        }
        position1.z = -1f;
        position2.z = -1f;
        if (!shrink)
            component.SetPosition(0, position1);
        component.SetPosition(1, position2);
    }

    protected class LinePosition
    {
        public float StartX;
        public float StartY;
        public float EndX;
        public float EndY;

        public LinePosition(float dx1, float dy1, float dx2, float dy2)
        {
            this.StartX = dx1;
            this.StartY = dy1;
            this.EndX = dx2;
            this.EndY = dy2;
        }
    }
}
