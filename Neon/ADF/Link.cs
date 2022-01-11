using System;
using System.Collections.Generic;
using System.Text;

namespace ADF
{
    public class Link  :IDocData
    {
        public Link NextLink;
        public Link PreviousLink;
        public string PageName; //Name of page in document to link to
        public string FileSpec = "";

        public ushort MemADDR { get; set; }

        /*
        public ushort AbsoluteADDR
        {
            get { return (ushort)(MemADDR + 0x4000); }
        }
        */

        public Pointers Pointer;

        public struct Pointers
        {
            public ushort NextLink;
            public ushort PreviousLink;
            public ushort DisplayLine;
            public byte Start;
            public byte End;
        }

        public ushort c_sizeof()
        {
            //The size of the following structure
            /*
	                Link *NextLink;
	                Link *PreviousLink;
	                char *DisplayLine;
	                char Start;
	                char End;	
	                char PageName[PAGENAME_LEN]; //Name of page in document to link to
	                char FileSpec[1]; 
            */

            return (ushort)(((ushort)FileSpec.Length + 1) + (ushort)Size.PAGENAME_LEN + 8);
        }

        public byte[] BinaryData()
        {
            byte[] link = new byte[c_sizeof()];
            byte i, x;

            x = 0;

            link[x] = (byte)(Pointer.NextLink % 256);
            ++x;
            link[x] = (byte)(Pointer.NextLink / 256);
            ++x;

            link[x] = (byte)(Pointer.PreviousLink % 256);
            ++x;
            link[x] = (byte)(Pointer.PreviousLink / 256);
            ++x;

            link[x] = (byte)(Pointer.DisplayLine % 256);
            ++x;
            link[x] = (byte)(Pointer.DisplayLine / 256);
            ++x;

            link[x] = Pointer.Start;
            ++x;
            link[x] = Pointer.End;
            ++x;



            PageName = PageName.PadRight(8, '\0').Substring(0, (int)Size.PAGENAME_LEN - 1);

            for (i = 0; i < (int)Size.PAGENAME_LEN - 1; i++)
            {
                link[x] = (byte)PageName[i];
                ++x;
            }

            link[x] = (byte)'\0';
            ++x;

            for (i = 0; i < FileSpec.Length; i++)
            {
                link[x] = (byte)FileSpec[i];
                ++x;
            }

            link[x] = (byte)'\0';
            ++x;

            return link;
        }
    }
}
