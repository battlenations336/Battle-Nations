
using System;
using System.Collections.Generic;
using System.Linq;

namespace BNR
{
    public class ArmyUnit
    {
        private float buildSpeed = 1f;
        private string name;
        public int total;
        public int busy;
        public int repairing;
        public int injured;
        public int level;
        private BattleUnit battleUnit;
        public int xp;

        public int Id { get; set; }

        public bool Upgrading { get; set; }

        public float UpgradeTimer { get; set; }

        public DateTime UpgradeStart { get; set; }

        public string Name
        {
            get
            {
                return this.name;
            }
            set
            {
                this.name = value;
            }
        }

        public bool IsDefence()
        {
            return ((IEnumerable<string>)this.GetBattleUnit().tags).Contains<string>("Defense");
        }

        public void IncreaseSkill(int sp)
        {
            this.xp += sp;
            if (this.xp <= this.GetBattleUnitStats().levelCutOff)
                return;
            this.xp = this.GetBattleUnitStats().levelCutOff;
        }

        public int AvailableQty()
        {
            return this.total - (this.busy + this.injured + this.repairing);
        }

        public BattleUnit GetBattleUnit()
        {
            return GameData.BattleUnits[this.name];
        }

        public Stats GetBattleUnitStats()
        {
            return GameData.BattleUnits[this.name].stats[this.level];
        }

        public void StartPromotion()
        {
            if (this.level >= this.GetBattleUnit().stats.Length)
                return;
            this.UpgradeStart = DateTime.Now;
            this.UpgradeTimer = 0.0f;
            this.Upgrading = true;
            GameData.Player.Storage.DebitStorage(this.GetBattleUnit().stats[this.level].levelUpCost);
        }

        public void Hurry()
        {
            if (!this.Upgrading || (double)this.buildSpeed != 1.0)
                return;
            this.buildSpeed = (float)(((double)this.GetBattleUnit().stats[this.level].levelUpTime - (double)this.UpgradeTimer) / 3.0);
            GameData.Player.Storage.DebitStorage(new Cost()
            {
                currency = this.GetHurryCost()
            });
        }

        public void Collect()
        {
            if (!this.PromotionComplete())
                return;
            this.UpgradeTimer = 0.0f;
            this.Upgrading = false;
            this.UpgradeStart = DateTime.MinValue;
            if (this.GetBattleUnit().stats[this.level].levelUpRewards != null)
            {
                Rewards levelUpRewards = this.GetBattleUnit().stats[this.level].levelUpRewards;
                if (levelUpRewards.XP > 0)
                    GameData.Player.IncreaseXP(levelUpRewards.XP);
                if (levelUpRewards.amount != null)
                {
                    if (levelUpRewards.amount.money > 0)
                        GameData.Player.Storage.AddMoney(levelUpRewards.amount.money);
                    if (levelUpRewards.amount.currency > 0)
                        GameData.Player.Storage.AddNanopods(levelUpRewards.amount.currency);
                }
                if (levelUpRewards.amount.resources != null)
                {
                    foreach (string resourceName in Functions.ResourceNames())
                    {
                        int propertyValue = (int)Functions.GetPropertyValue((object)levelUpRewards.amount.resources, resourceName);
                        if (propertyValue > 0)
                            GameData.Player.Storage.AddResource(Functions.ResourceNameToEnum(resourceName), propertyValue);
                    }
                }
            }
            ++this.level;
            this.xp = 0;
        }

        public int GetHurryCost()
        {
            return Functions.GetHours((float)this.GetBattleUnit().stats[this.level].levelUpTime, this.UpgradeTimer);
        }

        public Cost GetPromotionCost()
        {
            return this.GetBattleUnit().stats[this.level].levelUpCost;
        }

        public float GetProgressPercent()
        {
            return this.UpgradeTimer / (float)this.GetBattleUnit().stats[this.level].levelUpTime;
        }

        public string GetTaskTime()
        {
            return Functions.GetTaskTime(this.GetBattleUnit().stats[this.level].levelUpTime, this.UpgradeTimer);
        }

        public bool PromotionInProgress()
        {
            return this.Upgrading;
        }

        public bool PromotionComplete()
        {
            return this.Upgrading && (double)this.GetProgressPercent() >= 1.0;
        }

        public bool ReadyToPromote()
        {
            return !this.Upgrading && this.xp >= this.GetBattleUnit().stats[this.level].levelCutOff && this.GetBattleUnit().stats[this.level].levelCutOff > 0;
        }

        public void Update(float deltaTime)
        {
            if (!this.Upgrading || (double)this.GetProgressPercent() >= 1.0)
                return;
            this.UpgradeTimer += deltaTime * this.buildSpeed;
            if ((double)this.UpgradeTimer < (double)this.GetBattleUnit().stats[this.level].levelUpTime)
                return;
            this.UpgradeTimer = (float)this.GetBattleUnit().stats[this.level].levelUpTime;
        }
    }
}
