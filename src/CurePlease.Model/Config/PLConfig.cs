
namespace CurePlease.Model.Config
{
    public class PLConfig
    {
        public bool PLSilenceItemEnabled { get; set; }
        public int PLSilenceItem { get; set; }
        public bool PLDoomItemEnabled { get; set; }
        public int PLDoomItem { get; set; }
        public bool DivineSeal { get; set; }
        public bool Convert { get; set; }
        public decimal ConvertMP { get; set; }
        public decimal MinCastingMP { get; set; }
        public bool LowMPEnabled { get; set; }
        public bool HealLowMPEnabled { get; set; }
        public decimal HealMPThreshold { get; set; }
        public bool StandMPEnabled { get; set; }
        public decimal StandMPThreshold { get; set; }

        public bool BarElementEnabled { get; set; }
        public bool AOEBarElementEnabled { get; set; }
        public string BarElementSpell { get; set; }
        public bool BarStatusEnabled { get; set; }
        public bool AOEBarStatusEnabled { get; set; }
        public string BarStatusSpell { get; set; }
        public bool EnSpellEnabled { get; set; }
        public string EnSpell { get; set; }
        public bool EnspellAccession { get; set; }
        public bool EnspellPerpetuance { get; set; }
        public bool StormSpellEnabled { get; set; }
        public string StormSpell { get; set; }
        public bool GainBoostSpellEnabled { get; set; }
        public string GainBoostSpell { get; set; }

        public bool Composure { get; set; }
        public bool LightArts { get; set; }
        public bool AddendumWhite { get; set; }
        public bool DarkArts { get; set; }
        public bool AddendumBlack { get; set; }
        public bool ShellraEnabled { get; set; }
        public int ShellraLevel { get; set; }
        public bool ProtectraEnabled { get; set; }
        public int ProtectraLevel { get; set; }
        public bool AccessionEnabled { get; set; }
        public bool BarElementAccession { get; set; }
        public bool BarStatusAccession { get; set; }
        public bool PerpetuanceEnabled { get; set;  }
        public bool BarElemenetPerpetuance { get; set; }
        public bool BarStatusPerpetuance { get; set; }
        public bool StormspellAccession { get; set; }
        public bool StormspellPerpetuance { get; set; }

        public bool ProtectEnabled { get; set; }
        public string ProtectSpell { get; set; }
        public bool ShellEnabled { get; set; }
        public string ShellSpell { get; set; }
        public bool AccessionProtectShell { get; set; }
        public bool ReraiseEnabled { get; set; }
        public string ReraiseSpell { get; set; }
        public bool EnlightenmentReraise { get; set; }

        public bool UtsusemiEnabled { get; set; }

        public bool BlinkEnabled { get; set; }
        public bool BlinkAccession { get; set; }
        public bool BlinkPerpetuance { get; set; }

        public bool PhalanxEnabled { get; set; }
        public bool PhalanxAccession { get; set; }
        public bool PhalanxPerpetuance { get; set; }

        public bool RefreshEnabled { get; set; }
        public string RefreshSpell { get; set; }
        public bool RefreshAccession { get; set; }
        public bool RefreshPerpetuance { get; set; }

        public bool RegenEnabled { get; set; }
        public string RegenSpell { get; set; }
        public bool RegenAccession { get; set; }
        public bool RegenPerpetuance { get; set; }

        public bool AdloquiumEnabled { get; set; }
        public bool AdloquiumAccession { get; set; }
        public bool AdloquiumPerpetuance { get; set; }

        public bool StoneskinEnabled { get; set; }
        public bool StoneskinAccession { get; set; }
        public bool StoneskinPerpetuance { get; set; }

        public bool AquaveilEnabled { get; set; }
        public bool AquaveilAccession { get; set; }
        public bool AquaveilPerpetuance { get; set; }

        public bool KlimaformEnabled { get; set; }

        public bool TemperEnabled { get; set; }
        public string TemperSpell { get; set; }

        public bool HasteEnabled { get; set; }
        public string HasteSpell { get; set; }

        public bool SpikesEnabled { get; set; }
        public string SpikesSpell { get; set; }

        public bool AuspiceEnabled { get; set; }

        public bool AutoTargetEnabled { get; set; }
        public string AutoTargetSpell { get; set; }
        public string AutoTargetTarget { get; set; }
        public bool AssistSpecifiedTarget { get; set; }
        public bool PartyBasedHateSpell { get; set; }

        public bool AfflatusSolaceEnabled { get; set; }
        public bool AfflatusMiseryEnabled { get; set; }

        public bool SublimationEnabled { get; set; }
        public decimal SublimationMPLossThreshold { get; set; }

        public bool DivineCaressEnabled { get; set; }
        public bool DebuffsEnabled { get; set; }

        public bool DevotionEnabled { get; set; }
        public bool DevotionWhenEngaged { get; set; }
        public decimal DevotionMPThreshold { get; set; }
        public bool DevotionSpecifiedTarget { get; set; }
        public string DevotionTargetName { get; set; }
    }
}
