using System;
using System.Collections.Generic;
using System.Text;

namespace ADF
{
    public class LoadFile :IDocData
    {
        public LoadFile NextFile;
        public ushort StartAddr;
        public ushort Size;
        public string FileName = "";

        public ushort MemADDR { get; set; }

        public ushort NextADDR;


        public ushort c_sizeof()
        {
            //The size of the following structure
            /*
                struct LoadFile
                {
	                LoadFile *NextFile;
	                int StartAddr;
	                int Size;
	                char FileSpec[1]; //this need to be dynamically sized
                };
            */

            return (ushort)(FileName.Length + 7);
        }

        public byte[] BinaryData()
        {
            byte[] p = new byte[c_sizeof()];
            byte i, x;

            x = 0;

            p[x] = (byte)(NextADDR % 256);
            ++x;
            p[x] = (byte)(NextADDR / 256);
            ++x;
            p[x] = (byte)(StartAddr % 256);
            ++x;
            p[x] = (byte)(StartAddr / 256);
            ++x;
            p[x] = (byte)(Size % 256);
            ++x;
            p[x] = (byte)(Size / 256);
            ++x;                           

            for (i = 0; i < FileName.Length; i++)
            {
                p[x] = (byte)FileName[i];
                ++x;
            }

            p[x] = (byte)'\0';
            ++x;

            return p;
        }


    }
}
