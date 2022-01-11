using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace ADF
{


    public class Document
    {       
        //How many bytes each Antic Mode takes
        public byte[] LineWidth = { 0, 0, 40, 40, 40, 40, 20, 20, 10, 10, 20, 20, 20, 40, 40, 40 };
        //How many lines each Antic Mode takes
        public byte[] LineHeight = { 1, 0, 8, 10, 8, 16, 8, 16, 8, 4, 4, 2, 1, 2, 1, 1 };
        //Antic Commands for Blank Lines
        public byte[] Blanks = { 0x00, 0x10, 0x20, 0x30, 0x40, 0x50, 0x60, 0x70 };

        //Document Memory
        public byte[] Data = new byte[(int)MEM.DEFAULT_HIGHMEM];

        public ushort LowRAM;
        public ushort HighRAM;

        //Display List Buffer
        byte[] DLBuffer = new byte[256];
        public byte DLBuffer_Len;

        //DLI Code
        public byte DLCode_Len;
        public int[] DLCode = new int[116];

        //DisplayList Interrupt
        public byte NumDLI;
        public byte LastDLILine;

        //Dynamic Load File
        public LoadFile FileCursor;
                
        //Current Address
        ushort ADDR;

        //Current Antic Settings
        public byte AnticMode;
        public byte NeedLMS;
        public ushort LineAddress; //Address of the last line

        //Current Display LineWidth
        public byte LineCursor;
        //Highest Display LineNumber for a page
        public byte LineLimit;
        //Current Screen Line
        public byte LineCount;

        //DATA
        public ushort DataCursor;

        //PAGE
        public Page FirstPage { get; set; }
        public List<DataBank> Bank = new List<DataBank> { new DataBank(0xFF, BankLevel.Level_ReadOnly, 0xE000) };
        public Page CurrentPage;
        public Link CurrentLink;

        public Document()
        {
            LowRAM = (ushort)MEM.DEFAULT_LOWMEM;
            HighRAM = (ushort)MEM.DEFAULT_HIGHMEM;
        }


        public ushort Document_Address(ushort Address)
        {
            return (ushort)(Address + 0x4000);  //Document starts at 16K
        }

        public bool CopyLine(byte[] Line, byte AnticMode, byte Alignment, byte Select, byte MarginLeft, byte MarginRight)
        {

            byte temp;
            byte width;

            ushort Boundary;
            byte cursor;
            byte ch;
            byte tmpWidth;
            byte tmpLeft;

            cursor = 0;

            AnticMode = (byte)(AnticMode % 16);
            this.AnticMode = AnticMode;

            //AnticMode = command_dict[ATT_MO] % 16;

            width = LineWidth[AnticMode];

            MarginRight = Math.Min(MarginRight, (byte)(width - 1));

            Select = (byte)((Select > 0) ? 128 : 0);

            if (MarginLeft > MarginRight)
            {
                //print(0,"Margin error \n");		
                return false;   //invalid line
            }


            //Switched from while - to - do-while to fix bug where an empty line is ignored - so look for bugs if this causes another issue
            do
            {
                if (LineCount + LineHeight[AnticMode] > LineLimit)
                {
                    //Error = ERR_OVERRUN;
                    LineAddress = 0; //Clear Line address so links can be added
                    throw new Exception("Error 08: Overrun margin bottom (#MB).");
                }

                //Need to add logic to handle crossing 4K Boundary (if (A & 0xF000) != (B x 0xF000) then need LMS)
                Boundary = (ushort)((LowRAM + width) & 0xF000);

                if ((LowRAM & 0xF000) != (int)Boundary)
                {
                    LowRAM = Boundary;
                    NeedLMS = 1;
                }

                ADDR = Mem_AllocLow(width);
                LineAddress = ADDR;

                if (NeedLMS > 0)
                {
                    AddDisplayLine(AnticMode, ADDR);
                    NeedLMS = 0;
                }
                else
                {
                    AddDisplayLine(AnticMode, 0);
                }

                //Alignment
                tmpWidth = (byte)((MarginRight + 1) - MarginLeft);
                temp = (byte)(Line.Length - cursor);
                temp = Math.Min(tmpWidth, temp);
                temp = (byte)(tmpWidth - temp);

                switch (Alignment)
                {
                    case 0: //Left Align		
                        tmpLeft = MarginLeft;
                        break;

                    case 1://Center Align						
                        temp = (byte)(temp / 2);
                        tmpLeft = (byte)(MarginLeft + temp);
                        break;

                    case 2://Right Align
                    default:
                        tmpLeft = (byte)(MarginLeft + temp);
                        break;
                }


                for (temp = 0; temp < tmpLeft; ++temp)
                {
                    Append(0);  //Fill current memory with a 0					
                }

                while ((temp <= MarginRight) && (cursor < Line.Length))
                {
                    ch = (byte)(Line[cursor] + Select);
                    Append(ch);
                    ++cursor;
                    ++temp;
                }

                while (temp < width)
                {
                    Append(0);
                    ++temp;
                }

            } while (cursor < Line.Length);

            return true;

        }

        public ushort Append(byte value)
        {
            Data[ADDR] = value;  //Fill current memory with a 0
            ++ADDR;
            return value;
        }

        public ushort Mem_AllocLow(ushort size)
        {
            ushort result;

            result = LowRAM;

            if ((LowRAM + size) < HighRAM)
            {
                LowRAM += size;
            }
            else
            {
                LowRAM = (ushort)(HighRAM - 1);
                //Error = ERR_MEM;
                throw new Exception("Error 03: Out of Memory.");
            }

            return result;
        }

        public ushort Mem_AllocHigh(ushort size)
        {
            //int result;

            if ((LowRAM + size) < HighRAM)
            {
                //result = HighRAM - size;
                HighRAM = (ushort)(HighRAM - size);
            }
            else
            {
                HighRAM = (ushort)(LowRAM + 1);
                //Error = ERR_MEM;
                throw new Exception("Error 03: Out of Memory.");
                
            }

            return HighRAM;
        }


        public void AddDisplayLine(byte mode, ushort LMS)
        {
            if (NumDLI > 0)
            {
                if (LastDLILine > 0)
                {
                    DLBuffer[LastDLILine] = (byte)(DLBuffer[LastDLILine] + 0x80);  //Mark the last DLI line to be DLI so we don't have that DLI issue where it is the following line
                }

                --NumDLI;
            }

            LastDLILine = DLBuffer_Len;

            if (LMS > 0)
            {
                //if((Mem_Check(3) && ((DLBuffer_Len + 3) < MAX_DLSIZE)))
                if ((DLBuffer_Len + 3) < (int)MEM.MAX_DLSIZE)
                {
                    DLBuffer[DLBuffer_Len] = (byte)(mode | 0x40); // Set LMS Bit
                    ++DLBuffer_Len;
                    DLBuffer[DLBuffer_Len] = (byte)(LMS % 256);
                    ++DLBuffer_Len;
                    DLBuffer[DLBuffer_Len] = (byte)(LMS / 256);
                    ++DLBuffer_Len;
                }
                else
                {
                    //Error = ERR_MEM;
                    throw new Exception("Error 03: Out of Memory.");
                }
            }
            else
            {
                //if(Mem_Check(1)&& ((DLBuffer_Len < MAX_DLSIZE)))
                if ((DLBuffer_Len < (int)MEM.MAX_DLSIZE))
                {
                    DLBuffer[DLBuffer_Len] = mode;
                    ++DLBuffer_Len;
                }
                else
                {
                    //Error = ERR_MEM;
                    throw new Exception("Error 03: Out of Memory.");
                }
            }
            if (mode < 16)
            {
                LineCount += LineHeight[mode];
            }
            else
            {
                LineCount += (byte)((mode / 16) + 1);
            }
        }

        public DataBank FindDataBank(byte ID)
        {
            foreach (DataBank B in Bank)
            {
                if (B.ID == ID)
                {
                    return B;
                }
            }

            return null;
        }

        public void BlankLines(byte size)
        {
            byte t;

            while (size > 0)
            {
                t = (byte)((size > 7) ? 8 : size);
                AddDisplayLine(Blanks[t - 1], 0);
                size -= t;
            }
        }

        public void AddDataObject(Object newObj)
        {
    
            ADDR = ((IDocData)newObj).MemADDR;
            //byte[] test = ((IDocData)newObj).BinaryData();

            foreach (byte b in ((IDocData)newObj).BinaryData())
            {
                Data[ADDR] = b;
                ++ADDR;
            }

        }

        public bool AddEmptyLine(byte Mode)
        {
            byte temp;
            byte width;
            ushort ADDR;
            ushort Start;
            ushort Boundary;

            width = LineWidth[Mode];
            LineAddress = 0;

            //Need to add logic to handle crossing 4K Boundary (if (A & 0xF000) != (B x 0xF000) then need LMS)

            Boundary = (ushort)((LowRAM + width) & 0xF000);

            if ((LowRAM & 0xF000) != (int)Boundary)
            {
                LowRAM = Boundary;
                NeedLMS = 1;
            }


            //Need to add logic to handle crossing 4K Boundary (if (A & 0xF000) != (B x 0xF000) then need LMS)
            ADDR = Mem_AllocLow(width);
            Start = ADDR;

            for (temp = 0; temp < width; ++temp)
            {
                Data[ADDR] = 0;  //Fill current memory with a 0
                ++ADDR;
            }

            if (NeedLMS > 0)
            {
                AddDisplayLine(Mode, Start);
                NeedLMS = 0;
            }
            else
            {
                AddDisplayLine(Mode, 0);
            }

            return true;
        }


        public void UpdateDataObject(Object newObj)
        {
            ushort i;

            i = ((IDocData)newObj).MemADDR;

            foreach (byte b in ((IDocData)newObj).BinaryData())
            {
                Data[i] = b;
                ++i;
            }

        }

        public void FinishPage()
        {
            ushort ADDR;
            ushort Start;
            ushort End;
            ushort Boundary;

            byte i;
            byte  DLSize;


            //We need to make this more robust so it doesn't cross a 1K boundary
            DLSize = DLBuffer_Len;           

            Boundary = (ushort)((LowRAM + DLSize + 3) & 0xFC00);

            if ((LowRAM & 0xFC00) != Boundary)
            {
                LowRAM = Boundary;
            }

            End = (ushort)(LowRAM + DLSize);
            ADDR =  Mem_AllocLow((ushort)(DLSize + 3));
            Start = ADDR;

            //Set the Display List Pointer to the end of the Page Data
            CurrentPage.Pointer.DisplayList = ADDR;

            //Copy the Display List to the end of the Page Data
            i = 0;
            while (ADDR < End)
            {
                Data[ADDR] = DLBuffer[i];
                ++ADDR;
                ++i;
            }         

            //Add trailer

            Data[ADDR] = 0x41;               //Jump and wait for vertical blank
            ++ADDR;
            Data[ADDR] = (byte)(Start % 256);
            ++ADDR;
            Data[ADDR] = (byte)(Start / 256);
            ++ADDR;

            //Copy call list from DLCode
            if (DLCode_Len > 0)
            {
                End = (ushort)(LowRAM + (DLCode_Len * 2));
                ADDR = Mem_AllocLow((ushort)(DLCode_Len * 2));
                CurrentPage.Pointer.DLInterrupt = ADDR; 

                i = 0;
                while (ADDR < End)
                {
                    Data[ADDR] = (byte)(DLCode[i] % 256);
                    ++ADDR;
                    Data[ADDR] = (byte)(DLCode[i] / 256);
                    ++ADDR;
                    ++i;
                }
            }

            UpdateDataObject(CurrentPage);

        }

       public bool CopyDataLow(byte[] Buff)
        {
            int i;           

            for (i = 0; i < Buff.Length; i++)
            {
                if (DataCursor >= HighRAM)
                {
                   LowRAM = DataCursor;
                    return false;
                }

                Data[DataCursor] = Buff[i];
                ++DataCursor;
            }

            if (DataCursor > LowRAM)
            {
                LowRAM = DataCursor;
            }

            return true;
        }

        public void SaveADX(string FileSpec)
        {

            //Uses OutputBuffer
            ushort len;
            char status;
            byte[] tmpArray = new byte[2];

            FileStream fs = new FileStream(FileSpec, FileMode.Create);

            //if (status != 1)
            //{
            //    close(1);
            //    return status;
            //}


            //printrec(1, OutputBuffer, 2);
            fs.WriteByte((byte)Version.MAJOR);
            fs.WriteByte((byte)Version.MINOR);
            fs.WriteByte((byte)ADF.ATASCII.EOL);

            //len = LowRAM - DEFAULT_LOWMEM;

            len = (ushort)(LowRAM - MEM.DEFAULT_LOWMEM);

            // putchr(1, (char*)&len, 2);

            fs.WriteByte((byte)len);
            fs.WriteByte((byte)(len >> 8));

            // putchr(1, (char*)DEFAULT_LOWMEM, len);
            fs.Write(Data, (ushort)MEM.DEFAULT_LOWMEM, len);

            //len = DEFAULT_HIGHMEM - HighRAM;

            len = (ushort)(MEM.DEFAULT_HIGHMEM - HighRAM);

            //putchr(1, (char*)&len, 2);
            fs.WriteByte((byte)len);
            fs.WriteByte((byte)(len >> 8));

            // putchr(1, (char*)HighRAM, len);

            if (len == 0)
            {
                fs.WriteByte(0);
            }
            else
            {
                fs.Write(Data, HighRAM, len);
            }


            //fs.WriteByte(16);

            //putchr(1, (char*)&FirstPage, 2);

            fs.WriteByte((byte)FirstPage.MemADDR);
            fs.WriteByte((byte)(FirstPage.MemADDR >> 8));

            //putchr(1, (char*)&FileCursor, 2);

            if (FileCursor == null)
            {
                fs.WriteByte(0);
                fs.WriteByte(0);
            }
            else
            {
                fs.WriteByte((byte)FileCursor.MemADDR);
                fs.WriteByte((byte)(FileCursor.MemADDR >> 8));
            }

            fs.Close();

            return;

        }
    }

}

