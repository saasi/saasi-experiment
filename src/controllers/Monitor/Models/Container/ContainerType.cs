using System;
using System.Collections.Generic;
using System.Text;

namespace Monitor
{
    public enum ContainerType
    {
        Unknown = 0,
        IOMicroservice = 1,
        CPUMicroservice = 2,
        MemoryMicroservice = 3,
        BusinessFunction = 4,
        BusinessMicroservice = 5
    }
}
