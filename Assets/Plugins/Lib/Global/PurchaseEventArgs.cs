using System;

namespace BNR
{
    public class PurchaseEventArgs : EventArgs
    {
        public string Name;

        public PurchaseEventArgs(string Name)
        {
            this.Name = Name;
        }
    }
}
