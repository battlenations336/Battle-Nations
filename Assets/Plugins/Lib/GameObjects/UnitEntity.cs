
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BNR
{
    public class UnitEntity
    {
        public string Name;
        public bool HasAttacked;
        public bool WasAttacked;
        private EventHandler<UnitEventArgs> onDeath;
        private EventHandler<UnitEventArgs> onFirstDamage;
        private EventHandler onDamage;
        private BattleUnit battleUnit;
        public Dictionary<string, Weapon> WeaponList;
        public string CurrentWeapon;
        public int CurrentAbility;
        private int level;
        private int health;
        private int maxHealth;
        private int armour;
        private int maxArmour;
        private string unitName;
        private bool damaged;
        private GameObject healthBar;

        public override string ToString()
        {
            return string.Format("{0}-{1}", (object)this.Name, (object)this.level);
        }

        public EventHandler<UnitEventArgs> OnFirstDamage
        {
            get
            {
                return this.onFirstDamage;
            }
            set
            {
                this.onFirstDamage = value;
            }
        }

        public EventHandler<UnitEventArgs> OnDeath
        {
            get
            {
                return this.onDeath;
            }
            set
            {
                this.onDeath = value;
            }
        }

        public EventHandler OnDamage
        {
            get
            {
                return this.onDamage;
            }
            set
            {
                this.onDamage = value;
            }
        }

        public bool IsDead { get; set; }

        public UnitEntity(string unitName)
        {
            this.IsDead = false;
            this.WasAttacked = false;
            this.HasAttacked = false;
            this.battleUnit = GameData.BattleUnits[unitName];
            this.level = 0;
            this.level = !GameData.Player.Army.ContainsKey(unitName) ? 0 : GameData.Player.Army[unitName].level;
            this.maxHealth = this.battleUnit.stats[this.level].hp;
            this.health = this.maxHealth;
            this.maxArmour = this.battleUnit.stats[this.level].armorHp;
            this.armour = this.maxArmour;
            this.Name = unitName;
            this.WeaponList = new Dictionary<string, Weapon>();
            if (this.battleUnit.weapons == null)
                return;
            foreach (string key in this.battleUnit.weapons.Keys)
            {
                Weaponry weapon1 = this.battleUnit.weapons[key];
                Weapon weapon2 = new Weapon();
                weapon2.Ammo = weapon1.stats.ammo;
                this.WeaponList.Add(key, weapon2);
                if (weapon1.abilities != null)
                {
                    foreach (string ability in weapon1.abilities)
                        weapon2.AbilityList.Add(ability, new Ability()
                        {
                            Cooldown = 0
                        });
                }
            }
        }

        public BattleUnit BattleUnit
        {
            get
            {
                return this.battleUnit;
            }
        }

        public int GetHealth()
        {
            return this.health;
        }

        public int GetArmour()
        {
            return this.armour;
        }

        public int GetMaxHealth()
        {
            return this.maxHealth;
        }

        public Weapon GetWeapon(string weaponName)
        {
            return this.WeaponList.ContainsKey(weaponName) ? this.WeaponList[weaponName] : (Weapon)null;
        }

        public Ability GetAbility(string weaponName, string abilityName)
        {
            if (this.WeaponList.ContainsKey(weaponName))
            {
                Weapon weapon = this.WeaponList[weaponName];
                if (weapon.AbilityList.ContainsKey(abilityName))
                    return weapon.AbilityList[abilityName];
            }
            return (Ability)null;
        }

        public void ApplyCooldown()
        {
            this.WeaponList[this.CurrentWeapon].AbilityList[this.GetSelectedWeapon().abilities[this.CurrentAbility]].Cooldown = this.GetSelectedAbility().stats.abilityCooldown;
        }

        public bool IsOnCooldown()
        {
            bool flag = false;
            if (this.Cooldown() > 0)
                flag = true;
            return flag;
        }

        public int Cooldown()
        {
            return this.WeaponList[this.CurrentWeapon].AbilityList[this.GetSelectedWeapon().abilities[this.CurrentAbility]].Cooldown;
        }

        public void SkillUp(int points)
        {
            this.IsMaxRank();
        }

        public bool IsMaxRank()
        {
            bool flag = false;
            if (GameData.Player.Army[this.unitName].level >= this.battleUnit.stats.Length)
                flag = true;
            return flag;
        }

        public void SetInitialHealth(int _cellNo, int _health)
        {
            this.health = _health;
            this.damaged = true;
            if (this.OnFirstDamage == null)
                return;
            this.OnFirstDamage((object)this, new UnitEventArgs(_cellNo));
        }

        public void DamageUnit(int cellNo, int _damage)
        {
            int num = _damage;
            if (this.IsDead)
                return;
            if (!this.damaged)
            {
                this.damaged = true;
                if (this.OnFirstDamage != null)
                    this.OnFirstDamage((object)this, new UnitEventArgs(cellNo));
            }
            if (this.armour > 0)
            {
                if (num > this.armour)
                {
                    num -= this.armour;
                    this.armour = 0;
                }
                else
                {
                    this.armour -= num;
                    num = 0;
                }
            }
            this.health -= num;
            if (this.health <= 0)
                this.health = 0;
            if (this.OnDamage != null)
                this.OnDamage((object)this, new EventArgs());
            if (this.health > 0)
                return;
            this.IsDead = true;
            GameData.Player.InjureUnit(this.Name);
            if (this.OnDeath == null)
                return;
            this.OnDeath((object)this, new UnitEventArgs(cellNo));
        }

        public bool IsImmune()
        {
            return this.Name == "hero_cast_raiderkid";
        }

        public int BaseDamageAmount()
        {
            return UnityEngine.Random.Range(this.GetSelectedWeapon().stats.base_damage_min, this.GetSelectedWeapon().stats.base_damage_max);
        }

        public int Power()
        {
            return this.battleUnit.stats[this.level].power;
        }

        public float GetHealthPercent()
        {
            return (float)this.health / (float)(this.maxHealth + this.maxArmour);
        }

        public float GetArmourPercent()
        {
            return (float)this.armour / (float)(this.maxHealth + this.maxArmour);
        }

        public Weaponry GetSelectedWeapon()
        {
            Weaponry weaponry = new Weaponry();
            if (this.battleUnit.weapons != null)
                weaponry = this.battleUnit.weapons[this.CurrentWeapon];
            return weaponry;
        }

        public BattleAbilities GetSelectedAbility()
        {
            BattleAbilities battleAbilities = new BattleAbilities();
            if (this.battleUnit.weapons != null)
            {
                string ability = this.battleUnit.weapons[this.CurrentWeapon].abilities[this.CurrentAbility];
                battleAbilities = GameData.BattleAbilities[ability];
            }
            return battleAbilities;
        }

        public bool IsAOE()
        {
            bool flag = false;
            if (this.GetSelectedAbility().stats.damageArea != null)
                flag = true;
            if (this.GetSelectedAbility().stats.targetArea != null && ((IEnumerable<Area>)this.GetSelectedAbility().stats.targetArea.data).Count<Area>() == 1)
                flag = true;
            return flag;
        }

        public bool IsDirectFire()
        {
            bool flag = false;
            if (this.GetSelectedAbility().stats.targetArea == null && this.GetSelectedAbility().stats.damageArea == null)
                flag = true;
            if (this.GetSelectedAbility().stats.targetArea != null && ((IEnumerable<Area>)this.GetSelectedAbility().stats.targetArea.data).Count<Area>() <= 1 && this.GetSelectedAbility().stats.damageArea == null)
                flag = true;
            return flag;
        }

        public bool IsTargetAttack()
        {
            bool flag = false;
            if (this.GetSelectedAbility().stats.targetArea != null && ((IEnumerable<Area>)this.GetSelectedAbility().stats.targetArea.data).Count<Area>() > 1 && this.GetSelectedAbility().stats.targetArea.type == "Target")
                flag = true;
            return flag;
        }

        public bool IsDefensive()
        {
            return this.battleUnit.weapons == null;
        }

        public bool HasDamageAnimation()
        {
            bool flag = false;
            if (this.GetSelectedAbility().damageAnimationType != null && this.GetSelectedAbility().damageAnimationType != string.Empty)
                flag = true;
            return flag;
        }

        public string GetFrontDamageAnimation()
        {
            string empty = string.Empty;
            return GameData.DamageAnimConfig[this.GetSelectedAbility().damageAnimationType].front;
        }

        public string GetBackDamageAnimation()
        {
            string empty = string.Empty;
            return GameData.DamageAnimConfig[this.GetSelectedAbility().damageAnimationType].back;
        }
    }
}
