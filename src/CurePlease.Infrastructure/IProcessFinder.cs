using System.Collections.Generic;
using System.Diagnostics;

namespace CurePlease.Infrastructure
{
    public interface IProcessUtilities
    {
        IEnumerable<Process> GetRunningFFXIProcesses();

        bool ProcessHasModuleWithName(Process process, string fileName);
    }
}