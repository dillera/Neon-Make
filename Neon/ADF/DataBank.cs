using System;
using System.Collections.Generic;
using System.Text;

namespace ADF
{
    public enum BankLevel { Level_Load, Level_Run, Level_ReadOnly };
    public class DataBank
    {
        public byte ID;
        public BankLevel Level;
        public ushort Address;

        public DataBank(byte tmpID, BankLevel tmpLevel, ushort tmpAddress)
        {
            ID = tmpID;
            Level = tmpLevel;
            Address = tmpAddress;
        }
    }
}
