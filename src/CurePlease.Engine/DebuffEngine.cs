
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
        private readonly Dictionary<string, IEnumerable<short>> ActiveDebuffs = new();

        private IEnumerable<string> SpecifiedPartyMembers = new List<string>();

        private readonly ILogger<DebuffEngine> _Logger;
        public DebuffEngine(ILogger<DebuffEngine> logger)
        {
            _Logger = logger;
        }

        public EngineAction Run(EliteAPI pl, DebuffConfig config)
        {
            try
            {
                var wakeSleepSpell = Data.WakeSleepSpells[config.WakeSleepSpell];

                // PL Specific debuff removal
                if (pl.Player.Status != 33 && config.PLDebuffEnabled)
                {
                    var debuffIds = pl.Player.Buffs.Where(id => Data.DebuffPriorities.Keys.Cast<short>().Contains(id));
                    var debuffPriorityList = debuffIds.Cast<StatusEffect>().OrderBy(status => Array.IndexOf(Data.DebuffPriorities.Keys.ToArray(), status));

                    if (debuffPriorityList.Any())
                    {
                        // Get the highest priority debuff we have the right spell off cooldown for.
                        var targetDebuff = debuffPriorityList.FirstOrDefault(status => config.DebuffEnabled.ContainsKey(status) && config.DebuffEnabled[status] && pl.SpellAvailable(Data.DebuffPriorities[status]));

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

                // PARTY DEBUFF REMOVAL
                lock (ActiveDebuffs)
                {
                    // First remove the highest priority debuff.
                    var priorityMember = pl.GetHighestPriorityDebuff(ActiveDebuffs);

                    if (priorityMember == null)
                    {
                        return null;
                    }

                    var name = priorityMember.Name;

                    if (config.PartyDebuffEnabled && (!config.OnlySpecificMembers || SpecifiedPartyMembers.Contains(name)))
                    {
                        if (priorityMember != null && ActiveDebuffs.ContainsKey(name) && ActiveDebuffs[name].Any())
                        {
                            // Filter out non-debuffs, and convert to short IDs. Then calculate the priority order.
                            var debuffPriorityList = ActiveDebuffs[name].Cast<StatusEffect>().OrderBy(status => Array.IndexOf(Data.DebuffPriorities.Keys.ToArray(), status));


                            // Get the highest priority debuff we have the right spell off cooldown for.
                            var targetDebuff = debuffPriorityList.FirstOrDefault(status => config.DebuffEnabled[status] && pl.SpellAvailable(Data.DebuffPriorities[status]));

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
