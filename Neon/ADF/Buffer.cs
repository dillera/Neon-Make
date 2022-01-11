using System;
using System.Collections.Generic;
using System.Text;

namespace ADF
{
    public partial class Compiler
    {
        public string InputString;
        public LinkedList<object> OutputList = new LinkedList<object>();

        ushort Input_Pos;
        LinkedListNode<object> Output_Pos;

        Dictionary<string, byte> Symbols = new Dictionary<string, byte>();
        byte NextSymbol = 1;

        public List<ADFAttribute> Attributes = new List<ADFAttribute>
        {
            new ADFAttribute(Attr.ATT_MO, "#MO", "#MODE"),          //ATT_MO
            new ADFAttribute(Attr.ATT_AL, "#AL", "#ALIGN"),         //ATT_AL
            new ADFAttribute(Attr.ATT_SL, "#SL", "#SELECT"),        //ATT_SL
            new ADFAttribute(Attr.ATT_LN, "#LN", "#LINES"),         //ATT_LN
            new ADFAttribute(Attr.ATT_ML, "#ML", "#MARGINLEFT"),    //ATT_ML
            new ADFAttribute(Attr.ATT_MR, "#MR", "#MARGINRIGHT"),   //ATT_MR
            new ADFAttribute(Attr.ATT_MT, "#MT", "#MARGINTOP"),     //ATT_MT
            new ADFAttribute(Attr.ATT_MB, "#MB", "#MARGINBOTTOM"),  //ATT_MB
            new ADFAttribute(Attr.ATT_NA, "$NA", "$NAME"),          //ATT_NA
            new ADFAttribute(Attr.ATT_FS, "$FS", "$FILESPEC"),      //ATT_FS 
            new ADFAttribute(Attr.ATT_LI, "$LI", "$LINK"),          //ATT_LI
            new ADFAttribute(Attr.ATT_ID, "#ID", "#ID"),            //ATT_ID
            new ADFAttribute(Attr.ATT_OF, "%OF", "%OFFSET"),        //ATT_OF
            new ADFAttribute(Attr.ATT_SZ, "%SZ", "%SIZE"),          //ATT_SZ
            new ADFAttribute(Attr.ATT_C0, "#C0", "#COLOR0"),        //ATT_C0
            new ADFAttribute(Attr.ATT_C1, "#C1", "#COLOR1"),        //ATT_C1
            new ADFAttribute(Attr.ATT_C2, "#C2", "#COLOR2"),        //ATT_C2
            new ADFAttribute(Attr.ATT_C3, "#C3", "#COLOR3"),        //ATT_C3
            new ADFAttribute(Attr.ATT_C4, "#C4", "#COLOR4"),        //ATT_C4
            new ADFAttribute(Attr.ATT_C5, "#C5", "#COLOR5"),        //ATT_C5
            new ADFAttribute(Attr.ATT_C6, "#C6", "#COLOR6"),        //ATT_C6
            new ADFAttribute(Attr.ATT_C7, "#C7", "#COLOR7"),        //ATT_C7
            new ADFAttribute(Attr.ATT_C8, "#C8", "#COLOR8"),        //ATT_C8
            new ADFAttribute(Attr.ATT_CM, "#CM", "#CHARMODE"),      //ATT_CM
            new ADFAttribute(Attr.ATT_X0, "#X0", "#XPOS0"),         //ATT_X0
            new ADFAttribute(Attr.ATT_X1, "#X1", "#XPOS1"),         //ATT_X1
            new ADFAttribute(Attr.ATT_X2, "#X2", "#XPOS2"),         //ATT_X2
            new ADFAttribute(Attr.ATT_X3, "#X3", "#XPOS3"),         //ATT_X3
            new ADFAttribute(Attr.ATT_X4, "#X4", "#XPOS4"),         //ATT_X4
            new ADFAttribute(Attr.ATT_X5, "#X5", "#XPOS5"),         //ATT_X5
            new ADFAttribute(Attr.ATT_X6, "#X6", "#XPOS6"),         //ATT_X6
            new ADFAttribute(Attr.ATT_X7, "#X7", "#XPOS7"),         //ATT_X7
            new ADFAttribute(Attr.ATT_Z0, "#Z0", "#P0SIZE"),        //ATT_Z0
            new ADFAttribute(Attr.ATT_Z1, "#Z1", "#P1SIZE"),        //ATT_Z1
            new ADFAttribute(Attr.ATT_Z2, "#Z2", "#P2SIZE"),        //ATT_Z2
            new ADFAttribute(Attr.ATT_Z3, "#Z3", "#P3SIZE"),        //ATT_Z3
            new ADFAttribute(Attr.ATT_ZM, "#ZM", "#MSIZE"),         //ATT_ZM
            new ADFAttribute(Attr.ATT_PR, "#PR", "#PRIORITY"),      //ATT_PR
            new ADFAttribute(Attr.ATT_DM, "#DM", "#DMA"),           //ATT_DM
            new ADFAttribute(Attr.ATT_CH, "#CH", "#CHBASE"),        //ATT_CH
            new ADFAttribute(Attr.ATT_PM, "#PM", "#PMBASE"),        //ATT_PM
            new ADFAttribute(Attr.ATT_WS, "#WS", "#WSYNC"),     //ATT_WS

        };

        public Dictionary<Attr, Object> default_dict;
        public Dictionary<Attr, Object> document_dict;
        public Dictionary<Attr, Object> page_dict;
        public Dictionary<Attr, Object> command_dict;

        public Dictionary<Attr, Object> Populate_Default()
        {
            Dictionary<Attr, Object> tmpDict = new Dictionary<Attr, object>();

            tmpDict.Add(Attr.ATT_MO, 2);    //ATT_MO
            tmpDict.Add(Attr.ATT_AL, 0);	//ATT_AL
            tmpDict.Add(Attr.ATT_SL, 0); 	//ATT_SL
            tmpDict.Add(Attr.ATT_LN, 1); 	//ATT_LN
            tmpDict.Add(Attr.ATT_ML, 0); 	//ATT_ML
            tmpDict.Add(Attr.ATT_MR, 255); 	//ATT_MR
            tmpDict.Add(Attr.ATT_MT, 24); 	//ATT_MT
            tmpDict.Add(Attr.ATT_MB, 216); 	//ATT_MB
            tmpDict.Add(Attr.ATT_NA, ""); 	//ATT_NA
            tmpDict.Add(Attr.ATT_FS, "");	//ATT_FS 
            tmpDict.Add(Attr.ATT_LI, "");   //ATT_LI
            tmpDict.Add(Attr.ATT_ID, 0xFF); //ATT_ID
            tmpDict.Add(Attr.ATT_OF, 0); 	//ATT_OF
            tmpDict.Add(Attr.ATT_SZ, 0);    //ATT_SZ
            tmpDict.Add(Attr.ATT_C0, 0);	//ATT_C0
            tmpDict.Add(Attr.ATT_C1, 0);    //ATT_C1
            tmpDict.Add(Attr.ATT_C2, 0);    //ATT_C2
            tmpDict.Add(Attr.ATT_C3, 0);    //ATT_C3
            tmpDict.Add(Attr.ATT_C4, 40); 	//ATT_C4
            tmpDict.Add(Attr.ATT_C5, 202);	//ATT_C5
            tmpDict.Add(Attr.ATT_C6, 148);	//ATT_C6
            tmpDict.Add(Attr.ATT_C7, 70);   //ATT_C7
            tmpDict.Add(Attr.ATT_C8, 0);    //ATT_C8
            tmpDict.Add(Attr.ATT_CM, 2); 	//ATT_CM
            tmpDict.Add(Attr.ATT_X0, 0);    //ATT_X0
            tmpDict.Add(Attr.ATT_X1, 0);    //ATT_X1
            tmpDict.Add(Attr.ATT_X2, 0);    //ATT_X2
            tmpDict.Add(Attr.ATT_X3, 0);    //ATT_X3
            tmpDict.Add(Attr.ATT_X4, 0);    //ATT_X4
            tmpDict.Add(Attr.ATT_X5, 0);    //ATT_X5
            tmpDict.Add(Attr.ATT_X6, 0);    //ATT_X6
            tmpDict.Add(Attr.ATT_X7, 0);    //ATT_X7
            tmpDict.Add(Attr.ATT_Z0, 0);    //ATT_Z0
            tmpDict.Add(Attr.ATT_Z1, 0);    //ATT_Z1
            tmpDict.Add(Attr.ATT_Z2, 0);    //ATT_Z2
            tmpDict.Add(Attr.ATT_Z3, 0);    //ATT_Z3
            tmpDict.Add(Attr.ATT_ZM, 0);    //ATT_ZM
            tmpDict.Add(Attr.ATT_PR, 0);    //ATT_PR
            tmpDict.Add(Attr.ATT_DM, 34);   //ATT_DM
            tmpDict.Add(Attr.ATT_CH, 0xFF); //ATT_CH
            tmpDict.Add(Attr.ATT_PM, 0);    //ATT_PM
            tmpDict.Add(Attr.ATT_WS, 0);    //ATT_WS

            return tmpDict;
        }

        ushort[] HardwareRegister = new ushort[(int)Size.HWR_COUNT] 
        {
                (ushort)Map.COLPM0,				//ATT_C0
	            (ushort)Map.COLPM1,				//ATT_C1
	            (ushort)Map.COLPM2,				//ATT_C2
	            (ushort)Map.COLPM3,				//ATT_C3
	            (ushort)Map.COLPF0,				//ATT_C4
	            (ushort)Map.COLPF1,				//ATT_C5
	            (ushort)Map.COLPF2,				//ATT_C6
	            (ushort)Map.COLPF3,				//ATT_C7
	            (ushort)Map.COLPF4,				//ATT_C8	
	            (ushort)Map.CHACTL,				//ATT_CM
	            (ushort)Map.HPOSP0,				//ATT_X0
	            (ushort)Map.HPOSP1,				//ATT_X1
	            (ushort)Map.HPOSP2,				//ATT_X2
	            (ushort)Map.HPOSP3,				//ATT_X3
	            (ushort)Map.HPOSM0,				//ATT_X4
	            (ushort)Map.HPOSM1,				//ATT_X5
	            (ushort)Map.HPOSM2,				//ATT_X6
	            (ushort)Map.HPOSM3,				//ATT_X7	
	            (ushort)Map.SIZEP0,				//ATT_Z0
	            (ushort)Map.SIZEP1,				//ATT_Z1
	            (ushort)Map.SIZEP2,				//ATT_Z2
	            (ushort)Map.SIZEP3,				//ATT_Z3
	            (ushort)Map.SIZEM,				//ATT_ZM
	            (ushort)Map.PRIOR,				//ATT_PR
	            (ushort)Map.DMACTL,				//ATT_DM
	            (ushort)Map.CHBASE,				//ATT_FO
	            (ushort)Map.PMBASE,				//ATT_PM
	            (ushort)Map.WSYNC, 				//ATT_WS
            };


    }
}
