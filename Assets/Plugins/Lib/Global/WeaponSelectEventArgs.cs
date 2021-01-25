using System;

namespace BNR
{
    public class WeaponSelectEventArgs : EventArgs
    {
        public string Weapon;
        public string Ability;

        public WeaponSelectEventArgs(string Weapon, string Ability)
        {
            this.Weapon = Weapon;
            this.Ability = Ability;
        }
    }
}
