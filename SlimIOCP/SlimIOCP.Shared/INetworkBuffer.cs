using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SlimIOCP
{
    public interface INetworkBuffer
    {
        int BytesTransferred { get; }
        object Tag { get; }
    }
}
