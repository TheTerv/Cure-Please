using CurePlease.Model.Constants;
using EliteMMO.API;
using System.Collections.Generic;

namespace CurePlease.Model
{
    // TODO: Mapping of Debuffs -> Cure spell + priority
    public static class Data
    {
        public static Dictionary<string, int> SpellCosts = new Dictionary<string, int> {
            { Spells.Cure, 8 } ,
            { Spells.Cure_II, 24 },
            { Spells.Cure_III, 46 },
            { Spells.Cure_IV, 88 },
            { Spells.Cure_V, 125 },
            { Spells.Cure_VI, 227 },
            { Spells.Curaga, 60 },
            { Spells.Curaga_II, 120 },
            { Spells.Curaga_III, 180 },
            { Spells.Curaga_IV, 260 },
            { Spells.Curaga_V, 380 },
            { Spells.Regen, 15 },
            { Spells.Regen_II, 36 },
            { Spells.Regen_III, 64 },
            { Spells.Regen_IV, 82 },
            { Spells.Regen_V, 100 },
            { Spells.Reraise, 150 },
            { Spells.Reraise_II, 150 },
            { Spells.Reraise_III, 150 },
            { Spells.Reraise_IV, 150 },
        };

        public static string[] CureTiers = { Spells.Cure, Spells.Cure_II, Spells.Cure_III, Spells.Cure_IV, Spells.Cure_V, Spells.Cure_VI };

        public static string[] CuragaTiers = { Spells.Curaga, Spells.Curaga_II, Spells.Curaga_III, Spells.Curaga_IV, Spells.Curaga_V };

        public static string[] ProtectTiers = { Spells.Protect, Spells.Protect_II, Spells.Protect_III, Spells.Protect_IV, Spells.Protect_V };
        public static string[] ShellTiers = { Spells.Shell, Spells.Shell_II, Spells.Shell_III, Spells.Shell_IV, Spells.Shell_V };
       
        public static string[] ProtectraTiers = { Spells.Protectra, Spells.Protectra_II, Spells.Protectra_III, Spells.Protectra_IV, Spells.Protectra_V };
        public static string[] ShellraTiers = { Spells.Shellra, Spells.Shellra_II, Spells.Shellra_III, Spells.Shellra_IV, Spells.Shellra_V };

        public static string[] ReraiseTiers = { Spells.Reraise, Spells.Reraise_II, Spells.Reraise_III, Spells.Reraise_IV };

        public static string[] RegenTiers = { Spells.Regen, Spells.Regen_II, Spells.Regen_III, Spells.Regen_IV, Spells.Regen_V };
        public static string[] RefreshTiers = { Spells.Refresh, Spells.Refresh_II, Spells.Refresh_III };

        public static string[] GainBoostSpells = {
            Spells.Gain_STR,Spells.Gain_DEX, Spells.Gain_VIT, Spells.Gain_AGI, Spells.Gain_INT, Spells.Gain_MND, Spells.Gain_CHR,
            Spells.Boost_STR, Spells.Boost_DEX, Spells.Boost_VIT, Spells.Boost_AGI, Spells.Boost_INT, Spells.Boost_MND, Spells.Boost_CHR
        };

        //TODO: Add storms, and fix storm casting.
        public static Dictionary<string, StatusEffect> SpellEffects = new Dictionary<string, StatusEffect>
        {
            { Spells.Gain_STR, StatusEffect.STR_Boost2 },
            { Spells.Gain_DEX, StatusEffect.DEX_Boost2 },
            { Spells.Gain_VIT, StatusEffect.VIT_Boost2 },
            { Spells.Gain_AGI, StatusEffect.AGI_Boost2 },
            { Spells.Gain_INT, StatusEffect.INT_Boost2 },
            { Spells.Gain_MND, StatusEffect.MND_Boost2 },
            { Spells.Gain_CHR, StatusEffect.CHR_Boost2 },
            { Spells.Boost_STR, StatusEffect.STR_Boost2 },
            { Spells.Boost_DEX, StatusEffect.DEX_Boost2 },
            { Spells.Boost_VIT, StatusEffect.VIT_Boost2 },
            { Spells.Boost_AGI, StatusEffect.AGI_Boost2 },
            { Spells.Boost_INT, StatusEffect.INT_Boost2 },
            { Spells.Boost_MND, StatusEffect.MND_Boost2 },
            { Spells.Boost_CHR, StatusEffect.CHR_Boost2 },
            { Spells.AuroraStorm }
        };

        public static StatusEffect[] GainBoostEffects =
        {
            StatusEffect.STR_Boost2, StatusEffect.DEX_Boost2, StatusEffect.VIT_Boost2, StatusEffect.AGI_Boost2, StatusEffect.INT_Boost2, StatusEffect.MND_Boost2, StatusEffect.CHR_Boost2
        };
    }
}
