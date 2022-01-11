using System;
using System.Collections.Generic;
using System.Text;

namespace ADF
{

    public enum Attr
    {
        ATT_MO, ATT_AL, ATT_SL, ATT_LN, ATT_ML, ATT_MR, ATT_MT, ATT_MB, ATT_NA, ATT_FS, ATT_LI, ATT_ID, ATT_OF, ATT_SZ, ATT_C0, ATT_C1,
        ATT_C2, ATT_C3, ATT_C4, ATT_C5, ATT_C6, ATT_C7, ATT_C8, ATT_CM, ATT_X0, ATT_X1, ATT_X2, ATT_X3, ATT_X4, ATT_X5, ATT_X6, ATT_X7,
        ATT_Z0, ATT_Z1, ATT_Z2, ATT_Z3, ATT_ZM, ATT_PR, ATT_DM, ATT_CH, ATT_PM, ATT_WS
    };

    public enum Register { HWR_START = Attr.ATT_C0, HWR_BYTE_END = Attr.ATT_CH }

    partial class Compiler
    {
        public class ADFAttribute
        {
            public Attr Key;
            public string Name { get; set; }
            public string FullName { get; set; }

            public ADFAttribute()
            {

            }
            public ADFAttribute(Attr tmpKey, string tmpName, string tmpFullName)
            {
                Key = tmpKey;
                Name = tmpName;
                FullName = tmpFullName;
            }


        }
    }
}
