using CurePlease.Engine;
using CurePlease.Model;
using CurePlease.Model.Constants;
using CurePlease.Model.Enums;
using CurePlease.Utilities;
using EliteMMO.API;
using Serilog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static EliteMMO.API.EliteAPI;

namespace CurePlease
{
    public partial class MainForm : Form
    {
        private static ConfigForm Config = new ConfigForm();

        private int lastCommand = 0;

        public bool CastingBackground_Check = false;
        public bool JobAbilityLock_Check = false;

        public string JobAbilityCMD = string.Empty;

        public string WindowerMode = string.Empty;

        public static EliteAPI PL;

        public static EliteAPI Monitored;

        public ListBox processids = new ListBox();

        public UdpClient AddonClient;

        // TODO: Initialize these configs explicitly after we've hooked into the game
        // and/or loaded/saved our config form.
        public SongEngine SongEngine;

        private readonly IGeoEngine _GeoEngine;
        private readonly IFollowEngine _FollowEngine;

        public BuffEngine _BuffEngine = new BuffEngine();

        public DebuffEngine _DebuffEngine = new DebuffEngine();

        public PLEngine PLEngine;

        public CureEngine CureEngine = new CureEngine();        

        public string castingSpell = string.Empty;

        // Stores the previously-colored button, if any
        public Dictionary<string, IEnumerable<short>> ActiveBuffs = new Dictionary<string, IEnumerable<short>>();

        private byte playerOptionsSelected;

        private byte autoOptionsSelected;

        private bool pauseActions;

        private void PaintBorderlessGroupBox(object sender, PaintEventArgs e)
        {
            GroupBox box = sender as GroupBox;
            DrawGroupBox(box, e.Graphics, Color.Black, Color.Gray);
        }

        private void DrawGroupBox(GroupBox box, Graphics g, Color textColor, Color borderColor)
        {
            if (box != null)
            {
                Brush textBrush = new SolidBrush(textColor);
                Brush borderBrush = new SolidBrush(borderColor);
                Pen borderPen = new Pen(borderBrush);
                SizeF strSize = g.MeasureString(box.Text, box.Font);
                Rectangle rect = new Rectangle(box.ClientRectangle.X,
                                           box.ClientRectangle.Y + (int)(strSize.Height / 2),
                                           box.ClientRectangle.Width - 1,
                                           box.ClientRectangle.Height - (int)(strSize.Height / 2) - 1);

                // Clear text and border
                g.Clear(BackColor);

                // Draw text
                g.DrawString(box.Text, box.Font, textBrush, box.Padding.Left, 0);

                // Drawing Border
                //Left
                g.DrawLine(borderPen, rect.Location, new Point(rect.X, rect.Y + rect.Height));
                //Right
                g.DrawLine(borderPen, new Point(rect.X + rect.Width, rect.Y), new Point(rect.X + rect.Width, rect.Y + rect.Height));
                //Bottom
                g.DrawLine(borderPen, new Point(rect.X, rect.Y + rect.Height), new Point(rect.X + rect.Width, rect.Y + rect.Height));
                //Top1
                g.DrawLine(borderPen, new Point(rect.X, rect.Y), new Point(rect.X + box.Padding.Left, rect.Y));
                //Top2
                g.DrawLine(borderPen, new Point(rect.X + box.Padding.Left + (int)strSize.Width, rect.Y), new Point(rect.X + rect.Width, rect.Y));
            }
        }

        private void PaintButton(object sender, PaintEventArgs e)
        {
            Button button = sender as Button;

            button.FlatAppearance.BorderColor = Color.Gray;
        }

        public MainForm(IGeoEngine geoEngine, IFollowEngine followEngine)
        {

            _GeoEngine = geoEngine;
            _FollowEngine = followEngine;

            InitializeComponent();

            if (!CheckForDLLFiles())
            {
                return;
            }

            // Show the current version number..
            Text = notifyIcon1.Text = "Cure Please v" + Application.ProductVersion;

            notifyIcon1.BalloonTipTitle = "Cure Please v" + Application.ProductVersion;
            notifyIcon1.BalloonTipText = "CurePlease has been minimized.";
            notifyIcon1.BalloonTipIcon = ToolTipIcon.Info;

            CheckForProcesses();
        }

        private void CheckForProcesses()
        {
            IEnumerable<Process> pol = Process.GetProcessesByName("pol").Union(Process.GetProcessesByName("xiloader")).Union(Process.GetProcessesByName("edenxi"));

            if (pol.Count() < 1)
            {
                MessageBox.Show("FFXI not found");
            }
            else
            {
                for (int i = 0; i < pol.Count(); i++)
                {
                    POLID.Items.Add(pol.ElementAt(i).MainWindowTitle);
                    POLID2.Items.Add(pol.ElementAt(i).MainWindowTitle);
                    processids.Items.Add(pol.ElementAt(i).Id);
                }

                POLID.SelectedIndex = 0;
                POLID2.SelectedIndex = 0;
                processids.SelectedIndex = 0;
            }
        }

        private void LoadAddon()
        {
            if (string.IsNullOrWhiteSpace(WindowerMode))
                return;

            if (!ConfigForm.config.pauseOnStartBox && PL != null)
            {
                AddonEngine.LoadAddonInClient(ConfigForm.config, PL.ThirdParty, WindowerMode);

                if (AddonClient == null)
                {
                    AddonClient = new UdpClient(Convert.ToInt32(ConfigForm.config.listeningPort));
                    AddonClient.BeginReceive(new AsyncCallback(OnAddonDataReceived), AddonClient);
                }

                AddCurrentAction("LUA Addon loaded. ( " + ConfigForm.config.ipAddress + " - " + ConfigForm.config.listeningPort + " )");
            }
        }

        private void setinstance_Click(object sender, EventArgs e)
        {
            if (!CheckForDLLFiles())
            {
                return;
            }

            processids.SelectedIndex = POLID.SelectedIndex;
            PL = new EliteAPI((int)processids.SelectedItem);
            plLabel.Text = "Selected PL: " + PL.Player.Name;
            Text = notifyIcon1.Text = PL.Player.Name + " - " + "Cure Please v" + Application.ProductVersion;

            plLabel.ForeColor = Color.Green;
            POLID.BackColor = Color.White;
            plPosition.Enabled = true;
            setinstance2.Enabled = true;

            partyMembersUpdate.Enabled = true;
            actionTimer.Enabled = true;
            pauseButton.Enabled = true;

            // LOAD AUTOMATIC SETTINGS
            ConfigForm.LoadConfiguration();

            LoadAddon();

            StartYourEngines();

            _FollowEngine.Start();
        }

        private void setinstance2_Click(object sender, EventArgs e)
        {
            if (!CheckForDLLFiles())
            {
                return;
            }

            processids.SelectedIndex = POLID2.SelectedIndex;
            Monitored = new EliteAPI((int)processids.SelectedItem);
            monitoredLabel.Text = "Monitoring: " + Monitored.Player.Name;
            monitoredLabel.ForeColor = Color.Green;
            POLID2.BackColor = Color.White;

            if (ConfigForm.config.pauseOnStartBox)
            {                
                pauseButton.Text = "Loaded, Paused!";
                pauseButton.ForeColor = Color.Red;
                StopBot();
            }
            else
            {
                if (ConfigForm.config.MinimiseonStart == true && WindowState != FormWindowState.Minimized)
                {
                    WindowState = FormWindowState.Minimized;
                }
            }

            lastCommand = Monitored.ThirdParty.ConsoleIsNewCommand();

            StartYourEngines();
        }

        private void StartYourEngines()
        {
            // TODO: Solve this hack for intializing these after both selected.
            // TODO: Disconnect from "Monitored" (ie: we shouldn't need monitored to run)
            if (Monitored != null)
            {
                SongEngine = new SongEngine(PL, Monitored);
                PLEngine = new PLEngine(PL, Monitored);
            }

            _FollowEngine.Setup(PL, Monitored, ConfigForm.config);
        }

        public void AddCurrentAction(string message)
        {
            if (!string.IsNullOrWhiteSpace(message))
            {
                var timestamp = DateTime.Now.ToString("[HH:mm:ss] ");
                message = timestamp + message;

                currentAction.Text = message;
                actionlog_box.AppendText(message + Environment.NewLine);
            }
        }

        private bool CheckForDLLFiles()
        {
            foreach (Process dats in Process.GetProcessesByName("pol").Union(Process.GetProcessesByName("xiloader")).Union(Process.GetProcessesByName("edenxi")))
            {
                for (int i = 0; i < dats.Modules.Count; i++)
                {
                    if (dats.Modules[i].FileName.Contains("Ashita.dll"))
                    {
                        WindowerMode = "Ashita";
                        return true;
                    }
                    else if (dats.Modules[i].FileName.Contains("Hook.dll"))
                    {
                        WindowerMode = "Windower";
                        return true;
                    }
                }
            }

            MessageBox.Show(
                    "Unable to identify if Windower or Ashita. Addon will not be loaded.",
                    "Error");

            return true;
        }

        private void partyMembersUpdate_TickAsync(object sender, EventArgs e)
        {
            if (PL == null)
            {
                return;
            }

            if (PL.Player.LoginStatus == (int)LoginStatus.Loading)
            {
                if (ConfigForm.config.pauseOnZoneBox == true)
                {
                    if (pauseActions != true)
                    {
                        pauseButton.Text = "Zoned, paused.";
                        pauseButton.ForeColor = Color.Red;
                        StopBot();
                    }
                }
                else
                {
                    if (pauseActions != true)
                    {
                        pauseButton.Text = "Zoned, waiting.";
                        pauseButton.ForeColor = Color.Red;
                        _FollowEngine.Stop();

                        Thread.Sleep(17000);

                        pauseButton.Text = "Pause";
                        pauseButton.ForeColor = Color.Black;
                        _FollowEngine.Start();
                    }
                }

                ActiveBuffs.Clear();
            }

            PartyUtils.UpdatePartyControls(this.Controls);
        }

        private void CastSpell(string partyMemberName, string spellName, [Optional] string OptionalExtras)
        {
            if (CastingBackground_Check)
            {
                return;
            }

            var apiSpell = PL.Resources.GetSpell(spellName, 0);

            CastSpell(partyMemberName, apiSpell, OptionalExtras);
        }


        private void CastSpell(string partyMemberName, ISpell magic, [Optional] string OptionalExtras)
        {
            if (magic == null)
            {
                AddCurrentAction("Tried to cast a a spell but magic was NULL!!!");
                return;
            }

            castingSpell = magic.Name[0];

            PL.ThirdParty.SendString("/ma \"" + castingSpell + "\" " + partyMemberName);

            if (OptionalExtras != null)
            {
                AddCurrentAction("Casting: " + castingSpell + " [" + OptionalExtras + "]");
            }
            else
            {
                AddCurrentAction("Casting: " + castingSpell);
            }

            CastingBackground_Check = true;

            if (ConfigForm.config.trackCastingPackets == true && ConfigForm.config.EnableAddOn == true)
            {
                if (!ProtectCasting.IsBusy) { ProtectCasting.RunWorkerAsync(); }
            }
            else
            {
                castingLockLabel.Text = "Casting is LOCKED";
                if (!ProtectCasting.IsBusy) { ProtectCasting.RunWorkerAsync(); }
            }
        }

        #region Primary Logic

        private bool ReadyForAction()
        {
            if (pauseActions)
            {
                return false;
            }

            // Skip if we aren't hooked into the game.
            if (PL == null || PL.Player.LoginStatus != (int)LoginStatus.LoggedIn)
            {
                return false;
            }

            // Skip if we're busy or immobilized.
            if (JobAbilityLock_Check || CastingBackground_Check || PL.CantAct())
            {
                return false;
            }

            if (PL.Player.Status != (uint)Status.Standing && PL.Player.Status != (uint)Status.Fighting)
            {
                return false;
            }

            if (_FollowEngine.IsMoving())
            {
                return false;
            }

            return true;
        }

        // This is the timer that does our decision loop.
        // All the main action related stuff happens in here.
        private async void actionTimer_TickAsync(object sender, EventArgs e)
        {
            this.actionTimer.Stop();

            if (!ReadyForAction())
            {
                this.actionTimer.Start();
                return;
            }

            // IF ENABLED PAUSE ON KO
            if (ConfigForm.config.pauseOnKO && (PL.Player.Status == 2 || PL.Player.Status == 3))
            {
                pauseButton.Text = "Paused!";
                pauseButton.ForeColor = Color.Red;
                StopBot();
                this.actionTimer.Start();
                return;
            }

            // IF YOU ARE DEAD BUT RERAISE IS AVAILABLE THEN ACCEPT RAISE
            if (ConfigForm.config.AcceptRaise == true && (PL.Player.Status == 2 || PL.Player.Status == 3))
            {
                if (PL.Menu.IsMenuOpen && PL.Menu.HelpName == "Revival" && PL.Menu.MenuIndex == 1 && ((ConfigForm.config.AcceptRaiseOnlyWhenNotInCombat == true && Monitored != null && Monitored.Player.Status != 1) || ConfigForm.config.AcceptRaiseOnlyWhenNotInCombat == false))
                {
                    await Task.Delay(2000);
                    AddCurrentAction("Accepting Raise or Reraise.");
                    PL.ThirdParty.KeyPress(EliteMMO.API.Keys.NUMPADENTER);
                    await Task.Delay(5000);
                    AddCurrentAction(string.Empty);
                }
            }

            IEnumerable<PartyMember> activeMembers = PL.GetActivePartyMembers();

            /////////////////////////// Charmed CHECK /////////////////////////////////////
            // TODO: Charm logic is messy because it's not configurable currently. Clean this up when adding auto-sleep options.
            if (PL.Player.MainJob == (byte)Job.BRD)
            {
                // Get the list of anyone who's charmed and in range.
                var charmedMembers = activeMembers.Where(pm => PL.CanCastOn(pm) && ActiveBuffs.ContainsKey(pm.Name) && (ActiveBuffs[pm.Name].Contains((short)StatusEffect.Charm1) || ActiveBuffs[pm.Name].Contains((short)StatusEffect.Charm2)));

                if (charmedMembers.Any())
                {
                    // We target the first charmed member who's not already asleep.
                    var sleepTarget = charmedMembers.FirstOrDefault(member => !(ActiveBuffs[member.Name].Contains((short)StatusEffect.Sleep) || ActiveBuffs[member.Name].Contains((short)StatusEffect.Sleep2)));

                    if (sleepTarget != default)
                    {
                        // For now add some redundancy in case the first cast is resisted.
                        var sleepSong = PL.SpellAvailable(Spells.Foe_Lullaby_II) ? Spells.Foe_Lullaby_II : Spells.Foe_Lullaby;

                        CastSpell(sleepTarget.Name, sleepSong);
                        this.actionTimer.Start();
                        return;
                    }
                }
            }

            /////////////////////////// DOOM CHECK /////////////////////////////////////
            var doomedMembers = activeMembers.Count(pm => PL.CanCastOn(pm) && ActiveBuffs.ContainsKey(pm.Name) && ActiveBuffs[pm.Name].Contains((short)StatusEffect.Doom));
            if (doomedMembers > 0)
            {
                var doomCheckResult = _DebuffEngine.Run(PL, Monitored, Config.GetDebuffConfig());
                if (doomCheckResult != null && doomCheckResult.Spell != null)
                {
                    CastSpell(doomCheckResult.Target, doomCheckResult.Spell);
                    this.actionTimer.Start();
                    return;
                }
            }

            // Set array values for GUI "Enabled" checkboxes
            bool[] enabledBoxes = new bool[] {
                player0enabled.Checked, player1enabled.Checked, player2enabled.Checked, player3enabled.Checked, player4enabled.Checked, player5enabled.Checked,
                player6enabled.Checked, player7enabled.Checked, player8enabled.Checked, player9enabled.Checked, player10enabled.Checked, player11enabled.Checked,
                player12enabled.Checked, player13enabled.Checked, player14enabled.Checked, player15enabled.Checked, player16enabled.Checked, player17enabled.Checked
            };


            // Set array values for GUI "High Priority" checkboxes
            bool[] highPriorityBoxes = new bool[] {
                player0priority.Checked, player1priority.Checked, player2priority.Checked, player3priority.Checked, player4priority.Checked, player5priority.Checked,
                player6priority.Checked, player7priority.Checked, player8priority.Checked, player9priority.Checked, player10priority.Checked, player11priority.Checked,
                player12priority.Checked, player13priority.Checked, player14priority.Checked, player15priority.Checked, player16priority.Checked, player17priority.Checked
            };

            // For now we run these before deciding what to do, in case we need
            // to skip a low priority cure.
            // CURE ENGINE
            var cureResult = CureEngine?.Run(PL, Monitored, Config.GetCureConfig(), enabledBoxes, highPriorityBoxes);

            if (cureResult != null)
            {
                // RUN DEBUFF REMOVAL - CONVERTED TO FUNCTION SO CAN BE RUN IN MULTIPLE AREAS
                var debuffResult = _DebuffEngine.Run(PL, Monitored, Config.GetDebuffConfig());

                if (!string.IsNullOrEmpty(cureResult.Spell))
                {
                    bool lowPriority = Array.IndexOf(Data.CureTiers, cureResult.Spell) < 2;

                    // Only cast the spell/JA if we don't need to skip debuffs based on
                    // config and low priority.
                    if (!lowPriority || !ConfigForm.config.PrioritiseOverLowerTier || debuffResult == null)
                    {
                        if (!string.IsNullOrEmpty(cureResult.JobAbility))
                        {
                            JobAbility_Wait(cureResult.Message, cureResult.JobAbility);
                        }

                        CastSpell(cureResult.Target, cureResult.Spell);
                        this.actionTimer.Start();
                        return;
                    }
                }

                if (debuffResult != null)
                {
                    CastSpell(debuffResult.Target, debuffResult.Spell);
                    this.actionTimer.Start();
                    return;
                }

                // TODO: Need to run cure AND debuff engine then decide which to execute.
                // I consider cure/cure II to be low tier once cure III gets above 700 HP.
                //if (Array.IndexOf(Data.CureTiers, cureSpell) < 2 && ConfigForm.config.PrioritiseOverLowerTier == true)
                //{
                //    var debuffResult = DebuffEngine.Run();
                //    if (debuffResult != null && debuffResult.Spell != null)
                //    {
                //        CastSpell(debuffResult.Target, debuffResult.Spell);
                //        return;
                //    }
                //}
            }

            // PL AUTO BUFFS
            var plEngineResult = PLEngine?.Run(Config.GetPLConfig());
            if (plEngineResult != null)
            {
                if (!string.IsNullOrEmpty(plEngineResult.Item))
                {
                    Item_Wait(plEngineResult.Item);
                }

                if (!string.IsNullOrEmpty(plEngineResult.JobAbility))
                {
                    if (plEngineResult.JobAbility == Ability.Devotion)
                    {
                        PL.ThirdParty.SendString($"/ja \"{Ability.Devotion}\" {plEngineResult.Target}");
                    }
                    else
                    {
                        JobAbility_Wait(plEngineResult.Message, plEngineResult.JobAbility);
                    }
                }

                if (!string.IsNullOrEmpty(plEngineResult.Spell))
                {
                    var target = string.IsNullOrEmpty(plEngineResult.Target) ? "<me>" : plEngineResult.Target;
                    CastSpell(target, plEngineResult.Spell);
                }
            }


            // BARD SONGS

            if (PL.Player.MainJob == (byte)Job.BRD && ConfigForm.config.enableSinging && !PL.HasStatus(StatusEffect.Silence) && (PL.Player.Status == 1 || PL.Player.Status == 0))
            {
                var songAction = SongEngine.Run(Config.GetSongConfig());

                if (!string.IsNullOrEmpty(songAction.Spell))
                {
                    CastSpell(songAction.Target, songAction.Spell);
                }
            }

            // GEO Stuff

            else if (PL.Player.MainJob == (byte)Job.GEO && !PL.HasStatus(StatusEffect.Silence) && (PL.Player.Status == 1 || PL.Player.Status == 0))
            {
                var geoAction = _GeoEngine.Run(PL, Monitored, Config.GetGeoConfig());

                if (geoAction != null)
                {
                    // TODO: Abstract out this idea of error/ability/spell handling
                    // as it will apply to all the engines.
                    if (!string.IsNullOrEmpty(geoAction.Error))
                    {
                        showErrorMessage(geoAction.Error);
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(geoAction.JobAbility))
                        {
                            JobAbility_Wait(geoAction.JobAbility, geoAction.JobAbility);
                        }

                        if (!string.IsNullOrEmpty(geoAction.Spell))
                        {
                            CastSpell(geoAction.Target, geoAction.Spell);
                        }
                    }
                }
            }

            // Auto Casting BUFF STUFF                    
            var buffAction = _BuffEngine?.Run(Config.GetBuffConfig(), PL);

            if (buffAction != null)
            {
                if (!string.IsNullOrEmpty(buffAction.Error))
                {
                    showErrorMessage(buffAction.Error);
                }
                else
                {
                    if (!string.IsNullOrEmpty(buffAction.JobAbility))
                    {
                        JobAbility_Wait(buffAction.JobAbility, buffAction.JobAbility);
                    }

                    if (!string.IsNullOrEmpty(buffAction.Spell))
                    {
                        CastSpell(buffAction.Target, buffAction.Spell);
                    }
                }
            }

            this.actionTimer.Start();
        }

        #endregion

        private void settingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ConfigForm settings = new ConfigForm();
            settings.Show();
        }

        private void ShowPlayerOptionsFor(GroupBox party, byte ptIndex)
        {
            playerOptionsSelected = ptIndex;
            var name = PL.Party.GetPartyMembers()[ptIndex].Name;

            autoHasteToolStripMenuItem.Checked = _BuffEngine.BuffEnabled(name, Spells.Haste);
            autoHasteIIToolStripMenuItem.Checked = _BuffEngine.BuffEnabled(name, Spells.Haste_II);
            autoAdloquiumToolStripMenuItem.Checked = _BuffEngine.BuffEnabled(name, Spells.Adloquium);
            autoFlurryToolStripMenuItem.Checked = _BuffEngine.BuffEnabled(name, Spells.Flurry);
            autoFlurryIIToolStripMenuItem.Checked = _BuffEngine.BuffEnabled(name, Spells.Flurry_II);
            autoProtectToolStripMenuItem.Checked = _BuffEngine.BuffEnabled(name, Spells.Protect);
            autoShellToolStripMenuItem.Checked = _BuffEngine.BuffEnabled(name, Spells.Shell);

            playerOptions.Show(party, new Point(0, 0));
        }

        private void ShowPlayerBuffsFor(GroupBox party, byte ptIndex)
        {
            autoOptionsSelected = ptIndex;
            var name = PL.Party.GetPartyMembers()[ptIndex].Name;

            // TODO: Figure out tiers and stuff, don't play SCH so not tier-II storms probably busted.
            if (party == party0)
            {
                autoPhalanxIIToolStripMenuItem1.Checked = _BuffEngine.BuffEnabled(name, Spells.Phalanx_II);
                autoRegenVToolStripMenuItem.Checked = _BuffEngine.BuffEnabled(name, Spells.Regen);
                autoRefreshIIToolStripMenuItem.Checked = _BuffEngine.BuffEnabled(name, Spells.Refresh);
                SandstormToolStripMenuItem.Checked = _BuffEngine.BuffEnabled(name, Spells.Sandstorm);
                RainstormToolStripMenuItem.Checked = _BuffEngine.BuffEnabled(name, Spells.Rainstorm);
                WindstormToolStripMenuItem.Checked = _BuffEngine.BuffEnabled(name, Spells.Windstorm);
                FirestormToolStripMenuItem.Checked = _BuffEngine.BuffEnabled(name, Spells.Firestorm);
                HailstormToolStripMenuItem.Checked = _BuffEngine.BuffEnabled(name, Spells.Hailstorm);
                ThunderstormToolStripMenuItem.Checked = _BuffEngine.BuffEnabled(name, Spells.Thunderstorm);
                VoidstormToolStripMenuItem.Checked = _BuffEngine.BuffEnabled(name, Spells.Voidstorm);
                AurorastormToolStripMenuItem.Checked = _BuffEngine.BuffEnabled(name, Spells.Aurorastorm);
            }

            autoOptions.Show(party, new Point(0, 0));
        }

        private void player0optionsButton_Click(object sender, EventArgs e)
        {
            ShowPlayerOptionsFor(party0, 0);
        }

        private void player1optionsButton_Click(object sender, EventArgs e)
        {
            ShowPlayerOptionsFor(party0, 1);
        }

        private void player2optionsButton_Click(object sender, EventArgs e)
        {
            ShowPlayerOptionsFor(party0, 2);
        }

        private void player3optionsButton_Click(object sender, EventArgs e)
        {
            ShowPlayerOptionsFor(party0, 3);
        }

        private void player4optionsButton_Click(object sender, EventArgs e)
        {
            ShowPlayerOptionsFor(party0, 4);
        }

        private void player5optionsButton_Click(object sender, EventArgs e)
        {
            ShowPlayerOptionsFor(party0, 5);
        }

        private void player6optionsButton_Click(object sender, EventArgs e)
        {
            ShowPlayerOptionsFor(party1, 6);
        }

        private void player7optionsButton_Click(object sender, EventArgs e)
        {
            ShowPlayerOptionsFor(party1, 7);
        }

        private void player8optionsButton_Click(object sender, EventArgs e)
        {
            ShowPlayerOptionsFor(party1, 8);
        }

        private void player9optionsButton_Click(object sender, EventArgs e)
        {
            ShowPlayerOptionsFor(party1, 9);
        }

        private void player10optionsButton_Click(object sender, EventArgs e)
        {
            ShowPlayerOptionsFor(party1, 10);
        }

        private void player11optionsButton_Click(object sender, EventArgs e)
        {
            ShowPlayerOptionsFor(party1, 11);
        }

        private void player12optionsButton_Click(object sender, EventArgs e)
        {
            ShowPlayerOptionsFor(party2, 12);
        }

        private void player13optionsButton_Click(object sender, EventArgs e)
        {
            ShowPlayerOptionsFor(party2, 13);
        }

        private void player14optionsButton_Click(object sender, EventArgs e)
        {
            ShowPlayerOptionsFor(party2, 14);
        }

        private void player15optionsButton_Click(object sender, EventArgs e)
        {
            ShowPlayerOptionsFor(party2, 15);
        }

        private void player16optionsButton_Click(object sender, EventArgs e)
        {
            ShowPlayerOptionsFor(party2, 16);
        }

        private void player17optionsButton_Click(object sender, EventArgs e)
        {
            ShowPlayerOptionsFor(party2, 17);
        }

        private void player0buffsButton_Click(object sender, EventArgs e)
        {
            ShowPlayerBuffsFor(party0, 0);
        }

        private void player1buffsButton_Click(object sender, EventArgs e)
        {
            ShowPlayerBuffsFor(party0, 1);
        }

        private void player2buffsButton_Click(object sender, EventArgs e)
        {
            ShowPlayerBuffsFor(party0, 2);
        }

        private void player3buffsButton_Click(object sender, EventArgs e)
        {
            ShowPlayerBuffsFor(party0, 3);
        }

        private void player4buffsButton_Click(object sender, EventArgs e)
        {
            ShowPlayerBuffsFor(party0, 4);
        }

        private void player5buffsButton_Click(object sender, EventArgs e)
        {
            ShowPlayerBuffsFor(party0, 5);
        }

        private void player6buffsButton_Click(object sender, EventArgs e)
        {
            ShowPlayerBuffsFor(party1, 6);
        }

        private void player7buffsButton_Click(object sender, EventArgs e)
        {
            ShowPlayerBuffsFor(party1, 7);
        }

        private void player8buffsButton_Click(object sender, EventArgs e)
        {
            ShowPlayerBuffsFor(party1, 8);
        }

        private void player9buffsButton_Click(object sender, EventArgs e)
        {
            ShowPlayerBuffsFor(party1, 9);
        }

        private void player10buffsButton_Click(object sender, EventArgs e)
        {
            ShowPlayerBuffsFor(party1, 10);
        }

        private void player11buffsButton_Click(object sender, EventArgs e)
        {
            ShowPlayerBuffsFor(party1, 11);
        }

        private void player12buffsButton_Click(object sender, EventArgs e)
        {
            ShowPlayerBuffsFor(party2, 12);
        }

        private void player13buffsButton_Click(object sender, EventArgs e)
        {
            ShowPlayerBuffsFor(party2, 13);
        }

        private void player14buffsButton_Click(object sender, EventArgs e)
        {
            ShowPlayerBuffsFor(party2, 14);
        }

        private void player15buffsButton_Click(object sender, EventArgs e)
        {
            ShowPlayerBuffsFor(party2, 15);
        }

        private void player16buffsButton_Click(object sender, EventArgs e)
        {
            ShowPlayerBuffsFor(party2, 16);
        }

        private void player17buffsButton_Click(object sender, EventArgs e)
        {
            ShowPlayerBuffsFor(party2, 17);
        }

        private void Item_Wait(string ItemName)
        {
            if (CastingBackground_Check != true && JobAbilityLock_Check != true)
            {
                Invoke((MethodInvoker)(async () =>
                {
                    JobAbilityLock_Check = true;
                    castingLockLabel.Text = "Casting is LOCKED for ITEM Use.";
                    AddCurrentAction("Using an Item: " + ItemName);
                    PL.ThirdParty.SendString("/item \"" + ItemName + "\" <me>");
                    await Task.Delay(TimeSpan.FromSeconds(5));
                    castingLockLabel.Text = "Casting is UNLOCKED";
                    AddCurrentAction(string.Empty);
                    castingSpell = string.Empty;
                    JobAbilityLock_Check = false;
                }));
            }
        }

        private void JobAbility_Wait(string JobabilityDATA, string JobAbilityName)
        {
            if (CastingBackground_Check != true && JobAbilityLock_Check != true)
            {
                Invoke((MethodInvoker)(async () =>
                {
                    JobAbilityLock_Check = true;
                    castingLockLabel.Text = "Casting is LOCKED for a JA.";
                    AddCurrentAction("Using a Job Ability: " + JobabilityDATA);
                    PL.ThirdParty.SendString("/ja \"" + JobAbilityName + "\" <me>");
                    await Task.Delay(TimeSpan.FromSeconds(2));
                    castingLockLabel.Text = "Casting is UNLOCKED";
                    AddCurrentAction(string.Empty);
                    castingSpell = string.Empty;
                    JobAbilityLock_Check = false;
                }));
            }
        }

        private void autoHasteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // TODO: Add in special logic to make sure we can't select more then
            // ONE of haste/haste2/flurry/flurry2
            var name = PL.Party.GetPartyMembers()[playerOptionsSelected].Name;
            _BuffEngine.ToggleAutoBuff(name, Spells.Haste);
        }

        private void autoHasteIIToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var name = PL.Party.GetPartyMembers()[playerOptionsSelected].Name;
            _BuffEngine.ToggleAutoBuff(name, Spells.Haste_II);
        }

        private void autoAdloquiumToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var name = PL.Party.GetPartyMembers()[playerOptionsSelected].Name;
            _BuffEngine.ToggleAutoBuff(name, Spells.Adloquium);
        }

        private void autoFlurryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var name = PL.Party.GetPartyMembers()[playerOptionsSelected].Name;
            _BuffEngine.ToggleAutoBuff(name, Spells.Flurry);
        }

        private void autoFlurryIIToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var name = PL.Party.GetPartyMembers()[playerOptionsSelected].Name;
            _BuffEngine.ToggleAutoBuff(name, Spells.Flurry_II);
        }

        private void autoProtectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var name = PL.Party.GetPartyMembers()[playerOptionsSelected].Name;
            _BuffEngine.ToggleAutoBuff(name, Spells.Protect);
        }

        private void enableDebuffRemovalToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _DebuffEngine.ToggleSpecifiedMember(PL.Party.GetPartyMembers()[playerOptionsSelected].Name);
        }

        private void autoShellToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var name = PL.Party.GetPartyMembers()[playerOptionsSelected].Name;
            _BuffEngine.ToggleAutoBuff(name, Spells.Shell);
        }

        private void autoHasteToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            var name = PL.Party.GetPartyMembers()[autoOptionsSelected].Name;
            _BuffEngine.ToggleAutoBuff(name, Spells.Haste);
        }

        private void autoPhalanxIIToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            var name = PL.Party.GetPartyMembers()[autoOptionsSelected].Name;
            _BuffEngine.ToggleAutoBuff(name, Spells.Phalanx_II);
        }

        private void autoRegenVToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var name = PL.Party.GetPartyMembers()[autoOptionsSelected].Name;
            _BuffEngine.ToggleAutoBuff(name, Spells.Regen);
        }

        private void autoRefreshIIToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var name = PL.Party.GetPartyMembers()[autoOptionsSelected].Name;
            _BuffEngine.ToggleAutoBuff(name, Spells.Refresh);
        }

        private void hasteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CastSpell(PL.Party.GetPartyMembers()[playerOptionsSelected].Name, Spells.Haste);
        }

        private void followToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ConfigForm.config.autoFollowName = PL.Party.GetPartyMembers()[playerOptionsSelected].Name;
        }

        private void stopfollowToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ConfigForm.config.autoFollowName = string.Empty;
        }

        private void EntrustTargetToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ConfigForm.config.EntrustedSpell_Target = PL.Party.GetPartyMembers()[playerOptionsSelected].Name;
        }

        private void GeoTargetToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ConfigForm.config.LuopanSpell_Target = PL.Party.GetPartyMembers()[playerOptionsSelected].Name;
        }

        private void DevotionTargetToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ConfigForm.config.DevotionTargetName = PL.Party.GetPartyMembers()[playerOptionsSelected].Name;
        }

        private void HateEstablisherToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ConfigForm.config.autoTarget_Target = PL.Party.GetPartyMembers()[playerOptionsSelected].Name;
        }

        private void phalanxIIToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CastSpell(PL.Party.GetPartyMembers()[playerOptionsSelected].Name, Spells.Phalanx_II);
        }

        private void invisibleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CastSpell(PL.Party.GetPartyMembers()[playerOptionsSelected].Name, Spells.Invisible);
        }

        private void refreshToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CastSpell(PL.Party.GetPartyMembers()[playerOptionsSelected].Name, Spells.Refresh);
        }

        private void refreshIIToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CastSpell(PL.Party.GetPartyMembers()[playerOptionsSelected].Name, Spells.Refresh_II);
        }

        private void refreshIIIToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CastSpell(PL.Party.GetPartyMembers()[playerOptionsSelected].Name, Spells.Refresh_III);
        }

        private void sneakToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CastSpell(PL.Party.GetPartyMembers()[playerOptionsSelected].Name, Spells.Sneak);
        }

        private void regenIIToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CastSpell(PL.Party.GetPartyMembers()[playerOptionsSelected].Name, Spells.Regen_II);
        }

        private void regenIIIToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CastSpell(PL.Party.GetPartyMembers()[playerOptionsSelected].Name, Spells.Regen_III);
        }

        private void regenIVToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CastSpell(PL.Party.GetPartyMembers()[playerOptionsSelected].Name, Spells.Regen_IV);
        }

        private void eraseToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CastSpell(PL.Party.GetPartyMembers()[playerOptionsSelected].Name, Spells.Erase);
        }

        private void sacrificeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CastSpell(PL.Party.GetPartyMembers()[playerOptionsSelected].Name, Spells.Sacrifice);
        }

        private void blindnaToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CastSpell(PL.Party.GetPartyMembers()[playerOptionsSelected].Name, Spells.Blindna);
        }

        private void cursnaToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CastSpell(PL.Party.GetPartyMembers()[playerOptionsSelected].Name, Spells.Cursna);
        }

        private void paralynaToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CastSpell(PL.Party.GetPartyMembers()[playerOptionsSelected].Name, Spells.Paralyna);
        }

        private void poisonaToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CastSpell(PL.Party.GetPartyMembers()[playerOptionsSelected].Name, Spells.Poisona);
        }

        private void stonaToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CastSpell(PL.Party.GetPartyMembers()[playerOptionsSelected].Name, Spells.Stona);
        }

        private void silenaToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CastSpell(PL.Party.GetPartyMembers()[playerOptionsSelected].Name, Spells.Silena);
        }

        private void virunaToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CastSpell(PL.Party.GetPartyMembers()[playerOptionsSelected].Name, Spells.Viruna);
        }

        private void SandstormToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // TODO: Similar to haste/flurry, etc. add logic to deal with storm
            // tiers and only one at a time being selected.
            var name = PL.Party.GetPartyMembers()[autoOptionsSelected].Name;
            _BuffEngine.ToggleAutoBuff(name, Spells.Sandstorm);
        }

        private void RainstormToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var name = PL.Party.GetPartyMembers()[autoOptionsSelected].Name;
            _BuffEngine.ToggleAutoBuff(name, Spells.Rainstorm);
        }

        private void WindstormToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var name = PL.Party.GetPartyMembers()[autoOptionsSelected].Name;
            _BuffEngine.ToggleAutoBuff(name, Spells.Windstorm);
        }

        private void FirestormToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var name = PL.Party.GetPartyMembers()[autoOptionsSelected].Name;
            _BuffEngine.ToggleAutoBuff(name, Spells.Firestorm);
        }

        private void HailstormToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var name = PL.Party.GetPartyMembers()[autoOptionsSelected].Name;
            _BuffEngine.ToggleAutoBuff(name, Spells.Hailstorm);
        }

        private void ThunderstormToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var name = PL.Party.GetPartyMembers()[autoOptionsSelected].Name;
            _BuffEngine.ToggleAutoBuff(name, Spells.Thunderstorm);
        }

        private void VoidstormToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var name = PL.Party.GetPartyMembers()[autoOptionsSelected].Name;
            _BuffEngine.ToggleAutoBuff(name, Spells.Voidstorm);
        }

        private void AurorastormToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var name = PL.Party.GetPartyMembers()[autoOptionsSelected].Name;
            _BuffEngine.ToggleAutoBuff(name, Spells.Aurorastorm);
        }

        private void protectIVToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CastSpell(PL.Party.GetPartyMembers()[playerOptionsSelected].Name, Spells.Protect_IV);
        }

        private void protectVToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CastSpell(PL.Party.GetPartyMembers()[playerOptionsSelected].Name, Spells.Protect_V);
        }

        private void shellIVToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CastSpell(PL.Party.GetPartyMembers()[playerOptionsSelected].Name, Spells.Shell_IV);
        }

        private void shellVToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CastSpell(PL.Party.GetPartyMembers()[playerOptionsSelected].Name, Spells.Shell_V);
        }

        private void button3_Click(object sender, EventArgs e)
        {

            if (pauseActions == false)
            {
                pauseButton.Text = "Paused!";
                pauseButton.ForeColor = Color.Red;
                StopBot();
            }
            else
            {
                pauseButton.Text = "Pause";
                pauseButton.ForeColor = Color.Black;
                actionTimer.Enabled = true;
                pauseActions = false;
                _FollowEngine.Start();

                if (ConfigForm.config.MinimiseonStart == true && WindowState != FormWindowState.Minimized)
                {
                    WindowState = FormWindowState.Minimized;
                }

                AddonEngine.LoadAddonInClient(ConfigForm.config, PL.ThirdParty, WindowerMode);

                if (AddonClient == null)
                {
                    AddonClient = new UdpClient(Convert.ToInt32(ConfigForm.config.listeningPort));
                    AddonClient.BeginReceive(new AsyncCallback(OnAddonDataReceived), AddonClient);
                }

                AddOnStatus_Click(sender, e);
            }
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            TopMost = !TopMost;
        }

        private void MouseClickTray(object sender, MouseEventArgs e)
        {
            if (WindowState == FormWindowState.Minimized && Visible == false)
            {
                Show();
                WindowState = FormWindowState.Normal;
            }
            else
            {
                Hide();
                WindowState = FormWindowState.Minimized;
            }
        }

        private void chatLogToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ChatlogForm form4 = new ChatlogForm(this);
            form4.Show();
        }

        private void partyBuffsdebugToolStripMenuItem_Click(object sender, EventArgs e)
        {
            PartyBuffs PartyBuffs = new PartyBuffs(this);
            PartyBuffs.Show();
        }

        private void refreshCharactersToolStripMenuItem_Click(object sender, EventArgs e)
        {
            IEnumerable<Process> pol = Process.GetProcessesByName("pol").Union(Process.GetProcessesByName("xiloader")).Union(Process.GetProcessesByName("edenxi"));

            if (PL.Player.LoginStatus != (int)LoginStatus.Loading)
            {
                if (pol.Count() < 1)
                {
                    MessageBox.Show("FFXI not found");
                }
                else
                {
                    POLID.Items.Clear();
                    POLID2.Items.Clear();
                    processids.Items.Clear();

                    for (int i = 0; i < pol.Count(); i++)
                    {
                        POLID.Items.Add(pol.ElementAt(i).MainWindowTitle);
                        POLID2.Items.Add(pol.ElementAt(i).MainWindowTitle);
                        processids.Items.Add(pol.ElementAt(i).Id);
                    }

                    POLID.SelectedIndex = 0;
                    POLID2.SelectedIndex = 0;
                }
            }
        }


        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            notifyIcon1.Icon = null;
            notifyIcon1.Dispose();

            if (PL != null)
            {
                AddonEngine.UnloadAddonInClient(ConfigForm.config, PL.ThirdParty, WindowerMode);
            }

        }

        private void showErrorMessage(string ErrorMessage)
        {
            pauseActions = true;
            pauseButton.Text = "Error!";
            pauseButton.ForeColor = Color.Red;
            actionTimer.Enabled = false;
            MessageBox.Show(ErrorMessage);
        }

        private void updateInstances_Tick(object sender, EventArgs e)
        {
            if (PL != null && PL.Player.LoginStatus == (int)LoginStatus.Loading)
            {
                return;
            }

            IEnumerable<Process> pol = Process.GetProcessesByName("pol").Union(Process.GetProcessesByName("xiloader")).Union(Process.GetProcessesByName("edenxi"));

            if (pol.Count() >= 1)
            {
                POLID.Items.Clear();
                POLID2.Items.Clear();
                processids.Items.Clear();

                int selectedPOLID = 0;
                int selectedPOLID2 = 0;

                for (int i = 0; i < pol.Count(); i++)
                {
                    POLID.Items.Add(pol.ElementAt(i).MainWindowTitle);
                    POLID2.Items.Add(pol.ElementAt(i).MainWindowTitle);
                    processids.Items.Add(pol.ElementAt(i).Id);

                    if (PL != null && PL.Player.Name != null)
                    {
                        if (pol.ElementAt(i).MainWindowTitle.ToLower() == PL.Player.Name.ToLower())
                        {
                            selectedPOLID = i;
                            plLabel.Text = "Selected PL: " + PL.Player.Name;
                            Text = notifyIcon1.Text = PL.Player.Name + " - " + "Cure Please v" + Application.ProductVersion;
                        }
                    }

                    if (Monitored != null && Monitored.Player.Name != null && Monitored.Player.LoginStatus == (int)LoginStatus.Loading)
                    {
                        if (pol.ElementAt(i).MainWindowTitle == Monitored.Player.Name)
                        {
                            selectedPOLID2 = i;
                            monitoredLabel.Text = "Monitored Player: " + Monitored.Player.Name;
                        }
                    }
                }

                POLID.SelectedIndex = selectedPOLID;
                POLID2.SelectedIndex = selectedPOLID2;
            }
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            if (FormWindowState.Minimized == WindowState)
            {
                notifyIcon1.Visible = true;
                notifyIcon1.ShowBalloonTip(500);
                Hide();
            }
        }

        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            Show();
            WindowState = FormWindowState.Normal;
        }

        private void StopBot()
        {
            pauseActions = true;
            actionTimer.Enabled = false;
            ActiveBuffs.Clear();
            _FollowEngine.Stop();
        }

        private void CheckCustomActions_TickAsync(object sender, EventArgs e)
        {
            if (PL != null && Monitored != null)
            {
                int cmdTime = Monitored.ThirdParty.ConsoleIsNewCommand();

                if (lastCommand != cmdTime)
                {
                    lastCommand = cmdTime;

                    if (Monitored.ThirdParty.ConsoleGetArg(0) == "cureplease")
                    {
                        int argCount = Monitored.ThirdParty.ConsoleGetArgCount();

                        // 0 = cureplease or cp so ignore
                        // 1 = command to run
                        // 2 = (if set) PL's name

                        if (argCount >= 3)
                        {
                            if ((Monitored.ThirdParty.ConsoleGetArg(1) == "stop" || Monitored.ThirdParty.ConsoleGetArg(1) == "pause") && PL.Player.Name == Monitored.ThirdParty.ConsoleGetArg(2))
                            {
                                pauseButton.Text = "Paused!";
                                pauseButton.ForeColor = Color.Red;
                                StopBot();
                            }
                            else if ((Monitored.ThirdParty.ConsoleGetArg(1) == "unpause" || Monitored.ThirdParty.ConsoleGetArg(1) == "start") && PL.Player.Name.ToLower() == Monitored.ThirdParty.ConsoleGetArg(2).ToLower())
                            {
                                pauseButton.Text = "Pause";
                                pauseButton.ForeColor = Color.Black;
                                actionTimer.Enabled = true;
                                pauseActions = false;
                                _FollowEngine.Start();
                            }
                            else if ((Monitored.ThirdParty.ConsoleGetArg(1) == "toggle") && PL.Player.Name.ToLower() == Monitored.ThirdParty.ConsoleGetArg(2).ToLower())
                            {
                                pauseButton.PerformClick();
                            }
                        }
                        else if (argCount < 3)
                        {
                            if (Monitored.ThirdParty.ConsoleGetArg(1) == "stop" || Monitored.ThirdParty.ConsoleGetArg(1) == "pause")
                            {
                                pauseButton.Text = "Paused!";
                                pauseButton.ForeColor = Color.Red;
                                StopBot();
                            }
                            else if (Monitored.ThirdParty.ConsoleGetArg(1) == "unpause" || Monitored.ThirdParty.ConsoleGetArg(1) == "start")
                            {
                                pauseButton.Text = "Pause";
                                pauseButton.ForeColor = Color.Black;
                                actionTimer.Enabled = true;
                                pauseActions = false;
                                _FollowEngine.Start();
                            }
                            else if (Monitored.ThirdParty.ConsoleGetArg(1) == "toggle")
                            {
                                pauseButton.PerformClick();
                            }
                        }
                    }
                }
            }
        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            Opacity = trackBar1.Value * 0.01;
        }

        private Form settings;

        private void OptionsButton_Click(object sender, EventArgs e)
        {
            if ((settings == null) || settings.IsDisposed)
            {
                settings = new ConfigForm();
            }
            settings.Show();

        }

        private void ChatLogButton_Click(object sender, EventArgs e)
        {
            ChatlogForm form4 = new ChatlogForm(this);

            if (PL != null)
            {
                form4.Show();
            }
        }

        private void PartyBuffsButton_Click(object sender, EventArgs e)
        {
            PartyBuffs PartyBuffs = new PartyBuffs(this);
            if (PL != null)
            {
                PartyBuffs.Show();
            }
        }

        private void AboutButton_Click(object sender, EventArgs e)
        {
            new AboutForm().Show();
        }

        private void OnAddonDataReceived(IAsyncResult result)
        {
            if (!ConfigForm.config.EnableAddOn || PL == null)
                return;

            UdpClient socket = result.AsyncState as UdpClient;

            IPEndPoint groupEP = new IPEndPoint(IPAddress.Parse(ConfigForm.config.ipAddress), Convert.ToInt32(ConfigForm.config.listeningPort));

            try
            {
                byte[] receive_byte_array = socket.EndReceive(result, ref groupEP);

                string received_data = Encoding.ASCII.GetString(receive_byte_array, 0, receive_byte_array.Length);

                string[] commands = received_data.Split('_');
         
                if (commands[1] == "casting" && commands.Count() == 3 && ConfigForm.config.trackCastingPackets == true)
                {
                    if (commands[2] == "blocked")
                    {
                        Invoke((MethodInvoker)(() =>
                        {
                            CastingBackground_Check = true;
                            castingLockLabel.Text = "PACKET: Casting is LOCKED";
                        }));

                        if (!ProtectCasting.IsBusy) { ProtectCasting.RunWorkerAsync(); }

                        pauseActions = true;
                        _FollowEngine.Stop();
                    }
                    else
                    {
                        if (commands[2] == "interrupted")
                        {
                            Invoke((MethodInvoker)(async () =>
                            {
                                ProtectCasting.CancelAsync();
                                castingLockLabel.Text = "PACKET: Casting is INTERRUPTED";
                                await Task.Delay(TimeSpan.FromSeconds(2));
                                castingLockLabel.Text = "Casting is UNLOCKED";
                                CastingBackground_Check = false;
                            }));
                        }
                        else if (commands[2] == "finished")
                        {

                            Invoke((MethodInvoker)(async () =>
                            {
                                ProtectCasting.CancelAsync();
                                castingLockLabel.Text = "PACKET: Casting is soon to be AVAILABLE!";
                                await Task.Delay(TimeSpan.FromSeconds(3));
                                castingLockLabel.Text = "Casting is UNLOCKED";
                                AddCurrentAction(string.Empty);
                                castingSpell = string.Empty;
                                CastingBackground_Check = false;
                            }));
                        }

                        pauseActions = false;
                        _FollowEngine.Start();
                    }
                }
                else if (commands[1] == "confirmed")
                {
                    AddOnStatus.BackColor = Color.ForestGreen;
                }
                else if (commands[1] == "command")
                {
                    // MessageBox.Show(commands[2]);
                    if (commands[2] == "start" || commands[2] == "unpause")
                    {
                        Invoke((MethodInvoker)(() =>
                        {
                            pauseButton.Text = "Pause";
                            pauseButton.ForeColor = Color.Black;
                            actionTimer.Enabled = true;
                            pauseActions = false;
                            _FollowEngine.Start();
                        }));
                    }
                    if (commands[2] == "stop" || commands[2] == "pause")
                    {
                        Invoke((MethodInvoker)(() =>
                        {

                            pauseButton.Text = "Paused!";
                            pauseButton.ForeColor = Color.Red;
                            StopBot();
                        }));
                    }
                    if (commands[2] == "toggle")
                    {
                        Invoke((MethodInvoker)(() =>
                        {
                            pauseButton.PerformClick();
                        }));
                    }
                }
                else if (commands[1] == "buffs" && commands.Count() == 4)
                {
                    lock (ActiveBuffs)
                    {
                        var memberName = commands[2];
                        var memberBuffs = commands[3];
                                
                        if(!string.IsNullOrEmpty(memberBuffs))
                        {
                            var buffs = memberBuffs.Split(',').Select(str => short.Parse(str.Trim())).Where(buff => !Data.DebuffPriorities.Keys.Cast<short>().Contains(buff));
                            var debuffs = memberBuffs.Split(',').Select(str => short.Parse(str.Trim())).Where(buff => Data.DebuffPriorities.Keys.Cast<short>().Contains(buff));

                            if (_BuffEngine != null)
                                _BuffEngine.UpdateBuffs(memberName, buffs);

                            if (_DebuffEngine != null)
                                _DebuffEngine.UpdateDebuffs(memberName, debuffs);
                        }                             
                    }

                }                    
            }
            catch (Exception error1)
            {
                Console.WriteLine(error1.StackTrace);
            }

            try
            {
                if (socket.Client != null)
                    socket.BeginReceive(new AsyncCallback(OnAddonDataReceived), socket);
            }
            catch (ObjectDisposedException) 
            { 
                // haven't found a way to check if a socket is disposed yet and this will happen if you attach twice. Just ignoring.
            }
        }   

        private void AddOnStatus_Click(object sender, EventArgs e)
        {
            if (PL != null)
            {
                if (WindowerMode == "Ashita")
                {
                    PL.ThirdParty.SendString(string.Format("/cpaddon verify"));
                }
                else if (WindowerMode == "Windower")
                {
                    PL.ThirdParty.SendString(string.Format("//cpaddon verify"));
                }
            }
        }

        // This will get cancelled if we get an interrupted/finished casting packet.
        private void ProtectCasting_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            Thread.Sleep(TimeSpan.FromSeconds(1));
            int count = 0;
            float lastPercent = 0;
            float castPercent = PL.CastBar.Percent;
            while (castPercent < 1)
            {
                Thread.Sleep(TimeSpan.FromSeconds(0.1));
                castPercent = PL.CastBar.Percent;
                if (lastPercent != castPercent)
                {
                    count = 0;
                    lastPercent = castPercent;
                }
                else if (count == 10)
                {
                    // We break if we don't get a new percent value within 5*loopTimeout
                    // As configured now, 1 second (0.1 * 10).
                    break;
                }
                else
                {
                    count++;
                    lastPercent = castPercent;
                }
            }

            Thread.Sleep(TimeSpan.FromSeconds(2));

            castingSpell = string.Empty;

            castingLockLabel.Invoke(new Action(() => { castingLockLabel.Text = "Casting is UNLOCKED"; }));
            castingSpell = string.Empty;

            CastingBackground_Check = false;
        }


        private void CustomCommand_Tracker_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {

        }

        private void CustomCommand_Tracker_RunWorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            CustomCommand_Tracker.RunWorkerAsync();
        }

        private void actionlog_box_TextChanged(object sender, EventArgs e)
        {
            actionlog_box.SelectionStart = actionlog_box.Text.Length;
            actionlog_box.ScrollToCaret();
        }
    }

    // END OF THE FORM SCRIPT

    public static class RichTextBoxExtensions
    {
        public static void AppendText(this RichTextBox box, string text, Color color)
        {
            box.SelectionStart = box.TextLength;
            box.SelectionLength = 0;

            box.SelectionColor = color;
            box.AppendText(text);
            box.SelectionColor = box.ForeColor;
        }
    }
}