using System;

namespace CurePlease.Model.Config
{
    [Serializable]
    public class MySettings
    {
        // BASE NEEDED FOR CONFIRMATION
        public bool settingsSet
        {
            get; set;
        }

        // HEALING SPELLS TAB
        public bool cure1enabled
        {
            get; set;
        }

        public int cure1amount
        {
            get; set;
        }

        public bool cure2enabled
        {
            get; set;
        }

        public int cure2amount
        {
            get; set;
        }

        public bool cure3enabled
        {
            get; set;
        }

        public int cure3amount
        {
            get; set;
        }

        public bool cure4enabled
        {
            get; set;
        }

        public int cure4amount
        {
            get; set;
        }

        public bool cure5enabled
        {
            get; set;
        }

        public int cure5amount
        {
            get; set;
        }

        public bool cure6enabled
        {
            get; set;
        }

        public int cure6amount
        {
            get; set;
        }

        public bool curagaEnabled
        {
            get; set;
        }

        public int curagaAmount
        {
            get; set;
        }

        public bool curaga2enabled
        {
            get; set;
        }

        public int curaga2Amount
        {
            get; set;
        }

        public bool curaga3enabled
        {
            get; set;
        }

        public int curaga3Amount
        {
            get; set;
        }

        public bool curaga4enabled
        {
            get; set;
        }

        public int curaga4Amount
        {
            get; set;
        }

        public bool curaga5enabled
        {
            get; set;
        }

        public int curaga5Amount
        {
            get; set;
        }

        public int curePercentage
        {
            get; set;
        }

        public int priorityCurePercentage
        {
            get; set;
        }

        public int monitoredCurePercentage
        {
            get; set;
        }

        public int curagaCurePercentage
        {
            get; set;
        }

        public int curagaTargetType
        {
            get; set;
        }

        public string curagaTargetName
        {
            get; set;
        }

        public decimal curagaRequiredMembers
        {
            get; set;
        }

        // ENHANCING MAGIC TAB / BASIC
        public decimal autoHasteMinutes
        {
            get; set;
        }

        public decimal autoAdloquiumMinutes
        {
            get; set;
        }

        public decimal autoPhalanxIIMinutes
        {
            get; set;
        }

        public decimal autoStormspellMinutes
        {
            get; set;
        }

        public decimal autoRefresh_Minutes
        {
            get; set;
        }

        public int autoRefresh_Spell
        {
            get; set;
        }

        public decimal autoRegen_Minutes
        {
            get; set;
        }

        public int autoRegen_Spell
        {
            get; set;
        }

        public decimal autoProtect_Minutes
        {
            get; set;
        }

        public int autoProtect_Spell
        {
            get; set;
        }

        public decimal autoShellMinutes
        {
            get; set;
        }

        public int autoShell_Spell
        {
            get; set;
        }
        public int autoStorm_Spell
        {
            get; set;
        }

        public bool plShellra
        {
            get; set;
        }

        public decimal plShellra_Level
        {
            get; set;
        }

        public bool plProtectra
        {
            get; set;
        }

        public decimal plProtectra_Level
        {
            get; set;
        }

        public bool plGainBoost
        {
            get; set;
        }

        public int plGainBoost_Spell
        {
            get; set;
        }

        public bool plBarElement
        {
            get; set;
        }

        public int plBarElement_Spell
        {
            get; set;
        }

        public bool AOE_Barelemental
        {
            get; set;
        }

        public bool plBarStatus
        {
            get; set;
        }

        public int plBarStatus_Spell
        {
            get; set;
        }

        public bool AOE_Barstatus
        {
            get; set;
        }

        public bool plAuspice
        {
            get; set;
        }

        public bool plRegen
        {
            get; set;
        }

        public int plRegen_Level
        {
            get; set;
        }

        public bool plReraise
        {
            get; set;
        }

        public int plReraise_Level
        {
            get; set;
        }

        public bool plRefresh
        {
            get; set;
        }

        public int plRefresh_Level
        {
            get; set;
        }

        public bool plProtect
        {
            get; set;
        }

        public bool plShell
        {
            get; set;
        }

        public bool plBlink
        {
            get; set;
        }

        public bool plPhalanx
        {
            get; set;
        }

        public bool plStoneskin
        {
            get; set;
        }

        public bool plTemper
        {
            get; set;
        }

        public int plTemper_Level
        {
            get; set;
        }

        public bool plEnspell
        {
            get; set;
        }

        public int plEnspell_Spell
        {
            get; set;
        }

        public bool plStormSpell
        {
            get; set;
        }

        public int plStormSpell_Spell
        {
            get; set;
        }

        public bool plAdloquium
        {
            get; set;
        }

        public bool plKlimaform
        {
            get; set;
        }

        public bool plAquaveil
        {
            get; set;
        }

        public bool plHaste
        {
            get; set;
        }

        public int plHaste_Level
        {
            get; set;
        }

        public bool plSpikes
        {
            get; set;
        }

        public int plSpikes_Spell
        {
            get; set;
        }

        public bool plUtsusemi
        {
            get; set;
        }

        // ENHANCING MAGIC TAB / SCHOLAR

        public bool accessionCure
        {
            get; set;
        }

        public bool accessionProShell
        {
            get; set;
        }

        public bool AccessionRegen
        {
            get; set;
        }

        public bool PerpetuanceRegen
        {
            get; set;
        }


        public bool regenPerpetuance
        {
            get; set;
        }

        public bool regenAccession
        {
            get; set;
        }




        public bool refreshPerpetuance
        {
            get; set;
        }

        public bool refreshAccession
        {
            get; set;
        }

        public bool blinkPerpetuance
        {
            get; set;
        }

        public bool blinkAccession
        {
            get; set;
        }

        public bool phalanxPerpetuance
        {
            get; set;
        }

        public bool phalanxAccession
        {
            get; set;
        }

        public bool stoneskinPerpetuance
        {
            get; set;
        }

        public bool stoneskinAccession
        {
            get; set;
        }

        public bool enspellPerpetuance
        {
            get; set;
        }

        public bool enspellAccession
        {
            get; set;
        }

        public bool stormspellPerpetuance
        {
            get; set;
        }

        public bool stormspellAccession
        {
            get; set;
        }

        public bool adloquiumPerpetuance
        {
            get; set;
        }

        public bool adloquiumAccession
        {
            get; set;
        }

        public bool aquaveilPerpetuance
        {
            get; set;
        }

        public bool aquaveilAccession
        {
            get; set;
        }

        public bool barspellPerpetuance
        {
            get; set;
        }

        public bool barspellAccession
        {
            get; set;
        }

        public bool barstatusPerpetuance
        {
            get; set;
        }

        public bool barstatusAccession
        {
            get; set;
        }

        public bool EnlightenmentReraise
        {
            get; set;
        }

        // GEOMANCY MAGIC TAB
        public bool EnableIndiSpells
        {
            get; set;
        }

        public bool EngagedOnly
        {
            get; set;
        }

        public bool EnableLuopanSpells
        {
            get; set;
        }

        public bool GeoWhenEngaged
        {
            get; set;
        }

        public bool specifiedEngageTarget
        {
            get; set;
        }

        public int IndiSpell_Spell
        {
            get; set;
        }

        public int GeoSpell_Spell
        {
            get; set;
        }

        public int EntrustedSpell_Spell
        {
            get; set;
        }

        public string LuopanSpell_Target
        {
            get; set;
        }

        public string EntrustedSpell_Target
        {
            get; set;
        }

        // SINGING MAGIC TAB
        public bool enableSinging
        {
            get; set;
        }

        public bool recastSongs_Monitored
        {
            get; set;
        }

        public bool SongsOnlyWhenNear
        {
            get; set;
        }

        public int song1
        {
            get; set;
        }

        public int song2
        {
            get; set;
        }

        public int song3
        {
            get; set;
        }

        public int song4
        {
            get; set;
        }

        public int dummy1
        {
            get; set;
        }

        public int dummy2
        {
            get; set;
        }

        public decimal recastSongTime
        {
            get; set;
        }

        // JOB ABILITIES

        // SCH
        public bool LightArts
        {
            get; set;
        }

        public bool Sublimation
        {
            get; set;
        }

        public bool AddendumWhite
        {
            get; set;
        }

        public bool Celerity
        {
            get; set;
        }

        public bool Accession
        {
            get; set;
        }

        public bool Perpetuance
        {
            get; set;
        }

        public bool Penury
        {
            get; set;
        }

        public bool Rapture
        {
            get; set;
        }
        public bool DarkArts
        {
            get; set;
        }

        public bool AddendumBlack
        {
            get; set;
        }

        // WHM
        public bool AfflatusSolace
        {
            get; set;
        }

        public bool AfflatusMisery
        {
            get; set;
        }

        public bool DivineSeal
        {
            get; set;
        }

        public bool Devotion
        {
            get; set;
        }

        public bool DivineCaress
        {
            get; set;
        }

        // RDM
        public bool Composure
        {
            get; set;
        }

        public bool Convert
        {
            get; set;
        }

        // GEO
        public bool EntrustEnabled
        {
            get; set;
        }

        public bool FullCircle
        {
            get; set;
        }

        public bool Dematerialize
        {
            get; set;
        }

        public bool BlazeOfGlory
        {
            get; set;
        }

        public bool RadialArcana
        {
            get; set;
        }

        public bool EclipticAttrition
        {
            get; set;
        }

        public bool LifeCycle
        {
            get; set;
        }

        // BRD
        public bool Pianissimo
        {
            get; set;
        }

        public bool Nightingale
        {
            get; set;
        }

        public bool Troubadour
        {
            get; set;
        }

        public bool Marcato
        {
            get; set;
        }

        // DEBUFF REMOVAL
        public bool plDebuffEnabled
        {
            get; set;
        }

        public bool monitoredDebuffEnabled
        {
            get; set;
        }

        public bool enablePartyDebuffRemoval
        {
            get; set;
        }

        public bool SpecifiednaSpellsenable
        {
            get; set;
        }

        public bool PrioritiseOverLowerTier
        {
            get; set;
        }

        public bool plSilenceItemEnabled
        {
            get; set;
        }

        public int plSilenceItem
        {
            get; set;
        }

        public bool plDoomEnabled
        {
            get; set;
        }

        public int plDoomitem
        {
            get; set;
        }

        public bool wakeSleepEnabled
        {
            get; set;
        }

        public int wakeSleepSpell
        {
            get; set;
        }

        // PARTY DEBUFFS
        public bool naBlindness
        {
            get; set;
        }

        public bool naCurse
        {
            get; set;
        }

        public bool naDisease
        {
            get; set;
        }

        public bool naParalysis
        {
            get; set;
        }

        public bool naPetrification
        {
            get; set;
        }

        public bool naPlague
        {
            get; set;
        }

        public bool naPoison
        {
            get; set;
        }

        public bool naSilence
        {
            get; set;
        }

        public bool naErase
        {
            get; set;
        }

        public bool Esuna
        {
            get;
            set;
        }

        public bool EsunaOnlyAmnesia
        {
            get;
            set;
        }

        // PL DEBUFFS
        public bool plAgiDown
        {
            get; set;
        }

        public bool plAccuracyDown
        {
            get; set;
        }

        public bool plAddle
        {
            get; set;
        }

        public bool plAttackDown
        {
            get; set;
        }

        public bool plBane
        {
            get; set;
        }

        public bool plBind
        {
            get; set;
        }

        public bool plBio
        {
            get; set;
        }

        public bool plBlindness
        {
            get; set;
        }

        public bool plBurn
        {
            get; set;
        }

        public bool plChrDown
        {
            get; set;
        }

        public bool plChoke
        {
            get; set;
        }

        public bool plCurse
        {
            get; set;
        }

        public bool plCurse2
        {
            get; set;
        }

        public bool plDexDown
        {
            get; set;
        }

        public bool plDefenseDown
        {
            get; set;
        }

        public bool plDia
        {
            get; set;
        }

        public bool plDisease
        {
            get; set;
        }

        public bool plDoom
        {
            get; set;
        }

        public bool plDrown
        {
            get; set;
        }

        public bool plElegy
        {
            get; set;
        }

        public bool plEvasionDown
        {
            get; set;
        }

        public bool plFlash
        {
            get; set;
        }

        public bool plFrost
        {
            get; set;
        }

        public bool plHelix
        {
            get; set;
        }

        public bool plIntDown
        {
            get; set;
        }

        public bool plMndDown
        {
            get; set;
        }

        public bool plMagicAccDown
        {
            get; set;
        }

        public bool plMagicAtkDown
        {
            get; set;
        }

        public bool plMaxHpDown
        {
            get; set;
        }

        public bool plMaxMpDown
        {
            get; set;
        }

        public bool plMaxTpDown
        {
            get; set;
        }

        public bool plParalysis
        {
            get; set;
        }

        public bool plPlague
        {
            get; set;
        }

        public bool plPoison
        {
            get; set;
        }

        public bool plRasp
        {
            get; set;
        }

        public bool plRequiem
        {
            get; set;
        }

        public bool plStrDown
        {
            get; set;
        }

        public bool plShock
        {
            get; set;
        }

        public bool plSilence
        {
            get; set;
        }

        public bool plSlow
        {
            get; set;
        }

        public bool plThrenody
        {
            get; set;
        }

        public bool plVitDown
        {
            get; set;
        }

        public bool plWeight
        {
            get; set;
        }

        public bool plAmnesia
        {
            get;
            set;
        }

        // MONITORED DEBUFFS
        public bool monitoredAgiDown
        {
            get; set;
        }

        public bool monitoredAccuracyDown
        {
            get; set;
        }

        public bool monitoredAddle
        {
            get; set;
        }

        public bool monitoredAttackDown
        {
            get; set;
        }

        public bool monitoredBane
        {
            get; set;
        }

        public bool monitoredBind
        {
            get; set;
        }

        public bool monitoredBio
        {
            get; set;
        }

        public bool monitoredBlindness
        {
            get; set;
        }

        public bool monitoredBurn
        {
            get; set;
        }

        public bool monitoredChrDown
        {
            get; set;
        }

        public bool monitoredChoke
        {
            get; set;
        }

        public bool monitoredCurse
        {
            get; set;
        }

        public bool monitoredCurse2
        {
            get; set;
        }

        public bool monitoredDexDown
        {
            get; set;
        }

        public bool monitoredDefenseDown
        {
            get; set;
        }

        public bool monitoredDia
        {
            get; set;
        }

        public bool monitoredDisease
        {
            get; set;
        }

        public bool monitoredDoom
        {
            get; set;
        }

        public bool monitoredDrown
        {
            get; set;
        }

        public bool monitoredElegy
        {
            get; set;
        }

        public bool monitoredEvasionDown
        {
            get; set;
        }

        public bool monitoredFlash
        {
            get; set;
        }

        public bool monitoredFrost
        {
            get; set;
        }

        public bool monitoredHelix
        {
            get; set;
        }

        public bool monitoredIntDown
        {
            get; set;
        }

        public bool monitoredMndDown
        {
            get; set;
        }

        public bool monitoredMagicAccDown
        {
            get; set;
        }

        public bool monitoredMagicAtkDown
        {
            get; set;
        }

        public bool monitoredMaxHpDown
        {
            get; set;
        }

        public bool monitoredMaxMpDown
        {
            get; set;
        }

        public bool monitoredMaxTpDown
        {
            get; set;
        }

        public bool monitoredParalysis
        {
            get; set;
        }

        public bool monitoredPetrification
        {
            get; set;
        }

        public bool monitoredPlague
        {
            get; set;
        }

        public bool monitoredPoison
        {
            get; set;
        }

        public bool monitoredRasp
        {
            get; set;
        }

        public bool monitoredRequiem
        {
            get; set;
        }

        public bool monitoredStrDown
        {
            get; set;
        }

        public bool monitoredShock
        {
            get; set;
        }

        public bool monitoredSilence
        {
            get; set;
        }

        public bool monitoredSleep
        {
            get; set;
        }

        public bool monitoredSleep2
        {
            get; set;
        }

        public bool monitoredSlow
        {
            get; set;
        }

        public bool monitoredThrenody
        {
            get; set;
        }

        public bool monitoredVitDown
        {
            get; set;
        }

        public bool monitoredWeight
        {
            get; set;
        }

        public bool monitoredAmnesia
        {
            get;
            set;
        }

        // NA SPECIFICATION CHECKBOXES

        public bool na_Weight
        {
            get; set;
        }

        public bool na_VitDown
        {
            get; set;
        }

        public bool na_Threnody
        {
            get; set;
        }

        public bool na_Slow
        {
            get; set;
        }

        public bool na_Shock
        {
            get; set;
        }

        public bool na_StrDown
        {
            get; set;
        }

        public bool na_Requiem
        {
            get; set;
        }

        public bool na_Rasp
        {
            get; set;
        }

        public bool na_MaxTpDown
        {
            get; set;
        }

        public bool na_MaxMpDown
        {
            get; set;
        }

        public bool na_MaxHpDown
        {
            get; set;
        }

        public bool na_MagicAttackDown
        {
            get; set;
        }

        public bool na_MagicAccDown
        {
            get; set;
        }

        public bool na_MagicDefenseDown
        {
            get; set;
        }

        public bool na_MndDown
        {
            get; set;
        }

        public bool na_IntDown
        {
            get; set;
        }

        public bool na_Helix
        {
            get; set;
        }

        public bool na_Frost
        {
            get; set;
        }

        public bool na_EvasionDown
        {
            get; set;
        }

        public bool na_Elegy
        {
            get; set;
        }

        public bool na_Drown
        {
            get; set;
        }

        public bool na_Dia
        {
            get; set;
        }

        public bool na_DefenseDown
        {
            get; set;
        }

        public bool na_DexDown
        {
            get; set;
        }

        public bool na_Choke
        {
            get; set;
        }

        public bool na_ChrDown
        {
            get; set;
        }

        public bool na_Burn
        {
            get; set;
        }

        public bool na_Bio
        {
            get; set;
        }

        public bool na_Bind
        {
            get; set;
        }

        public bool na_AttackDown
        {
            get; set;
        }

        public bool na_Addle
        {
            get; set;
        }

        public bool na_AccuracyDown
        {
            get; set;
        }

        public bool na_AgiDown
        {
            get; set;
        }

        // OTHER SETTINGS

        // MP OPTIONS
        public decimal mpMinCastValue
        {
            get; set;
        }

        public bool lowMPcheckBox
        {
            get; set;
        }

        public bool healLowMP
        {
            get; set;
        }

        public decimal healWhenMPBelow
        {
            get; set;
        }

        public bool standAtMP
        {
            get; set;
        }

        public decimal standAtMP_Percentage
        {
            get; set;
        }

        // CONVERT SETTINGS
        public decimal convertMP
        {
            get; set;
        }

        // SUBLIMATION SETTINGS
        public decimal sublimationMP
        {
            get; set;
        }

        // FULL CIRCLE SETTINGS
        public bool Fullcircle_DisableEnemy
        {
            get; set;
        }
        public bool Fullcircle_GEOTarget
        {
            get; set;
        }

        // RADIAL ARCANA SETTINGS
        public decimal RadialArcanaMP
        {
            get; set;
        }

        public int RadialArcana_Spell
        {
            get; set;
        }

        // DEVOTION SETTINGS
        public decimal DevotionMP
        {
            get; set;
        }

        public int DevotionTargetType
        {
            get; set;
        }

        public string DevotionTargetName
        {
            get; set;
        }

        public bool DevotionWhenEngaged
        {
            get; set;
        }

        //AUTO CASTING SPELLS OPTIONS
        public bool autoTarget
        {
            get; set;
        }

        public int Hate_SpellType
        {
            get; set;
        }

        public string autoTargetSpell
        {
            get; set;
        }

        public string autoTarget_Target
        {
            get; set;
        }

        public bool AssistSpecifiedTarget
        {
            get; set;
        }

        // DISABLE CANCEL TARGETTING
        public bool DisableTargettingCancel
        {
            get; set;
        }

        // DELAY BEFORE REMOVING TARGET
        public decimal TargetRemoval_Delay
        {
            get; set;
        }

        // RAISE SETTINGS
        public bool AcceptRaise
        {
            get; set;
        }

        public bool AcceptRaiseOnlyWhenNotInCombat
        {
            get; set;
        }

        // CURING OPTIONS
        public bool Overcure
        {
            get; set;
        }

        public bool Undercure
        {
            get; set;
        }

        public bool enableMonitoredPriority
        {
            get; set;
        }

        public bool enableOutOfPartyHealing
        {
            get; set;
        }

        public bool OvercureOnHighPriority
        {
            get; set;
        }

        public bool EnableAddOn
        {
            get; set;
        }

        // PROGRAM OPTIONS

        // PAUSE OPTIONS
        public bool pauseOnZoneBox
        {
            get; set;
        }

        public bool pauseOnStartBox
        {
            get; set;
        }

        public bool pauseOnKO
        {
            get; set;
        }

        public bool MinimiseonStart
        {
            get; set;
        }

        // AUTO FOLLOW OPTIONS
        public string autoFollowName
        {
            get; set;
        }

        public decimal autoFollowDistance
        {
            get; set;
        }

        public bool autoFollow_Warning
        {
            get; set;
        }

        public bool FFXIDefaultAutoFollow
        {
            get; set;
        }
        public bool enableHotKeys
        {
            get; set;
        }

        // FAST CAST MODE
        public bool enableFastCast_Mode
        {
            get; set;
        }

        // trackCastingPacketsMODE
        public bool trackCastingPackets
        {
            get; set;
        }

        // ADD ON OPTIONS
        public string ipAddress
        {
            get; set;
        }

        public string listeningPort
        {
            get; set;
        }
    }
}
