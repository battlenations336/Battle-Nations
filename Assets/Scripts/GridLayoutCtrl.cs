using BNR;
using System;
using System.Collections.Generic;
using UnityEngine;
using static BNR.BattleGrid;

public class GridLayoutCtrl : MonoBehaviour
{
    class LinePosition
    {
        public float StartX, StartY;
        public float EndX, EndY;

        public LinePosition(float dx1, float dy1, float dx2, float dy2)
        {
            StartX = dx1;
            StartY = dy1;
            EndX = dx2;
            EndY = dy2;
        }
    }

    public BattleMapType MapType;

    private  BattleGrid.GridCell[] cells;

    //private BattleGrid grid;
    private List<LinePosition> linePositions;
    private Vector3[] cellPositions;
    private List<GameObject> vertexList;
    private float mapScale;
    private float battleTileWidth;
    private float battleTileHeight;

    public int AttackingCell;
    public int AttackDamage;
    int lastAttacker;
    public int TargetCell;
    public List<int> TargetList;
    public List<int> SplashList;
    public List<UnitEntity> RepairList;

    GameObject parent;
    Material mat;
    Vector3 centre;
    Vector3 gridOffset_e;
    Vector3 gridOffset_p;
    float scaleRatio;
    const float lineWidth = 0.01f;

    int hiddenRows;
    bool collapsing;
    float Speed = 0.5f;
    float TransitionOffset;
    float MaxTransitionOffset;

    private void Start()
    {
    }

    private void Update()
    {
        if (collapsing)
        {
            CollapseEnemy();
        }
    }

    #region Properties
    public BattleGrid.GridCell[] Cells
    {
        get { return cells; }
        set { cells = value; }
    }

    #endregion

    private void Init()
    {
        // Could expand this further in the future
        cells = new BattleGrid.GridCell[13];
        cellPositions = new Vector3[13];
        
        TargetList = new List<int>();
        SplashList = new List<int>();

        for (int x = 0; x < 13; x++)
        {
            cells[x] = new BattleGrid.GridCell();
        }

        AttackingCell = -1;

        vertexList = new List<GameObject>();
        linePositions = new List<LinePosition>();

        mapScale = 1.0f;

        battleTileWidth = Constants.DefaultBattleTileWidth * mapScale;
        battleTileHeight = Constants.DefaultBattleTileHeight * mapScale;

        centre = BattleMapCtrl.instance.MainCamera.ViewportToScreenPoint(BattleMapCtrl.instance.MainCamera.rect.center);
        scaleRatio = BattleMapCtrl.instance.TopObject.GetComponent<BackgroundController>().GetRatio();
        gridOffset_e = new Vector3(50, 10) * scaleRatio;
        gridOffset_e = new Vector3(1.0f, 1.5f, 0);
        gridOffset_p = new Vector3(-80, -60) * scaleRatio;
        gridOffset_p = new Vector3(-0.5f, 0.75f, 0);

        if (!mat)
        {
        }

        hiddenRows = 0;
        collapsing = false;

        cellPositions[0] = new Vector3(-1.0f, 1.00f);
        cellPositions[1] = new Vector3(-0.5f, 0.5f);
        cellPositions[2] = new Vector3(0f, 0f);
        cellPositions[3] = new Vector3(0.5f, -0.5f);
        cellPositions[4] = new Vector3(1.0f, -1.0f);

        cellPositions[5] = new Vector3(-0.5f, 1.5f);
        cellPositions[6] = new Vector3(0f, 1.0f);
        cellPositions[7] = new Vector3(0.5f, 0.5f);
        cellPositions[8] = new Vector3(1.0f, 0f);
        cellPositions[9] = new Vector3(1.5f, -0.5f);

        cellPositions[10] = new Vector3(0.5f, 1.5f);
        cellPositions[11] = new Vector3(1.0f, 1.0f);
        cellPositions[12] = new Vector3(1.5f, 0.5f);
    }

    #region Methods

    public bool IsFrontRowEmpty()
    {
        bool result = true;

        if (hiddenRows == 2)
            return (false);

        if (hiddenRows == 1)
        {
            for (int x = 5; x <= 9; x++)
            {
                if (Cells[x].UnitAlive())
                    result = false;
            }
        }

        if (hiddenRows == 0)
        {
            for (int x = 0; x <= 4; x++)
            {
                if (Cells[x].UnitAlive())
                    result = false;
            }
        }

        return (result);
    }

    public int Row(int cellNo)
    {
        int row = 0;

        if (cellNo > 4)
            row = 1;
        if (cellNo > 9)
            row = 2;

        return (row);
    }

    public void Collapse()
    {
        switch (MapType)
        {
            case BattleMapType.ThreeByFive:
                if (hiddenRows == 0)
                {
                    TransitionOffset = 0;
                    MaxTransitionOffset = 0.5f;
                    collapsing = true;
                }
                break;
        }
    }

    void MoveCellUnit(int from, int to)
    {
        Cells[to].Unit = Cells[from].Unit;
        Cells[to].UnitSprite = Cells[from].UnitSprite;
        Cells[to].HealthSprite = Cells[from].HealthSprite;

        Cells[from].Unit = null;
        Cells[from].UnitSprite = null;
        Cells[from].HealthSprite = null;
    }

    void CollapseEnemy()
    {
        bool moved = false;

        float delta = Time.deltaTime * Speed;

        TransitionOffset += delta;
        if (TransitionOffset > MaxTransitionOffset)
        {
            moveVertexVertex(vertexList[5], linePositions[5], -MaxTransitionOffset, false);
            moveVertexVertex(vertexList[8], linePositions[8], -MaxTransitionOffset, false);
            moveVertexVertex(vertexList[9], linePositions[9], -MaxTransitionOffset, false);
            moveVertexVertex(vertexList[1], linePositions[1], -MaxTransitionOffset, true);
            moveVertexVertex(vertexList[2], linePositions[2], -MaxTransitionOffset, true);
            moveVertexVertex(vertexList[3], linePositions[3], -MaxTransitionOffset, true);
            moveVertexVertex(vertexList[4], linePositions[4], -MaxTransitionOffset, true);
            moveVertexVertex(vertexList[6], linePositions[6], -MaxTransitionOffset, true);
            moveVertexVertex(vertexList[7], linePositions[7], -MaxTransitionOffset, true);

            int index = 0;
            foreach(GridCell gridCell in Cells)
            {
                if (gridCell.UnitAlive())
                {
                    SpriteAnim spriteAnim = gridCell.UnitSprite.GetComponent<SpriteAnim>();
                    spriteAnim.NewCellPosition(calculatePosition(cellPositions[index].x - MaxTransitionOffset, cellPositions[index].y - MaxTransitionOffset, gridOffset_e));
                }
                index++;
            }

            MoveCellUnit(5, 0);
            MoveCellUnit(6, 1);
            MoveCellUnit(7, 2);
            MoveCellUnit(8, 3);
            MoveCellUnit(9, 4);

            Cells[5].Active = false;
            Cells[9].Active = false;
            collapsing = false;
            hiddenRows++;
        }
        else
        {
            moveVertexVertex(vertexList[5], linePositions[5], -TransitionOffset, false);
            moveVertexVertex(vertexList[8], linePositions[8], -TransitionOffset, false);
            moveVertexVertex(vertexList[9], linePositions[9], -TransitionOffset, false);
            moveVertexVertex(vertexList[1], linePositions[1], -TransitionOffset, true);
            moveVertexVertex(vertexList[2], linePositions[2], -TransitionOffset, true);
            moveVertexVertex(vertexList[3], linePositions[3], -TransitionOffset, true);
            moveVertexVertex(vertexList[4], linePositions[4], -TransitionOffset, true);
            moveVertexVertex(vertexList[6], linePositions[6], -TransitionOffset, true);
            moveVertexVertex(vertexList[7], linePositions[7], -TransitionOffset, true);

            int index = 0;
            foreach (GridCell gridCell in Cells)
            {
                if (gridCell.UnitAlive())
                {
                    SpriteAnim spriteAnim = gridCell.UnitSprite.GetComponent<SpriteAnim>();
                    spriteAnim.Move(calculatePosition(cellPositions[index].x - TransitionOffset, cellPositions[index].y - TransitionOffset, gridOffset_e));                    
                }
                index++;
            }

        }
    }

    public BattleGrid.GridCell Attacker()
    {
        BattleGrid.GridCell cell = null;

        if (AttackingCell >= 0)
            cell = Cells[AttackingCell];

        return (cell);
    }

    public void UpdateCooldowns()
    {
        for (int x = 0; x <= 12; x++)
        {
            if (cells[x].UnitAlive())
            {
                foreach (string weapKey in cells[x].Unit.WeaponList.Keys)
                {
                    Weapon weap = cells[x].Unit.WeaponList[weapKey];
                    foreach (string abilityKey in weap.AbilityList.Keys)
                    {
                        Ability ability = weap.AbilityList[abilityKey];
                        if (x == AttackingCell && weapKey == cells[x].Unit.CurrentWeapon && abilityKey == cells[x].Unit.GetSelectedWeapon().abilities[cells[x].Unit.CurrentAbility])
                            continue;
                        if (ability.Cooldown > 0)
                            ability.Cooldown--;
                    }
                }
            }
        }
    }

    public BattleGrid.GridCell TargetUnit(int cellNo)
    {
        BattleGrid.GridCell cell = null;

        if (cellNo >= 0 && cellNo <= 12)
            cell = Cells[cellNo];

        return (cell);
    }

    public BattleGrid.GridCell Target()
    {
        BattleGrid.GridCell cell = null;

        if (TargetCell >= 0 && TargetCell <= 12)
            cell = Cells[TargetCell];

        return (cell);
    }

    public BattleGrid.GridCell SelectTarget()
    {
        BattleGrid.GridCell cell = null;
        List<int> targetList = new List<int>();

        TargetCell = -1;

        for (int x = 0; x <= 12; x++)
            if (cells[x].UnitAlive())
                targetList.Add(x);

        if (targetList.Count > 0)
        {
            TargetCell = targetList[UnityEngine.Random.Range(0, targetList.Count)];
            cell = Cells[TargetCell];
            cell.LightUp = HighlightType.Hit;
        }

        return (cell);
    }

    public void SetTarget(int cellNo)
    {
        int[] targetOrder = new int[] { 0, 5, 1, 6, 10, 2, 7, 11, 3, 8, 12, 4, 9 };
        TargetCell = cellNo;
        foreach (int x in targetOrder)
        {
            if (cells[x].LightUp == HighlightType.SplashDamage && cells[x].UnitAlive())
                SplashList.Add(x);
            if (cells[x].LightUp == HighlightType.Damage && cells[x].UnitAlive())
                TargetList.Add(x);
            if (cells[x].LightUp == HighlightType.Hit && cells[x].UnitAlive())
                TargetList.Add(x);
        }
    }

    public void SetTarget1(int target)
    {
        BattleGrid.GridCell cell = null;

        TargetCell = target;
        cell = Cells[TargetCell];
        cell.LightUp = HighlightType.Hit;
    }

    public void SetPossibleTargets(int _cell, GridCell _attacking)
    {
        if (_attacking.Unit.GetSelectedAbility().stats.targetArea == null)
        {
            setInRange(_cell, _attacking);
        }
        else
        {
            setTargetArea(_cell, _attacking);
        }
    }

    void setTargetArea(int _cell, GridCell _attacking)
    {
        //if (AttackingCell < 0 || TargetCell < 0)
        //    return;
        TargetCell = _cell;
        foreach (Area area in _attacking.Unit.GetSelectedAbility().stats.targetArea.data)
        {
            int newTargetX = area.pos.x;
            int newTargetY = area.pos.y;
            int newPos = TargetCell;

            if ((TargetCell == 0 && newTargetX < 0) || (TargetCell == 4 && newTargetX > 0))
                continue;

            //if (newTargetY == -2)
            //    newPos += 5;
            newPos = adjustRow(newPos, newTargetY);

            if (Row(newPos + newTargetX) != Row(newPos))
                continue;

            newPos += newTargetX;
            if (newTargetY == -1 && newPos > 4)
                continue;

            if (newTargetX < 0 && (TargetCell == 5 || TargetCell == 10))
                continue;

            if (newTargetX > 0 && (TargetCell == 4 || TargetCell == 9))
                continue;

            if (newPos >= 0 && newPos <= 12 && cells[newPos].Active)
            {
                //if (newTargetX == 0 && newTargetY == 0)
                //    currentTarget = newPos;
                cells[newPos].LightUp = HighlightType.Damage;
                cells[newPos].Weight = area.weight;
            }
        }

        if (_attacking.Unit.GetSelectedAbility().stats.damageArea != null)
        {
            foreach (Area area in _attacking.Unit.GetSelectedAbility().stats.damageArea.data)
            {
                int newTargetX = area.pos.x;
                int newTargetY = area.pos.y;
                int newPos = TargetCell;

                if ((TargetCell == 0 && newTargetX < 0))
                    continue;

                if (newTargetY != 0)
                    newPos = calcNewEnemyRankPos(newPos, newTargetY);

                newPos += newTargetX;
                if (newTargetX < 0 && (TargetCell == 5 || TargetCell == 10))
                    continue;

                if (newTargetX > 0 && (TargetCell == 4 || TargetCell == 9))
                    continue;

                if (newPos >= 0 && newPos <= 12 && cells[newPos].LightUp == HighlightType.None && cells[newPos].Active)
                {
                    cells[newPos].LightUp = HighlightType.SplashDamage;
                    if (area.weight == 0)
                        cells[newPos].Weight = area.damagePercent;
                    else
                        cells[newPos].Weight = area.weight;
                }
            }
        }
    }

    int adjustRow(int cell, int offset)
    {
        int result = cell;
        int currentRow = Row(cell);

        switch (currentRow)
        {
            case 0:
                if (offset == -2)
                    result += 5;
                if (offset == -3)
                    result += 10;
                break;
            case 1:
                if (offset == -2)
                    result -= 5;
                if (offset == -4)
                    result += 5;
                break;
        }

        return (result);
    }

    int calcNewEnemyRankPos(int _pos, int _rows)
    {
        int result = _pos;
        int x = Math.Abs(_rows);
        int polarity = Math.Sign(_rows);

        if (_pos < 5)
        {
            if (polarity == 1)
            {
                if (x == 1)
                {
                    result += 5;
                }
                else
                {
                    result += 9;
                }
            }
        }
        else if (_pos < 10)
        {
            if (x == 1)
            {
                if (polarity == 1)
                {
                    if (_pos != 5)
                        result += 4;
                }
                else
                {
                    result -= 5;
                }
            }
        }
        else
        {
            if (polarity == -1)
            {
                if (x == 1)
                {
                    result -= 4;
                }
                else
                {
                    result += 9;
                }
            }
        }

        return (result);
    }

    void setInRange(int _cell, GridCell _attacking)
    {
        bool canTarget;

        for (int x = 0; x <= 12; x++)
        {
            canTarget = true;
            if (!cells[x].Active)
                canTarget = false;
            if (_attacking.Unit.GetSelectedAbility().stats.minRange > 0)
            {
                if (Row(_cell) + (Row(x) + 1) < _attacking.Unit.GetSelectedAbility().stats.minRange)
                    canTarget = false;
            }
            if (_attacking.Unit.GetSelectedAbility().stats.maxRange > 0)
            {
                if (Row(_cell) + (Row(x) + 1) > _attacking.Unit.GetSelectedAbility().stats.maxRange)
                    canTarget = false;
            }
            if (canTarget)
                cells[x].LightUp = HighlightType.Selected;
        }
    }

    public GridCell SelectAttacker_Rnd()
    {
        GridCell cell = null;
        List<int> attackList = new List<int>();

        AttackingCell = -1;

        for (int x = 0; x <= 12; x++)
            if (cells[x].UnitAlive())
                attackList.Add(x);

        if (attackList.Count > 0)
        {
            AttackingCell = attackList[UnityEngine.Random.Range(0, attackList.Count)];
            cell = Cells[AttackingCell];
        }

        lastAttacker = AttackingCell;
        return (cell);
    }

    public GridCell SelectAttacker_Seq()
    {
        int choice = -1;
        GridCell cell = null;
        List<int> attackList = new List<int>();

        AttackingCell = -1;

        if (lastAttacker < 12)
            choice = lastAttacker + 1;
        else
            choice = 0;

        for (int x = choice; x <= 12; x++)
            if (cells[x].UnitAlive() && !cells[x].Unit.IsDefensive())
            {
                AttackingCell = x;
                cell = Cells[AttackingCell];
                break;
            }

        if (AttackingCell < 0)
            for (int x = 0; x <= choice; x++)
                if (cells[x].UnitAlive() && !cells[x].Unit.IsDefensive())
                {
                    AttackingCell = x;
                    cell = Cells[AttackingCell];
                    break;
                }

        lastAttacker = AttackingCell;
        return (cell);
    }

    public void SetAttacker(int cellNo)
    {
        if (cells[cellNo].UnitAlive() && !cells[cellNo].Unit.IsDefensive())
        {
            AttackingCell = cellNo;
            AttackDamage = cells[cellNo].Unit.BaseDamageAmount();
            cells[cellNo].LightUp = HighlightType.Selected;
        }
    }

    public int IsCellClicked(Vector3 mousePos, float cellWidth, float cellHeight)
    {
        int result = -1;

        for (int x = 0; x <= 12; x++)
        {
            Vector3 p = cells[x].Position;
            if (underOroverTheLine(new Vector2(p.x - cellWidth / 2, p.y), new Vector2(p.x, p.y - (cellHeight * 0.5f)), mousePos) < 0)
                continue;
            if (underOroverTheLine(new Vector2(p.x, p.y - (cellHeight * 0.5f)), new Vector2(p.x + cellWidth / 2, p.y), mousePos) < 0)
                continue;

            if (underOroverTheLine(new Vector2(p.x, p.y + cellHeight / 2), new Vector2(p.x + cellWidth / 2, p.y), mousePos) > 0)
                continue;
            if (underOroverTheLine(new Vector2(p.x - cellWidth / 2, p.y), new Vector2(p.x, p.y + cellHeight / 2), mousePos) > 0)
                continue;

            result = x;
        }
        return result;
    }

    float underOroverTheLine(Vector2 lineStart, Vector2 lineEnd, Vector3 point)
    {
        float Ax = lineStart.x;
        float Ay = lineStart.y;
        float Bx = lineEnd.x;
        float By = lineEnd.y;

        float Cx = point.x;
        float Cy = point.y;

        return (Bx - Ax) * (Cy - Ay) - (By - Ay) * (Cx - Ax);
    }

    public void ClearHighlights()
    {
        for (int x = 0; x <= 12; x++)
        {
            cells[x].LightUp = HighlightType.None;
        }
    }

    public void ResetAttack()
    {
        AttackingCell = -1;
        TargetList = new List<int>();
        SplashList = new List<int>();
    }

    public int GetActiveUnitCount()
    {
        int result = 0;

        for (int x = 0; x <= 12; x++)
            if (cells[x].UnitAlive() == true)
                result++;

        return (result);
    }

    public int GetNextCell()
    {
        int result = -1;

        for (int x = 0; x <= 12; x++)
        {
            if (Cells[x].Unit == null)
            {
                result = x;
                break;
            }
        }

        return (result);
    }

    public void ReleaseUnits()
    {
        for (int x = 0; x <= 12; x++)
        {
            if (Cells[x].UnitAlive())
                GameData.Player.ReleaseUnit(Cells[x].Unit.Name);
        }
    }
    #endregion

    public void BuildEnemyGrid(GameObject _parent, Material _mat)
    {
        parent = _parent;
        mat = _mat;

        Init();

        switch (MapType)
        {
            case BattleMapType.ThreeByFive:
                linePositions.Add(new LinePosition(-1.5f, 1.0f, 1.0f, -1.5f));
                linePositions.Add(new LinePosition(-1.5f, 1.0f, -0.5f, 2.0f));
                linePositions.Add(new LinePosition(-1.0f, 0.5f, 0.5f, 2.0f));
                linePositions.Add(new LinePosition(0.5f, -1.0f, 2.0f, 0.5f));
                linePositions.Add(new LinePosition(1.0f, -1.5f, 2.0f, -0.5f));
                linePositions.Add(new LinePosition(-1.0f, 1.5f, 1.5f, -1.0f));
                linePositions.Add(new LinePosition(-0.5f, 0f, 1.0f, 1.5f));
                linePositions.Add(new LinePosition(0f, -0.5f, 1.5f, 1.0f));
                linePositions.Add(new LinePosition(-0.5f, 2.0f, 2.0f, -0.5f));
                linePositions.Add(new LinePosition(0.5f, 2.0f, 2.0f, 0.5f));

                Cells[0].Position = calculatePosition(cellPositions[0], gridOffset_e);
                Cells[1].Position = calculatePosition(cellPositions[1], gridOffset_e);
                Cells[2].Position = calculatePosition(cellPositions[2], gridOffset_e);
                Cells[3].Position = calculatePosition(cellPositions[3], gridOffset_e);
                Cells[4].Position = calculatePosition(cellPositions[4], gridOffset_e);

                Cells[5].Position = calculatePosition(cellPositions[5], gridOffset_e);
                Cells[6].Position = calculatePosition(cellPositions[6], gridOffset_e);
                Cells[7].Position = calculatePosition(cellPositions[7], gridOffset_e);
                Cells[8].Position = calculatePosition(cellPositions[8], gridOffset_e);
                Cells[9].Position = calculatePosition(cellPositions[9], gridOffset_e);

                Cells[10].Position = calculatePosition(cellPositions[10], gridOffset_e);
                Cells[11].Position = calculatePosition(cellPositions[11], gridOffset_e);
                Cells[12].Position = calculatePosition(cellPositions[12], gridOffset_e);

                break;
            case BattleMapType.ThreeByThree:
                linePositions.Add(new LinePosition(-1.0f, 0.5f, 0.5f, -1.0f));
                linePositions.Add(new LinePosition(-1.0f, 0.5f, 0.5f, 2.0f));

                linePositions.Add(new LinePosition(-0.5f, 1.0f, 1.0f, -0.5f));
                linePositions.Add(new LinePosition(-0.5f, 0f, 1.0f, 1.5f));

                linePositions.Add(new LinePosition(0.0f, 1.5f, 1.5f, 0f));
                linePositions.Add(new LinePosition(0.0f, -0.5f, 1.5f, 1.0f));

                linePositions.Add(new LinePosition(0.5f, 2.0f, 2.0f, 0.5f));
                linePositions.Add(new LinePosition(0.5f, -1.0f, 2.0f, 0.5f));

                Cells[0].Position = calculatePosition(-0.5f, 0.5f, gridOffset_e);
                Cells[1].Position = calculatePosition(0f, 0f, gridOffset_e);
                Cells[2].Position = calculatePosition(0.5f, -0.5f, gridOffset_e);

                Cells[3].Position = calculatePosition(0f, 1.0f, gridOffset_e);
                Cells[4].Position = calculatePosition(0.5f, 0.5f, gridOffset_e);
                Cells[5].Position = calculatePosition(1.0f, 0f, gridOffset_e);

                Cells[6].Position = calculatePosition(0.5f, 1.5f, gridOffset_e);
                Cells[7].Position = calculatePosition(1.0f, 1.0f, gridOffset_e);
                Cells[8].Position = calculatePosition(1.5f, 0.5f, gridOffset_e);
                break;
        }

        foreach (LinePosition line in linePositions)
            vertexList.Add(createVertexEnemy(line));
    }

    Vector3 calculatePosition(Vector3 dxdy, Vector3 offset)
    {
        return (calculatePosition(dxdy.x, dxdy.y, offset));
    }

    Vector3 calculatePosition(float dx, float dy, Vector3 offset)
    {
        //GameObject top = GameObject.Find("Background");
        Vector3 pos = centre + new Vector3((battleTileWidth * dx * scaleRatio), (battleTileHeight * dy * scaleRatio)) + offset;
        RectTransform rt = parent.transform as RectTransform;
        pos = new Vector3((battleTileWidth * dx * scaleRatio) + rt.rect.center.x, (battleTileHeight * dy * scaleRatio) + rt.rect.center.x);
        pos /= 100f;
        pos.y += offset.y;
        pos.x += offset.x;
        pos.z = 0f;

        return (pos);
    }

//    GameObject createVertexEnemy(float dx1, float dy1, float dx2, float dy2)
    GameObject createVertexEnemy(LinePosition linePosition)
    {
        float dx1, dy1, dx2, dy2;

        GameObject top = GameObject.Find("Background");
        BackgroundController backCtrl = top.GetComponent<BackgroundController>();
        float scale = backCtrl.GetRatio();

        GameObject prefab = (GameObject)Resources.Load("Gridline");
        GameObject newVertex = (GameObject)Instantiate(prefab, Vector3.zero, Quaternion.identity);

        newVertex.name = "EnemyGridLine";

        LineRenderer ln = newVertex.GetComponent<LineRenderer>();
        ln.material = mat;
        ln.startWidth = lineWidth;
        ln.endWidth = lineWidth;
        ln.startColor = Color.black;
        ln.endColor = Color.black;
        ln.positionCount = 2;
        ln.useWorldSpace = false;

        dx1 = linePosition.StartX * scale;
        dy1 = linePosition.StartY * scale;
        dx2 = linePosition.EndX * scale;
        dy2 = linePosition.EndY * scale;

        Vector3 X1 = BattleMapCtrl.instance.MainCamera.ScreenToWorldPoint(centre + new Vector3((battleTileWidth * dx1), (battleTileHeight * dy1)) + gridOffset_e);
        Vector3 X2 = BattleMapCtrl.instance.MainCamera.ScreenToWorldPoint(centre + new Vector3((battleTileWidth * dx2), (battleTileHeight * dy2)) + gridOffset_e);
        RectTransform rt = top.transform as RectTransform;

        X1 = new Vector3((battleTileWidth * dx1) + rt.rect.center.x, (battleTileHeight * dy1) + rt.rect.center.y);// + gridOffset_e;
        X2 = new Vector3((battleTileWidth * dx2) + rt.rect.center.x, (battleTileHeight * dy2) + rt.rect.center.y);// + gridOffset_e;

        X1 /= 100.0f;
        X2 /= 100.0f;

        X1.y += 1.5f;
        X2.y += 1.5f;
        X1.x += 1.0f;
        X2.x += 1.0f;

        X1.z = -1;
        X2.z = -1;

        ln.SetPosition(0, X1);
        ln.SetPosition(1, X2);

        return (newVertex);
    }

    void moveVertexVertex(GameObject vertex, LinePosition linePosition, float offset, bool shrink)
    {
        float dx1 = 0, dx2 = 0, dy1 = 0, dy2 = 0;

        GameObject top = GameObject.Find("Background");
        BackgroundController backCtrl = top.GetComponent<BackgroundController>();

        float scale = backCtrl.GetRatio();

        LineRenderer ln = vertex.GetComponent<LineRenderer>();

        dx1 = (linePosition.StartX + offset) * scale;
        dy1 = (linePosition.StartY + offset) * scale;
        dx2 = (linePosition.EndX + offset) * scale;
        dy2 = (linePosition.EndY + offset) * scale;

        Vector3 X1 = BattleMapCtrl.instance.MainCamera.ScreenToWorldPoint(centre + new Vector3((battleTileWidth * dx1), (battleTileHeight * dy1)) + gridOffset_e);
        Vector3 X2 = BattleMapCtrl.instance.MainCamera.ScreenToWorldPoint(centre + new Vector3((battleTileWidth * dx2), (battleTileHeight * dy2)) + gridOffset_e);
        RectTransform rt = top.transform as RectTransform;

        X1 = new Vector3((battleTileWidth * dx1) + rt.rect.center.x, (battleTileHeight * dy1) + rt.rect.center.y);// + gridOffset_e;
        X2 = new Vector3((battleTileWidth * dx2) + rt.rect.center.x, (battleTileHeight * dy2) + rt.rect.center.y);// + gridOffset_e;

        X1 /= 100.0f;
        X2 /= 100.0f;

        X1.y += 1.5f;
        X2.y += 1.5f;
        X1.x += 1.0f;
        X2.x += 1.0f;

        X1.z = -1;
        X2.z = -1;

        if (!shrink)
            ln.SetPosition(0, X1);
        ln.SetPosition(1, X2);
    }
}

