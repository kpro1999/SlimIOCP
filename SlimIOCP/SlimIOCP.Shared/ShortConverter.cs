using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace SlimIOCP
{
    [StructLayout(LayoutKind.Explicit)]
    public struct ShortConverter
    {
        [FieldOffset(0)] public ushort Short;
        [FieldOffset(0)] public byte Byte0;
        [FieldOffset(1)] public byte Byte1;
    }
}
