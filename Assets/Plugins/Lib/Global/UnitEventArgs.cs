using System;

namespace BNR
{
    public class UnitEventArgs : EventArgs
    {
        public int CellNo;

        public UnitEventArgs(int _cellNo)
        {
            this.CellNo = _cellNo;
        }
    }
}
