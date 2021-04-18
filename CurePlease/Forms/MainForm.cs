namespace CurePlease
{
    using CurePlease.Engine;
    using CurePlease.Model;
    using CurePlease.Model.Constants;
    using CurePlease.Model.Enums;
    using CurePlease.Properties;
    using CurePlease.Utilities;
    using EliteMMO.API;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Drawing;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Net.Sockets;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows.Forms;
    using System.Xml.Serialization;
    using static EliteMMO.API.EliteAPI;

    public partial class MainForm : Form
    {

        private static ConfigForm Form2 = new ConfigForm();

        private string debug_MSG_show = string.Empty;

        private int lastCommand = 0;

        private int lastKnownEstablisherTarget = 0;       

        public bool CastingBackground_Check = false;
        public bool JobAbilityLock_Check = false;

        public string JobAbilityCMD = string.Empty;

        private DateTime DefaultTime = new DateTime(1970, 1, 1);

        private bool curePlease_autofollow = false;

        private List<string> characterNames_naRemoval = new List<string>();    

        public string WindowerMode = "Windower";

        private int GetInventoryItemCount(EliteAPI api, ushort itemid)
        {
            int count = 0;
            for (int x = 0; x <= 80; x++)
            {
                InventoryItem item = api.Inventory.GetContainerItem(0, x);
                if (item != null && item.Id == itemid)
                {
                    count += (int)item.Count;
                }
            }

            return count;
        }

        private int GetTempItemCount(EliteAPI api, ushort itemid)
        {
            int count = 0;
            for (int x = 0; x <= 80; x++)
            {
                InventoryItem item = api.Inventory.GetContainerItem(3, x);
                if (item != null && item.Id == itemid)
                {
                    count += (int)item.Count;
                }
            }

            return count;
        }

        private ushort GetItemId(string name)
        {
            IItem item = PL.Resources.GetItem(name, 0);
            return item != null ? (ushort)item.ItemID : (ushort)0;
        }

        public static EliteAPI PL;

        public static EliteAPI Monitored;

        public ListBox processids = new ListBox();

        public ListBox activeprocessids = new ListBox();

        public SongEngine SongEngine = new SongEngine(PL, Monitored, Form2.GetSongConfig());

        public GeoEngine GeoEngine = new GeoEngine(PL, Monitored, Form2.GetGeoConfig());

        public BuffEngine BuffEngine = new BuffEngine(PL, Monitored, Form2.GetBuffConfig());

        public double last_percent = 1;

        public string castingSpell = string.Empty;

        public int max_count = 10;
        public int spell_delay_count = 0;

        public int geo_step = 0;

        public int followWarning = 0;

        public bool stuckWarning = false;
        public int stuckCount = 0;

        public int protectionCount = 0;

        public float lastZ;
        public float lastX;
        public float lastY;

        // Stores the previously-colored button, if any
        public Dictionary<string, IEnumerable<short>> ActiveBuffs = new Dictionary<string, IEnumerable<short>>();     

        public List<string> TemporaryItem_Zones = new List<string> { "Escha Ru'Aun", "Escha Zi'Tah", "Reisenjima", "Abyssea - La Theine", "Abyssea - Konschtat", "Abyssea - Tahrongi",
                                                                        "Abyssea - Attohwa", "Abyssea - Misareaux", "Abyssea - Vunkerl", "Abyssea - Altepa", "Abyssea - Uleguerand", "Abyssea - Grauberg", "Walk of Echoes" };

        public string wakeSleepSpell = Spells.Cure;

        public string plSilenceitemName = "Echo Drops";

        public string plDoomItemName = "Holy Water";

        private float plX;

        private float plY;

        private float plZ;

        private byte playerOptionsSelected;

        private byte autoOptionsSelected;

        private bool pauseActions;

        private bool islowmp;

        public int LUA_Plugin_Loaded = 0;

        public int firstTime_Pause = 0;          

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

            button.FlatAppearance.BorderColor = System.Drawing.Color.Gray;
        }


        public MainForm()
        {


            StartPosition = FormStartPosition.CenterScreen;

            InitializeComponent();

            currentAction.Text = string.Empty;

            if (System.IO.File.Exists("debug"))
            {
                debug.Visible = true;
            }              

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
                    activeprocessids.Items.Add(pol.ElementAt(i).Id);
                }
                POLID.SelectedIndex = 0;
                POLID2.SelectedIndex = 0;
                processids.SelectedIndex = 0;
                activeprocessids.SelectedIndex = 0;
            }
            // Show the current version number..
            Text = notifyIcon1.Text = "Cure Please v" + Application.ProductVersion;

            notifyIcon1.BalloonTipTitle = "Cure Please v" + Application.ProductVersion;
            notifyIcon1.BalloonTipText = "CurePlease has been minimized.";
            notifyIcon1.BalloonTipIcon = ToolTipIcon.Info;
        }

        private void setinstance_Click(object sender, EventArgs e)
        {
            if (!CheckForDLLFiles())
            {
                MessageBox.Show(
                    "Unable to locate EliteAPI.dll or EliteMMO.API.dll\nMake sure both files are in the same directory as the application",
                    "Error");
                return;
            }

            processids.SelectedIndex = POLID.SelectedIndex;
            activeprocessids.SelectedIndex = POLID.SelectedIndex;
            PL = new EliteAPI((int)processids.SelectedItem);
            plLabel.Text = "Selected PL: " + PL.Player.Name;
            Text = notifyIcon1.Text = PL.Player.Name + " - " + "Cure Please v" + Application.ProductVersion;

            plLabel.ForeColor = Color.Green;
            POLID.BackColor = Color.White;
            plPosition.Enabled = true;
            setinstance2.Enabled = true;
            ConfigForm.config.autoFollowName = string.Empty;

            foreach (Process dats in Process.GetProcessesByName("pol").Union(Process.GetProcessesByName("xiloader")).Union(Process.GetProcessesByName("edenxi")).Where(dats => POLID.Text == dats.MainWindowTitle))
            {
                for (int i = 0; i < dats.Modules.Count; i++)
                {
                    if (dats.Modules[i].FileName.Contains("Ashita.dll"))
                    {
                        WindowerMode = "Ashita";
                    }
                    else if (dats.Modules[i].FileName.Contains("Hook.dll"))
                    {
                        WindowerMode = "Windower";
                    }
                }
            }

            if (firstTime_Pause == 0)
            {
                Follow_BGW.RunWorkerAsync();
                AddonReader.RunWorkerAsync();
                firstTime_Pause = 1;
            }

            // LOAD AUTOMATIC SETTINGS
            string path = System.IO.Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "Settings");
            if (System.IO.File.Exists(path + "/loadSettings"))
            {
                if (PL.Player.MainJob != 0)
                {
                    if (PL.Player.SubJob != 0)
                    {
                        Job mainJob = (Job)PL.Player.MainJob;
                        Job subJob = (Job)PL.Player.SubJob;

                        string filename = path + "\\" + PL.Player.Name + "_" + mainJob.ToString() + "_" + subJob.ToString() + ".xml";
                        string filename2 = path + "\\" + mainJob.ToString() + "_" + subJob.ToString() + ".xml";


                        if (System.IO.File.Exists(filename))
                        {
                            ConfigForm.MySettings config = new ConfigForm.MySettings();

                            XmlSerializer mySerializer = new XmlSerializer(typeof(ConfigForm.MySettings));

                            StreamReader reader = new StreamReader(filename);
                            config = (ConfigForm.MySettings)mySerializer.Deserialize(reader);

                            reader.Close();
                            reader.Dispose();
                            Form2.updateForm(config);
                            Form2.button4_Click(sender, e);
                        }
                        else if (System.IO.File.Exists(filename2))
                        {
                            ConfigForm.MySettings config = new ConfigForm.MySettings();

                            XmlSerializer mySerializer = new XmlSerializer(typeof(ConfigForm.MySettings));

                            StreamReader reader = new StreamReader(filename2);
                            config = (ConfigForm.MySettings)mySerializer.Deserialize(reader);

                            reader.Close();
                            reader.Dispose();
                            Form2.updateForm(config);
                            Form2.button4_Click(sender, e);
                        }
                    }
                }
            }

            if (LUA_Plugin_Loaded == 0 && !ConfigForm.config.pauseOnStartBox && Monitored != null)
            {
                // Wait a milisecond and then load and set the config.
                Thread.Sleep(500);

                if (WindowerMode == "Windower")
                {
                    PL.ThirdParty.SendString("//lua load CurePlease_addon");
                    Thread.Sleep(1500);
                    PL.ThirdParty.SendString("//cpaddon settings " + ConfigForm.config.ipAddress + " " + ConfigForm.config.listeningPort);
                    Thread.Sleep(100);
                    PL.ThirdParty.SendString("//cpaddon verify");
                    if (ConfigForm.config.enableHotKeys)
                    {
                        PL.ThirdParty.SendString("//bind ^!F1 cureplease toggle");
                        PL.ThirdParty.SendString("//bind ^!F2 cureplease start");
                        PL.ThirdParty.SendString("//bind ^!F3 cureplease pause");
                    }
                }
                else if (WindowerMode == "Ashita")
                {
                    PL.ThirdParty.SendString("/addon load CurePlease_addon");
                    Thread.Sleep(1500);
                    PL.ThirdParty.SendString("/cpaddon settings " + ConfigForm.config.ipAddress + " " + ConfigForm.config.listeningPort);
                    Thread.Sleep(100);

                    PL.ThirdParty.SendString("/cpaddon verify");
                    if (ConfigForm.config.enableHotKeys)
                    {
                        PL.ThirdParty.SendString("/bind ^!F1 /cureplease toggle");
                        PL.ThirdParty.SendString("/bind ^!F2 /cureplease start");
                        PL.ThirdParty.SendString("/bind ^!F3 /cureplease pause");
                    }
                }

                AddOnStatus_Click(sender, e);


                currentAction.Text = "LUA Addon loaded. ( " + ConfigForm.config.ipAddress + " - " + ConfigForm.config.listeningPort + " )";

                LUA_Plugin_Loaded = 1;
            }
        }

        private void setinstance2_Click(object sender, EventArgs e)
        {
            if (!CheckForDLLFiles())
            {
                MessageBox.Show(
                    "Unable to locate EliteAPI.dll or EliteMMO.API.dll\nMake sure both files are in the same directory as the application",
                    "Error");
                return;
            }
            processids.SelectedIndex = POLID2.SelectedIndex;
            Monitored = new EliteAPI((int)processids.SelectedItem);
            monitoredLabel.Text = "Monitoring: " + Monitored.Player.Name;
            monitoredLabel.ForeColor = Color.Green;
            POLID2.BackColor = Color.White;
            partyMembersUpdate.Enabled = true;
            actionTimer.Enabled = true;
            pauseButton.Enabled = true;
            hpUpdates.Enabled = true;

            if (ConfigForm.config.pauseOnStartBox)
            {
                pauseActions = true;
                pauseButton.Text = "Loaded, Paused!";
                pauseButton.ForeColor = Color.Red;
                actionTimer.Enabled = false;
            }
            else
            {
                if (ConfigForm.config.MinimiseonStart == true && WindowState != FormWindowState.Minimized)
                {
                    WindowState = FormWindowState.Minimized;
                }
            }

            if (LUA_Plugin_Loaded == 0 && !ConfigForm.config.pauseOnStartBox && PL != null)
            {
                // Wait a milisecond and then load and set the config.
                Thread.Sleep(500);
                if (WindowerMode == "Windower")
                {
                    PL.ThirdParty.SendString("//lua load CurePlease_addon");
                    Thread.Sleep(1500);
                    PL.ThirdParty.SendString("//cpaddon settings " + ConfigForm.config.ipAddress + " " + ConfigForm.config.listeningPort);
                    Thread.Sleep(100);
                    PL.ThirdParty.SendString("//cpaddon verify");

                    if (ConfigForm.config.enableHotKeys)
                    {
                        PL.ThirdParty.SendString("//bind ^!F1 cureplease toggle");
                        PL.ThirdParty.SendString("//bind ^!F2 cureplease start");
                        PL.ThirdParty.SendString("//bind ^!F3 cureplease pause");
                    }
                }
                else if (WindowerMode == "Ashita")
                {
                    PL.ThirdParty.SendString("/addon load CurePlease_addon");
                    Thread.Sleep(1500);
                    PL.ThirdParty.SendString("/cpaddon settings " + ConfigForm.config.ipAddress + " " + ConfigForm.config.listeningPort);
                    Thread.Sleep(100);
                    PL.ThirdParty.SendString("/cpaddon verify");
                    if (ConfigForm.config.enableHotKeys)
                    {
                        PL.ThirdParty.SendString("/bind ^!F1 /cureplease toggle");
                        PL.ThirdParty.SendString("/bind ^!F2 /cureplease start");
                        PL.ThirdParty.SendString("/bind ^!F3 /cureplease pause");
                    }
                }

                currentAction.Text = "LUA Addon loaded. ( " + ConfigForm.config.ipAddress + " - " + ConfigForm.config.listeningPort + " )";

                LUA_Plugin_Loaded = 1;

                AddOnStatus_Click(sender, e);

                lastCommand = Monitored.ThirdParty.ConsoleIsNewCommand();
            }
        }

        private bool CheckForDLLFiles()
        {
            if (!File.Exists("eliteapi.dll") || !File.Exists("elitemmo.api.dll"))
            {
                return false;
            }
            return true;
        }

        private string PickCureTier(string cureSpell, string[] tierList)
        {
            int spellIndex = Array.IndexOf(tierList, cureSpell);

            string overSpell = Spells.Unknown;
            string underSpell = Spells.Unknown;

            // This will end up with a situation where Cure + Cure II on cooldown results in the "Undercure"
            // solution being Cure III. But I think it might not be possible to cast both fast enough
            // to make that a concern?
            if(cureSpell == tierList.Last())
            {
                overSpell = tierList[tierList.Length-2];
                underSpell = tierList[tierList.Length-3];
            }
            else if(cureSpell == tierList.First())
            {
                overSpell = tierList[1];
                underSpell = tierList[2];
            }
            else
            {
                overSpell = tierList[spellIndex + 1];
                underSpell = tierList[spellIndex - 1];
            }

            if(PL.SpellAvailable(cureSpell) && PL.HasMPFor(cureSpell))
            {
                return cureSpell;
            }
            else if(ConfigForm.config.Overcure && PL.SpellAvailable(overSpell) && PL.HasMPFor(overSpell))
            {
                return overSpell;
            }
            else if(ConfigForm.config.Undercure && PL.SpellAvailable(underSpell) && PL.HasMPFor(underSpell))
            {
                return underSpell;
            }

            return Spells.Unknown;
        }

        private bool partyMemberUpdateMethod(byte partyMemberId)
        {
            if (Monitored.Party.GetPartyMembers()[partyMemberId].Active >= 1)
            {
                if (PL.Player.ZoneId == Monitored.Party.GetPartyMembers()[partyMemberId].Zone)
                {
                    return true;
                }

                return false;
            }
            return false;
        }

        private async void partyMembersUpdate_TickAsync(object sender, EventArgs e)
        {
            if (PL == null || Monitored == null)
            {
                return;
            }

            if (PL.Player.LoginStatus == (int)EliteMMO.API.LoginStatus.Loading || Monitored.Player.LoginStatus == (int)LoginStatus.Loading)
            {
                if (ConfigForm.config.pauseOnZoneBox == true)
                {
                    if (pauseActions != true)
                    {
                        pauseButton.Text = "Zoned, paused.";
                        pauseButton.ForeColor = Color.Red;
                        pauseActions = true;
                        actionTimer.Enabled = false;
                    }
                }
                else
                {
                    if (pauseActions != true)
                    {
                        pauseButton.Text = "Zoned, waiting.";
                        pauseButton.ForeColor = Color.Red;
                        await Task.Delay(100);
                        Thread.Sleep(17000);
                        pauseButton.Text = "Pause";
                        pauseButton.ForeColor = Color.Black;
                    }
                }
                ActiveBuffs.Clear();
            }

            if (PL.Player.LoginStatus != (int)LoginStatus.LoggedIn || Monitored.Player.LoginStatus != (int)LoginStatus.LoggedIn)
            {
                return;
            }
            if (partyMemberUpdateMethod(0))
            {
                player0.Text = Monitored.Party.GetPartyMember(0).Name;
                player0.Enabled = true;
                player0optionsButton.Enabled = true;
                player0buffsButton.Enabled = true;
            }
            else
            {
                player0.Text = "Inactive or out of zone";
                player0.Enabled = false;
                player0HP.Value = 0;
                player0optionsButton.Enabled = false;
                player0buffsButton.Enabled = false;
            }

            if (partyMemberUpdateMethod(1))
            {
                player1.Text = Monitored.Party.GetPartyMember(1).Name;
                player1.Enabled = true;
                player1optionsButton.Enabled = true;
                player1buffsButton.Enabled = true;
            }
            else
            {
                player1.Text = Resources.Form1_partyMembersUpdate_Tick_Inactive;
                player1.Enabled = false;
                player1HP.Value = 0;
                player1optionsButton.Enabled = false;
                player1buffsButton.Enabled = false;
            }

            if (partyMemberUpdateMethod(2))
            {
                player2.Text = Monitored.Party.GetPartyMember(2).Name;
                player2.Enabled = true;
                player2optionsButton.Enabled = true;
                player2buffsButton.Enabled = true;
            }
            else
            {
                player2.Text = Resources.Form1_partyMembersUpdate_Tick_Inactive;
                player2.Enabled = false;
                player2HP.Value = 0;
                player2optionsButton.Enabled = false;
                player2buffsButton.Enabled = false;
            }

            if (partyMemberUpdateMethod(3))
            {
                player3.Text = Monitored.Party.GetPartyMember(3).Name;
                player3.Enabled = true;
                player3optionsButton.Enabled = true;
                player3buffsButton.Enabled = true;
            }
            else
            {
                player3.Text = Resources.Form1_partyMembersUpdate_Tick_Inactive;
                player3.Enabled = false;
                player3HP.Value = 0;
                player3optionsButton.Enabled = false;
                player3buffsButton.Enabled = false;
            }

            if (partyMemberUpdateMethod(4))
            {
                player4.Text = Monitored.Party.GetPartyMember(4).Name;
                player4.Enabled = true;
                player4optionsButton.Enabled = true;
                player4buffsButton.Enabled = true;
            }
            else
            {
                player4.Text = Resources.Form1_partyMembersUpdate_Tick_Inactive;
                player4.Enabled = false;
                player4HP.Value = 0;
                player4optionsButton.Enabled = false;
                player4buffsButton.Enabled = false;
            }

            if (partyMemberUpdateMethod(5))
            {
                player5.Text = Monitored.Party.GetPartyMember(5).Name;
                player5.Enabled = true;
                player5optionsButton.Enabled = true;
                player5buffsButton.Enabled = true;
            }
            else
            {
                player5.Text = Resources.Form1_partyMembersUpdate_Tick_Inactive;
                player5.Enabled = false;
                player5HP.Value = 0;
                player5optionsButton.Enabled = false;
                player5buffsButton.Enabled = false;
            }
            if (partyMemberUpdateMethod(6))
            {
                player6.Text = Monitored.Party.GetPartyMember(6).Name;
                player6.Enabled = true;
                player6optionsButton.Enabled = true;
            }
            else
            {
                player6.Text = Resources.Form1_partyMembersUpdate_Tick_Inactive;
                player6.Enabled = false;
                player6HP.Value = 0;
                player6optionsButton.Enabled = false;
            }

            if (partyMemberUpdateMethod(7))
            {
                player7.Text = Monitored.Party.GetPartyMember(7).Name;
                player7.Enabled = true;
                player7optionsButton.Enabled = true;
            }
            else
            {
                player7.Text = Resources.Form1_partyMembersUpdate_Tick_Inactive;
                player7.Enabled = false;
                player7HP.Value = 0;
                player7optionsButton.Enabled = false;
            }

            if (partyMemberUpdateMethod(8))
            {
                player8.Text = Monitored.Party.GetPartyMember(8).Name;
                player8.Enabled = true;
                player8optionsButton.Enabled = true;
            }
            else
            {
                player8.Text = Resources.Form1_partyMembersUpdate_Tick_Inactive;
                player8.Enabled = false;
                player8HP.Value = 0;
                player8optionsButton.Enabled = false;
            }

            if (partyMemberUpdateMethod(9))
            {
                player9.Text = Monitored.Party.GetPartyMember(9).Name;
                player9.Enabled = true;
                player9optionsButton.Enabled = true;
            }
            else
            {
                player9.Text = Resources.Form1_partyMembersUpdate_Tick_Inactive;
                player9.Enabled = false;
                player9HP.Value = 0;
                player9optionsButton.Enabled = false;
            }

            if (partyMemberUpdateMethod(10))
            {
                player10.Text = Monitored.Party.GetPartyMember(10).Name;
                player10.Enabled = true;
                player10optionsButton.Enabled = true;
            }
            else
            {
                player10.Text = Resources.Form1_partyMembersUpdate_Tick_Inactive;
                player10.Enabled = false;
                player10HP.Value = 0;
                player10optionsButton.Enabled = false;
            }

            if (partyMemberUpdateMethod(11))
            {
                player11.Text = Monitored.Party.GetPartyMember(11).Name;
                player11.Enabled = true;
                player11optionsButton.Enabled = true;
            }
            else
            {
                player11.Text = Resources.Form1_partyMembersUpdate_Tick_Inactive;
                player11.Enabled = false;
                player11HP.Value = 0;
                player11optionsButton.Enabled = false;
            }

            if (partyMemberUpdateMethod(12))
            {
                player12.Text = Monitored.Party.GetPartyMember(12).Name;
                player12.Enabled = true;
                player12optionsButton.Enabled = true;
            }
            else
            {
                player12.Text = Resources.Form1_partyMembersUpdate_Tick_Inactive;
                player12.Enabled = false;
                player12HP.Value = 0;
                player12optionsButton.Enabled = false;
            }

            if (partyMemberUpdateMethod(13))
            {
                player13.Text = Monitored.Party.GetPartyMember(13).Name;
                player13.Enabled = true;
                player13optionsButton.Enabled = true;
            }
            else
            {
                player13.Text = Resources.Form1_partyMembersUpdate_Tick_Inactive;
                player13.Enabled = false;
                player13HP.Value = 0;
                player13optionsButton.Enabled = false;
            }

            if (partyMemberUpdateMethod(14))
            {
                player14.Text = Monitored.Party.GetPartyMember(14).Name;
                player14.Enabled = true;
                player14optionsButton.Enabled = true;
            }
            else
            {
                player14.Text = Resources.Form1_partyMembersUpdate_Tick_Inactive;
                player14.Enabled = false;
                player14HP.Value = 0;
                player14optionsButton.Enabled = false;
            }

            if (partyMemberUpdateMethod(15))
            {
                player15.Text = Monitored.Party.GetPartyMember(15).Name;
                player15.Enabled = true;
                player15optionsButton.Enabled = true;
            }
            else
            {
                player15.Text = Resources.Form1_partyMembersUpdate_Tick_Inactive;
                player15.Enabled = false;
                player15HP.Value = 0;
                player15optionsButton.Enabled = false;
            }

            if (partyMemberUpdateMethod(16))
            {
                player16.Text = Monitored.Party.GetPartyMember(16).Name;
                player16.Enabled = true;
                player16optionsButton.Enabled = true;
            }
            else
            {
                player16.Text = Resources.Form1_partyMembersUpdate_Tick_Inactive;
                player16.Enabled = false;
                player16HP.Value = 0;
                player16optionsButton.Enabled = false;
            }

            if (partyMemberUpdateMethod(17))
            {
                player17.Text = Monitored.Party.GetPartyMember(17).Name;
                player17.Enabled = true;
                player17optionsButton.Enabled = true;
            }
            else
            {
                player17.Text = Resources.Form1_partyMembersUpdate_Tick_Inactive;
                player17.Enabled = false;
                player17HP.Value = 0;
                player17optionsButton.Enabled = false;
            }
        }

        private void hpUpdates_Tick(object sender, EventArgs e)
        {
            if (PL == null || Monitored == null)
            {
                return;
            }

            if (PL.Player.LoginStatus != (int)LoginStatus.LoggedIn || Monitored.Player.LoginStatus != (int)LoginStatus.LoggedIn)
            {
                return;
            }

            if (player0.Enabled)
            {
                UpdateHPProgressBar(player0HP, Monitored.Party.GetPartyMember(0).CurrentHPP);
            }

            if (player0.Enabled)
            {
                UpdateHPProgressBar(player0HP, Monitored.Party.GetPartyMember(0).CurrentHPP);
            }

            if (player1.Enabled)
            {
                UpdateHPProgressBar(player1HP, Monitored.Party.GetPartyMember(1).CurrentHPP);
            }

            if (player2.Enabled)
            {
                UpdateHPProgressBar(player2HP, Monitored.Party.GetPartyMember(2).CurrentHPP);
            }

            if (player3.Enabled)
            {
                UpdateHPProgressBar(player3HP, Monitored.Party.GetPartyMember(3).CurrentHPP);
            }

            if (player4.Enabled)
            {
                UpdateHPProgressBar(player4HP, Monitored.Party.GetPartyMember(4).CurrentHPP);
            }

            if (player5.Enabled)
            {
                UpdateHPProgressBar(player5HP, Monitored.Party.GetPartyMember(5).CurrentHPP);
            }

            if (player6.Enabled)
            {
                UpdateHPProgressBar(player6HP, Monitored.Party.GetPartyMember(6).CurrentHPP);
            }

            if (player7.Enabled)
            {
                UpdateHPProgressBar(player7HP, Monitored.Party.GetPartyMember(7).CurrentHPP);
            }

            if (player8.Enabled)
            {
                UpdateHPProgressBar(player8HP, Monitored.Party.GetPartyMember(8).CurrentHPP);
            }

            if (player9.Enabled)
            {
                UpdateHPProgressBar(player9HP, Monitored.Party.GetPartyMember(9).CurrentHPP);
            }

            if (player10.Enabled)
            {
                UpdateHPProgressBar(player10HP, Monitored.Party.GetPartyMember(10).CurrentHPP);
            }

            if (player11.Enabled)
            {
                UpdateHPProgressBar(player11HP, Monitored.Party.GetPartyMember(11).CurrentHPP);
            }

            if (player12.Enabled)
            {
                UpdateHPProgressBar(player12HP, Monitored.Party.GetPartyMember(12).CurrentHPP);
            }

            if (player13.Enabled)
            {
                UpdateHPProgressBar(player13HP, Monitored.Party.GetPartyMember(13).CurrentHPP);
            }

            if (player14.Enabled)
            {
                UpdateHPProgressBar(player14HP, Monitored.Party.GetPartyMember(14).CurrentHPP);
            }

            if (player15.Enabled)
            {
                UpdateHPProgressBar(player15HP, Monitored.Party.GetPartyMember(15).CurrentHPP);
            }

            if (player16.Enabled)
            {
                UpdateHPProgressBar(player16HP, Monitored.Party.GetPartyMember(16).CurrentHPP);
            }

            if (player17.Enabled)
            {
                UpdateHPProgressBar(player17HP, Monitored.Party.GetPartyMember(17).CurrentHPP);
            }
        }

        private void UpdateHPProgressBar(ProgressBar playerHP, int CurrentHPP)
        {
            playerHP.Value = CurrentHPP;
            if (CurrentHPP >= 75)
            {
                playerHP.ForeColor = Color.DarkGreen;
            }
            else if (CurrentHPP > 50 && CurrentHPP < 75)
            {
                playerHP.ForeColor = Color.Yellow;
            }
            else if (CurrentHPP > 25 && CurrentHPP < 50)
            {
                playerHP.ForeColor = Color.Orange;
            }
            else if (CurrentHPP < 25)
            {
                playerHP.ForeColor = Color.Red;
            }
        }

        private void plPosition_Tick(object sender, EventArgs e)
        {
            if (PL == null || Monitored == null)
            {
                return;
            }

            if (PL.Player.LoginStatus != (int)LoginStatus.LoggedIn || Monitored.Player.LoginStatus != (int)LoginStatus.LoggedIn)
            {
                return;
            }

            plX = PL.Player.X;
            plY = PL.Player.Y;
            plZ = PL.Player.Z;
        }

        private string PickCure(uint hpLoss)
        {
            if (ConfigForm.config.cure6enabled && hpLoss >= ConfigForm.config.cure6amount && PL.HasMPFor(Spells.Cure_VI))
            {
                return PickCureTier(Spells.Cure_VI, Data.CureTiers);
            }
            else if (ConfigForm.config.cure5enabled && hpLoss >= ConfigForm.config.cure5amount && PL.HasMPFor(Spells.Cure_V))
            {
                return PickCureTier(Spells.Cure_V, Data.CureTiers);
            }
            else if (ConfigForm.config.cure4enabled && hpLoss >= ConfigForm.config.cure4amount && PL.HasMPFor(Spells.Cure_IV))
            {
                return PickCureTier(Spells.Cure_IV, Data.CureTiers);
            }
            else if (ConfigForm.config.cure3enabled && hpLoss >= ConfigForm.config.cure3amount && PL.HasMPFor(Spells.Cure_III))
            {
                return PickCureTier(Spells.Cure_III, Data.CureTiers);
            }
            else if (ConfigForm.config.cure2enabled && hpLoss >= ConfigForm.config.cure2amount && PL.HasMPFor(Spells.Cure_II))
            {
                return PickCureTier(Spells.Cure_II, Data.CureTiers);
            }
            else if (ConfigForm.config.cure1enabled && hpLoss >= ConfigForm.config.cure1amount && PL.HasMPFor(Spells.Cure))
            {
                return PickCureTier(Spells.Cure, Data.CureTiers);
            }

            return Spells.Unknown;
        }    

        private void CureCalculator(PartyMember partyMember)
        {
            // Only do this is party member is alive.
            if (partyMember.CurrentHP > 0)
            {
                string cureSpell = PickCure(partyMember.HPLoss());                       

                if (cureSpell != Spells.Unknown)
                {
                    // I consider cure/cure II to be low tier once cure III gets above 700 HP.
                    if (Array.IndexOf(Data.CureTiers, cureSpell) < 2 && ConfigForm.config.PrioritiseOverLowerTier == true)
                    {
                        RunDebuffChecker();
                    }

                    CastSpell(partyMember.Name, cureSpell);
                }
            }
        }

        private void RunDebuffChecker()
        {
            if (ConfigForm.config.plSilenceItem == 0)
            {
                plSilenceitemName = "Catholicon";
            }
            else if (ConfigForm.config.plSilenceItem == 1)
            {
                plSilenceitemName = "Echo Drops";
            }
            else if (ConfigForm.config.plSilenceItem == 2)
            {
                plSilenceitemName = "Remedy";
            }
            else if (ConfigForm.config.plSilenceItem == 3)
            {
                plSilenceitemName = "Remedy Ointment";
            }
            else if (ConfigForm.config.plSilenceItem == 4)
            {
                plSilenceitemName = "Vicar's Drink";
            }

            if (ConfigForm.config.plDoomitem == 0)
            {
                plDoomItemName = "Holy Water";
            }
            else if (ConfigForm.config.plDoomitem == 1)
            {
                plDoomItemName = "Hallowed Water";
            }

            if (ConfigForm.config.wakeSleepSpell == 0)
            {
                wakeSleepSpell = Spells.Cure;
            }
            else if (ConfigForm.config.wakeSleepSpell == 1)
            {
                wakeSleepSpell = Spells.Cura;
            }
            else if (ConfigForm.config.wakeSleepSpell == 2)
            {
                wakeSleepSpell = Spells.Curaga;
            }

            // PL and Monitored Player Debuff Removal Starting with PL
            if (PL.Player.Status != 33 && ConfigForm.config.plDebuffEnabled)
            {            
                var debuffIds = PL.Player.Buffs.Where(id => Data.DebuffPriorities.Keys.Cast<short>().Contains(id));
                var debuffPriorityList = debuffIds.Cast<StatusEffect>().OrderBy(status => Array.IndexOf(Data.DebuffPriorities.Keys.ToArray(), status));

                if (debuffPriorityList.Any())
                {
                    // Get the highest priority debuff we have the right spell off cooldown for.
                    var targetDebuff = debuffPriorityList.FirstOrDefault(status => Form2.DebuffEnabled.ContainsKey(status) && Form2.DebuffEnabled[status] && PL.SpellAvailable(Data.DebuffPriorities[status]));

                    if ((short)targetDebuff > 0)
                    {
                        CastSpell(Target.Me, Data.DebuffPriorities[targetDebuff]);
                    }
                }
            }

            // Next, we check monitored player
            if (ConfigForm.config.monitoredDebuffEnabled && (PL.Entity.GetEntity((int)Monitored.Party.GetPartyMember(0).TargetIndex).Distance < 21) && (Monitored.Player.HP > 0) && PL.Player.Status != 33)
            {
                var debuffIds = Monitored.Player.Buffs.Where(id => Data.DebuffPriorities.Keys.Cast<short>().Contains(id));
                var debuffPriorityList = debuffIds.Cast<StatusEffect>().OrderBy(status => Array.IndexOf(Data.DebuffPriorities.Keys.ToArray(), status));

                if (debuffPriorityList.Any())
                {
                    // Get the highest priority debuff we have the right spell off cooldown for.
                    var targetDebuff = debuffPriorityList.FirstOrDefault(status => Form2.DebuffEnabled[status] && PL.SpellAvailable(Data.DebuffPriorities[status]));

                    if ((short)targetDebuff > 0)
                    {
                        // Don't try and curaga outside our party.
                        if ((targetDebuff == StatusEffect.Sleep || targetDebuff == StatusEffect.Sleep2) && !plMonitoredSameParty())
                        {
                            CastSpell(Monitored.Player.Name, Spells.Cure);
                        }

                        if (Data.DebuffPriorities[targetDebuff] != Spells.Erase || plMonitoredSameParty()) 
                        { 
                            CastSpell(Monitored.Player.Name, Data.DebuffPriorities[targetDebuff]);
                        }
                    }
                 }
            }

            if (ConfigForm.config.EnableAddOn)
            {
                
                lock (ActiveBuffs)
                {
                    // ==============================================================================================================================================================================
                    // PARTY DEBUFF REMOVAL

                    // First remove the highest priority debuff.
                    var priorityMember = PL.GetHighestPriorityDebuff(ActiveBuffs);
                    if(priorityMember != null && ActiveBuffs.ContainsKey(priorityMember.Name))
                    {
                        var name = priorityMember.Name;
                        // Filter out non-debuffs, and convert to short IDs. Then calculate the priority order.
                        var debuffIds = ActiveBuffs[name].Where(id => Data.DebuffPriorities.Keys.Cast<short>().Contains(id));
                        var debuffPriorityList = debuffIds.Cast<StatusEffect>().OrderBy(status => Array.IndexOf(Data.DebuffPriorities.Keys.ToArray(), status));

                        if (debuffPriorityList.Any() && ConfigForm.config.enablePartyDebuffRemoval && (characterNames_naRemoval.Contains(name) || ConfigForm.config.SpecifiednaSpellsenable == false))
                        {
                            // Get the highest priority debuff we have the right spell off cooldown for.
                            var targetDebuff = debuffPriorityList.FirstOrDefault(status => Form2.DebuffEnabled[status] && PL.SpellAvailable(Data.DebuffPriorities[status]));

                            if ((short)targetDebuff > 0)
                            {
                                // Don't try and curaga outside our party.
                                if(!priorityMember.InParty(1) && (targetDebuff == StatusEffect.Sleep || targetDebuff == StatusEffect.Sleep2))
                                {
                                    CastSpell(name, Spells.Cure);
                                }

                                CastSpell(name, Data.DebuffPriorities[targetDebuff]);
                            }
                        }
                    }
                }
            }
        }

        private void CuragaCalculator(PartyMember member)
        {
            uint hpLoss = member.HPLoss();
            string cureSpell = Spells.Unknown;
  
            if (ConfigForm.config.curaga5enabled && (hpLoss >= ConfigForm.config.curaga5Amount))
            {
                cureSpell = Spells.Curaga_V;          
            }
            else if (ConfigForm.config.curaga4enabled && (hpLoss >= ConfigForm.config.curaga4Amount))
            {
                cureSpell = Spells.Curaga_IV;           
            }
            else if (ConfigForm.config.curaga3enabled && (hpLoss >= ConfigForm.config.curaga3Amount))
            {
                cureSpell = Spells.Curaga_III;
            }
            else if (ConfigForm.config.curaga2enabled && (hpLoss >= ConfigForm.config.curaga2Amount))
            {
                cureSpell = Spells.Curaga_II;
            }
            else if (ConfigForm.config.curagaEnabled && (hpLoss >= ConfigForm.config.curagaAmount))
            {
                cureSpell = Spells.Curaga;
            }

            if (cureSpell != Spells.Unknown)
            {
                // Check if we need to over/under cure.
                var curagaTier = PickCureTier(cureSpell, Data.CuragaTiers);
                if (ConfigForm.config.curagaTargetType == 0)
                {
                    CastSpell(member.Name, curagaTier);
                }
                else
                {
                    CastSpell(ConfigForm.config.curagaTargetName, curagaTier);
                }
            }

        }

        private bool monitoredStatusCheck(StatusEffect requestedStatus)
        {
            bool statusFound = false;
            foreach (StatusEffect status in Monitored.Player.Buffs.Cast<StatusEffect>().Where(status => requestedStatus == status))
            {
                statusFound = true;
            }
            return statusFound;
        }

        private void CastSpell(string partyMemberName, string spellName, [Optional] string OptionalExtras)
        {
            if(CastingBackground_Check)
            {
                return;
            }

            var apiSpell = PL.Resources.GetSpell(spellName, 0);

            CastSpell(partyMemberName, apiSpell, OptionalExtras);
        }


        private void CastSpell(string partyMemberName, ISpell magic, [Optional] string OptionalExtras)
        {
            castingSpell = magic.Name[0];

            PL.ThirdParty.SendString("/ma \"" + castingSpell + "\" " + partyMemberName);

            if (OptionalExtras != null)
            {
                currentAction.Text = "Casting: " + castingSpell + " [" + OptionalExtras + "]";
            }
            else
            {
                currentAction.Text = "Casting: " + castingSpell;
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

        

        private bool ActiveSpikes()
        {
            if ((ConfigForm.config.plSpikes_Spell == 0) && PL.HasStatus(StatusEffect.Blaze_Spikes))
            {
                return true;
            }
            else if ((ConfigForm.config.plSpikes_Spell == 1) && PL.HasStatus(StatusEffect.Ice_Spikes))
            {
                return true;
            }
            else if ((ConfigForm.config.plSpikes_Spell == 2) && PL.HasStatus(StatusEffect.Shock_Spikes))
            {
                return true;
            }
            return false;
        }

        private bool PLInParty()
        {
            // FALSE IS WANTED WHEN NOT IN PARTY

            if (PL.Player.Name == Monitored.Player.Name) // MONITORED AND POL ARE BOTH THE SAME THEREFORE IN THE PARTY
            {
                return true;
            }

            var PARTYD = PL.Party.GetPartyMembers().Where(p => p.Active != 0 && p.Zone == PL.Player.ZoneId);

            List<string> gen = new List<string>();
            foreach (PartyMember pData in PARTYD)
            {
                if (pData != null && pData.Name != "")
                {
                    gen.Add(pData.Name);
                }
            }

            if (gen.Contains(PL.Player.Name) && gen.Contains(Monitored.Player.Name))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private async void actionTimer_TickAsync(object sender, EventArgs e)
        {
            string[] shell_spells = { "Shell", "Shell II", "Shell III", "Shell IV", "Shell V" };
            string[] protect_spells = { "Protect", "Protect II", "Protect III", "Protect IV", "Protect V" };

            if (PL == null || Monitored == null)
            {
                return;
            }

            if (PL.Player.LoginStatus != (int)LoginStatus.LoggedIn || Monitored.Player.LoginStatus != (int)LoginStatus.LoggedIn)
            {
                return;
            }             

            // Set array values for GUI "Enabled" checkboxes
            CheckBox[] enabledBoxes = new CheckBox[18];
            enabledBoxes[0] = player0enabled;
            enabledBoxes[1] = player1enabled;
            enabledBoxes[2] = player2enabled;
            enabledBoxes[3] = player3enabled;
            enabledBoxes[4] = player4enabled;
            enabledBoxes[5] = player5enabled;
            enabledBoxes[6] = player6enabled;
            enabledBoxes[7] = player7enabled;
            enabledBoxes[8] = player8enabled;
            enabledBoxes[9] = player9enabled;
            enabledBoxes[10] = player10enabled;
            enabledBoxes[11] = player11enabled;
            enabledBoxes[12] = player12enabled;
            enabledBoxes[13] = player13enabled;
            enabledBoxes[14] = player14enabled;
            enabledBoxes[15] = player15enabled;
            enabledBoxes[16] = player16enabled;
            enabledBoxes[17] = player17enabled;

            // Set array values for GUI "High Priority" checkboxes
            CheckBox[] highPriorityBoxes = new CheckBox[18];
            highPriorityBoxes[0] = player0priority;
            highPriorityBoxes[1] = player1priority;
            highPriorityBoxes[2] = player2priority;
            highPriorityBoxes[3] = player3priority;
            highPriorityBoxes[4] = player4priority;
            highPriorityBoxes[5] = player5priority;
            highPriorityBoxes[6] = player6priority;
            highPriorityBoxes[7] = player7priority;
            highPriorityBoxes[8] = player8priority;
            highPriorityBoxes[9] = player9priority;
            highPriorityBoxes[10] = player10priority;
            highPriorityBoxes[11] = player11priority;
            highPriorityBoxes[12] = player12priority;
            highPriorityBoxes[13] = player13priority;
            highPriorityBoxes[14] = player14priority;
            highPriorityBoxes[15] = player15priority;
            highPriorityBoxes[16] = player16priority;
            highPriorityBoxes[17] = player17priority;
         
            // IF ENABLED PAUSE ON KO
            if (ConfigForm.config.pauseOnKO && (PL.Player.Status == 2 || PL.Player.Status == 3))
            {
                pauseButton.Text = "Paused!";
                pauseButton.ForeColor = Color.Red;
                actionTimer.Enabled = false;
                ActiveBuffs.Clear();
                pauseActions = true;
                if (ConfigForm.config.FFXIDefaultAutoFollow == false)
                {
                    PL.AutoFollow.IsAutoFollowing = false;
                }
            }

            // IF YOU ARE DEAD BUT RERAISE IS AVAILABLE THEN ACCEPT RAISE
            if (ConfigForm.config.AcceptRaise == true && (PL.Player.Status == 2 || PL.Player.Status == 3))
            {
                if (PL.Menu.IsMenuOpen && PL.Menu.HelpName == "Revival" && PL.Menu.MenuIndex == 1 && ((ConfigForm.config.AcceptRaiseOnlyWhenNotInCombat == true && Monitored.Player.Status != 1) || ConfigForm.config.AcceptRaiseOnlyWhenNotInCombat == false))
                {
                    await Task.Delay(2000);
                    currentAction.Text = "Accepting Raise or Reraise.";
                    PL.ThirdParty.KeyPress(EliteMMO.API.Keys.NUMPADENTER);
                    await Task.Delay(5000);
                    currentAction.Text = string.Empty;
                }
            }


            // If CastingLock is not FALSE and you're not Terrorized, Petrified, or Stunned run the actions
            if (JobAbilityLock_Check != true && CastingBackground_Check != true && !PL.HasStatus(StatusEffect.Terror) && !PL.HasStatus(StatusEffect.Petrification) && !PL.HasStatus(StatusEffect.Stun))
            {

                // FIRST IF YOU ARE SILENCED OR DOOMED ATTEMPT REMOVAL NOW
                if (PL.HasStatus(StatusEffect.Silence) && ConfigForm.config.plSilenceItemEnabled)
                {
                    // Check to make sure we have echo drops
                    if (GetInventoryItemCount(PL, GetItemId(plSilenceitemName)) > 0 || GetTempItemCount(PL, GetItemId(plSilenceitemName)) > 0)
                    {
                        Item_Wait(plSilenceitemName);
                    }

                }
                else if (PL.HasStatus(StatusEffect.Doom) && ConfigForm.config.plDoomEnabled /* Add more options from UI HERE*/)
                {
                    // Check to make sure we have holy water
                    if (GetInventoryItemCount(PL, GetItemId(plDoomItemName)) > 0 || GetTempItemCount(PL, GetItemId(plDoomItemName)) > 0)
                    {
                        PL.ThirdParty.SendString(string.Format("/item \"{0}\" <me>", plDoomItemName));
                        await Task.Delay(TimeSpan.FromSeconds(2));
                    }
                }

                else if (ConfigForm.config.DivineSeal && PL.Player.MPP <= 11 && PL.AbilityAvailable(Ability.DivineSeal) && !PL.Player.Buffs.Contains((short)StatusEffect.Weakness))
                {
                    JobAbility_Wait(Ability.DivineSeal, Ability.DivineSeal);
                }
                else if (ConfigForm.config.Convert && (PL.Player.MP <= ConfigForm.config.convertMP) && PL.AbilityAvailable(Ability.Convert) && !PL.Player.Buffs.Contains((short)StatusEffect.Weakness))
                {
                    JobAbility_Wait(Ability.Convert, Ability.Convert);
                    return;
                }                            
                if (PL.Player.MP <= (int)ConfigForm.config.mpMinCastValue && PL.Player.MP != 0)
                {
                    if (ConfigForm.config.lowMPcheckBox && !islowmp && !ConfigForm.config.healLowMP)
                    {
                        PL.ThirdParty.SendString("/tell " + Monitored.Player.Name + " MP is low!");
                        islowmp = true;
                        return;
                    }
                    islowmp = true;
                    return;
                }
                if (PL.Player.MP > (int)ConfigForm.config.mpMinCastValue && PL.Player.MP != 0)
                {
                    if (ConfigForm.config.lowMPcheckBox && islowmp && !ConfigForm.config.healLowMP)
                    {
                        PL.ThirdParty.SendString("/tell " + Monitored.Player.Name + " MP OK!");
                        islowmp = false;
                    }
                }

                if (ConfigForm.config.healLowMP == true && PL.Player.MP <= ConfigForm.config.healWhenMPBelow && PL.Player.Status == 0)
                {
                    if (ConfigForm.config.lowMPcheckBox && !islowmp)
                    {
                        PL.ThirdParty.SendString("/tell " + Monitored.Player.Name + " MP is seriously low, /healing.");
                        islowmp = true;
                    }
                    PL.ThirdParty.SendString("/heal");
                }
                else if (ConfigForm.config.standAtMP == true && PL.Player.MPP >= ConfigForm.config.standAtMP_Percentage && PL.Player.Status == 33)
                {
                    if (ConfigForm.config.lowMPcheckBox && !islowmp)
                    {
                        PL.ThirdParty.SendString("/tell " + Monitored.Player.Name + " MP has recovered.");
                        islowmp = false;
                    }
                    PL.ThirdParty.SendString("/heal");
                }

                // Only perform actions if PL is stationary PAUSE GOES HERE
                if ((PL.Player.X == plX) && (PL.Player.Y == plY) && (PL.Player.Z == plZ) && (PL.Player.LoginStatus == (int)LoginStatus.LoggedIn) && JobAbilityLock_Check != true && CastingBackground_Check != true && curePlease_autofollow == false && ((PL.Player.Status == (uint)Status.Standing) || (PL.Player.Status == (uint)Status.Fighting)))
                {
                    plSilenceitemName = Items.SilenceRemoval[ConfigForm.config.plSilenceItem];

                    if (PL.Player.Buffs.Contains((short)StatusEffect.Silence) && ConfigForm.config.plSilenceItemEnabled)
                    {
                        // Check to make sure we have echo drops
                        if (GetInventoryItemCount(PL, GetItemId(plSilenceitemName)) > 0 || GetTempItemCount(PL, GetItemId(plSilenceitemName)) > 0)
                        {
                            PL.ThirdParty.SendString(string.Format("/item \"{0}\" <me>", plSilenceitemName));
                            await Task.Delay(4000);
                        }
                    }

                    #region Primary Logic    
                    IEnumerable<PartyMember> partyByHP = Monitored.GetActivePartyMembers();

                    /////////////////////////// Charmed CHECK /////////////////////////////////////
                    // TODO: Charm logic is messy because it's not configurable currently. Clean this up when adding auto-sleep options.
                    if (PL.Player.MainJob == (byte)Job.BRD)
                    {
                        // Get the list of anyone who's charmed and in range.
                        var charmedMembers = partyByHP.Where(pm => PL.CanCastOn(pm) && ActiveBuffs.ContainsKey(pm.Name) && (ActiveBuffs[pm.Name].Contains((short)StatusEffect.Charm1) || ActiveBuffs[pm.Name].Contains((short)StatusEffect.Charm2)));
                        
                        if (charmedMembers.Any())
                        {
                            // We target the first charmed member who's not already asleep.
                            var sleepTarget = charmedMembers.FirstOrDefault(member => !(ActiveBuffs[member.Name].Contains((short)StatusEffect.Sleep) || ActiveBuffs[member.Name].Contains((short)StatusEffect.Sleep2)));

                            if (sleepTarget != default)
                            {
                                // For now add some redundancy in case the first cast is resisted.
                                var sleepSong = PL.SpellAvailable(Spells.Foe_Lullaby_II) ? Spells.Foe_Lullaby_II : Spells.Foe_Lullaby;
                                
                                CastSpell(sleepTarget.Name, sleepSong);
                                return;
                            }
                        }
                    }

                    /////////////////////////// DOOM CHECK /////////////////////////////////////
                    var doomedMembers = partyByHP.Count(pm => PL.CanCastOn(pm) && ActiveBuffs.ContainsKey(pm.Name) && (ActiveBuffs[pm.Name].Contains((short)StatusEffect.Doom) || ActiveBuffs[pm.Name].Contains((short)StatusEffect.Charm1) || ActiveBuffs[pm.Name].Contains((short)StatusEffect.Charm2)));
                    if(doomedMembers > 0)
                    {
                        RunDebuffChecker();
                        return;
                    }

                    /////////////////////////// PL CURE //////////////////////////////////////////////////////////////////////////////////////////////////////////////////

                    // TODO: Test this! Pretty sure your own character is always party member index 0.
                    // TODO: Cure tiers selecing cures that don't exist!
                    if (PL.Player.HP > 0 && (PL.Player.HPP <= ConfigForm.config.monitoredCurePercentage) && ConfigForm.config.enableOutOfPartyHealing == true && PLInParty() == false)
                    {
                        var plAsPartyMember = PL.Party.GetPartyMember(0);
                        CureCalculator(plAsPartyMember);
                        return;
                    }

                    /////////////////////////// CURAGA //////////////////////////////////////////////////////////////////////////////////////////////////////////////////                                    
                    if (ConfigForm.config.curagaEnabled || ConfigForm.config.curaga2enabled || ConfigForm.config.curaga3enabled || ConfigForm.config.curaga4enabled || ConfigForm.config.curaga5enabled)
                    {
                        int plParty = PLPartyRelativeToMonitored();

                        // Order parties that qualify for AOE cures by average missing HP.
                        var partyNeedsAoe = Monitored.PartyNeedsAoeCure((int)ConfigForm.config.curagaRequiredMembers, ConfigForm.config.curagaCurePercentage).OrderBy(partyNumber => Monitored.AverageHpLossForParty(partyNumber));

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
                                if (partyByHP.Count(pm => pm.InParty(party) && enabledBoxes[pm.MemberNumber].Checked) > 0)
                                {
                                    if (partyByHP.Count(pm => pm.InParty(party) && pm.CurrentHPP < ConfigForm.config.curagaCurePercentage && PL.CanCastOn(pm)) > 0)
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
                                        CuragaCalculator(target);
                                        return;
                                    }
                                    else
                                    {
                                        // We've already determined we can accession, or already have the status.
                                        if (!PL.HasStatus(StatusEffect.Accession))
                                        {
                                            JobAbility_Wait("Accession AOE Cure", Ability.Accession);
                                        }

                                        CureCalculator(target);
                                        return;
                                    }
                                }
                            }
                        }
                    }

                    /////////////////////////// CURE //////////////////////////////////////////////////////////////////////////////////////////////////////////////////

                    // First run a check on the monitored target
                    if (ConfigForm.config.enableMonitoredPriority && Monitored.Player.HP > 0 && (Monitored.Player.HPP <= ConfigForm.config.monitoredCurePercentage))
                    {
                        // Need to get monitored player as a PartyMember
                        PartyMember monitoredPlayer = partyByHP.FirstOrDefault(p => p.Name == Monitored.Player.Name);
                        if(monitoredPlayer != default)
                        {
                            CureCalculator(monitoredPlayer);
                        }
                          
                        return;
                    }
                    else
                    {
                        // Calculate who needs a cure, and is a valid target.
                        // Anyone who's: Enabled + Active + Alive + Under cure threshold
                        var validCures = partyByHP.Where(pm => enabledBoxes[pm.MemberNumber].Checked && (pm.CurrentHPP <= ConfigForm.config.curePercentage) && PL.CanCastOn(pm));

                        // Now run a scan to check all targets in the High Priority Threshold
                        if (validCures != null && validCures.Any()) {
                            var highPriorityCures = validCures.Where(pm => highPriorityBoxes[pm.MemberNumber].Checked);

                            if(highPriorityCures != null && highPriorityCures.Any())
                            {
                                CureCalculator(highPriorityCures.First());
                                return;
                            }
                            else
                            {
                                CureCalculator(validCures.First());
                                return;
                            }
                        }                     
                    }

                    // RUN DEBUFF REMOVAL - CONVERTED TO FUNCTION SO CAN BE RUN IN MULTIPLE AREAS
                    RunDebuffChecker();

                    // PL Auto Buffs

                    string barSpell = ConfigForm.config.AOE_Barelemental ? Data.AoeBarSpells[ConfigForm.config.plBarElement_Spell] : Data.BarSpells[ConfigForm.config.plBarElement_Spell];

                    string barStatus = ConfigForm.config.AOE_Barstatus ? Data.AoeBarStatus[ConfigForm.config.plBarStatus_Spell] : Data.BarStatus[ConfigForm.config.plBarStatus_Spell];

                    string enspell = Data.Enspells[ConfigForm.config.plEnspell_Spell];
                    string stormspell = Data.StormTiers[ConfigForm.config.plStormSpell_Spell];
                    string gainBoostSpell = Data.GainBoostSpells[ConfigForm.config.plGainBoost_Spell];

                    if (PL.Player.LoginStatus == (int)LoginStatus.LoggedIn && JobAbilityLock_Check != true && CastingBackground_Check != true)
                    {
                        if (ConfigForm.config.Composure && (!PL.HasStatus(StatusEffect.Composure)) && PL.AbilityAvailable(Ability.Composure))
                        {

                            JobAbility_Wait(Ability.Composure, Ability.Composure);
                        }
                        else if (ConfigForm.config.LightArts && (!PL.HasStatus(StatusEffect.Light_Arts)) && (!PL.HasStatus(StatusEffect.Addendum_White)) && PL.AbilityAvailable(Ability.LightArts))
                        {
                            JobAbility_Wait(Ability.LightArts, Ability.LightArts);
                        }
                        else if (ConfigForm.config.AddendumWhite && (!PL.HasStatus(StatusEffect.Addendum_White)) && PL.HasStatus(StatusEffect.Light_Arts) && PL.CurrentSCHCharges() > 0)
                        {
                            JobAbility_Wait(Ability.AddendumWhite, Ability.AddendumWhite);
                        }
                        else if (ConfigForm.config.DarkArts && (!PL.HasStatus(StatusEffect.Dark_Arts)) && (!PL.HasStatus(StatusEffect.Addendum_Black)) && PL.AbilityAvailable(Ability.DarkArts))
                        {
                            JobAbility_Wait(Ability.DarkArts, Ability.DarkArts);
                        }
                        else if (ConfigForm.config.AddendumBlack && PL.HasStatus(StatusEffect.Dark_Arts) && (!PL.HasStatus(StatusEffect.Addendum_Black)) && PL.CurrentSCHCharges() > 0)
                        {
                            JobAbility_Wait(Ability.AddendumBlack, Ability.AddendumBlack);
                        }
                        else if (ConfigForm.config.plShellra && (!PL.HasStatus(StatusEffect.Shell)))
                        {
                            var shellraSpell = Data.ShellraTiers[(int)ConfigForm.config.plShellra_Level - 1];
                            if (PL.SpellAvailable(shellraSpell))
                            {
                                CastSpell(Target.Me, shellraSpell);
                            }
                        }
                        else if (ConfigForm.config.plProtectra && (!PL.HasStatus(StatusEffect.Protect)))
                        {
                            var protectraSpell = Data.ProtectraTiers[(int)ConfigForm.config.plProtectra_Level - 1];
                            if (PL.SpellAvailable(protectraSpell))
                            {
                                CastSpell(Target.Me, protectraSpell);
                            }
                        }
                        else if (ConfigForm.config.plBarElement && !PL.HasStatus(Data.SpellEffects[barSpell]) && PL.SpellAvailable(barSpell))
                        {
                            if (ConfigForm.config.Accession && ConfigForm.config.barspellAccession && PL.CurrentSCHCharges() > 0 && PL.AbilityAvailable(Ability.Accession) && ConfigForm.config.AOE_Barelemental == false && !PL.HasStatus(StatusEffect.Accession))
                            {
                                JobAbility_Wait("Barspell, Accession", Ability.Accession);
                                return;
                            }

                            if (ConfigForm.config.Perpetuance && ConfigForm.config.barspellPerpetuance && PL.CurrentSCHCharges() > 0 && PL.AbilityAvailable(Ability.Perpetuance) && !PL.HasStatus(StatusEffect.Perpetuance))
                            {
                                JobAbility_Wait("Barspell, Perpetuance", Ability.Perpetuance);
                                return;
                            }

                            CastSpell(Target.Me, barSpell);
                        }
                        else if (ConfigForm.config.plBarStatus && !PL.HasStatus(Data.SpellEffects[barStatus]) && PL.SpellAvailable(barStatus))
                        {
                            if (ConfigForm.config.Accession && ConfigForm.config.barstatusAccession && PL.CurrentSCHCharges() > 0 && PL.AbilityAvailable(Ability.Accession) && ConfigForm.config.AOE_Barstatus == false && !PL.HasStatus(StatusEffect.Accession))
                            {
                                JobAbility_Wait("Barstatus, Accession", Ability.Accession);
                                return;
                            }

                            if (ConfigForm.config.Perpetuance && ConfigForm.config.barstatusPerpetuance && PL.CurrentSCHCharges() > 0 && PL.AbilityAvailable(Ability.Perpetuance) && !PL.HasStatus(StatusEffect.Perpetuance))
                            {
                                JobAbility_Wait("Barstatus, Perpetuance", Ability.Perpetuance);
                                return;
                            }

                            CastSpell(Target.Me, barStatus);
                        }
                        else if (ConfigForm.config.plGainBoost && !PL.HasStatus(Data.SpellEffects[gainBoostSpell]) && PL.SpellAvailable(gainBoostSpell))
                        {
                            CastSpell(Target.Me, gainBoostSpell);
                        }
                        else if (ConfigForm.config.plStormSpell && !PL.HasStatus(Data.SpellEffects[stormspell]) && PL.SpellAvailable(stormspell))
                        {
                            if (ConfigForm.config.Accession && ConfigForm.config.stormspellAccession && PL.CurrentSCHCharges() > 0 && PL.AbilityAvailable(Ability.Accession) && !PL.HasStatus(StatusEffect.Accession))
                            {
                                JobAbility_Wait("Stormspell, Accession", Ability.Accession);
                                return;
                            }

                            if (ConfigForm.config.Perpetuance && ConfigForm.config.stormspellPerpetuance && PL.CurrentSCHCharges() > 0 && PL.AbilityAvailable(Ability.Perpetuance) && !PL.HasStatus(StatusEffect.Perpetuance))
                            {
                                JobAbility_Wait("Stormspell, Perpetuance", Ability.Perpetuance);
                                return;
                            }

                            CastSpell(Target.Me, stormspell);
                        }
                        else if (ConfigForm.config.plProtect && (!PL.HasStatus(StatusEffect.Protect)))
                        {
                            string protectSpell = Data.ProtectTiers[ConfigForm.config.autoProtect_Spell];

                            if (protectSpell != Spells.Unknown && PL.SpellAvailable(protectSpell))
                            {
                                if (ConfigForm.config.Accession && ConfigForm.config.accessionProShell && PL.Party.GetPartyMembers().Count() > 2 && PL.AbilityAvailable(Ability.Accession) && PL.CurrentSCHCharges() > 0)
                                {
                                    if (!PL.HasStatus(StatusEffect.Accession))
                                    {
                                        JobAbility_Wait("Protect, Accession", Ability.Accession);
                                        return;
                                    }
                                }

                                CastSpell(Target.Me, protectSpell);
                            }
                        }
                        else if (ConfigForm.config.plShell && (!PL.HasStatus(StatusEffect.Shell)))
                        {
                            string shellSpell = Data.ShellTiers[ConfigForm.config.autoShell_Spell];

                            if (shellSpell != Spells.Unknown && PL.SpellAvailable(shellSpell))
                            {
                                if (ConfigForm.config.Accession && ConfigForm.config.accessionProShell && PL.Party.GetPartyMembers().Count() > 2 && PL.CurrentSCHCharges() > 0 && PL.AbilityAvailable(Ability.Accession))
                                {
                                    if (!PL.HasStatus(StatusEffect.Accession))
                                    {
                                        JobAbility_Wait("Shell, Accession", Ability.Accession);
                                        return;
                                    }
                                }

                                CastSpell(Target.Me, shellSpell);
                            }
                        }
                        else if (ConfigForm.config.plReraise && ConfigForm.config.EnlightenmentReraise && (!PL.HasStatus(StatusEffect.Reraise)) && !PL.HasStatus(StatusEffect.Addendum_White) && PL.AbilityAvailable(Ability.Enlightenment))
                        {
                            if (!PL.HasStatus(StatusEffect.Enlightenment) && PL.AbilityAvailable(Ability.Enlightenment))
                            {
                                JobAbility_Wait("Reraise, Enlightenment", Ability.Enlightenment);
                                return;
                            }

                            var reraiseSpell = Data.ReraiseTiers[ConfigForm.config.plReraise_Level - 1];
                            if (PL.HasMPFor(reraiseSpell) && PL.SpellAvailable(reraiseSpell))
                            {
                                CastSpell(Target.Me, reraiseSpell);
                            }
                        }
                        else if (ConfigForm.config.plReraise && (!PL.HasStatus(StatusEffect.Reraise)))
                        {
                            var reraiseSpell = Data.ReraiseTiers[ConfigForm.config.plReraise_Level - 1];
                            if (PL.HasMPFor(reraiseSpell) && PL.SpellAvailable(reraiseSpell))
                            {
                                CastSpell(Target.Me, reraiseSpell);
                            }
                        }
                        else if (ConfigForm.config.plUtsusemi && PL.ShadowsRemaining() < 2)
                        {
                            if (PL.SpellAvailable(Spells.Utsusemi_Ni) && GetInventoryItemCount(PL, GetItemId("Shihei")) > 0)
                            {
                                CastSpell(Target.Me, Spells.Utsusemi_Ni);
                            }
                            else if (PL.SpellAvailable(Spells.Utsusemi_Ichi) && (PL.ShadowsRemaining() == 0) && GetInventoryItemCount(PL, GetItemId("Shihei")) > 0)
                            {
                                CastSpell(Target.Me, Spells.Utsusemi_Ichi);
                            }
                        }

                        else if (ConfigForm.config.plBlink && (!PL.HasStatus(StatusEffect.Blink)) && PL.SpellAvailable(Spells.Blink))
                        {

                            if (ConfigForm.config.Accession && ConfigForm.config.blinkAccession && PL.CurrentSCHCharges() > 0 && PL.AbilityAvailable(Ability.Accession) && !PL.HasStatus(StatusEffect.Accession))
                            {
                                JobAbility_Wait("Blink, Accession", Ability.Accession);
                                return;
                            }

                            if (ConfigForm.config.Perpetuance && ConfigForm.config.blinkPerpetuance && PL.CurrentSCHCharges() > 0 && PL.AbilityAvailable(Ability.Perpetuance) && !PL.HasStatus(StatusEffect.Perpetuance))
                            {
                                JobAbility_Wait("Blink, Perpetuance", Ability.Perpetuance);
                                return;
                            }

                            CastSpell(Target.Me, Spells.Blink);
                        }
                        else if (ConfigForm.config.plPhalanx && (!PL.HasStatus(StatusEffect.Phalanx)) && PL.SpellAvailable(Spells.Phalanx))
                        {
                            if (ConfigForm.config.Accession && ConfigForm.config.phalanxAccession && PL.CurrentSCHCharges() > 0 && PL.AbilityAvailable(Ability.Accession) && !PL.HasStatus(StatusEffect.Accession))
                            {
                                JobAbility_Wait("Phalanx, Accession", Ability.Accession);
                                return;
                            }

                            if (ConfigForm.config.Perpetuance && ConfigForm.config.phalanxPerpetuance && PL.CurrentSCHCharges() > 0 && PL.AbilityAvailable(Ability.Perpetuance) && !PL.HasStatus(StatusEffect.Perpetuance))
                            {
                                JobAbility_Wait("Phalanx, Perpetuance", Ability.Perpetuance);
                                return;
                            }

                            CastSpell(Target.Me, Spells.Phalanx);
                        }
                        else if (ConfigForm.config.plRefresh && (!PL.HasStatus(StatusEffect.Refresh)))
                        {
                            var refreshSpell = Data.RefreshTiers[ConfigForm.config.plRefresh_Level - 1];

                            if (PL.SpellAvailable(Spells.Refresh))
                            {
                                if (ConfigForm.config.Accession && ConfigForm.config.refreshAccession && PL.CurrentSCHCharges() > 0 && PL.AbilityAvailable(Ability.Accession) && !PL.HasStatus(StatusEffect.Accession))
                                {
                                    JobAbility_Wait("Refresh, Accession", Ability.Accession);
                                    return;
                                }
                                if (ConfigForm.config.Perpetuance && ConfigForm.config.refreshPerpetuance && PL.CurrentSCHCharges() > 0 && PL.AbilityAvailable(Ability.Perpetuance) && !PL.HasStatus(StatusEffect.Perpetuance))
                                {
                                    JobAbility_Wait("Refresh, Perpetuance", Ability.Perpetuance);
                                    return;
                                }

                                CastSpell(Target.Me, refreshSpell);
                            }
                        }
                        else if (ConfigForm.config.plRegen && (!PL.HasStatus(StatusEffect.Regen)))
                        {
                            if (ConfigForm.config.Accession && ConfigForm.config.regenAccession && PL.CurrentSCHCharges() > 0 && PL.AbilityAvailable(Ability.Accession) && !PL.HasStatus(StatusEffect.Accession))
                            {
                                JobAbility_Wait("Regen, Accession", Ability.Accession);
                                return;
                            }
                            if (ConfigForm.config.Perpetuance && ConfigForm.config.regenPerpetuance && PL.CurrentSCHCharges() > 0 && PL.AbilityAvailable(Ability.Perpetuance) && !PL.HasStatus(StatusEffect.Perpetuance))
                            {
                                JobAbility_Wait("Regen, Perpetuance", Ability.Perpetuance);
                                return;
                            }

                            var regenSpell = Data.RegenTiers[ConfigForm.config.plRegen_Level - 1];
                            if (PL.HasMPFor(regenSpell) && PL.SpellAvailable(regenSpell))
                            {
                                CastSpell(Target.Me, regenSpell);
                            }
                        }
                        else if (ConfigForm.config.plAdloquium && (!PL.HasStatus(StatusEffect.Regain)) && PL.SpellAvailable(Spells.Adloquium))
                        {
                            if (ConfigForm.config.Accession && ConfigForm.config.adloquiumAccession && PL.CurrentSCHCharges() > 0 && PL.AbilityAvailable(Ability.Accession) && !PL.HasStatus(StatusEffect.Accession))
                            {
                                JobAbility_Wait("Adloquium, Accession", Ability.Accession);
                                return;
                            }

                            if (ConfigForm.config.Perpetuance && ConfigForm.config.adloquiumPerpetuance && PL.CurrentSCHCharges() > 0 && PL.AbilityAvailable(Ability.Perpetuance) && !PL.HasStatus(StatusEffect.Perpetuance))
                            {
                                JobAbility_Wait("Adloquium, Perpetuance", Ability.Perpetuance);
                                return;
                            }

                            CastSpell(Target.Me, Spells.Adloquium);
                        }
                        else if (ConfigForm.config.plStoneskin && (!PL.HasStatus(StatusEffect.Stoneskin)) && PL.SpellAvailable(Spells.Stoneskin))
                        {
                            if (ConfigForm.config.Accession && ConfigForm.config.stoneskinAccession && PL.CurrentSCHCharges() > 0 && PL.AbilityAvailable(Ability.Accession) && !PL.HasStatus(StatusEffect.Accession))
                            {
                                JobAbility_Wait("Stoneskin, Accession", Ability.Accession);
                                return;
                            }

                            if (ConfigForm.config.Perpetuance && ConfigForm.config.stoneskinPerpetuance && PL.CurrentSCHCharges() > 0 && PL.AbilityAvailable(Ability.Perpetuance) && !PL.HasStatus(StatusEffect.Perpetuance))
                            {
                                JobAbility_Wait("Stoneskin, Perpetuance", Ability.Perpetuance);
                                return;
                            }

                            CastSpell(Target.Me, Spells.Stoneskin);
                        }
                        else if (ConfigForm.config.plAquaveil && (!PL.HasStatus(StatusEffect.Aquaveil)) && PL.SpellAvailable(Spells.Aquaveil))
                        {
                            if (ConfigForm.config.Accession && ConfigForm.config.aquaveilAccession && PL.CurrentSCHCharges() > 0 && PL.AbilityAvailable(Ability.Accession) && !PL.HasStatus(StatusEffect.Accession))
                            {
                                JobAbility_Wait("Aquaveil, Accession", Ability.Accession);
                                return;
                            }

                            if (ConfigForm.config.Perpetuance && ConfigForm.config.aquaveilPerpetuance && PL.CurrentSCHCharges() > 0 && PL.AbilityAvailable(Ability.Perpetuance) && PL.HasStatus(StatusEffect.Perpetuance))
                            {
                                JobAbility_Wait("Aquaveil, Perpetuance", Ability.Perpetuance);
                                return;
                            }

                            CastSpell(Target.Me, Spells.Aquaveil);
                        }
                        else if (ConfigForm.config.plKlimaform && !PL.HasStatus(StatusEffect.Klimaform))
                        {
                            if (PL.SpellAvailable(Spells.Klimaform))
                            {
                                CastSpell(Target.Me, Spells.Klimaform);
                            }
                        }
                        else if (ConfigForm.config.plTemper && (!PL.HasStatus(StatusEffect.Multi_Strikes)))
                        {
                            if ((ConfigForm.config.plTemper_Level == 1) && PL.SpellAvailable(Spells.Temper))
                            {
                                CastSpell(Target.Me, Spells.Temper);
                            }
                            else if ((ConfigForm.config.plTemper_Level == 2) && PL.SpellAvailable(Spells.Temper_II))
                            {
                                CastSpell(Target.Me, Spells.Temper_II);
                            }
                        }
                        else if (ConfigForm.config.plHaste && (!PL.HasStatus(StatusEffect.Haste)))
                        {
                            if ((ConfigForm.config.plHaste_Level == 1) && PL.SpellAvailable(Spells.Haste))
                            {
                                CastSpell(Target.Me, Spells.Haste);
                            }
                            else if ((ConfigForm.config.plHaste_Level == 2) && PL.SpellAvailable(Spells.Haste_II))
                            {
                                CastSpell(Target.Me, Spells.Haste_II);
                            }
                        }
                        else if (ConfigForm.config.plSpikes && ActiveSpikes() == false)
                        {
                            if ((ConfigForm.config.plSpikes_Spell == 0) && PL.SpellAvailable(Spells.Blaze_Spikes))
                            {
                                CastSpell(Target.Me, Spells.Blaze_Spikes);
                            }
                            else if ((ConfigForm.config.plSpikes_Spell == 1) && PL.SpellAvailable(Spells.Ice_Spikes))
                            {
                                CastSpell(Target.Me, Spells.Ice_Spikes);
                            }
                            else if ((ConfigForm.config.plSpikes_Spell == 2) && PL.SpellAvailable(Spells.Shock_Spikes))
                            {
                                CastSpell(Target.Me, Spells.Shock_Spikes);
                            }
                        }
                        else if (ConfigForm.config.plEnspell && !PL.HasStatus(Data.SpellEffects[enspell]) && PL.SpellAvailable(enspell))
                        {
                            // Only tries to accession for tier 1 enspells, not tier 2.
                            if (ConfigForm.config.Accession && ConfigForm.config.enspellAccession && PL.CurrentSCHCharges() > 0 && PL.AbilityAvailable(Ability.Accession) && ConfigForm.config.plEnspell_Spell < 6 && !PL.HasStatus(StatusEffect.Accession))
                            {
                                JobAbility_Wait("Enspell, Accession", Ability.Accession);
                                return;
                            }

                            if (ConfigForm.config.Perpetuance && ConfigForm.config.enspellPerpetuance && PL.CurrentSCHCharges() > 0 && PL.AbilityAvailable(Ability.Perpetuance) && ConfigForm.config.plEnspell_Spell < 6 && !PL.HasStatus(StatusEffect.Perpetuance))
                            {
                                JobAbility_Wait("Enspell, Perpetuance", Ability.Perpetuance);
                                return;
                            }

                            CastSpell(Target.Me, enspell);
                        }
                        else if (ConfigForm.config.plAuspice && (!PL.HasStatus(StatusEffect.Auspice)) && PL.SpellAvailable(Spells.Auspice))
                        {
                            CastSpell(Target.Me, Spells.Auspice);
                        }
                        #endregion                      

                        else if (ConfigForm.config.autoTarget && PL.SpellAvailable(ConfigForm.config.autoTargetSpell))
                        {
                            if (ConfigForm.config.Hate_SpellType == 1) // PARTY BASED HATE SPELL
                            {
                                int enemyID = CheckEngagedStatus_Hate();

                                if (enemyID != 0 && enemyID != lastKnownEstablisherTarget)
                                {
                                    CastSpell(ConfigForm.config.autoTarget_Target, ConfigForm.config.autoTargetSpell);
                                    lastKnownEstablisherTarget = enemyID;
                                }
                            }
                            else // ENEMY BASED TARGET
                            {
                                int enemyID = CheckEngagedStatus_Hate();

                                if (enemyID != 0 && enemyID != lastKnownEstablisherTarget)
                                {
                                    PL.Target.SetTarget(enemyID);
                                    await Task.Delay(TimeSpan.FromMilliseconds(500));
                                    CastSpell("<t>", ConfigForm.config.autoTargetSpell);
                                    lastKnownEstablisherTarget = enemyID;
                                    await Task.Delay(TimeSpan.FromMilliseconds(1000));

                                    if (ConfigForm.config.DisableTargettingCancel == false)
                                    {
                                        await Task.Delay(TimeSpan.FromSeconds((double)ConfigForm.config.TargetRemoval_Delay));
                                        PL.Target.SetTarget(0);
                                    }
                                }
                            }
                        }

                        // BARD SONGS

                        else if (PL.Player.MainJob == (byte)Job.BRD && ConfigForm.config.enableSinging && !PL.HasStatus(StatusEffect.Silence) && (PL.Player.Status == 1 || PL.Player.Status == 0))
                        {
                            var songAction = SongEngine.Run();

                            if (!string.IsNullOrEmpty(songAction.Spell))
                            {
                                CastSpell(songAction.Target, songAction.Spell);
                            }
                        }

                        // GEO Stuff

                        else if (PL.Player.MainJob == (byte)Job.GEO && ConfigForm.config.EnableGeoSpells && !PL.HasStatus(StatusEffect.Silence) && (PL.Player.Status == 1 || PL.Player.Status == 0))
                        {
                            var geoAction = GeoEngine.Run();

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

                        int plParty = PLPartyRelativeToMonitored();

                        // so PL job abilities are in order
                        if (PL.Player.Status == 1 || PL.Player.Status == 0)
                        {
                            if (ConfigForm.config.AfflatusSolace && (!PL.HasStatus(StatusEffect.Afflatus_Solace)) && PL.AbilityAvailable(Ability.AfflatusSolace))
                            {
                                JobAbility_Wait(Ability.AfflatusSolace, Ability.AfflatusSolace);
                            }
                            else if (ConfigForm.config.AfflatusMisery && (!PL.HasStatus(StatusEffect.Afflatus_Misery)) && PL.AbilityAvailable(Ability.AfflatusMisery))
                            {
                                JobAbility_Wait(Ability.AfflatusMisery, Ability.AfflatusMisery);
                            }
                            else if (ConfigForm.config.Composure && (!PL.HasStatus(StatusEffect.Composure)) && PL.AbilityAvailable(Ability.Composure))
                            {
                                JobAbility_Wait("Composure #2", Ability.Composure);
                            }
                            else if (ConfigForm.config.LightArts && (!PL.HasStatus(StatusEffect.Light_Arts)) && (!PL.HasStatus(StatusEffect.Addendum_White)) && PL.AbilityAvailable(Ability.LightArts))
                            {
                                JobAbility_Wait("Light Arts #2", Ability.LightArts);
                            }
                            else if (ConfigForm.config.AddendumWhite && (!PL.HasStatus(StatusEffect.Addendum_White)) && PL.CurrentSCHCharges() > 0)
                            {
                                JobAbility_Wait(Ability.AddendumWhite, Ability.AddendumWhite);
                            }
                            else if (ConfigForm.config.Sublimation && (!PL.HasStatus(StatusEffect.Sublimation_Activated)) && (!PL.HasStatus(StatusEffect.Sublimation_Complete)) && (!PL.HasStatus(StatusEffect.Refresh)) && PL.AbilityAvailable(Ability.Sublimation))
                            {
                                JobAbility_Wait("Sublimation, Charging", Ability.Sublimation);
                            }
                            else if (ConfigForm.config.Sublimation && ((PL.Player.MPMax - PL.Player.MP) > ConfigForm.config.sublimationMP) && PL.HasStatus(StatusEffect.Sublimation_Complete) && PL.AbilityAvailable(Ability.Sublimation))
                            {
                                JobAbility_Wait("Sublimation, Recovery", Ability.Sublimation);
                            }
                            else if (ConfigForm.config.DivineCaress && (ConfigForm.config.plDebuffEnabled || ConfigForm.config.monitoredDebuffEnabled || ConfigForm.config.enablePartyDebuffRemoval) && PL.AbilityAvailable(Ability.DivineCaress))
                            {
                                JobAbility_Wait(Ability.DivineCaress, Ability.DivineCaress);
                            }
                            else if (ConfigForm.config.Devotion && PL.AbilityAvailable(Ability.Devotion) && PL.Player.HPP > 80 && (!ConfigForm.config.DevotionWhenEngaged || (Monitored.Player.Status == 1)))
                            {
                                // Get all active members who are in the PLs party.
                                IEnumerable<PartyMember> cParty = Monitored.GetActivePartyMembers().Where(member => member.InParty(plParty) && member.Name != PL.Player.Name);

                                // If we're set to only devotion a specific target, filter the list for that target.
                                if (ConfigForm.config.DevotionTargetType == 0)
                                {
                                    cParty = cParty.Where(member => member.Name == ConfigForm.config.DevotionTargetName);
                                }

                                // Get the first member who's within range, and has enough missing MP to meet our config criteria.
                                var devotionTarget = cParty.FirstOrDefault(member => member.CurrentMP <= ConfigForm.config.DevotionMP && PL.EntityWithin(10, member.TargetIndex));

                                if (devotionTarget != default)
                                {
                                    PL.ThirdParty.SendString("/ja \"Devotion\" " + devotionTarget.Name);
                                    Thread.Sleep(TimeSpan.FromSeconds(2));
                                }
                            }
                        }

                        var playerBuffOrder = Monitored.Party.GetPartyMembers().OrderBy(p => p.MemberNumber).OrderBy(p => p.Active == 0).Where(p => p.Active == 1);

                        // Auto Casting BUFF STUFF
                        if (PL.Player.Status == 1 || PL.Player.Status == 0)
                        {
                            var buffAction = BuffEngine.Run();

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
                    }
                }
            }
        }

        private void settingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ConfigForm settings = new ConfigForm();
            settings.Show();
        }

        private void ShowPlayerOptionsFor(GroupBox party, byte ptIndex)
        {
            playerOptionsSelected = ptIndex;
            var name = Monitored.Party.GetPartyMembers()[ptIndex].Name;

            autoHasteToolStripMenuItem.Checked = BuffEngine.BuffEnabled(name, Spells.Haste);
            autoHasteIIToolStripMenuItem.Checked = BuffEngine.BuffEnabled(name, Spells.Haste_II);
            autoAdloquiumToolStripMenuItem.Checked = BuffEngine.BuffEnabled(name, Spells.Adloquium);
            autoFlurryToolStripMenuItem.Checked = BuffEngine.BuffEnabled(name, Spells.Flurry);
            autoFlurryIIToolStripMenuItem.Checked = BuffEngine.BuffEnabled(name, Spells.Flurry_II);
            autoProtectToolStripMenuItem.Checked = BuffEngine.BuffEnabled(name, Spells.Protect);
            autoShellToolStripMenuItem.Checked = BuffEngine.BuffEnabled(name, Spells.Shell);

            playerOptions.Show(party, new Point(0, 0));               
        }

        private void ShowPlayerBuffsFor(GroupBox party, byte ptIndex)
        {
            autoOptionsSelected = ptIndex;
            var name = Monitored.Party.GetPartyMembers()[ptIndex].Name;

            // TODO: Figure out tiers and stuff, don't play SCH so not tier-II storms probably busted.
            if (party == party0)
            {
                autoPhalanxIIToolStripMenuItem1.Checked = BuffEngine.BuffEnabled(name, Spells.Phalanx_II);
                autoRegenVToolStripMenuItem.Checked = BuffEngine.BuffEnabled(name, Spells.Regen);
                autoRefreshIIToolStripMenuItem.Checked = BuffEngine.BuffEnabled(name, Spells.Refresh);
                SandstormToolStripMenuItem.Checked = BuffEngine.BuffEnabled(name, Spells.Sandstorm);
                RainstormToolStripMenuItem.Checked = BuffEngine.BuffEnabled(name, Spells.Rainstorm);
                WindstormToolStripMenuItem.Checked = BuffEngine.BuffEnabled(name, Spells.Windstorm);
                FirestormToolStripMenuItem.Checked = BuffEngine.BuffEnabled(name, Spells.Firestorm);
                HailstormToolStripMenuItem.Checked = BuffEngine.BuffEnabled(name, Spells.Hailstorm);
                ThunderstormToolStripMenuItem.Checked = BuffEngine.BuffEnabled(name, Spells.Thunderstorm);
                VoidstormToolStripMenuItem.Checked = BuffEngine.BuffEnabled(name, Spells.Voidstorm);
                AurorastormToolStripMenuItem.Checked = BuffEngine.BuffEnabled(name, Spells.Aurorastorm);
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
                    currentAction.Text = "Using an Item: " + ItemName;
                    PL.ThirdParty.SendString("/item \"" + ItemName + "\" <me>");
                    await Task.Delay(TimeSpan.FromSeconds(5));
                    castingLockLabel.Text = "Casting is UNLOCKED";
                    currentAction.Text = string.Empty;
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
                    currentAction.Text = "Using a Job Ability: " + JobabilityDATA;
                    PL.ThirdParty.SendString("/ja \"" + JobAbilityName + "\" <me>");
                    await Task.Delay(TimeSpan.FromSeconds(2));
                    castingLockLabel.Text = "Casting is UNLOCKED";
                    currentAction.Text = string.Empty;
                    castingSpell = string.Empty;
                    JobAbilityLock_Check = false;
                }));
            }
        }

        private void autoHasteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // TODO: Add in special logic to make sure we can't select more then
            // ONE of haste/haste2/flurry/flurry2
            var name = Monitored.Party.GetPartyMembers()[playerOptionsSelected].Name;
            BuffEngine.ToggleAutoBuff(name, Spells.Haste);
        }

        private void autoHasteIIToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var name = Monitored.Party.GetPartyMembers()[playerOptionsSelected].Name;
            BuffEngine.ToggleAutoBuff(name, Spells.Haste_II);
        }

        private void autoAdloquiumToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var name = Monitored.Party.GetPartyMembers()[playerOptionsSelected].Name;
            BuffEngine.ToggleAutoBuff(name, Spells.Adloquium);
        }

        private void autoFlurryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var name = Monitored.Party.GetPartyMembers()[playerOptionsSelected].Name;
            BuffEngine.ToggleAutoBuff(name, Spells.Flurry);
        }

        private void autoFlurryIIToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var name = Monitored.Party.GetPartyMembers()[playerOptionsSelected].Name;
            BuffEngine.ToggleAutoBuff(name, Spells.Flurry_II);
        }

        private void autoProtectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var name = Monitored.Party.GetPartyMembers()[playerOptionsSelected].Name;
            BuffEngine.ToggleAutoBuff(name, Spells.Protect);
        }

        private void enableDebuffRemovalToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string generated_name = Monitored.Party.GetPartyMembers()[playerOptionsSelected].Name;
            characterNames_naRemoval.Add(generated_name);
        }

        private void autoShellToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var name = Monitored.Party.GetPartyMembers()[playerOptionsSelected].Name;
            BuffEngine.ToggleAutoBuff(name, Spells.Shell);
        }

        private void autoHasteToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            var name = Monitored.Party.GetPartyMembers()[autoOptionsSelected].Name;
            BuffEngine.ToggleAutoBuff(name, Spells.Haste);
        }

        private void autoPhalanxIIToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            var name = Monitored.Party.GetPartyMembers()[autoOptionsSelected].Name;
            BuffEngine.ToggleAutoBuff(name, Spells.Phalanx_II);
        }

        private void autoRegenVToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var name = Monitored.Party.GetPartyMembers()[autoOptionsSelected].Name;
            BuffEngine.ToggleAutoBuff(name, Spells.Regen);
        }

        private void autoRefreshIIToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var name = Monitored.Party.GetPartyMembers()[autoOptionsSelected].Name;
            BuffEngine.ToggleAutoBuff(name, Spells.Refresh);
        }

        private void hasteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CastSpell(Monitored.Party.GetPartyMembers()[playerOptionsSelected].Name, Spells.Haste);
        }

        private void followToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ConfigForm.config.autoFollowName = Monitored.Party.GetPartyMembers()[playerOptionsSelected].Name;
        }

        private void stopfollowToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ConfigForm.config.autoFollowName = string.Empty;
        }

        private void EntrustTargetToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ConfigForm.config.EntrustedSpell_Target = Monitored.Party.GetPartyMembers()[playerOptionsSelected].Name;
        }

        private void GeoTargetToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ConfigForm.config.LuopanSpell_Target = Monitored.Party.GetPartyMembers()[playerOptionsSelected].Name;
        }

        private void DevotionTargetToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ConfigForm.config.DevotionTargetName = Monitored.Party.GetPartyMembers()[playerOptionsSelected].Name;
        }

        private void HateEstablisherToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ConfigForm.config.autoTarget_Target = Monitored.Party.GetPartyMembers()[playerOptionsSelected].Name;
        }

        private void phalanxIIToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CastSpell(Monitored.Party.GetPartyMembers()[playerOptionsSelected].Name, Spells.Phalanx_II);
        }

        private void invisibleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CastSpell(Monitored.Party.GetPartyMembers()[playerOptionsSelected].Name, Spells.Invisible);
        }

        private void refreshToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CastSpell(Monitored.Party.GetPartyMembers()[playerOptionsSelected].Name, Spells.Refresh);
        }

        private void refreshIIToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CastSpell(Monitored.Party.GetPartyMembers()[playerOptionsSelected].Name, Spells.Refresh_II);
        }

        private void refreshIIIToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CastSpell(Monitored.Party.GetPartyMembers()[playerOptionsSelected].Name, Spells.Refresh_III);
        }

        private void sneakToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CastSpell(Monitored.Party.GetPartyMembers()[playerOptionsSelected].Name, Spells.Sneak);
        }

        private void regenIIToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CastSpell(Monitored.Party.GetPartyMembers()[playerOptionsSelected].Name, Spells.Regen_II);
        }

        private void regenIIIToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CastSpell(Monitored.Party.GetPartyMembers()[playerOptionsSelected].Name, Spells.Regen_III);
        }

        private void regenIVToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CastSpell(Monitored.Party.GetPartyMembers()[playerOptionsSelected].Name, Spells.Regen_IV);
        }

        private void eraseToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CastSpell(Monitored.Party.GetPartyMembers()[playerOptionsSelected].Name, Spells.Erase);
        }

        private void sacrificeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CastSpell(Monitored.Party.GetPartyMembers()[playerOptionsSelected].Name, Spells.Sacrifice);
        }

        private void blindnaToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CastSpell(Monitored.Party.GetPartyMembers()[playerOptionsSelected].Name, Spells.Blindna);
        }

        private void cursnaToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CastSpell(Monitored.Party.GetPartyMembers()[playerOptionsSelected].Name, Spells.Cursna);
        }

        private void paralynaToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CastSpell(Monitored.Party.GetPartyMembers()[playerOptionsSelected].Name, Spells.Paralyna);
        }

        private void poisonaToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CastSpell(Monitored.Party.GetPartyMembers()[playerOptionsSelected].Name, Spells.Poisona);
        }

        private void stonaToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CastSpell(Monitored.Party.GetPartyMembers()[playerOptionsSelected].Name, Spells.Stona);
        }

        private void silenaToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CastSpell(Monitored.Party.GetPartyMembers()[playerOptionsSelected].Name, Spells.Silena);
        }

        private void virunaToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CastSpell(Monitored.Party.GetPartyMembers()[playerOptionsSelected].Name, Spells.Viruna);
        }        

        private void SandstormToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // TODO: Similar to haste/flurry, etc. add logic to deal with storm
            // tiers and only one at a time being selected.
            var name = Monitored.Party.GetPartyMembers()[autoOptionsSelected].Name;
            BuffEngine.ToggleAutoBuff(name, Spells.Sandstorm);
        }

        private void RainstormToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var name = Monitored.Party.GetPartyMembers()[autoOptionsSelected].Name;
            BuffEngine.ToggleAutoBuff(name, Spells.Rainstorm);
        }

        private void WindstormToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var name = Monitored.Party.GetPartyMembers()[autoOptionsSelected].Name;
            BuffEngine.ToggleAutoBuff(name, Spells.Windstorm);
        }

        private void FirestormToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var name = Monitored.Party.GetPartyMembers()[autoOptionsSelected].Name;
            BuffEngine.ToggleAutoBuff(name, Spells.Firestorm);
        }

        private void HailstormToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var name = Monitored.Party.GetPartyMembers()[autoOptionsSelected].Name;
            BuffEngine.ToggleAutoBuff(name, Spells.Hailstorm);
        }

        private void ThunderstormToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var name = Monitored.Party.GetPartyMembers()[autoOptionsSelected].Name;
            BuffEngine.ToggleAutoBuff(name, Spells.Thunderstorm);
        }

        private void VoidstormToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var name = Monitored.Party.GetPartyMembers()[autoOptionsSelected].Name;
            BuffEngine.ToggleAutoBuff(name, Spells.Voidstorm);
        }

        private void AurorastormToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var name = Monitored.Party.GetPartyMembers()[autoOptionsSelected].Name;
            BuffEngine.ToggleAutoBuff(name, Spells.Aurorastorm);
        }

        private void protectIVToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CastSpell(Monitored.Party.GetPartyMembers()[playerOptionsSelected].Name, Spells.Protect_IV);
        }

        private void protectVToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CastSpell(Monitored.Party.GetPartyMembers()[playerOptionsSelected].Name, Spells.Protect_V);
        }

        private void shellIVToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CastSpell(Monitored.Party.GetPartyMembers()[playerOptionsSelected].Name, Spells.Shell_IV);
        }

        private void shellVToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CastSpell(Monitored.Party.GetPartyMembers()[playerOptionsSelected].Name, Spells.Shell_V);
        }

        private void button3_Click(object sender, EventArgs e)
        {

            if (pauseActions == false)
            {
                pauseButton.Text = "Paused!";
                pauseButton.ForeColor = Color.Red;
                actionTimer.Enabled = false;
                ActiveBuffs.Clear();
                pauseActions = true;
                if (ConfigForm.config.FFXIDefaultAutoFollow == false)
                {
                    PL.AutoFollow.IsAutoFollowing = false;
                }
            }
            else
            {
                pauseButton.Text = "Pause";
                pauseButton.ForeColor = Color.Black;
                actionTimer.Enabled = true;
                pauseActions = false;

                if (ConfigForm.config.MinimiseonStart == true && WindowState != FormWindowState.Minimized)
                {
                    WindowState = FormWindowState.Minimized;
                }

                if (ConfigForm.config.EnableAddOn && LUA_Plugin_Loaded == 0)
                {
                    if (WindowerMode == "Windower")
                    {
                        PL.ThirdParty.SendString("//lua load CurePlease_addon");
                        Thread.Sleep(1500);
                        PL.ThirdParty.SendString("//cpaddon settings " + ConfigForm.config.ipAddress + " " + ConfigForm.config.listeningPort);
                        Thread.Sleep(100);
                        if (ConfigForm.config.enableHotKeys)
                        {
                            PL.ThirdParty.SendString("//bind ^!F1 cureplease toggle");
                            PL.ThirdParty.SendString("//bind ^!F2 cureplease start");
                            PL.ThirdParty.SendString("//bind ^!F3 cureplease pause");
                        }
                    }
                    else if (WindowerMode == "Ashita")
                    {
                        PL.ThirdParty.SendString("/addon load CurePlease_addon");
                        Thread.Sleep(1500);
                        PL.ThirdParty.SendString("/cpaddon settings " + ConfigForm.config.ipAddress + " " + ConfigForm.config.listeningPort);
                        Thread.Sleep(100);
                        if (ConfigForm.config.enableHotKeys)
                        {
                            PL.ThirdParty.SendString("/bind ^!F1 /cureplease toggle");
                            PL.ThirdParty.SendString("/bind ^!F2 /cureplease start");
                            PL.ThirdParty.SendString("/bind ^!F3 /cureplease pause");
                        }
                    }

                    AddOnStatus_Click(sender, e);


                    LUA_Plugin_Loaded = 1;


                }
            }
        }

        private void Debug_Click(object sender, EventArgs e)
        {
            if (Monitored == null)
            {

                MessageBox.Show("Attach to process before pressing this button", "Error");
                return;
            }

            MessageBox.Show(debug_MSG_show);
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (TopMost)
            {
                TopMost = false;
            }
            else
            {
                TopMost = true;
            }
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

            if (PL.Player.LoginStatus == (int)LoginStatus.Loading || Monitored.Player.LoginStatus == (int)LoginStatus.Loading)
            {
            }
            else
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
            notifyIcon1.Dispose();

            if (PL != null)
            {
                if (WindowerMode == "Ashita")
                {
                    PL.ThirdParty.SendString("/addon unload CurePlease_addon");
                    if (ConfigForm.config.enableHotKeys)
                    {
                        PL.ThirdParty.SendString("/unbind ^!F1");
                        PL.ThirdParty.SendString("/unbind ^!F2");
                        PL.ThirdParty.SendString("/unbind ^!F3");
                    }
                }
                else if (WindowerMode == "Windower")
                {
                    PL.ThirdParty.SendString("//lua unload CurePlease_addon");

                    if (ConfigForm.config.enableHotKeys)
                    {
                        PL.ThirdParty.SendString("//unbind ^!F1");
                        PL.ThirdParty.SendString("//unbind ^!F2");
                        PL.ThirdParty.SendString("//unbind ^!F3");
                    }

                }
            }

        }

        private int followID()
        {
            if ((setinstance2.Enabled == true) && !string.IsNullOrEmpty(ConfigForm.config.autoFollowName) && !pauseActions)
            {
                for (int x = 0; x < 2048; x++)
                {
                    XiEntity entity = PL.Entity.GetEntity(x);

                    if (entity.Name != null && entity.Name.ToLower().Equals(ConfigForm.config.autoFollowName.ToLower()))
                    {
                        return Convert.ToInt32(entity.TargetID);
                    }
                }
                return -1;
            }
            else
            {
                return -1;
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

        public bool plMonitoredSameParty()
        {
            int PLParty = PLPartyRelativeToMonitored();
            // I believe that the party from EliteAPI always has our player at index 0.
            // So if the PL is in party 1, it's the same party as Monitored.
            return PLParty == 1;
        }

        public int PLPartyRelativeToMonitored()
        {
            // FIRST CHECK THAT BOTH THE PL AND MONITORED PLAYER ARE IN THE SAME PT/ALLIANCE
            List<PartyMember> monitoredParty = Monitored.Party.GetPartyMembers();

            if (monitoredParty.Any(member => member.Name == PL.Player.Name))
            {
                int plParty = monitoredParty.FirstOrDefault(p => p.Name == PL.Player.Name).MemberNumber;

                if (plParty <= 5)
                {
                    return 1;
                }
                else if (plParty <= 11 && plParty >= 6)
                {
                    return 2;
                }
                else if (plParty <= 17 && plParty >= 12)
                {
                    return 3;
                }
            }

            return 0;
        }                          

        private int CheckEngagedStatus_Hate()
        {
            if (ConfigForm.config.AssistSpecifiedTarget == true && !string.IsNullOrEmpty(ConfigForm.config.autoTarget_Target))
            {
                for (int x = 0; x < 2048; x++)
                {
                    XiEntity z = PL.Entity.GetEntity(x);

                    if (!string.IsNullOrEmpty(z.Name) && z.Name.ToLower() == ConfigForm.config.autoTarget_Target.ToLower())
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
            else
            {
                if (Monitored.Player.Status == 1)
                {
                    TargetInfo target = Monitored.Target.GetTargetInfo();
                    XiEntity entity = Monitored.Entity.GetEntity(Convert.ToInt32(target.TargetIndex));
                    return Convert.ToInt32(entity.TargetID);

                }
                else
                {
                    return 0;
                }
            }
        }    

        private void updateInstances_Tick(object sender, EventArgs e)
        {
            if ((PL != null && PL.Player.LoginStatus == (int)LoginStatus.Loading) || (Monitored != null && Monitored.Player.LoginStatus == (int)LoginStatus.Loading))
            {
                return;
            }

            IEnumerable<Process> pol = Process.GetProcessesByName("pol").Union(Process.GetProcessesByName("xiloader")).Union(Process.GetProcessesByName("edenxi"));

            if (pol.Count() < 1)
            {
            }
            else
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

                    if (Monitored != null && Monitored.Player.Name != null)
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
            else if (FormWindowState.Normal == WindowState)
            {
            }
        }

        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            Show();
            WindowState = FormWindowState.Normal;
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
                                actionTimer.Enabled = false;
                                ActiveBuffs.Clear();
                                pauseActions = true;
                                if (ConfigForm.config.FFXIDefaultAutoFollow == false)
                                {
                                    PL.AutoFollow.IsAutoFollowing = false;
                                }
                            }
                            else if ((Monitored.ThirdParty.ConsoleGetArg(1) == "unpause" || Monitored.ThirdParty.ConsoleGetArg(1) == "start") && PL.Player.Name.ToLower() == Monitored.ThirdParty.ConsoleGetArg(2).ToLower())
                            {
                                pauseButton.Text = "Pause";
                                pauseButton.ForeColor = Color.Black;
                                actionTimer.Enabled = true;
                                pauseActions = false;
                            }
                            else if ((Monitored.ThirdParty.ConsoleGetArg(1) == "toggle") && PL.Player.Name.ToLower() == Monitored.ThirdParty.ConsoleGetArg(2).ToLower())
                            {
                                pauseButton.PerformClick();
                            }
                            else
                            {

                            }
                        }
                        else if (argCount < 3)
                        {
                            if (Monitored.ThirdParty.ConsoleGetArg(1) == "stop" || Monitored.ThirdParty.ConsoleGetArg(1) == "pause")
                            {
                                pauseButton.Text = "Paused!";
                                pauseButton.ForeColor = Color.Red;
                                actionTimer.Enabled = false;
                                ActiveBuffs.Clear();
                                pauseActions = true;
                                if (ConfigForm.config.FFXIDefaultAutoFollow == false)
                                {
                                    PL.AutoFollow.IsAutoFollowing = false;
                                }
                            }
                            else if (Monitored.ThirdParty.ConsoleGetArg(1) == "unpause" || Monitored.ThirdParty.ConsoleGetArg(1) == "start")
                            {
                                pauseButton.Text = "Pause";
                                pauseButton.ForeColor = Color.Black;
                                actionTimer.Enabled = true;
                                pauseActions = false;
                            }
                            else if (Monitored.ThirdParty.ConsoleGetArg(1) == "toggle")
                            {
                                pauseButton.PerformClick();
                            }
                            else
                            {
                            }
                        }
                        else
                        {
                            // DO NOTHING
                        }
                    }
                }
            }
        }

        private void Follow_BGW_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {

            // MAKE SURE BOTH ELITEAPI INSTANCES ARE ACTIVE, THE BOT ISN'T PAUSED, AND THERE IS AN AUTOFOLLOWTARGET NAMED
            if (PL != null && Monitored != null && !string.IsNullOrEmpty(ConfigForm.config.autoFollowName) && !pauseActions)
            {

                if (ConfigForm.config.FFXIDefaultAutoFollow != true)
                {
                    // CANCEL ALL PREVIOUS FOLLOW ACTIONS
                    PL.AutoFollow.IsAutoFollowing = false;
                    curePlease_autofollow = false;
                    stuckWarning = false;
                    stuckCount = 0;
                }

                // RUN THE FUNCTION TO GRAB THE ID OF THE FOLLOW TARGET THIS ALSO MAKES SURE THEY ARE IN RANGE TO FOLLOW
                int followersTargetID = followID();

                // If the FOLLOWER'S ID is NOT -1 THEN THEY WERE LOCATED SO CONTINUE THE CHECKS
                if (followersTargetID != -1)
                {
                    // GRAB THE FOLLOW TARGETS ENTITY TABLE TO CHECK DISTANCE ETC
                    XiEntity followTarget = PL.Entity.GetEntity(followersTargetID);

                    if (Math.Truncate(followTarget.Distance) >= (int)ConfigForm.config.autoFollowDistance && curePlease_autofollow == false)
                    {
                        // THE DISTANCE IS GREATER THAN REQUIRED SO IF AUTOFOLLOW IS NOT ACTIVE THEN DEPENDING ON THE TYPE, FOLLOW

                        // SQUARE ENIX FINAL FANTASY XI DEFAULT AUTO FOLLOW
                        if (ConfigForm.config.FFXIDefaultAutoFollow == true && PL.AutoFollow.IsAutoFollowing != true)
                        {
                            // IF THE CURRENT TARGET IS NOT THE FOLLOWERS TARGET ID THEN CHANGE THAT NOW
                            if (PL.Target.GetTargetInfo().TargetIndex != followersTargetID)
                            {
                                // FIRST REMOVE THE CURRENT TARGET
                                PL.Target.SetTarget(0);
                                // NOW SET THE NEXT TARGET AFTER A WAIT
                                Thread.Sleep(TimeSpan.FromSeconds(0.1));
                                PL.Target.SetTarget(followersTargetID);
                            }
                            // IF THE TARGET IS CORRECT BUT YOU'RE NOT LOCKED ON THEN DO SO NOW
                            else if (PL.Target.GetTargetInfo().TargetIndex == followersTargetID && !PL.Target.GetTargetInfo().LockedOn)
                            {
                                PL.ThirdParty.SendString("/lockon <t>");
                            }
                            // EVERYTHING SHOULD BE FINE SO FOLLOW THEM
                            else
                            {
                                Thread.Sleep(TimeSpan.FromSeconds(0.1));
                                PL.ThirdParty.SendString("/follow");
                            }
                        }
                        // ELITEAPI'S IMPROVED AUTO FOLLOW
                        else if (ConfigForm.config.FFXIDefaultAutoFollow != true && PL.AutoFollow.IsAutoFollowing != true)
                        {
                            // IF YOU ARE TOO FAR TO FOLLOW THEN STOP AND IF ENABLED WARN THE MONITORED PLAYER
                            if (ConfigForm.config.autoFollow_Warning == true && Math.Truncate(followTarget.Distance) >= 40 && Monitored.Player.Name != PL.Player.Name && followWarning == 0)
                            {
                                string createdTell = "/tell " + Monitored.Player.Name + " " + "You're too far to follow.";
                                PL.ThirdParty.SendString(createdTell);
                                followWarning = 1;
                                Thread.Sleep(TimeSpan.FromSeconds(0.3));
                            }
                            else if (Math.Truncate(followTarget.Distance) <= 40)
                            {
                                // ONLY TARGET AND BEGIN FOLLOW IF TARGET IS AT THE DEFINED DISTANCE
                                if (Math.Truncate(followTarget.Distance) >= (int)ConfigForm.config.autoFollowDistance && Math.Truncate(followTarget.Distance) <= 48)
                                {
                                    followWarning = 0;

                                    // Cancel current target this is to make sure the character is not locked
                                    // on and therefore unable to move freely. Wait 5ms just to allow it to work

                                    PL.Target.SetTarget(0);
                                    Thread.Sleep(TimeSpan.FromSeconds(0.1));

                                    float Target_X;
                                    float Target_Y;
                                    float Target_Z;

                                    XiEntity FollowerTargetEntity = PL.Entity.GetEntity(followersTargetID);

                                    if (!string.IsNullOrEmpty(FollowerTargetEntity.Name))
                                    {
                                        while (Math.Truncate(followTarget.Distance) >= (int)ConfigForm.config.autoFollowDistance)
                                        {

                                            float Player_X = PL.Player.X;
                                            float Player_Y = PL.Player.Y;
                                            float Player_Z = PL.Player.Z;


                                            if (FollowerTargetEntity.Name == Monitored.Player.Name)
                                            {
                                                Target_X = Monitored.Player.X;
                                                Target_Y = Monitored.Player.Y;
                                                Target_Z = Monitored.Player.Z;
                                                float dX = Target_X - Player_X;
                                                float dY = Target_Y - Player_Y;
                                                float dZ = Target_Z - Player_Z;

                                                PL.AutoFollow.SetAutoFollowCoords(dX, dY, dZ);

                                                PL.AutoFollow.IsAutoFollowing = true;
                                                curePlease_autofollow = true;


                                                lastX = PL.Player.X;
                                                lastY = PL.Player.Y;
                                                lastZ = PL.Player.Z;

                                                Thread.Sleep(TimeSpan.FromSeconds(0.1));
                                            }
                                            else
                                            {
                                                Target_X = FollowerTargetEntity.X;
                                                Target_Y = FollowerTargetEntity.Y;
                                                Target_Z = FollowerTargetEntity.Z;

                                                float dX = Target_X - Player_X;
                                                float dY = Target_Y - Player_Y;
                                                float dZ = Target_Z - Player_Z;


                                                PL.AutoFollow.SetAutoFollowCoords(dX, dY, dZ);

                                                PL.AutoFollow.IsAutoFollowing = true;
                                                curePlease_autofollow = true;


                                                lastX = PL.Player.X;
                                                lastY = PL.Player.Y;
                                                lastZ = PL.Player.Z;

                                                Thread.Sleep(TimeSpan.FromSeconds(0.1));
                                            }

                                            // STUCK CHECKER
                                            float genX = lastX - PL.Player.X;
                                            float genY = lastY - PL.Player.Y;
                                            float genZ = lastZ - PL.Player.Z;

                                            double distance = Math.Sqrt(genX * genX + genY * genY + genZ * genZ);

                                            if (distance < .1)
                                            {
                                                stuckCount = stuckCount + 1;
                                                if (ConfigForm.config.autoFollow_Warning == true && stuckWarning != true && FollowerTargetEntity.Name == Monitored.Player.Name && stuckCount == 10)
                                                {
                                                    string createdTell = "/tell " + Monitored.Player.Name + " " + "I appear to be stuck.";
                                                    PL.ThirdParty.SendString(createdTell);
                                                    stuckWarning = true;
                                                }
                                            }
                                        }

                                        PL.AutoFollow.IsAutoFollowing = false;
                                        curePlease_autofollow = false;
                                        stuckWarning = false;
                                        stuckCount = 0;
                                    }
                                }
                            }
                            else
                            {
                                // YOU ARE NOT AT NOR FURTHER THAN THE DISTANCE REQUIRED SO CANCEL ELITEAPI AUTOFOLLOW
                                curePlease_autofollow = false;
                            }
                        }
                    }
                }
            }

            Thread.Sleep(TimeSpan.FromSeconds(1));

        }

        private void Follow_BGW_RunWorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            Follow_BGW.RunWorkerAsync();
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

        private void AddonReader_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            if (ConfigForm.config.EnableAddOn && !pauseActions && Monitored != null && PL != null)
            {

                bool done = false;

                UdpClient listener = new UdpClient(Convert.ToInt32(ConfigForm.config.listeningPort));
                IPEndPoint groupEP = new IPEndPoint(IPAddress.Parse(ConfigForm.config.ipAddress), Convert.ToInt32(ConfigForm.config.listeningPort));
                string received_data;
                byte[] receive_byte_array;
                try
                {
                    while (!done)
                    {

                        receive_byte_array = listener.Receive(ref groupEP);

                        received_data = Encoding.ASCII.GetString(receive_byte_array, 0, receive_byte_array.Length);

                        string[] commands = received_data.Split('_');

                        // MessageBox.Show(commands[1] + " " + commands[2]);
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
                            }
                            else if (commands[2] == "interrupted")
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
                                    currentAction.Text = string.Empty;
                                    castingSpell = string.Empty;
                                    CastingBackground_Check = false;
                                }));
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
                                }));
                            }
                            if (commands[2] == "stop" || commands[2] == "pause")
                            {
                                Invoke((MethodInvoker)(() =>
                                {

                                    pauseButton.Text = "Paused!";
                                    pauseButton.ForeColor = Color.Red;
                                    actionTimer.Enabled = false;
                                    ActiveBuffs.Clear();
                                    pauseActions = true;
                                    if (ConfigForm.config.FFXIDefaultAutoFollow == false)
                                    {
                                        PL.AutoFollow.IsAutoFollowing = false;
                                    }

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
                                    ActiveBuffs[memberName] = memberBuffs.Split(',').Select(str => short.Parse(str.Trim()));
                                }                             
                            }

                        }
                    }
                }
                catch (Exception error1)
                {
                    Console.WriteLine(error1.StackTrace);
                }

                listener.Close();

            }

            Thread.Sleep(TimeSpan.FromSeconds(0.2));
        }

        private void AddonReader_RunWorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            AddonReader.RunWorkerAsync();
        }       

        private void AddOnStatus_Click(object sender, EventArgs e)
        {
            if (Monitored != null && PL != null)
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
