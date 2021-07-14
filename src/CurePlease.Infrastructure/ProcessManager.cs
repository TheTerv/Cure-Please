using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace CurePlease.Infrastructure
{
    public class ProcessManager : IProcessManager
    {
        public const string ERROR_NO_FFXI_PROCESSES_FOUND = "Could not find a running instance of FFXI. Please load FFXI first";
        public const string ERROR_NO_WRAPPERS_FOUND = "Unable to identify Windower or Ashita. CurePlease requires one of these FFXi wrappers";
        public const string WRAPPER_WINDOWER = "Windower";
        public const string WRAPPER_ASHITA = "Ashita";

        private IProcessUtilities _ProcessUtilities;

        public ProcessManager(IProcessUtilities processFinder)
        {
            _ProcessUtilities = processFinder;
        }

        public List<ProcessDetails> CheckForDLLFiles(out string errorMessage)
        {
            errorMessage = string.Empty;

            var results = new List<ProcessDetails>();

            IEnumerable<Process> pol = _ProcessUtilities.GetRunningFFXIProcesses();

            if (pol == null || !pol.Any())
            {
                errorMessage = ERROR_NO_FFXI_PROCESSES_FOUND;
                return results;
            }

            foreach (Process process in pol)
            {
                if (_ProcessUtilities.ProcessHasModuleWithName(process, "Ashita.dll"))
                {
                    results.Add(new ProcessDetails(process.MainWindowTitle, process.Id, WRAPPER_ASHITA));
                }
                else if (_ProcessUtilities.ProcessHasModuleWithName(process, "\\Hook.dll"))
                {
                    results.Add(new ProcessDetails(process.MainWindowTitle, process.Id, WRAPPER_WINDOWER));
                }
            }

            if (!results.Any())
            {
                errorMessage = ERROR_NO_WRAPPERS_FOUND;
            }

            return results;
        }
    }
}
