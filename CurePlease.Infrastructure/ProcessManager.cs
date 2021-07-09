using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace CurePlease.Infrastructure
{
    public class ProcessManager
    {
        public static List<ProcessDetails> CheckForDLLFiles(out string wrapper, out string errorMessage)
        {
            wrapper = string.Empty;
            errorMessage = string.Empty;

            var results = new List<ProcessDetails>();

            IEnumerable<Process> pol = Process.GetProcessesByName("pol").Union(Process.GetProcessesByName("xiloader")).Union(Process.GetProcessesByName("edenxi"));

            if (!pol.Any())
            {
                errorMessage = "Could not find a running instance of FFXI. Please load FFXI first";
            }

            foreach (Process process in pol)
            {
                for (int i = 0; i < process.Modules.Count; i++)
                {
                    if (process.Modules[i].FileName.Contains("Ashita.dll"))
                    {
                        wrapper = "Ashita";
                        results.Add(new ProcessDetails(process.MainWindowTitle, process.Id, "Ashita"));
                        break;
                    }
                    else if (process.Modules[i].FileName.Contains("\\Hook.dll"))
                    {
                        wrapper = "Windower";
                        results.Add(new ProcessDetails(process.MainWindowTitle, process.Id, "Windower"));
                        break;
                    }
                }
            }

            if (!results.Any())
            {
                errorMessage = "Unable to identify Windower or Ashita. CurePlease requires one of these FFXi wrappers";
            }

            return results;
        }
    }
}
