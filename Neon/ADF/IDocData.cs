using System;
using System.Collections.Generic;
using System.Text;

namespace ADF
{
    public interface IDocData
    {
        byte[] BinaryData();
        ushort MemADDR { get; set; }
        ushort c_sizeof();

    }
}
