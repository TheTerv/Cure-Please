
namespace CurePlease.Model.Config
{
    public class CureConfig
    {
        public bool[] EnabledCureTiers { get; set; }
        public int[] CureTierThresholds { get; set; }
        public int CureHealthPercent { get; set; }
        public bool[] EnabledCuragaTiers { get; set; }
        public int[] CuragaTierThresholds { get; set; }
        public int CuragaMinPlayers { get; set; }
        public int CuragaHealthPercent { get; set; }

        public bool EnableOutOfPartyHealing { get; set; }
        public int MonitoredCurePercentage { get; set; }

        public bool MonitoredPriorityEnabled { get; set; }

        public bool OverCureEnabled { get; set; }
        public bool UnderCureEnabled { get; set; }

        public bool CuragaSpecifiedEnabled { get; set; }
        public string CuragaSpecifiedName { get; set; }
    }
}
