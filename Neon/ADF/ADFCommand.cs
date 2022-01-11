using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ADF
{
    public enum CommandStates { S_PAGE = 16, S_DATA = 32, S_HEAD = 128 }
    public enum CMD { CMD_HEAD, CMD_LOAD, CMD_DATA, CMD_NAME, CMD_CODE, CMD_PAGE, CMD_TEXT, CMD_BYTE, CMD_VIEW, CMD_SHOW, CMD_LINK, CMD_FILL, CMD_CALL };


    partial class Compiler
    {
        public class ADFCommand
        {
            public string Name;
            public CMD Key;
            public Action Action;
            public List<CommandStates> ValidStates;
            public List<Attr> Attributes;
            public Dictionary<Attr, Object> Source_Dictionary;
            public Dictionary<Attr, Object> Target_Dictionary;

            public ADFCommand()
            {

            }

            public ADFCommand(string tmpName, CMD tmpKey, Action tmpAction, List<CommandStates> tmpStates, List<Attr> tmpAttributes, Dictionary<Attr, Object> tmpSource, Dictionary<Attr, Object> tmpTarget)
            {
                Name = tmpName;
                Key = tmpKey;
                Action = tmpAction;
                ValidStates = tmpStates;
                Attributes = tmpAttributes;
                Source_Dictionary = tmpSource;
                Target_Dictionary = tmpTarget;
            }

        }

        //Commands are in the root Compiler namespace to control CommandState
        public void cmd_None()
        {
            return;
        }

        public void cmd_Head()
        {
            CommandState = CommandStates.S_DATA;
            Document.FirstPage = null;
        }

        public void cmd_Page()
        {
            Page tmpPage;
            DataBank tmpBank;

            CommandState = CommandStates.S_PAGE;

            if (Document.FirstPage != null)
            {
                Document.FinishPage();
            }

            //tmpPage = (Page*)Mem_AllocLow(sizeof(Page));
            tmpPage = new Page();
            tmpPage.MemADDR = Document.Mem_AllocLow(tmpPage.c_sizeof());

            if (Document.FirstPage == null)
            {
                Document.FirstPage = tmpPage;
            }
            else
            {
                Document.CurrentPage.NextPage = tmpPage;
                Document.CurrentPage.Pointer.NextPage = tmpPage.MemADDR;
                Document.UpdateDataObject(Document.CurrentPage);
            }

            Document.CurrentPage = tmpPage;

            tmpPage.Name = (string)page_dict[Attr.ATT_NA];

            for (Attr i = (Attr)ADF.Register.HWR_START; i < (Attr)ADF.Register.HWR_BYTE_END; ++i)
            {
                //Console.WriteLine(page_dict[i]);
                tmpPage.PageStyle[i - (Attr)ADF.Register.HWR_START] = Convert.ToByte(page_dict[i]);
            }

            tmpBank = Document.FindDataBank(Convert.ToByte(page_dict[Attr.ATT_CH]));

            tmpPage.PageStyle[Attr.ATT_CH - (Attr)ADF.Register.HWR_START] = (byte)(tmpBank.Address / 256);


            if (Convert.ToByte(page_dict[Attr.ATT_PM]) != 0)
            {
                tmpBank = Document.FindDataBank(Convert.ToByte(page_dict[Attr.ATT_PM]));
                tmpPage.PageStyle[Attr.ATT_PM - (Attr)ADF.Register.HWR_START] = (byte)(tmpBank.Address / 256);
            }

            Document.DLBuffer_Len = 0;   //Reset our Display List buffer
            Document.NeedLMS = 1;
            Document.LineCount = 0;
            Document.LastDLILine = 0; //reset our last line pointer

            //Create Top Margin

            Document.BlankLines(Convert.ToByte(page_dict[Attr.ATT_MT]));

            Document.LineLimit = Convert.ToByte(page_dict[Attr.ATT_MB]);

            Document.CurrentPage.LinkList = null; //clear the list of links for this page	
            Document.CurrentLink = null;    //clear previous link as this is a new page	
            Document.CurrentPage.Pointer.DLInterrupt = 0;   //reset DL interrupt
            //Not sure where to hold the DLCODE yet
            Document.DLCode_Len = 0; //reset our DL Code pointer
            Document.NumDLI = 0;

            //Add new page
            Document.AddDataObject(tmpPage);
        }

        public void cmd_Text()
        {
            NextState = ParseType.PARSE_TEXT;
        }

        public void cmd_Byte()
        {
            NextState = ParseType.PARSE_BITMAP;
        }

        void cmd_Link()
        {
            Link tmpLink;
            byte len;
            byte width;
            string tmpString;

            //printrec(0,"LINK not implemented \n",32);

            if (Document.LineAddress == 0)
            {
                //If there isn't a Address, then ignore
                return;
            }

            len = (byte)((string)command_dict[Attr.ATT_FS]).Length;

            tmpLink = new Link();

            tmpLink.MemADDR = Document.Mem_AllocHigh((ushort)(tmpLink.c_sizeof() + len));

            if (Document.CurrentLink == null)
            {
                Document.CurrentPage.LinkList = tmpLink;
                Document.CurrentPage.Pointer.LinkList = tmpLink.MemADDR;
            }
            else
            {
                Document.CurrentLink.NextLink = tmpLink;
                Document.CurrentLink.Pointer.NextLink = tmpLink.MemADDR;
                Document.UpdateDataObject(Document.CurrentLink);

            }

            tmpLink.PreviousLink = Document.CurrentLink;

            if (Document.CurrentLink != null)
            {
                tmpLink.Pointer.PreviousLink = Document.CurrentLink.MemADDR;
            }


            Document.CurrentLink = tmpLink;

            //Set Link's properties


            width = Math.Min(Convert.ToByte(command_dict[Attr.ATT_MR]), (byte)(Document.LineWidth[(Document.AnticMode)] - 1));

            Document.CurrentLink.NextLink = null;              //We are the last link
            Document.CurrentLink.Pointer.DisplayLine = Document.LineAddress;     //Set its pointer to the current address of the last line created
            Document.CurrentLink.Pointer.Start = Convert.ToByte(command_dict[Attr.ATT_ML]);
            Document.CurrentLink.Pointer.End = width;

            Document.CurrentLink.PageName = (string)command_dict[Attr.ATT_LI];
            Document.CurrentLink.FileSpec = (string)command_dict[Attr.ATT_FS];


            //Set link to first link if SELECTED
            if (Convert.ToByte(command_dict[Attr.ATT_SL]) != 0)
            {
                Document.CurrentPage.LinkList = Document.CurrentLink;
                Document.CurrentPage.Pointer.LinkList = Document.CurrentLink.MemADDR;
            }

            Document.AddDataObject(tmpLink);

        }

        void cmd_Fill()
        {
            //Fill the rest of the Page Out with the current mode
            byte temp;

            temp = (byte)(Convert.ToByte(command_dict[Attr.ATT_MO]) % 16);
            if (temp > 0)
            {
                while (Document.LineCount + Document.LineHeight[temp] <= Convert.ToInt32(command_dict[Attr.ATT_MB]))
                {
                    Document.AddEmptyLine(temp);
                }
            }
            else
            {
                temp = (byte)((Convert.ToByte(command_dict[Attr.ATT_MB]) > Document.LineCount) ? Convert.ToByte(command_dict[Attr.ATT_MB]) - Document.LineCount : 0);
                Document.BlankLines(temp);
            }

        }

        void cmd_Show()
        {
            // printrec(0,"SHOW not implemented \n",32);

            numLines = Convert.ToByte(command_dict[Attr.ATT_LN]);

            if (numLines > 0)
            {
                NextState = ParseType.PARSE_LITERAL;
            }
        }


        void cmd_Data()
        {
            char status;
            //int length;
            //char* ADDR;
            DataBank b;

            //print(0,"Data not implemented \n");		

            CommandState = CommandStates.S_DATA;
            /*
                if(!Mem_Check(command_dict[ATT_OF] + command_dict[ATT_SZ]))
                {
                    //print(0,"not enough memory for data");
                    Have_Mem = MEM_FULL;
                    return;
                }
            */

            Document.DataCursor = Document.Mem_AllocLow((ushort)(Convert.ToUInt16(command_dict[Attr.ATT_OF]) + Convert.ToUInt16(command_dict[Attr.ATT_SZ])));
            Document.DataCursor += Convert.ToUInt16(command_dict[Attr.ATT_OF]);

            //b = SetDataBank(command_dict[Attr.ATT_ID], DataCursor, BankLevel.Level_Load);
            Document.Bank.Add(new DataBank(Convert.ToByte(command_dict[Attr.ATT_ID]), BankLevel.Level_Load, Document.DataCursor));

            //if (b == 0)
            //{
            //    //Error = ERR_ID;
            //    return;
            //}

            if ((string)command_dict[Attr.ATT_FS] == "")
            {
                //Enter Byte Entry Mode
                NextState = ParseType.PARSE_DATA;
            }
            else
            {
                //Enter file mode

                //status = open(2, 4, 0, command_dict[ATT_FS]);

                FileStream fs = new FileStream((string)command_dict[Attr.ATT_FS], FileMode.Open);

                fs.Read(Document.Data, Document.DataCursor, Convert.ToUInt16(command_dict[Attr.ATT_SZ]));


            }


        }
               
        void cmd_Code()
        {
            int address;
            byte i = 1; //start at the end of the command;	
            byte r;
            byte value;
            ushort ADDR;
            //byte[] Buff = (byte[])OutputList.First.Value;

            string Buff;
            DataBank b;
            LinkedListNode<object> n = OutputList.First.Next;

            //print(0,"Code not implemented \n");

            /* NEED MEM CHECK HERE BUT NEED TO FIND A BETTER WAY THEN SCATTERING THESE MEM CHECKS EVERWHERE */
            //Need to add header for our DLI

            ADDR = Document.Mem_AllocLow(1);
            Document.Bank.Add(new DataBank(Convert.ToByte(command_dict[Attr.ATT_ID]), BankLevel.Level_Run, ADDR));

            //if (!Document.SetDataBank(command_dict[ATT_ID], ADDR, Level_Run))
            //{
            //    Error = ERR_ID;
            //    return;
            //}

            Document.Data[ADDR] = 0x48;       //PHA
            ++ADDR;

            while (n != null)
            {
                r = FindRegister((string)n.Value);

                if (r > 0)
                {
                    r = (byte)(r - (byte)Register.HWR_START);

                    address = HardwareRegister[r];
                    //n = n.Next;
                    value = Convert.ToByte((n.Next.Value));

                    if ((r == 0x19) || (r == 0x1A))
                    {
                        //PMBASE and CHBASE needs to look up their values
                        b = Document.FindDataBank(value);
                        value = (byte)(b.Address / 256);
                    }

                    ADDR = Document.Mem_AllocLow(5);

                    //Add code for this item

                    Document.Data[ADDR] = 0xA9;       //LDA#
                    ++ADDR;
                    Document.Data[ADDR] = value;
                    ++ADDR;
                    Document.Data[ADDR] = 0x8D;       //STA
                    ++ADDR;
                    Document.Data[ADDR] = (byte)(address % 256);
                    ++ADDR;
                    Document.Data[ADDR] = (byte)(address / 256);

                }

                n = n.Next.Next;
            }

            //Need to add header for our DLI
            ADDR = Document.Mem_AllocLow(3);
            Document.Data[ADDR] = 0x4C;       //JMP
            ++ADDR;
            // Document.Data[ADDR] = (int)DoNextDLI % 256;
            Document.Data[ADDR] = 0;
            ++ADDR;
            //Document.Data[ADDR] = (int)DoNextDLI / 256;
            Document.Data[ADDR] = 6;
            ++ADDR;

        }

        void cmd_Call()
        {
            DataBank dl;
            int i;

            dl = Document.FindDataBank(Convert.ToByte(command_dict[Attr.ATT_ID]));

            if (dl.Level == BankLevel.Level_Run)
            {

                Document.NumDLI = Convert.ToByte(command_dict[Attr.ATT_LN]); //How many lines to call DLI 
                                               //Handle multiple calls
                for (i = 0; i < Document.NumDLI; ++i)
                {
                    Document.DLCode[Document.DLCode_Len] = dl.Address;
                    ++Document.DLCode_Len;
                }
            }
            else
            {
                Document.NumDLI = 0;
            }

        }

        void cmd_View()
        {
            //Need to look at again, seems to be adding extra lines - size is set
            //DataBank i;
            ushort Address;

            var b = Document.FindDataBank(Convert.ToByte(command_dict[Attr.ATT_ID]));

            Address = (ushort)(b.Address + Convert.ToInt16(command_dict[Attr.ATT_OF]));

            for (int i = 0; i < Convert.ToByte(command_dict[Attr.ATT_LN]); ++i)
            {
                Document.AddDisplayLine((byte)(Convert.ToByte(command_dict[Attr.ATT_MO]) % 16), Address);
                if (Convert.ToUInt16(command_dict[Attr.ATT_SZ]) > 0)
                {
                    Address += Convert.ToUInt16(command_dict[Attr.ATT_SZ]);
                }
                else
                {
                    Address = 0;
                }

            }

            Document.NeedLMS = 1;

        }
        
        void cmd_Name()
        {
            //SetDataBank(command_dict[ATT_ID], (char*)command_dict[ATT_SZ], Level_Load);
            Document.Bank.Add(new DataBank(Convert.ToByte(command_dict[Attr.ATT_ID]), BankLevel.Level_Load, Convert.ToUInt16(command_dict[Attr.ATT_SZ])));
        }

        void cmd_Load()
        {
            LoadFile tmpFile;
            int len;
            byte b;

            Document.DataCursor = Document.Mem_AllocLow((ushort)(Convert.ToUInt16(command_dict[Attr.ATT_OF]) + Convert.ToUInt16(command_dict[Attr.ATT_SZ])));
            Document.DataCursor += Convert.ToUInt16(command_dict[Attr.ATT_OF]);

            Document.Bank.Add(new DataBank(Convert.ToByte(command_dict[Attr.ATT_ID]), BankLevel.Level_Load, Document.DataCursor));
            //b = SetDataBank(command_dict[ATT_ID], DataCursor, Level_Load);

            //if (b == 0)
            //{
            //    Error = ERR_ID;
            //    return;
            //}

            len = (command_dict[Attr.ATT_FS]).ToString().Length;

            //tmpFile = (LoadFile*)Mem_AllocHigh(sizeof(LoadFile) + len);

            tmpFile = new LoadFile();
            tmpFile.MemADDR = Document.Mem_AllocHigh((ushort)(tmpFile.c_sizeof() + len));

            tmpFile.NextFile = Document.FileCursor;

            if(Document.FileCursor != null)
            {
                tmpFile.NextADDR = Document.FileCursor.MemADDR;
            }
         
            Document.FileCursor = tmpFile;

            //Set File's properties

            tmpFile.StartAddr = Document.DataCursor;
            tmpFile.Size = (ushort)Convert.ToInt16(command_dict[Attr.ATT_SZ]);
            //strcopy(command_dict[ATT_FS], tmpFile->FileSpec, len);
            tmpFile.FileName = command_dict[Attr.ATT_FS].ToString();

            Document.AddDataObject(tmpFile);
        }

    }

}
