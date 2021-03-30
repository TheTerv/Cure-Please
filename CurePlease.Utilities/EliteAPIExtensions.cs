using CurePlease.Model;
using CurePlease.Model.Enums;
using EliteMMO.API;
using System;
using System.Linq;

namespace CurePlease.Utilities
{
    public static class EliteAPIExtensions
    {
        public static bool HasMPFor(this EliteAPI api, string spell)
        {
            return api.Player.MP >= Data.SpellCosts[spell];
        }

        public static bool HasStatus(this EliteAPI api, StatusEffect effect)
        {
            return api.Player.Buffs.Any(buff => (StatusEffect)buff == effect);
        }

        public static bool SpellAvailable(this EliteAPI api, string spell)
        {
            // IF YOU HAVE OMERTA THEN BLOCK MAGIC CASTING
            if (api.HasStatus(StatusEffect.No_Magic_Casting))
            {
                return false;
            }

            var apiSpell = api.Resources.GetSpell(spell, 0);

            // Return true if we possess the spell AND spell is off cooldown.
            return api.Player.HasSpell(apiSpell.ID) && (api.Recast.GetSpellRecast(apiSpell.Index) == 0);
        }

        public static bool AbilityAvailable(this EliteAPI api, string ability)
        {
            // IF YOU HAVE INPAIRMENT/AMNESIA THEN BLOCK JOB ABILITY CASTING
            if (api.HasStatus(StatusEffect.No_Job_Abilities) || api.HasStatus(StatusEffect.Amnesia))
            {
                return false;
            }
            var apiAbility = api.Resources.GetAbility(ability, 0);
            return api.Player.HasAbility(apiAbility.ID) && (api.Recast.GetAbilityRecast(apiAbility.TimerID) == 0);
        }

        public static int CurrentSCHCharges(this EliteAPI api)
        {
            if (api != null)
            {
                int MainJob = api.Player.MainJob;
                int SubJob = api.Player.SubJob;

                if (MainJob == (int)Job.SCH || SubJob == (int)Job.SCH)
                {
                    if (api.HasStatus(StatusEffect.Light_Arts) || api.HasStatus(StatusEffect.Addendum_White))
                    {
                        // Stragem charge recast = ability ID 231?
                        int currentRecastTimer = api.Recast.GetAbilityRecast(231);

                        int SpentPoints = api.Player.GetJobPoints((int)Job.SCH).SpentJobPoints;

                        int MainLevel = api.Player.MainJobLevel;
                        int SubLevel = api.Player.SubJobLevel;

                        int baseTimer = 240;
                        int baseCharges = 1;

                        // Generate the correct timer between charges depending on level / Job Points
                        if (MainJob == (int)Job.SCH)
                        {
                            if (SpentPoints >= 550)
                            {
                                baseTimer = 33;
                                baseCharges = 5;
                            }
                            else if (MainLevel >= 90)
                            {
                                baseTimer = 48;
                                baseCharges = 5;
                            }
                            else if (MainLevel >= 70 && MainLevel < 90)
                            {
                                baseTimer = 60;
                                baseCharges = 4;
                            }
                            else if (MainLevel >= 50 && MainLevel < 70)
                            {
                                baseTimer = 80;
                                baseCharges = 3;
                            }
                            else if (MainLevel >= 30 && MainLevel < 50)
                            {
                                baseTimer = 120;
                                baseCharges = 2;
                            }
                            else if (MainLevel >= 10 && MainLevel < 30)
                            {
                                baseTimer = 240;
                                baseCharges = 1;
                            }
                        }
                        else if (SubJob == (int)Job.SCH)
                        {
                            if (SubLevel >= 30 && SubLevel < 50)
                            {
                                baseTimer = 120;
                                baseCharges = 2;
                            }
                        }

                        // Now knowing what the time between charges is lets calculate how many
                        // charges are available

                        if (currentRecastTimer == 0)
                        {
                            return baseCharges;
                        }
                        else
                        {
                            int t = currentRecastTimer / 60;

                            int stratsUsed = t / baseTimer;

                            int currentCharges = (int)Math.Ceiling((decimal)baseCharges - stratsUsed);

                            return (baseTimer == 120) ? currentCharges-- : currentCharges;
                        }
                    }
                }
            }

            return -1;
        }
    }
}
