
using CurePlease.Model;
using CurePlease.Model.Config;
using CurePlease.Model.Constants;
using CurePlease.Utilities;
using EliteMMO.API;
using System.Collections.Generic;
using System.Linq;
using static EliteMMO.API.EliteAPI;

namespace CurePlease.Engine
{
    // TODO:
    // - Figure out tiers.
    // - Prevent overlap with haste/flurry/storms.
    // - Don't consider buffs we can't cast? Or just prevent that before we get here?
    public class BuffEngine
    {
        // Auto Spells:
        // Haste, Haste II, Phalanx II, Regen, Shell, Protect, Sandstorm, Rainstorm, Windstorm, Firestorm, Hailstorm, Thunderstorm, Voidstorm, Aurorastorm, Refresh, Adloquium
             
        private BuffConfig Config { get; set; }
        private EliteAPI PL { get; set; }
        private EliteAPI Monitored { get; set; }

        private Dictionary<string, IEnumerable<short>> ActiveBuffs = new Dictionary<string, IEnumerable<short>>();

        // TODO: Should this just be the exact spell we want to cast instead of the buff id?
        // Would probably be cleaner.
        private Dictionary<string, IEnumerable<string>> AutoBuffs = new Dictionary<string, IEnumerable<string>>();

        public BuffEngine(EliteAPI pl, EliteAPI mon, BuffConfig config)
        {
            Config = config;

            PL = pl;
            Monitored = mon;                  
        }

        public EngineAction Run()
        {
            lock (ActiveBuffs)
            {
                // Want to find party members where they have an autobuff configured but it isn't in their list of buffs.
                foreach (PartyMember ptMember in Monitored.GetActivePartyMembers())
                {
                    // Make sure there's at least 1 auto-buff for the player.
                    if(AutoBuffs.ContainsKey(ptMember.Name) && AutoBuffs[ptMember.Name].Any())
                    {
                        // First check if they're ActiveBuffs are empty, and if so return first buff to cast.
                        if(!ActiveBuffs.ContainsKey(ptMember.Name) || !ActiveBuffs[ptMember.Name].Any())
                        {
                            return new EngineAction()
                            {
                                Spell = AutoBuffs[ptMember.Name].First(),
                                Target = ptMember.Name
                            };
                           
                        }
                        else
                        {
                            var missingBuffSpell = AutoBuffs[ptMember.Name].FirstOrDefault(buff => !ActiveBuffs[ptMember.Name].Contains(Data.SpellEffects[buff]));
                            if(!string.IsNullOrEmpty(missingBuffSpell))
                            {
                                return new EngineAction()
                                {
                                    Spell = missingBuffSpell,
                                    Target = ptMember.Name
                                };
                            }
                        }
                            
                    }
                }
            }        

            return null;
        }

        public void ToggleAutoBuff(string memberName, string spellName)
        {
            if(!AutoBuffs.ContainsKey(memberName))
            {
                AutoBuffs.Add(memberName, new List<string>());
            }

            if(AutoBuffs[memberName].Contains(spellName))
            {
                // If we already had the buff enabled, remove it.
                AutoBuffs[memberName] = AutoBuffs[memberName].Where(spell => spell != spellName);
            }
            else
            {
                AutoBuffs[memberName].Prepend(spellName);
            }
        }

        public bool BuffEnabled(string memberName, string spellName)
        {
            return AutoBuffs.ContainsKey(memberName) && AutoBuffs[memberName].Any(spell => spell == spellName);
        }

        // Since we can only have one socket for the addon, we let the main form
        // explicitly manage our buff list instead of doing it internally.
        public void UpdateBuffs(string memberName, IEnumerable<short> buffs)
        {
            lock(ActiveBuffs)
            {
                if(buffs.Any())
                {
                    ActiveBuffs[memberName] = buffs;
                }
                else if(ActiveBuffs.ContainsKey(memberName))
                {
                    ActiveBuffs.Remove(memberName);
                }
            }
        }

    }
}
