using System;
using System.Collections.Generic;
using System.Text;

namespace ADF
{
    public partial class Compiler
    {
        ParseType ProcessState;
        ParseType NextState;
        CommandStates CommandState;
        public Document Document;
        byte numLines;


        public bool Process(List<string> SourceList)
        {
            bool status;
            ParseResult Result;

            ADFCommand cmd;

            ProcessState = ParseType.PARSE_CODE;
            NextState = ParseType.PARSE_CODE;   //Do we need to set this every loop?
            CommandState = CommandStates.S_HEAD;

            // Error = ErrorCode.ERR_NONE;

            //Set initial RAM pointers

            Document = new Document();
            status = true;

            foreach (string s in SourceList)
            {
                InputString = s; //need to redo this
                Result = ParseLine(ProcessState);

                if (Result == ParseResult.PR_INVALID)
                {
                    ProcessState = ParseType.PARSE_CODE; //This will trigger an error and then have it printed				
                }

                if (numLines == 0)
                {
                    if (Result == ParseResult.PR_CODE)
                    {
                        ProcessState = ParseType.PARSE_CODE;
                        NextState = ParseType.PARSE_CODE;

                    }
                }

                switch (ProcessState)
                {
                    case ParseType.PARSE_CODE:

                        //Do_Command:					

                        switch (Result)
                        {

                            case ParseResult.PR_NULL:
                                if (!string.IsNullOrWhiteSpace(InputString))
                                {
                                    throw new Exception("Error 02: Command or Syntax Error.", new Exception(InputString));
                                }
                                break;

                            case ParseResult.PR_CODE:
                                cmd = FindCommand();
                                if ((cmd != null) && (cmd.ValidStates.Contains(CommandState)))
                                {
                                    PopulateDictionary(cmd.Attributes, cmd.Source_Dictionary, ref cmd.Target_Dictionary);
                                    cmd.Action();
                                }
                                else
                                {
                                    //Error = ERR_CMD;
                                   // throw new Exception("Error 02: Command or Syntax Error.", new Exception(InputString));
                                }
                                break;

                            default:
                                //Error = ERR_CMD;
                                throw new Exception("Error 02: Command or Syntax Error.", new Exception(InputString));

                        }
                        break;


                    case ParseType.PARSE_LITERAL:
                        CopyLine();
                        --numLines;
                        if (numLines == 0)
                        {
                            NextState = ParseType.PARSE_CODE;
                        }
                        break;

                    case ParseType.PARSE_TEXT:
                    case ParseType.PARSE_BITMAP:
                        CopyLine();
                        break;

                    case ParseType.PARSE_DATA:
                        Document.CopyDataLow((byte[])OutputList.First.Value);
                        break;
                }


                ProcessState = NextState;

            }

            //Finish the last page

            Document.FinishPage();

            /*
            if (Error)
            {
                printrec(0, InputBuffer, 255);
                LogError(Error);
            }
            */


            return status;
        }

        ADFCommand FindCommand()
        {
            if(OutputList.First == null)
            {
                return null;
            }

            string Name = (string)OutputList.First.Value;

            foreach (ADFCommand cmd in CommandList)
            {
                if (cmd.Name == Name.ToUpper())
                {
                    return cmd;
                }

            }

            return null;
        }


        void PopulateDictionary(List<Attr> cmdAttributes, Dictionary<Attr, Object> source_dict, ref Dictionary<Attr, Object> target_dict)
        {
            bool valid;
            object value;

            foreach (Attr attr in cmdAttributes)
            {
                ADFAttribute tmpAttr = new ADFAttribute();

                foreach (ADFAttribute a in Attributes)
                {
                    if (a.Key == attr)
                    {
                        tmpAttr = a;
                        break;
                    }
                }

                valid = FindAttribute(tmpAttr.Name, out value);


                if (!valid) //Not found
                {
                    value = source_dict[tmpAttr.Key];
                }

                target_dict[tmpAttr.Key] = value;   

            }


        }

        bool FindAttribute(string AttrName, out object value)
        {
            bool valid = false;


            LinkedListNode<object> tmpObj;
            tmpObj = OutputList.First.Next; //skip command

            value = null;

            while (tmpObj != null)
            {

                if ((string)tmpObj.Value == AttrName)
                {
                    value = tmpObj.Next.Value;
                    valid = true;
                    break;
                }

                tmpObj = tmpObj.Next.Next; //skip the value        
            }

            return valid;

        }

        bool CopyLine()
        {            
            return Document.CopyLine((byte[])OutputList.First.Value, Convert.ToByte(command_dict[Attr.ATT_MO]), Convert.ToByte(command_dict[Attr.ATT_AL]), Convert.ToByte(command_dict[Attr.ATT_SL]), Convert.ToByte(command_dict[Attr.ATT_ML]), Convert.ToByte(command_dict[Attr.ATT_MR]));
        }


        byte FindRegister(string AttrName)
        {
            //byte valid;
            //byte j;

            int i = (int)Register.HWR_START;

            // int i = (int)Register.HWR_START * (int)Size.ATT_LEN;
            //char value = 0xFF;

            AttrName = AttrName.Substring(0, 3);

            while (i < ((int)Size.ATT_COUNT))
            {
                if (Attributes[i].Name == AttrName)
                {
                    return (byte)i;
                }

                ++i;
            }

            return 0;

        }



    }
}
