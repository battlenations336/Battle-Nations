
using System;
using System.Collections.Generic;
using System.Linq;

namespace BNR
{
    public class Storage
    {
        public List<Resource> StorageResources;
        private Dictionary<Resource, Storage.ResourceInfo> bank;
        private int currency;
        private int currency_b;
        private int money;
        private int z2points;

        public EventHandler OnChangeGold { get; set; }

        public EventHandler OnChangeNano { get; set; }

        public EventHandler OnChangeResource { get; set; }

        public Storage()
        {
            this.init();
        }

        public void init()
        {
            Array values = Enum.GetValues(typeof(Resource));
            this.bank = new Dictionary<Resource, Storage.ResourceInfo>();
            foreach (Resource key in values)
                this.bank.Add(key, new Storage.ResourceInfo());
            this.StorageResources = new List<Resource>()
      {
        Resource.stone,
        Resource.wood,
        Resource.iron,
        Resource.oil,
        Resource.concrete,
        Resource.lumber,
        Resource.steel,
        Resource.coal
      };
        }

        public void AddMoney(int amount)
        {
            this.money += amount;
            if (this.OnChangeGold == null)
                return;
            this.OnChangeGold((object)this, new EventArgs());
        }

        public void SetMoney(int amount)
        {
            this.money = amount;
            if (this.OnChangeGold == null)
                return;
            this.OnChangeGold((object)this, new EventArgs());
        }

        public void AddNanopods(int amount)
        {
            this.currency += amount;
            if (this.OnChangeNano == null)
                return;
            this.OnChangeNano((object)this, new EventArgs());
        }

        public void SetNanopods(int amount)
        {
            this.currency = amount;
            if (this.OnChangeNano == null)
                return;
            this.OnChangeNano((object)this, new EventArgs());
        }

        public void AddResource(TaxResources taxResources)
        {
            if (taxResources == null)
                return;
            if (taxResources.stone != 0)
                this.AddResource(Resource.stone, taxResources.stone);
            if (taxResources.wood != 0)
                this.AddResource(Resource.wood, taxResources.wood);
            if (taxResources.iron != 0)
                this.AddResource(Resource.iron, taxResources.iron);
            if (taxResources.coal != 0)
                this.AddResource(Resource.coal, taxResources.coal);
            if (taxResources.oil != 0)
                this.AddResource(Resource.oil, taxResources.oil);
            if (taxResources.concrete != 0)
                this.AddResource(Resource.concrete, taxResources.concrete);
            if (taxResources.lumber != 0)
                this.AddResource(Resource.lumber, taxResources.lumber);
            if (taxResources.steel == 0)
                return;
            this.AddResource(Resource.steel, taxResources.steel);
        }

        public void AddResource(Resource type, int amount)
        {
            this.bank[type].qty += amount;
            if (this.OnChangeResource == null)
                return;
            this.OnChangeResource((object)this, new EventArgs());
        }

        public void SetResource(Resource type, int amount)
        {
            this.bank[type].qty = amount;
        }

        public bool CheckResourceCapacity(Resource type)
        {
            return this.CheckResourceCapacity(type, 0);
        }

        public bool CheckResourceCapacity(Resource type, int amount)
        {
            bool flag = true;
            if (this.bank[type].max < this.bank[type].qty + amount)
                flag = false;
            if (this.bank[type].max == this.bank[type].qty && amount == 0)
                flag = false;
            return flag;
        }

        public int GetGoldAmnt()
        {
            return this.money;
        }

        public int GetNanoAmnt()
        {
            return this.currency;
        }

        public int GetZ2points()
        {
            return this.z2points;
        }

        public bool ResourceFull()
        {
            KeyValuePair<Resource, Storage.ResourceInfo> keyValuePair = this.bank.Where<KeyValuePair<Resource, Storage.ResourceInfo>>((Func<KeyValuePair<Resource, Storage.ResourceInfo>, bool>)(x => x.Value.qty >= x.Value.max && x.Value.qty > 0)).FirstOrDefault<KeyValuePair<Resource, Storage.ResourceInfo>>();
            return keyValuePair.Value != null && keyValuePair.Value.max != 0;
        }

        public bool ResourceFull(Resource type)
        {
            return this.bank[type].qty >= this.bank[type].max;
        }

        public int GetResource(Resource type)
        {
            return this.bank[type].qty;
        }

        public int GetResourceCapacity(Resource type)
        {
            return this.bank[type].max - this.bank[type].qty;
        }

        public int GetMaxResource(Resource type)
        {
            return this.bank[type].max;
        }

        public int SetMaxResource(Resource type, int quantity)
        {
            this.bank[type].max = quantity;
            return this.bank[type].max;
        }

        public int AddMaxResource(Resource type, int quantity)
        {
            this.bank[type].max += quantity;
            return this.bank[type].max;
        }

        public void DebitStorage(Cost _cost)
        {
            if (_cost.money > 0)
                this.AddMoney(_cost.money * -1);
            if (_cost.currency > 0)
                this.AddNanopods(_cost.currency * -1);
            if (_cost.resources == null)
                return;
            foreach (string resourceName in Functions.ResourceNames())
            {
                int propertyValue = (int)Functions.GetPropertyValue((object)_cost.resources, resourceName);
                if (propertyValue > 0)
                    this.AddResource(Functions.ResourceNameToEnum(resourceName), propertyValue * -1);
            }
        }

        public void CreditStorage(Cost _cost)
        {
            if (_cost.money > 0)
                this.AddMoney(_cost.money);
            if (_cost.currency > 0)
                this.AddNanopods(_cost.currency);
            if (_cost.resources == null)
                return;
            foreach (string resourceName in Functions.ResourceNames())
            {
                int propertyValue = (int)Functions.GetPropertyValue((object)_cost.resources, resourceName);
                if (propertyValue > 0)
                    this.AddResource(Functions.ResourceNameToEnum(resourceName), propertyValue);
            }
        }

        public class ResourceInfo
        {
            public int qty;
            public int max;
            public int tier;
            public string texture;
        }
    }
}
