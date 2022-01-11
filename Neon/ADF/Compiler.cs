using System;
using System.Collections.Generic;
using System.Text;
using System.IO;


namespace ADF
{
    partial class Compiler
    {
        List<ADFCommand> CommandList;

        public Compiler()
        {
            // ***  POPULATE DICTIONARIES ***

            default_dict = Populate_Default();
            document_dict = Populate_Default();
            page_dict = Populate_Default();
            command_dict = Populate_Default();

            // ************** POPULATE COMMANDS ************** 


            CommandList = new List<Compiler.ADFCommand>
            {
                //HEAD
                new ADFCommand("HEAD", CMD.CMD_HEAD,  cmd_Head, new List<CommandStates>{ CommandStates.S_HEAD },
                    new List<Attr>
                    { Attr.ATT_MO, Attr.ATT_AL, Attr.ATT_ML, Attr.ATT_MR, Attr.ATT_MT, Attr.ATT_MB, Attr.ATT_NA, Attr.ATT_C0, Attr.ATT_C1,
                      Attr.ATT_C2, Attr.ATT_C3, Attr.ATT_C4, Attr.ATT_C5, Attr.ATT_C6, Attr.ATT_C7, Attr.ATT_C8, Attr.ATT_CM, Attr.ATT_X0,
                      Attr.ATT_X1, Attr.ATT_X2, Attr.ATT_X3, Attr.ATT_X4, Attr.ATT_X5, Attr.ATT_X6, Attr.ATT_X7, Attr.ATT_Z0, Attr.ATT_Z1,
                      Attr.ATT_Z2, Attr.ATT_Z3, Attr.ATT_ZM, Attr.ATT_PR, Attr.ATT_DM, Attr.ATT_CH, Attr.ATT_PM
                    }, default_dict, document_dict),
                //LOAD
                new ADFCommand("LOAD", CMD.CMD_LOAD, cmd_Load, new List<CommandStates>{ CommandStates.S_DATA },
                    new List<Attr> { Attr.ATT_ID, Attr.ATT_OF, Attr.ATT_SZ, Attr.ATT_FS },
                     document_dict, command_dict),
                //DATA
                new ADFCommand("DATA", CMD.CMD_DATA, cmd_Data,  new List<CommandStates>{ CommandStates.S_DATA },
                    new List<Attr> { Attr.ATT_ID, Attr.ATT_OF, Attr.ATT_SZ, Attr.ATT_FS },
                     document_dict, command_dict),
                //NAME
                 new ADFCommand("NAME", CMD.CMD_NAME, cmd_Name,  new List<CommandStates>{ CommandStates.S_DATA },
                    new List<Attr> { Attr.ATT_ID, Attr.ATT_SZ },
                     document_dict, command_dict),
                 //CODE
                 new ADFCommand("CODE", CMD.CMD_CODE, cmd_Code,  new List<CommandStates>{ CommandStates.S_DATA },
                    new List<Attr> { Attr.ATT_ID },
                     document_dict, command_dict),
                 //PAGE
                 new ADFCommand("PAGE", CMD.CMD_PAGE, cmd_Page,  new List<CommandStates>{ CommandStates.S_DATA, CommandStates.S_PAGE },
                    new List<Attr>
                    { Attr.ATT_MO, Attr.ATT_AL, Attr.ATT_ML, Attr.ATT_MR, Attr.ATT_MT, Attr.ATT_MB, Attr.ATT_NA, Attr.ATT_C0, Attr.ATT_C1,
                      Attr.ATT_C2, Attr.ATT_C3, Attr.ATT_C4, Attr.ATT_C5, Attr.ATT_C6, Attr.ATT_C7, Attr.ATT_C8, Attr.ATT_CM, Attr.ATT_X0,
                      Attr.ATT_X1, Attr.ATT_X2, Attr.ATT_X3, Attr.ATT_X4, Attr.ATT_X5, Attr.ATT_X6, Attr.ATT_X7, Attr.ATT_Z0, Attr.ATT_Z1,
                      Attr.ATT_Z2, Attr.ATT_Z3, Attr.ATT_ZM, Attr.ATT_PR, Attr.ATT_DM, Attr.ATT_CH, Attr.ATT_PM
                    }, document_dict, page_dict),
                 //TEXT
                 new ADFCommand("TEXT", CMD.CMD_TEXT, cmd_Text,  new List<CommandStates>{ CommandStates.S_PAGE },
                    new List<Attr> { Attr.ATT_MO, Attr.ATT_AL, Attr.ATT_SL, Attr.ATT_ML, Attr.ATT_MR },
                    page_dict, command_dict),
                 //BYTE
                 new ADFCommand("BYTE", CMD.CMD_BYTE, cmd_Byte, new List<CommandStates>{ CommandStates.S_PAGE },
                    new List<Attr> { Attr.ATT_MO, Attr.ATT_AL, Attr.ATT_SL, Attr.ATT_ML, Attr.ATT_MR },
                    page_dict, command_dict),
                 //VIEW
                 new ADFCommand("VIEW", CMD.CMD_LINK, cmd_View, new List<CommandStates>{ CommandStates.S_PAGE },
                    new List<Attr> { Attr.ATT_MO, Attr.ATT_ID, Attr.ATT_OF, Attr.ATT_LN, Attr.ATT_SZ },
                    page_dict, command_dict),
                 //SHOW
                 new  ADFCommand("SHOW", CMD.CMD_SHOW, cmd_Show, new List<CommandStates>{ CommandStates.S_PAGE },
                    new List<Attr> { Attr.ATT_MO, Attr.ATT_AL, Attr.ATT_SL, Attr.ATT_ML, Attr.ATT_MR, Attr.ATT_LN },
                    page_dict, command_dict),
                 //LINK
                 new  ADFCommand("LINK", CMD.CMD_LINK, cmd_Link, new List<CommandStates>{ CommandStates.S_PAGE },
                    new List<Attr> { Attr.ATT_SL, Attr.ATT_ML, Attr.ATT_MR, Attr.ATT_FS, Attr.ATT_LI },
                    page_dict, command_dict),
                 //FILL
                 new  ADFCommand("FILL", CMD.CMD_FILL, cmd_Fill, new List<CommandStates>{ CommandStates.S_PAGE },
                    new List<Attr> { Attr.ATT_MO, Attr.ATT_MB },
                    page_dict, command_dict),
                 //CALL                 
                 new  ADFCommand("CALL", CMD.CMD_FILL, cmd_Call, new List<CommandStates>{ CommandStates.S_PAGE },
                    new List<Attr> { Attr.ATT_ID, Attr.ATT_LN },
                    page_dict, command_dict),
            };
        }

        public void SaveDocument(string FileSpec)
        {
            Document.SaveADX(FileSpec);
        }

        public bool ProcessFile(string FileSpec, FileFormat Format = FileFormat.ATASCII)
        {
            List<string> Source;
            FileStream fs;
            bool Result = false;

            try
            {
                fs = IO.OpenSource(FileSpec, Format);
                Source = IO.LoadFile(fs);
                IO.Close(fs);
                Process(Source);

                Result = true;

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                if (ex.InnerException != null)
                {
                    Console.WriteLine(ex.InnerException.Message);
                }               
            }


            return Result;
        }
    }
}
