
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BNR
{
    public class BattleGrid
    {
        public int AttackingCell;
        public int AttackDamage;
        private int lastAttacker;
        public int TargetCell;
        public List<int> TargetList;
        public List<int> SplashList;
        public List<UnitEntity> RepairList;
        private BattleGrid.GridCell[] cells;

        public BattleGrid()
        {
            this.cells = new BattleGrid.GridCell[13];
            this.TargetList = new List<int>();
            this.SplashList = new List<int>();
            for (int index = 0; index < 13; ++index)
                this.cells[index] = new BattleGrid.GridCell();
            this.AttackingCell = -1;
        }

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

        public int Row(int cellNo)
        {
            int num = 0;
            if (cellNo > 4)
                num = 1;
            if (cellNo > 9)
                num = 2;
            return num;
        }

        public BattleGrid.GridCell Attacker()
        {
            BattleGrid.GridCell gridCell = (BattleGrid.GridCell)null;
            if (this.AttackingCell >= 0)
                gridCell = this.Cells[this.AttackingCell];
            return gridCell;
        }

        public void UpdateCooldowns()
        {
            for (int index = 0; index <= 12; ++index)
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
            if (cellNo >= 0 && cellNo <= 12)
                gridCell = this.Cells[cellNo];
            return gridCell;
        }

        public BattleGrid.GridCell Target()
        {
            BattleGrid.GridCell gridCell = (BattleGrid.GridCell)null;
            if (this.TargetCell >= 0 && this.TargetCell <= 12)
                gridCell = this.Cells[this.TargetCell];
            return gridCell;
        }

        public BattleGrid.GridCell SelectTarget()
        {
            BattleGrid.GridCell gridCell = (BattleGrid.GridCell)null;
            List<int> source = new List<int>();
            this.TargetCell = -1;
            for (int index = 0; index <= 12; ++index)
            {
                if (this.cells[index].UnitAlive())
                    source.Add(index);
            }
            if (source.Count<int>() > 0)
            {
                this.TargetCell = source[UnityEngine.Random.Range(0, source.Count<int>())];
                gridCell = this.Cells[this.TargetCell];
                gridCell.LightUp = HighlightType.Hit;
            }
            return gridCell;
        }

        public void SetTarget1(int target)
        {
            this.TargetCell = target;
            this.Cells[this.TargetCell].LightUp = HighlightType.Hit;
        }

        public void SetPossibleTargets(int _cell, BattleGrid.GridCell _attacking)
        {
            if (_attacking.Unit.GetSelectedAbility().stats.targetArea == null)
                this.setInRange(_cell, _attacking);
            else
                this.setTargetArea(_cell, _attacking);
        }

        private void setTargetArea(int _cell, BattleGrid.GridCell _attacking)
        {
            this.TargetCell = _cell;
            foreach (Area area in _attacking.Unit.GetSelectedAbility().stats.targetArea.data)
            {
                int x = area.pos.x;
                int y = area.pos.y;
                int targetCell = this.TargetCell;
                if ((this.TargetCell != 0 || x >= 0) && (this.TargetCell != 4 || x <= 0))
                {
                    int cellNo = this.adjustRow(targetCell, y);
                    if (this.Row(cellNo + x) == this.Row(cellNo))
                    {
                        int index = cellNo + x;
                        if ((y != -1 || index <= 4) && (x >= 0 || this.TargetCell != 5 && this.TargetCell != 10) && ((x <= 0 || this.TargetCell != 4 && this.TargetCell != 9) && (index >= 0 && index <= 12 && this.cells[index].Active)))
                        {
                            this.cells[index].LightUp = HighlightType.Damage;
                            this.cells[index].Weight = area.weight;
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
                    if ((x >= 0 || this.TargetCell != 5 && this.TargetCell != 10) && (x <= 0 || this.TargetCell != 4 && this.TargetCell != 9) && (index >= 0 && index <= 12 && (this.cells[index].LightUp == HighlightType.None && this.cells[index].Active)))
                    {
                        this.cells[index].LightUp = HighlightType.SplashDamage;
                        this.cells[index].Weight = area.weight != 0.0 ? area.weight : (double)area.damagePercent;
                    }
                }
            }
        }

        private int adjustRow(int cell, int offset)
        {
            int num = cell;
            switch (this.Row(cell))
            {
                case 0:
                    if (offset == -2)
                        num += 5;
                    if (offset == -3)
                    {
                        num += 10;
                        break;
                    }
                    break;
                case 1:
                    if (offset == -2)
                        num -= 5;
                    if (offset == -4)
                    {
                        num += 5;
                        break;
                    }
                    break;
            }
            return num;
        }

        private int calcNewEnemyRankPos(int _pos, int _rows)
        {
            int num1 = _pos;
            int num2 = Math.Abs(_rows);
            int num3 = Math.Sign(_rows);
            if (_pos < 5)
            {
                if (num3 == 1)
                {
                    if (num2 == 1)
                        num1 += 5;
                    else
                        num1 += 9;
                }
            }
            else if (_pos < 10)
            {
                if (num2 == 1)
                {
                    if (num3 == 1)
                    {
                        if (_pos != 5)
                            num1 += 4;
                    }
                    else
                        num1 -= 5;
                }
            }
            else if (num3 == -1)
            {
                if (num2 == 1)
                    num1 -= 4;
                else
                    num1 += 9;
            }
            return num1;
        }

        private void setInRange(int _cell, BattleGrid.GridCell _attacking)
        {
            for (int cellNo = 0; cellNo <= 12; ++cellNo)
            {
                bool flag = true;
                if (_attacking.Unit.GetSelectedAbility().stats.minRange > 0 && this.Row(_cell) + (this.Row(cellNo) + 1) < _attacking.Unit.GetSelectedAbility().stats.minRange)
                    flag = false;
                if (_attacking.Unit.GetSelectedAbility().stats.maxRange > 0 && this.Row(_cell) + (this.Row(cellNo) + 1) > _attacking.Unit.GetSelectedAbility().stats.maxRange)
                    flag = false;
                if (flag)
                    this.cells[cellNo].LightUp = HighlightType.Selected;
            }
        }

        public BattleGrid.GridCell SelectAttacker_Rnd()
        {
            BattleGrid.GridCell gridCell = (BattleGrid.GridCell)null;
            List<int> source = new List<int>();
            this.AttackingCell = -1;
            for (int index = 0; index <= 12; ++index)
            {
                if (this.cells[index].UnitAlive())
                    source.Add(index);
            }
            if (source.Count<int>() > 0)
            {
                this.AttackingCell = source[UnityEngine.Random.Range(0, source.Count<int>())];
                gridCell = this.Cells[this.AttackingCell];
            }
            this.lastAttacker = this.AttackingCell;
            return gridCell;
        }

        public BattleGrid.GridCell SelectAttacker_Seq()
        {
            BattleGrid.GridCell gridCell = (BattleGrid.GridCell)null;
            List<int> intList = new List<int>();
            this.AttackingCell = -1;
            int num = this.lastAttacker >= 12 ? 0 : this.lastAttacker + 1;
            for (int index = num; index <= 12; ++index)
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
            return gridCell;
        }

        public void SetAttacker(int cellNo)
        {
            if (!this.cells[cellNo].UnitAlive() || this.cells[cellNo].Unit.IsDefensive())
                return;
            this.AttackingCell = cellNo;
            this.AttackDamage = this.cells[cellNo].Unit.BaseDamageAmount();
            this.cells[cellNo].LightUp = HighlightType.Selected;
        }

        public void SetTarget(int cellNo)
        {
            int[] numArray = new int[13]
            {
        0,
        5,
        1,
        6,
        10,
        2,
        7,
        11,
        3,
        8,
        12,
        4,
        9
            };
            this.TargetCell = cellNo;
            foreach (int index in numArray)
            {
                if (this.cells[index].LightUp == HighlightType.SplashDamage && this.cells[index].UnitAlive())
                    this.SplashList.Add(index);
                if (this.cells[index].LightUp == HighlightType.Damage && this.cells[index].UnitAlive())
                    this.TargetList.Add(index);
                if (this.cells[index].LightUp == HighlightType.Hit && this.cells[index].UnitAlive())
                    this.TargetList.Add(index);
            }
        }

        public int IsCellClicked(Vector3 mousePos, float cellWidth, float cellHeight)
        {
            int num = -1;
            for (int index = 0; index <= 12; ++index)
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
            for (int index = 0; index <= 12; ++index)
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
            for (int index = 0; index <= 12; ++index)
            {
                if (this.cells[index].UnitAlive())
                    ++num;
            }
            return num;
        }

        public int GetNextCell()
        {
            int num = -1;
            for (int index = 0; index <= 12; ++index)
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
            for (int index = 0; index <= 12; ++index)
            {
                if (this.Cells[index].UnitAlive())
                    GameData.Player.ReleaseUnit(this.Cells[index].Unit.Name);
            }
        }

        public class GridCell
        {
            private bool active;
            private Vector3 position;
            private HighlightType lightUp;
            private UnitEntity unit;
            private double weight;
            private GameObject cellSprite;
            private GameObject cellMesh;
            private GameObject unitSprite;
            private GameObject healthSprite;

            public EventHandler OnChangeState { get; set; }

            public bool SystemUnit { get; set; }

            public override string ToString()
            {
                return this.unit != null ? string.Format("{0}", (object)this.unit.Name) : "Empty";
            }

            public bool Active
            {
                get
                {
                    return this.active;
                }
                set
                {
                    this.active = value;
                }
            }

            public Vector3 Position
            {
                get
                {
                    return this.position;
                }
                set
                {
                    this.position = value;
                    this.active = true;
                }
            }

            public HighlightType LightUp
            {
                get
                {
                    return this.lightUp;
                }
                set
                {
                    if (this.lightUp == value)
                        return;
                    this.lightUp = value;
                    switch (this.lightUp)
                    {
                        case HighlightType.None:
                            this.setCellSprite("squareBlank");
                            break;
                        case HighlightType.Selected:
                            this.setCellSprite("squareSelectBlue");
                            break;
                        case HighlightType.Damage:
                            this.setCellSprite("squareSelectBlue");
                            break;
                        case HighlightType.SplashDamage:
                            this.setCellSprite("squareAttackYellow");
                            break;
                        case HighlightType.Hit:
                            this.setCellSprite("squareSelectRed");
                            break;
                        case HighlightType.Splashed:
                            this.setCellSprite("squareSplashRed");
                            break;
                    }
                    if (this.OnChangeState == null)
                        return;
                    this.OnChangeState((object)this, new EventArgs());
                }
            }

            private void setCellSprite(string _cellSprite)
            {
                GameObject cellSprite = this.CellSprite;
            }

            public UnitEntity Unit
            {
                get
                {
                    return this.unit;
                }
                set
                {
                    this.unit = value;
                }
            }

            public double Weight
            {
                get
                {
                    return this.weight;
                }
                set
                {
                    this.weight = value;
                }
            }

            public GameObject CellSprite
            {
                get
                {
                    return this.cellSprite;
                }
                set
                {
                    this.cellSprite = value;
                }
            }

            public GameObject UnitSprite
            {
                get
                {
                    return this.unitSprite;
                }
                set
                {
                    this.unitSprite = value;
                }
            }

            public GameObject HealthSprite
            {
                get
                {
                    return this.healthSprite;
                }
                set
                {
                    this.healthSprite = value;
                }
            }

            public bool UnitAlive()
            {
                bool flag = false;
                if (this.active && this.unit != null && !this.unit.IsDead)
                    flag = true;
                return flag;
            }

            public bool UnitImportant()
            {
                return !this.unit.BattleUnit.unimportant;
            }
        }
    }
}
