using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BNR
{
    public class Weapon
    {
        public Dictionary<string, Ability> AbilityList;
        public int Ammo;

        public Weapon()
        {
            AbilityList = new Dictionary<string, Ability>();
        }
    }
}
