
using System;
using System.Collections.Generic;

namespace BNR
{
    public class AGAbility
    {
        public static AGAbility NO_ABILITY = new AGAbility();
        public const int LOF_CONTACT = 0;
        public const int LOF_DIRECT = 1;
        public const int LOF_PRECISE = 2;
        public const int LOF_INDIRECT = 3;
        private BattleAbilities battleAbilities;
        private string tag;
        private string name;
        private string frontAnimationName;
        private string backAnimationName;
        private double damageFromWeapon;
        private double damageFromUnit;
        private int damageBonus;
        private int numAttacks;
        private int minRange;
        private int maxRange;
        private int lineOfFire;
        private bool capture;
        private int aoeDelay;
        private string targetType;
        private bool randomTarget;
        private AGAbility.TargetSquare[] targetArea;
        private AGAbility.TargetSquare[] damageArea;
        private Dictionary<string, AGPrerequisites> prereqs;

        public AGAbility()
        {
            this.tag = "none";
            this.name = "(None)";
            this.minRange = 1;
            this.maxRange = 5;
        }

        public AGAbility(string tag, BattleAbilities json)
        {
            this.tag = tag;
            this.name = json.name;
            if (this.name == null)
                this.name = tag;
            this.initStats(json.stats);
            this.initPrereqs(json.reqs);
        }

        public void initAnimation(BattleAbilities json, Dictionary<string, DamageAnimConfig> dmgAnim)
        {
            string damageAnimationType = json.damageAnimationType;
            if (damageAnimationType == null)
                return;
            DamageAnimConfig damageAnimConfig = dmgAnim[damageAnimationType];
            if (damageAnimConfig == null)
                return;
            this.frontAnimationName = damageAnimConfig.front;
            this.backAnimationName = damageAnimConfig.back;
        }

        private void initStats(AbilityStats stats)
        {
            if (stats == null)
                return;
            this.damageBonus = stats.damage;
            this.damageFromWeapon = stats.damageFromWeapon;
            this.damageFromUnit = stats.damageFromUnit;
            this.minRange = stats.minRange;
            this.maxRange = stats.maxRange;
            this.numAttacks = stats.shotsPerAttack * stats.attacksPerUse;
            this.lineOfFire = stats.lineOfFire;
            this.capture = stats.capture;
            this.damageArea = AGAbility.initArea(stats.damageArea, false);
            TargetArea targetArea = stats.targetArea;
            if (targetArea == null)
                return;
            this.targetType = targetArea.type;
            this.randomTarget = targetArea.random;
            this.aoeDelay = (int)Math.Round((double)targetArea.aoeOrderDelay * 20.0);
            this.targetArea = AGAbility.initArea(targetArea, this.randomTarget);
        }

        private void initPrereqs(Dictionary<string, Req> json)
        {
            if (json == null || json.Count == 0)
                return;
            this.prereqs = new Dictionary<string, AGPrerequisites>();
            foreach (string key in json.Keys)
            {
                AGPrerequisites agPrerequisites = AGPrerequisites.create(json[key].prereq);
                if (agPrerequisites != null)
                    this.prereqs.Add(key, agPrerequisites);
            }
        }

        private static AGAbility.TargetSquare[] initArea(TargetArea area, bool random)
        {
            if (area == null)
                return (AGAbility.TargetSquare[])null;
            Area[] data = area.data;
            if (data == null)
                return (AGAbility.TargetSquare[])null;
            double weight = 0.0;
            if (random)
            {
                foreach (Area area1 in data)
                    weight += area1.weight;
            }
            AGAbility.TargetSquare[] targetSquareArray = new AGAbility.TargetSquare[data.Length];
            for (int index = 0; index < targetSquareArray.Length; ++index)
                targetSquareArray[index] = new AGAbility.TargetSquare(data[index], weight);
            return targetSquareArray;
        }

        public static AGAbility get(string tag)
        {
            return new AGAbility(tag, GameData.BattleAbilities[tag]);
        }

        public string getTag()
        {
            return this.tag;
        }

        public string getName()
        {
            return this.name;
        }

        public string toString()
        {
            return this.name;
        }

        public AGPrerequisites getPrereqs(string unit)
        {
            return this.prereqs != null && this.prereqs.ContainsKey(unit) ? this.prereqs[unit] : (AGPrerequisites)null;
        }

        public int adjustDamage(int damage, int power)
        {
            return (int)((Math.Floor((double)damage * this.damageFromWeapon) + (double)this.damageBonus) * (1.0 + (double)power * this.damageFromUnit / 50.0));
        }

        public int getMinRange()
        {
            return this.minRange;
        }

        public int getMaxRange()
        {
            return this.maxRange;
        }

        public int getLineOfFire()
        {
            return this.lineOfFire;
        }

        public int getNumAttacks()
        {
            return this.numAttacks;
        }

        public int getAoeDelay()
        {
            return this.aoeDelay;
        }

        public bool getRandomTarget()
        {
            return this.randomTarget;
        }

        public string getTargetType()
        {
            return this.targetType;
        }

        public bool getCapture()
        {
            return this.capture;
        }

        public AGAbility.TargetSquare[] getDamageArea()
        {
            return this.damageArea != null ? (AGAbility.TargetSquare[])this.damageArea.Clone() : this.damageArea;
        }

        public AGAbility.TargetSquare[] getTargetArea()
        {
            return this.targetArea != null ? (AGAbility.TargetSquare[])this.targetArea.Clone() : (AGAbility.TargetSquare[])null;
        }

        public class TargetSquare
        {
            public static AGAbility.TargetSquare SINGLE_TARGET = new AGAbility.TargetSquare();
            private double value;
            private double chance;
            private int x;
            private int y;
            private int damage;
            public int Order;

            public TargetSquare()
            {
                this.value = this.chance = 1.0;
            }

            public TargetSquare(Area json, double weight)
            {
                if (weight == 0.0)
                {
                    this.value = (double)json.damagePercent / 100.0;
                    this.chance = 1.0;
                    this.Order = json.order;
                }
                else
                    this.value = this.chance = json.weight / weight;
                AreaPos pos = json.pos;
                if (pos == null)
                    return;
                this.x = pos.x;
                this.y = pos.y;
            }

            public TargetSquare(AGAbility.TargetSquare a, AGAbility.TargetSquare b)
            {
                this.x = a.x + b.x;
                this.y = a.y + b.y;
                this.Order = a.Order + b.Order;
                this.value = a.value * b.value;
                this.chance = a.chance * b.chance;
            }

            public double getValue()
            {
                return this.value;
            }

            public double getChance()
            {
                return this.chance;
            }

            public int getX()
            {
                return this.x;
            }

            public int getY()
            {
                return this.y;
            }

            public int getDamage()
            {
                return this.damage;
            }

            public void setDamage(int value)
            {
                this.damage = value;
            }

            public int getOrder()
            {
                return this.Order;
            }

            public int compareTo(AGAbility.TargetSquare that)
            {
                if (this.y != that.y)
                    return that.y - this.y;
                return this.x != that.x ? this.x - that.x : that.Order - this.Order;
            }

            public static AGAbility.TargetSquare[] Convolution(
              AGAbility.TargetSquare[] in1,
              AGAbility.TargetSquare[] in2)
            {
                AGAbility.TargetSquare[] array = new AGAbility.TargetSquare[in1.Length * in2.Length];
                int num1 = 0;
                for (int index1 = 0; index1 < in1.Length; ++index1)
                {
                    for (int index2 = 0; index2 < in2.Length; ++index2)
                        array[num1++] = new AGAbility.TargetSquare(in1[index1], in2[index2]);
                }
                Array.Sort<AGAbility.TargetSquare>(array);
                int num2 = 1;
                AGAbility.TargetSquare targetSquare1 = array[0];
                for (int index = 1; index < array.Length; ++index)
                {
                    AGAbility.TargetSquare targetSquare2 = array[index];
                    if (targetSquare1.x == targetSquare2.x && targetSquare1.y == targetSquare2.y)
                    {
                        targetSquare1.value += targetSquare2.value;
                        targetSquare1.chance += targetSquare2.chance;
                    }
                    else
                        array[num2++] = targetSquare1 = targetSquare2;
                }
                return array;
            }

            public static int width(AGAbility.TargetSquare[] area)
            {
                int num1 = 0;
                int num2 = 0;
                for (int index = 0; index < area.Length; ++index)
                {
                    int x = area[index].x;
                    if (x < num1)
                        num1 = x;
                    else if (x > num2)
                        num2 = x;
                }
                return num2 - num1 + 1;
            }
        }
    }
}
