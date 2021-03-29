using CurePlease.Model.Constants;
using EliteMMO.API;
using System.Collections.Generic;

namespace CurePlease.Model
{
    public static class Data
    {
        public static string[] CureTiers = { Spells.Cure, Spells.Cure_II, Spells.Cure_III, Spells.Cure_IV, Spells.Cure_V, Spells.Cure_VI };
        public static Dictionary<string, int> SpellCosts = new Dictionary<string, int> { 
            { Spells.Cure, 8 } ,
            { Spells.Cure_II, 24 },
            { Spells.Cure_III, 46 },
            { Spells.Cure_IV, 88 },
            { Spells.Cure_V, 125 },
            { Spells.Cure_VI, 227 }
        };

        public static string[] CuragaTiers = { Spells.Curaga, Spells.Curaga_II, Spells.Curaga_III, Spells.Curaga_IV, Spells.Curaga_V };

        public static string[] RegenSpells = { Spells.Regen, Spells.Regen_II, Spells.Regen_III, Spells.Regen_IV, Spells.Regen_V };
        public static string[] RefreshSpells = { Spells.Refresh, Spells.Refresh_II, Spells.Refresh_III };

        public static string[] GainBoostSpells = {
            Spells.Gain_STR,Spells.Gain_DEX, Spells.Gain_VIT, Spells.Gain_AGI, Spells.Gain_INT, Spells.Gain_MND, Spells.Gain_CHR,
            Spells.Boost_STR, Spells.Boost_DEX, Spells.Boost_VIT, Spells.Boost_AGI, Spells.Boost_INT, Spells.Boost_MND, Spells.Boost_CHR
        };

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
            { Spells.Boost_CHR, StatusEffect.CHR_Boost2 }
        };

        public static StatusEffect[] GainBoostEffects =
        {
            StatusEffect.STR_Boost2, StatusEffect.DEX_Boost2, StatusEffect.VIT_Boost2, StatusEffect.AGI_Boost2, StatusEffect.INT_Boost2, StatusEffect.MND_Boost2, StatusEffect.CHR_Boost2
        };
    }
}
