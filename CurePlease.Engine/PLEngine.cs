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

        public PLEngine(ILogger<PLEngine> logger)
        {
            _Logger = logger;
        }

        public EngineAction Run(EliteAPI pl, EliteAPI monitored, PLConfig Config)
        {
            try
            {
                PL = pl;

                string whoToTell = monitored?.Player.Name;

                // FIRST IF YOU ARE SILENCED OR DOOMED ATTEMPT REMOVAL NOW
                if (PL.HasStatus(StatusEffect.Silence) && Config.PLSilenceItemEnabled)
                {
                    var plSilenceItem = Items.SilenceRemoval[Config.PLSilenceItem];

                    // Check to make sure we have echo drops
                    if (PL.GetInventoryItemCount(PL.GetItemId(plSilenceItem)) > 0 || PL.GetTempItemCount(PL.GetItemId(plSilenceItem)) > 0)
                    {
                        return new EngineAction
                        {
                            Item = plSilenceItem
                        };
                    }

                }
                else if (PL.HasStatus(StatusEffect.Doom) && Config.PLDoomItemEnabled)
                {
                    var plDoomItem = Items.DoomRemoval[Config.PLDoomItem];

                    // Check to make sure we have holy water
                    if (PL.GetInventoryItemCount(PL.GetItemId(plDoomItem)) > 0 || PL.GetTempItemCount(PL.GetItemId(plDoomItem)) > 0)
                    {
                        return new EngineAction
                        {
                            Item = plDoomItem
                        };
                    }
                }

                else if (Config.DivineSeal && PL.Player.MPP <= 11 && PL.AbilityAvailable(Ability.DivineSeal) && !PL.Player.Buffs.Contains((short)StatusEffect.Weakness))
                {
                    return new EngineAction
                    {
                        Target = Target.Me,
                        JobAbility = Ability.DivineSeal
                    };
                }
                else if (Config.Convert && (PL.Player.MP <= Config.ConvertMP) && PL.AbilityAvailable(Ability.Convert) && !PL.Player.Buffs.Contains((short)StatusEffect.Weakness))
                {
                    return new EngineAction
                    {
                        Target = Target.Me,
                        JobAbility = Ability.Convert
                    };
                }

                if (!string.IsNullOrWhiteSpace(whoToTell))
                {
                    if (PL.Player.MP <= Config.MinCastingMP && PL.Player.MP != 0)
                    {
                        if (Config.LowMPEnabled && !LowMP && !Config.HealLowMPEnabled)
                        {
                            PL.ThirdParty.SendString("/tell " + whoToTell + " MP is low!");

                        }

                        LowMP = true;
                    }
                    if (PL.Player.MP > Config.MinCastingMP && PL.Player.MP != 0)
                    {
                        if (Config.LowMPEnabled && LowMP && !Config.HealLowMPEnabled)
                        {
                            PL.ThirdParty.SendString("/tell " + whoToTell + " MP OK!");

                        }

                        LowMP = false;
                    }

                    if (Config.HealLowMPEnabled && PL.Player.MP <= Config.HealMPThreshold && PL.Player.Status == 0)
                    {
                        if (Config.LowMPEnabled && !LowMP)
                        {
                            PL.ThirdParty.SendString("/tell " + whoToTell + " MP is seriously low, /healing.");
                        }

                        LowMP = true;

                        return new EngineAction
                        {
                            CustomAction = "/heal"
                        };
                    }
                    else if (Config.StandMPEnabled && PL.Player.MPP >= Config.StandMPThreshold && PL.Player.Status == 33)
                    {
                        if (Config.LowMPEnabled && !LowMP)
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

                if (Config.AfflatusSolaceEnabled && (!PL.HasStatus(StatusEffect.Afflatus_Solace)) && PL.AbilityAvailable(Ability.AfflatusSolace))
                {
                    return new EngineAction { JobAbility = Ability.AfflatusSolace };
                }

                if (Config.AfflatusMiseryEnabled && (!PL.HasStatus(StatusEffect.Afflatus_Misery)) && PL.AbilityAvailable(Ability.AfflatusMisery))
                {
                    return new EngineAction { JobAbility = Ability.AfflatusMisery };
                }

                if (Config.Composure && (!PL.HasStatus(StatusEffect.Composure)) && PL.AbilityAvailable(Ability.Composure))
                {
                    return new EngineAction
                    {
                        JobAbility = Ability.Composure
                    };
                }

                if (Config.LightArts && (!PL.HasStatus(StatusEffect.Light_Arts)) && (!PL.HasStatus(StatusEffect.Addendum_White)) && PL.AbilityAvailable(Ability.LightArts))
                {
                    return new EngineAction
                    {
                        JobAbility = Ability.LightArts
                    };
                }

                if (Config.AddendumWhite && (!PL.HasStatus(StatusEffect.Addendum_White)) && PL.HasStatus(StatusEffect.Light_Arts) && PL.CurrentSCHCharges() > 0)
                {
                    return new EngineAction
                    {
                        JobAbility = Ability.AddendumWhite
                    };
                }

                if (Config.DarkArts && (!PL.HasStatus(StatusEffect.Dark_Arts)) && (!PL.HasStatus(StatusEffect.Addendum_Black)) && PL.AbilityAvailable(Ability.DarkArts))
                {
                    return new EngineAction
                    {
                        JobAbility = Ability.DarkArts
                    };
                }

                if (Config.AddendumBlack && PL.HasStatus(StatusEffect.Dark_Arts) && (!PL.HasStatus(StatusEffect.Addendum_Black)) && PL.CurrentSCHCharges() > 0)
                {
                    return new EngineAction
                    {
                        JobAbility = Ability.AddendumBlack
                    };
                }
                if (Config.SublimationEnabled && (!PL.HasStatus(StatusEffect.Sublimation_Activated)) && (!PL.HasStatus(StatusEffect.Sublimation_Complete)) && (!PL.HasStatus(StatusEffect.Refresh)) && PL.AbilityAvailable(Ability.Sublimation))
                {
                    return new EngineAction { JobAbility = Ability.Sublimation };
                }

                if (Config.SublimationEnabled && ((PL.Player.MPMax - PL.Player.MP) > Config.SublimationMPLossThreshold) && PL.HasStatus(StatusEffect.Sublimation_Complete) && PL.AbilityAvailable(Ability.Sublimation))
                {
                    return new EngineAction { JobAbility = Ability.Sublimation };
                }

                if (Config.DivineCaressEnabled && Config.DebuffsEnabled && PL.AbilityAvailable(Ability.DivineCaress))
                {
                    return new EngineAction { JobAbility = Ability.DivineCaress };
                }

                if (Config.DevotionEnabled && PL.AbilityAvailable(Ability.Devotion) && PL.Player.HPP > 80 && !Config.DevotionWhenEngaged)
                {
                    // Get all active members who are in the PLs party.
                    IEnumerable<PartyMember> cParty = PL.GetActivePartyMembers().Where(member => member.Name != PL.Player.Name);

                    // If we're set to only devotion a specific target, filter the list for that target.
                    if (Config.DevotionSpecifiedTarget)
                    {
                        cParty = cParty.Where(member => member.Name == Config.DevotionTargetName);
                    }

                    // Get the first member who's within range, and has enough missing MP to meet our config criteria.
                    var devotionTarget = cParty.FirstOrDefault(member => member.CurrentMP <= Config.DevotionMPThreshold && PL.EntityWithin(10, member.TargetIndex));

                    if (devotionTarget != default)
                    {
                        return new EngineAction
                        {
                            Target = devotionTarget.Name,
                            JobAbility = Ability.Devotion
                        };
                    }
                }

                if (Config.ShellraEnabled && (!PL.HasStatus(StatusEffect.Shell)))
                {
                    var shellraSpell = Data.ShellraTiers[Config.ShellraLevel - 1];
                    if (PL.SpellAvailable(shellraSpell))
                    {
                        return new EngineAction
                        {
                            Spell = shellraSpell
                        };
                    }
                }

                if (Config.ProtectraEnabled && (!PL.HasStatus(StatusEffect.Protect)))
                {
                    var protectraSpell = Data.ProtectraTiers[Config.ProtectraLevel - 1];
                    if (PL.SpellAvailable(protectraSpell))
                    {
                        return new EngineAction { Spell = protectraSpell };
                    }
                }

                if (Config.BarElementEnabled && !PL.HasStatus(Data.SpellEffects[Config.BarElementSpell]) && PL.SpellAvailable(Config.BarElementSpell))
                {
                    // TODO: Make this work properly, so it can figure out if there's 2 charges and return 
                    // an action that says: Do Accession + Perpetuance + Barspell.
                    var result = new EngineAction
                    {
                        Spell = Config.BarElementSpell
                    };

                    if (Config.AccessionEnabled && Config.BarElementAccession && PL.CurrentSCHCharges() > 0 && PL.AbilityAvailable(Ability.Accession) && !Config.AOEBarElementEnabled && !PL.HasStatus(StatusEffect.Accession))
                    {
                        result.JobAbility = Ability.Accession;
                    }
                    else if (Config.PerpetuanceEnabled && Config.BarElemenetPerpetuance && PL.CurrentSCHCharges() > 0 && PL.AbilityAvailable(Ability.Perpetuance) && !PL.HasStatus(StatusEffect.Perpetuance))
                    {
                        result.JobAbility = Ability.Perpetuance;
                    }

                    return result;
                }

                if (Config.BarStatusEnabled && !PL.HasStatus(Data.SpellEffects[Config.BarStatusSpell]) && PL.SpellAvailable(Config.BarStatusSpell))
                {
                    var result = new EngineAction
                    {
                        Spell = Config.BarStatusSpell
                    };

                    if (Config.AccessionEnabled && Config.BarStatusAccession && PL.CurrentSCHCharges() > 0 && PL.AbilityAvailable(Ability.Accession) && !Config.AOEBarStatusEnabled && !PL.HasStatus(StatusEffect.Accession))
                    {
                        result.JobAbility = Ability.Accession;
                    }
                    else if (Config.PerpetuanceEnabled && Config.BarStatusPerpetuance && PL.CurrentSCHCharges() > 0 && PL.AbilityAvailable(Ability.Perpetuance) && !PL.HasStatus(StatusEffect.Perpetuance))
                    {
                        result.JobAbility = Ability.Perpetuance;
                    }

                    return result;
                }

                if (Config.GainBoostSpellEnabled && !PL.HasStatus(Data.SpellEffects[Config.GainBoostSpell]) && PL.SpellAvailable(Config.GainBoostSpell))
                {
                    return new EngineAction { Spell = Config.GainBoostSpell };
                }

                if (Config.StormSpellEnabled && !PL.HasStatus(Data.SpellEffects[Config.StormSpell]) && PL.SpellAvailable(Config.StormSpell))
                {
                    var result = new EngineAction { Spell = Config.StormSpell };
                    if (Config.AccessionEnabled && Config.StormspellAccession && PL.CurrentSCHCharges() > 0 && PL.AbilityAvailable(Ability.Accession) && !PL.HasStatus(StatusEffect.Accession))
                    {
                        result.JobAbility = Ability.Accession;
                    }
                    else if (Config.PerpetuanceEnabled && Config.StormspellPerpetuance && PL.CurrentSCHCharges() > 0 && PL.AbilityAvailable(Ability.Perpetuance) && !PL.HasStatus(StatusEffect.Perpetuance))
                    {
                        result.JobAbility = Ability.Perpetuance;
                    }

                    return result;
                }

                if (Config.ProtectEnabled && (!PL.HasStatus(StatusEffect.Protect)))
                {
                    var result = new EngineAction { Spell = Config.ProtectSpell };

                    if (Config.AccessionEnabled && Config.AccessionProtectShell && PL.Party.GetPartyMembers().Count > 2 && PL.AbilityAvailable(Ability.Accession) && PL.CurrentSCHCharges() > 0)
                    {
                        if (!PL.HasStatus(StatusEffect.Accession))
                        {
                            result.JobAbility = Ability.Accession;
                        }
                    }

                    return result;
                }

                if (Config.ShellEnabled && (!PL.HasStatus(StatusEffect.Shell)))
                {
                    var result = new EngineAction { Spell = Config.ShellSpell };

                    if (Config.AccessionEnabled && Config.AccessionProtectShell && PL.Party.GetPartyMembers().Count > 2 && PL.CurrentSCHCharges() > 0 && PL.AbilityAvailable(Ability.Accession))
                    {
                        if (!PL.HasStatus(StatusEffect.Accession))
                        {
                            result.JobAbility = Ability.Accession;
                        }
                    }

                    return result;
                }

                if (Config.ReraiseEnabled && (!PL.HasStatus(StatusEffect.Reraise)))
                {
                    var result = new EngineAction { Spell = Config.ReraiseSpell };

                    if (Config.EnlightenmentReraise && !PL.HasStatus(StatusEffect.Addendum_White) && PL.AbilityAvailable(Ability.Enlightenment))
                    {
                        result.JobAbility = Ability.Enlightenment;
                    }

                    return result;
                }

                if (Config.UtsusemiEnabled && PL.ShadowsRemaining() < 2)
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

                if (Config.BlinkEnabled && (!PL.HasStatus(StatusEffect.Blink)) && PL.SpellAvailable(Spells.Blink))
                {
                    var result = new EngineAction { Spell = Spells.Blink };

                    if (Config.AccessionEnabled && Config.BlinkAccession && PL.CurrentSCHCharges() > 0 && PL.AbilityAvailable(Ability.Accession) && !PL.HasStatus(StatusEffect.Accession))
                    {
                        result.JobAbility = Ability.Accession;
                    }
                    else if (Config.PerpetuanceEnabled && Config.BlinkPerpetuance && PL.CurrentSCHCharges() > 0 && PL.AbilityAvailable(Ability.Perpetuance) && !PL.HasStatus(StatusEffect.Perpetuance))
                    {
                        result.JobAbility = Ability.Perpetuance;
                    }

                    return result;
                }

                if (Config.PhalanxEnabled && (!PL.HasStatus(StatusEffect.Phalanx)) && PL.SpellAvailable(Spells.Phalanx))
                {
                    var result = new EngineAction { Spell = Spells.Phalanx };

                    if (Config.AccessionEnabled && Config.PhalanxAccession && PL.CurrentSCHCharges() > 0 && PL.AbilityAvailable(Ability.Accession) && !PL.HasStatus(StatusEffect.Accession))
                    {
                        result.JobAbility = Ability.Accession;
                    }
                    else if (Config.PerpetuanceEnabled && Config.PhalanxPerpetuance && PL.CurrentSCHCharges() > 0 && PL.AbilityAvailable(Ability.Perpetuance) && !PL.HasStatus(StatusEffect.Perpetuance))
                    {
                        result.JobAbility = Ability.Perpetuance;
                    }

                    return result;
                }

                if (Config.RefreshEnabled && (!PL.HasStatus(StatusEffect.Refresh)))
                {
                    var result = new EngineAction { Spell = Config.RefreshSpell };

                    if (Config.AccessionEnabled && Config.RefreshAccession && PL.CurrentSCHCharges() > 0 && PL.AbilityAvailable(Ability.Accession) && !PL.HasStatus(StatusEffect.Accession))
                    {
                        result.JobAbility = Ability.Accession;
                    }
                    else if (Config.PerpetuanceEnabled && Config.RefreshPerpetuance && PL.CurrentSCHCharges() > 0 && PL.AbilityAvailable(Ability.Perpetuance) && !PL.HasStatus(StatusEffect.Perpetuance))
                    {
                        result.JobAbility = Ability.Perpetuance;
                    }

                    return result;
                }

                if (Config.RegenEnabled && (!PL.HasStatus(StatusEffect.Regen)))
                {
                    var result = new EngineAction { Spell = Config.RegenSpell };

                    if (Config.AccessionEnabled && Config.RegenAccession && PL.CurrentSCHCharges() > 0 && PL.AbilityAvailable(Ability.Accession) && !PL.HasStatus(StatusEffect.Accession))
                    {
                        result.JobAbility = Ability.Accession;
                    }
                    else if (Config.PerpetuanceEnabled && Config.RegenPerpetuance && PL.CurrentSCHCharges() > 0 && PL.AbilityAvailable(Ability.Perpetuance) && !PL.HasStatus(StatusEffect.Perpetuance))
                    {
                        result.JobAbility = Ability.Perpetuance;
                    }

                    return result;
                }

                if (Config.AdloquiumEnabled && (!PL.HasStatus(StatusEffect.Regain)) && PL.SpellAvailable(Spells.Adloquium))
                {
                    var result = new EngineAction { Spell = Spells.Adloquium };

                    if (Config.AccessionEnabled && Config.AdloquiumAccession && PL.CurrentSCHCharges() > 0 && PL.AbilityAvailable(Ability.Accession) && !PL.HasStatus(StatusEffect.Accession))
                    {
                        result.JobAbility = Ability.Accession;
                    }
                    else if (Config.PerpetuanceEnabled && Config.AdloquiumPerpetuance && PL.CurrentSCHCharges() > 0 && PL.AbilityAvailable(Ability.Perpetuance) && !PL.HasStatus(StatusEffect.Perpetuance))
                    {
                        result.JobAbility = Ability.Perpetuance;
                    }

                    return result;
                }

                if (Config.StoneskinEnabled && (!PL.HasStatus(StatusEffect.Stoneskin)) && PL.SpellAvailable(Spells.Stoneskin))
                {
                    var result = new EngineAction { Spell = Spells.Stoneskin };

                    if (Config.AccessionEnabled && Config.StoneskinAccession && PL.CurrentSCHCharges() > 0 && PL.AbilityAvailable(Ability.Accession) && !PL.HasStatus(StatusEffect.Accession))
                    {
                        result.JobAbility = Ability.Accession;
                    }
                    else if (Config.PerpetuanceEnabled && Config.StoneskinPerpetuance && PL.CurrentSCHCharges() > 0 && PL.AbilityAvailable(Ability.Perpetuance) && !PL.HasStatus(StatusEffect.Perpetuance))
                    {
                        result.JobAbility = Ability.Perpetuance;
                    }

                    return result;
                }

                if (Config.AquaveilEnabled && (!PL.HasStatus(StatusEffect.Aquaveil)) && PL.SpellAvailable(Spells.Aquaveil))
                {
                    var result = new EngineAction { Spell = Spells.Aquaveil };

                    if (Config.AccessionEnabled && Config.AquaveilAccession && PL.CurrentSCHCharges() > 0 && PL.AbilityAvailable(Ability.Accession) && !PL.HasStatus(StatusEffect.Accession))
                    {
                        result.JobAbility = Ability.Accession;
                    }
                    else if (Config.PerpetuanceEnabled && Config.AquaveilPerpetuance && PL.CurrentSCHCharges() > 0 && PL.AbilityAvailable(Ability.Perpetuance) && !PL.HasStatus(StatusEffect.Perpetuance))
                    {
                        result.JobAbility = Ability.Perpetuance;
                    }

                    return result;
                }

                if (Config.KlimaformEnabled && !PL.HasStatus(StatusEffect.Klimaform))
                {
                    return new EngineAction { Spell = Spells.Klimaform };
                }

                if (Config.TemperEnabled && (!PL.HasStatus(StatusEffect.Multi_Strikes)))
                {
                    return new EngineAction { Spell = Config.TemperSpell };
                }

                if (Config.HasteEnabled && (!PL.HasStatus(StatusEffect.Haste)))
                {
                    return new EngineAction { Spell = Config.HasteSpell };
                }

                if (Config.SpikesEnabled && !PL.HasStatus(Data.SpellEffects[Config.SpikesSpell]))
                {
                    return new EngineAction { Spell = Config.SpikesSpell };
                }

                if (Config.EnSpellEnabled && !PL.HasStatus(Data.SpellEffects[Config.EnSpell]) && PL.SpellAvailable(Config.EnSpell))
                {
                    var result = new EngineAction { Spell = Config.EnSpell };

                    // Don't want to try and accession/perpetuance tier II.
                    if (!Config.EnSpell.Contains("II"))
                    {
                        if (Config.AccessionEnabled && Config.EnspellAccession && PL.CurrentSCHCharges() > 0 && PL.AbilityAvailable(Ability.Accession) && !PL.HasStatus(StatusEffect.Accession))
                        {
                            result.JobAbility = Ability.Accession;
                        }
                        else if (Config.PerpetuanceEnabled && Config.EnspellPerpetuance && PL.CurrentSCHCharges() > 0 && PL.AbilityAvailable(Ability.Perpetuance) && !PL.HasStatus(StatusEffect.Perpetuance))
                        {
                            result.JobAbility = Ability.Perpetuance;
                        }
                    }

                    return result;
                }

                if (Config.AuspiceEnabled && (!PL.HasStatus(StatusEffect.Auspice)) && PL.SpellAvailable(Spells.Auspice))
                {
                    return new EngineAction { Spell = Spells.Auspice };
                }

                // TODO: Rethink this whole logic.
                // Probably doesn't work at all right now, and need to figure out a better way
                // to find entities than iterating 2048 things...           
                if (Config.AutoTargetEnabled && PL.SpellAvailable(Config.AutoTargetSpell))
                {
                    if (Config.PartyBasedHateSpell)
                    {
                        // PARTY BASED HATE SPELL
                        int enemyID = CheckEngagedStatus_Hate(Config);

                        if (enemyID != 0 && enemyID != lastKnownEstablisherTarget)
                        {
                            lastKnownEstablisherTarget = enemyID;

                            return new EngineAction
                            {
                                Target = Config.AutoTargetTarget,
                                Spell = Config.AutoTargetSpell
                            };
                        }
                    }
                    else
                    {
                        // ENEMY BASED TARGET
                        int enemyID = CheckEngagedStatus_Hate(Config);

                        if (enemyID != 0 && enemyID != lastKnownEstablisherTarget)
                        {
                            PL.Target.SetTarget(enemyID);

                            lastKnownEstablisherTarget = enemyID;

                            return new EngineAction
                            {
                                Target = "<t>",
                                Spell = Config.AutoTargetSpell
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
