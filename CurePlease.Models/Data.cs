using CurePlease.Models.Constants;
using EliteMMO.API;
using System.Collections.Generic;

namespace CurePlease.Models
{
    public static class Data
    {
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
