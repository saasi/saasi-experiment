using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Threading.Tasks;

namespace Saasi.Shared.Workload
{
    public interface IWorkload
    {
        Task<ExecutionResult> Run(int time);
    }
}
