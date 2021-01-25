
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BNR
{
    public class AGDamagePattern : Drawable
    {
        private static Color REDDISH = new Color((float)byte.MaxValue, 76f, 76f);
        private int startFrame;
        private int endFrame;
        private bool keep;
        private double xPos;
        private double yPos;
        private Color color;
        private string label;
        private string label2;
        private AGAbility.TargetSquare[] targetList;

        public AGDamagePattern(int x, int y)
        {
            this.xPos = (double)x;
            this.yPos = (double)y;
            this.keep = true;
        }

        public List<Drawable> buildTargetPattern(
          AGUnit.Attack attack,
          int column,
          int pos,
          int range,
          List<Drawable> list,
          int rank)
        {
            list = new List<Drawable>();
            bool flag = pos < 0;
            AGAbility ability = attack.getAbility();
            AGAbility.TargetSquare[] targetArea = ability.getTargetArea();
            if (targetArea == null)
                return list;
            GridPoint gridPoint = new GridPoint(column, pos + range);
            int num1;
            int num2;
            int num3;
            if ("Weapon".Equals(ability.getTargetType()))
            {
                num1 = -5;
                if (ability.getLineOfFire() == 0 && AGAbility.TargetSquare.width(targetArea) == 1 && attack.getMaxRange() < 5)
                    num1 = -attack.getMaxRange();
                num2 = -5;
                num3 = 0;
                gridPoint = gridPoint.translate(0, -range + (flag ? 1 : -1));
            }
            else
            {
                num2 = num1 = -2;
                num3 = Math.Min(attack.getMaxRange() - 1, 2);
            }
            double num4 = 0.0;
            double num5 = 9999.0;
            for (int index = 0; index < targetArea.Length; ++index)
            {
                AGAbility.TargetSquare targetSquare = targetArea[index];
                int x = targetSquare.getX();
                int y = targetSquare.getY();
                if (y < num1 || y > num3 || (x < -4 || x > 4) || (y == num3 || y == num2) && (x == -4 || x == 4))
                {
                    targetArea[index] = (AGAbility.TargetSquare)null;
                }
                else
                {
                    double num6 = targetSquare.getValue();
                    if (num6 > num4)
                        num4 = num6;
                    if (num6 < num5)
                        num5 = num6;
                }
            }
            double num7 = num4 - num5;
            if (num7 < 1E-06)
                num7 = 1E-06;
            double averageDamage = attack.getAverageDamage(rank);
            foreach (AGAbility.TargetSquare targetSquare in targetArea)
            {
                if (targetSquare != null)
                {
                    int x = targetSquare.getX();
                    int y = targetSquare.getY();
                    if (flag)
                    {
                        x = -x;
                        y = -y;
                    }
                    gridPoint.translate(x, y);
                    AGDamagePattern agDamagePattern = new AGDamagePattern(x, y);
                    int num6 = (int)Math.Round(averageDamage * targetSquare.getValue());
                    if (num6 < 1 && averageDamage > 0.0)
                        num6 = 1;
                    targetSquare.setDamage(num6);
                    agDamagePattern.label = num6.ToString();
                    agDamagePattern.color = this.blend((num4 - targetSquare.getValue()) / num7, AGDamagePattern.REDDISH, Color.yellow);
                    if (ability.getRandomTarget())
                    {
                        int num8 = (int)Math.Round(100.0 * (1.0 - Math.Pow(1.0 - targetSquare.getChance(), (double)ability.getNumAttacks())));
                        if (num8 < 1)
                            num8 = 1;
                        if (num8 > 99 && targetSquare.getChance() < 0.9999999999)
                            num8 = 99;
                        agDamagePattern.label2 = num8.ToString() + "%";
                    }
                    list.Add((Drawable)agDamagePattern);
                }
            }
            this.targetList = targetArea;
            return list;
        }

        public List<Drawable> buildAnimation(
          AGUnit.Attack attack,
          int column,
          int pos,
          int range,
          List<Drawable> list,
          int rank)
        {
            list = new List<Drawable>();
            bool flag = pos < 0;
            AGAbility ability = attack.getAbility();
            AGAbility.TargetSquare[] targetSquareArray = ability.getTargetArea();
            AGAbility.TargetSquare[] damageArea = ability.getDamageArea();
            if (targetSquareArray == null || targetSquareArray.Length == 1)
            {
                if (damageArea != null)
                    targetSquareArray = damageArea;
                else if (targetSquareArray == null)
                    targetSquareArray = new AGAbility.TargetSquare[1]
                    {
            AGAbility.TargetSquare.SINGLE_TARGET
                    };
            }
            else if (damageArea != null && damageArea.Length != 1)
                targetSquareArray = AGAbility.TargetSquare.Convolution(targetSquareArray, damageArea);
            GridPoint gridPoint1 = new GridPoint(column, pos + range);
            int num1;
            int num2;
            int num3;
            if ("Weapon".Equals(ability.getTargetType()))
            {
                num1 = -5;
                if (ability.getLineOfFire() == 0 && AGAbility.TargetSquare.width(targetSquareArray) == 1 && attack.getMaxRange() < 5)
                    num1 = -attack.getMaxRange();
                num2 = -5;
                num3 = 0;
                gridPoint1 = gridPoint1.translate(0, -range + (flag ? 1 : -1));
            }
            else
            {
                num2 = num1 = -2;
                num3 = Math.Min(attack.getMaxRange() - 1, 2);
            }
            double num4 = 0.0;
            double num5 = 9999.0;
            for (int index = 0; index < targetSquareArray.Length; ++index)
            {
                AGAbility.TargetSquare targetSquare = targetSquareArray[index];
                int x = targetSquare.getX();
                int y = targetSquare.getY();
                if (y < num1 || y > num3 || (x < -4 || x > 4) || (y == num3 || y == num2) && (x == -4 || x == 4))
                {
                    targetSquareArray[index] = (AGAbility.TargetSquare)null;
                }
                else
                {
                    double num6 = targetSquare.getValue();
                    if (num6 > num4)
                        num4 = num6;
                    if (num6 < num5)
                        num5 = num6;
                }
            }
            double num7 = num4 - num5;
            if (num7 < 1E-06)
                num7 = 1E-06;
            double averageDamage = attack.getAverageDamage(rank);
            foreach (AGAbility.TargetSquare targetSquare in targetSquareArray)
            {
                if (targetSquare != null)
                {
                    int x = targetSquare.getX();
                    int y = targetSquare.getY();
                    if (flag)
                    {
                        x = -x;
                        y = -y;
                    }
                    GridPoint gridPoint2 = gridPoint1.translate(x, y);
                    AGDamagePattern agDamagePattern = new AGDamagePattern(gridPoint2.x, gridPoint2.y);
                    int num6 = (int)Math.Round(averageDamage * targetSquare.getValue());
                    if (num6 < 1 && averageDamage > 0.0)
                        num6 = 1;
                    targetSquare.setDamage(num6);
                    agDamagePattern.label = num6.ToString();
                    agDamagePattern.color = this.blend((num4 - targetSquare.getValue()) / num7, AGDamagePattern.REDDISH, Color.yellow);
                    if (ability.getRandomTarget())
                    {
                        int num8 = (int)Math.Round(100.0 * (1.0 - Math.Pow(1.0 - targetSquare.getChance(), (double)ability.getNumAttacks())));
                        if (num8 < 1)
                            num8 = 1;
                        if (num8 > 99 && targetSquare.getChance() < 0.9999999999)
                            num8 = 99;
                        agDamagePattern.label2 = num8.ToString() + "%";
                    }
                    list.Add((Drawable)agDamagePattern);
                }
            }
            this.targetList = targetSquareArray;
            return list;
        }

        private Color blend(double x, Color c1, Color c2)
        {
            if (x < 1.0 / 510.0)
                return c1;
            if (x > 509.0 / 510.0)
                return c2;
            double num = 1.0 - x;
            return new Color((float)(int)Math.Round(num * (double)c1.r + x * (double)c2.r), (float)(int)Math.Round(num * (double)c1.g + x * (double)c2.g), (float)(int)Math.Round(num * (double)c1.b + x * (double)c2.b));
        }

        private bool isVisible(int frame)
        {
            if (frame < this.startFrame)
                return false;
            return this.keep || frame < this.endFrame;
        }

        public void drawFrame(int frame, GameObject g)
        {
            this.isVisible(frame);
        }

        public double getSortPosition()
        {
            return this.yPos - 1000.0;
        }

        public RectangleD getBounds()
        {
            return new RectangleD(this.xPos - (double)GridPoint.GRID_X, this.yPos - (double)GridPoint.GRID_Y, (double)(2 * GridPoint.GRID_X), (double)(2 * GridPoint.GRID_Y));
        }

        public RectangleD getBounds(int frame)
        {
            return !this.isVisible(frame) ? (RectangleD)null : this.getBounds();
        }

        public int getEndFrame()
        {
            return this.endFrame;
        }

        public double getX()
        {
            return this.xPos;
        }

        public double getY()
        {
            return this.yPos;
        }

        public AGAbility.TargetSquare[] getTargetSquares()
        {
            return this.targetList;
        }

        public IEnumerable<AGAbility.TargetSquare> getOrderedTargetList()
        {
            return (IEnumerable<AGAbility.TargetSquare>)((IEnumerable<AGAbility.TargetSquare>)this.targetList).Where<AGAbility.TargetSquare>((Func<AGAbility.TargetSquare, bool>)(x => x != null)).ToList<AGAbility.TargetSquare>().OrderByDescending<AGAbility.TargetSquare, int>((Func<AGAbility.TargetSquare, int>)(x => x.getOrder())).ThenByDescending<AGAbility.TargetSquare, int>((Func<AGAbility.TargetSquare, int>)(x => x.getY()));
        }
    }
}
