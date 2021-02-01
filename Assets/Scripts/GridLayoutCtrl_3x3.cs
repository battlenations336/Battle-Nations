
using BNR;
using System;
using System.Collections.Generic;
using UnityEngine;

public class GridLayoutCtrl_3x3 : GridLayoutCtrl_Base
{
    public override int CellCount()
    {
        return 9;
    }

    public override List<int> AirstrikeOrder()
    {
        return new List<int>() { 0, 3, 6, 1, 4, 7, 2, 5, 8 };
    }

    public override int[] DeployOrder()
    {
        return new int[15]
        {
      0,
      2,
      1,
      0,
      0,
      0,
      5,
      4,
      3,
      0,
      0,
      8,
      7,
      6,
      0
        };
    }

    public override int Relative2Cell(int targetCell, int x, int y)
    {
        int num1 = 3;
        num1 = targetCell;
        int num2 = this.Cell2Row(targetCell);
        int num3 = this.AdjustColumn(Math.Abs(targetCell), x);
        if (num3 == -1)
            return num3;
        switch (num2)
        {
            case -2:
                if (y <= -3)
                    num3 = -1;
                if (y == -2)
                    num3 += 3;
                if (y >= -1)
                {
                    num3 = -1;
                    break;
                }
                break;
            case -1:
                if (y <= -3)
                    num3 = -1;
                if (y == -2)
                {
                    num3 += 3;
                    break;
                }
                break;
            case 0:
                if (y > 0)
                    num3 = -1;
                if (y == -1)
                    num3 += 3;
                if (y == -2)
                {
                    switch (num3)
                    {
                        case 0:
                        case 1:
                        case 2:
                            num3 += 6;
                            break;
                        case 3:
                        case 4:
                        case 5:
                            num3 += 3;
                            break;
                        default:
                            num3 = -1;
                            break;
                    }
                }
                if (y < -2)
                {
                    num3 = -1;
                    break;
                }
                break;
            case 1:
                if (y > 1)
                    num3 = -1;
                if (y == 1)
                    num3 -= 3;
                if (y == -1)
                {
                    if (num3 == 3 || num3 == 4 || num3 == 5)
                        num3 += 3;
                    else
                        num3 = -1;
                }
                if (y < -1)
                {
                    num3 = -1;
                    break;
                }
                break;
            case 2:
                if (y > 2)
                    num3 = -1;
                if (y == 1)
                    num3 -= 3;
                if (y == 2)
                    num3 -= 6;
                if (y < 0)
                {
                    num3 = -1;
                    break;
                }
                break;
        }
        if (num3 < 0 || num3 > 8)
            num3 = -1;
        return num3;
    }

    public int AdjustColumn(int cell, int offset)
    {
        int num1 = -1;
        int num2 = -1;
        int num3 = -1;
        if (cell >= 0 && cell <= 2)
        {
            num2 = 0;
            num3 = 2;
        }
        if (cell >= 3 && cell <= 5)
        {
            num2 = 3;
            num3 = 5;
        }
        if (cell >= 6 && cell <= 8)
        {
            num2 = 6;
            num3 = 8;
        }
        if (cell - offset >= num2 && cell - offset <= num3)
            num1 = cell - offset;
        return num1;
    }

    public override int Mirror2Cell(int attackingCell, int x, int y)
    {
        int num = attackingCell + x + y * 3;
        if (num < 0 || num > 8)
            num = -1;
        return num;
    }

    public override int Cell2Column(int cellNo)
    {
        int num = 0;
        switch (cellNo)
        {
            case 0:
            case 3:
            case 6:
                num = 0;
                break;
            case 1:
            case 4:
            case 7:
                num = 1;
                break;
            case 2:
            case 5:
            case 8:
                num = 0;
                break;
        }
        return num;
    }

    public override int Cell2Row(int cellNo)
    {
        int num = 0;
        switch (cellNo)
        {
            case -4:
            case -3:
            case -2:
            case -1:
                num = -1;
                break;
            case 0:
            case 1:
            case 2:
                num = 0;
                break;
            case 3:
            case 4:
            case 5:
                num = 1;
                break;
            case 6:
            case 7:
            case 8:
                num = 2;
                break;
        }
        return num;
    }

    public override void BuildGrid(GameObject _parent, Material _mat, bool _playerGrid)
    {
        this.parent = _parent;
        this.mat = _mat;
        this.PlayerGrid = _playerGrid;
        this.Init();
        foreach (GridLayoutCtrl_Base.LinePosition linePosition in this.linePositions)
            this.vertexList.Add(this.createVertex(linePosition));
        int index = 0;
        for (int i = 0; i < this.cellPositions.Length; i++)
        {
            Vector3 local = cellPositions[1];
            this.Cells[index].Position = this.calculatePosition(this.cellPositions[index], this.GridOffset());
            ++index;
        }
    }

    public override void InitLinePositions()
    {
        if (!this.PlayerGrid)
        {
            this.linePositions.Add(new GridLayoutCtrl_Base.LinePosition(-1f, 0.5f, 0.5f, -1f));
            this.linePositions.Add(new GridLayoutCtrl_Base.LinePosition(-0.5f, 1f, 1f, -0.5f));
            this.linePositions.Add(new GridLayoutCtrl_Base.LinePosition(0.0f, 1.5f, 1.5f, 0.0f));
            this.linePositions.Add(new GridLayoutCtrl_Base.LinePosition(0.5f, 2f, 2f, 0.5f));
            this.linePositions.Add(new GridLayoutCtrl_Base.LinePosition(-1f, 0.5f, 0.5f, 2f));
            this.linePositions.Add(new GridLayoutCtrl_Base.LinePosition(-0.5f, 0.0f, 1f, 1.5f));
            this.linePositions.Add(new GridLayoutCtrl_Base.LinePosition(0.0f, -0.5f, 1.5f, 1f));
            this.linePositions.Add(new GridLayoutCtrl_Base.LinePosition(0.5f, -1f, 2f, 0.5f));
        }
        else
        {
            this.linePositions.Add(new GridLayoutCtrl_Base.LinePosition(-0.5f, 1f, 1f, -0.5f));
            this.linePositions.Add(new GridLayoutCtrl_Base.LinePosition(-1f, 0.5f, 0.5f, -1f));
            this.linePositions.Add(new GridLayoutCtrl_Base.LinePosition(-1.5f, 0.0f, 0.0f, -1.5f));
            this.linePositions.Add(new GridLayoutCtrl_Base.LinePosition(-2f, -0.5f, -0.5f, -2f));
            this.linePositions.Add(new GridLayoutCtrl_Base.LinePosition(-0.5f, 1f, -2f, -0.5f));
            this.linePositions.Add(new GridLayoutCtrl_Base.LinePosition(0.0f, 0.5f, -1.5f, -1f));
            this.linePositions.Add(new GridLayoutCtrl_Base.LinePosition(0.5f, 0.0f, -1f, -1.5f));
            this.linePositions.Add(new GridLayoutCtrl_Base.LinePosition(1f, -0.5f, -0.5f, -2f));
        }
    }

    public override void InitCellPositions()
    {
        if (!this.PlayerGrid)
        {
            this.cellPositions[0] = new Vector3(-0.5f, 0.5f);
            this.cellPositions[1] = new Vector3(0.0f, 0.0f);
            this.cellPositions[2] = new Vector3(0.5f, -0.5f);
            this.cellPositions[3] = new Vector3(0.0f, 1f);
            this.cellPositions[4] = new Vector3(0.5f, 0.5f);
            this.cellPositions[5] = new Vector3(1f, 0.0f);
            this.cellPositions[6] = new Vector3(0.5f, 1.5f);
            this.cellPositions[7] = new Vector3(1f, 1f);
            this.cellPositions[8] = new Vector3(1.5f, 0.5f);
        }
        else
        {
            this.cellPositions[0] = new Vector3(-0.5f, 0.5f);
            this.cellPositions[1] = new Vector3(0.0f, 0.0f);
            this.cellPositions[2] = new Vector3(0.5f, -0.5f);
            this.cellPositions[3] = new Vector3(-1f, 0.0f);
            this.cellPositions[4] = new Vector3(-0.5f, -0.5f);
            this.cellPositions[5] = new Vector3(0.0f, -1f);
            this.cellPositions[6] = new Vector3(-1.5f, -0.5f);
            this.cellPositions[7] = new Vector3(-1f, -1f);
            this.cellPositions[8] = new Vector3(-0.5f, -1.5f);
        }
    }

    public override void CollapseGrid()
    {
        float num = Time.deltaTime * this.Speed;
        this.TransitionOffset += num;
        this.UnitTransitionOffset += num;
        if ((double)this.TransitionOffset > (double)this.MaxTransitionOffset)
        {
            this.moveVertexVertex(this.vertexList[5], this.linePositions[5], this.MaxTransitionOffset * (float)this.CollapseDirection(), true);
            if (this.hiddenRows == 0)
                this.moveVertexVertex(this.vertexList[1], this.linePositions[1], this.MaxTransitionOffset * (float)this.CollapseDirection(), false);
            this.moveVertexVertex(this.vertexList[2], this.linePositions[2], this.MaxTransitionOffset * (float)this.CollapseDirection(), false);
            this.moveVertexVertex(this.vertexList[3], this.linePositions[3], this.MaxTransitionOffset * (float)this.CollapseDirection(), false);
            this.moveVertexVertex(this.vertexList[4], this.linePositions[4], this.MaxTransitionOffset * (float)this.CollapseDirection(), true);
            this.moveVertexVertex(this.vertexList[6], this.linePositions[6], this.MaxTransitionOffset * (float)this.CollapseDirection(), true);
            this.moveVertexVertex(this.vertexList[7], this.linePositions[7], this.MaxTransitionOffset * (float)this.CollapseDirection(), true);
            int index = 0;
            foreach (BattleGrid.GridCell cell in this.Cells)
            {
                if (cell.UnitAlive())
                {
                    SpriteAnim component = cell.UnitSprite.GetComponent<SpriteAnim>();
                    Vector3 position = this.calculatePosition(this.cellPositions[index].x + this.MaxUnitTransitionOffset * (float)this.CollapseDirection(), this.cellPositions[index].y + this.MaxUnitTransitionOffset * (float)this.CollapseDirection(), this.GridOffset());
                    Vector3 pos = position;
                    component.NewCellPosition(pos);
                    if ((UnityEngine.Object)cell.HealthSprite != (UnityEngine.Object)null)
                        cell.HealthSprite.transform.position = new Vector3(position.x, position.y - 0.4f);
                }
                ++index;
            }
            if (this.hiddenRows == 0)
            {
                this.MoveCellUnit(3, 0);
                this.MoveCellUnit(4, 1);
                this.MoveCellUnit(5, 2);
                this.MoveCellUnit(6, 3);
                this.MoveCellUnit(7, 4);
                this.MoveCellUnit(8, 5);
                this.Cells[6].Active = false;
                this.Cells[7].Active = false;
                this.Cells[8].Active = false;
            }
            if (this.hiddenRows == 1)
            {
                this.MoveCellUnit(3, 0);
                this.MoveCellUnit(4, 1);
                this.MoveCellUnit(5, 2);
                this.Cells[3].Active = false;
                this.Cells[4].Active = false;
                this.Cells[5].Active = false;
                this.Cells[6].Active = false;
                this.Cells[7].Active = false;
                this.Cells[8].Active = false;
            }
            this.collapsing = false;
            ++this.hiddenRows;
            if (this.OnCollapseComplete == null)
                return;
            this.OnCollapseComplete((object)this, new EventArgs());
        }
        else
        {
            this.moveVertexVertex(this.vertexList[5], this.linePositions[5], this.TransitionOffset * (float)this.CollapseDirection(), true);
            if (this.hiddenRows == 0)
                this.moveVertexVertex(this.vertexList[1], this.linePositions[1], this.TransitionOffset * (float)this.CollapseDirection(), false);
            this.moveVertexVertex(this.vertexList[2], this.linePositions[2], this.TransitionOffset * (float)this.CollapseDirection(), false);
            this.moveVertexVertex(this.vertexList[3], this.linePositions[3], this.TransitionOffset * (float)this.CollapseDirection(), false);
            this.moveVertexVertex(this.vertexList[4], this.linePositions[4], this.TransitionOffset * (float)this.CollapseDirection(), true);
            this.moveVertexVertex(this.vertexList[6], this.linePositions[6], this.TransitionOffset * (float)this.CollapseDirection(), true);
            this.moveVertexVertex(this.vertexList[7], this.linePositions[7], this.TransitionOffset * (float)this.CollapseDirection(), true);
            int index = 0;
            foreach (BattleGrid.GridCell cell in this.Cells)
            {
                if (cell.UnitAlive())
                {
                    SpriteAnim component = cell.UnitSprite.GetComponent<SpriteAnim>();
                    Vector3 position = this.calculatePosition(this.cellPositions[index].x + this.UnitTransitionOffset * (float)this.CollapseDirection(), this.cellPositions[index].y + this.UnitTransitionOffset * (float)this.CollapseDirection(), this.GridOffset());
                    Vector3 pos = position;
                    component.Move(pos);
                    if ((UnityEngine.Object)cell.HealthSprite != (UnityEngine.Object)null)
                        cell.HealthSprite.transform.position = new Vector3(position.x, position.y - 0.4f);
                }
                ++index;
            }
        }
    }

    public override bool IsFrontRowEmpty()
    {
        bool flag = true;
        if (this.hiddenRows == 2)
            return false;
        if (this.hiddenRows == 4)
        {
            for (int index = 3; index <= 5; ++index)
            {
                if (this.Cells[index].UnitAlive())
                    flag = false;
            }
        }
        if (this.hiddenRows == 0 || this.hiddenRows == 1)
        {
            for (int index = 0; index <= 2; ++index)
            {
                if (this.Cells[index].UnitAlive())
                    flag = false;
            }
        }
        return flag;
    }

    public override int Row(int cellNo)
    {
        int num = 0;
        if (cellNo > 2)
            num = 1;
        if (cellNo > 5)
            num = 2;
        return num;
    }

    public override void SetTarget(BattleGrid.GridCell attackingUnit, int cellNo, bool isAoE)
    {
        this.damagePattern = new AGDamagePattern(this.Cell2Column(cellNo), this.Cell2Row(cellNo));
        this.TargetCell = cellNo;
        UnitEntity unit = attackingUnit.Unit;
        Debug.Log((object)string.Format("{0} damage pattern", (object)unit.Name));
        this.DamagePattern = this.damagePattern.buildAnimation(new AGUnit(unit.BattleUnit.name, unit.BattleUnit).getWeapons()[0].getAttacks()[0], this.Cell2Column(cellNo), 1, 1, (List<Drawable>)null, 1);
        if (this.DamagePattern == null)
            return;
        foreach (Drawable drawable in this.DamagePattern)
            ;
    }

    public override void SetTargetArea(int _cell, BattleGrid.GridCell _attacking)
    {
        this.damagePattern = new AGDamagePattern(this.Cell2Column(_cell), this.Cell2Row(_cell));
        if (_attacking.Unit.IsAOE())
            this.TargetCell = _cell;
        else if (_attacking.Unit.IsTargetAttack())
            this.TargetCell = this.Cell2Column(_cell);
        else
            this.TargetCell = _cell * -1;
        UnitEntity unit = _attacking.Unit;
        Debug.Log((object)string.Format("{0} damage pattern", (object)unit.Name));
        AGUnit.Attack attack = new AGUnit(unit.BattleUnit.name, unit.BattleUnit).getWeapons()[0].getAttacks()[0];
        if (true)
        {
            if (attack.getAbility().getTargetArea() != null)
            {
                foreach (AGAbility.TargetSquare targetSquare in attack.getAbility().getTargetArea())
                {
                    int index = this.Relative2Cell(this.TargetCell, targetSquare.getX(), targetSquare.getY());
                    Debug.Log((object)string.Format("point {0}, {1} -> {2}", (object)targetSquare.getX(), (object)targetSquare.getY(), (object)index));
                    if (index >= 0 && index <= 8 && this.Cells[index].Active)
                        this.Cells[index].LightUp = HighlightType.Selected;
                }
            }
            if (attack.getAbility().getDamageArea() != null)
            {
                foreach (AGAbility.TargetSquare targetSquare in attack.getAbility().getDamageArea())
                {
                    int index = this.Relative2Cell(_cell, targetSquare.getX(), targetSquare.getY());
                    Debug.Log((object)string.Format("point {0}, {1} -> {2}", (object)targetSquare.getX(), (object)targetSquare.getY(), (object)index));
                    if (index >= 0 && index <= 8 && this.Cells[index].LightUp == HighlightType.None)
                        this.Cells[index].LightUp = HighlightType.SplashDamage;
                }
            }
        }
        else
        {
            foreach (Drawable drawable in this.damagePattern.buildAnimation(attack, this.Cell2Column(7), 0, 1, (List<Drawable>)null, 1))
            {
                AGDamagePattern agDamagePattern = drawable as AGDamagePattern;
                int index = this.Relative2Cell(this.TargetCell, (int)agDamagePattern.getX(), (int)agDamagePattern.getY());
                Debug.Log((object)string.Format("point {0}, {1} -> {2}", (object)agDamagePattern.getX(), (object)agDamagePattern.getY(), (object)index));
                if (index >= 0 && index <= 12)
                    this.Cells[index].LightUp = HighlightType.Selected;
            }
        }
        for (int index = 3; index <= 5; ++index)
        {
            if (this.Cells[index].LightUp == HighlightType.Selected && this.Cells[index - 3].UnitAlive() && (this.Cells[index - 3].Unit.BattleUnit.blocking != 0 && _attacking.Unit.GetSelectedAbility().stats.lineOfFire != 3))
                this.Cells[index].LightUp = HighlightType.None;
        }
    }

    public void SetTargetAreaOLD(int _cell, BattleGrid.GridCell _attacking)
    {
        this.TargetCell = _cell;
        foreach (Area area in _attacking.Unit.GetSelectedAbility().stats.targetArea.data)
        {
            int x = area.pos.x;
            int y = area.pos.y;
            int targetCell = this.TargetCell;
            if ((this.TargetCell != 0 || x >= 0) && (this.TargetCell != 4 || x <= 0))
            {
                int cellNo = this.AdjustRow(targetCell, y);
                if (this.Row(cellNo + x) == this.Row(cellNo))
                {
                    int index = cellNo + x;
                    if ((y != -1 || index <= 4) && (x >= 0 || this.TargetCell != 5 && this.TargetCell != 10) && ((x <= 0 || this.TargetCell != 4 && this.TargetCell != 9) && (index >= 0 && index <= 12 && this.Cells[index].UnitAlive())))
                    {
                        this.Cells[index].LightUp = HighlightType.Damage;
                        this.Cells[index].Weight = area.weight;
                    }
                }
            }
        }
        if (_attacking.Unit.GetSelectedAbility().stats.damageArea == null)
            return;
        foreach (Area area in _attacking.Unit.GetSelectedAbility().stats.damageArea.data)
        {
            int x = area.pos.x;
            int y = area.pos.y;
            int _pos = this.TargetCell;
            if (this.TargetCell != 0 || x >= 0)
            {
                if (y != 0)
                    _pos = this.calcNewEnemyRankPos(_pos, y);
                int index = _pos + x;
                if ((x >= 0 || this.TargetCell != 3 && this.TargetCell != 6) && (x <= 0 || this.TargetCell != 2 && this.TargetCell != 5) && (index >= 0 && index <= 12 && (this.Cells[index].LightUp == HighlightType.None && this.Cells[index].UnitAlive())))
                {
                    this.Cells[index].LightUp = HighlightType.SplashDamage;
                    if (area.weight == 0.0)
                        this.Cells[index].Weight = (double)area.damagePercent;
                    else
                        this.Cells[index].Weight = area.weight;
                }
            }
        }
    }

    public override bool IsBlocked(int cell)
    {
        bool flag = false;
        if (cell >= 3 && cell <= 5 && (this.Cells[cell].LightUp == HighlightType.Selected && this.Cells[cell - 3].UnitAlive()) && this.Cells[cell - 3].Unit.BattleUnit.blocking != 0)
            flag = true;
        if (cell >= 6 && cell <= 8 && this.Cells[cell].LightUp == HighlightType.Selected)
        {
            if (this.Cells[cell - 3].UnitAlive() && this.Cells[cell - 3].Unit.BattleUnit.blocking != 0)
                flag = true;
            if (this.Cells[cell - 6].UnitAlive() && this.Cells[cell - 6].Unit.BattleUnit.blocking != 0)
                flag = true;
        }
        return flag;
    }

    public override int AdjustRow(int cell, int offset)
    {
        int num = cell;
        switch (this.Row(cell))
        {
            case 0:
                if (offset == -2)
                    num += 3;
                if (offset == -3)
                {
                    num += 6;
                    break;
                }
                break;
            case 1:
                if (offset == -2)
                    num -= 3;
                if (offset == -4)
                {
                    num += 3;
                    break;
                }
                break;
        }
        return num;
    }

    public override int calcNewEnemyRankPos(int _pos, int _rows)
    {
        int num1 = _pos;
        int num2 = Math.Abs(_rows);
        int num3 = Math.Sign(_rows);
        if (_pos < 3)
        {
            if (num3 == 1)
            {
                if (num2 == 1)
                    num1 += 3;
                else
                    num1 += 6;
            }
        }
        else if (_pos < 6)
        {
            if (num2 == 1)
            {
                if (num3 == 1)
                {
                    if (_pos != 3)
                        num1 += 3;
                }
                else
                    num1 -= 3;
            }
        }
        else if (num3 == -1)
        {
            if (num2 == 1)
                num1 -= 3;
            else
                num1 += 6;
        }
        return num1;
    }

    public override void SetInRange(int _cell, BattleGrid.GridCell _attacking, int attackingCell)
    {
        for (int index = 0; index <= this.CellCount() - 1; ++index)
        {
            bool flag = true;
            if (!this.Cells[index].UnitAlive())
                flag = false;
            if (_attacking.Unit.GetSelectedAbility().stats.minRange > 0 && this.Row(attackingCell) + (this.Row(index) + 1) < _attacking.Unit.GetSelectedAbility().stats.minRange)
                flag = false;
            if (_attacking.Unit.GetSelectedAbility().stats.maxRange > 0 && this.Row(attackingCell) + (this.Row(index) + 1) > _attacking.Unit.GetSelectedAbility().stats.maxRange)
                flag = false;
            if (flag && this.IsBlocked(_cell, index, _attacking))
                flag = false;
            if (flag)
                this.Cells[index].LightUp = HighlightType.Selected;
        }
    }

    public bool IsBlocked(int _cell, int checkCell, BattleGrid.GridCell _attacking)
    {
        bool flag = false;
        if (checkCell >= 4 && checkCell <= 6 && (this.Cells[checkCell - 3].UnitAlive() && this.Cells[checkCell - 3].Unit.BattleUnit.blocking != 0) && _attacking.Unit.GetSelectedAbility().stats.lineOfFire != 3)
            flag = true;
        if (checkCell >= 7 && checkCell <= 9)
        {
            if (this.Cells[checkCell - 3].UnitAlive() && this.Cells[checkCell - 3].Unit.BattleUnit.blocking != 0 && _attacking.Unit.GetSelectedAbility().stats.lineOfFire != 3)
                flag = true;
            if (this.Cells[checkCell - 6].UnitAlive() && this.Cells[checkCell - 6].Unit.BattleUnit.blocking != 0 && _attacking.Unit.GetSelectedAbility().stats.lineOfFire != 3)
                flag = true;
        }
        return flag;
    }

    public override int GetAbilityTargetCell(int currentCell)
    {
        int num = currentCell;
        if (currentCell >= 6 && currentCell <= 8)
            num -= 6;
        if (currentCell >= 0 && currentCell <= 2)
            num += 6;
        return num;
    }
}
