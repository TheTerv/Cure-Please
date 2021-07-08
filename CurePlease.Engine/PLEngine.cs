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
    public class PLEngine : IPLEngine
    {
        private EliteAPI PL { get; set; }

        private bool LowMP = false;

        private int lastKnownEstablisherTarget = 0;

        private readonly ILogger<PLEngine> _Logger;
        private readonly IFollowEngine _FollowEngine;

        public PLEngine(ILogger<PLEngine> logger, IFollowEngine followEngine)
        {
            _Logger = logger;
            _FollowEngine = followEngine;
        }

        public EngineAction Run(EliteAPI pl, EliteAPI monitored, PLConfig config)
        {
            try
            {
                if (_FollowEngine.IsMoving())
                    return null;

                PL = pl;

                string whoToTell = monitored?.Player.Name;

                // FIRST IF YOU ARE SILENCED OR DOOMED ATTEMPT REMOVAL NOW
                if (PL.HasStatus(StatusEffect.Silence) && config.PLSilenceItemEnabled)
                {
                    var plSilenceItem = Items.SilenceRemoval[config.PLSilenceItem];

                    // Check to make sure we have echo drops
                    if (PL.GetInventoryItemCount(PL.GetItemId(plSilenceItem)) > 0 || PL.GetTempItemCount(PL.GetItemId(plSilenceItem)) > 0)
                    {
                        return new EngineAction
                        {
                            Item = plSilenceItem
                        };
                    }

                }
                else if (PL.HasStatus(StatusEffect.Doom) && config.PLDoomItemEnabled)
                {
                    var plDoomItem = Items.DoomRemoval[config.PLDoomItem];

                    // Check to make sure we have holy water
                    if (PL.GetInventoryItemCount(PL.GetItemId(plDoomItem)) > 0 || PL.GetTempItemCount(PL.GetItemId(plDoomItem)) > 0)
                    {
                        return new EngineAction
                        {
                            Item = plDoomItem
                        };
                    }
                }

                else if (config.DivineSeal && PL.Player.MPP <= 11 && PL.AbilityAvailable(Ability.DivineSeal) && !PL.Player.Buffs.Contains((short)StatusEffect.Weakness))
                {
                    return new EngineAction
                    {
                        Target = Target.Me,
                        JobAbility = Ability.DivineSeal
                    };
                }
                else if (config.Convert && (PL.Player.MP <= config.ConvertMP) && PL.AbilityAvailable(Ability.Convert) && !PL.Player.Buffs.Contains((short)StatusEffect.Weakness))
                {
                    return new EngineAction
                    {
                        Target = Target.Me,
                        JobAbility = Ability.Convert
                    };
                }

                if (!string.IsNullOrWhiteSpace(whoToTell))
                {
                    if (PL.Player.MP <= config.MinCastingMP && PL.Player.MP != 0)
                    {
                        if (config.LowMPEnabled && !LowMP && !config.HealLowMPEnabled)
                        {
                            PL.ThirdParty.SendString("/tell " + whoToTell + " MP is low!");

                        }

                        LowMP = true;
                    }
                    if (PL.Player.MP > config.MinCastingMP && PL.Player.MP != 0)
                    {
                        if (config.LowMPEnabled && LowMP && !config.HealLowMPEnabled)
                        {
                            PL.ThirdParty.SendString("/tell " + whoToTell + " MP OK!");

                        }

                        LowMP = false;
                    }

                    if (config.HealLowMPEnabled && PL.Player.MP <= config.HealMPThreshold && PL.Player.Status == 0)
                    {
                        if (config.LowMPEnabled && !LowMP)
                        {
                            PL.ThirdParty.SendString("/tell " + whoToTell + " MP is seriously low, /healing.");
                        }

                        LowMP = true;

                        return new EngineAction
                        {
                            CustomAction = "/heal"
                        };
                    }
                    else if (config.StandMPEnabled && PL.Player.MPP >= config.StandMPThreshold && PL.Player.Status == 33)
                    {
                        if (config.LowMPEnabled && !LowMP)
                        {
                            PL.ThirdParty.SendString("/tell " + whoToTell + " MP has recovered.");
                        }

                        LowMP = false;

                        return new EngineAction
                        {
                            CustomAction = "/heal"
                        };
                    }
                }

                if (config.AfflatusSolaceEnabled && (!PL.HasStatus(StatusEffect.Afflatus_Solace)) && PL.AbilityAvailable(Ability.AfflatusSolace))
                {
                    return new EngineAction { JobAbility = Ability.AfflatusSolace };
                }

                if (config.AfflatusMiseryEnabled && (!PL.HasStatus(StatusEffect.Afflatus_Misery)) && PL.AbilityAvailable(Ability.AfflatusMisery))
                {
                    return new EngineAction { JobAbility = Ability.AfflatusMisery };
                }

                if (config.Composure && (!PL.HasStatus(StatusEffect.Composure)) && PL.AbilityAvailable(Ability.Composure))
                {
                    return new EngineAction
                    {
                        JobAbility = Ability.Composure
                    };
                }

                if (config.LightArts && (!PL.HasStatus(StatusEffect.Light_Arts)) && (!PL.HasStatus(StatusEffect.Addendum_White)) && PL.AbilityAvailable(Ability.LightArts))
                {
                    return new EngineAction
                    {
                        JobAbility = Ability.LightArts
                    };
                }

                if (config.AddendumWhite && (!PL.HasStatus(StatusEffect.Addendum_White)) && PL.HasStatus(StatusEffect.Light_Arts) && PL.CurrentSCHCharges() > 0)
                {
                    return new EngineAction
                    {
                        JobAbility = Ability.AddendumWhite
                    };
                }

                if (config.DarkArts && (!PL.HasStatus(StatusEffect.Dark_Arts)) && (!PL.HasStatus(StatusEffect.Addendum_Black)) && PL.AbilityAvailable(Ability.DarkArts))
                {
                    return new EngineAction
                    {
                        JobAbility = Ability.DarkArts
                    };
                }

                if (config.AddendumBlack && PL.HasStatus(StatusEffect.Dark_Arts) && (!PL.HasStatus(StatusEffect.Addendum_Black)) && PL.CurrentSCHCharges() > 0)
                {
                    return new EngineAction
                    {
                        JobAbility = Ability.AddendumBlack
                    };
                }
                if (config.SublimationEnabled && (!PL.HasStatus(StatusEffect.Sublimation_Activated)) && (!PL.HasStatus(StatusEffect.Sublimation_Complete)) && (!PL.HasStatus(StatusEffect.Refresh)) && PL.AbilityAvailable(Ability.Sublimation))
                {
                    return new EngineAction { JobAbility = Ability.Sublimation };
                }

                if (config.SublimationEnabled && ((PL.Player.MPMax - PL.Player.MP) > config.SublimationMPLossThreshold) && PL.HasStatus(StatusEffect.Sublimation_Complete) && PL.AbilityAvailable(Ability.Sublimation))
                {
                    return new EngineAction { JobAbility = Ability.Sublimation };
                }

                if (config.DivineCaressEnabled && config.DebuffsEnabled && PL.AbilityAvailable(Ability.DivineCaress))
                {
                    return new EngineAction { JobAbility = Ability.DivineCaress };
                }

                if (config.DevotionEnabled && PL.AbilityAvailable(Ability.Devotion) && PL.Player.HPP > 80 && !config.DevotionWhenEngaged)
                {
                    // Get all active members who are in the PLs party.
                    IEnumerable<PartyMember> cParty = PL.GetActivePartyMembers().Where(member => member.Name != PL.Player.Name);

                    // If we're set to only devotion a specific target, filter the list for that target.
                    if (config.DevotionSpecifiedTarget)
                    {
                        cParty = cParty.Where(member => member.Name == config.DevotionTargetName);
                    }

                    // Get the first member who's within range, and has enough missing MP to meet our config criteria.
                    var devotionTarget = cParty.FirstOrDefault(member => member.CurrentMP <= config.DevotionMPThreshold && PL.EntityWithin(10, member.TargetIndex));

                    if (devotionTarget != default)
                    {
                        return new EngineAction
                        {
                            Target = devotionTarget.Name,
                            JobAbility = Ability.Devotion
                        };
                    }
                }

                if (config.ShellraEnabled && (!PL.HasStatus(StatusEffect.Shell)))
                {
                    var shellraSpell = Data.ShellraTiers[config.ShellraLevel - 1];
                    if (PL.SpellAvailable(shellraSpell))
                    {
                        return new EngineAction
                        {
                            Spell = shellraSpell
                        };
                    }
                }

                if (config.ProtectraEnabled && (!PL.HasStatus(StatusEffect.Protect)))
                {
                    var protectraSpell = Data.ProtectraTiers[config.ProtectraLevel - 1];
                    if (PL.SpellAvailable(protectraSpell))
                    {
                        return new EngineAction { Spell = protectraSpell };
                    }
                }

                if (config.BarElementEnabled && !PL.HasStatus(Data.SpellEffects[config.BarElementSpell]) && PL.SpellAvailable(config.BarElementSpell))
                {
                    // TODO: Make this work properly, so it can figure out if there's 2 charges and return 
                    // an action that says: Do Accession + Perpetuance + Barspell.
                    var result = new EngineAction
                    {
                        Spell = config.BarElementSpell
                    };

                    if (config.AccessionEnabled && config.BarElementAccession && PL.CurrentSCHCharges() > 0 && PL.AbilityAvailable(Ability.Accession) && !config.AOEBarElementEnabled && !PL.HasStatus(StatusEffect.Accession))
                    {
                        result.JobAbility = Ability.Accession;
                    }
                    else if (config.PerpetuanceEnabled && config.BarElemenetPerpetuance && PL.CurrentSCHCharges() > 0 && PL.AbilityAvailable(Ability.Perpetuance) && !PL.HasStatus(StatusEffect.Perpetuance))
                    {
                        result.JobAbility = Ability.Perpetuance;
                    }

                    return result;
                }

                if (config.BarStatusEnabled && !PL.HasStatus(Data.SpellEffects[config.BarStatusSpell]) && PL.SpellAvailable(config.BarStatusSpell))
                {
                    var result = new EngineAction
                    {
                        Spell = config.BarStatusSpell
                    };

                    if (config.AccessionEnabled && config.BarStatusAccession && PL.CurrentSCHCharges() > 0 && PL.AbilityAvailable(Ability.Accession) && !config.AOEBarStatusEnabled && !PL.HasStatus(StatusEffect.Accession))
                    {
                        result.JobAbility = Ability.Accession;
                    }
                    else if (config.PerpetuanceEnabled && config.BarStatusPerpetuance && PL.CurrentSCHCharges() > 0 && PL.AbilityAvailable(Ability.Perpetuance) && !PL.HasStatus(StatusEffect.Perpetuance))
                    {
                        result.JobAbility = Ability.Perpetuance;
                    }

                    return result;
                }

                if (config.GainBoostSpellEnabled && !PL.HasStatus(Data.SpellEffects[config.GainBoostSpell]) && PL.SpellAvailable(config.GainBoostSpell))
                {
                    return new EngineAction { Spell = config.GainBoostSpell };
                }

                if (config.StormSpellEnabled && !PL.HasStatus(Data.SpellEffects[config.StormSpell]) && PL.SpellAvailable(config.StormSpell))
                {
                    return ApplyStratagems(config.StormSpell, config, config.StormspellAccession, config.StormspellPerpetuance);
                }

                if (config.ProtectEnabled && (!PL.HasStatus(StatusEffect.Protect)))
                {
                    return ApplyStratagems(config.ProtectSpell, config, config.AccessionProtectShell, false);
                }

                if (config.ShellEnabled && (!PL.HasStatus(StatusEffect.Shell)))
                {
                    return ApplyStratagems(config.ShellSpell, config, config.AccessionProtectShell, false);
                }

                if (config.ReraiseEnabled && (!PL.HasStatus(StatusEffect.Reraise)))
                {
                    var result = new EngineAction { Spell = config.ReraiseSpell };

                    if (config.EnlightenmentReraise && !PL.HasStatus(StatusEffect.Addendum_White) && PL.AbilityAvailable(Ability.Enlightenment))
                    {
                        result.JobAbility = Ability.Enlightenment;
                    }

                    return result;
                }

                if (config.UtsusemiEnabled && PL.ShadowsRemaining() < 2)
                {
                    if (PL.GetInventoryItemCount(PL.GetItemId(Items.Shihei)) > 0)
                    {
                        if (PL.SpellAvailable(Spells.Utsusemi_Ni))
                        {
                            return new EngineAction { Spell = Spells.Utsusemi_Ni };
                        }
                        else if (PL.SpellAvailable(Spells.Utsusemi_Ichi) && (PL.ShadowsRemaining() == 0))
                        {
                            return new EngineAction { Spell = Spells.Utsusemi_Ichi };
                        }
                    }
                }

                if (config.BlinkEnabled && (!PL.HasStatus(StatusEffect.Blink)) && PL.SpellAvailable(Spells.Blink))
                {
                    return ApplyStratagems(Spells.Blink, config, config.BlinkAccession, config.BlinkPerpetuance);
                }

                if (config.PhalanxEnabled && !PL.HasStatus(StatusEffect.Phalanx) && PL.SpellAvailable(Spells.Phalanx))
                {
                    return ApplyStratagems(Spells.Phalanx, config, config.PhalanxAccession, config.PhalanxPerpetuance);
                }

                if (config.RefreshEnabled && (!PL.HasStatus(StatusEffect.Refresh)))
                {
                    return ApplyStratagems(config.RefreshSpell, config, config.RefreshAccession, config.RefreshPerpetuance);
                }

                if (config.RegenEnabled && (!PL.HasStatus(StatusEffect.Regen)))
                {
                    return ApplyStratagems(config.RegenSpell, config, config.RegenAccession, config.RegenPerpetuance);
                }

                if (config.AdloquiumEnabled && (!PL.HasStatus(StatusEffect.Regain)) && PL.SpellAvailable(Spells.Adloquium))
                {
                    return ApplyStratagems(Spells.Adloquium, config, config.AdloquiumAccession, config.AdloquiumPerpetuance);
                }

                if (config.StoneskinEnabled && (!PL.HasStatus(StatusEffect.Stoneskin)) && PL.SpellAvailable(Spells.Stoneskin))
                {
                    return ApplyStratagems(Spells.Stoneskin, config, config.StoneskinAccession, config.StoneskinPerpetuance);
                }

                if (config.AquaveilEnabled && (!PL.HasStatus(StatusEffect.Aquaveil)) && PL.SpellAvailable(Spells.Aquaveil))
                {
                    return ApplyStratagems(Spells.Aquaveil, config, config.AquaveilAccession, config.AquaveilPerpetuance);
                }

                if (config.KlimaformEnabled && !PL.HasStatus(StatusEffect.Klimaform))
                {
                    return new EngineAction { Spell = Spells.Klimaform };
                }

                if (config.TemperEnabled && (!PL.HasStatus(StatusEffect.Multi_Strikes)))
                {
                    return new EngineAction { Spell = config.TemperSpell };
                }

                if (config.HasteEnabled && (!PL.HasStatus(StatusEffect.Haste)))
                {
                    return new EngineAction { Spell = config.HasteSpell };
                }

                if (config.SpikesEnabled && !PL.HasStatus(Data.SpellEffects[config.SpikesSpell]))
                {
                    return new EngineAction { Spell = config.SpikesSpell };
                }

                if (config.EnSpellEnabled && !PL.HasStatus(Data.SpellEffects[config.EnSpell]) && PL.SpellAvailable(config.EnSpell))
                {
                    var result = new EngineAction { Spell = config.EnSpell };

                    // Don't want to try and accession/perpetuance tier II.
                    if (!config.EnSpell.Contains("II"))
                    {
                        return ApplyStratagems(config.EnSpell, config, config.EnspellAccession, config.EnspellPerpetuance);
                    }

                    return result;
                }

                if (config.AuspiceEnabled && (!PL.HasStatus(StatusEffect.Auspice)) && PL.SpellAvailable(Spells.Auspice))
                {
                    return new EngineAction { Spell = Spells.Auspice };
                }

                // TODO: Rethink this whole logic.
                // Probably doesn't work at all right now, and need to figure out a better way
                // to find entities than iterating 2048 things...           
                if (config.AutoTargetEnabled && PL.SpellAvailable(config.AutoTargetSpell))
                {
                    if (config.PartyBasedHateSpell)
                    {
                        // PARTY BASED HATE SPELL
                        int enemyID = CheckEngagedStatus_Hate(config);

                        if (enemyID != 0 && enemyID != lastKnownEstablisherTarget)
                        {
                            lastKnownEstablisherTarget = enemyID;

                            return new EngineAction
                            {
                                Target = config.AutoTargetTarget,
                                Spell = config.AutoTargetSpell
                            };
                        }
                    }
                    else
                    {
                        // ENEMY BASED TARGET
                        int enemyID = CheckEngagedStatus_Hate(config);

                        if (enemyID != 0 && enemyID != lastKnownEstablisherTarget)
                        {
                            PL.Target.SetTarget(enemyID);

                            lastKnownEstablisherTarget = enemyID;

                            return new EngineAction
                            {
                                Target = "<t>",
                                Spell = config.AutoTargetSpell
                            };

                            //if (ConfigForm.config.DisableTargettingCancel == false)
                            //{
                            //    await Task.Delay(TimeSpan.FromSeconds((double)ConfigForm.config.TargetRemoval_Delay));
                            //    PL.Target.SetTarget(0);
                            //}
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _Logger.LogError("Unknown exception occurred running PLEngine", ex);
            }
           
            return null;
        }

        private EngineAction ApplyStratagems(string spell, PLConfig config, bool accessionOnSpellEnabled, bool perpetuanceOnSpellEnabled)
        {
            var result = new EngineAction { Spell = spell };

            var strategemsNeeded = CountStrategemsNeeded(config.AccessionEnabled && accessionOnSpellEnabled && PL.GetActivePartyMembers().Count() > 1, config.PerpetuanceEnabled && perpetuanceOnSpellEnabled);

            // we're not using SCH buffs
            if (strategemsNeeded == 0)
                return result;

            // if we don't have enough strategems to buff what we want, do nothing.
            if (PL.CurrentSCHCharges() < strategemsNeeded)
            {
                result.Spell = null;
                return result;
            }

            if (config.AccessionEnabled && accessionOnSpellEnabled && PL.AbilityAvailable(Ability.Accession) && !PL.HasStatus(StatusEffect.Accession) && PL.GetActivePartyMembers().Count() > 1)
            {
                result.JobAbility = Ability.Accession;
            }

            if (config.PerpetuanceEnabled && perpetuanceOnSpellEnabled && PL.AbilityAvailable(Ability.Perpetuance) && !PL.HasStatus(StatusEffect.Perpetuance))
            {
                if (string.IsNullOrWhiteSpace(result.JobAbility))
                    result.JobAbility = Ability.Perpetuance;
                else
                    result.JobAbility2 = Ability.Perpetuance;
            }

            return result;
        }

        private static int CountStrategemsNeeded(params bool[] args)
        {
            return args.Count(t => t);
        }

        private int CheckEngagedStatus_Hate(PLConfig config)
        {
            if (config.AssistSpecifiedTarget == true && !string.IsNullOrEmpty(config.AutoTargetTarget))
            {
                for (int x = 0; x < 2048; x++)
                {
                    XiEntity z = PL.Entity.GetEntity(x);

                    if (!string.IsNullOrEmpty(z.Name) && z.Name.ToLower() == config.AutoTargetTarget.ToLower())
                    {
                        if (z.Status == 1)
                        {
                            return z.TargetingIndex;
                        }
                        else
                        {
                            return 0;
                        }
                    }
                }
                return 0;
            }

            return 0;
        }
    }
}
