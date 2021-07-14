using System.Collections.Generic;
using System.Diagnostics;

namespace CurePlease.Infrastructure
{
    public interface IProcessManager
    {
        List<ProcessDetails> CheckForDLLFiles(out string errorMessage);
    }
}
