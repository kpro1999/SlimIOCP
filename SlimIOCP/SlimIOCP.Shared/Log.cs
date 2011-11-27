using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SlimIOCP
{
    public static class Log
    {
        public static Action<string> Logger;

        public static void Info(string msg)
        {
            if (Logger != null)
            {
                Logger(msg + "\r\n");
            }
        }
    }
}
