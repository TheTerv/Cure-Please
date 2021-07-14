using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace CurePlease.Infrastructure
{
    public class ProcessUtilities : IProcessUtilities
    {
        public IEnumerable<Process> GetRunningFFXIProcesses()
        {
            return Process.GetProcessesByName("pol").Union(Process.GetProcessesByName("xiloader")).Union(Process.GetProcessesByName("edenxi"));
        }

        public bool ProcessHasModuleWithName(Process process, string fileName)
        {
            return process.Modules.OfType<ProcessModule>().Any(process => process.FileName.Contains(fileName));
        }
    }
}
