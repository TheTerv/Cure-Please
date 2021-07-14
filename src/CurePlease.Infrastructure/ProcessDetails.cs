namespace CurePlease.Infrastructure
{
    public class ProcessDetails
    {
        public ProcessDetails(string processName, int processId, string wrapperMode)
        {
            ProcessName = processName;
            ProcessId = processId;
            WrapperMode = wrapperMode;
        }

        public string ProcessName { get; set; }
        public int ProcessId { get; set; }
        public string WrapperMode { get; set; }
    }
}
