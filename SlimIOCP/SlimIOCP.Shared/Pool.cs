using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SlimIOCP
{
    public class Pool
    {
        protected int Pooled;
        protected int Allocated;

        public int Used
        {
            get { return (Allocated - Pooled); }
        }
    }
}
