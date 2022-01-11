using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;

namespace ADF
{
    public partial class Compiler
    {
        public enum ParseType { PARSE_CODE, PARSE_LITERAL, PARSE_TEXT, PARSE_BITMAP, PARSE_DATA };
        public enum ParseState { PS_GETTEXT, PS_ISCOMMAND, PS_GETCMD, PS_GETBITS, PS_GETBYTE, PS_GETTINYINT, PS_GETINT, PS_INVALID, PS_GETWORD, PS_GETSTRING, PS_GETATTR, PS_GETTYPE, PS_GETVALUE };
        public enum ParseResult { PR_VALID, PR_PARTIAL, PR_INVALID, PR_CODE, PR_OVERFLOW, PR_NULL };

        public ParseResult ParseLine(ParseType ParseAction)
        {
            ParseResult Result = ParseResult.PR_NULL;
            Input_Pos = 0;
            OutputList.Clear();
            bool Valid;

            if (String.IsNullOrEmpty(InputString))
            {
                OutputList.AddLast(new byte[1]);
                return Result;
            }

            switch (ParseAction)
            {
                case ParseType.PARSE_LITERAL:
                    GetText();
                    break;

                case ParseType.PARSE_TEXT:
                    if (IsCommand())
                    {
                        goto case ParseType.PARSE_CODE;
                    }
                    GetText();
                    break;


                case ParseType.PARSE_BITMAP:
                case ParseType.PARSE_DATA:
                    if (IsCommand())
                    {
                        goto case ParseType.PARSE_CODE;
                    }
                    GetData();
                    break;


                case ParseType.PARSE_CODE:
                    //Eliminate Blank lines

                    if (string.IsNullOrWhiteSpace(InputString))
                    {
                        break;
                    }

                    while (InputString[Input_Pos] == 32)
                    {
                        ++Input_Pos;
                    }

                    if ((Input_Pos == InputString.Length) || (InputString[Input_Pos] == ';'))
                    {
                        return ParseResult.PR_CODE;
                    }

                    if (!IsCommand())
                    {
                        return ParseResult.PR_INVALID;
                    }

                    if (ParseCommand())
                    {
                        return ParseResult.PR_CODE;
                    }

                    break;

            }
            return Result;
        }

        bool IsCommand()
        {
            return InputString[0] == ':';
        }

        bool ParseCommand()
        {
            ushort value = 0;
            bool Result;
            Result = IsCommand();
            string str;

            if (Result)
            {
                ++Input_Pos;
       

                Result = GetWord((int)Size.CMD_LEN);

                if (Result)
                {

                    while (Input_Pos < InputString.Length)
                    {
                        RemoveSpaces();

                        if ((Input_Pos == InputString.Length) || (InputString[Input_Pos] == ';'))
                        {     
                            return true;
                        }

                        Result = GetWord((int)Size.ATT_LEN);

                        if (Result)
                        {
                            RemoveSpaces();

                            switch (((string)OutputList.Last.Value)[0])
                            {
                                case '#':
                                    //OutBuffer_Len += ATT_LEN;       //Add the attribute name to command buffers
                                    if (GetNum(out value, 2))
                                    {
                                        OutputList.AddLast(value);
                                    }
                                    else
                                    {
                                        throw new Exception("Error 06: Value Error.");
                                    }
                                    break;

                                case '%':
                                    //OutBuffer_Len += ATT_LEN;       //Add the attribute name to command buffers
                                    if(GetNum(out value, 4))
                                    {
                                        OutputList.AddLast(value);
                                    }
                                    else
                                    {
                                        throw new Exception("Error 06: Value Error.");
                                    }                                    
                                    break;

                                case '$':
                                    //OutBuffer_Len += ATT_LEN;       //Add the attribute name to command buffers					
                                    if(GetString(out str))
                                    {
                                        OutputList.AddLast(str);
                                    }
                                    else
                                    {
                                        throw new Exception("Error 06: Value Error.");
                                    }                                   
                                    break;
                              

                                default:
                                    throw new Exception("Error 05: Attribute/Syntax Error.");
                                   // return false;
                                    // break;
                                    //Result = ParseResult.PR_CODE;
                                    // break;
                            }

                        }
                        else
                        {
                            return false;
                        }

                    }

                }
            }




            return Result;
        }

        bool GetWord(int size)
        {
            int calc = 0;
            char curr;
            StringBuilder tmpString = new StringBuilder();


            while (Input_Pos < InputString.Length)
            {
                curr = InputString[Input_Pos];

                if (curr == 32)
                {
                    break;
                }

                if (curr > 96)
                {
                    curr = (char)(curr - 32);
                }

                tmpString.Append(curr);
                ++calc;
                ++Input_Pos;
            }

            OutputList.AddLast(tmpString.ToString());
            return (calc == size);
        }

        //bool GetAttr()
        //{
        //    char curr;
        //    bool Result;

        //    curr = InputString[Input_Pos];

        //    while (curr == 32)
        //    {
        //        //i++;
        //        ++Input_Pos;
        //        curr = InputString[Input_Pos];
        //    }
        //    if ((Input_Pos == InputString.Length) || (curr == ';'))
        //    {
        //        //Result = PR_VALID;
        //        // Result = ParseResult.PR_CODE;
        //        // break;
        //        return true;
        //    }

        //    Result = GetWord((int)Size.ATT_LEN);

        //    //if (Result)
        //    //{

        //    //}

        //    return Result;

        //}

        bool GetNum(out ushort value, ushort len)
        {
            char digit;
            int i;
            ushort tmp = 0;
            StringBuilder key = new StringBuilder();
            ushort size = 0; //number of digits of number

            if(Input_Pos < InputString.Length)
            {
                if (InputString[Input_Pos] == '@')
                {
                    ++Input_Pos;

                    while(Input_Pos < InputString.Length)
                    {
                        if(InputString[Input_Pos] == ' ')
                        {
                            break;
                        }

                        key.Append(InputString[Input_Pos]);
                        ++Input_Pos;
                    }

                    if(!Symbols.ContainsKey(key.ToString()))
                    {
                        Symbols.Add(key.ToString(), NextSymbol);
                        ++NextSymbol;
                    }
                    value = Symbols[key.ToString()];
                    return true;

                }                
            }

            value = 0;
                       
            for (i = 0; i < len; i++)
            {
                if (Input_Pos == InputString.Length)
                {
                    return true;
                }

                digit = InputString[Input_Pos];

                if (digit == ' ')
                {
                    return true;    //Space/EOL is fine - number of digits found
                }


                if (digit > 96)
                {
                    digit -= (char)32;
                }

                if ((digit < '0') || (((digit > '9') && (digit < 'A'))) || (digit > 'F'))
                {
                    return false; //Fail
                }

                value = (ushort)(value * 16);

                if ((digit > 47) && (digit < 58))
                {
                    tmp = (ushort)(digit - 48);
                }
                else
                {
                    tmp = (ushort)(digit - 55);
                }

                value += tmp;
                ++size;     //add one to the number of valid digits
                ++Input_Pos;
            }

            return true;    //number of digits found
        }

        void RemoveSpaces()
        {
            while ((Input_Pos < InputString.Length) && (InputString[Input_Pos] == ' '))
            {
                ++Input_Pos;
            }
        }

        bool GetString(out string value)
        {
            bool Result = false;
            StringBuilder tmpString = new StringBuilder();
            char c;

            if (InputString[Input_Pos] == '"')
            {
                ++Input_Pos;
                while (Input_Pos < InputString.Length)
                {
                    c = InputString[Input_Pos];
                    ++Input_Pos;
                    if (c == '"')
                    {
                        break;
                    }

                    tmpString.Append(c);

                }

                Result = true;

            }
            value = tmpString.ToString();
            return Result;
        }

        bool GetText()
        {
            //value = new byte[] { };
            var tmpText = new ArrayList();
            char curr;
            while (Input_Pos < InputString.Length)
            {
                curr = InputString[Input_Pos];

                tmpText.Add(atasciitocode(curr));
                //OutputBuffer_Pos = OutputBuffer_Pos.Next;
                //i++;
                ++Input_Pos;
            }
            //Result = ParseResult.PR_VALID;
            // OutputList.AddLast((byte[])tmpText.ToArray(typeof(byte)));
            OutputList.AddLast(tmpText.ToArray(typeof(byte)));
            return true;

        }

        bool GetData()
        {
            //value = new byte[] { };
            var tmpText = new ArrayList();
            ushort curr;
            while (Input_Pos < InputString.Length)
            {
                RemoveSpaces();
                if (Input_Pos < InputString.Length) //position may have changed after removing spaces
                {
                    if (GetNum(out curr, 2))
                    {
                        tmpText.Add((byte)curr);
                    }
                    else
                    {
                        return false;
                    }
                }

            }
            OutputList.AddLast(tmpText.ToArray(typeof(byte)));
            return true;

        }

        public byte atasciitocode(char ch)
        {
            byte tmp;
            tmp = (byte)(ch & 127);
            if (tmp < 32)
            {
                return (byte)(ch + 64);
            }

            if (tmp < 96)
            {
                return (byte)(ch - 32);
            }

            return (byte)ch;
        }

    }
}
