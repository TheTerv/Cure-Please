using CurePlease.Engine;
using CurePlease.Model;
using CurePlease.Model.Config;
using EliteMMO.API;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Xml.Serialization;
using static CurePlease.Model.JobUtils;
using Keys = System.Windows.Forms.Keys;

namespace CurePlease
{
    public partial class ConfigForm : Form
    {
        #region "== Form2"

        public static MySettings Config = new();
        
        public int runOnce = 0;

        public ConfigForm ( )
        {
            StartPosition = FormStartPosition.CenterScreen;

            InitializeComponent();

            if (Config.settingsSet != true)
            {
                LoadConfiguration();
                Config.settingsSet = true;
            }

            UpdateForm(Config);
        }

        public static void LoadConfiguration()
        {
            var loadedSettings = ReadConfigurationFromFile();

            Config = loadedSettings ?? LoadDefaultConfig();
        }

        public SongConfig GetSongConfig()
        {
            return new SongConfig()
            {
                SingingEnabled = enableSinging.Checked,
                SingOnlyWhenNear = SongsOnlyWhenNearEngaged.Checked,
                SongRecastMinutes = recastSong.Value,
                Dummy1 = dummy1.SelectedIndex,
                Dummy2 = dummy2.SelectedIndex,
                Song1 = song1.SelectedIndex,
                Song2 = song2.SelectedIndex,
                Song3 = song3.SelectedIndex,
                Song4 = song4.SelectedIndex,
                TroubadourEnabled = Config.Troubadour,
                NightingaleEnabled = Config.Nightingale
            };
        }

        public static GeoConfig GetGeoConfig()
        {
            return new GeoConfig()
            {
                IndiSpellsEnabled = Config.EnableIndiSpells,
                GeoWhenEngaged = Config.GeoWhenEngaged,
                RadialArcanaEnabled = Config.RadialArcana,
                RadialArcanaMP = Config.RadialArcanaMP,
                RadialArcanaSpell = Config.RadialArcana_Spell,
                GeoSpell = Config.GeoSpell_Spell,
                FullCircleEnabled = Config.FullCircle,
                FullCircleGeoTarget = Config.Fullcircle_GEOTarget,
                FullCircleDisableEnemy = Config.Fullcircle_DisableEnemy,
                LuopanSpellsEnabled = Config.EnableLuopanSpells,
                LuopanSpellTarget = Config.LuopanSpell_Target,
                SpecifiedEngageTarget = Config.specifiedEngageTarget,
                EntrustEnabled = Config.EntrustEnabled,
                EntrustSpell = Config.EntrustedSpell_Spell,
                EntrustSpellTarget = Config.EntrustedSpell_Target,
                IndiWhenEngaged = Config.EngagedOnly,
                DematerializeEnabled = Config.Dematerialize,
                EclipticAttritionEnabled = Config.EclipticAttrition,
                LifeCycleEnabled = Config.LifeCycle,
                BlazeOfGloryEnabled = Config.BlazeOfGlory,
                IndiSpell = Config.IndiSpell_Spell
            };
        }

        public static BuffConfig GetBuffConfig()
        {
            return new BuffConfig()
            {
                AddonIP = Config.ipAddress,
                AddonPort = Config.listeningPort
            };
        }

        public DebuffConfig GetDebuffConfig()
        {
            return new DebuffConfig()
            {
                AddonIP = Config.ipAddress,
                AddonPort = Config.listeningPort,
                PLDebuffEnabled = Config.plDebuffEnabled,
                MonitoredDebuffEnabled = Config.monitoredDebuffEnabled,
                PartyDebuffEnabled = Config.enablePartyDebuffRemoval,
                OnlySpecificMembers = Config.SpecifiednaSpellsenable,
                DebuffEnabled = DebuffEnabled,
                PrioritizeOverLowerCures = Config.PrioritiseOverLowerTier
            };
        }

        public static PLConfig GetPLConfig()
        {
            return new PLConfig()
            {
                PLSilenceItemEnabled = Config.plSilenceItemEnabled,
                PLSilenceItem = Config.plSilenceItem,
                PLDoomItemEnabled = Config.plDoomEnabled,
                PLDoomItem = Config.plDoomitem,
                DivineSeal = Config.DivineSeal,
                Convert = Config.Convert,
                ConvertMP = Config.convertMP,
                MinCastingMP = Config.mpMinCastValue,
                LowMPEnabled = Config.lowMPcheckBox,
                HealLowMPEnabled = Config.healLowMP,
                HealMPThreshold = Config.healWhenMPBelow,
                StandMPEnabled = Config.standAtMP,
                StandMPThreshold = Config.standAtMP_Percentage,
                BarElementEnabled = Config.plBarElement,
                AOEBarElementEnabled = Config.AOE_Barelemental,
                BarElementSpell = Config.AOE_Barelemental ? Data.AoeBarSpells[Config.plBarElement_Spell] : Data.BarSpells[Config.plBarElement_Spell],
                BarStatusEnabled = Config.plBarStatus,
                AOEBarStatusEnabled = Config.AOE_Barstatus,
                BarStatusSpell = Config.AOE_Barstatus ? Data.AoeBarStatus[Config.plBarStatus_Spell] : Data.BarStatus[Config.plBarStatus_Spell],
                EnSpellEnabled = Config.plEnspell,
                EnSpell = Data.Enspells[Config.plEnspell_Spell],
                EnspellAccession = Config.enspellAccession,
                EnspellPerpetuance = Config.enspellPerpetuance,
                StormSpellEnabled = Config.plStormSpell,
                StormSpell = Data.StormTiers[Config.plStormSpell_Spell],
                GainBoostSpellEnabled = Config.plGainBoost,
                GainBoostSpell = Data.GainBoostSpells[Config.plGainBoost_Spell],
                Composure = Config.Composure,
                LightArts = Config.LightArts,
                AddendumWhite = Config.AddendumWhite,
                DarkArts = Config.DarkArts,
                AddendumBlack = Config.AddendumBlack,
                ShellraEnabled = Config.plShellra,
                ShellraLevel = (int)Config.plShellra_Level,
                ProtectraEnabled = Config.plProtectra,
                ProtectraLevel = (int)Config.plProtectra_Level,
                AccessionEnabled = Config.Accession,
                BarElementAccession = Config.barspellAccession,
                BarStatusAccession = Config.barstatusAccession,
                PerpetuanceEnabled = Config.Perpetuance,
                BarElemenetPerpetuance = Config.barspellPerpetuance,
                BarStatusPerpetuance = Config.barstatusPerpetuance,
                StormspellAccession = Config.stormspellAccession,
                StormspellPerpetuance = Config.stormspellPerpetuance,
                ProtectEnabled = Config.plProtect,
                ProtectSpell = Data.ProtectTiers[Config.autoProtect_Spell],
                ShellEnabled = Config.plShell,
                ShellSpell = Data.ShellTiers[Config.autoShell_Spell],
                AccessionProtectShell = Config.accessionProShell,
                ReraiseEnabled = Config.plReraise,
                ReraiseSpell = Config.plReraise_Level > 0 ? Data.ReraiseTiers[Config.plReraise_Level - 1] : Data.ReraiseTiers[0],
                EnlightenmentReraise = Config.EnlightenmentReraise,
                UtsusemiEnabled = Config.plUtsusemi,
                BlinkEnabled = Config.plBlink,
                BlinkAccession = Config.blinkAccession,
                BlinkPerpetuance = Config.blinkPerpetuance,
                PhalanxEnabled = Config.plPhalanx,
                PhalanxAccession = Config.phalanxAccession,
                PhalanxPerpetuance = Config.phalanxPerpetuance,
                RefreshEnabled = Config.plRefresh,
                RefreshSpell = Config.plRefresh_Level > 0 ? Data.RefreshTiers[Config.plRefresh_Level - 1] : Data.RefreshTiers[0],
                RefreshAccession = Config.refreshAccession,
                RefreshPerpetuance = Config.refreshPerpetuance,
                RegenEnabled = Config.plRegen,
                RegenSpell = Config.plRegen_Level > 0 ? Data.RegenTiers[Config.plRegen_Level - 1] : Data.RegenTiers[0],
                RegenAccession = Config.regenAccession,
                RegenPerpetuance = Config.regenPerpetuance,
                AdloquiumEnabled = Config.plAdloquium,
                AdloquiumAccession = Config.adloquiumAccession,
                AdloquiumPerpetuance = Config.adloquiumPerpetuance,
                StoneskinEnabled = Config.plStoneskin,
                StoneskinAccession = Config.stoneskinAccession,
                StoneskinPerpetuance = Config.stoneskinPerpetuance,
                AquaveilEnabled = Config.plAquaveil,
                AquaveilAccession = Config.aquaveilAccession,
                AquaveilPerpetuance = Config.aquaveilPerpetuance,
                KlimaformEnabled = Config.plKlimaform,
                TemperEnabled = Config.plTemper,
                TemperSpell = Config.plTemper_Level > 0 ? Data.TemperTiers[Config.plTemper_Level - 1] : Data.TemperTiers[0],
                HasteEnabled = Config.plHaste,
                HasteSpell = Config.plHaste_Level > 0 ? Data.HasteTiers[Config.plHaste_Level - 1] : Data.HasteTiers[0],
                SpikesEnabled = Config.plSpikes,
                SpikesSpell = Data.SpikesSpells[Config.plSpikes_Spell],
                AuspiceEnabled = Config.plAuspice,
                AutoTargetEnabled = Config.autoTarget,
                AutoTargetSpell = Config.autoTargetSpell,
                AutoTargetTarget = Config.autoTarget_Target,
                AssistSpecifiedTarget = Config.AssistSpecifiedTarget,
                PartyBasedHateSpell = Config.Hate_SpellType == 1,
                AfflatusSolaceEnabled = Config.AfflatusSolace,
                AfflatusMiseryEnabled = Config.AfflatusMisery,
                SublimationEnabled = Config.Sublimation,
                SublimationMPLossThreshold = Config.sublimationMP,
                DivineCaressEnabled = Config.DivineCaress,
                DebuffsEnabled = (Config.plDebuffEnabled || Config.monitoredDebuffEnabled || Config.enablePartyDebuffRemoval),
                DevotionEnabled = Config.Devotion,
                DevotionWhenEngaged = Config.DevotionWhenEngaged,
                DevotionMPThreshold = Config.DevotionMP,
                DevotionSpecifiedTarget = Config.DevotionTargetType == 0,
                DevotionTargetName = Config.DevotionTargetName
            };
        }

        public CureConfig GetCureConfig()
        {
            return new CureConfig()
            {
                EnabledCureTiers = new bool[] { Config.cure1enabled, Config.cure2enabled, Config.cure3enabled, Config.cure4enabled, Config.cure5enabled, Config.cure6enabled },
                CureTierThresholds = new int[] { Config.cure1amount, Config.cure2amount, Config.cure3amount, Config.cure4amount, Config.cure5amount, Config.cure6amount },
                EnabledCuragaTiers = new bool[] { Config.curagaEnabled, Config.curaga2enabled, Config.curaga3enabled, Config.curaga4enabled, Config.curaga5enabled },
                CuragaTierThresholds = new int[] { Config.curagaAmount, Config.curaga2Amount, Config.curaga3Amount, Config.curaga4Amount, Config.curaga5Amount },
                EnableOutOfPartyHealing = Config.enableOutOfPartyHealing,
                MonitoredCurePercentage = Config.monitoredCurePercentage,
                CuragaMinPlayers = (int)Config.curagaRequiredMembers,
                CuragaHealthPercent = curagaCurePercentage.Value,
                CureHealthPercent = curePercentage.Value,
                MonitoredPriorityEnabled = Config.enableMonitoredPriority,
                OverCureEnabled = Config.Overcure,
                UnderCureEnabled = Config.Undercure,
                CuragaSpecifiedEnabled = Config.curagaTargetType != 0,
                CuragaSpecifiedName = Config.curagaTargetName
            };
        }

        private static MySettings ReadConfigurationFromFile()
        {
            string fileName = CreateFileName();
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Settings", fileName);
            return ReadSettings(path);
        }

        private static MySettings LoadDefaultConfig()
        {
            #region Healing Defaults

            // HEALING MAGIC
            Config.cure1enabled = false;
            Config.cure2enabled = false;
            Config.cure3enabled = true;
            Config.cure4enabled = true;
            Config.cure5enabled = true;
            Config.cure6enabled = true;
            Config.cure1amount = 10;
            Config.cure2amount = 60;
            Config.cure3amount = 130;
            Config.cure4amount = 270;
            Config.cure5amount = 450;
            Config.cure6amount = 600;
            Config.curePercentage = 75;
            Config.monitoredCurePercentage = 85;
            Config.priorityCurePercentage = 95;

            Config.curagaEnabled = false;
            Config.curaga2enabled = false;
            Config.curaga3enabled = false;
            Config.curaga4enabled = false;
            Config.curaga5enabled = false;
            Config.curagaAmount = 20;
            Config.curaga2Amount = 70;
            Config.curaga3Amount = 165;
            Config.curaga4Amount = 330;
            Config.curaga5Amount = 570;
            Config.curagaCurePercentage = 75;
            Config.curagaTargetType = 0;
            Config.curagaTargetName = "";
            Config.curagaRequiredMembers = 3;

            #endregion

            #region Enhancing Defaults

            // ENHANCING MAGIC

            // BASIC ENHANCING
            Config.autoHasteMinutes = 2;
            Config.autoAdloquiumMinutes = 2;
            Config.autoProtect_Minutes = 29;
            Config.autoShellMinutes = 29;
            Config.autoPhalanxIIMinutes = 2;
            Config.autoStormspellMinutes = 3;
            Config.autoRefresh_Minutes = 2;
            Config.autoRegen_Minutes = 1;
            Config.autoRefresh_Minutes = 2;
            Config.plProtect = false;
            Config.plShell = false;
            Config.plBlink = false;
            Config.plReraise = false;
            Config.autoRegen_Spell = 3;
            Config.autoRefresh_Spell = 1;
            Config.autoShell_Spell = 4;
            Config.autoStorm_Spell = 0;
            Config.autoProtect_Spell = 4;
            Config.plRegen = false;
            Config.plRegen_Level = 4;
            Config.plReraise = false;
            Config.plReraise_Level = 2;
            Config.plRefresh = false;
            Config.plRefresh_Level = 2;
            Config.plStoneskin = false;
            Config.plPhalanx = false;
            Config.plProtectra = false;
            Config.plShellra = false;
            Config.plProtectra_Level = 5;
            Config.plShellra_Level = 5;
            Config.plTemper = false;
            Config.plTemper_Level = 0;
            Config.plEnspell = false;
            Config.plEnspell_Spell = 0;
            Config.plGainBoost = false;
            Config.plGainBoost_Spell = 0;
            Config.plBarElement = false;
            Config.plBarElement_Spell = 0;
            Config.AOE_Barelemental = false;
            Config.plBarStatus = false;
            Config.plBarStatus_Spell = 0;
            Config.AOE_Barstatus = false;
            Config.plStormSpell = false;
            Config.plAdloquium = false;
            Config.plKlimaform = false;
            Config.plStormSpell_Spell = 0;
            Config.plAuspice = false;
            Config.plAquaveil = false;
            Config.plHaste = false;
            Config.plHaste_Level = 0;
            Config.plSpikes = false;
            Config.plSpikes_Spell = 0;

            Config.plUtsusemi = false;

            #endregion

            #region SCH Defaults

            // SCHOLAR STRATAGEMS
            Config.AccessionRegen = false;
            Config.PerpetuanceRegen = false;
            Config.accessionCure = false;
            Config.accessionProShell = false;

            Config.regenPerpetuance = false;
            Config.regenAccession = false;
            Config.refreshPerpetuance = false;
            Config.refreshAccession = false;
            Config.blinkPerpetuance = false;
            Config.blinkAccession = false;
            Config.phalanxPerpetuance = false;
            Config.phalanxAccession = false;
            Config.stoneskinPerpetuance = false;
            Config.stoneskinAccession = false;
            Config.enspellPerpetuance = false;
            Config.enspellAccession = false;
            Config.stormspellPerpetuance = false;
            Config.stormspellAccession = false;
            Config.adloquiumPerpetuance = false;
            Config.adloquiumAccession = false;
            Config.aquaveilPerpetuance = false;
            Config.aquaveilAccession = false;
            Config.barspellPerpetuance = false;
            Config.barspellAccession = false;
            Config.barstatusPerpetuance = false;
            Config.barstatusAccession = false;

            Config.EnlightenmentReraise = false;

            #endregion

            // GEOMANCER
            Config.EnableIndiSpells = false;
            Config.GeoWhenEngaged = false;
            Config.GeoSpell_Spell = 0;
            Config.LuopanSpell_Target = "";
            Config.IndiSpell_Spell = 0;
            Config.EntrustedSpell_Spell = 0;
            Config.EntrustedSpell_Target = "";
            Config.EnableLuopanSpells = false;
            Config.GeoWhenEngaged = false;

            Config.specifiedEngageTarget = false;

            // SINGING
            Config.enableSinging = false;
            Config.song1 = 0;
            Config.song2 = 0;
            Config.song3 = 0;
            Config.song4 = 0;
            Config.dummy1 = 0;
            Config.dummy2 = 0;
            Config.recastSongTime = 2;
            Config.enableSinging = false;
            Config.recastSongs_Monitored = false;
            Config.SongsOnlyWhenNear = false;

            // JOB ABILITIES
            Config.AfflatusSolace = false;
            Config.AfflatusMisery = false;
            Config.LightArts = false;
            Config.Composure = false;
            Config.Convert = false;
            Config.DivineSeal = false;
            Config.AddendumWhite = false;
            Config.Sublimation = false;
            Config.Celerity = false;
            Config.Accession = false;
            Config.Perpetuance = false;
            Config.Penury = false;
            Config.Rapture = false;
            Config.EclipticAttrition = false;
            Config.LifeCycle = false;
            Config.EntrustEnabled = false;
            Config.Dematerialize = false;
            Config.FullCircle = false;
            Config.BlazeOfGlory = false;
            Config.RadialArcana = false;
            Config.Troubadour = false;
            Config.Nightingale = false;
            Config.Marcato = false;
            Config.Devotion = false;
            Config.DivineCaress = false;
            Config.DarkArts = false;
            Config.AddendumBlack = false;

            #region Debuffs

            // DEBUFF REMOVAL
            Config.plSilenceItemEnabled = false;
            Config.plSilenceItem = 0;
            Config.wakeSleepEnabled = false;
            Config.wakeSleepSpell = 2;
            Config.plDoomEnabled = false;
            Config.plDoomitem = 0;

            Config.plDebuffEnabled = false;
            Config.plAgiDown = false;
            Config.plAccuracyDown = false;
            Config.plAddle = false;
            Config.plAttackDown = false;
            Config.plBane = false;
            Config.plBind = false;
            Config.plBio = false;
            Config.plBlindness = false;
            Config.plBurn = false;
            Config.plChrDown = false;
            Config.plChoke = false;
            Config.plCurse = false;
            Config.plCurse2 = false;
            Config.plDexDown = false;
            Config.plDefenseDown = false;
            Config.plDia = false;
            Config.plDisease = false;
            Config.plDoom = false;
            Config.plDrown = false;
            Config.plElegy = false;
            Config.plEvasionDown = false;
            Config.plFlash = false;
            Config.plFrost = false;
            Config.plHelix = false;
            Config.plIntDown = false;
            Config.plMndDown = false;
            Config.plMagicAccDown = false;
            Config.plMagicAtkDown = false;
            Config.plMaxHpDown = false;
            Config.plMaxMpDown = false;
            Config.plMaxTpDown = false;
            Config.plParalysis = false;
            Config.plPlague = false;
            Config.plPoison = false;
            Config.plRasp = false;
            Config.plRequiem = false;
            Config.plStrDown = false;
            Config.plShock = false;
            Config.plSilence = false;
            Config.plSlow = false;
            Config.plThrenody = false;
            Config.plVitDown = false;
            Config.plWeight = false;
            Config.plAmnesia = false;

            Config.enablePartyDebuffRemoval = false;
            Config.SpecifiednaSpellsenable = false;
            Config.naBlindness = false;
            Config.naCurse = false;
            Config.naDisease = false;
            Config.naParalysis = false;
            Config.naPetrification = false;
            Config.naPlague = false;
            Config.naPoison = false;
            Config.naSilence = false;
            Config.naErase = false;
            Config.Esuna = false;
            Config.EsunaOnlyAmnesia = false;

            Config.PrioritiseOverLowerTier = false;

            Config.monitoredDebuffEnabled = false;
            Config.monitoredAgiDown = false;
            Config.monitoredAccuracyDown = false;
            Config.monitoredAddle = false;
            Config.monitoredAttackDown = false;
            Config.monitoredBane = false;
            Config.monitoredBind = false;
            Config.monitoredBio = false;
            Config.monitoredBlindness = false;
            Config.monitoredBurn = false;
            Config.monitoredChrDown = false;
            Config.monitoredChoke = false;
            Config.monitoredCurse = false;
            Config.monitoredCurse2 = false;
            Config.monitoredDexDown = false;
            Config.monitoredDefenseDown = false;
            Config.monitoredDia = false;
            Config.monitoredDisease = false;
            Config.monitoredDoom = false;
            Config.monitoredDrown = false;
            Config.monitoredElegy = false;
            Config.monitoredEvasionDown = false;
            Config.monitoredFlash = false;
            Config.monitoredFrost = false;
            Config.monitoredHelix = false;
            Config.monitoredIntDown = false;
            Config.monitoredMndDown = false;
            Config.monitoredMagicAccDown = false;
            Config.monitoredMagicAtkDown = false;
            Config.monitoredMaxHpDown = false;
            Config.monitoredMaxMpDown = false;
            Config.monitoredMaxTpDown = false;
            Config.monitoredParalysis = false;
            Config.monitoredPetrification = false;
            Config.monitoredPlague = false;
            Config.monitoredPoison = false;
            Config.monitoredRasp = false;
            Config.monitoredRequiem = false;
            Config.monitoredStrDown = false;
            Config.monitoredShock = false;
            Config.monitoredSilence = false;
            Config.monitoredSleep = false;
            Config.monitoredSleep2 = false;
            Config.monitoredSlow = false;
            Config.monitoredThrenody = false;
            Config.monitoredVitDown = false;
            Config.monitoredWeight = false;
            Config.monitoredAmnesia = false;

            Config.na_Weight = false;
            Config.na_VitDown = false;
            Config.na_Threnody = false;
            Config.na_Slow = false;
            Config.na_Shock = false;
            Config.na_StrDown = false;
            Config.na_Requiem = false;
            Config.na_Rasp = false;
            Config.na_MaxTpDown = false;
            Config.na_MaxMpDown = false;
            Config.na_MaxHpDown = false;
            Config.na_MagicAttackDown = false;
            Config.na_MagicDefenseDown = false;
            Config.na_MagicAccDown = false;
            Config.na_MndDown = false;
            Config.na_IntDown = false;
            Config.na_Helix = false;
            Config.na_Frost = false;
            Config.na_EvasionDown = false;
            Config.na_Elegy = false;
            Config.na_Drown = false;
            Config.na_Dia = false;
            Config.na_DefenseDown = false;
            Config.na_DexDown = false;
            Config.na_Choke = false;
            Config.na_ChrDown = false;
            Config.na_Burn = false;
            Config.na_Bio = false;
            Config.na_Bind = false;
            Config.na_AttackDown = false;
            Config.na_Addle = false;
            Config.na_AccuracyDown = false;
            Config.na_AgiDown = false;

            #endregion

            #region Other

            // OTHER OPTIONS

            Config.lowMPcheckBox = false;
            Config.mpMinCastValue = 100;

            Config.autoTarget = false;
            Config.autoTargetSpell = "Dia";
            Config.AssistSpecifiedTarget = false;

            Config.DisableTargettingCancel = false;
            Config.TargetRemoval_Delay = 3;

            Config.AcceptRaise = false;
            Config.AcceptRaiseOnlyWhenNotInCombat = false;

            Config.Fullcircle_DisableEnemy = false;
            Config.Fullcircle_GEOTarget = false;

            Config.RadialArcana_Spell = 0;
            Config.RadialArcanaMP = 300;

            Config.convertMP = 300;

            Config.DevotionMP = 200;

            Config.DevotionTargetType = 1;
            Config.DevotionTargetName = "";
            Config.DevotionWhenEngaged = false;

            Config.Hate_SpellType = 0;
            Config.autoTarget_Target = "";

            Config.healWhenMPBelow = 5;
            Config.healLowMP = false;

            Config.standAtMP_Percentage = 99;
            Config.standAtMP = false;

            Config.Overcure = true;
            Config.Undercure = true;
            Config.enableMonitoredPriority = false;
            Config.enableOutOfPartyHealing = true;
            Config.OvercureOnHighPriority = false;

            Config.EnableAddOn = false;

            Config.sublimationMP = 100;

            #endregion

            // PROGRAM OPTIONS

            Config.pauseOnZoneBox = false;
            Config.pauseOnStartBox = false;
            Config.pauseOnKO = false;
            Config.MinimiseonStart = false;

            Config.autoFollowName = "";
            Config.autoFollowDistance = 5;
            Config.autoFollow_Warning = false;
            Config.FFXIDefaultAutoFollow = false;
            Config.enableHotKeys = false;

            Config.ipAddress = "127.0.0.1";
            Config.listeningPort = "19769";

            Config.enableFastCast_Mode = false;
            Config.trackCastingPackets = false;

            return Config;
        }

        private static MySettings ReadSettings(string filePath)
        {
            TextReader reader = null;
            try
            {
                var serializer = new XmlSerializer(typeof(MySettings));
                reader = new StreamReader(filePath);
                return (MySettings)serializer.Deserialize(reader);
            }
            catch(Exception)
            {
                return null;
            }
            finally
            {
                if (reader != null)
                    reader.Close();
            }
        }

        #endregion "== Form2"

        #region "== Cure Percentage's Changed"

        private void CurePercentage_ValueChanged ( object sender, EventArgs e )
        {
            curePercentageValueLabel.Text = curePercentage.Value.ToString ( );
        }

        private void PriorityCurePercentage_ValueChanged ( object sender, EventArgs e )
        {
            priorityCurePercentageValueLabel.Text = priorityCurePercentage.Value.ToString ( );
        }

        private void CuragaPercentage_ValueChanged ( object sender, EventArgs e )
        {
            curagaPercentageValueLabel.Text = curagaCurePercentage.Value.ToString ( );
        }

        private void MonitoredPercentage_ValueChanged ( object sender, EventArgs e )
        {
            monitoredCurePercentageValueLabel.Text = monitoredCurePercentage.Value.ToString ( );
        }

        #endregion "== Cure Percentage's Changed"

        #region "== All Settings Saved"

        private void SaveAllSettings_Click ( object sender, EventArgs e )
        {
            FollowEngine.ClearFollowing();

            // HEALING MAGIC
            Config.cure1enabled = cure1enabled.Checked;
            Config.cure2enabled = cure2enabled.Checked;
            Config.cure3enabled = cure3enabled.Checked;
            Config.cure4enabled = cure4enabled.Checked;
            Config.cure5enabled = cure5enabled.Checked;
            Config.cure6enabled = cure6enabled.Checked;
            Config.cure1amount = Convert.ToInt32 ( cure1amount.Value );
            Config.cure2amount = Convert.ToInt32 ( cure2amount.Value );
            Config.cure3amount = Convert.ToInt32 ( cure3amount.Value );
            Config.cure4amount = Convert.ToInt32 ( cure4amount.Value );
            Config.cure5amount = Convert.ToInt32 ( cure5amount.Value );
            Config.cure6amount = Convert.ToInt32 ( cure6amount.Value );
            Config.curePercentage = curePercentage.Value;
            Config.priorityCurePercentage = priorityCurePercentage.Value;
            Config.monitoredCurePercentage = monitoredCurePercentage.Value;

            Config.curagaEnabled = curagaEnabled.Checked;
            Config.curaga2enabled = curaga2Enabled.Checked;
            Config.curaga3enabled = curaga3Enabled.Checked;
            Config.curaga4enabled = curaga4Enabled.Checked;
            Config.curaga5enabled = curaga5Enabled.Checked;
            Config.curagaAmount = Convert.ToInt32 ( curagaAmount.Value );
            Config.curaga2Amount = Convert.ToInt32 ( curaga2Amount.Value );
            Config.curaga3Amount = Convert.ToInt32 ( curaga3Amount.Value );
            Config.curaga4Amount = Convert.ToInt32 ( curaga4Amount.Value );
            Config.curaga5Amount = Convert.ToInt32 ( curaga5Amount.Value );
            Config.curagaCurePercentage = curagaCurePercentage.Value;
            Config.curagaTargetType = curagaTargetType.SelectedIndex;
            Config.curagaTargetName = curagaTargetName.Text;
            Config.curagaRequiredMembers = requiredCuragaNumbers.Value;

            // ENHANCING MAGIC

            // BASIC ENHANCING
            Config.autoHasteMinutes = autoHasteMinutes.Value;
            Config.autoAdloquiumMinutes = autoAdloquium_Minutes.Value;
            Config.autoProtect_Minutes = autoProtect_Minutes.Value;
            Config.autoShellMinutes = autoShell_Minutes.Value;
            Config.autoPhalanxIIMinutes = autoPhalanxIIMinutes.Value;
            Config.autoStormspellMinutes = autoStormspellMinutes.Value;
            Config.autoRegen_Minutes = autoRegen_Minutes.Value;
            Config.autoRefresh_Minutes = autoRefresh_Minutes.Value;
            Config.plProtect = plProtect.Checked;
            Config.plShell = plShell.Checked;
            Config.plBlink = plBlink.Checked;
            Config.plReraise = plReraise.Checked;
            if ( plReraiseLevel1.Checked )
            {
                Config.plReraise_Level = 1;
            }
            else if ( plReraiseLevel2.Checked )
            {
                Config.plReraise_Level = 2;
            }
            else if ( plReraiseLevel3.Checked )
            {
                Config.plReraise_Level = 3;
            }
            else if ( plReraiseLevel4.Checked )
            {
                Config.plReraise_Level = 4;
            }
            Config.plRegen = plRegen.Checked;
            if ( plRegenLevel1.Checked )
            {
                Config.plRegen_Level = 1;
            }
            else if ( plRegenLevel2.Checked )
            {
                Config.plRegen_Level = 2;
            }
            else if ( plRegenLevel3.Checked )
            {
                Config.plRegen_Level = 3;
            }
            else if ( plRegenLevel4.Checked )
            {
                Config.plRegen_Level = 4;
            }
            else if ( plRegenLevel5.Checked )
            {
                Config.plRegen_Level = 5;
            }
            Config.plStoneskin = plStoneskin.Checked;
            Config.plPhalanx = plPhalanx.Checked;
            Config.plShellra = plShellra.Checked;
            Config.plProtectra = plProtectra.Checked;
            Config.plProtectra_Level = plProtectralevel.Value;
            Config.plShellra_Level = plShellralevel.Value;
            Config.autoRegen_Spell = autoRegen.SelectedIndex;
            Config.autoRefresh_Spell = autoRefresh.SelectedIndex;
            Config.autoShell_Spell = autoShell.SelectedIndex;
            Config.autoStorm_Spell = autoStorm.SelectedIndex;
            Config.autoProtect_Spell = autoProtect.SelectedIndex;
            Config.plTemper = plTemper.Checked;
            if ( plTemperLevel1.Checked )
            {
                Config.plTemper_Level = 1;
            }
            else if ( plTemperLevel2.Checked )
            {
                Config.plTemper_Level = 2;
            }
            Config.plEnspell = plEnspell.Checked;
            Config.plEnspell_Spell = plEnspell_spell.SelectedIndex;
            Config.plGainBoost = plGainBoost.Checked;
            Config.plGainBoost_Spell = plGainBoost_spell.SelectedIndex;
            Config.plBarElement = plBarElement.Checked;
            Config.plBarElement_Spell = plBarElement_Spell.SelectedIndex;
            Config.AOE_Barelemental = AOE_Barelemental.Checked;
            Config.plBarStatus = plBarStatus.Checked;
            Config.plBarStatus_Spell = plBarStatus_Spell.SelectedIndex;
            Config.plStormSpell = plStormSpell.Checked;
            Config.plAdloquium = plAdloquium.Checked;
            Config.AOE_Barstatus = AOE_Barstatus.Checked;
            Config.plStormSpell_Spell = plStormSpell_Spell.SelectedIndex;
            Config.plKlimaform = plKlimaform.Checked;
            Config.plAuspice = plAuspice.Checked;
            Config.plAquaveil = plAquaveil.Checked;

            Config.plHaste = plHaste.Checked;
            if ( plHasteLevel1.Checked )
            {
                Config.plHaste_Level = 1;
            }
            else if ( plHasteLevel2.Checked )
            {
                Config.plHaste_Level = 2;
            }

            Config.plSpikes = plSpikes.Checked;
            Config.plSpikes_Spell = plSpikes_Spell.SelectedIndex;

            Config.plUtsusemi = plUtsusemi.Checked;
            Config.plRefresh = plRefresh.Checked;
            if ( plRefreshLevel1.Checked )
            {
                Config.plRefresh_Level = 1;
            }
            else if ( plRefreshLevel2.Checked )
            {
                Config.plRefresh_Level = 2;
            }
            else if ( plRefreshLevel3.Checked )
            {
                Config.plRefresh_Level = 3;
            }

            // SCHOLAR STRATAGEMS
            Config.accessionCure = accessionCure.Checked;
            Config.AccessionRegen = accessionRegen.Checked;
            Config.PerpetuanceRegen = perpetuanceRegen.Checked;
            Config.accessionProShell = accessionProShell.Checked;

            Config.regenPerpetuance = regenPerpetuance.Checked;
            Config.regenAccession = regenAccession.Checked;
            Config.refreshPerpetuance = refreshPerpetuance.Checked;
            Config.refreshAccession = refreshAccession.Checked;
            Config.blinkPerpetuance = blinkPerpetuance.Checked;
            Config.blinkAccession = blinkAccession.Checked;
            Config.phalanxPerpetuance = phalanxPerpetuance.Checked;
            Config.phalanxAccession = phalanxAccession.Checked;
            Config.stoneskinPerpetuance = stoneskinPerpetuance.Checked;
            Config.stoneskinAccession = stoneskinAccession.Checked;
            Config.enspellPerpetuance = enspellPerpetuance.Checked;
            Config.enspellAccession = enspellAccession.Checked;
            Config.stormspellPerpetuance = stormspellPerpetuance.Checked;
            Config.stormspellAccession = stormspellAccession.Checked;
            Config.adloquiumPerpetuance = adloquiumPerpetuance.Checked;
            Config.adloquiumAccession = adloquiumAccession.Checked;
            Config.aquaveilPerpetuance = aquaveilPerpetuance.Checked;
            Config.aquaveilAccession = aquaveilAccession.Checked;
            Config.barspellPerpetuance = barspellPerpetuance.Checked;
            Config.barspellAccession = barstatusAccession.Checked;
            Config.barstatusPerpetuance = barstatusPerpetuance.Checked;
            Config.barstatusAccession = barstatusAccession.Checked;

            Config.EnlightenmentReraise = EnlightenmentReraise.Checked;

            // GEOMANCER
            Config.EnableIndiSpells = EnableIndiSpells.Checked;
            Config.EngagedOnly = EngagedOnly.Checked;
            Config.EnableLuopanSpells = EnableLuopanSpells.Checked;
            Config.GeoSpell_Spell = GEOSpell.SelectedIndex;
            Config.LuopanSpell_Target = GEOSpell_target.Text;
            Config.IndiSpell_Spell = INDISpell.SelectedIndex;
            Config.EntrustedSpell_Spell = entrustINDISpell.SelectedIndex;
            Config.EntrustedSpell_Target = entrustSpell_target.Text;
            Config.GeoWhenEngaged = GeoAOE_Engaged.Checked;

            Config.specifiedEngageTarget = false;

            // SINGING
            Config.enableSinging = enableSinging.Checked;

            Config.song1 = song1.SelectedIndex;
            Config.song2 = song2.SelectedIndex;
            Config.song3 = song3.SelectedIndex;
            Config.song4 = song4.SelectedIndex;

            Config.dummy1 = dummy1.SelectedIndex;
            Config.dummy2 = dummy2.SelectedIndex;

            Config.recastSongTime = recastSong.Value;

            Config.recastSongs_Monitored = recastSongs_monitored.Checked;
            Config.SongsOnlyWhenNear = SongsOnlyWhenNearEngaged.Checked;

            // JOB ABILITIES

            Config.AfflatusSolace = afflatusSolace.Checked;
            Config.AfflatusMisery = afflatusMisery.Checked;
            Config.LightArts = lightArts.Checked;
            Config.AddendumWhite = addWhite.Checked;
            Config.Sublimation = sublimation.Checked;
            Config.Celerity = celerity.Checked;
            Config.Accession = accession.Checked;
            Config.Perpetuance = perpetuance.Checked;
            Config.Penury = penury.Checked;
            Config.Rapture = rapture.Checked;
            Config.DarkArts = darkArts.Checked;
            Config.AddendumBlack = addBlack.Checked;

            Config.Composure = composure.Checked;
            Config.Convert = convert.Checked;

            Config.DivineSeal = divineSealBox.Checked;
            Config.Devotion = DevotionBox.Checked;
            Config.DivineCaress = DivineCaressBox.Checked;

            Config.EntrustEnabled = EnableEntrust.Checked;
            Config.Dematerialize = DematerializeBox.Checked;
            Config.BlazeOfGlory = BlazeOfGloryBox.Checked;

            Config.RadialArcana = RadialArcanaBox.Checked;
            Config.RadialArcana_Spell = RadialArcanaSpell.SelectedIndex;
            Config.FullCircle = FullCircleBox.Checked;
            Config.EclipticAttrition = EclipticAttritionBox.Checked;
            Config.LifeCycle = LifeCycleBox.Checked;

            Config.Troubadour = troubadour.Checked;
            Config.Nightingale = nightingale.Checked;
            Config.Marcato = marcato.Checked;

            // DEBUFF REMOVAL
            Config.plSilenceItemEnabled = plSilenceItemEnabled.Checked;
            Config.plSilenceItem = plSilenceItem.SelectedIndex;
            Config.wakeSleepEnabled = wakeSleepEnabled.Checked;
            Config.wakeSleepSpell = wakeSleepSpell.SelectedIndex;
            Config.plDebuffEnabled = plDebuffEnabled.Checked;
            Config.monitoredDebuffEnabled = monitoredDebuffEnabled.Checked;

            Config.plAgiDown = plAgiDown.Checked;
            Config.plAccuracyDown = plAccuracyDown.Checked;
            Config.plAddle = plAddle.Checked;
            Config.plAttackDown = plAttackDown.Checked;
            Config.plBane = plBane.Checked;
            Config.plBind = plBind.Checked;
            Config.plBio = plBio.Checked;
            Config.plBlindness = plBlindness.Checked;
            Config.plBurn = plBurn.Checked;
            Config.plChrDown = plChrDown.Checked;
            Config.plChoke = plChoke.Checked;
            Config.plCurse = plCurse.Checked;
            Config.plCurse2 = plCurse2.Checked;
            Config.plDexDown = plDexDown.Checked;
            Config.plDefenseDown = plDefenseDown.Checked;
            Config.plDia = plDia.Checked;
            Config.plDisease = plDisease.Checked;
            Config.plDoom = plDoom.Checked;
            Config.plDrown = plDrown.Checked;
            Config.plElegy = plElegy.Checked;
            Config.plEvasionDown = plEvasionDown.Checked;
            Config.plFlash = plFlash.Checked;
            Config.plFrost = plFrost.Checked;
            Config.plHelix = plHelix.Checked;
            Config.plIntDown = plIntDown.Checked;
            Config.plMndDown = plMndDown.Checked;
            Config.plMagicAccDown = plMagicAccDown.Checked;
            Config.plMagicAtkDown = plMagicAtkDown.Checked;
            Config.plMaxHpDown = plMaxHpDown.Checked;
            Config.plMaxMpDown = plMaxMpDown.Checked;
            Config.plMaxTpDown = plMaxTpDown.Checked;
            Config.plParalysis = plParalysis.Checked;
            Config.plPlague = plPlague.Checked;
            Config.plPoison = plPoison.Checked;
            Config.plRasp = plRasp.Checked;
            Config.plRequiem = plRequiem.Checked;
            Config.plStrDown = plStrDown.Checked;
            Config.plShock = plShock.Checked;
            Config.plSilence = plSilence.Checked;
            Config.plSlow = plSlow.Checked;
            Config.plThrenody = plThrenody.Checked;
            Config.plVitDown = plVitDown.Checked;
            Config.plWeight = plWeight.Checked;
            Config.plAmnesia = plAmnesia.Checked;
            Config.plDoomEnabled = plDoomEnabled.Checked;
            Config.plDoomitem = plDoomitem.SelectedIndex;

            Config.enablePartyDebuffRemoval = naSpellsenable.Checked;
            Config.SpecifiednaSpellsenable = SpecifiednaSpellsenable.Checked;
            Config.PrioritiseOverLowerTier = PrioritiseOverLowerTier.Checked;
            Config.naBlindness = naBlindness.Checked;
            Config.naCurse = naCurse.Checked;
            Config.naDisease = naDisease.Checked;
            Config.naParalysis = naParalysis.Checked;
            Config.naPetrification = naPetrification.Checked;
            Config.naPlague = naPlague.Checked;
            Config.naPoison = naPoison.Checked;
            Config.naSilence = naSilence.Checked;
            Config.naErase = naErase.Checked;
            Config.Esuna = Esuna.Checked;
            Config.EsunaOnlyAmnesia = EsunaOnlyAmnesia.Checked;

            Config.na_Weight = na_Weight.Checked;
            Config.na_VitDown = na_VitDown.Checked;
            Config.na_Threnody = na_Threnody.Checked;
            Config.na_Slow = na_Slow.Checked;
            Config.na_Shock = na_Shock.Checked;
            Config.na_StrDown = na_StrDown.Checked;
            Config.na_Requiem = na_Requiem.Checked;
            Config.na_Rasp = na_Rasp.Checked;
            Config.na_MaxTpDown = na_MaxTpDown.Checked;
            Config.na_MaxMpDown = na_MaxMpDown.Checked;
            Config.na_MaxHpDown = na_MaxHpDown.Checked;
            Config.na_MagicAttackDown = na_MagicAttackDown.Checked;
            Config.na_MagicDefenseDown = na_MagicDefenseDown.Checked;
            Config.na_MagicAccDown = na_MagicAccDown.Checked;
            Config.na_MndDown = na_MndDown.Checked;
            Config.na_IntDown = na_IntDown.Checked;
            Config.na_Helix = na_Helix.Checked;
            Config.na_Frost = na_Frost.Checked;
            Config.na_EvasionDown = na_EvasionDown.Checked;
            Config.na_Elegy = na_Elegy.Checked;
            Config.na_Drown = na_Drown.Checked;
            Config.na_Dia = na_Dia.Checked;
            Config.na_DefenseDown = na_DefenseDown.Checked;
            Config.na_DexDown = na_DexDown.Checked;
            Config.na_Choke = na_Choke.Checked;
            Config.na_ChrDown = na_ChrDown.Checked;
            Config.na_Burn = na_Burn.Checked;
            Config.na_Bio = na_Bio.Checked;
            Config.na_Bind = na_Bind.Checked;
            Config.na_AttackDown = na_AttackDown.Checked;
            Config.na_Addle = na_Addle.Checked;
            Config.na_AccuracyDown = na_AccuracyDown.Checked;
            Config.na_AgiDown = na_AgiDown.Checked;

            Config.monitoredAgiDown = monitoredAgiDown.Checked;
            Config.monitoredAccuracyDown = monitoredAccuracyDown.Checked;
            Config.monitoredAddle = monitoredAddle.Checked;
            Config.monitoredAttackDown = monitoredAttackDown.Checked;
            Config.monitoredBane = monitoredBane.Checked;
            Config.monitoredBind = monitoredBind.Checked;
            Config.monitoredBio = monitoredBio.Checked;
            Config.monitoredBlindness = monitoredBlindness.Checked;
            Config.monitoredBurn = monitoredBurn.Checked;
            Config.monitoredChrDown = monitoredChrDown.Checked;
            Config.monitoredChoke = monitoredChoke.Checked;
            Config.monitoredCurse = monitoredCurse.Checked;
            Config.monitoredCurse2 = monitoredCurse2.Checked;
            Config.monitoredDexDown = monitoredDexDown.Checked;
            Config.monitoredDefenseDown = monitoredDefenseDown.Checked;
            Config.monitoredDia = monitoredDia.Checked;
            Config.monitoredDisease = monitoredDisease.Checked;
            Config.monitoredDoom = monitoredDoom.Checked;
            Config.monitoredDrown = monitoredDrown.Checked;
            Config.monitoredElegy = monitoredElegy.Checked;
            Config.monitoredEvasionDown = monitoredEvasionDown.Checked;
            Config.monitoredFlash = monitoredFlash.Checked;
            Config.monitoredFrost = monitoredFrost.Checked;
            Config.monitoredHelix = monitoredHelix.Checked;
            Config.monitoredIntDown = monitoredIntDown.Checked;
            Config.monitoredMndDown = monitoredMndDown.Checked;
            Config.monitoredMagicAccDown = monitoredMagicAccDown.Checked;
            Config.monitoredMagicAtkDown = monitoredMagicAtkDown.Checked;
            Config.monitoredMaxHpDown = monitoredMaxHpDown.Checked;
            Config.monitoredMaxMpDown = monitoredMaxMpDown.Checked;
            Config.monitoredMaxTpDown = monitoredMaxTpDown.Checked;
            Config.monitoredParalysis = monitoredParalysis.Checked;
            Config.monitoredPetrification = monitoredPetrification.Checked;
            Config.monitoredPlague = monitoredPlague.Checked;
            Config.monitoredPoison = monitoredPoison.Checked;
            Config.monitoredRasp = monitoredRasp.Checked;
            Config.monitoredRequiem = monitoredRequiem.Checked;
            Config.monitoredStrDown = monitoredStrDown.Checked;
            Config.monitoredShock = monitoredShock.Checked;
            Config.monitoredSilence = monitoredSilence.Checked;
            Config.monitoredSleep = monitoredSleep.Checked;
            Config.monitoredSleep2 = monitoredSleep2.Checked;
            Config.monitoredSlow = monitoredSlow.Checked;
            Config.monitoredThrenody = monitoredThrenody.Checked;
            Config.monitoredVitDown = monitoredVitDown.Checked;
            Config.monitoredWeight = monitoredWeight.Checked;
            Config.monitoredAmnesia = monitoredAmnesia.Checked;

            // OTHER OPTIONS

            Config.lowMPcheckBox = lowMPcheckBox.Checked;
            Config.mpMinCastValue = mpMinCastValue.Value;

            Config.autoTarget = autoTarget.Checked;
            Config.autoTargetSpell = autoTargetSpell.Text;
            Config.Hate_SpellType = Hate_SpellType.SelectedIndex;
            Config.autoTarget_Target = autoTarget_target.Text;
            Config.AssistSpecifiedTarget = AssistSpecifiedTarget.Checked;

            Config.DisableTargettingCancel = DisableTargettingCancel.Checked;
            Config.TargetRemoval_Delay = TargetRemoval_Delay.Value;

            Config.AcceptRaise = acceptRaise.Checked;
            Config.AcceptRaiseOnlyWhenNotInCombat = acceptRaiseOnlyWhenNotInCombat.Checked;


            Config.Fullcircle_DisableEnemy = Fullcircle_DisableEnemy.Checked;
            Config.Fullcircle_GEOTarget = Fullcircle_GEOTarget.Checked;

            Config.RadialArcanaMP = RadialArcanaMP.Value;

            Config.convertMP = ConvertMP.Value;

            Config.sublimationMP = sublimationMP.Value;

            Config.DevotionMP = DevotionMP.Value;
            Config.DevotionTargetType = DevotionTargetType.SelectedIndex;
            Config.DevotionTargetName = DevotionTargetName.Text;
            Config.DevotionWhenEngaged = DevotionWhenEngaged.Checked;

            Config.healLowMP = healLowMP.Checked;
            Config.standAtMP = standAtMP.Checked;
            Config.specifiedEngageTarget = specifiedEngageTarget.Checked;
            Config.standAtMP_Percentage = standAtMP_Percentage.Value;
            Config.healWhenMPBelow = healWhenMPBelow.Value;

            Config.Overcure = Overcure.Checked;
            Config.Undercure = Undercure.Checked;
            Config.enableMonitoredPriority = enableMonitoredPriority.Checked;
            Config.enableOutOfPartyHealing = enableOutOfPartyHealing.Checked;
            Config.OvercureOnHighPriority = OvercureOnHighPriority.Checked;
            Config.EnableAddOn = enableAddOn.Checked;

            // PROGRAM OPTIONS

            Config.pauseOnZoneBox = pauseOnZoneBox.Checked;
            Config.pauseOnStartBox = pauseOnStartBox.Checked;
            Config.pauseOnKO = pauseOnKO.Checked;
            Config.MinimiseonStart = MinimiseonStart.Checked;

            Config.autoFollowName = autoFollowName.Text;
            Config.autoFollowDistance = autoFollowDistance.Value;
            Config.autoFollow_Warning = autoFollow_Warning.Checked;
            Config.FFXIDefaultAutoFollow = FFXIDefaultAutoFollow.Checked;
            Config.enableHotKeys = enableHotKeys.Checked;

            Config.ipAddress = ipAddress.Text;
            Config.listeningPort = listeningPort.Text;

            Config.enableFastCast_Mode = enableFastCast_Mode.Checked;
            Config.trackCastingPackets = trackCastingPackets.Checked;

            // OTHERS
            var path = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "Settings");
            Directory.CreateDirectory(path);
            
            var fileName = CreateFileName();

            WriteFileToXml(Path.Combine(path, fileName));

            Close();
        }

        #endregion "== All Settings Saved"

        private static string CreateFileName()
        {
            var fileName = "Settings.xml";

            if (MainForm.PL != null)
            {
                if (MainForm.PL.Player.MainJob != 0)
                {
                    if (MainForm.PL.Player.SubJob != 0)
                    {
                        JobTitles mainJob = JobUtils.JobNames.Where(c => c.job_number == MainForm.PL.Player.MainJob).FirstOrDefault();
                        JobTitles subJob = JobUtils.JobNames.Where(c => c.job_number == MainForm.PL.Player.SubJob).FirstOrDefault();
                        fileName = mainJob.job_name + "_" + subJob.job_name + ".xml";
                    }
                    else
                    {
                        JobTitles mainJob = JobUtils.JobNames.Where(c => c.job_number == MainForm.PL.Player.MainJob).FirstOrDefault();
                        fileName = mainJob + ".xml";
                    }
                }
            }

            return fileName;
        }

        #region "== PL Debuff Check Boxes"

        private void PLDebuffEnabled_CheckedChanged ( object sender, EventArgs e )
        {
            if ( plDebuffEnabled.Checked )
            {
                plAgiDown.Checked = true;
                plAgiDown.Enabled = true;
                plAccuracyDown.Checked = true;
                plAccuracyDown.Enabled = true;
                plAddle.Checked = true;
                plAddle.Enabled = true;
                plAttackDown.Checked = true;
                plAttackDown.Enabled = true;
                plBane.Checked = true;
                plBane.Enabled = true;
                plBind.Checked = true;
                plBind.Enabled = true;
                plBio.Checked = true;
                plBio.Enabled = true;
                plBlindness.Checked = true;
                plBlindness.Enabled = true;
                plBurn.Checked = true;
                plBurn.Enabled = true;
                plChrDown.Checked = true;
                plChrDown.Enabled = true;
                plChoke.Checked = true;
                plChoke.Enabled = true;
                plCurse.Checked = true;
                plCurse.Enabled = true;
                plCurse2.Checked = true;
                plCurse2.Enabled = true;
                plDexDown.Checked = true;
                plDexDown.Enabled = true;
                plDefenseDown.Checked = true;
                plDefenseDown.Enabled = true;
                plDia.Checked = true;
                plDia.Enabled = true;
                plDisease.Checked = true;
                plDisease.Enabled = true;
                plDoom.Checked = true;
                plDoom.Enabled = true;
                plDrown.Checked = true;
                plDrown.Enabled = true;
                plElegy.Checked = true;
                plElegy.Enabled = true;
                plEvasionDown.Checked = true;
                plEvasionDown.Enabled = true;
                plFlash.Checked = true;
                plFlash.Enabled = true;
                plFrost.Checked = true;
                plFrost.Enabled = true;
                plHelix.Checked = true;
                plHelix.Enabled = true;
                plIntDown.Checked = true;
                plIntDown.Enabled = true;
                plMndDown.Checked = true;
                plMndDown.Enabled = true;
                plMagicAccDown.Checked = true;
                plMagicAccDown.Enabled = true;
                plMagicAtkDown.Checked = true;
                plMagicAtkDown.Enabled = true;
                plMaxHpDown.Checked = true;
                plMaxHpDown.Enabled = true;
                plMaxMpDown.Checked = true;
                plMaxMpDown.Enabled = true;
                plMaxTpDown.Checked = true;
                plMaxTpDown.Enabled = true;
                plParalysis.Checked = true;
                plParalysis.Enabled = true;
                plPlague.Checked = true;
                plPlague.Enabled = true;
                plPoison.Checked = true;
                plPoison.Enabled = true;
                plRasp.Checked = true;
                plRasp.Enabled = true;
                plRequiem.Checked = true;
                plRequiem.Enabled = true;
                plStrDown.Checked = true;
                plStrDown.Enabled = true;
                plShock.Checked = true;
                plShock.Enabled = true;
                plSilence.Checked = true;
                plSilence.Enabled = true;
                plSlow.Checked = true;
                plSlow.Enabled = true;
                plThrenody.Checked = true;
                plThrenody.Enabled = true;
                plVitDown.Checked = true;
                plVitDown.Enabled = true;
                plWeight.Checked = true;
                plWeight.Enabled = true;
                plAmnesia.Checked = true;
                plAmnesia.Enabled = true;
            }
            else if ( plDebuffEnabled.Checked == false )
            {
                plAgiDown.Checked = false;
                plAgiDown.Enabled = false;
                plAccuracyDown.Checked = false;
                plAccuracyDown.Enabled = false;
                plAddle.Checked = false;
                plAddle.Enabled = false;
                plAttackDown.Checked = false;
                plAttackDown.Enabled = false;
                plBane.Checked = false;
                plBane.Enabled = false;
                plBind.Checked = false;
                plBind.Enabled = false;
                plBio.Checked = false;
                plBio.Enabled = false;
                plBlindness.Checked = false;
                plBlindness.Enabled = false;
                plBurn.Checked = false;
                plBurn.Enabled = false;
                plChrDown.Checked = false;
                plChrDown.Enabled = false;
                plChoke.Checked = false;
                plChoke.Enabled = false;
                plCurse.Checked = false;
                plCurse.Enabled = false;
                plCurse2.Checked = false;
                plCurse2.Enabled = false;
                plDexDown.Checked = false;
                plDexDown.Enabled = false;
                plDefenseDown.Checked = false;
                plDefenseDown.Enabled = false;
                plDia.Checked = false;
                plDia.Enabled = false;
                plDisease.Checked = false;
                plDisease.Enabled = false;
                plDoom.Checked = false;
                plDoom.Enabled = false;
                plDrown.Checked = false;
                plDrown.Enabled = false;
                plElegy.Checked = false;
                plElegy.Enabled = false;
                plEvasionDown.Checked = false;
                plEvasionDown.Enabled = false;
                plFlash.Checked = false;
                plFlash.Enabled = false;
                plFrost.Checked = false;
                plFrost.Enabled = false;
                plHelix.Checked = false;
                plHelix.Enabled = false;
                plIntDown.Checked = false;
                plIntDown.Enabled = false;
                plMndDown.Checked = false;
                plMndDown.Enabled = false;
                plMagicAccDown.Checked = false;
                plMagicAccDown.Enabled = false;
                plMagicAtkDown.Checked = false;
                plMagicAtkDown.Enabled = false;
                plMaxHpDown.Checked = false;
                plMaxHpDown.Enabled = false;
                plMaxMpDown.Checked = false;
                plMaxMpDown.Enabled = false;
                plMaxTpDown.Checked = false;
                plMaxTpDown.Enabled = false;
                plParalysis.Checked = false;
                plParalysis.Enabled = false;
                plPlague.Checked = false;
                plPlague.Enabled = false;
                plPoison.Checked = false;
                plPoison.Enabled = false;
                plRasp.Checked = false;
                plRasp.Enabled = false;
                plRequiem.Checked = false;
                plRequiem.Enabled = false;
                plStrDown.Checked = false;
                plStrDown.Enabled = false;
                plShock.Checked = false;
                plShock.Enabled = false;
                plSilence.Checked = false;
                plSilence.Enabled = false;
                plSlow.Checked = false;
                plSlow.Enabled = false;
                plThrenody.Checked = false;
                plThrenody.Enabled = false;
                plVitDown.Checked = false;
                plVitDown.Enabled = false;
                plWeight.Checked = false;
                plWeight.Enabled = false;
                plAmnesia.Checked = false;
                plAmnesia.Enabled = false;
            }
        }

        #endregion "== PL Debuff Check Boxes"

        #region "== Na spell check boxes"

        private void NASpellsEnable_CheckedChanged ( object sender, EventArgs e )
        {
            if ( naSpellsenable.Checked )
            {
                naBlindness.Checked = true;
                naBlindness.Enabled = true;
                naCurse.Checked = true;
                naCurse.Enabled = true;
                naDisease.Checked = true;
                naDisease.Enabled = true;
                naBlindness.Checked = true;
                naBlindness.Enabled = true;
                naParalysis.Checked = true;
                naParalysis.Enabled = true;
                naPetrification.Checked = true;
                naPetrification.Enabled = true;
                naPlague.Checked = true;
                naPlague.Enabled = true;
                naPoison.Checked = true;
                naPoison.Enabled = true;
                naSilence.Checked = true;
                naSilence.Enabled = true;
                naErase.Enabled = true;
                Esuna.Enabled = true;
            }
            else if ( naSpellsenable.Checked == false )
            {
                naBlindness.Checked = false;
                naBlindness.Enabled = false;
                naCurse.Checked = false;
                naCurse.Enabled = false;
                naDisease.Checked = false;
                naDisease.Enabled = false;
                naBlindness.Checked = false;
                naBlindness.Enabled = false;
                naParalysis.Checked = false;
                naParalysis.Enabled = false;
                naPetrification.Checked = false;
                naPetrification.Enabled = false;
                naPlague.Checked = false;
                naPlague.Enabled = false;
                naPoison.Checked = false;
                naPoison.Enabled = false;
                naSilence.Checked = false;
                naSilence.Enabled = false;
                naErase.Checked = false;
                naErase.Enabled = false;
                Esuna.Checked = false;
                Esuna.Enabled = false;
            }
        }

        #endregion "== Na spell check boxes"

        #region "== Monitored Player Debuff Check Boxes"

        private void MonitoredDebuffEnabled_CheckedChanged ( object sender, EventArgs e )
        {
            if ( monitoredDebuffEnabled.Checked )
            {
                monitoredAgiDown.Checked = true;
                monitoredAgiDown.Enabled = true;
                monitoredAccuracyDown.Checked = true;
                monitoredAccuracyDown.Enabled = true;
                monitoredAddle.Checked = true;
                monitoredAddle.Enabled = true;
                monitoredAttackDown.Checked = true;
                monitoredAttackDown.Enabled = true;
                monitoredBane.Checked = true;
                monitoredBane.Enabled = true;
                monitoredBind.Checked = true;
                monitoredBind.Enabled = true;
                monitoredBio.Checked = true;
                monitoredBio.Enabled = true;
                monitoredBlindness.Checked = true;
                monitoredBlindness.Enabled = true;
                monitoredBurn.Checked = true;
                monitoredBurn.Enabled = true;
                monitoredChrDown.Checked = true;
                monitoredChrDown.Enabled = true;
                monitoredChoke.Checked = true;
                monitoredChoke.Enabled = true;
                monitoredCurse.Checked = true;
                monitoredCurse.Enabled = true;
                monitoredCurse2.Checked = true;
                monitoredCurse2.Enabled = true;
                monitoredDexDown.Checked = true;
                monitoredDexDown.Enabled = true;
                monitoredDefenseDown.Checked = true;
                monitoredDefenseDown.Enabled = true;
                monitoredDia.Checked = true;
                monitoredDia.Enabled = true;
                monitoredDisease.Checked = true;
                monitoredDisease.Enabled = true;
                monitoredDoom.Checked = true;
                monitoredDoom.Enabled = true;
                monitoredDrown.Checked = true;
                monitoredDrown.Enabled = true;
                monitoredElegy.Checked = true;
                monitoredElegy.Enabled = true;
                monitoredEvasionDown.Checked = true;
                monitoredEvasionDown.Enabled = true;
                monitoredFlash.Checked = true;
                monitoredFlash.Enabled = true;
                monitoredFrost.Checked = true;
                monitoredFrost.Enabled = true;
                monitoredHelix.Checked = true;
                monitoredHelix.Enabled = true;
                monitoredIntDown.Checked = true;
                monitoredIntDown.Enabled = true;
                monitoredMndDown.Checked = true;
                monitoredMndDown.Enabled = true;
                monitoredMagicAccDown.Checked = true;
                monitoredMagicAccDown.Enabled = true;
                monitoredMagicAtkDown.Checked = true;
                monitoredMagicAtkDown.Enabled = true;
                monitoredMaxHpDown.Checked = true;
                monitoredMaxHpDown.Enabled = true;
                monitoredMaxMpDown.Checked = true;
                monitoredMaxMpDown.Enabled = true;
                monitoredMaxTpDown.Checked = true;
                monitoredMaxTpDown.Enabled = true;
                monitoredParalysis.Checked = true;
                monitoredParalysis.Enabled = true;
                monitoredPetrification.Checked = true;
                monitoredPetrification.Enabled = true;
                monitoredPlague.Checked = true;
                monitoredPlague.Enabled = true;
                monitoredPoison.Checked = true;
                monitoredPoison.Enabled = true;
                monitoredRasp.Checked = true;
                monitoredRasp.Enabled = true;
                monitoredRequiem.Checked = true;
                monitoredRequiem.Enabled = true;
                monitoredStrDown.Checked = true;
                monitoredStrDown.Enabled = true;
                monitoredShock.Checked = true;
                monitoredShock.Enabled = true;
                monitoredSilence.Checked = true;
                monitoredSilence.Enabled = true;
                monitoredSleep.Checked = true;
                monitoredSleep.Enabled = true;
                monitoredSleep2.Checked = true;
                monitoredSleep2.Enabled = true;
                monitoredSlow.Checked = true;
                monitoredSlow.Enabled = true;
                monitoredThrenody.Checked = true;
                monitoredThrenody.Enabled = true;
                monitoredVitDown.Checked = true;
                monitoredVitDown.Enabled = true;
                monitoredWeight.Checked = true;
                monitoredWeight.Enabled = true;
                monitoredAmnesia.Checked = true;
                monitoredAmnesia.Enabled = true;
            }
            else if ( monitoredDebuffEnabled.Checked == false )
            {
                monitoredAgiDown.Checked = false;
                monitoredAgiDown.Enabled = false;
                monitoredAccuracyDown.Checked = false;
                monitoredAccuracyDown.Enabled = false;
                monitoredAddle.Checked = false;
                monitoredAddle.Enabled = false;
                monitoredAttackDown.Checked = false;
                monitoredAttackDown.Enabled = false;
                monitoredBane.Checked = false;
                monitoredBane.Enabled = false;
                monitoredBind.Checked = false;
                monitoredBind.Enabled = false;
                monitoredBio.Checked = false;
                monitoredBio.Enabled = false;
                monitoredBlindness.Checked = false;
                monitoredBlindness.Enabled = false;
                monitoredBurn.Checked = false;
                monitoredBurn.Enabled = false;
                monitoredChrDown.Checked = false;
                monitoredChrDown.Enabled = false;
                monitoredChoke.Checked = false;
                monitoredChoke.Enabled = false;
                monitoredCurse.Checked = false;
                monitoredCurse.Enabled = false;
                monitoredCurse2.Checked = false;
                monitoredCurse2.Enabled = false;
                monitoredDexDown.Checked = false;
                monitoredDexDown.Enabled = false;
                monitoredDefenseDown.Checked = false;
                monitoredDefenseDown.Enabled = false;
                monitoredDia.Checked = false;
                monitoredDia.Enabled = false;
                monitoredDisease.Checked = false;
                monitoredDisease.Enabled = false;
                monitoredDoom.Checked = false;
                monitoredDoom.Enabled = false;
                monitoredDrown.Checked = false;
                monitoredDrown.Enabled = false;
                monitoredElegy.Checked = false;
                monitoredElegy.Enabled = false;
                monitoredEvasionDown.Checked = false;
                monitoredEvasionDown.Enabled = false;
                monitoredFlash.Checked = false;
                monitoredFlash.Enabled = false;
                monitoredFrost.Checked = false;
                monitoredFrost.Enabled = false;
                monitoredHelix.Checked = false;
                monitoredHelix.Enabled = false;
                monitoredIntDown.Checked = false;
                monitoredIntDown.Enabled = false;
                monitoredMndDown.Checked = false;
                monitoredMndDown.Enabled = false;
                monitoredMagicAccDown.Checked = false;
                monitoredMagicAccDown.Enabled = false;
                monitoredMagicAtkDown.Checked = false;
                monitoredMagicAtkDown.Enabled = false;
                monitoredMaxHpDown.Checked = false;
                monitoredMaxHpDown.Enabled = false;
                monitoredMaxMpDown.Checked = false;
                monitoredMaxMpDown.Enabled = false;
                monitoredMaxTpDown.Checked = false;
                monitoredMaxTpDown.Enabled = false;
                monitoredParalysis.Checked = false;
                monitoredParalysis.Enabled = false;
                monitoredPetrification.Checked = false;
                monitoredPetrification.Enabled = false;
                monitoredPlague.Checked = false;
                monitoredPlague.Enabled = false;
                monitoredPoison.Checked = false;
                monitoredPoison.Enabled = false;
                monitoredRasp.Checked = false;
                monitoredRasp.Enabled = false;
                monitoredRequiem.Checked = false;
                monitoredRequiem.Enabled = false;
                monitoredStrDown.Checked = false;
                monitoredStrDown.Enabled = false;
                monitoredShock.Checked = false;
                monitoredShock.Enabled = false;
                monitoredSilence.Checked = false;
                monitoredSilence.Enabled = false;
                monitoredSleep.Checked = false;
                monitoredSleep.Enabled = false;
                monitoredSleep2.Checked = false;
                monitoredSleep2.Enabled = false;
                monitoredSlow.Checked = false;
                monitoredSlow.Enabled = false;
                monitoredThrenody.Checked = false;
                monitoredThrenody.Enabled = false;
                monitoredVitDown.Checked = false;
                monitoredVitDown.Enabled = false;
                monitoredWeight.Checked = false;
                monitoredAmnesia.Checked = false;
                monitoredAmnesia.Enabled = false;
                monitoredWeight.Enabled = false;
            }
        }

        #endregion "== Monitored Player Debuff Check Boxes"

        #region "== Geomancy Check Boxes"

        private void EnableGeoSpells_CheckedChanged ( object sender, EventArgs e )
        {
            INDISpell.Enabled = EnableIndiSpells.Checked;
        }

        private void EnableLuopanSpells_CheckedChanged ( object sender, EventArgs e )
        {
            GEOSpell.Enabled = EnableLuopanSpells.Checked;
            GEOSpell_target.Enabled = EnableLuopanSpells.Checked;
        }

        private void EnableEntrust_CheckedChanged(object sender, EventArgs e)
        {
            entrustINDISpell.Enabled = EnableEntrust.Checked;
            entrustSpell_target.Enabled = EnableEntrust.Checked;
        }

        #endregion "== Geomancy Check Boxes"

        private void SaveAsButton_Click ( object sender, EventArgs e )
        {
            SaveAllSettings_Click ( sender, e );

            SaveFileDialog savefile = new()
            {
                FileName = CreateFileName(),
                Filter = " Extensible Markup Language (*.xml)|*.xml",
                FilterIndex = 2,
                InitialDirectory = System.IO.Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "Settings")
            };

            if ( savefile.ShowDialog ( ) == DialogResult.OK )
            {
                WriteFileToXml(savefile.FileName);
            }
        }

        private static void WriteFileToXml(string fileName)
        {
            TextWriter writer = null;
            try
            {
                XmlSerializer mySerializer = new(typeof(MySettings));
                writer = new StreamWriter(fileName);
                mySerializer.Serialize(writer, Config);
            }
            finally
            {
                if (writer != null)
                    writer.Close();
            }
        }

        private void LoadButton_Click ( object sender, EventArgs e )
        {
            OpenFileDialog openFileDialog1 = new()
            {
                Filter = " Extensible Markup Language (*.xml)|*.xml",
                FilterIndex = 2,
                InitialDirectory = System.IO.Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "Settings")
            };

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                Config = ReadSettings(openFileDialog1.FileName);
                UpdateForm(Config);
            }
        }

        private void UpdateForm(MySettings config)
        {
            // HEALING MAGIC
            cure1enabled.Checked = config.cure1enabled;
            cure2enabled.Checked = config.cure2enabled;
            cure3enabled.Checked = config.cure3enabled;
            cure4enabled.Checked = config.cure4enabled;
            cure5enabled.Checked = config.cure5enabled;
            cure6enabled.Checked = config.cure6enabled;
            cure1amount.Value = config.cure1amount;
            cure2amount.Value = config.cure2amount;
            cure3amount.Value = config.cure3amount;
            cure4amount.Value = config.cure4amount;
            cure5amount.Value = config.cure5amount;
            cure6amount.Value = config.cure6amount;
            curePercentage.Value = config.curePercentage;
            curePercentageValueLabel.Text = config.curePercentage.ToString ( CultureInfo.InvariantCulture );
            priorityCurePercentage.Value = config.priorityCurePercentage;
            priorityCurePercentageValueLabel.Text = config.priorityCurePercentage.ToString ( CultureInfo.InvariantCulture );
            monitoredCurePercentage.Value = config.monitoredCurePercentage;
            monitoredCurePercentageValueLabel.Text = config.monitoredCurePercentage.ToString ( CultureInfo.InvariantCulture );

            curagaEnabled.Checked = config.curagaEnabled;
            curaga2Enabled.Checked = config.curaga2enabled;
            curaga3Enabled.Checked = config.curaga3enabled;
            curaga4Enabled.Checked = config.curaga4enabled;
            curaga5Enabled.Checked = config.curaga5enabled;

            curagaAmount.Value = config.curagaAmount;
            curaga2Amount.Value = config.curaga2Amount;
            curaga3Amount.Value = config.curaga3Amount;
            curaga4Amount.Value = config.curaga4Amount;
            curaga5Amount.Value = config.curaga5Amount;

            curagaCurePercentage.Value = config.curagaCurePercentage;
            curagaPercentageValueLabel.Text = config.curagaCurePercentage.ToString ( CultureInfo.InvariantCulture );
            curagaTargetType.SelectedIndex = config.curagaTargetType;
            curagaTargetName.Text = config.curagaTargetName;
            requiredCuragaNumbers.Value = config.curagaRequiredMembers;

            // ENHANCING MAGIC

            // BASIC ENHANCING
            autoHasteMinutes.Value = config.autoHasteMinutes;
            if ( config.autoAdloquiumMinutes != 0 )
            {
                autoAdloquium_Minutes.Value = config.autoAdloquiumMinutes;
            }
            else
            {
                config.autoAdloquiumMinutes = 2;
                autoAdloquium_Minutes.Value = 2;
            }
            autoProtect_Minutes.Value = config.autoProtect_Minutes;
            autoShell_Minutes.Value = config.autoShellMinutes;
            autoPhalanxIIMinutes.Value = config.autoPhalanxIIMinutes;
            if ( config.autoStormspellMinutes == 0 )
            {
                autoStormspellMinutes.Value = 3;
            }
            else
            {
                autoStormspellMinutes.Value = config.autoStormspellMinutes;
            }
            autoRefresh_Minutes.Value = config.autoRefresh_Minutes;
            autoRegen_Minutes.Value = config.autoRegen_Minutes;
            autoRefresh_Minutes.Value = config.autoRefresh_Minutes;
            plBlink.Checked = config.plBlink;

            autoRegen.SelectedIndex = config.autoRegen_Spell;
            autoRefresh.SelectedIndex = config.autoRefresh_Spell;
            autoShell.SelectedIndex = config.autoShell_Spell;
            autoStorm.SelectedIndex = config.autoStorm_Spell;
            autoProtect.SelectedIndex = config.autoProtect_Spell;
            plProtect.Checked = config.plProtect;
            plShell.Checked = config.plProtect;




            plRegen.Checked = config.plRegen;
            if ( config.plRegen_Level == 1 && plRegen.Checked == true )
            {
                plRegenLevel1.Checked = true;
            }
            else if ( config.plRegen_Level == 2 && plRegen.Checked == true )
            {
                plRegenLevel2.Checked = true;
            }
            else if ( config.plRegen_Level == 3 && plRegen.Checked == true )
            {
                plRegenLevel3.Checked = true;
            }
            else if ( config.plRegen_Level == 4 && plRegen.Checked == true )
            {
                plRegenLevel4.Checked = true;
            }
            else if ( config.plRegen_Level == 5 && plRegen.Checked == true )
            {
                plRegenLevel5.Checked = true;
            }

            plReraise.Checked = config.plReraise;
            if ( config.plReraise_Level == 1 && plReraise.Checked == true )
            {
                plReraiseLevel1.Checked = true;
            }
            else if ( config.plReraise_Level == 2 && plReraise.Checked == true )
            {
                plReraiseLevel2.Checked = true;
            }
            else if ( config.plReraise_Level == 3 && plReraise.Checked == true )
            {
                plReraiseLevel3.Checked = true;
            }
            else if ( config.plReraise_Level == 4 && plReraise.Checked == true )
            {
                plReraiseLevel4.Checked = true;
            }
            plRefresh.Checked = config.plRefresh;
            if ( config.plRefresh_Level == 1 && plRefresh.Checked == true )
            {
                plRefreshLevel1.Checked = true;
            }
            else if ( config.plRefresh_Level == 2 && plRefresh.Checked == true )
            {
                plRefreshLevel2.Checked = true;
            }
            else if ( config.plRefresh_Level == 3 && plRefresh.Checked == true )
            {
                plRefreshLevel3.Checked = true;
            }
            plStoneskin.Checked = config.plStoneskin;
            plPhalanx.Checked = config.plPhalanx;
            plTemper.Checked = config.plTemper;
            if ( config.plTemper_Level == 1 && plTemper.Checked == true )
            {
                plTemperLevel1.Checked = true;
            }
            else if ( config.plTemper_Level == 2 && plTemper.Checked == true )
            {
                plTemperLevel2.Checked = true;
            }

            plHaste.Checked = config.plHaste;
            if ( config.plHaste_Level == 1 && plHaste.Checked == true )
            {
                plHasteLevel1.Checked = true;
            }
            else if ( config.plHaste_Level == 2 && plHaste.Checked == true )
            {
                plHasteLevel2.Checked = true;
            }

            plSpikes.Checked = config.plSpikes;
            plSpikes_Spell.SelectedIndex = config.plSpikes_Spell;


            plEnspell.Checked = config.plEnspell;
            plEnspell_spell.SelectedIndex = config.plEnspell_Spell;
            plGainBoost.Checked = config.plGainBoost;
            plGainBoost_spell.SelectedIndex = config.plGainBoost_Spell;
            EnableEntrust.Checked = config.EntrustEnabled;
            DematerializeBox.Checked = config.Dematerialize;
            plBarElement.Checked = config.plBarElement;
            if ( config.plBarElement_Spell > 5 )
            {
                plBarElement_Spell.SelectedIndex = 0;
                config.plBarElement_Spell = 0; ;
            }
            else
            {
                plBarElement_Spell.SelectedIndex = config.plBarElement_Spell;
            }
            AOE_Barelemental.Checked = config.AOE_Barelemental;
            plBarStatus.Checked = config.plBarStatus;
            if ( config.plBarStatus_Spell > 8 )
            {
                plBarStatus_Spell.SelectedIndex = 0;
                config.plBarStatus_Spell = 0; ;
            }
            else
            {
                plBarStatus_Spell.SelectedIndex = config.plBarStatus_Spell;
            }
            AOE_Barstatus.Checked = config.AOE_Barstatus;
            plStormSpell.Checked = config.plStormSpell;
            plKlimaform.Checked = config.plKlimaform;
            plStormSpell_Spell.SelectedIndex = config.plStormSpell_Spell;
            plAdloquium.Checked = config.plAdloquium;
            plAuspice.Checked = config.plAuspice;
            plAquaveil.Checked = config.plAquaveil;
            plUtsusemi.Checked = config.plUtsusemi;

            // SCHOLAR STRATAGEMS
            accessionCure.Checked = config.accessionCure;
            accessionProShell.Checked = config.accessionProShell;
            perpetuanceRegen.Checked = config.PerpetuanceRegen;
            accessionRegen.Checked = config.AccessionRegen;

            regenPerpetuance.Checked = config.regenPerpetuance;
            regenAccession.Checked = config.regenAccession;
            refreshPerpetuance.Checked = config.refreshPerpetuance;
            refreshAccession.Checked = config.refreshAccession;
            blinkPerpetuance.Checked = config.blinkPerpetuance;
            blinkAccession.Checked = config.blinkPerpetuance;
            phalanxPerpetuance.Checked = config.phalanxPerpetuance;
            phalanxAccession.Checked = config.phalanxAccession;
            stoneskinPerpetuance.Checked = config.stoneskinPerpetuance;
            stoneskinAccession.Checked = config.stoneskinAccession;
            enspellPerpetuance.Checked = config.enspellPerpetuance;
            enspellAccession.Checked = config.enspellAccession;
            stormspellPerpetuance.Checked = config.stormspellPerpetuance;
            stormspellAccession.Checked = config.stormspellAccession;
            adloquiumAccession.Checked = config.adloquiumAccession;
            adloquiumPerpetuance.Checked = config.adloquiumPerpetuance;
            aquaveilPerpetuance.Checked = config.aquaveilPerpetuance;
            aquaveilAccession.Checked = config.aquaveilAccession;
            barspellPerpetuance.Checked = config.barspellPerpetuance;
            barspellAccession.Checked = config.barspellAccession;
            barstatusPerpetuance.Checked = config.barstatusPerpetuance;
            barstatusAccession.Checked = config.barstatusAccession;

            EnlightenmentReraise.Checked = config.EnlightenmentReraise;

            // GEOMANCER
            EnableIndiSpells.Checked = config.EnableIndiSpells;
            EngagedOnly.Checked = config.EngagedOnly;
            GEOSpell.SelectedIndex = config.GeoSpell_Spell;
            GEOSpell_target.Text = config.LuopanSpell_Target;
            INDISpell.SelectedIndex = config.IndiSpell_Spell;
            entrustINDISpell.SelectedIndex = config.EntrustedSpell_Spell;
            entrustSpell_target.Text = config.EntrustedSpell_Target;
            EnableLuopanSpells.Checked = config.EnableLuopanSpells;
            specifiedEngageTarget.Checked = config.specifiedEngageTarget;
            GeoAOE_Engaged.Checked = config.GeoWhenEngaged;

            // SINGING
            song1.SelectedIndex = config.song1;
            song2.SelectedIndex = config.song2;
            song3.SelectedIndex = config.song3;
            song4.SelectedIndex = config.song4;
            dummy1.SelectedIndex = config.dummy1;
            dummy2.SelectedIndex = config.dummy2;
            recastSong.Value = config.recastSongTime;
            enableSinging.Checked = config.enableSinging;
            recastSongs_monitored.Checked = config.recastSongs_Monitored;
            SongsOnlyWhenNearEngaged.Checked = config.SongsOnlyWhenNear;

            //JOB ABILITIES
            afflatusSolace.Checked = config.AfflatusSolace;
            afflatusMisery.Checked = config.AfflatusMisery;
            divineSealBox.Checked = config.DivineSeal;
            DevotionBox.Checked = config.Devotion;
            DivineCaressBox.Checked = config.DivineCaress;

            lightArts.Checked = config.LightArts;
            addWhite.Checked = config.AddendumWhite;
            sublimation.Checked = config.Sublimation;
            celerity.Checked = config.Celerity;
            accession.Checked = config.Accession;
            perpetuance.Checked = config.Perpetuance;
            penury.Checked = config.Penury;
            rapture.Checked = config.Rapture;
            darkArts.Checked = config.DarkArts;
            addBlack.Checked = config.AddendumBlack;

            composure.Checked = config.Composure;
            convert.Checked = config.Convert;

            BlazeOfGloryBox.Checked = config.BlazeOfGlory;
            FullCircleBox.Checked = config.FullCircle;
            EclipticAttritionBox.Checked = config.EclipticAttrition;
            LifeCycleBox.Checked = config.LifeCycle;

            troubadour.Checked = config.Troubadour;
            nightingale.Checked = config.Nightingale;
            marcato.Checked = config.Marcato;

            //DEBUFF REMOVAL
            plSilenceItemEnabled.Checked = config.plSilenceItemEnabled;
            plSilenceItem.SelectedIndex = config.plSilenceItem;
            wakeSleepEnabled.Checked = config.wakeSleepEnabled;
            wakeSleepSpell.SelectedIndex = config.wakeSleepSpell;
            plDoomEnabled.Checked = config.plDoomEnabled;
            plDoomitem.SelectedIndex = config.plDoomitem;

            plDebuffEnabled.Checked = config.plDebuffEnabled;
            plAgiDown.Checked = config.plAgiDown;
            plAccuracyDown.Checked = config.plAccuracyDown;
            plAddle.Checked = config.plAddle;
            plAttackDown.Checked = config.plAttackDown;
            plBane.Checked = config.plBane;
            plBind.Checked = config.plBind;
            plBio.Checked = config.plBio;
            plBlindness.Checked = config.plBlindness;
            plBurn.Checked = config.plBurn;
            plChrDown.Checked = config.plChrDown;
            plChoke.Checked = config.plChoke;
            plCurse.Checked = config.plCurse;
            plCurse2.Checked = config.plCurse2;
            plDexDown.Checked = config.plDexDown;
            plDefenseDown.Checked = config.plDefenseDown;
            plDia.Checked = config.plDia;
            plDisease.Checked = config.plDisease;
            plDoom.Checked = config.plDoom;
            plDrown.Checked = config.plDrown;
            plElegy.Checked = config.plElegy;
            plEvasionDown.Checked = config.plEvasionDown;
            plFlash.Checked = config.plFlash;
            plFrost.Checked = config.plFrost;
            plHelix.Checked = config.plHelix;
            plIntDown.Checked = config.plIntDown;
            plMndDown.Checked = config.plMndDown;
            plMagicAccDown.Checked = config.plMagicAccDown;
            plMagicAtkDown.Checked = config.plMagicAtkDown;
            plMaxHpDown.Checked = config.plMaxHpDown;
            plMaxMpDown.Checked = config.plMaxMpDown;
            plMaxTpDown.Checked = config.plMaxTpDown;
            plParalysis.Checked = config.plParalysis;
            plPlague.Checked = config.plPlague;
            plPoison.Checked = config.plPoison;
            plRasp.Checked = config.plRasp;
            plRequiem.Checked = config.plRequiem;
            plStrDown.Checked = config.plStrDown;
            plShock.Checked = config.plShock;
            plSilence.Checked = config.plSilence;
            plSlow.Checked = config.plSlow;
            plThrenody.Checked = config.plThrenody;
            plVitDown.Checked = config.plVitDown;
            plWeight.Checked = config.plWeight;
            plAmnesia.Checked = config.plAmnesia;

            monitoredDebuffEnabled.Checked = config.monitoredDebuffEnabled;
            plProtectra.Checked = config.plProtectra;
            plShellra.Checked = config.plShellra;
            plProtectralevel.Value = config.plProtectra_Level;
            plShellralevel.Value = config.plShellra_Level;
            monitoredAgiDown.Checked = config.monitoredAgiDown;
            monitoredAccuracyDown.Checked = config.monitoredAccuracyDown;
            monitoredAddle.Checked = config.monitoredAddle;
            monitoredAttackDown.Checked = config.monitoredAttackDown;
            monitoredBane.Checked = config.monitoredBane;
            monitoredBind.Checked = config.monitoredBind;
            monitoredBio.Checked = config.monitoredBio;
            monitoredBlindness.Checked = config.monitoredBlindness;
            monitoredBurn.Checked = config.monitoredBurn;
            monitoredChrDown.Checked = config.monitoredChrDown;
            monitoredChoke.Checked = config.monitoredChoke;
            monitoredCurse.Checked = config.monitoredCurse;
            monitoredCurse2.Checked = config.monitoredCurse2;
            monitoredDexDown.Checked = config.monitoredDexDown;
            monitoredDefenseDown.Checked = config.monitoredDefenseDown;
            monitoredDia.Checked = config.monitoredDia;
            monitoredDisease.Checked = config.monitoredDisease;
            monitoredDoom.Checked = config.monitoredDoom;
            monitoredDrown.Checked = config.monitoredDrown;
            monitoredElegy.Checked = config.monitoredElegy;
            monitoredEvasionDown.Checked = config.monitoredEvasionDown;
            monitoredFlash.Checked = config.monitoredFlash;
            monitoredFrost.Checked = config.monitoredFrost;
            monitoredHelix.Checked = config.monitoredHelix;
            monitoredIntDown.Checked = config.monitoredIntDown;
            monitoredMndDown.Checked = config.monitoredMndDown;
            monitoredMagicAccDown.Checked = config.monitoredMagicAccDown;
            monitoredMagicAtkDown.Checked = config.monitoredMagicAtkDown;
            monitoredMaxHpDown.Checked = config.monitoredMaxHpDown;
            monitoredMaxMpDown.Checked = config.monitoredMaxMpDown;
            monitoredMaxTpDown.Checked = config.monitoredMaxTpDown;
            monitoredParalysis.Checked = config.monitoredParalysis;
            monitoredPetrification.Checked = config.monitoredPetrification;
            monitoredPlague.Checked = config.monitoredPlague;
            monitoredPoison.Checked = config.monitoredPoison;
            monitoredRasp.Checked = config.monitoredRasp;
            monitoredRequiem.Checked = config.monitoredRequiem;
            monitoredStrDown.Checked = config.monitoredStrDown;
            monitoredShock.Checked = config.monitoredShock;
            monitoredSilence.Checked = config.monitoredSilence;
            monitoredSleep.Checked = config.monitoredSleep;
            monitoredSleep2.Checked = config.monitoredSleep2;
            monitoredSlow.Checked = config.monitoredSlow;
            monitoredThrenody.Checked = config.monitoredThrenody;
            monitoredVitDown.Checked = config.monitoredVitDown;
            monitoredWeight.Checked = config.monitoredWeight;
            monitoredAmnesia.Checked = config.monitoredAmnesia;

            naSpellsenable.Checked = config.enablePartyDebuffRemoval;
            SpecifiednaSpellsenable.Checked = config.SpecifiednaSpellsenable;
            PrioritiseOverLowerTier.Checked = config.PrioritiseOverLowerTier;
            naBlindness.Checked = config.naBlindness;
            naCurse.Checked = config.naCurse;
            naDisease.Checked = config.naDisease;
            naParalysis.Checked = config.naParalysis;
            naPetrification.Checked = config.naPetrification;
            naPlague.Checked = config.naPlague;
            naPoison.Checked = config.naPoison;
            naSilence.Checked = config.naSilence;
            naErase.Checked = config.naErase;
            Esuna.Checked = config.Esuna;
            EsunaOnlyAmnesia.Checked = config.EsunaOnlyAmnesia;

            na_Weight.Checked = config.na_Weight;
            na_VitDown.Checked = config.na_VitDown;
            na_Threnody.Checked = config.na_Threnody;
            na_Slow.Checked = config.na_Slow;
            na_Shock.Checked = config.na_Shock;
            na_StrDown.Checked = config.na_StrDown;
            na_Requiem.Checked = config.na_Requiem;
            na_Rasp.Checked = config.na_Rasp;
            na_MaxTpDown.Checked = config.na_MaxTpDown;
            na_MaxMpDown.Checked = config.na_MaxMpDown;
            na_MaxHpDown.Checked = config.na_MaxHpDown;
            na_MagicAttackDown.Checked = config.na_MagicAttackDown;
            na_MagicDefenseDown.Checked = config.na_MagicDefenseDown;
            na_MagicAccDown.Checked = config.na_MagicAccDown;
            na_MndDown.Checked = config.na_MndDown;
            na_IntDown.Checked = config.na_IntDown;
            na_Helix.Checked = config.na_Helix;
            na_Frost.Checked = config.na_Frost;
            na_EvasionDown.Checked = config.na_EvasionDown;
            na_Elegy.Checked = config.na_Elegy;
            na_Drown.Checked = config.na_Drown;
            na_Dia.Checked = config.na_Dia;
            na_DefenseDown.Checked = config.na_DefenseDown;
            na_DexDown.Checked = config.na_DexDown;
            na_Choke.Checked = config.na_Choke;
            na_ChrDown.Checked = config.na_ChrDown;
            na_Burn.Checked = config.na_Burn;
            na_Bio.Checked = config.na_Bio;
            na_Bind.Checked = config.na_Bind;
            na_AttackDown.Checked = config.na_AttackDown;
            na_Addle.Checked = config.na_Addle;
            na_AccuracyDown.Checked = config.na_AccuracyDown;
            na_AgiDown.Checked = config.na_AgiDown;

            // OTHER OPTIONS
            lowMPcheckBox.Checked = config.lowMPcheckBox;
            mpMinCastValue.Value = config.mpMinCastValue;

            autoTarget.Checked = config.autoTarget;
            autoTargetSpell.Text = config.autoTargetSpell;
            Hate_SpellType.SelectedIndex = config.Hate_SpellType;
            autoTarget_target.Text = config.autoTarget_Target;

            AssistSpecifiedTarget.Checked = config.AssistSpecifiedTarget;

            DisableTargettingCancel.Checked = config.DisableTargettingCancel;
            TargetRemoval_Delay.Value = config.TargetRemoval_Delay;

            acceptRaise.Checked = config.AcceptRaise;
            acceptRaiseOnlyWhenNotInCombat.Checked = config.AcceptRaiseOnlyWhenNotInCombat;

            RadialArcanaBox.Checked = config.RadialArcana;



            Fullcircle_DisableEnemy.Checked = config.Fullcircle_DisableEnemy;
            Fullcircle_GEOTarget.Checked = config.Fullcircle_GEOTarget;

            RadialArcanaSpell.SelectedIndex = config.RadialArcana_Spell;
            RadialArcanaMP.Value = config.RadialArcanaMP;

            ConvertMP.Value = config.convertMP;

            DevotionMP.Value = config.DevotionMP;
            DevotionTargetType.SelectedIndex = config.DevotionTargetType;
            DevotionTargetName.Text = config.DevotionTargetName;
            DevotionWhenEngaged.Checked = config.DevotionWhenEngaged;

            sublimationMP.Value = config.sublimationMP;

            healLowMP.Checked = config.healLowMP;
            healWhenMPBelow.Value = config.healWhenMPBelow;

            standAtMP.Checked = config.standAtMP;
            standAtMP_Percentage.Value = config.standAtMP_Percentage;

            Overcure.Checked = config.Overcure;
            Undercure.Checked = config.Undercure;
            enableMonitoredPriority.Checked = config.enableMonitoredPriority;
            enableOutOfPartyHealing.Checked = config.enableOutOfPartyHealing;
            OvercureOnHighPriority.Checked = config.OvercureOnHighPriority;

            enableAddOn.Checked = config.EnableAddOn;

            // PROGRAM OPTIONS

            pauseOnZoneBox.Checked = config.pauseOnZoneBox;
            pauseOnStartBox.Checked = config.pauseOnStartBox;
            pauseOnKO.Checked = config.pauseOnKO;
            MinimiseonStart.Checked = config.MinimiseonStart;

            autoFollowName.Text = config.autoFollowName;
            autoFollowDistance.Value = config.autoFollowDistance;
            autoFollow_Warning.Checked = config.autoFollow_Warning;
            FFXIDefaultAutoFollow.Checked = config.FFXIDefaultAutoFollow;
            enableHotKeys.Checked = config.enableHotKeys;

            ipAddress.Text = config.ipAddress;
            listeningPort.Text = config.listeningPort;

            enableFastCast_Mode.Checked = config.enableFastCast_Mode;
            trackCastingPackets.Checked = config.trackCastingPackets;
        }

        private void AutoAdjust_Cure_Click ( object sender, EventArgs e )
        {
            //decimal level = this.cureLevel.Value;
            double potency = System.Convert.ToDouble(curePotency.Value);

            if ( MainForm.PL != null )
            {
                // First calculate default potency

                double MND = MainForm.PL.Player.Stats.Mind;
                double VIT = MainForm.PL.Player.Stats.Vitality;

                ushort Healing = MainForm.PL.Player.CombatSkills.Healing.Skill;

                // Now grab calculations for each tier

                double MND_B = Math.Floor(MND / 2);
                double VIT_B = Math.Floor(VIT / 4);

                double Power = MND_B + VIT_B + Healing;

                double Cure = 0;

                if ( Power >= 0 && Power < 20 )
                {
                    Cure = ( 0 + Power ) - 0;
                    Cure /= 1;
                    Cure = Math.Floor ( Cure + 10 );
                }
                else if ( Power >= 20 && Power < 40 )
                {
                    Cure = ( 0 + Power ) - 20;
                    Cure /= 1.33;
                    Cure = Math.Floor ( Cure + 15 );
                }
                else if ( Power >= 40 && Power < 125 )
                {
                    Cure = ( 0 + Power ) - 40;
                    Cure /= 8.5;
                    Cure = Math.Floor ( Cure + 30 );
                }
                else if ( Power >= 125 && Power < 200 )
                {
                    Cure = ( 0 + Power ) - 125;
                    Cure /= 8.5;
                    Cure = Math.Floor ( Cure + 40 );
                }
                else if ( Power >= 200 && Power < 600 )
                {
                    Cure = ( 0 + Power ) - 200;
                    Cure /= 20;
                    Cure = Math.Floor ( Cure + 45 );
                }
                else if ( Power >= 600 )
                {
                    Cure = 65;
                }

                double Cure_pot = Cure * 00.01;
                Cure_pot *= potency;

                double Cure_mathed = Math.Round(Cure + Cure_pot);
                Cure_mathed -= ( Cure_mathed * 0.10 );

                double Cure2 = 0;

                if ( Power >= 40 && Power < 70 )
                {
                    Cure2 = ( 0 + Power ) - 40;
                    Cure2 /= 1;
                    Cure2 = Math.Floor ( Cure2 + 60 );
                }
                else if ( Power >= 70 && Power < 125 )
                {
                    Cure2 = ( 0 + Power ) - 70;
                    Cure2 /= 5.5;
                    Cure2 = Math.Floor ( Cure2 + 90 );
                }
                else if ( Power >= 125 && Power < 200 )
                {
                    Cure2 = ( 0 + Power ) - 125;
                    Cure2 /= 7.5;
                    Cure2 = Math.Floor ( Cure2 + 100 );
                }
                else if ( Power >= 200 && Power < 400 )
                {
                    Cure2 = ( 0 + Power ) - 200;
                    Cure2 /= 10;
                    Cure2 = Math.Floor ( Cure2 + 110 );
                }
                else if ( Power >= 400 && Power < 700 )
                {
                    Cure2 = ( 0 + Power ) - 400;
                    Cure2 /= 20;
                    Cure2 = Math.Floor ( Cure2 + 130 );
                }
                else if ( Power >= 700 )
                {
                    Cure2 = 145;
                }

                double Cure2_pot = Cure2 * 00.01;
                Cure2_pot *= potency;

                double Cure2_mathed = Math.Round(Cure2 + Cure2_pot);
                Cure2_mathed -= ( Cure2_mathed * 0.10 );

                double Cure3 = 0;

                if ( Power >= 70 && Power < 125 )
                {
                    Cure3 = ( 0 + Power ) - 70;
                    Cure3 /= 2.2;
                    Cure3 = Math.Floor ( Cure3 + 130 );
                }
                else if ( Power >= 125 && Power < 200 )
                {
                    Cure3 = ( 0 + Power ) - 125;
                    Cure3 /= 1.15;
                    Cure3 = Math.Floor ( Cure3 + 155 );
                }
                else if ( Power >= 200 && Power < 300 )
                {
                    Cure3 = ( 0 + Power ) - 200;
                    Cure3 /= 2.5;
                    Cure3 = Math.Floor ( Cure3 + 220 );
                }
                else if ( Power >= 300 && Power < 700 )
                {
                    Cure3 = ( 0 + Power ) - 300;
                    Cure3 /= 5;
                    Cure3 = Math.Floor ( Cure3 + 260 );
                }
                else if ( Power >= 700 )
                {
                    Cure3 = 340;
                }

                double Cure3_pot = Cure3 * 00.01;
                Cure3_pot *= potency;

                double Cure3_mathed = Math.Round(Cure3 + Cure3_pot);
                Cure3_mathed -= ( Cure3_mathed * 0.10 );

                double Cure4 = 0;

                if ( Power >= 70 && Power < 200 )
                {
                    Cure4 = ( 0 + Power ) - 70;
                    Cure4 /= 1;
                    Cure4 = Math.Floor ( Cure4 + 270 );
                }
                else if ( Power >= 200 && Power < 300 )
                {
                    Cure4 = ( 0 + Power ) - 200;
                    Cure4 /= 2;
                    Cure4 = Math.Floor ( Cure4 + 400 );
                }
                else if ( Power >= 300 && Power < 400 )
                {
                    Cure4 = ( 0 + Power ) - 300;
                    Cure4 /= 1.43;
                    Cure4 = Math.Floor ( Cure4 + 450 );
                }
                else if ( Power >= 400 && Power < 700 )
                {
                    Cure4 = ( 0 + Power ) - 400;
                    Cure4 /= 2.5;
                    Cure4 = Math.Floor ( Cure4 + 520 );
                }
                else if ( Power >= 700 )
                {
                    Cure4 = 640;
                }

                double Cure4_pot = Cure4 * 00.01;
                Cure4_pot *= potency;

                double Cure4_mathed = Math.Round(Cure4 + Cure4_pot);
                Cure4_mathed -= ( Cure4_mathed * 0.10 );

                double Cure5 = 0;

                if ( Power >= 80 && Power < 150 )
                {
                    Cure5 = ( 0 + Power ) - 80;
                    Cure5 /= 0.7;
                    Cure5 = Math.Floor ( Cure5 + 450 );
                }
                else if ( Power >= 150 && Power < 190 )
                {
                    Cure5 = ( 0 + Power ) - 150;
                    Cure5 /= 1.25;
                    Cure5 = Math.Floor ( Cure5 + 550 );
                }
                else if ( Power >= 190 && Power < 260 )
                {
                    Cure5 = ( 0 + Power ) - 190;
                    Cure5 /= 1.84;
                    Cure5 = Math.Floor ( Cure5 + 582 );
                }
                else if ( Power >= 260 && Power < 300 )
                {
                    Cure5 = ( 0 + Power ) - 260;
                    Cure5 /= 2;
                    Cure5 = Math.Floor ( Cure5 + 620 );
                }
                else if ( Power >= 300 && Power < 500 )
                {
                    Cure5 = ( 0 + Power ) - 300;
                    Cure5 /= 2.5;
                    Cure5 = Math.Floor ( Cure5 + 640 );
                }
                else if ( Power >= 500 && Power < 700 )
                {
                    Cure5 = ( 0 + Power ) - 500;
                    Cure5 /= 3.33;
                    Cure5 = Math.Floor ( Cure5 + 720 );
                }
                else if ( Power >= 700 )
                {
                    Cure5 = 780;
                }

                double Cure5_pot = Cure5 * 00.01;
                Cure5_pot *= potency;

                double Cure5_mathed = Math.Round(Cure5 + Cure5_pot);
                Cure5_mathed -= ( Cure5_mathed * 0.10 );

                double Cure6 = 0;

                if ( Power >= 90 && Power < 210 )
                {
                    Cure6 = ( 0 + Power ) - 90;
                    Cure6 /= 1.5;
                    Cure6 = Math.Floor ( Cure6 + 600 );
                }
                else if ( Power >= 210 && Power < 300 )
                {
                    Cure6 = ( 0 + Power ) - 210;
                    Cure6 /= 0.9;
                    Cure6 = Math.Floor ( Cure6 + 680 );
                }
                else if ( Power >= 300 && Power < 400 )
                {
                    Cure6 = ( 0 + Power ) - 300;
                    Cure6 /= 1.43;
                    Cure6 = Math.Floor ( Cure6 + 780 );
                }
                else if ( Power >= 400 && Power < 500 )
                {
                    Cure6 = ( 0 + Power ) - 400;
                    Cure6 /= 2.5;
                    Cure6 = Math.Floor ( Cure6 + 850 );
                }
                else if ( Power >= 500 && Power < 700 )
                {
                    Cure6 = ( 0 + Power ) - 500;
                    Cure6 /= 1.67;
                    Cure6 = Math.Floor ( Cure6 + 890 );
                }
                else if ( Power >= 700 )
                {
                    Cure6 = 1010;
                }

                double Cure6_pot = Cure6 * 00.01;
                Cure6_pot *= potency;

                double Cure6_mathed = Math.Round(Cure6 + Cure6_pot);
                Cure6_mathed -= ( Cure6_mathed * 0.10 );

                cure1amount.Value = Convert.ToDecimal ( Cure_mathed );
                cure2amount.Value = Convert.ToDecimal ( Cure2_mathed );
                cure3amount.Value = Convert.ToDecimal ( Cure3_mathed );
                cure4amount.Value = Convert.ToDecimal ( Cure4_mathed );
                cure5amount.Value = Convert.ToDecimal ( Cure5_mathed );
                cure6amount.Value = Convert.ToDecimal ( Cure6_mathed );

                curagaAmount.Value = Convert.ToDecimal ( Cure2_mathed );
                curaga2Amount.Value = Convert.ToDecimal ( Cure3_mathed );
                curaga3Amount.Value = Convert.ToDecimal ( Cure4_mathed );
                curaga4Amount.Value = Convert.ToDecimal ( Cure5_mathed );
                curaga5Amount.Value = Convert.ToDecimal ( Cure6_mathed );
            }
            else
            {
                MessageBox.Show ( "Select a PL from the main screen before running this." );
            }
        }

        protected override bool ProcessCmdKey ( ref Message msg, Keys keyData )
        {
            if ( keyData == ( Keys.Control | Keys.S ) )
            {
                loadButton.PerformClick ( );
            }
            else if ( keyData == ( Keys.Control | Keys.O ) )
            {
                saveAsButton.PerformClick ( );
            }
            else if ( keyData == ( Keys.Escape ) )
            {
                button4.PerformClick ( );
            }
            return base.ProcessCmdKey ( ref msg, keyData );
        }

        private void NaErase_CheckedChanged ( object sender, EventArgs e )
        {
            if ( naErase.Checked == true )
            {
                na_Weight.Enabled = true;
                na_VitDown.Enabled = true;
                na_Threnody.Enabled = true;
                na_Slow.Enabled = true;
                na_Shock.Enabled = true;
                na_StrDown.Enabled = true;
                na_Requiem.Enabled = true;
                na_Rasp.Enabled = true;
                na_MaxTpDown.Enabled = true;
                na_MaxMpDown.Enabled = true;
                na_MaxHpDown.Enabled = true;
                na_MagicAttackDown.Enabled = true;
                na_MagicDefenseDown.Enabled = true;
                na_MagicAccDown.Enabled = true;
                na_MndDown.Enabled = true;
                na_IntDown.Enabled = true;
                na_Helix.Enabled = true;
                na_Frost.Enabled = true;
                na_EvasionDown.Enabled = true;
                na_Elegy.Enabled = true;
                na_Drown.Enabled = true;
                na_Dia.Enabled = true;
                na_DefenseDown.Enabled = true;
                na_DexDown.Enabled = true;
                na_Choke.Enabled = true;
                na_ChrDown.Enabled = true;
                na_Burn.Enabled = true;
                na_Bio.Enabled = true;
                na_Bind.Enabled = true;
                na_AttackDown.Enabled = true;
                na_Addle.Enabled = true;
                na_AccuracyDown.Enabled = true;
                na_AgiDown.Enabled = true;

                na_Weight.Checked = true;
                na_VitDown.Checked = true;
                na_Threnody.Checked = true;
                na_Slow.Checked = true;
                na_Shock.Checked = true;
                na_StrDown.Checked = true;
                na_Requiem.Checked = true;
                na_Rasp.Checked = true;
                na_MaxTpDown.Checked = true;
                na_MaxMpDown.Checked = true;
                na_MaxHpDown.Checked = true;
                na_MagicAttackDown.Checked = true;
                na_MagicDefenseDown.Checked = true;
                na_MagicAccDown.Checked = true;
                na_MndDown.Checked = true;
                na_IntDown.Checked = true;
                na_Helix.Checked = true;
                na_Frost.Checked = true;
                na_EvasionDown.Checked = true;
                na_Elegy.Checked = true;
                na_Drown.Checked = true;
                na_Dia.Checked = true;
                na_DefenseDown.Checked = true;
                na_DexDown.Checked = true;
                na_Choke.Checked = true;
                na_ChrDown.Checked = true;
                na_Burn.Checked = true;
                na_Bio.Checked = true;
                na_Bind.Checked = true;
                na_AttackDown.Checked = true;
                na_Addle.Checked = true;
                na_AccuracyDown.Checked = true;
                na_AgiDown.Checked = true;
            }
            else
            {
                na_Weight.Checked = false;
                na_VitDown.Checked = false;
                na_Threnody.Checked = false;
                na_Slow.Checked = false;
                na_Shock.Checked = false;
                na_StrDown.Checked = false;
                na_Requiem.Checked = false;
                na_Rasp.Checked = false;
                na_MaxTpDown.Checked = false;
                na_MaxMpDown.Checked = false;
                na_MaxHpDown.Checked = false;
                na_MagicAttackDown.Checked = false;
                na_MagicDefenseDown.Checked = false;
                na_MagicAccDown.Checked = false;
                na_MndDown.Checked = false;
                na_IntDown.Checked = false;
                na_Helix.Checked = false;
                na_Frost.Checked = false;
                na_EvasionDown.Checked = false;
                na_Elegy.Checked = false;
                na_Drown.Checked = false;
                na_Dia.Checked = false;
                na_DefenseDown.Checked = false;
                na_DexDown.Checked = false;
                na_Choke.Checked = false;
                na_ChrDown.Checked = false;
                na_Burn.Checked = false;
                na_Bio.Checked = false;
                na_Bind.Checked = false;
                na_AttackDown.Checked = false;
                na_Addle.Checked = false;
                na_AccuracyDown.Checked = false;
                na_AgiDown.Checked = false;

                na_Weight.Enabled = false;
                na_VitDown.Enabled = false;
                na_Threnody.Enabled = false;
                na_Slow.Enabled = false;
                na_Shock.Enabled = false;
                na_StrDown.Enabled = false;
                na_Requiem.Enabled = false;
                na_Rasp.Enabled = false;
                na_MaxTpDown.Enabled = false;
                na_MaxMpDown.Enabled = false;
                na_MaxHpDown.Enabled = false;
                na_MagicAttackDown.Enabled = false;
                na_MagicDefenseDown.Enabled = false;
                na_MagicAccDown.Enabled = false;
                na_MndDown.Enabled = false;
                na_IntDown.Enabled = false;
                na_Helix.Enabled = false;
                na_Frost.Enabled = false;
                na_EvasionDown.Enabled = false;
                na_Elegy.Enabled = false;
                na_Drown.Enabled = false;
                na_Dia.Enabled = false;
                na_DefenseDown.Enabled = false;
                na_DexDown.Enabled = false;
                na_Choke.Enabled = false;
                na_ChrDown.Enabled = false;
                na_Burn.Enabled = false;
                na_Bio.Enabled = false;
                na_Bind.Enabled = false;
                na_AttackDown.Enabled = false;
                na_Addle.Enabled = false;
                na_AccuracyDown.Enabled = false;
                na_AgiDown.Enabled = false;
            }
        }

        private Dictionary<StatusEffect, bool> DebuffEnabled => new()
        {
            { StatusEffect.Doom, true},
            { StatusEffect.Sleep, true },
            { StatusEffect.Sleep2, true },
            { StatusEffect.Petrification, naPetrification.Checked },
            { StatusEffect.Silence, naSilence.Checked },
            { StatusEffect.Paralysis, naParalysis.Checked },
            { StatusEffect.Amnesia, Esuna.Checked },
            { StatusEffect.Blindness, naBlindness.Checked },
            { StatusEffect.Bind, na_Bind.Checked },
            { StatusEffect.Weight, na_Weight.Checked },
            { StatusEffect.Slow, na_Slow.Checked },
            { StatusEffect.Poison, naPoison.Checked },
            { StatusEffect.Attack_Down, na_AttackDown.Checked },
            { StatusEffect.Curse, naCurse.Checked },
            { StatusEffect.Curse2, naCurse.Checked },
            { StatusEffect.Addle, na_Addle.Checked },
            { StatusEffect.Bane, naCurse.Checked },
            { StatusEffect.Plague, naPlague.Checked },
            { StatusEffect.Disease, naPlague.Checked },
            { StatusEffect.Burn, na_Burn.Checked },
            { StatusEffect.Frost, na_Frost.Checked },
            { StatusEffect.Choke, na_Choke.Checked },
            { StatusEffect.Rasp, na_Rasp.Checked},
            { StatusEffect.Shock, na_Shock.Checked },
            { StatusEffect.Drown, na_Drown.Checked},
            { StatusEffect.Dia, na_Dia.Checked},
            { StatusEffect.Bio, na_Bio.Checked },
            { StatusEffect.STR_Down, na_StrDown.Checked },
            { StatusEffect.DEX_Down, na_DexDown.Checked },
            { StatusEffect.VIT_Down, na_VitDown.Checked },
            { StatusEffect.AGI_Down, na_AgiDown.Checked},
            { StatusEffect.INT_Down, na_IntDown.Checked},
            { StatusEffect.MND_Down, na_MndDown.Checked },
            { StatusEffect.CHR_Down, na_ChrDown.Checked},
            { StatusEffect.Max_HP_Down, na_MaxHpDown.Checked},
            { StatusEffect.Max_MP_Down, na_MaxMpDown.Checked },
            { StatusEffect.Accuracy_Down, na_AccuracyDown.Checked },
            { StatusEffect.Evasion_Down, na_EvasionDown.Checked },
            { StatusEffect.Defense_Down, na_DefenseDown.Checked },
            { StatusEffect.Flash, plFlash.Checked },
            { StatusEffect.Magic_Acc_Down, na_MagicAccDown.Checked },
            { StatusEffect.Magic_Atk_Down, na_MagicAttackDown.Checked },
            { StatusEffect.Helix, na_Helix.Checked },
            { StatusEffect.Max_TP_Down, na_MaxTpDown.Checked },
            { StatusEffect.Requiem, na_Requiem.Checked },
            { StatusEffect.Elegy, na_Elegy.Checked },
            { StatusEffect.Threnody, na_Threnody.Checked }
        };
    }
}
