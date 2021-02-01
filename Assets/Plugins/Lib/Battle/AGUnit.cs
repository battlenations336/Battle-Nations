using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BNR
{
    public class AGUnit : IComparable
    {
        private static Dictionary<String, BattleUnit> battleUnits;

        private String tag, name, shortName, side;
        private String backAnimName, frontAnimName;
        private Rank[] ranks;
        public Weapon[] weapons;

        public static BattleUnit get(String tag)
        {
            return GameData.BattleUnits[tag];
        }

        public AGUnit(String tag, BattleUnit json)
        {
            string srchname = string.Empty;

            this.tag = tag;
            name = json.name;
            if (json.name.EndsWith("_name"))
                srchname = json.name;
            else
                srchname = json.name + "_name";
            if (GameData.TextFile.ContainsKey(srchname.ToLower()))
                name = GameData.TextFile[srchname.ToLower()];
            else
                name = json.name;
            if (name == null) name = tag;
            if (name.StartsWith("Speciment ")) // fix game file typo
                name = "Specimen" + name.Substring(9);
            shortName = json.shortName;
            if (shortName == null) shortName = name;
            side = json.side;
            backAnimName = json.backIdleAnimation;
            frontAnimName = json.frontIdleAnimation;
            initWeapons(json.weapons);
            initRanks(json.stats);
        }

        private void initRanks(Stats[] json)
        {
            ranks = new Rank[json.Length];
            for (int i = 0; i < ranks.Length; i++)
                ranks[i] = new Rank(json[i]);
        }

        private void initWeapons(Dictionary<string, Weaponry> json)
        {
            if (json == null || json.Count() == 0)
            {
                this.weapons = new Weapon[0];
                return;
            }
            Dictionary<String, Weapon> weapons = new Dictionary<String, Weapon>();
            foreach (string item in json.Keys)
            {
                String name = item;
                Weapon weap = new Weapon(this, name, json[name]);
                switch (name)
                {
                    case "primary": name = "1primary"; break;
                    case "secondary": name = "2secondary"; break;
                }
                weapons.Add(name, weap);
            }
            String[] names = new String[weapons.Count];
            names = weapons.Keys.ToArray();
            Array.Sort(names);
            this.weapons = new Weapon[names.Length];
            for (int i = 0; i < names.Length; i++)
                this.weapons[i] = weapons[names[i]];
        }

        public String getTag()
        {
            return tag;
        }

        public String getName()
        {
            return name;
        }

        public String getShortName()
        {
            return shortName;
        }

        public String toString()
        {
            return name;
        }

        public String getSide()
        {
            return side;
        }

        /*
        public BNAnimation getBackAnimation()
        {
            return BNAnimation.get(backAnimName);
        }

        public BNAnimation getFrontAnimation()
        {
            return BNAnimation.get(frontAnimName);
        }
        */
        public Weapon[] getWeapons()
        {
            Weapon[] copy = new Weapon[weapons.Length];
//            copy[0] = new Weapon(this);
            for (int i = 0; i < weapons.Length; i++)
                copy[i] = weapons[i];
            return copy;
        }

        int IComparable.CompareTo(Object obj)
        {
            AGUnit that = (AGUnit)obj;
            int cmp = this.name.CompareTo(that.name);
            if (cmp == 0)
                cmp = this.tag.CompareTo(that.tag);
            return cmp;
        }

        public int getMaxRank()
        {
            return ranks.Length;
        }

        public Rank getRank(int rank)
        {
            return ranks[rank - 1];
        }

        public int getPower(int rank)
        {
            return ranks[rank - 1].getPower();
        }

        public class Rank
        {
            private int power;
            private AGPrerequisites prereq;

            public Rank(Stats json)
            {
                power = json.power;
                prereq = AGPrerequisites.create(
                        json.prereqsForLevel);
            }

            public int getPower()
            {
                return power;
            }

            public int getMinLevel()
            {
                return prereq == null ? 0 : prereq.getMinLevel();
            }
        }

        public class Weapon
        {
            private AGUnit outer;
            private String name, tag;
            private String frontAnimationName, backAnimationName;
            private Attack[] attacks;
            private int hitDelay;
            private int minDamage, maxDamage;
            private int rangeBonus;             

            public Weapon(AGUnit _outer)
            {
                outer = _outer;
                name = "(None)";
                tag = "none";
                attacks = new Attack[0];
            }

            public Weapon(AGUnit _outer, String tag, Weaponry json)
            {
                outer = _outer;
                this.tag = tag;
                name = json.name;
                if (name == null) name = tag;
                frontAnimationName = json.frontAttackAnimation;
                backAnimationName = json.backAttackAnimation;
                hitDelay = json.damageAnimationDelay + json.firesoundFrame;
                initStats(json.stats);
                string[] abilities = json.abilities;
                attacks = new Attack[abilities.Count()];
                for (int i = 0; i < attacks.Length; i++)
                    attacks[i] = new Attack(outer, abilities[i], this);
            }

            private void initStats(WeaponStats json)
            {
                if (json == null) return;
                minDamage = json.base_damage_min;
                maxDamage = json.base_damage_max;
                rangeBonus = json.rangeBonus;
            }

            public String getName()
            {
                return name;
            }

            public String getTag()
            {
                return tag;
            }
            /*
            public BNAnimation getFrontAnimation()
            {
                return BNAnimation.get(frontAnimationName);
            }

            public BNAnimation getBackAnimation()
            {
                return BNAnimation.get(backAnimationName);
            }
            */
            public Attack[] getAttacks()
            {
                Attack[] array = new Attack[attacks.Length];
//                array[0] = new Attack(outer, null, this);
                for (int i = 0; i < attacks.Length; i++)
                    array[i] = attacks[i];
                return array;
            }

            public int getHitDelay()
            {
                return hitDelay;
            }

            public int getMinDamage()
            {
                return minDamage;
            }

            public int getMaxDamage()
            {
                return maxDamage;
            }
            public int getRangeBonus()
            {
                return rangeBonus;
            }

            public String toString()
            {
                return name;
            }
        }

        public class Attack
        {
            private AGUnit outer;
            private AGAbility ability;
            private Weapon weapon;
            private AGPrerequisites prereq;

            public Attack(AGUnit _outer, String tag, Weapon weapon)
            {
                outer = _outer;
                if (!string.IsNullOrEmpty(tag))
                    ability = AGAbility.get(tag);
                if (ability == null)
                    ability = AGAbility.NO_ABILITY;
                this.weapon = weapon;
                prereq = ability.getPrereqs(tag);
            }

            public String getName()
            {
                return ability.getName();
            }

            public String getTag()
            {
                return ability.getTag();
            }

            public String toString()
            {
                return ability.toString();
            }
            /*
            public BNAnimation getFrontAnimation()
            {
                return ability.getFrontAnimation();
            }

            public BNAnimation getBackAnimation()
            {
                return ability.getBackAnimation();
            }
            */
            public int getMinDamage(int rank)
            {
                return ability.adjustDamage(weapon.getMinDamage(),
                        outer.getPower(rank));
            }

            public int getMaxDamage(int rank)
            {
                return ability.adjustDamage(weapon.getMaxDamage(),
                        outer.getPower(rank));
            }

            public double getAverageDamage(int rank)
            {
                return 0.5 * (getMinDamage(rank) + getMaxDamage(rank));
            }

            public int getMinRange()
            {
                return ability.getMinRange();
            }

            public int getMaxRange()
            {
                return ability.getMaxRange() + weapon.getRangeBonus();
            }

            public int getMinRank()
            {
                return prereq == null ? 1
                        : Math.Max(prereq.getMinRank(), 1);
            }

            public int getMaxRank()
            {
                return outer.getMaxRank();
            }

            public int getHitDelay()
            {
                AGAbility.TargetSquare[] area = ability.getTargetArea();
                int aoeDelay = ability.getAoeDelay();
                if (area != null && aoeDelay != 0)
                {
                    foreach (AGAbility.TargetSquare sq in area)
                        if (sq.getX() == 0)
                            return weapon.getHitDelay()
                                    + aoeDelay * (sq.getOrder() - 1);
                }
                return weapon.getHitDelay();
            }

            public AGAbility getAbility()
            {
                return ability;
            }

            public Weapon getWeapon()
            {
                return weapon;
            }
        }
    }
}
