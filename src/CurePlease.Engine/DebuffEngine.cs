
using CurePlease.Model;
using CurePlease.Model.Config;
using CurePlease.Model.Constants;
using CurePlease.Utilities;
using EliteMMO.API;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CurePlease.Engine
{
    public class DebuffEngine : IDebuffEngine
    {                
        private EliteAPI PL { get; set; }

        private readonly Dictionary<string, IEnumerable<short>> ActiveDebuffs = new();

        private IEnumerable<string> SpecifiedPartyMembers = new List<string>();

        private readonly ILogger<DebuffEngine> _Logger;
        public DebuffEngine(ILogger<DebuffEngine> logger)
        {
            _Logger = logger;
        }

        public EngineAction Run(EliteAPI pl, DebuffConfig Config)
        {
            try
            {
                PL = pl;

                var wakeSleepSpell = Data.WakeSleepSpells[Config.WakeSleepSpell];

                // PL Specific debuff removal
                if (PL.Player.Status != 33 && Config.PLDebuffEnabled)
                {
                    var debuffIds = PL.Player.Buffs.Where(id => Data.DebuffPriorities.Keys.Cast<short>().Contains(id));
                    var debuffPriorityList = debuffIds.Cast<StatusEffect>().OrderBy(status => Array.IndexOf(Data.DebuffPriorities.Keys.ToArray(), status));

                    if (debuffPriorityList.Any())
                    {
                        // Get the highest priority debuff we have the right spell off cooldown for.
                        var targetDebuff = debuffPriorityList.FirstOrDefault(status => Config.DebuffEnabled.ContainsKey(status) && Config.DebuffEnabled[status] && PL.SpellAvailable(Data.DebuffPriorities[status]));

                        if ((short)targetDebuff > 0)
                        {
                            return new EngineAction
                            {
                                Target = Target.Me,
                                Spell = Data.DebuffPriorities[targetDebuff]
                            };
                        }
                    }
                }

                //TODO - Maybe change this to an array of "remove debuffs from <names>"?

                // Monitored specific debuff removal
                //if (Config.MonitoredDebuffEnabled && Monitored != null && (PL.Entity.GetEntity((int)Monitored.Party.GetPartyMember(0).TargetIndex).Distance < 21) && (Monitored.Player.HP > 0) && PL.Player.Status != 33)
                //{
                //    var debuffIds = Monitored.Player.Buffs.Where(id => Data.DebuffPriorities.Keys.Cast<short>().Contains(id));
                //    var debuffPriorityList = debuffIds.Cast<StatusEffect>().OrderBy(status => Array.IndexOf(Data.DebuffPriorities.Keys.ToArray(), status));

                //    if (debuffPriorityList.Any())
                //    {
                //        // Get the highest priority debuff we have the right spell off cooldown for.
                //        var targetDebuff = debuffPriorityList.FirstOrDefault(status => Config.DebuffEnabled[status] && PL.SpellAvailable(Data.DebuffPriorities[status]));

                //        if ((short)targetDebuff > 0)
                //        {
                //            // Don't try and curaga outside our party.
                //            if ((targetDebuff == StatusEffect.Sleep || targetDebuff == StatusEffect.Sleep2) && !PL.SamePartyAs(Monitored))
                //            {
                //                return new EngineAction
                //                {
                //                    Target = Monitored.Player.Name,
                //                    Spell = Spells.Cure
                //                };
                //            }

                //            if (Data.DebuffPriorities[targetDebuff] != Spells.Erase || PL.SamePartyAs(Monitored))
                //            {
                //                return new EngineAction
                //                {
                //                    Target = Monitored.Player.Name,
                //                    Spell = Data.DebuffPriorities[targetDebuff]
                //                };
                //            }
                //        }
                //    }
                //}

                // PARTY DEBUFF REMOVAL
                lock (ActiveDebuffs)
                {
                    // First remove the highest priority debuff.
                    var priorityMember = PL.GetHighestPriorityDebuff(ActiveDebuffs);

                    if (priorityMember == null)
                    {
                        return null;
                    }

                    var name = priorityMember.Name;

                    if (Config.PartyDebuffEnabled && (!Config.OnlySpecificMembers || SpecifiedPartyMembers.Contains(name)))
                    {
                        if (priorityMember != null && ActiveDebuffs.ContainsKey(name) && ActiveDebuffs[name].Any())
                        {
                            // Filter out non-debuffs, and convert to short IDs. Then calculate the priority order.
                            var debuffPriorityList = ActiveDebuffs[name].Cast<StatusEffect>().OrderBy(status => Array.IndexOf(Data.DebuffPriorities.Keys.ToArray(), status));


                            // Get the highest priority debuff we have the right spell off cooldown for.
                            var targetDebuff = debuffPriorityList.FirstOrDefault(status => Config.DebuffEnabled[status] && PL.SpellAvailable(Data.DebuffPriorities[status]));

                            if ((short)targetDebuff > 0)
                            {
                                // Don't try and curaga outside our party.
                                if (!priorityMember.InParty(1) && (targetDebuff == StatusEffect.Sleep || targetDebuff == StatusEffect.Sleep2))
                                {
                                    return new EngineAction
                                    {
                                        Target = name,
                                        Spell = Spells.Cure
                                    };
                                }

                                return new EngineAction
                                {
                                    Target = name,
                                    Spell = Data.DebuffPriorities[targetDebuff]
                                };
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _Logger.LogError("Unknown exception occurred running DebuffEngine", ex);
            }

            return null;
        }

        public void UpdateDebuffs(string memberName, IEnumerable<short> debuffs)
        {
            lock (ActiveDebuffs)
            {
                if (debuffs.Any())
                {
                    ActiveDebuffs[memberName] = debuffs;
                }
                else if (ActiveDebuffs.ContainsKey(memberName))
                {
                    ActiveDebuffs.Remove(memberName);
                }
            }
        }

        public void ToggleSpecifiedMember(string memberName)
        {
            if(SpecifiedPartyMembers.Contains(memberName))
            {
                SpecifiedPartyMembers = SpecifiedPartyMembers.Where(name => name != memberName);
            }
            else
            {
                SpecifiedPartyMembers.Append(memberName);
            }
        }   
    }
}
