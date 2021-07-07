using CurePlease.Model;
using CurePlease.Model.Config;
using CurePlease.Model.Constants;
using CurePlease.Utilities;
using EliteMMO.API;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using static EliteMMO.API.EliteAPI;

namespace CurePlease.Engine
{
    public class CureEngine : ICureEngine
    {
        private CureConfig _Config;

        private EliteAPI PL { get; set; }

        private readonly ILogger<CureEngine> _Logger;

        public CureEngine(ILogger<CureEngine> logger)
        {
            _Logger = logger;
        }

        public EngineAction Run(EliteAPI pl, CureConfig config, bool[] enabledMembers, bool[] highPriorityMembers)
        {
            try
            {
                PL = pl;
                _Config = config;

                List<PartyMember> partyByHP = PL.GetActivePartyMembers().OrderBy(member => member.CurrentHPP).ToList();

                /////////////////////////// PL CURE //////////////////////////////////////////////////////////////////////////////////////////////////////////////////

                if (PL.Player.HP > 0 && (PL.Player.HPP <= config.MonitoredCurePercentage))
                {
                    var plAsPartyMember = PL.Party.GetPartyMember(0);
                    return CureCalculator(plAsPartyMember);
                }

                /////////////////////////// CURAGA //////////////////////////////////////////////////////////////////////////////////////////////////////////////////                                    
                if (config.EnabledCuragaTiers.Any(x => x))
                {
                    //TODO parameter "PL" could probably be switched to Following
                    int plParty = PL.GetPartyRelativeTo(PL);

                    // Order parties that qualify for AOE cures by average missing HP.
                    var partyNeedsAoe = PL.PartyNeedsAoeCure(config.CuragaMinPlayers, config.CuragaHealthPercent).OrderBy(partyNumber => PL.AverageHpLossForParty(partyNumber));

                    // If PL is in same alliance, and there's at least 1 party that needs an AOE cure.
                    // Parties are ordered by most average missing HP.
                    if (plParty > 0 && partyNeedsAoe.Any())
                    {
                        int targetParty = 0;

                        // We can accession if we have light arts/addendum white, and either we already have the status or we have the ability available,
                        // and have the charges to use it.
                        bool plCanAccession = (PL.HasStatus(StatusEffect.Light_Arts) || PL.HasStatus(StatusEffect.Addendum_White))
                            && (PL.HasStatus(StatusEffect.Accession) || (PL.AbilityAvailable(Ability.Accession) && PL.CurrentSCHCharges() > 0));

                        foreach (int party in partyNeedsAoe)
                        {
                            // We check whether we can accession here, so that if we can't accession we don't skip a chance to curaga our own party.
                            if (party != plParty && !plCanAccession)
                            {
                                continue;
                            }

                            // We get the first party with at least 1 person who's in it and checked.
                            // As well as 1 person who's both under the cure threshold AND in casting range.
                            // This way we won't AOE parties we haven't got anyone checked in, and we won't attempt
                            // to AOE a party where we can't reach any of the injured members.
                            if (partyByHP.Any(pm => pm.InParty(party) && enabledMembers[pm.MemberNumber]))
                            {
                                if (partyByHP.Any(pm => pm.InParty(party) && pm.CurrentHPP < config.CuragaHealthPercent && PL.CanCastOn(pm)))
                                {
                                    targetParty = party;
                                }
                            }
                        }

                        if (targetParty > 0)
                        {
                            // The target is the first person we can cast on, since they're already ordered by HPP.
                            var target = partyByHP.FirstOrDefault(pm => pm.InParty(targetParty) && PL.CanCastOn(pm));

                            if (target != default)
                            {
                                // If same party as PL, curaga. Otherwise we try to accession cure.
                                if (targetParty == plParty)
                                {
                                    // TODO: Don't do this if we have no curagas enabled, prevents curing!
                                    return CuragaCalculator(target);
                                }
                                else
                                {
                                    var actionResult = CureCalculator(target);

                                    // We've already determined we can accession, or already have the status.
                                    if (actionResult != null && !PL.HasStatus(StatusEffect.Accession))
                                    {
                                        actionResult.JobAbility = Ability.Accession;
                                    }

                                    return actionResult;
                                }
                            }
                        }
                    }
                }

                /////////////////////////// CURE //////////////////////////////////////////////////////////////////////////////////////////////////////////////////

                // Calculate who needs a cure, and is a valid target.
                // Anyone who's: Enabled + Active + Alive + Under cure threshold
                var validCures = partyByHP.Where(pm => enabledMembers[pm.MemberNumber] && (pm.CurrentHPP <= config.CureHealthPercent) && PL.CanCastOn(pm));

                // Now run a scan to check all targets in the High Priority Threshold
                if (validCures != null && validCures.Any())
                {
                    var highPriorityCures = validCures.Where(pm => highPriorityMembers[pm.MemberNumber]);

                    if (highPriorityCures != null && highPriorityCures.Any())
                    {
                        return CureCalculator(highPriorityCures.First());
                    }
                    else
                    {
                        return CureCalculator(validCures.First());
                    }
                }
            }
            catch (Exception ex)
            {
                _Logger.LogError("Unexpected issue occurred while running cure engine", ex);
            }

            return null;
        }

        private string PickCure(uint hpLoss)
        {
            for (int i = 5; i >= 0; i--)
            {
                if (_Config.EnabledCureTiers[i] && hpLoss >= _Config.CureTierThresholds[i] && PL.HasMPFor(Data.CureTiers[i]))
                {
                    return PickCureTier(Data.CureTiers[i], Data.CureTiers); 
                }
            }         

            return Spells.Unknown;
        }

        private EngineAction CureCalculator(PartyMember partyMember)
        {
            var actionResult = new EngineAction
            {
                Target = partyMember.Name
            };

            // Only do this is party member is alive.
            if (partyMember.CurrentHP > 0)
            {
                var cureSpell = PickCure(partyMember.HPLoss());
                
                if(cureSpell != Spells.Unknown)
                {
                    actionResult.Spell = cureSpell;

                    return actionResult;
                }
            }

            return null;
        }

        private EngineAction CuragaCalculator(PartyMember member)
        {
            uint hpLoss = member.HPLoss();
            string cureSpell = Spells.Unknown;

            var actionResult = new EngineAction
            {
                Target = member.Name
            };

            for (int i = 4; i <= 0; i--)
            {
                if (_Config.EnabledCuragaTiers[i] && hpLoss >= _Config.CuragaTierThresholds[i] && PL.HasMPFor(Data.CuragaTiers[i]))
                {
                    cureSpell = Data.CuragaTiers[i];
                    break;
                }
            }

            if (cureSpell != Spells.Unknown)
            {
                // Check if we need to over/under cure.
                var curagaTier = PickCureTier(cureSpell, Data.CuragaTiers);
                
                actionResult.Spell = curagaTier;
                
                if (_Config.CuragaSpecifiedEnabled)
                {
                    actionResult.Target = _Config.CuragaSpecifiedName;
                }

                return actionResult;
            }

            return null;
        }

        private string PickCureTier(string cureSpell, string[] tierList)
        {
            int spellIndex = Array.IndexOf(tierList, cureSpell);

            string overSpell;
            string underSpell;

            // This will end up with a situation where Cure + Cure II on cooldown results in the "Undercure"
            // solution being Cure III. But I think it might not be possible to cast both fast enough
            // to make that a concern?
            if (cureSpell == tierList.Last())
            {
                overSpell = tierList[^2];
                underSpell = tierList[^3];
            }
            else if (cureSpell == tierList.First())
            {
                overSpell = tierList[1];
                underSpell = tierList[2];
            }
            else
            {
                overSpell = tierList[spellIndex + 1];
                underSpell = tierList[spellIndex - 1];
            }

            if (PL.SpellAvailable(cureSpell) && PL.HasMPFor(cureSpell))
            {
                return cureSpell;
            }
            else if (_Config.OverCureEnabled && PL.SpellAvailable(overSpell) && PL.HasMPFor(overSpell))
            {
                return overSpell;
            }
            else if (_Config.UnderCureEnabled && PL.SpellAvailable(underSpell) && PL.HasMPFor(underSpell))
            {
                return underSpell;
            }

            return Spells.Unknown;
        }
    }
}
