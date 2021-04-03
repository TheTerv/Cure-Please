using CurePlease.Model.Constants;
using EliteMMO.API;
using System.Collections.Generic;

namespace CurePlease.Model
{
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

        public static string[] StormTiers =
        {
            Spells.Firestorm, Spells.Hailstorm, Spells.Windstorm, Spells.Sandstorm, Spells.Thunderstorm, Spells.Rainstorm, Spells.Aurorastorm, Spells.Voidstorm,
            Spells.Firestorm_II, Spells.Hailstorm_II, Spells.Windstorm_II, Spells.Sandstorm_II, Spells.Thunderstorm_II, Spells.Rainstorm_II, Spells.Aurorastorm_II, Spells.Voidstorm_II
        };

        // This is tricky because the EliteAPI is closed source, so we're stuck working around it.
        // In this case they don't have the newer status effects yet, so we need to still treat them
        // as shorts, for the situations where there isn't an enum value for it.
        public static Dictionary<string, short> SpellEffects = new Dictionary<string, short>
        {
            { Spells.Gain_STR, (short)StatusEffect.STR_Boost2 },
            { Spells.Gain_DEX, (short)StatusEffect.DEX_Boost2 },
            { Spells.Gain_VIT, (short)StatusEffect.VIT_Boost2 },
            { Spells.Gain_AGI, (short)StatusEffect.AGI_Boost2 },
            { Spells.Gain_INT, (short)StatusEffect.INT_Boost2 },
            { Spells.Gain_MND, (short)StatusEffect.MND_Boost2 },
            { Spells.Gain_CHR, (short)StatusEffect.CHR_Boost2 },
            { Spells.Boost_STR, (short)StatusEffect.STR_Boost2 },
            { Spells.Boost_DEX, (short)StatusEffect.DEX_Boost2 },
            { Spells.Boost_VIT, (short)StatusEffect.VIT_Boost2 },
            { Spells.Boost_AGI, (short)StatusEffect.AGI_Boost2 },
            { Spells.Boost_INT, (short)StatusEffect.INT_Boost2 },
            { Spells.Boost_MND, (short)StatusEffect.MND_Boost2 },
            { Spells.Boost_CHR, (short)StatusEffect.CHR_Boost2 },
            { Spells.Firestorm, (short)StatusEffect.Firestorm },
            { Spells.Hailstorm, (short)StatusEffect.Hailstorm },
            { Spells.Windstorm, (short)StatusEffect.Windstorm },
            { Spells.Sandstorm, (short)StatusEffect.Sandstorm },
            { Spells.Thunderstorm, (short)StatusEffect.Thunderstorm },
            { Spells.Rainstorm, (short)StatusEffect.Rainstorm },
            { Spells.Aurorastorm, (short)StatusEffect.Aurorastorm },
            { Spells.Voidstorm, (short)StatusEffect.Voidstorm },
            { Spells.Firestorm_II, 589 },
            { Spells.Hailstorm_II, 590},
            { Spells.Windstorm_II, 591 },
            { Spells.Sandstorm_II, 592 },
            { Spells.Thunderstorm_II, 593 },
            { Spells.Rainstorm_II, 594 },
            { Spells.Aurorastorm_II, 595 },
            { Spells.Voidstorm_II, 596 },
        };

        public static StatusEffect[] GainBoostEffects =
        {
            StatusEffect.STR_Boost2, StatusEffect.DEX_Boost2, StatusEffect.VIT_Boost2, StatusEffect.AGI_Boost2, StatusEffect.INT_Boost2, StatusEffect.MND_Boost2, StatusEffect.CHR_Boost2
        };

        // We use this dictionary to both prioritize certain debuffs above others, and map which spells are
        // used to cure each debuff.
        public static Dictionary<StatusEffect, string> DebuffPriorities = new Dictionary<StatusEffect, string>
        {
            { StatusEffect.Doom, Spells.Cursna },
            { StatusEffect.Sleep, Spells.Curaga },
            { StatusEffect.Sleep2, Spells.Curaga },
            { StatusEffect.Petrification, Spells.Stona },
            { StatusEffect.Silence, Spells.Silena },
            { StatusEffect.Bind, Spells.Erase },
            { StatusEffect.Weight, Spells.Erase },
            { StatusEffect.Paralysis, Spells.Paralyna }, 
            { StatusEffect.Amnesia, Spells.Esuna },
            { StatusEffect.Slow, Spells.Erase },
            { StatusEffect.Blindness, Spells.Blindna },
            { StatusEffect.Poison, Spells.Poisona }, 
            { StatusEffect.Attack_Down, Spells.Erase }, 
            { StatusEffect.Curse, Spells.Cursna }, 
            { StatusEffect.Curse2, Spells.Cursna }, 
            { StatusEffect.Addle, Spells.Erase }, 
            { StatusEffect.Bane, Spells.Cursna }, 
            { StatusEffect.Plague, Spells.Viruna },
            { StatusEffect.Disease, Spells.Viruna }, 
            { StatusEffect.Burn, Spells.Erase },
            { StatusEffect.Frost, Spells.Erase },
            { StatusEffect.Choke, Spells.Erase },
            { StatusEffect.Rasp, Spells.Erase }, 
            { StatusEffect.Shock, Spells.Erase }, 
            { StatusEffect.Drown, Spells.Erase },
            { StatusEffect.Dia, Spells.Erase },
            { StatusEffect.Bio, Spells.Erase },
            { StatusEffect.STR_Down, Spells.Erase },
            { StatusEffect.DEX_Down, Spells.Erase },
            { StatusEffect.VIT_Down, Spells.Erase }, 
            { StatusEffect.AGI_Down, Spells.Erase }, 
            { StatusEffect.INT_Down, Spells.Erase }, 
            { StatusEffect.MND_Down, Spells.Erase }, 
            { StatusEffect.CHR_Down, Spells.Erase }, 
            { StatusEffect.Max_HP_Down, Spells.Erase }, 
            { StatusEffect.Max_MP_Down, Spells.Erase }, 
            { StatusEffect.Accuracy_Down, Spells.Erase },
            { StatusEffect.Evasion_Down, Spells.Erase },
            { StatusEffect.Defense_Down, Spells.Erase }, 
            { StatusEffect.Flash, Spells.Erase }, 
            { StatusEffect.Magic_Acc_Down, Spells.Erase }, 
            { StatusEffect.Magic_Atk_Down, Spells.Erase }, 
            { StatusEffect.Helix, Spells.Erase },
            { StatusEffect.Max_TP_Down, Spells.Erase },
            { StatusEffect.Requiem, Spells.Erase },
            { StatusEffect.Elegy, Spells.Erase }, 
            { StatusEffect.Threnody, Spells.Erase }
        };
    }
}
