using System;
using System.Collections.Generic;
using System.Text;

namespace ADF
{
    public class Page : IDocData
    {


        //public Page NextPage
        //{
        //    get { return _NextPage; }
        //    set
        //    {
        //        _NextPage = value;
        //        Pointer.NextPage = value.MemADDR;
        //    }
        //}

        //public Link LinkList
        //{
        //    get { return _LinkList; }
        //    set
        //    {
        //        _LinkList = value;
        //        if(value == null)
        //        {
        //            Pointer.LinkList = 0;
        //        }
        //        else
        //        {
        //            Pointer.LinkList = value.MemADDR;
        //        }

        //    }
        //}

        public Page NextPage;
        public byte[] PageStyle = new byte[(byte)Size.HWR_COUNT];
        public string Name;
        public Link LinkList;
        public ushort MemADDR { get; set; }  //Memory address in the DOCUMENT this Page will be stored at        

        /*
        public ushort AbsoluteADDR
        {
            get { return (ushort)(MemADDR + 0x4000); }
        }
        */

        public Pointers Pointer;

        public struct Pointers
        {
            public ushort NextPage;
            public ushort LinkList;
            public ushort DisplayList;
            public ushort DLInterrupt;
        }

        public ushort c_sizeof()
        {
            //The size of the following structure
            /*
                struct Page
                {
                Page *NextPage;
                char PageStyle[HWR_COUNT];
                char Name[PAGENAME_LEN];
                Link *LinkList;
                char *DisplayList;	
                char *DLInterrupt;
                };
            */

            return (ushort)Size.HWR_COUNT + (ushort)Size.PAGENAME_LEN + 8;
        }

        public byte[] BinaryData()
        {
            byte[] p = new byte[c_sizeof()];
            byte i, x;

            x = 0;

            p[x] = (byte)(Pointer.NextPage % 256);
            ++x;
            p[x] = (byte)(Pointer.NextPage / 256);
            ++x;

            for (i = 0; i < (int)Size.HWR_COUNT; i++)
            {
                p[x] = PageStyle[i];
                ++x;
            }

            Name = Name.PadRight(8, '\0').Substring(0, (int)Size.PAGENAME_LEN - 1);

            for (i = 0; i < (int)Size.PAGENAME_LEN - 1; i++)
            {
                p[x] = (byte)Name[i];
                ++x;
            }

            p[x] = (byte)'\0';
            ++x;

            p[x] = (byte)(Pointer.LinkList % 256);
            ++x;
            p[x] = (byte)(Pointer.LinkList / 256);
            ++x;
            p[x] = (byte)(Pointer.DisplayList % 256);
            ++x;
            p[x] = (byte)(Pointer.DisplayList / 256);
            ++x;
            p[x] = (byte)(Pointer.DLInterrupt % 256);
            ++x;
            p[x] = (byte)(Pointer.DLInterrupt / 256);
            ++x;

            return p;
        }

    
    }
}
