using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace ADF
{
    public enum FileFormat { ATASCII, CRLF, LF }
    public class IO
    {

        public static FileFormat EndingFormat;

        public static FileStream OpenSource(string FileName, FileFormat Format = FileFormat.ATASCII)
        {
            EndingFormat = Format;

            FileStream fs = new FileStream(FileName, FileMode.Open);

            return fs;
        }

        public static List<string> LoadFile(FileStream fs)
        {
            List<string> tmpList = new List<string>();
            int ch;
            byte b;
            StringBuilder tmpString = new StringBuilder();

            do
            {
                while (true)
                {
                    ch = fs.ReadByte();
                    if (ch == -1)
                    {
                        break;
                    }

                    b = (byte)ch;

                    if (b == 155) //ATASCII RETURN
                    {
                        if (EndingFormat == FileFormat.ATASCII)
                        {
                            break;
                        }
                    }

                    if (b == 13) //CR
                    {
                        if (EndingFormat == FileFormat.CRLF)
                        {
                            ch = fs.ReadByte(); //read the next byte which should be LF                            
                            break;
                        }
                    }

                    if (b == 10) //LF
                    {
                        if (EndingFormat == FileFormat.LF)
                        {
                            break;
                        }
                    }

                    tmpString.Append(Convert.ToChar(ch));
                }
                tmpList.Add(tmpString.ToString());
                tmpString.Clear();

            } while (ch != -1);

            return tmpList;
        }



        public static bool ReadRecord(FileStream fs, ref byte[] OutputRecord)
        {
            List<byte> Record = new List<byte>();
            int ch;
            byte b;
            bool Done = false;
            bool Result = true;

            while (!Done)
            {
                ch = fs.ReadByte();
                if (ch == -1)
                {
                    Done = true;
                    Result = false;
                    break;
                }

                b = (byte)ch;

                if (b == 155) //ATASCII RETURN
                {
                    if (EndingFormat == FileFormat.ATASCII)
                    {
                        Done = true;
                        break;
                    }
                }

                if (b == 13) //CR
                {
                    if (EndingFormat == FileFormat.CRLF)
                    {
                        ch = fs.ReadByte(); //read the next byte which should be LF
                        Done = true;
                        break;
                    }
                }

                if (b == 10) //LF
                {
                    if (EndingFormat == FileFormat.LF)
                    {
                        Done = true;
                        break;
                    }
                }

                Record.Add(b);

            }

            Record.Add(155); //Add ATASCII RETURN to end record
            OutputRecord = Record.ToArray();
            return Result;
        }

        public static bool ReadRecord(FileStream fs, ref LinkedList<byte> lstRecord)
        {
            lstRecord = new LinkedList<byte>();
            int ch;
            byte b;
            bool Done = false;
            bool Result = true;

            while (!Done)
            {
                ch = fs.ReadByte();
                if (ch == -1)
                {
                    Done = true;
                    Result = false;
                    break;
                }

                b = (byte)ch;

                if (b == 155) //ATASCII RETURN
                {
                    if (EndingFormat == FileFormat.ATASCII)
                    {
                        Done = true;
                        break;
                    }
                }

                if (b == 13) //CR
                {
                    if (EndingFormat == FileFormat.CRLF)
                    {
                        ch = fs.ReadByte(); //read the next byte which should be LF
                        Done = true;
                        break;
                    }
                }

                if (b == 10) //LF
                {
                    if (EndingFormat == FileFormat.LF)
                    {
                        Done = true;
                        break;
                    }
                }

                lstRecord.AddLast(b);

            }

            lstRecord.AddLast((byte)ATASCII.EOL); //Add ATASCII RETURN to end record
                                                  //OutputRecord = Record.ToArray();
            return Result;
        }

        public static void Close(FileStream fs)
        {
            fs.Close();
        }

        public static string RecordToString(byte[] record)
        {
            StringBuilder outstring = new StringBuilder();

            for (int i = 0; i < record.Length - 1; i++)
            {
                // outstring.Append(char.ConvertToUtf32(record[i]));
                outstring.Append(Convert.ToChar(record[i]));
            }

            return outstring.ToString();
        }

        public static string RecordToString(LinkedList<byte> record)
        {
            StringBuilder outstring = new StringBuilder();
            LinkedListNode<byte> CurrentRecord = record.First;

            while (CurrentRecord.Value != (byte)ATASCII.EOL)
            {
                // outstring.Append(char.ConvertToUtf32(record[i]));
                outstring.Append(Convert.ToChar(CurrentRecord.Value));
                CurrentRecord = CurrentRecord.Next;
            }

            return outstring.ToString();
        }


    }
}


