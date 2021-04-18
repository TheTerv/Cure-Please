
using CurePlease.Model;
using CurePlease.Model.Config;
using CurePlease.Model.Constants;
using CurePlease.Utilities;
using EliteMMO.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
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
             
        private DateTime currentTime = DateTime.Now;    

        private BuffConfig Config { get; set; }
        private EliteAPI PL { get; set; }
        private EliteAPI Monitored { get; set; }

        private bool AddonLoaded = false;
        private Dictionary<string, IEnumerable<short>> ActiveBuffs = new Dictionary<string, IEnumerable<short>>();
        private UdpClient BuffSocket;

        // TODO: Should this just be the exact spell we want to cast instead of the buff id?
        // Would probably be cleaner.
        private Dictionary<string, IEnumerable<string>> AutoBuffs = new Dictionary<string, IEnumerable<string>>();

        public BuffEngine(EliteAPI pl, EliteAPI mon, BuffConfig config)
        {
            Config = config;

            PL = pl;
            Monitored = mon;

            // Initialize the socket on our port, and then begin receiving data.
            // This will continually call itself to receive the next packet in the background.
            BuffSocket = new UdpClient(Convert.ToInt32(Config.AddonPort));
            BuffSocket.BeginReceive(new AsyncCallback(OnBuffDataReceived), BuffSocket);
            
        }

        // TODO: Setup buffs to be based on a priority system, and don't search whole party!
        // Going to try making the addon mandatory, and making all the buff decisions based on
        // what we're receiving from there instead of the timer mess.
        // May have to go back to the old system later.
        public EngineAction Run()
        {
            var actionResult = new EngineAction
            {
                Target = Target.Me
            };

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
                            actionResult.Spell = AutoBuffs[ptMember.Name].First();
                            actionResult.Target = ptMember.Name;
                            break;
                        }
                        else
                        {
                            var missingBuffSpell = AutoBuffs[ptMember.Name].FirstOrDefault(buff => !ActiveBuffs[ptMember.Name].Contains(Data.SpellEffects[buff]));
                            if(!string.IsNullOrEmpty(missingBuffSpell))
                            {
                                actionResult.Spell = missingBuffSpell;
                                actionResult.Target = ptMember.Name;
                                break;
                            }
                        }
                            
                    }
                }
            }        

            return actionResult;
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

        private void OnBuffDataReceived(IAsyncResult result)
        {
            if(!AddonLoaded)
            {
                return;
            }

            // The only thing passed in through the async state is the client itself.
            // So that when we're done, we can tell it to receive the next packet.
            UdpClient socket = result.AsyncState as UdpClient;
            IPEndPoint groupEP = new IPEndPoint(IPAddress.Parse(Config.AddonIP), Convert.ToInt32(Config.AddonPort));

            try
            {
                byte[] receive_byte_array = socket.EndReceive(result, ref groupEP);

                string received_data = Encoding.ASCII.GetString(receive_byte_array, 0, receive_byte_array.Length);

                string[] commands = received_data.Split('_');

                
                if (commands[1] == "buffs" && commands.Count() == 4)
                {
                    
                    var memberName = commands[2];
                    var memberBuffs = commands[3];

                    if (!string.IsNullOrEmpty(memberBuffs))
                    {
                        // Filter out the debuffs.
                        var buffs = memberBuffs.Split(',').Select(str => short.Parse(str.Trim())).Where(buff => !Data.DebuffPriorities.Keys.Cast<short>().Contains(buff));
                        if (buffs.Any())
                        {
                            lock (ActiveBuffs)
                            {
                                ActiveBuffs[memberName] = buffs;
                            }
                        }                 
                    }               
                }           
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
            }

            socket.BeginReceive(new AsyncCallback(OnBuffDataReceived), socket);
        }
    }
}
