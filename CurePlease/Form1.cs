namespace CurePlease
{
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

    public partial class Form1 : Form
    {

        private Form2 Form2 = new Form2();

        public class CharacterData : List<CharacterData>
        {
            public int TargetIndex { get; set; }

            public int MemberNumber { get; set; }
        }

        public class SongData : List<SongData>
        {
            public string song_type { get; set; }

            public int song_position { get; set; }

            public string song_name { get; set; }

            public int buff_id { get; set; }
        }

        public class SpellsData : List<SpellsData>
        {
            public string Spell_Name { get; set; }

            public int spell_position { get; set; }

            public int type { get; set; }

            public int buffID { get; set; }

            public bool aoe_version { get; set; }
        }

        public class GeoData : List<GeoData>
        {
            public int geo_position { get; set; }

            public string indi_spell { get; set; }

            public string geo_spell { get; set; }
        }

        public class JobTitles : List<JobTitles>
        {
            public int job_number { get; set; }

            public string job_name { get; set; }
        }

        private string debug_MSG_show = string.Empty;

        private int lastCommand = 0;

        private int lastKnownEstablisherTarget = 0;

        // BARD SONG VARIABLES
        private int song_casting = 0;

        private int PL_BRDCount = 0;
        private bool ForceSongRecast = false;
        private string Last_Song_Cast = string.Empty;


        private uint PL_Index = 0;
        private uint Monitored_Index = 0;


        //  private int song_casting = 0;
        //  private string LastSongCast = Spells.Unknown;


        // private bool ForceSongRecast = false;
        //  private string Last_Song_Cast = Spells.Unknown;


        // GEO ENGAGED CHECK
        public bool targetEngaged = false;

        public bool EclipticStillUp = false;

        public bool CastingBackground_Check = false;
        public bool JobAbilityLock_Check = false;

        public string JobAbilityCMD = string.Empty;

        private DateTime DefaultTime = new DateTime(1970, 1, 1);

        private bool curePlease_autofollow = false;

        private List<string> characterNames_naRemoval = new List<string>();    

        public string WindowerMode = "Windower";

        public List<SpellsData> barspells = new List<SpellsData>();

        public List<SpellsData> enspells = new List<SpellsData>();

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

        public EliteAPI Monitored;

        public ListBox processids = new ListBox();

        public ListBox activeprocessids = new ListBox();

        public double last_percent = 1;

        public string castingSpell = string.Empty;

        public int max_count = 10;
        public int spell_delay_count = 0;

        public int geo_step = 0;

        public int followWarning = 0;

        public bool stuckWarning = false;
        public int stuckCount = 0;

        public int protectionCount = 0;

        public int IDFound = 0;

        public float lastZ;
        public float lastX;
        public float lastY;

        // Stores the previously-colored button, if any
        public Dictionary<string, IEnumerable<short>> ActiveBuffs = new Dictionary<string, IEnumerable<short>>();

        public List<SongData> SongInfo = new List<SongData>();

        public List<GeoData> GeomancerInfo = new List<GeoData>();

        public List<int> known_song_buffs = new List<int>();

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

        public bool IsCure(string spell)
        {
            return spell.Contains("Cure");
        }

        // SPELL CHECKER CODE: (PL.SpellAvailable("") == 0) && (PL.SpellAvailable(""))
        // ABILITY CHECKER CODE: (GetAbilityRecast("") == 0) && (PL.AbilityAvailable(""))
        // PIANISSIMO TIME FORMAT
        // SONGNUMBER_SONGSET (Example: 1_2 = Song #1 in Set #2
        private bool[] autoHasteEnabled = new bool[]
      {
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false
      };

        private bool[] autoHaste_IIEnabled = new bool[]
      {
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false
      };

        private bool[] autoFlurryEnabled = new bool[]
      {
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false
      };

        private bool[] autoFlurry_IIEnabled = new bool[]
      {
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false
      };

        private bool[] autoPhalanx_IIEnabled = new bool[]
       {
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false
       };

        private bool[] autoRegen_Enabled = new bool[]
      {
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false
      };

        private bool[] autoShell_Enabled = new bool[]
      {
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false
      };

        private bool[] autoProtect_Enabled = new bool[]
      {
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false
      };

        private bool[] autoSandstormEnabled = new bool[]
{
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false
};

        private bool[] autoRainstormEnabled = new bool[]
{
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false
};

        private bool[] autoWindstormEnabled = new bool[]
{
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false
};

        private bool[] autoFirestormEnabled = new bool[]
{
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false
};

        private bool[] autoHailstormEnabled = new bool[]
{
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false
};

        private bool[] autoThunderstormEnabled = new bool[]
{
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false
};

        private bool[] autoVoidstormEnabled = new bool[]
{
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false
};

        private bool[] autoAurorastormEnabled = new bool[]
{
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false
};



        private bool[] autoRefreshEnabled = new bool[]
      {
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false
      };

        private bool[] autoAdloquium_Enabled = new bool[]
      {
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false
      };



        private DateTime currentTime = DateTime.Now;

        private DateTime[] playerHaste = new DateTime[]
      {
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0)
      };

        private DateTime[] playerHaste_II = new DateTime[]
      {
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0)
      };

        private DateTime[] playerStormspell = new DateTime[]
      {
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0)
      };

        private DateTime[] playerFlurry = new DateTime[]
      {
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0)
      };

        private DateTime[] playerFlurry_II = new DateTime[]
      {
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0)
      };

        private DateTime[] playerShell = new DateTime[]
      {
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0)
      };

        private DateTime[] playerProtect = new DateTime[]
      {
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0)
      };

        private DateTime[] playerPhalanx_II = new DateTime[]
      {
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0)
      };

        private DateTime[] playerRegen = new DateTime[]
       {
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0)
       };

        private DateTime[] playerRefresh = new DateTime[]
      {
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0)
      };

        private DateTime[] playerAdloquium = new DateTime[]
      {
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0)
      };

        private DateTime[] playerSong1 = new DateTime[]
      {
            new DateTime(1970, 1, 1, 0, 0, 0)
      };

        private DateTime[] playerSong2 = new DateTime[]
      {
            new DateTime(1970, 1, 1, 0, 0, 0)
      };

        private DateTime[] playerSong3 = new DateTime[]
      {
            new DateTime(1970, 1, 1, 0, 0, 0)
      };

        private DateTime[] playerSong4 = new DateTime[]
      {
            new DateTime(1970, 1, 1, 0, 0, 0)
      };

        private DateTime[] Last_SongCast_Timer = new DateTime[]
      {
            new DateTime(1970, 1, 1, 0, 0, 0)
      };

        private DateTime[] playerPianissimo1_1 = new DateTime[]
      {
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0)
      };

        private DateTime[] playerPianissimo2_1 = new DateTime[]
      {
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0)
      };

        private DateTime[] playerPianissimo1_2 = new DateTime[]
      {
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0)
      };

        private DateTime[] playerPianissimo2_2 = new DateTime[]
      {
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0)
      };

        private TimeSpan[] playerHasteSpan = new TimeSpan[]
      {
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan()
      };

        private TimeSpan[] playerStormspellSpan = new TimeSpan[]
      {
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan()
      };

        private TimeSpan[] playerHaste_IISpan = new TimeSpan[]
      {
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan()
      };

        private TimeSpan[] playerFlurrySpan = new TimeSpan[]
      {
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan()
      };

        private TimeSpan[] playerFlurry_IISpan = new TimeSpan[]
      {
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan()
      };

        private TimeSpan[] playerShell_Span = new TimeSpan[]
      {
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan()
      };

        private TimeSpan[] playerProtect_Span = new TimeSpan[]
      {
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan()
      };

        private TimeSpan[] playerPhalanx_IISpan = new TimeSpan[]
      {
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan()
      };

        private TimeSpan[] playerRegen_Span = new TimeSpan[]
      {
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan()
      };

        private TimeSpan[] playerRefresh_Span = new TimeSpan[]
      {
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan()
      };


        private TimeSpan[] playerAdloquium_Span = new TimeSpan[]
      {
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan()
      };

        private TimeSpan[] playerSong1_Span = new TimeSpan[]
      {
            new TimeSpan()
      };

        private TimeSpan[] playerSong2_Span = new TimeSpan[]
      {
            new TimeSpan()
      };

        private TimeSpan[] playerSong3_Span = new TimeSpan[]
      {
            new TimeSpan()
      };

        private TimeSpan[] playerSong4_Span = new TimeSpan[]
     {
            new TimeSpan()
     };

        private TimeSpan[] Last_SongCast_Timer_Span = new TimeSpan[]
     {
            new TimeSpan()
     };

        private TimeSpan[] pianissimo1_1_Span = new TimeSpan[]
      {
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
      };

        private TimeSpan[] pianissimo2_1_Span = new TimeSpan[]
      {
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
      };

        private TimeSpan[] pianissimo1_2_Span = new TimeSpan[]
      {
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
      };

        private TimeSpan[] pianissimo2_2_Span = new TimeSpan[]
      {
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
      };

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


        public Form1()
        {


            StartPosition = FormStartPosition.CenterScreen;

            InitializeComponent();




            currentAction.Text = string.Empty;

            if (System.IO.File.Exists("debug"))
            {
                debug.Visible = true;
            }

            int position = 0;

            // Buff lists
            known_song_buffs.Add(197);
            known_song_buffs.Add(198);
            known_song_buffs.Add(195);
            known_song_buffs.Add(199);
            known_song_buffs.Add(200);
            known_song_buffs.Add(215);
            known_song_buffs.Add(196);
            known_song_buffs.Add(214);
            known_song_buffs.Add(216);
            known_song_buffs.Add(218);
            known_song_buffs.Add(222);

            SongInfo.Add(new SongData
            {
                song_type = "Blank",
                song_name = "Blank",
                song_position = position,
                buff_id = 0
            });
            position++;

            SongInfo.Add(new SongData
            {
                song_type = "Minne",
                song_name = "Knight's Minne",
                song_position = position,
                buff_id = 197
            });
            position++;

            SongInfo.Add(new SongData
            {
                song_type = "Minne",
                song_name = "Knight's Minne II",
                song_position = position,
                buff_id = 197
            });
            position++;

            SongInfo.Add(new SongData
            {
                song_type = "Minne",
                song_name = "Knight's Minne III",
                song_position = position,
                buff_id = 197
            });
            position++;

            SongInfo.Add(new SongData
            {
                song_type = "Minne",
                song_name = "Knight's Minne IV",
                song_position = position,
                buff_id = 197
            });
            position++;

            SongInfo.Add(new SongData
            {
                song_type = "Minne",
                song_name = "Knight's Minne V",
                song_position = position,
                buff_id = 197
            });
            position++;

            SongInfo.Add(new SongData
            {
                song_type = "Blank",
                song_name = "Blank",
                song_position = position,
                buff_id = 0
            });
            position++;

            SongInfo.Add(new SongData
            {
                song_type = "Minuet",
                song_name = "Valor Minuet",
                song_position = position,
                buff_id = 198
            });
            position++;

            SongInfo.Add(new SongData
            {
                song_type = "Minuet",
                song_name = "Valor Minuet II",
                song_position = position,
                buff_id = 198
            });
            position++;

            SongInfo.Add(new SongData
            {
                song_type = "Minuet",
                song_name = "Valor Minuet III",
                song_position = position,
                buff_id = 198
            });
            position++;

            SongInfo.Add(new SongData
            {
                song_type = "Minuet",
                song_name = "Valor Minuet IV",
                song_position = position,
                buff_id = 198
            });
            position++;

            SongInfo.Add(new SongData
            {
                song_type = "Minuet",
                song_name = "Valor Minuet V",
                song_position = position,
                buff_id = 198
            });
            position++;

            SongInfo.Add(new SongData
            {
                song_type = "Blank",
                song_name = "Blank",
                song_position = position,
                buff_id = 0
            });
            position++;

            SongInfo.Add(new SongData
            {
                song_type = "Paeon",
                song_name = "Army's Paeon",
                song_position = position,
                buff_id = 195
            });
            position++;

            SongInfo.Add(new SongData
            {
                song_type = "Paeon",
                song_name = "Army's Paeon II",
                song_position = position,
                buff_id = 195
            });
            position++;

            SongInfo.Add(new SongData
            {
                song_type = "Paeon",
                song_name = "Army's Paeon III",
                song_position = position,
                buff_id = 195
            });
            position++;

            SongInfo.Add(new SongData
            {
                song_type = "Paeon",
                song_name = "Army's Paeon IV",
                song_position = position,
                buff_id = 195
            });
            position++;

            SongInfo.Add(new SongData
            {
                song_type = "Paeon",
                song_name = "Army's Paeon V",
                song_position = position,
                buff_id = 195
            });
            position++;

            SongInfo.Add(new SongData
            {
                song_type = "Paeon",
                song_name = "Army's Paeon VI",
                song_position = position,
                buff_id = 195
            });
            position++;

            SongInfo.Add(new SongData
            {
                song_type = "Blank",
                song_name = "Blank",
                song_position = position,
                buff_id = 0
            });
            position++;

            SongInfo.Add(new SongData
            {
                song_type = "Madrigal",
                song_name = "Sword Madrigal",
                song_position = position,
                buff_id = 199
            });
            position++;
            SongInfo.Add(new SongData
            {
                song_type = "Madrigal",
                song_name = "Blade Madrigal",
                song_position = position,
                buff_id = 199
            });
            position++;

            SongInfo.Add(new SongData
            {
                song_type = "Blank",
                song_name = "Blank",
                song_position = position,
                buff_id = 0
            });
            position++;

            SongInfo.Add(new SongData
            {
                song_type = "Prelude",
                song_name = "Hunter's Prelude",
                song_position = position,
                buff_id = 200
            });
            position++;

            SongInfo.Add(new SongData
            {
                song_type = "Prelude",
                song_name = "Archer's Prelude",
                song_position = position,
                buff_id = 200
            });
            position++;

            SongInfo.Add(new SongData
            {
                song_type = "Blank",
                song_name = "Blank",
                song_position = position,
                buff_id = 0
            });
            position++;

            SongInfo.Add(new SongData
            {
                song_type = "Etude",
                song_name = "Sinewy Etude",
                song_position = position,
                buff_id = 215
            });
            position++;

            SongInfo.Add(new SongData
            {
                song_type = "Etude",
                song_name = "Dextrous Etude",
                song_position = position,
                buff_id = 215
            });
            position++;

            SongInfo.Add(new SongData
            {
                song_type = "Etude",
                song_name = "Vivacious Etude",
                song_position = position,
                buff_id = 215
            });
            position++;

            SongInfo.Add(new SongData
            {
                song_type = "Etude",
                song_name = "Quick Etude",
                song_position = position,
                buff_id = 215
            });
            position++;

            SongInfo.Add(new SongData
            {
                song_type = "Etude",
                song_name = "Learned Etude",
                song_position = position,
                buff_id = 215
            });
            position++;

            SongInfo.Add(new SongData
            {
                song_type = "Etude",
                song_name = "Spirited Etude",
                song_position = position,
                buff_id = 215
            });
            position++;

            SongInfo.Add(new SongData
            {
                song_type = "Etude",
                song_name = "Enchanting Etude",
                song_position = position,
                buff_id = 215
            });
            position++;

            SongInfo.Add(new SongData
            {
                song_type = "Etude",
                song_name = "Herculean Etude",
                song_position = position,
                buff_id = 215
            });
            position++;

            SongInfo.Add(new SongData
            {
                song_type = "Etude",
                song_name = "Uncanny Etude",
                song_position = position,
                buff_id = 215
            });
            position++;

            SongInfo.Add(new SongData
            {
                song_type = "Etude",
                song_name = "Vital Etude",
                song_position = position,
                buff_id = 215
            });
            position++;

            SongInfo.Add(new SongData
            {
                song_type = "Etude",
                song_name = "Swift Etude",
                song_position = position,
                buff_id = 215
            });
            position++;

            SongInfo.Add(new SongData
            {
                song_type = "Etude",
                song_name = "Sage Etude",
                song_position = position,
                buff_id = 215
            });
            position++;

            SongInfo.Add(new SongData
            {
                song_type = "Etude",
                song_name = "Logical Etude",
                song_position = position,
                buff_id = 215
            });
            position++;

            SongInfo.Add(new SongData
            {
                song_type = "Etude",
                song_name = "Bewitching Etude",
                song_position = position,
                buff_id = 215
            });
            position++;

            SongInfo.Add(new SongData
            {
                song_type = "Blank",
                song_name = "Blank",
                song_position = position,
                buff_id = 0
            });
            position++;

            SongInfo.Add(new SongData
            {
                song_type = "Mambo",
                song_name = "Sheepfoe Mambo",
                song_position = position,
                buff_id = 201
            });
            position++;

            SongInfo.Add(new SongData
            {
                song_type = "Mambo",
                song_name = "Dragonfoe Mambo",
                song_position = position,
                buff_id = 201
            });
            position++;

            SongInfo.Add(new SongData
            {
                song_type = "Blank",
                song_name = "Blank",
                song_position = position,
                buff_id = 0
            });
            position++;

            SongInfo.Add(new SongData
            {
                song_type = "Ballad",
                song_name = "Mage's Ballad",
                song_position = position,
                buff_id = 196
            });
            position++;

            SongInfo.Add(new SongData
            {
                song_type = "Ballad",
                song_name = "Mage's Ballad II",
                song_position = position,
                buff_id = 196
            });
            position++;

            SongInfo.Add(new SongData
            {
                song_type = "Ballad",
                song_name = "Mage's Ballad III",
                song_position = position,
                buff_id = 196
            });
            position++;

            SongInfo.Add(new SongData
            {
                song_type = "Blank",
                song_name = "Blank",
                song_position = position,
                buff_id = 0
            });
            position++;

            SongInfo.Add(new SongData
            {
                song_type = "March",
                song_name = "Advancing March",
                song_position = position,
                buff_id = 214
            });
            position++;

            SongInfo.Add(new SongData
            {
                song_type = "March",
                song_name = "Victory March",
                song_position = position,
                buff_id = 214
            });
            position++;

            SongInfo.Add(new SongData
            {
                song_type = "March",
                song_name = "Honor March",
                song_position = position,
                buff_id = 214
            });
            position++;

            SongInfo.Add(new SongData
            {
                song_type = "Blank",
                song_name = "Blank",
                song_position = position,
                buff_id = 0
            });
            position++;

            SongInfo.Add(new SongData
            {
                song_type = "Carol",
                song_name = "Fire Carol",
                song_position = position
            });
            position++;

            SongInfo.Add(new SongData
            {
                song_type = "Carol",
                song_name = "Fire Carol II",
                song_position = position,
                buff_id = 216
            });
            position++;

            SongInfo.Add(new SongData
            {
                song_type = "Carol",
                song_name = "Ice Carol",
                song_position = position,
                buff_id = 216
            });
            position++;

            SongInfo.Add(new SongData
            {
                song_type = "Carol",
                song_name = "Ice Carol II",
                song_position = position,
                buff_id = 216
            });
            position++;

            SongInfo.Add(new SongData
            {
                song_type = "Carol",
                song_name = " Wind Carol",
                song_position = position,
                buff_id = 216
            });
            position++;

            SongInfo.Add(new SongData
            {
                song_type = "Carol",
                song_name = "Wind Carol II",
                song_position = position,
                buff_id = 216
            });
            position++;

            SongInfo.Add(new SongData
            {
                song_type = "Carol",
                song_name = "Earth Carol",
                song_position = position,
                buff_id = 216
            });
            position++;

            SongInfo.Add(new SongData
            {
                song_type = "Carol",
                song_name = "Earth Carol II",
                song_position = position,
                buff_id = 216
            });
            position++;

            SongInfo.Add(new SongData
            {
                song_type = "Carol",
                song_name = "Lightning Carol",
                song_position = position,
                buff_id = 216
            });
            position++;

            SongInfo.Add(new SongData
            {
                song_type = "Carol",
                song_name = "Lightning Carol II",
                song_position = position,
                buff_id = 216
            });
            position++;

            SongInfo.Add(new SongData
            {
                song_type = "Carol",
                song_name = "Water Carol",
                song_position = position,
                buff_id = 216
            });
            position++;

            SongInfo.Add(new SongData
            {
                song_type = "Carol",
                song_name = "Water Carol II",
                song_position = position,
                buff_id = 216
            });
            position++;

            SongInfo.Add(new SongData
            {
                song_type = "Carol",
                song_name = "Light Carol",
                song_position = position,
                buff_id = 216
            });
            position++;

            SongInfo.Add(new SongData
            {
                song_type = "Carol",
                song_name = "Light Carol II",
                song_position = position,
                buff_id = 216
            });
            position++;

            SongInfo.Add(new SongData
            {
                song_type = "Carol",
                song_name = "Dark Carol",
                song_position = position,
                buff_id = 216
            });
            position++;

            SongInfo.Add(new SongData
            {
                song_type = "Carol",
                song_name = "Dark Carol II",
                song_position = position,
                buff_id = 216
            });
            position++;

            SongInfo.Add(new SongData
            {
                song_type = "Blank",
                song_name = "Blank",
                song_position = position,
                buff_id = 0
            });
            position++;

            SongInfo.Add(new SongData
            {
                song_type = "Hymnus",
                song_name = "Godess's Hymnus",
                song_position = position,
                buff_id = 218
            });
            position++;

            SongInfo.Add(new SongData
            {
                song_type = "Blank",
                song_name = "Blank",
                song_position = position,
                buff_id = 0
            });
            position++;

            SongInfo.Add(new SongData
            {
                song_type = "Scherzo",
                song_name = "Sentinel's Scherzo",
                song_position = position,
                buff_id = 222
            });
            position++;

            int geo_position = 0;

            GeomancerInfo.Add(new GeoData
            {
                indi_spell = "Indi-Voidance",
                geo_spell = "Geo-Voidance",
                geo_position = geo_position,
            });
            geo_position++;

            GeomancerInfo.Add(new GeoData
            {
                indi_spell = "Indi-Precision",
                geo_spell = "Geo-Precision",
                geo_position = geo_position,
            });
            geo_position++;

            GeomancerInfo.Add(new GeoData
            {
                indi_spell = "Indi-Regen",
                geo_spell = "Geo-Regen",
                geo_position = geo_position,
            });
            geo_position++;

            GeomancerInfo.Add(new GeoData
            {
                indi_spell = "Indi-Haste",
                geo_spell = "Geo-Haste",
                geo_position = geo_position,
            });
            geo_position++;

            GeomancerInfo.Add(new GeoData
            {
                indi_spell = "Indi-Attunement",
                geo_spell = "Geo-Attunement",
                geo_position = geo_position,
            });
            geo_position++;

            GeomancerInfo.Add(new GeoData
            {
                indi_spell = "Indi-Focus",
                geo_spell = "Geo-Focus",
                geo_position = geo_position,
            });
            geo_position++;

            GeomancerInfo.Add(new GeoData
            {
                indi_spell = "Indi-Barrier",
                geo_spell = "Geo-Barrier",
                geo_position = geo_position,
            });
            geo_position++;

            GeomancerInfo.Add(new GeoData
            {
                indi_spell = "Indi-Refresh",
                geo_spell = "Geo-Refresh",
                geo_position = geo_position,
            });
            geo_position++;

            GeomancerInfo.Add(new GeoData
            {
                indi_spell = "Indi-CHR",
                geo_spell = "Geo-CHR",
                geo_position = geo_position,
            });
            geo_position++;

            GeomancerInfo.Add(new GeoData
            {
                indi_spell = "Indi-MND",
                geo_spell = "Geo-MND",
                geo_position = geo_position,
            });
            geo_position++;

            GeomancerInfo.Add(new GeoData
            {
                indi_spell = "Indi-Fury",
                geo_spell = "Geo-Fury",
                geo_position = geo_position,
            });
            geo_position++;

            GeomancerInfo.Add(new GeoData
            {
                indi_spell = "Indi-INT",
                geo_spell = "Geo-INT",
                geo_position = geo_position,
            });
            geo_position++;

            GeomancerInfo.Add(new GeoData
            {
                indi_spell = "Indi-AGI",
                geo_spell = "Geo-AGI",
                geo_position = geo_position,
            });
            geo_position++;

            GeomancerInfo.Add(new GeoData
            {
                indi_spell = "Indi-Fend",
                geo_spell = "Geo-Fend",
                geo_position = geo_position,
            });
            geo_position++;

            GeomancerInfo.Add(new GeoData
            {
                indi_spell = "Indi-VIT",
                geo_spell = "Geo-VIT",
                geo_position = geo_position,
            });
            geo_position++;

            GeomancerInfo.Add(new GeoData
            {
                indi_spell = "Indi-DEX",
                geo_spell = "Geo-DEX",
                geo_position = geo_position,
            });
            geo_position++;

            GeomancerInfo.Add(new GeoData
            {
                indi_spell = "Indi-Acumen",
                geo_spell = "Geo-Acumen",
                geo_position = geo_position,
            });
            geo_position++;

            GeomancerInfo.Add(new GeoData
            {
                indi_spell = "Indi-STR",
                geo_spell = "Geo-STR",
                geo_position = geo_position,
            });
            geo_position++;

            GeomancerInfo.Add(new GeoData
            {
                indi_spell = "Indi-Poison",
                geo_spell = "Geo-Poison",
                geo_position = geo_position,
            });
            geo_position++;

            GeomancerInfo.Add(new GeoData
            {
                indi_spell = "Indi-Slow",
                geo_spell = "Geo-Slow",
                geo_position = geo_position,
            });
            geo_position++;

            GeomancerInfo.Add(new GeoData
            {
                indi_spell = "Indi-Torpor",
                geo_spell = "Geo-Torpor",
                geo_position = geo_position,
            });
            geo_position++;

            GeomancerInfo.Add(new GeoData
            {
                indi_spell = "Indi-Slip",
                geo_spell = "Geo-Slip",
                geo_position = geo_position,
            });
            geo_position++;

            GeomancerInfo.Add(new GeoData
            {
                indi_spell = "Indi-Languor",
                geo_spell = "Geo-Languor",
                geo_position = geo_position,
            });
            geo_position++;

            GeomancerInfo.Add(new GeoData
            {
                indi_spell = "Indi-Paralysis",
                geo_spell = "Geo-Paralysis",
                geo_position = geo_position,
            });
            geo_position++;

            GeomancerInfo.Add(new GeoData
            {
                indi_spell = "Indi-Vex",
                geo_spell = "Geo-Vex",
                geo_position = geo_position,
            });
            geo_position++;

            GeomancerInfo.Add(new GeoData
            {
                indi_spell = "Indi-Frailty",
                geo_spell = "Geo-Frailty",
                geo_position = geo_position,
            });
            geo_position++;

            GeomancerInfo.Add(new GeoData
            {
                indi_spell = "Indi-Wilt",
                geo_spell = "Geo-Wilt",
                geo_position = geo_position,
            });
            geo_position++;

            GeomancerInfo.Add(new GeoData
            {
                indi_spell = "Indi-Malaise",
                geo_spell = "Geo-Malaise",
                geo_position = geo_position,
            });
            geo_position++;

            GeomancerInfo.Add(new GeoData
            {
                indi_spell = "Indi-Gravity",
                geo_spell = "Geo-Gravity",
                geo_position = geo_position,
            });
            geo_position++;

            GeomancerInfo.Add(new GeoData
            {
                indi_spell = "Indi-Fade",
                geo_spell = "Geo-Fade",
                geo_position = geo_position,
            });
            geo_position++;

            barspells.Add(new SpellsData
            {
                Spell_Name = "Barfire",
                type = 1,
                spell_position = 0,
                buffID = 100,
            });
            barspells.Add(new SpellsData
            {
                Spell_Name = "Barfira",
                type = 1,
                spell_position = 0,
                buffID = 100,
                aoe_version = true,
            });
            barspells.Add(new SpellsData
            {
                Spell_Name = "Barstone",
                type = 1,
                spell_position = 1,
                buffID = 103,
            });
            barspells.Add(new SpellsData
            {
                Spell_Name = "Barstonra",
                type = 1,
                spell_position = 1,
                buffID = 103,
                aoe_version = true,
            });
            barspells.Add(new SpellsData
            {
                Spell_Name = "Barwater",
                type = 1,
                spell_position = 2,
                buffID = 105,
            });
            barspells.Add(new SpellsData
            {
                Spell_Name = "Barwatera",
                type = 1,
                spell_position = 2,
                buffID = 105,
                aoe_version = true,
            });
            barspells.Add(new SpellsData
            {
                Spell_Name = "Baraero",
                type = 1,
                spell_position = 3,
                buffID = 102
            });
            barspells.Add(new SpellsData
            {
                Spell_Name = "Baraera",
                type = 1,
                spell_position = 3,
                buffID = 102,
                aoe_version = true,
            });
            barspells.Add(new SpellsData
            {
                Spell_Name = "Barblizzard",
                type = 1,
                spell_position = 4,
                buffID = 101
            });
            barspells.Add(new SpellsData
            {
                Spell_Name = "Barblizzara",
                type = 1,
                spell_position = 4,
                buffID = 101,
                aoe_version = true,
            });
            barspells.Add(new SpellsData
            {
                Spell_Name = "Barthunder",
                type = 1,
                spell_position = 5,
                buffID = 104
            });
            barspells.Add(new SpellsData
            {
                Spell_Name = "Barthundra",
                type = 1,
                spell_position = 5,
                buffID = 104,
                aoe_version = true,
            });

            barspells.Add(new SpellsData
            {
                Spell_Name = "Baramnesia",
                type = 2,
                spell_position = 0,
                buffID = 286,
            });
            barspells.Add(new SpellsData
            {
                Spell_Name = "Baramnesra",
                type = 2,
                spell_position = 0,
                buffID = 286,
                aoe_version = true,
            });
            barspells.Add(new SpellsData
            {
                Spell_Name = "Barvirus",
                type = 2,
                spell_position = 1,
                buffID = 112
            });
            barspells.Add(new SpellsData
            {
                Spell_Name = "Barvira",
                type = 2,
                spell_position = 1,
                buffID = 112,
                aoe_version = true,
            });
            barspells.Add(new SpellsData
            {
                Spell_Name = "Barparalyze",
                type = 2,
                spell_position = 2,
                buffID = 108
            });
            barspells.Add(new SpellsData
            {
                Spell_Name = "Barparalyzra",
                type = 2,
                spell_position = 2,
                buffID = 108,
                aoe_version = true,
            });
            barspells.Add(new SpellsData
            {
                Spell_Name = "Barsilence",
                type = 2,
                spell_position = 3,
                buffID = 110
            });
            barspells.Add(new SpellsData
            {
                Spell_Name = "Barsilencera",
                type = 2,
                spell_position = 3,
                buffID = 110,
                aoe_version = true,
            });
            barspells.Add(new SpellsData
            {
                Spell_Name = "Barpetrify",
                type = 2,
                spell_position = 4,
                buffID = 111
            });
            barspells.Add(new SpellsData
            {
                Spell_Name = "Barpetra",
                type = 2,
                spell_position = 4,
                buffID = 111,
                aoe_version = true,
            });
            barspells.Add(new SpellsData
            {
                Spell_Name = "Barpoison",
                type = 2,
                spell_position = 5,
                buffID = 107
            });
            barspells.Add(new SpellsData
            {
                Spell_Name = "Barpoisonra",
                type = 2,
                spell_position = 5,
                buffID = 107,
                aoe_version = true,
            });
            barspells.Add(new SpellsData
            {
                Spell_Name = "Barblind",
                type = 2,
                spell_position = 6,
                buffID = 109
            });
            barspells.Add(new SpellsData
            {
                Spell_Name = "Barblindra",
                type = 2,
                spell_position = 6,
                buffID = 109,
                aoe_version = true,
            });
            barspells.Add(new SpellsData
            {
                Spell_Name = "Barsleep",
                type = 2,
                spell_position = 7,
                buffID = 106
            });
            barspells.Add(new SpellsData
            {
                Spell_Name = "Barsleepra",
                type = 2,
                spell_position = 7,
                buffID = 106,
                aoe_version = true,
            });

            enspells.Add(new SpellsData
            {
                Spell_Name = "Enfire",
                type = 1,
                spell_position = 0,
                buffID = 94
            });
            enspells.Add(new SpellsData
            {
                Spell_Name = "Enstone",
                type = 1,
                spell_position = 1,
                buffID = 97
            });
            enspells.Add(new SpellsData
            {
                Spell_Name = "Enwater",
                type = 1,
                spell_position = 2,
                buffID = 99
            });
            enspells.Add(new SpellsData
            {
                Spell_Name = "Enaero",
                type = 1,
                spell_position = 3,
                buffID = 96
            });
            enspells.Add(new SpellsData
            {
                Spell_Name = "Enblizzard",
                type = 1,
                spell_position = 4,
                buffID = 95
            });
            enspells.Add(new SpellsData
            {
                Spell_Name = "Enthunder",
                type = 1,
                spell_position = 5,
                buffID = 98
            });

            enspells.Add(new SpellsData
            {
                Spell_Name = "Enfire II",
                type = 1,
                spell_position = 6,
                buffID = 277
            });
            enspells.Add(new SpellsData
            {
                Spell_Name = "Enstone II",
                type = 1,
                spell_position = 7,
                buffID = 280
            });
            enspells.Add(new SpellsData
            {
                Spell_Name = "Enwater II",
                type = 1,
                spell_position = 8,
                buffID = 282
            });
            enspells.Add(new SpellsData
            {
                Spell_Name = "Enaero II",
                type = 1,
                spell_position = 9,
                buffID = 279
            });
            enspells.Add(new SpellsData
            {
                Spell_Name = "Enblizzard II",
                type = 1,
                spell_position = 10,
                buffID = 278
            });
            enspells.Add(new SpellsData
            {
                Spell_Name = "Enthunder II",
                type = 1,
                spell_position = 11,
                buffID = 281
            });           

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
            Form2.config.autoFollowName = string.Empty;

            ForceSongRecast = true;

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
                            Form2.MySettings config = new Form2.MySettings();

                            XmlSerializer mySerializer = new XmlSerializer(typeof(Form2.MySettings));

                            StreamReader reader = new StreamReader(filename);
                            config = (Form2.MySettings)mySerializer.Deserialize(reader);

                            reader.Close();
                            reader.Dispose();
                            Form2.updateForm(config);
                            Form2.button4_Click(sender, e);
                        }
                        else if (System.IO.File.Exists(filename2))
                        {
                            Form2.MySettings config = new Form2.MySettings();

                            XmlSerializer mySerializer = new XmlSerializer(typeof(Form2.MySettings));

                            StreamReader reader = new StreamReader(filename2);
                            config = (Form2.MySettings)mySerializer.Deserialize(reader);

                            reader.Close();
                            reader.Dispose();
                            Form2.updateForm(config);
                            Form2.button4_Click(sender, e);
                        }
                    }
                }
            }

            if (LUA_Plugin_Loaded == 0 && !Form2.config.pauseOnStartBox && Monitored != null)
            {
                // Wait a milisecond and then load and set the config.
                Thread.Sleep(500);

                if (WindowerMode == "Windower")
                {
                    PL.ThirdParty.SendString("//lua load CurePlease_addon");
                    Thread.Sleep(1500);
                    PL.ThirdParty.SendString("//cpaddon settings " + Form2.config.ipAddress + " " + Form2.config.listeningPort);
                    Thread.Sleep(100);
                    PL.ThirdParty.SendString("//cpaddon verify");
                    if (Form2.config.enableHotKeys)
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
                    PL.ThirdParty.SendString("/cpaddon settings " + Form2.config.ipAddress + " " + Form2.config.listeningPort);
                    Thread.Sleep(100);

                    PL.ThirdParty.SendString("/cpaddon verify");
                    if (Form2.config.enableHotKeys)
                    {
                        PL.ThirdParty.SendString("/bind ^!F1 /cureplease toggle");
                        PL.ThirdParty.SendString("/bind ^!F2 /cureplease start");
                        PL.ThirdParty.SendString("/bind ^!F3 /cureplease pause");
                    }
                }

                AddOnStatus_Click(sender, e);


                currentAction.Text = "LUA Addon loaded. ( " + Form2.config.ipAddress + " - " + Form2.config.listeningPort + " )";

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

            if (Form2.config.pauseOnStartBox)
            {
                pauseActions = true;
                pauseButton.Text = "Loaded, Paused!";
                pauseButton.ForeColor = Color.Red;
                actionTimer.Enabled = false;
            }
            else
            {
                if (Form2.config.MinimiseonStart == true && WindowState != FormWindowState.Minimized)
                {
                    WindowState = FormWindowState.Minimized;
                }
            }

            if (LUA_Plugin_Loaded == 0 && !Form2.config.pauseOnStartBox && PL != null)
            {
                // Wait a milisecond and then load and set the config.
                Thread.Sleep(500);
                if (WindowerMode == "Windower")
                {
                    PL.ThirdParty.SendString("//lua load CurePlease_addon");
                    Thread.Sleep(1500);
                    PL.ThirdParty.SendString("//cpaddon settings " + Form2.config.ipAddress + " " + Form2.config.listeningPort);
                    Thread.Sleep(100);
                    PL.ThirdParty.SendString("//cpaddon verify");

                    if (Form2.config.enableHotKeys)
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
                    PL.ThirdParty.SendString("/cpaddon settings " + Form2.config.ipAddress + " " + Form2.config.listeningPort);
                    Thread.Sleep(100);
                    PL.ThirdParty.SendString("/cpaddon verify");
                    if (Form2.config.enableHotKeys)
                    {
                        PL.ThirdParty.SendString("/bind ^!F1 /cureplease toggle");
                        PL.ThirdParty.SendString("/bind ^!F2 /cureplease start");
                        PL.ThirdParty.SendString("/bind ^!F3 /cureplease pause");
                    }
                }

                currentAction.Text = "LUA Addon loaded. ( " + Form2.config.ipAddress + " - " + Form2.config.listeningPort + " )";

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
            else if(Form2.config.Overcure && PL.SpellAvailable(overSpell) && PL.HasMPFor(overSpell))
            {
                return overSpell;
            }
            else if(Form2.config.Undercure && PL.SpellAvailable(underSpell) && PL.HasMPFor(underSpell))
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
                if (Form2.config.pauseOnZoneBox == true)
                {
                    song_casting = 0;
                    ForceSongRecast = true;
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
                    song_casting = 0;
                    ForceSongRecast = true;

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

        private void removeDebuff(string characterName, int debuffID)
        {
            lock (ActiveBuffs)
            {
                if(ActiveBuffs.ContainsKey(characterName))
                {
                    // Filter out the specific debuff ID
                    var buffsFiltered = ActiveBuffs[characterName].Where(ab => ab != debuffID);                

                    ActiveBuffs[characterName] = buffsFiltered;
                }               
            }
        }

        private string PickCure(uint hpLoss)
        {
            if (Form2.config.cure6enabled && hpLoss >= Form2.config.cure6amount && PL.HasMPFor(Spells.Cure_VI))
            {
                return PickCureTier(Spells.Cure_VI, Data.CureTiers);
            }
            else if (Form2.config.cure5enabled && hpLoss >= Form2.config.cure5amount && PL.HasMPFor(Spells.Cure_V))
            {
                return PickCureTier(Spells.Cure_V, Data.CureTiers);
            }
            else if (Form2.config.cure4enabled && hpLoss >= Form2.config.cure4amount && PL.HasMPFor(Spells.Cure_IV))
            {
                return PickCureTier(Spells.Cure_IV, Data.CureTiers);
            }
            else if (Form2.config.cure3enabled && hpLoss >= Form2.config.cure3amount && PL.HasMPFor(Spells.Cure_III))
            {
                return PickCureTier(Spells.Cure_III, Data.CureTiers);
            }
            else if (Form2.config.cure2enabled && hpLoss >= Form2.config.cure2amount && PL.HasMPFor(Spells.Cure_II))
            {
                return PickCureTier(Spells.Cure_II, Data.CureTiers);
            }
            else if (Form2.config.cure1enabled && hpLoss >= Form2.config.cure1amount && PL.HasMPFor(Spells.Cure))
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
                    if (Array.IndexOf(Data.CureTiers, cureSpell) < 2 && Form2.config.PrioritiseOverLowerTier == true)
                    {
                        RunDebuffChecker();
                    }

                    CastSpell(partyMember.Name, cureSpell);
                }
            }
        }

        // TODO: Add debuff priorities.
        // TODO: Add custom DOOM logic ahead of cures.
        private void RunDebuffChecker()
        {
            // PL and Monitored Player Debuff Removal Starting with PL
            if (PL.Player.Status != 33)
            {
                if (Form2.config.plSilenceItem == 0)
                {
                    plSilenceitemName = "Catholicon";
                }
                else if (Form2.config.plSilenceItem == 1)
                {
                    plSilenceitemName = "Echo Drops";
                }
                else if (Form2.config.plSilenceItem == 2)
                {
                    plSilenceitemName = "Remedy";
                }
                else if (Form2.config.plSilenceItem == 3)
                {
                    plSilenceitemName = "Remedy Ointment";
                }
                else if (Form2.config.plSilenceItem == 4)
                {
                    plSilenceitemName = "Vicar's Drink";
                }

                if (Form2.config.plDoomitem == 0)
                {
                    plDoomItemName = "Holy Water";
                }
                else if (Form2.config.plDoomitem == 1)
                {
                    plDoomItemName = "Hallowed Water";
                }

                if (Form2.config.wakeSleepSpell == 0)
                {
                    wakeSleepSpell = Spells.Cure;
                }
                else if (Form2.config.wakeSleepSpell == 1)
                {
                    wakeSleepSpell = Spells.Cura;
                }
                else if (Form2.config.wakeSleepSpell == 2)
                {
                    wakeSleepSpell = Spells.Curaga;
                }

                foreach (StatusEffect plEffect in PL.Player.Buffs)
                {
                    if ((plEffect == StatusEffect.Doom) && Form2.config.plDoom && PL.SpellAvailable(Spells.Cursna))
                    {
                        CastSpell(PL.Player.Name, Spells.Cursna);
                    }
                    else if ((plEffect == StatusEffect.Paralysis) && Form2.config.plParalysis && PL.SpellAvailable(Spells.Paralyna))
                    {
                        CastSpell(PL.Player.Name, Spells.Paralyna);
                    }
                    else if ((plEffect == StatusEffect.Amnesia) && Form2.config.plAmnesia && PL.SpellAvailable(Spells.Esuna))
                    {
                        CastSpell(PL.Player.Name, Spells.Esuna);
                    }
                    else if ((plEffect == StatusEffect.Poison) && Form2.config.plPoison && PL.SpellAvailable(Spells.Poisona))
                    {
                        CastSpell(PL.Player.Name, Spells.Poisona);
                    }
                    else if ((plEffect == StatusEffect.Attack_Down) && Form2.config.plAttackDown && PL.SpellAvailable(Spells.Erase))
                    {
                        CastSpell(PL.Player.Name, Spells.Erase);
                    }
                    else if ((plEffect == StatusEffect.Blindness) && Form2.config.plBlindness && PL.SpellAvailable(Spells.Blindna))
                    {
                        CastSpell(PL.Player.Name, Spells.Blindna);
                    }
                    else if ((plEffect == StatusEffect.Bind) && Form2.config.plBind && Form2.config.plAttackDown && PL.SpellAvailable(Spells.Erase))
                    {
                        CastSpell(PL.Player.Name, Spells.Erase);
                    }
                    else if ((plEffect == StatusEffect.Weight) && Form2.config.plWeight && PL.SpellAvailable(Spells.Erase))
                    {
                        CastSpell(PL.Player.Name, Spells.Erase);
                    }
                    else if ((plEffect == StatusEffect.Slow) && Form2.config.plSlow && PL.SpellAvailable(Spells.Erase))
                    {
                        CastSpell(PL.Player.Name, Spells.Erase);
                    }
                    else if ((plEffect == StatusEffect.Curse) && Form2.config.plCurse && PL.SpellAvailable(Spells.Cursna))
                    {
                        CastSpell(PL.Player.Name, Spells.Cursna);
                    }
                    else if ((plEffect == StatusEffect.Curse2) && Form2.config.plCurse2 && PL.SpellAvailable(Spells.Cursna))
                    {
                        CastSpell(PL.Player.Name, Spells.Cursna);
                    }
                    else if ((plEffect == StatusEffect.Addle) && Form2.config.plAddle && Form2.config.plAttackDown && PL.SpellAvailable(Spells.Erase))
                    {
                        CastSpell(PL.Player.Name, Spells.Erase);
                    }
                    else if ((plEffect == StatusEffect.Bane) && Form2.config.plBane && PL.SpellAvailable(Spells.Cursna))
                    {
                        CastSpell(PL.Player.Name, Spells.Cursna);
                    }
                    else if ((plEffect == StatusEffect.Plague) && Form2.config.plPlague && PL.SpellAvailable(Spells.Viruna))
                    {
                        CastSpell(PL.Player.Name, Spells.Viruna);
                    }
                    else if ((plEffect == StatusEffect.Disease) && Form2.config.plDisease && PL.SpellAvailable(Spells.Viruna))
                    {
                        CastSpell(PL.Player.Name, Spells.Viruna);
                    }
                    else if ((plEffect == StatusEffect.Burn) && Form2.config.plBurn && Form2.config.plAttackDown && PL.SpellAvailable(Spells.Erase))
                    {
                        CastSpell(PL.Player.Name, Spells.Erase);
                    }
                    else if ((plEffect == StatusEffect.Frost) && Form2.config.plFrost && Form2.config.plAttackDown && PL.SpellAvailable(Spells.Erase))
                    {
                        CastSpell(PL.Player.Name, Spells.Erase);
                    }
                    else if ((plEffect == StatusEffect.Choke) && Form2.config.plChoke && Form2.config.plAttackDown && PL.SpellAvailable(Spells.Erase))
                    {
                        CastSpell(PL.Player.Name, Spells.Erase);
                    }
                    else if ((plEffect == StatusEffect.Rasp) && Form2.config.plRasp && Form2.config.plAttackDown && PL.SpellAvailable(Spells.Erase))
                    {
                        CastSpell(PL.Player.Name, Spells.Erase);
                    }
                    else if ((plEffect == StatusEffect.Shock) && Form2.config.plShock && Form2.config.plAttackDown && PL.SpellAvailable(Spells.Erase))
                    {
                        CastSpell(PL.Player.Name, Spells.Erase);
                    }
                    else if ((plEffect == StatusEffect.Drown) && Form2.config.plDrown && Form2.config.plAttackDown && PL.SpellAvailable(Spells.Erase))
                    {
                        CastSpell(PL.Player.Name, Spells.Erase);
                    }
                    else if ((plEffect == StatusEffect.Dia) && Form2.config.plDia && Form2.config.plAttackDown && PL.SpellAvailable(Spells.Erase))
                    {
                        CastSpell(PL.Player.Name, Spells.Erase);
                    }
                    else if ((plEffect == StatusEffect.Bio) && Form2.config.plBio && Form2.config.plAttackDown && PL.SpellAvailable(Spells.Erase))
                    {
                        CastSpell(PL.Player.Name, Spells.Erase);
                    }
                    else if ((plEffect == StatusEffect.STR_Down) && Form2.config.plStrDown && Form2.config.plAttackDown && PL.SpellAvailable(Spells.Erase))
                    {
                        CastSpell(PL.Player.Name, Spells.Erase);
                    }
                    else if ((plEffect == StatusEffect.DEX_Down) && Form2.config.plDexDown && Form2.config.plAttackDown && PL.SpellAvailable(Spells.Erase))
                    {
                        CastSpell(PL.Player.Name, Spells.Erase);
                    }
                    else if ((plEffect == StatusEffect.VIT_Down) && Form2.config.plVitDown && Form2.config.plAttackDown && PL.SpellAvailable(Spells.Erase))
                    {
                        CastSpell(PL.Player.Name, Spells.Erase);
                    }
                    else if ((plEffect == StatusEffect.AGI_Down) && Form2.config.plAgiDown && Form2.config.plAttackDown && PL.SpellAvailable(Spells.Erase))
                    {
                        CastSpell(PL.Player.Name, Spells.Erase);
                    }
                    else if ((plEffect == StatusEffect.INT_Down) && Form2.config.plIntDown && Form2.config.plAttackDown && PL.SpellAvailable(Spells.Erase))
                    {
                        CastSpell(PL.Player.Name, Spells.Erase);
                    }
                    else if ((plEffect == StatusEffect.MND_Down) && Form2.config.plMndDown && Form2.config.plAttackDown && PL.SpellAvailable(Spells.Erase))
                    {
                        CastSpell(PL.Player.Name, Spells.Erase);
                    }
                    else if ((plEffect == StatusEffect.CHR_Down) && Form2.config.plChrDown && Form2.config.plAttackDown && PL.SpellAvailable(Spells.Erase))
                    {
                        CastSpell(PL.Player.Name, Spells.Erase);
                    }
                    else if ((plEffect == StatusEffect.Max_HP_Down) && Form2.config.plMaxHpDown && Form2.config.plAttackDown && PL.SpellAvailable(Spells.Erase))
                    {
                        CastSpell(PL.Player.Name, Spells.Erase);
                    }
                    else if ((plEffect == StatusEffect.Max_MP_Down) && Form2.config.plMaxMpDown && Form2.config.plAttackDown && PL.SpellAvailable(Spells.Erase))
                    {
                        CastSpell(PL.Player.Name, Spells.Erase);
                    }
                    else if ((plEffect == StatusEffect.Accuracy_Down) && Form2.config.plAccuracyDown && Form2.config.plAttackDown && PL.SpellAvailable(Spells.Erase))
                    {
                        CastSpell(PL.Player.Name, Spells.Erase);
                    }
                    else if ((plEffect == StatusEffect.Evasion_Down) && Form2.config.plEvasionDown && Form2.config.plAttackDown && PL.SpellAvailable(Spells.Erase))
                    {
                        CastSpell(PL.Player.Name, Spells.Erase);
                    }
                    else if ((plEffect == StatusEffect.Defense_Down) && Form2.config.plDefenseDown && Form2.config.plAttackDown && PL.SpellAvailable(Spells.Erase))
                    {
                        CastSpell(PL.Player.Name, Spells.Erase);
                    }
                    else if ((plEffect == StatusEffect.Flash) && Form2.config.plFlash && Form2.config.plAttackDown && PL.SpellAvailable(Spells.Erase))
                    {
                        CastSpell(PL.Player.Name, Spells.Erase);
                    }
                    else if ((plEffect == StatusEffect.Magic_Acc_Down) && Form2.config.plMagicAccDown && Form2.config.plAttackDown && PL.SpellAvailable(Spells.Erase))
                    {
                        CastSpell(PL.Player.Name, Spells.Erase);
                    }
                    else if ((plEffect == StatusEffect.Magic_Atk_Down) && Form2.config.plMagicAtkDown && Form2.config.plAttackDown && PL.SpellAvailable(Spells.Erase))
                    {
                        CastSpell(PL.Player.Name, Spells.Erase);
                    }
                    else if ((plEffect == StatusEffect.Helix) && Form2.config.plHelix && Form2.config.plAttackDown && PL.SpellAvailable(Spells.Erase))
                    {
                        CastSpell(PL.Player.Name, Spells.Erase);
                    }
                    else if ((plEffect == StatusEffect.Max_TP_Down) && Form2.config.plMaxTpDown && Form2.config.plAttackDown && PL.SpellAvailable(Spells.Erase))
                    {
                        CastSpell(PL.Player.Name, Spells.Erase);
                    }
                    else if ((plEffect == StatusEffect.Requiem) && Form2.config.plRequiem && Form2.config.plAttackDown && PL.SpellAvailable(Spells.Erase))
                    {
                        CastSpell(PL.Player.Name, Spells.Erase);
                    }
                    else if ((plEffect == StatusEffect.Elegy) && Form2.config.plElegy && Form2.config.plAttackDown && PL.SpellAvailable(Spells.Erase))
                    {
                        CastSpell(PL.Player.Name, Spells.Erase);
                    }
                    else if ((plEffect == StatusEffect.Threnody) && Form2.config.plThrenody && Form2.config.plAttackDown && PL.SpellAvailable(Spells.Erase))
                    {
                        CastSpell(PL.Player.Name, Spells.Erase);
                    }
                }
            }

            // Next, we check monitored player
            if ((PL.Entity.GetEntity((int)Monitored.Party.GetPartyMember(0).TargetIndex).Distance < 21) && (PL.Entity.GetEntity((int)Monitored.Party.GetPartyMember(0).TargetIndex).Distance > 0) && (Monitored.Player.HP > 0) && PL.Player.Status != 33)
            {
                foreach (StatusEffect monitoredEffect in Monitored.Player.Buffs)
                {
                    if ((monitoredEffect == StatusEffect.Doom) && Form2.config.monitoredDoom && PL.SpellAvailable(Spells.Cursna))
                    {
                        CastSpell(Monitored.Player.Name, Spells.Cursna);
                    }
                    else if ((monitoredEffect == StatusEffect.Sleep) && Form2.config.monitoredSleep && Form2.config.wakeSleepEnabled && PL.SpellAvailable(wakeSleepSpell))
                    {
                        CastSpell(Monitored.Player.Name, wakeSleepSpell);
                    }
                    else if ((monitoredEffect == StatusEffect.Sleep2) && Form2.config.monitoredSleep2 && Form2.config.wakeSleepEnabled && PL.SpellAvailable(wakeSleepSpell))
                    {
                        CastSpell(Monitored.Player.Name, wakeSleepSpell);
                    }
                    else if ((monitoredEffect == StatusEffect.Silence) && Form2.config.monitoredSilence && PL.SpellAvailable(Spells.Silena))
                    {
                        CastSpell(Monitored.Player.Name, Spells.Silena);
                    }
                    else if ((monitoredEffect == StatusEffect.Petrification) && Form2.config.monitoredPetrification && PL.SpellAvailable(Spells.Stona))
                    {
                        CastSpell(Monitored.Player.Name, Spells.Stona);
                    }
                    else if ((monitoredEffect == StatusEffect.Paralysis) && Form2.config.monitoredParalysis && PL.SpellAvailable(Spells.Paralyna))
                    {
                        CastSpell(Monitored.Player.Name, Spells.Paralyna);
                    }
                    else if ((monitoredEffect == StatusEffect.Amnesia) && Form2.config.monitoredAmnesia && PL.SpellAvailable(Spells.Esuna))
                    {
                        CastSpell(Monitored.Player.Name, Spells.Esuna);
                    }
                    else if ((monitoredEffect == StatusEffect.Poison) && Form2.config.monitoredPoison && PL.SpellAvailable(Spells.Poisona))
                    {
                        CastSpell(Monitored.Player.Name, Spells.Poisona);
                    }
                    else if ((monitoredEffect == StatusEffect.Attack_Down) && Form2.config.monitoredAttackDown && PL.SpellAvailable(Spells.Erase) && plMonitoredSameParty())
                    {
                        CastSpell(Monitored.Player.Name, Spells.Erase);
                    }
                    else if ((monitoredEffect == StatusEffect.Blindness) && Form2.config.monitoredBlindness && PL.SpellAvailable(Spells.Blindna))
                    {
                        CastSpell(Monitored.Player.Name, Spells.Blindna);
                    }
                    else if ((monitoredEffect == StatusEffect.Bind) && Form2.config.monitoredBind && PL.SpellAvailable(Spells.Erase) && plMonitoredSameParty())
                    {
                        CastSpell(Monitored.Player.Name, Spells.Erase);
                    }
                    else if ((monitoredEffect == StatusEffect.Weight) && Form2.config.monitoredWeight && PL.SpellAvailable(Spells.Erase) && plMonitoredSameParty())
                    {
                        CastSpell(Monitored.Player.Name, Spells.Erase);
                    }
                    else if ((monitoredEffect == StatusEffect.Slow) && Form2.config.monitoredSlow && PL.SpellAvailable(Spells.Erase) && plMonitoredSameParty())
                    {
                        CastSpell(Monitored.Player.Name, Spells.Erase);
                    }
                    else if ((monitoredEffect == StatusEffect.Curse) && Form2.config.monitoredCurse && PL.SpellAvailable(Spells.Cursna))
                    {
                        CastSpell(Monitored.Player.Name, Spells.Cursna);
                    }
                    else if ((monitoredEffect == StatusEffect.Curse2) && Form2.config.monitoredCurse2 && PL.SpellAvailable(Spells.Cursna))
                    {
                        CastSpell(Monitored.Player.Name, Spells.Cursna);
                    }
                    else if ((monitoredEffect == StatusEffect.Addle) && Form2.config.monitoredAddle && PL.SpellAvailable(Spells.Erase) && plMonitoredSameParty())
                    {
                        CastSpell(Monitored.Player.Name, Spells.Erase);
                    }
                    else if ((monitoredEffect == StatusEffect.Bane) && Form2.config.monitoredBane && PL.SpellAvailable(Spells.Cursna))
                    {
                        CastSpell(Monitored.Player.Name, Spells.Cursna);
                    }
                    else if ((monitoredEffect == StatusEffect.Plague) && Form2.config.monitoredPlague && PL.SpellAvailable(Spells.Viruna))
                    {
                        CastSpell(Monitored.Player.Name, Spells.Viruna);
                    }
                    else if ((monitoredEffect == StatusEffect.Disease) && Form2.config.monitoredDisease && PL.SpellAvailable(Spells.Viruna))
                    {
                        CastSpell(Monitored.Player.Name, Spells.Viruna);
                    }
                    else if ((monitoredEffect == StatusEffect.Burn) && Form2.config.monitoredBurn && PL.SpellAvailable(Spells.Erase) && plMonitoredSameParty())
                    {
                        CastSpell(Monitored.Player.Name, Spells.Erase);
                    }
                    else if ((monitoredEffect == StatusEffect.Frost) && Form2.config.monitoredFrost && PL.SpellAvailable(Spells.Erase) && plMonitoredSameParty())
                    {
                        CastSpell(Monitored.Player.Name, Spells.Erase);
                    }
                    else if ((monitoredEffect == StatusEffect.Choke) && Form2.config.monitoredChoke && PL.SpellAvailable(Spells.Erase) && plMonitoredSameParty())
                    {
                        CastSpell(Monitored.Player.Name, Spells.Erase);
                    }
                    else if ((monitoredEffect == StatusEffect.Rasp) && Form2.config.monitoredRasp && PL.SpellAvailable(Spells.Erase) && plMonitoredSameParty())
                    {
                        CastSpell(Monitored.Player.Name, Spells.Erase);
                    }
                    else if ((monitoredEffect == StatusEffect.Shock) && Form2.config.monitoredShock && PL.SpellAvailable(Spells.Erase) && plMonitoredSameParty())
                    {
                        CastSpell(Monitored.Player.Name, Spells.Erase);
                    }
                    else if ((monitoredEffect == StatusEffect.Drown) && Form2.config.monitoredDrown && PL.SpellAvailable(Spells.Erase) && plMonitoredSameParty())
                    {
                        CastSpell(Monitored.Player.Name, Spells.Erase);
                    }
                    else if ((monitoredEffect == StatusEffect.Dia) && Form2.config.monitoredDia && PL.SpellAvailable(Spells.Erase) && plMonitoredSameParty())
                    {
                        CastSpell(Monitored.Player.Name, Spells.Erase);
                    }
                    else if ((monitoredEffect == StatusEffect.Bio) && Form2.config.monitoredBio && PL.SpellAvailable(Spells.Erase) && plMonitoredSameParty())
                    {
                        CastSpell(Monitored.Player.Name, Spells.Erase);
                    }
                    else if ((monitoredEffect == StatusEffect.STR_Down) && Form2.config.monitoredStrDown && PL.SpellAvailable(Spells.Erase) && plMonitoredSameParty())
                    {
                        CastSpell(Monitored.Player.Name, Spells.Erase);
                    }
                    else if ((monitoredEffect == StatusEffect.DEX_Down) && Form2.config.monitoredDexDown && PL.SpellAvailable(Spells.Erase) && plMonitoredSameParty())
                    {
                        CastSpell(Monitored.Player.Name, Spells.Erase);
                    }
                    else if ((monitoredEffect == StatusEffect.VIT_Down) && Form2.config.monitoredVitDown && PL.SpellAvailable(Spells.Erase) && plMonitoredSameParty())
                    {
                        CastSpell(Monitored.Player.Name, Spells.Erase);
                    }
                    else if ((monitoredEffect == StatusEffect.AGI_Down) && Form2.config.monitoredAgiDown && PL.SpellAvailable(Spells.Erase) && plMonitoredSameParty())
                    {
                        CastSpell(Monitored.Player.Name, Spells.Erase);
                    }
                    else if ((monitoredEffect == StatusEffect.INT_Down) && Form2.config.monitoredIntDown && PL.SpellAvailable(Spells.Erase) && plMonitoredSameParty())
                    {
                        CastSpell(Monitored.Player.Name, Spells.Erase);
                    }
                    else if ((monitoredEffect == StatusEffect.MND_Down) && Form2.config.monitoredMndDown && PL.SpellAvailable(Spells.Erase) && plMonitoredSameParty())
                    {
                        CastSpell(Monitored.Player.Name, Spells.Erase);
                    }
                    else if ((monitoredEffect == StatusEffect.CHR_Down) && Form2.config.monitoredChrDown && PL.SpellAvailable(Spells.Erase) && plMonitoredSameParty())
                    {
                        CastSpell(Monitored.Player.Name, Spells.Erase);
                    }
                    else if ((monitoredEffect == StatusEffect.Max_HP_Down) && Form2.config.monitoredMaxHpDown && PL.SpellAvailable(Spells.Erase) && plMonitoredSameParty())
                    {
                        CastSpell(Monitored.Player.Name, Spells.Erase);
                    }
                    else if ((monitoredEffect == StatusEffect.Max_MP_Down) && Form2.config.monitoredMaxMpDown && PL.SpellAvailable(Spells.Erase) && plMonitoredSameParty())
                    {
                        CastSpell(Monitored.Player.Name, Spells.Erase);
                    }
                    else if ((monitoredEffect == StatusEffect.Accuracy_Down) && Form2.config.monitoredAccuracyDown && PL.SpellAvailable(Spells.Erase) && plMonitoredSameParty())
                    {
                        CastSpell(Monitored.Player.Name, Spells.Erase);
                    }
                    else if ((monitoredEffect == StatusEffect.Evasion_Down) && Form2.config.monitoredEvasionDown && PL.SpellAvailable(Spells.Erase) && plMonitoredSameParty())
                    {
                        CastSpell(Monitored.Player.Name, Spells.Erase);
                    }
                    else if ((monitoredEffect == StatusEffect.Defense_Down) && Form2.config.monitoredDefenseDown && PL.SpellAvailable(Spells.Erase) && plMonitoredSameParty())
                    {
                        CastSpell(Monitored.Player.Name, Spells.Erase);
                    }
                    else if ((monitoredEffect == StatusEffect.Flash) && Form2.config.monitoredFlash && PL.SpellAvailable(Spells.Erase) && plMonitoredSameParty())
                    {
                        CastSpell(Monitored.Player.Name, Spells.Erase);
                    }
                    else if ((monitoredEffect == StatusEffect.Magic_Acc_Down) && Form2.config.monitoredMagicAccDown && PL.SpellAvailable(Spells.Erase) && plMonitoredSameParty())
                    {
                        CastSpell(Monitored.Player.Name, Spells.Erase);
                    }
                    else if ((monitoredEffect == StatusEffect.Magic_Atk_Down) && Form2.config.monitoredMagicAtkDown && PL.SpellAvailable(Spells.Erase) && plMonitoredSameParty())
                    {
                        CastSpell(Monitored.Player.Name, Spells.Erase);
                    }
                    else if ((monitoredEffect == StatusEffect.Helix) && Form2.config.monitoredHelix && PL.SpellAvailable(Spells.Erase) && plMonitoredSameParty())
                    {
                        CastSpell(Monitored.Player.Name, Spells.Erase);
                    }
                    else if ((monitoredEffect == StatusEffect.Max_TP_Down) && Form2.config.monitoredMaxTpDown && PL.SpellAvailable(Spells.Erase) && plMonitoredSameParty())
                    {
                        CastSpell(Monitored.Player.Name, Spells.Erase);
                    }
                    else if ((monitoredEffect == StatusEffect.Requiem) && Form2.config.monitoredRequiem && PL.SpellAvailable(Spells.Erase) && plMonitoredSameParty())
                    {
                        CastSpell(Monitored.Player.Name, Spells.Erase);
                    }
                    else if ((monitoredEffect == StatusEffect.Elegy) && Form2.config.monitoredElegy && PL.SpellAvailable(Spells.Erase) && plMonitoredSameParty())
                    {
                        CastSpell(Monitored.Player.Name, Spells.Erase);
                    }
                    else if ((monitoredEffect == StatusEffect.Threnody) && Form2.config.monitoredThrenody && PL.SpellAvailable(Spells.Erase) && plMonitoredSameParty())
                    {
                        CastSpell(Monitored.Player.Name, Spells.Erase);
                    }
                }
            }
            // End MONITORED Debuff Removal


            if (Form2.config.EnableAddOn)
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

                        if (debuffPriorityList.Any() && Form2.config.enablePartyDebuffRemoval && (characterNames_naRemoval.Contains(name) || Form2.config.SpecifiednaSpellsenable == false))
                        {
                            var targetDebuff = debuffPriorityList.First(status => Form2.DebuffEnabled[status] && PL.SpellAvailable(Data.DebuffPriorities[status]));

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

                    // Then reset any timers that need to be reset for buffs.
                    foreach (PartyMember ptMember in PL.Party.GetPartyMembers())
                    {
                        if (ActiveBuffs.ContainsKey(ptMember.Name))
                        {
                            if (ActiveBuffs[ptMember.Name].Any())
                            {
                                var buffs = ActiveBuffs[ptMember.Name];

                                // IF SLOW IS NOT ACTIVE, YET NEITHER IS HASTE / FLURRY DESPITE BEING ENABLED
                                // RESET THE TIMER TO FORCE IT TO BE CAST
                                if (!buffs.Contains((short)StatusEffect.Slow) && !buffs.Contains((short)StatusEffect.Haste) && !buffs.Contains((short)StatusEffect.Flurry) && !buffs.Contains((short)562))
                                {
                                    playerHaste[ptMember.MemberNumber] = new DateTime(1970, 1, 1, 0, 0, 0);
                                    playerHaste_II[ptMember.MemberNumber] = new DateTime(1970, 1, 1, 0, 0, 0);
                                    playerFlurry[ptMember.MemberNumber] = new DateTime(1970, 1, 1, 0, 0, 0);
                                    playerFlurry_II[ptMember.MemberNumber] = new DateTime(1970, 1, 1, 0, 0, 0);                                
                                }
                                // IF SUBLIMATION IS NOT ACTIVE, YET NEITHER IS REFRESH DESPITE BEING
                                // ENABLED RESET THE TIMER TO FORCE IT TO BE CAST
                                if (!buffs.Contains((short)StatusEffect.Sublimation_Activated) && !buffs.Contains((short)StatusEffect.Sublimation_Complete) && !buffs.Contains((short)StatusEffect.Refresh))
                                {
                                    playerRefresh[ptMember.MemberNumber] = new DateTime(1970, 1, 1, 0, 0, 0);  // ERROR                                   
                                }
                                // IF REGEN IS NOT ACTIVE DESPITE BEING ENABLED RESET THE TIMER TO
                                // FORCE IT TO BE CAST
                                if (!buffs.Contains((short)StatusEffect.Regen))
                                {
                                    playerRegen[ptMember.MemberNumber] = new DateTime(1970, 1, 1, 0, 0, 0);                                  
                                }
                                // IF PROTECT IS NOT ACTIVE DESPITE BEING ENABLED RESET THE TIMER TO
                                // FORCE IT TO BE CAST
                                if (!buffs.Contains((short)StatusEffect.Protect))
                                {
                                    playerProtect[ptMember.MemberNumber] = new DateTime(1970, 1, 1, 0, 0, 0);                                  
                                }

                                // IF SHELL IS NOT ACTIVE DESPITE BEING ENABLED RESET THE TIMER TO
                                // FORCE IT TO BE CAST
                                if (!buffs.Contains((short)StatusEffect.Shell))
                                {
                                    playerShell[ptMember.MemberNumber] = new DateTime(1970, 1, 1, 0, 0, 0);
                                }
                                // IF PHALANX II IS NOT ACTIVE DESPITE BEING ENABLED RESET THE TIMER
                                // TO FORCE IT TO BE CAST
                                if (!buffs.Contains((short)StatusEffect.Phalanx))
                                {
                                    playerPhalanx_II[ptMember.MemberNumber] = new DateTime(1970, 1, 1, 0, 0, 0);                                 
                                }
                                // If there's no storm tier where our buffs contain it's effect.
                                if(!Data.StormTiers.Any(tier => buffs.Contains(Data.SpellEffects[tier])))
                                {
                                    playerStormspell[ptMember.MemberNumber] = new DateTime(1970, 1, 1, 0, 0, 0);
                                }                                                                                                  
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
  
            if (Form2.config.curaga5enabled && (hpLoss >= Form2.config.curaga5Amount))
            {
                cureSpell = Spells.Curaga_V;          
            }
            else if (Form2.config.curaga4enabled && (hpLoss >= Form2.config.curaga4Amount))
            {
                cureSpell = Spells.Curaga_IV;           
            }
            else if (Form2.config.curaga3enabled && (hpLoss >= Form2.config.curaga3Amount))
            {
                cureSpell = Spells.Curaga_III;
            }
            else if (Form2.config.curaga2enabled && (hpLoss >= Form2.config.curaga2Amount))
            {
                cureSpell = Spells.Curaga_II;
            }
            else if (Form2.config.curagaEnabled && (hpLoss >= Form2.config.curagaAmount))
            {
                cureSpell = Spells.Curaga;
            }

            if (cureSpell != Spells.Unknown)
            {
                var curagaTier = PickCureTier(cureSpell, Data.CuragaTiers);
                if (Form2.config.curagaTargetType == 0)
                {
                    CastSpell(member.Name, curagaTier);
                }
                else
                {
                    CastSpell(Form2.config.curagaTargetName, curagaTier);
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

            if (Form2.config.trackCastingPackets == true && Form2.config.EnableAddOn == true)
            {
                if (!ProtectCasting.IsBusy) { ProtectCasting.RunWorkerAsync(); }
            }
            else
            {
                castingLockLabel.Text = "Casting is LOCKED";
                if (!ProtectCasting.IsBusy) { ProtectCasting.RunWorkerAsync(); }
            }
        }

        private void hastePlayer(byte partyMemberId)
        {
            CastSpell(Monitored.Party.GetPartyMembers()[partyMemberId].Name, Spells.Haste);
            playerHaste[partyMemberId] = DateTime.Now;
        }

        private void haste_IIPlayer(byte partyMemberId)
        {
            CastSpell(Monitored.Party.GetPartyMembers()[partyMemberId].Name, Spells.Haste_II);
            playerHaste_II[partyMemberId] = DateTime.Now;
        }

        private void AdloquiumPlayer(byte partyMemberId)
        {
            CastSpell(Monitored.Party.GetPartyMembers()[partyMemberId].Name, Spells.Adloquium);
            playerAdloquium[partyMemberId] = DateTime.Now;
        }

        private void FlurryPlayer(byte partyMemberId)
        {
            CastSpell(Monitored.Party.GetPartyMembers()[partyMemberId].Name, Spells.Flurry);
            playerFlurry[partyMemberId] = DateTime.Now;
        }

        private void Flurry_IIPlayer(byte partyMemberId)
        {
            CastSpell(Monitored.Party.GetPartyMembers()[partyMemberId].Name, Spells.Flurry_II);
            playerFlurry_II[partyMemberId] = DateTime.Now;
        }

        private void Phalanx_IIPlayer(byte partyMemberId)
        {
            CastSpell(Monitored.Party.GetPartyMembers()[partyMemberId].Name, Spells.Phalanx_II);
            playerPhalanx_II[partyMemberId] = DateTime.Now;
        }

        private void StormSpellPlayer(byte partyMemberId, string spell)
        {
            CastSpell(Monitored.Party.GetPartyMembers()[partyMemberId].Name, spell);
            playerStormspell[partyMemberId] = DateTime.Now;
        }

        private void Regen_Player(byte partyMemberId)
        {
            string[] regen_spells = { Spells.Regen, Spells.Regen_II, Spells.Regen_III, Spells.Regen_IV, Spells.Regen_V };
            CastSpell(Monitored.Party.GetPartyMembers()[partyMemberId].Name, regen_spells[Form2.config.autoRegen_Spell]);
            playerRegen[partyMemberId] = DateTime.Now;
        }

        private void Refresh_Player(byte partyMemberId)
        {
            string[] refresh_spells = { Spells.Refresh, Spells.Refresh_II, Spells.Refresh_III };
            CastSpell(Monitored.Party.GetPartyMembers()[partyMemberId].Name, refresh_spells[Form2.config.autoRefresh_Spell]);
            playerRefresh[partyMemberId] = DateTime.Now;
        }

        private void protectPlayer(byte partyMemberId)
        {
            string[] protect_spells = { Spells.Protect, Spells.Protect_II, Spells.Protect_III, Spells.Protect_IV, Spells.Protect_V };
            CastSpell(Monitored.Party.GetPartyMembers()[partyMemberId].Name, protect_spells[Form2.config.autoProtect_Spell]);
            playerProtect[partyMemberId] = DateTime.Now;
        }

        private void shellPlayer(byte partyMemberId)
        {
            string[] shell_spells = { Spells.Shell, Spells.Shell_II, Spells.Shell_III, Spells.Shell_IV, Spells.Shell_V };

            CastSpell(Monitored.Party.GetPartyMembers()[partyMemberId].Name, shell_spells[Form2.config.autoShell_Spell]);
            playerShell[partyMemberId] = DateTime.Now;
        }

        private bool ActiveSpikes()
        {
            if ((Form2.config.plSpikes_Spell == 0) && PL.HasStatus(StatusEffect.Blaze_Spikes))
            {
                return true;
            }
            else if ((Form2.config.plSpikes_Spell == 1) && PL.HasStatus(StatusEffect.Ice_Spikes))
            {
                return true;
            }
            else if ((Form2.config.plSpikes_Spell == 2) && PL.HasStatus(StatusEffect.Shock_Spikes))
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

        private void GrabPlayerMonitoredData()
        {
            for (int x = 0; x < 2048; x++)
            {
                XiEntity entity = PL.Entity.GetEntity(x);

                if (entity.Name != null && entity.Name == Monitored.Player.Name)
                {
                    Monitored_Index = entity.TargetID;
                }
                else if (entity.Name != null && entity.Name == PL.Player.Name)
                {
                    PL_Index = entity.TargetID;
                }
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


            GrabPlayerMonitoredData();

            // Grab current time for calculations below

            currentTime = DateTime.Now;
            // Calculate time since haste was cast on particular player
            playerHasteSpan[0] = currentTime.Subtract(playerHaste[0]);
            playerHasteSpan[1] = currentTime.Subtract(playerHaste[1]);
            playerHasteSpan[2] = currentTime.Subtract(playerHaste[2]);
            playerHasteSpan[3] = currentTime.Subtract(playerHaste[3]);
            playerHasteSpan[4] = currentTime.Subtract(playerHaste[4]);
            playerHasteSpan[5] = currentTime.Subtract(playerHaste[5]);
            playerHasteSpan[6] = currentTime.Subtract(playerHaste[6]);
            playerHasteSpan[7] = currentTime.Subtract(playerHaste[7]);
            playerHasteSpan[8] = currentTime.Subtract(playerHaste[8]);
            playerHasteSpan[9] = currentTime.Subtract(playerHaste[9]);
            playerHasteSpan[10] = currentTime.Subtract(playerHaste[10]);
            playerHasteSpan[11] = currentTime.Subtract(playerHaste[11]);
            playerHasteSpan[12] = currentTime.Subtract(playerHaste[12]);
            playerHasteSpan[13] = currentTime.Subtract(playerHaste[13]);
            playerHasteSpan[14] = currentTime.Subtract(playerHaste[14]);
            playerHasteSpan[15] = currentTime.Subtract(playerHaste[15]);
            playerHasteSpan[16] = currentTime.Subtract(playerHaste[16]);
            playerHasteSpan[17] = currentTime.Subtract(playerHaste[17]);

            playerHaste_IISpan[0] = currentTime.Subtract(playerHaste_II[0]);
            playerHaste_IISpan[1] = currentTime.Subtract(playerHaste_II[1]);
            playerHaste_IISpan[2] = currentTime.Subtract(playerHaste_II[2]);
            playerHaste_IISpan[3] = currentTime.Subtract(playerHaste_II[3]);
            playerHaste_IISpan[4] = currentTime.Subtract(playerHaste_II[4]);
            playerHaste_IISpan[5] = currentTime.Subtract(playerHaste_II[5]);
            playerHaste_IISpan[6] = currentTime.Subtract(playerHaste_II[6]);
            playerHaste_IISpan[7] = currentTime.Subtract(playerHaste_II[7]);
            playerHaste_IISpan[8] = currentTime.Subtract(playerHaste_II[8]);
            playerHaste_IISpan[9] = currentTime.Subtract(playerHaste_II[9]);
            playerHaste_IISpan[10] = currentTime.Subtract(playerHaste_II[10]);
            playerHaste_IISpan[11] = currentTime.Subtract(playerHaste_II[11]);
            playerHaste_IISpan[12] = currentTime.Subtract(playerHaste_II[12]);
            playerHaste_IISpan[13] = currentTime.Subtract(playerHaste_II[13]);
            playerHaste_IISpan[14] = currentTime.Subtract(playerHaste_II[14]);
            playerHaste_IISpan[15] = currentTime.Subtract(playerHaste_II[15]);
            playerHaste_IISpan[16] = currentTime.Subtract(playerHaste_II[16]);
            playerHaste_IISpan[17] = currentTime.Subtract(playerHaste_II[17]);

            playerFlurrySpan[0] = currentTime.Subtract(playerFlurry[0]);
            playerFlurrySpan[1] = currentTime.Subtract(playerFlurry[1]);
            playerFlurrySpan[2] = currentTime.Subtract(playerFlurry[2]);
            playerFlurrySpan[3] = currentTime.Subtract(playerFlurry[3]);
            playerFlurrySpan[4] = currentTime.Subtract(playerFlurry[4]);
            playerFlurrySpan[5] = currentTime.Subtract(playerFlurry[5]);
            playerFlurrySpan[6] = currentTime.Subtract(playerFlurry[6]);
            playerFlurrySpan[7] = currentTime.Subtract(playerFlurry[7]);
            playerFlurrySpan[8] = currentTime.Subtract(playerFlurry[8]);
            playerFlurrySpan[9] = currentTime.Subtract(playerFlurry[9]);
            playerFlurrySpan[10] = currentTime.Subtract(playerFlurry[10]);
            playerFlurrySpan[11] = currentTime.Subtract(playerFlurry[11]);
            playerFlurrySpan[12] = currentTime.Subtract(playerFlurry[12]);
            playerFlurrySpan[13] = currentTime.Subtract(playerFlurry[13]);
            playerFlurrySpan[14] = currentTime.Subtract(playerFlurry[14]);
            playerFlurrySpan[15] = currentTime.Subtract(playerFlurry[15]);
            playerFlurrySpan[16] = currentTime.Subtract(playerFlurry[16]);
            playerFlurrySpan[17] = currentTime.Subtract(playerFlurry[17]);

            playerFlurry_IISpan[0] = currentTime.Subtract(playerFlurry_II[0]);
            playerFlurry_IISpan[1] = currentTime.Subtract(playerFlurry_II[1]);
            playerFlurry_IISpan[2] = currentTime.Subtract(playerFlurry_II[2]);
            playerFlurry_IISpan[3] = currentTime.Subtract(playerFlurry_II[3]);
            playerFlurry_IISpan[4] = currentTime.Subtract(playerFlurry_II[4]);
            playerFlurry_IISpan[5] = currentTime.Subtract(playerFlurry_II[5]);
            playerFlurry_IISpan[6] = currentTime.Subtract(playerFlurry_II[6]);
            playerFlurry_IISpan[7] = currentTime.Subtract(playerFlurry_II[7]);
            playerFlurry_IISpan[8] = currentTime.Subtract(playerFlurry_II[8]);
            playerFlurry_IISpan[9] = currentTime.Subtract(playerFlurry_II[9]);
            playerFlurry_IISpan[10] = currentTime.Subtract(playerFlurry_II[10]);
            playerFlurry_IISpan[11] = currentTime.Subtract(playerFlurry_II[11]);
            playerFlurry_IISpan[12] = currentTime.Subtract(playerFlurry_II[12]);
            playerFlurry_IISpan[13] = currentTime.Subtract(playerFlurry_II[13]);
            playerFlurry_IISpan[14] = currentTime.Subtract(playerFlurry_II[14]);
            playerFlurry_IISpan[15] = currentTime.Subtract(playerFlurry_II[15]);
            playerFlurry_IISpan[16] = currentTime.Subtract(playerFlurry_II[16]);
            playerFlurry_IISpan[17] = currentTime.Subtract(playerFlurry_II[17]);

            // Calculate time since protect was cast on particular player
            playerProtect_Span[0] = currentTime.Subtract(playerProtect[0]);
            playerProtect_Span[1] = currentTime.Subtract(playerProtect[1]);
            playerProtect_Span[2] = currentTime.Subtract(playerProtect[2]);
            playerProtect_Span[3] = currentTime.Subtract(playerProtect[3]);
            playerProtect_Span[4] = currentTime.Subtract(playerProtect[4]);
            playerProtect_Span[5] = currentTime.Subtract(playerProtect[5]);
            playerProtect_Span[6] = currentTime.Subtract(playerProtect[6]);
            playerProtect_Span[7] = currentTime.Subtract(playerProtect[7]);
            playerProtect_Span[8] = currentTime.Subtract(playerProtect[8]);
            playerProtect_Span[9] = currentTime.Subtract(playerProtect[9]);
            playerProtect_Span[10] = currentTime.Subtract(playerProtect[10]);
            playerProtect_Span[11] = currentTime.Subtract(playerProtect[11]);
            playerProtect_Span[12] = currentTime.Subtract(playerProtect[12]);
            playerProtect_Span[13] = currentTime.Subtract(playerProtect[13]);
            playerProtect_Span[14] = currentTime.Subtract(playerProtect[14]);
            playerProtect_Span[15] = currentTime.Subtract(playerProtect[15]);
            playerProtect_Span[16] = currentTime.Subtract(playerProtect[16]);
            playerProtect_Span[17] = currentTime.Subtract(playerProtect[17]);

            // Calculate time since Stormspell was cast on particular player
            playerStormspellSpan[0] = currentTime.Subtract(playerStormspell[0]);
            playerStormspellSpan[1] = currentTime.Subtract(playerStormspell[1]);
            playerStormspellSpan[2] = currentTime.Subtract(playerStormspell[2]);
            playerStormspellSpan[3] = currentTime.Subtract(playerStormspell[3]);
            playerStormspellSpan[4] = currentTime.Subtract(playerStormspell[4]);
            playerStormspellSpan[5] = currentTime.Subtract(playerStormspell[5]);
            playerStormspellSpan[6] = currentTime.Subtract(playerStormspell[6]);
            playerStormspellSpan[7] = currentTime.Subtract(playerStormspell[7]);
            playerStormspellSpan[8] = currentTime.Subtract(playerStormspell[8]);
            playerStormspellSpan[9] = currentTime.Subtract(playerStormspell[9]);
            playerStormspellSpan[10] = currentTime.Subtract(playerStormspell[10]);
            playerStormspellSpan[11] = currentTime.Subtract(playerStormspell[11]);
            playerStormspellSpan[12] = currentTime.Subtract(playerStormspell[12]);
            playerStormspellSpan[13] = currentTime.Subtract(playerStormspell[13]);
            playerStormspellSpan[14] = currentTime.Subtract(playerStormspell[14]);
            playerStormspellSpan[15] = currentTime.Subtract(playerStormspell[15]);
            playerStormspellSpan[16] = currentTime.Subtract(playerStormspell[16]);
            playerStormspellSpan[17] = currentTime.Subtract(playerStormspell[17]);

            // Calculate time since shell was cast on particular player
            playerShell_Span[0] = currentTime.Subtract(playerShell[0]);
            playerShell_Span[1] = currentTime.Subtract(playerShell[1]);
            playerShell_Span[2] = currentTime.Subtract(playerShell[2]);
            playerShell_Span[3] = currentTime.Subtract(playerShell[3]);
            playerShell_Span[4] = currentTime.Subtract(playerShell[4]);
            playerShell_Span[5] = currentTime.Subtract(playerShell[5]);
            playerShell_Span[6] = currentTime.Subtract(playerShell[6]);
            playerShell_Span[7] = currentTime.Subtract(playerShell[7]);
            playerShell_Span[8] = currentTime.Subtract(playerShell[8]);
            playerShell_Span[9] = currentTime.Subtract(playerShell[9]);
            playerShell_Span[10] = currentTime.Subtract(playerShell[10]);
            playerShell_Span[11] = currentTime.Subtract(playerShell[11]);
            playerShell_Span[12] = currentTime.Subtract(playerShell[12]);
            playerShell_Span[13] = currentTime.Subtract(playerShell[13]);
            playerShell_Span[14] = currentTime.Subtract(playerShell[14]);
            playerShell_Span[15] = currentTime.Subtract(playerShell[15]);
            playerShell_Span[16] = currentTime.Subtract(playerShell[16]);
            playerShell_Span[17] = currentTime.Subtract(playerShell[17]);

            // Calculate time since phalanx II was cast on particular player
            playerPhalanx_IISpan[0] = currentTime.Subtract(playerPhalanx_II[0]);
            playerPhalanx_IISpan[1] = currentTime.Subtract(playerPhalanx_II[1]);
            playerPhalanx_IISpan[2] = currentTime.Subtract(playerPhalanx_II[2]);
            playerPhalanx_IISpan[3] = currentTime.Subtract(playerPhalanx_II[3]);
            playerPhalanx_IISpan[4] = currentTime.Subtract(playerPhalanx_II[4]);
            playerPhalanx_IISpan[5] = currentTime.Subtract(playerPhalanx_II[5]);

            // Calculate time since regen was cast on particular player
            playerRegen_Span[0] = currentTime.Subtract(playerRegen[0]);
            playerRegen_Span[1] = currentTime.Subtract(playerRegen[1]);
            playerRegen_Span[2] = currentTime.Subtract(playerRegen[2]);
            playerRegen_Span[3] = currentTime.Subtract(playerRegen[3]);
            playerRegen_Span[4] = currentTime.Subtract(playerRegen[4]);
            playerRegen_Span[5] = currentTime.Subtract(playerRegen[5]);

            // Calculate time since Refresh was cast on particular player
            playerRefresh_Span[0] = currentTime.Subtract(playerRefresh[0]);
            playerRefresh_Span[1] = currentTime.Subtract(playerRefresh[1]);
            playerRefresh_Span[2] = currentTime.Subtract(playerRefresh[2]);
            playerRefresh_Span[3] = currentTime.Subtract(playerRefresh[3]);
            playerRefresh_Span[4] = currentTime.Subtract(playerRefresh[4]);
            playerRefresh_Span[5] = currentTime.Subtract(playerRefresh[5]);

            // Calculate time since Songs were cast on particular player
            playerSong1_Span[0] = currentTime.Subtract(playerSong1[0]);
            playerSong2_Span[0] = currentTime.Subtract(playerSong2[0]);
            playerSong3_Span[0] = currentTime.Subtract(playerSong3[0]);
            playerSong4_Span[0] = currentTime.Subtract(playerSong4[0]);

            // Calculate time since Adloquium were cast on particular player
            playerAdloquium_Span[0] = currentTime.Subtract(playerAdloquium[0]);
            playerAdloquium_Span[1] = currentTime.Subtract(playerAdloquium[1]);
            playerAdloquium_Span[2] = currentTime.Subtract(playerAdloquium[2]);
            playerAdloquium_Span[3] = currentTime.Subtract(playerAdloquium[3]);
            playerAdloquium_Span[4] = currentTime.Subtract(playerAdloquium[4]);
            playerAdloquium_Span[5] = currentTime.Subtract(playerAdloquium[5]);
            playerAdloquium_Span[6] = currentTime.Subtract(playerAdloquium[6]);
            playerAdloquium_Span[7] = currentTime.Subtract(playerAdloquium[7]);
            playerAdloquium_Span[8] = currentTime.Subtract(playerAdloquium[8]);
            playerAdloquium_Span[9] = currentTime.Subtract(playerAdloquium[9]);
            playerAdloquium_Span[10] = currentTime.Subtract(playerAdloquium[10]);
            playerAdloquium_Span[11] = currentTime.Subtract(playerAdloquium[11]);
            playerAdloquium_Span[12] = currentTime.Subtract(playerAdloquium[12]);
            playerAdloquium_Span[13] = currentTime.Subtract(playerAdloquium[13]);
            playerAdloquium_Span[14] = currentTime.Subtract(playerAdloquium[14]);
            playerAdloquium_Span[15] = currentTime.Subtract(playerAdloquium[15]);
            playerAdloquium_Span[16] = currentTime.Subtract(playerAdloquium[16]);
            playerAdloquium_Span[17] = currentTime.Subtract(playerAdloquium[17]);


            Last_SongCast_Timer_Span[0] = currentTime.Subtract(Last_SongCast_Timer[0]);

            // Calculate time since Piannisimo Songs were cast on particular player
            pianissimo1_1_Span[0] = currentTime.Subtract(playerPianissimo1_1[0]);
            pianissimo2_1_Span[0] = currentTime.Subtract(playerPianissimo2_1[0]);
            pianissimo1_2_Span[0] = currentTime.Subtract(playerPianissimo1_2[0]);
            pianissimo2_2_Span[0] = currentTime.Subtract(playerPianissimo2_2[0]);

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


            int songs_currently_up1 = Monitored.Player.GetPlayerInfo().Buffs.Where(b => b == 197 || b == 198 || b == 195 || b == 199 || b == 200 || b == 215 || b == 196 || b == 214 || b == 216 || b == 218 || b == 222).Count();



            // IF ENABLED PAUSE ON KO
            if (Form2.config.pauseOnKO && (PL.Player.Status == 2 || PL.Player.Status == 3))
            {
                pauseButton.Text = "Paused!";
                pauseButton.ForeColor = Color.Red;
                actionTimer.Enabled = false;
                ActiveBuffs.Clear();
                pauseActions = true;
                if (Form2.config.FFXIDefaultAutoFollow == false)
                {
                    PL.AutoFollow.IsAutoFollowing = false;
                }
            }

            // IF YOU ARE DEAD BUT RERAISE IS AVAILABLE THEN ACCEPT RAISE
            if (Form2.config.AcceptRaise == true && (PL.Player.Status == 2 || PL.Player.Status == 3))
            {
                if (PL.Menu.IsMenuOpen && PL.Menu.HelpName == "Revival" && PL.Menu.MenuIndex == 1 && ((Form2.config.AcceptRaiseOnlyWhenNotInCombat == true && Monitored.Player.Status != 1) || Form2.config.AcceptRaiseOnlyWhenNotInCombat == false))
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
                if (PL.HasStatus(StatusEffect.Silence) && Form2.config.plSilenceItemEnabled)
                {
                    // Check to make sure we have echo drops
                    if (GetInventoryItemCount(PL, GetItemId(plSilenceitemName)) > 0 || GetTempItemCount(PL, GetItemId(plSilenceitemName)) > 0)
                    {
                        Item_Wait(plSilenceitemName);
                    }

                }
                else if (PL.HasStatus(StatusEffect.Doom) && Form2.config.plDoomEnabled /* Add more options from UI HERE*/)
                {
                    // Check to make sure we have holy water
                    if (GetInventoryItemCount(PL, GetItemId(plDoomItemName)) > 0 || GetTempItemCount(PL, GetItemId(plDoomItemName)) > 0)
                    {
                        PL.ThirdParty.SendString(string.Format("/item \"{0}\" <me>", plDoomItemName));
                        await Task.Delay(TimeSpan.FromSeconds(2));
                    }
                }

                else if (Form2.config.DivineSeal && PL.Player.MPP <= 11 && PL.AbilityAvailable(Ability.DivineSeal) && !PL.Player.Buffs.Contains((short)StatusEffect.Weakness))
                {
                    JobAbility_Wait(Ability.DivineSeal, Ability.DivineSeal);
                }
                else if (Form2.config.Convert && (PL.Player.MP <= Form2.config.convertMP) && PL.AbilityAvailable(Ability.Convert) && !PL.Player.Buffs.Contains((short)StatusEffect.Weakness))
                {
                    JobAbility_Wait(Ability.Convert, Ability.Convert);
                    return;
                }
                else if (Form2.config.RadialArcana && (PL.Player.MP <= Form2.config.RadialArcanaMP) && PL.AbilityAvailable(Ability.RadialArcana) && !PL.Player.Buffs.Contains((short)StatusEffect.Weakness))
                {
                    // Check if a pet is already active
                    if (PL.Player.Pet.HealthPercent >= 1 && PL.Player.Pet.Distance <= 9)
                    {
                        JobAbility_Wait(Ability.RadialArcana, Ability.RadialArcana);
                    }
                    else if (PL.Player.Pet.HealthPercent >= 1 && PL.Player.Pet.Distance >= 9 && PL.AbilityAvailable(Ability.FullCircle))
                    {
                        JobAbility_Wait(Ability.FullCircle, Ability.FullCircle);
                        await Task.Delay(2000);
                        string SpellCheckedResult = ReturnGeoSpell(Form2.config.RadialArcana_Spell, 2);
                        CastSpell(Target.Me, SpellCheckedResult);
                    }
                    else
                    {
                        string SpellCheckedResult = ReturnGeoSpell(Form2.config.RadialArcana_Spell, 2);
                        CastSpell(Target.Me, SpellCheckedResult);
                    }
                }
                else if (Form2.config.FullCircle)
                {


                    // When out of range Distance is 59 Yalms regardless, Must be within 15 yalms to gain
                    // the effect

                    //Check if "pet" is active and out of range of the monitored player
                    if (PL.Player.Pet.HealthPercent >= 1)
                    {
                        if (Form2.config.Fullcircle_GEOTarget == true && Form2.config.LuopanSpell_Target != "")
                        {

                            ushort PetsIndex = PL.Player.PetIndex;

                            XiEntity PetsEntity = PL.Entity.GetEntity(PetsIndex);

                            int FullCircle_CharID = 0;

                            for (int x = 0; x < 2048; x++)
                            {
                                XiEntity entity = PL.Entity.GetEntity(x);

                                if (entity.Name != null && entity.Name.ToLower().Equals(Form2.config.LuopanSpell_Target.ToLower()))
                                {
                                    FullCircle_CharID = Convert.ToInt32(entity.TargetID);
                                    break;
                                }
                            }

                            if (FullCircle_CharID != 0)
                            {
                                XiEntity FullCircleEntity = PL.Entity.GetEntity(FullCircle_CharID);

                                float fX = PetsEntity.X - FullCircleEntity.X;
                                float fY = PetsEntity.Y - FullCircleEntity.Y;
                                float fZ = PetsEntity.Z - FullCircleEntity.Z;

                                float generatedDistance = (float)Math.Sqrt((fX * fX) + (fY * fY) + (fZ * fZ));

                                if (generatedDistance >= 10)
                                {
                                    FullCircle_Timer.Enabled = true;
                                }
                            }

                        }
                        else if (Form2.config.Fullcircle_GEOTarget == false && Monitored.Player.Status == 1)
                        {
                            ushort PetsIndex = PL.Player.PetIndex;

                            XiEntity PetsEntity = Monitored.Entity.GetEntity(PetsIndex);

                            if (PetsEntity.Distance >= 10)
                            {
                                FullCircle_Timer.Enabled = true;
                            }
                        }

                    }
                }
                else if (Form2.config.Troubadour && PL.AbilityAvailable("Troubadour") && songs_currently_up1 == 0)
                {
                    JobAbility_Wait("Troubadour", "Troubadour");
                }
                else if (Form2.config.Nightingale && PL.AbilityAvailable("Nightingale") && songs_currently_up1 == 0)
                {
                    JobAbility_Wait("Nightingale", "Nightingale");
                }

                if (PL.Player.MP <= (int)Form2.config.mpMinCastValue && PL.Player.MP != 0)
                {
                    if (Form2.config.lowMPcheckBox && !islowmp && !Form2.config.healLowMP)
                    {
                        PL.ThirdParty.SendString("/tell " + Monitored.Player.Name + " MP is low!");
                        islowmp = true;
                        return;
                    }
                    islowmp = true;
                    return;
                }
                if (PL.Player.MP > (int)Form2.config.mpMinCastValue && PL.Player.MP != 0)
                {
                    if (Form2.config.lowMPcheckBox && islowmp && !Form2.config.healLowMP)
                    {
                        PL.ThirdParty.SendString("/tell " + Monitored.Player.Name + " MP OK!");
                        islowmp = false;
                    }
                }

                if (Form2.config.healLowMP == true && PL.Player.MP <= Form2.config.healWhenMPBelow && PL.Player.Status == 0)
                {
                    if (Form2.config.lowMPcheckBox && !islowmp)
                    {
                        PL.ThirdParty.SendString("/tell " + Monitored.Player.Name + " MP is seriously low, /healing.");
                        islowmp = true;
                    }
                    PL.ThirdParty.SendString("/heal");
                }
                else if (Form2.config.standAtMP == true && PL.Player.MPP >= Form2.config.standAtMP_Percentage && PL.Player.Status == 33)
                {
                    if (Form2.config.lowMPcheckBox && !islowmp)
                    {
                        PL.ThirdParty.SendString("/tell " + Monitored.Player.Name + " MP has recovered.");
                        islowmp = false;
                    }
                    PL.ThirdParty.SendString("/heal");
                }

                // Only perform actions if PL is stationary PAUSE GOES HERE
                if ((PL.Player.X == plX) && (PL.Player.Y == plY) && (PL.Player.Z == plZ) && (PL.Player.LoginStatus == (int)LoginStatus.LoggedIn) && JobAbilityLock_Check != true && CastingBackground_Check != true && curePlease_autofollow == false && ((PL.Player.Status == (uint)Status.Standing) || (PL.Player.Status == (uint)Status.Fighting)))
                {
                    plSilenceitemName = Items.SilenceRemoval[Form2.config.plSilenceItem];

                    if (PL.Player.Buffs.Contains((short)StatusEffect.Silence) && Form2.config.plSilenceItemEnabled)
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

                    /////////////////////////// CURSE CHECK /////////////////////////////////////
                    var cursedMembers = partyByHP.Count(pm => PL.CanCastOn(pm) && ActiveBuffs.ContainsKey(pm.Name) && ActiveBuffs[pm.Name].Contains((short)StatusEffect.Doom));
                    if(cursedMembers > 0)
                    {
                        RunDebuffChecker();
                    }

                    /////////////////////////// PL CURE //////////////////////////////////////////////////////////////////////////////////////////////////////////////////

                    // TODO: Test this! Pretty sure your own character is always party member index 0.
                    if (PL.Player.HP > 0 && (PL.Player.HPP <= Form2.config.monitoredCurePercentage) && Form2.config.enableOutOfPartyHealing == true && PLInParty() == false)
                    {
                        var plAsPartyMember = PL.Party.GetPartyMember(0);
                        CureCalculator(plAsPartyMember);
                    }

                    /////////////////////////// CURAGA //////////////////////////////////////////////////////////////////////////////////////////////////////////////////                                    

                    int plParty = PLPartyRelativeToMonitored();

                    // Order parties that qualify for AOE cures by average missing HP.
                    var partyNeedsAoe = Monitored.PartyNeedsAoeCure((int)Form2.config.curagaRequiredMembers, Form2.config.curagaCurePercentage).OrderBy(partyNumber => Monitored.AverageHpLossForParty(partyNumber));

                    // If PL is in same alliance, and there's at least 1 party that needs an AOE cure.
                    // Parties are ordered by most average missing HP.
                    if (plParty > 0 && partyNeedsAoe.Any())
                    {
                        int targetParty = 0;
                        
                        // We can accession if we have light arts/addendum white, and either we already have the status or we have the ability available,
                        // and have the charges to use it.
                        bool plCanAccession = (PL.HasStatus(StatusEffect.Light_Arts) || PL.HasStatus(StatusEffect.Addendum_White)) 
                            && (PL.HasStatus(StatusEffect.Accession) || (PL.AbilityAvailable(Ability.Accession) && PL.CurrentSCHCharges() > 0));
                                            
                        foreach(int party in partyNeedsAoe)
                        {
                            // We check whether we can accession here, so that if we can't accession we don't skip a chance to curaga our own party.
                            if(party != plParty && !plCanAccession) {
                                continue;
                            }

                            // We get the first party with at least 1 person who's in it and checked.
                            // As well as 1 person who's both under the cure threshold AND in casting range.
                            if (partyByHP.Count(pm => pm.InParty(party) && enabledBoxes[pm.MemberNumber].Checked) > 0)
                            {
                                if(partyByHP.Count(pm => pm.InParty(party) && pm.CurrentHPP < Form2.config.curagaCurePercentage && PL.CanCastOn(pm)) > 0)
                                {
                                    targetParty = party;
                                }
                            }                         
                        }

                        if (targetParty > 0)
                        {
                            // The target is the first person
                            var target = partyByHP.First(pm => pm.InParty(targetParty) && PL.CanCastOn(pm));

                            if (target != null)
                            {
                                // If same party as PL, curaga. Otherwise we try to accession cure.
                                if (targetParty == plParty)
                                {
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

                    /////////////////////////// CURE //////////////////////////////////////////////////////////////////////////////////////////////////////////////////

                    // First run a check on the monitored target
                    PartyMember monitoredPlayer = partyByHP.FirstOrDefault(p => p.Name == Monitored.Player.Name);

                    if (Form2.config.enableMonitoredPriority && monitoredPlayer.CurrentHP > 0 && (monitoredPlayer.CurrentHPP <= Form2.config.monitoredCurePercentage))
                    {
                        CureCalculator(monitoredPlayer);
                        return;
                    }
                    else
                    {
                        // Calculate who needs a cure, and is a valid target.
                        // Anyone who's: Enabled + Active + Alive + Under cure threshold
                        var validCures = partyByHP.Where(pm => enabledBoxes[pm.MemberNumber].Checked && (pm.CurrentHPP <= Form2.config.curePercentage) && PL.CanCastOn(pm));

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

                    string BarspellName = string.Empty;
                    int BarspellBuffID = 0;
                    bool BarSpell_AOE = false;

                    if (Form2.config.AOE_Barelemental == false)
                    {
                        SpellsData barspell = barspells.Where(c => c.spell_position == Form2.config.plBarElement_Spell && c.type == 1 && c.aoe_version != true).SingleOrDefault();

                        BarspellName = barspell.Spell_Name;
                        BarspellBuffID = barspell.buffID;
                        BarSpell_AOE = false;
                    }
                    else
                    {
                        SpellsData barspell = barspells.Where(c => c.spell_position == Form2.config.plBarElement_Spell && c.type == 1 && c.aoe_version == true).SingleOrDefault();

                        BarspellName = barspell.Spell_Name;
                        BarspellBuffID = barspell.buffID;
                        BarSpell_AOE = true;
                    }

                    string BarstatusName = string.Empty;
                    int BarstatusBuffID = 0;
                    bool BarStatus_AOE = false;

                    if (Form2.config.AOE_Barstatus == false)
                    {
                        SpellsData barstatus = barspells.Where(c => c.spell_position == Form2.config.plBarStatus_Spell && c.type == 2 && c.aoe_version != true).SingleOrDefault();

                        BarstatusName = barstatus.Spell_Name;
                        BarstatusBuffID = barstatus.buffID;
                        BarStatus_AOE = false;
                    }
                    else
                    {
                        SpellsData barstatus = barspells.Where(c => c.spell_position == Form2.config.plBarStatus_Spell && c.type == 2 && c.aoe_version == true).SingleOrDefault();

                        BarstatusName = barstatus.Spell_Name;
                        BarstatusBuffID = barstatus.buffID;
                        BarStatus_AOE = true;
                    }

                    SpellsData enspell = enspells.Where(c => c.spell_position == Form2.config.plEnspell_Spell && c.type == 1).SingleOrDefault();
                    string stormspell = Data.StormTiers[Form2.config.plStormSpell_Spell];
                    string gainBoostSpell = Data.GainBoostSpells[Form2.config.plGainBoost_Spell];

                    if (PL.Player.LoginStatus == (int)LoginStatus.LoggedIn && JobAbilityLock_Check != true && CastingBackground_Check != true)
                    {
                        if (Form2.config.Composure && (!PL.HasStatus(StatusEffect.Composure)) && PL.AbilityAvailable(Ability.Composure))
                        {

                            JobAbility_Wait(Ability.Composure, Ability.Composure);
                        }
                        else if (Form2.config.LightArts && (!PL.HasStatus(StatusEffect.Light_Arts)) && (!PL.HasStatus(StatusEffect.Addendum_White)) && PL.AbilityAvailable(Ability.LightArts))
                        {
                            JobAbility_Wait(Ability.LightArts, Ability.LightArts);
                        }
                        else if (Form2.config.AddendumWhite && (!PL.HasStatus(StatusEffect.Addendum_White)) && PL.HasStatus(StatusEffect.Light_Arts) && PL.CurrentSCHCharges() > 0)
                        {
                            JobAbility_Wait(Ability.AddendumWhite, Ability.AddendumWhite);
                        }
                        else if (Form2.config.DarkArts && (!PL.HasStatus(StatusEffect.Dark_Arts)) && (!PL.HasStatus(StatusEffect.Addendum_Black)) && PL.AbilityAvailable(Ability.DarkArts))
                        {
                            JobAbility_Wait(Ability.DarkArts, Ability.DarkArts);
                        }
                        else if (Form2.config.AddendumBlack && PL.HasStatus(StatusEffect.Dark_Arts) && (!PL.HasStatus(StatusEffect.Addendum_Black)) && PL.CurrentSCHCharges() > 0)
                        {
                            JobAbility_Wait(Ability.AddendumBlack, Ability.AddendumBlack);
                        }
                        else if (Form2.config.plShellra && (!PL.HasStatus(StatusEffect.Shell)))
                        {
                            var shellraSpell = Data.ShellraTiers[(int)Form2.config.plShellra_Level - 1];
                            if (PL.SpellAvailable(shellraSpell))
                            {
                                CastSpell(Target.Me, shellraSpell);
                            }
                        }
                        else if (Form2.config.plProtectra && (!PL.HasStatus(StatusEffect.Protect)))
                        {
                            var protectraSpell = Data.ProtectraTiers[(int)Form2.config.plProtectra_Level - 1];
                            if (PL.SpellAvailable(protectraSpell))
                            {
                                CastSpell(Target.Me, protectraSpell);
                            }
                        }
                        else if (Form2.config.plBarElement && !PL.HasStatus((short)BarspellBuffID) && PL.SpellAvailable(BarspellName))
                        {
                            if (Form2.config.Accession && Form2.config.barspellAccession && PL.CurrentSCHCharges() > 0 && PL.AbilityAvailable(Ability.Accession) && BarSpell_AOE == false && !PL.HasStatus(StatusEffect.Accession))
                            {
                                JobAbility_Wait("Barspell, Accession", Ability.Accession);
                                return;
                            }

                            if (Form2.config.Perpetuance && Form2.config.barspellPerpetuance && PL.CurrentSCHCharges() > 0 && PL.AbilityAvailable(Ability.Perpetuance) && !PL.HasStatus(StatusEffect.Perpetuance))
                            {
                                JobAbility_Wait("Barspell, Perpetuance", Ability.Perpetuance);
                                return;
                            }

                            CastSpell(Target.Me, BarspellName);
                        }
                        else if (Form2.config.plBarStatus && !PL.HasStatus((short)BarstatusBuffID) && PL.SpellAvailable(BarstatusName))
                        {
                            if (Form2.config.Accession && Form2.config.barstatusAccession && PL.CurrentSCHCharges() > 0 && PL.AbilityAvailable(Ability.Accession) && BarStatus_AOE == false && !PL.HasStatus(StatusEffect.Accession))
                            {
                                JobAbility_Wait("Barstatus, Accession", Ability.Accession);
                                return;
                            }

                            if (Form2.config.Perpetuance && Form2.config.barstatusPerpetuance && PL.CurrentSCHCharges() > 0 && PL.AbilityAvailable(Ability.Perpetuance) && !PL.HasStatus(StatusEffect.Perpetuance))
                            {
                                JobAbility_Wait("Barstatus, Perpetuance", Ability.Perpetuance);
                                return;
                            }

                            CastSpell(Target.Me, BarstatusName);
                        }
                        else if (Form2.config.plGainBoost && !PL.HasStatus(Data.SpellEffects[gainBoostSpell]) && PL.SpellAvailable(gainBoostSpell))
                        {
                            CastSpell(Target.Me, gainBoostSpell);
                        }
                        else if (Form2.config.plStormSpell && !PL.HasStatus(Data.SpellEffects[stormspell]) && PL.SpellAvailable(stormspell))
                        {
                            if (Form2.config.Accession && Form2.config.stormspellAccession && PL.CurrentSCHCharges() > 0 && PL.AbilityAvailable(Ability.Accession) && !PL.HasStatus(StatusEffect.Accession))
                            {
                                JobAbility_Wait("Stormspell, Accession", Ability.Accession);
                                return;
                            }

                            if (Form2.config.Perpetuance && Form2.config.stormspellPerpetuance && PL.CurrentSCHCharges() > 0 && PL.AbilityAvailable(Ability.Perpetuance) && !PL.HasStatus(StatusEffect.Perpetuance))
                            {
                                JobAbility_Wait("Stormspell, Perpetuance", Ability.Perpetuance);
                                return;
                            }

                            CastSpell(Target.Me, stormspell);
                        }
                        else if (Form2.config.plProtect && (!PL.HasStatus(StatusEffect.Protect)))
                        {
                            string protectSpell = Data.ProtectTiers[Form2.config.autoProtect_Spell];

                            if (protectSpell != Spells.Unknown && PL.SpellAvailable(protectSpell))
                            {
                                if (Form2.config.Accession && Form2.config.accessionProShell && PL.Party.GetPartyMembers().Count() > 2 && PL.AbilityAvailable(Ability.Accession) && PL.CurrentSCHCharges() > 0)
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
                        else if (Form2.config.plShell && (!PL.HasStatus(StatusEffect.Shell)))
                        {
                            string shellSpell = Data.ShellTiers[Form2.config.autoShell_Spell];

                            if (shellSpell != Spells.Unknown && PL.SpellAvailable(shellSpell))
                            {
                                if (Form2.config.Accession && Form2.config.accessionProShell && PL.Party.GetPartyMembers().Count() > 2 && PL.CurrentSCHCharges() > 0 && PL.AbilityAvailable(Ability.Accession))
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
                        else if (Form2.config.plReraise && Form2.config.EnlightenmentReraise && (!PL.HasStatus(StatusEffect.Reraise)) && !PL.HasStatus(StatusEffect.Addendum_White) && PL.AbilityAvailable(Ability.Enlightenment))
                        {
                            if (!PL.HasStatus(StatusEffect.Enlightenment) && PL.AbilityAvailable(Ability.Enlightenment))
                            {
                                JobAbility_Wait("Reraise, Enlightenment", Ability.Enlightenment);
                                return;
                            }

                            var reraiseSpell = Data.ReraiseTiers[Form2.config.plReraise_Level-1];
                            if (PL.HasMPFor(reraiseSpell) && PL.SpellAvailable(reraiseSpell))
                            {
                                CastSpell(Target.Me, reraiseSpell);
                            }
                        }
                        else if (Form2.config.plReraise && (!PL.HasStatus(StatusEffect.Reraise)))
                        {
                            var reraiseSpell = Data.ReraiseTiers[Form2.config.plReraise_Level-1];
                            if (PL.HasMPFor(reraiseSpell) && PL.SpellAvailable(reraiseSpell)) 
                            {
                                CastSpell(Target.Me, reraiseSpell);
                            }                           
                        }
                        else if (Form2.config.plUtsusemi && PL.ShadowsRemaining() < 2)
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
                        
                        else if (Form2.config.plBlink && (!PL.HasStatus(StatusEffect.Blink)) && PL.SpellAvailable(Spells.Blink))
                        {

                            if (Form2.config.Accession && Form2.config.blinkAccession && PL.CurrentSCHCharges() > 0 && PL.AbilityAvailable(Ability.Accession) && !PL.HasStatus(StatusEffect.Accession))
                            {
                                JobAbility_Wait("Blink, Accession", Ability.Accession);
                                return;
                            }

                            if (Form2.config.Perpetuance && Form2.config.blinkPerpetuance && PL.CurrentSCHCharges() > 0 && PL.AbilityAvailable(Ability.Perpetuance) && !PL.HasStatus(StatusEffect.Perpetuance))
                            {
                                JobAbility_Wait("Blink, Perpetuance", Ability.Perpetuance);
                                return;
                            }

                            CastSpell(Target.Me, Spells.Blink);
                        }
                        else if (Form2.config.plPhalanx && (!PL.HasStatus(StatusEffect.Phalanx)) && PL.SpellAvailable(Spells.Phalanx))
                        {
                            if (Form2.config.Accession && Form2.config.phalanxAccession && PL.CurrentSCHCharges() > 0 && PL.AbilityAvailable(Ability.Accession) && !PL.HasStatus(StatusEffect.Accession))
                            {
                                JobAbility_Wait("Phalanx, Accession", Ability.Accession);
                                return;
                            }

                            if (Form2.config.Perpetuance && Form2.config.phalanxPerpetuance && PL.CurrentSCHCharges() > 0 && PL.AbilityAvailable(Ability.Perpetuance) && !PL.HasStatus(StatusEffect.Perpetuance))
                            {
                                JobAbility_Wait("Phalanx, Perpetuance", Ability.Perpetuance);
                                return;
                            }

                            CastSpell(Target.Me, Spells.Phalanx);
                        }
                        else if (Form2.config.plRefresh && (!PL.HasStatus(StatusEffect.Refresh)))
                        {
                            var refreshSpell = Data.RefreshTiers[Form2.config.plRefresh_Level - 1];

                            if (PL.SpellAvailable(Spells.Refresh))
                            {
                                if (Form2.config.Accession && Form2.config.refreshAccession && PL.CurrentSCHCharges() > 0 && PL.AbilityAvailable(Ability.Accession) && !PL.HasStatus(StatusEffect.Accession))
                                {
                                    JobAbility_Wait("Refresh, Accession", Ability.Accession);
                                    return;
                                }
                                if (Form2.config.Perpetuance && Form2.config.refreshPerpetuance && PL.CurrentSCHCharges() > 0 && PL.AbilityAvailable(Ability.Perpetuance) && !PL.HasStatus(StatusEffect.Perpetuance))
                                {
                                    JobAbility_Wait("Refresh, Perpetuance", Ability.Perpetuance);
                                    return;
                                }

                                CastSpell(Target.Me, refreshSpell);
                            }                         
                        }
                        else if (Form2.config.plRegen && (!PL.HasStatus(StatusEffect.Regen)))
                        {
                            if (Form2.config.Accession && Form2.config.regenAccession && PL.CurrentSCHCharges() > 0 && PL.AbilityAvailable(Ability.Accession) && !PL.HasStatus(StatusEffect.Accession))
                            {
                                JobAbility_Wait("Regen, Accession", Ability.Accession);
                                return;
                            }
                            if (Form2.config.Perpetuance && Form2.config.regenPerpetuance && PL.CurrentSCHCharges() > 0 && PL.AbilityAvailable(Ability.Perpetuance) && !PL.HasStatus(StatusEffect.Perpetuance))
                            {
                                JobAbility_Wait("Regen, Perpetuance", Ability.Perpetuance);
                                return;
                            }

                            var regenSpell = Data.RegenTiers[Form2.config.plRegen_Level - 1];
                            if(PL.HasMPFor(regenSpell) && PL.SpellAvailable(regenSpell))
                            {
                                CastSpell(Target.Me, regenSpell);
                            }                    
                        }
                        else if (Form2.config.plAdloquium && (!PL.HasStatus(StatusEffect.Regain)) && PL.SpellAvailable(Spells.Adloquium))
                        {
                            if (Form2.config.Accession && Form2.config.adloquiumAccession && PL.CurrentSCHCharges() > 0 && PL.AbilityAvailable(Ability.Accession) && !PL.HasStatus(StatusEffect.Accession))
                            {
                                JobAbility_Wait("Adloquium, Accession", Ability.Accession);
                                return;
                            }

                            if (Form2.config.Perpetuance && Form2.config.adloquiumPerpetuance && PL.CurrentSCHCharges() > 0 && PL.AbilityAvailable(Ability.Perpetuance) && !PL.HasStatus(StatusEffect.Perpetuance))
                            {
                                JobAbility_Wait("Adloquium, Perpetuance", Ability.Perpetuance);
                                return;
                            }

                            CastSpell(Target.Me, Spells.Adloquium);
                        }
                        else if (Form2.config.plStoneskin && (!PL.HasStatus(StatusEffect.Stoneskin)) && PL.SpellAvailable(Spells.Stoneskin))
                        {
                            if (Form2.config.Accession && Form2.config.stoneskinAccession && PL.CurrentSCHCharges() > 0 && PL.AbilityAvailable(Ability.Accession) && !PL.HasStatus(StatusEffect.Accession))
                            {
                                JobAbility_Wait("Stoneskin, Accession", Ability.Accession);
                                return;
                            }

                            if (Form2.config.Perpetuance && Form2.config.stoneskinPerpetuance && PL.CurrentSCHCharges() > 0 && PL.AbilityAvailable(Ability.Perpetuance) && !PL.HasStatus(StatusEffect.Perpetuance))
                            {
                                JobAbility_Wait("Stoneskin, Perpetuance", Ability.Perpetuance);
                                return;
                            }

                            CastSpell(Target.Me, Spells.Stoneskin);
                        }
                        else if (Form2.config.plAquaveil && (!PL.HasStatus(StatusEffect.Aquaveil)) && PL.SpellAvailable(Spells.Aquaveil))
                        {
                            if (Form2.config.Accession && Form2.config.aquaveilAccession && PL.CurrentSCHCharges() > 0 && PL.AbilityAvailable(Ability.Accession) && !PL.HasStatus(StatusEffect.Accession))
                            {
                                JobAbility_Wait("Aquaveil, Accession", Ability.Accession);
                                return;
                            }

                            if (Form2.config.Perpetuance && Form2.config.aquaveilPerpetuance && PL.CurrentSCHCharges() > 0 && PL.AbilityAvailable(Ability.Perpetuance) && PL.HasStatus(StatusEffect.Perpetuance))
                            {
                                JobAbility_Wait("Aquaveil, Perpetuance", Ability.Perpetuance);
                                return;
                            }

                            CastSpell(Target.Me, Spells.Aquaveil);
                        }                       
                        else if (Form2.config.plKlimaform && !PL.HasStatus(StatusEffect.Klimaform))
                        {
                            if (PL.SpellAvailable(Spells.Klimaform))
                            {
                                CastSpell(Target.Me, Spells.Klimaform);
                            }
                        }
                        else if (Form2.config.plTemper && (!PL.HasStatus(StatusEffect.Multi_Strikes)))
                        {
                            if ((Form2.config.plTemper_Level == 1) && PL.SpellAvailable(Spells.Temper))
                            {
                                CastSpell(Target.Me, Spells.Temper);
                            }
                            else if ((Form2.config.plTemper_Level == 2) && PL.SpellAvailable(Spells.Temper_II))
                            {
                                CastSpell(Target.Me, Spells.Temper_II);
                            }
                        }
                        else if (Form2.config.plHaste && (!PL.HasStatus(StatusEffect.Haste)))
                        {
                            if ((Form2.config.plHaste_Level == 1) && PL.SpellAvailable(Spells.Haste))
                            {
                                CastSpell(Target.Me,   Spells.Haste);
                            }
                            else if ((Form2.config.plHaste_Level == 2) && PL.SpellAvailable(Spells.Haste_II))
                            {
                                CastSpell(Target.Me, Spells.Haste_II);
                            }
                        }
                        else if (Form2.config.plSpikes && ActiveSpikes() == false)
                        {
                            if ((Form2.config.plSpikes_Spell == 0) && PL.SpellAvailable(Spells.Blaze_Spikes))
                            {
                                CastSpell(Target.Me, Spells.Blaze_Spikes);
                            }
                            else if ((Form2.config.plSpikes_Spell == 1) && PL.SpellAvailable(Spells.Ice_Spikes))
                            {
                                CastSpell(Target.Me, Spells.Ice_Spikes);
                            }
                            else if ((Form2.config.plSpikes_Spell == 2) && PL.SpellAvailable(Spells.Shock_Spikes))
                            {
                                CastSpell(Target.Me, Spells.Shock_Spikes);
                            }
                        }
                        else if (Form2.config.plEnspell && !PL.HasStatus((short)enspell.buffID) && PL.SpellAvailable(enspell.Spell_Name))
                        {
                            if (Form2.config.Accession && Form2.config.enspellAccession && PL.CurrentSCHCharges() > 0 && PL.AbilityAvailable(Ability.Accession) && enspell.spell_position < 6 && !PL.HasStatus(StatusEffect.Accession))
                            {
                                JobAbility_Wait("Enspell, Accession", Ability.Accession);
                                return;
                            }

                            if (Form2.config.Perpetuance && Form2.config.enspellPerpetuance && PL.CurrentSCHCharges() > 0 && PL.AbilityAvailable(Ability.Perpetuance) && enspell.spell_position < 6 && !PL.HasStatus(StatusEffect.Perpetuance))
                            {
                                JobAbility_Wait("Enspell, Perpetuance", Ability.Perpetuance);
                                return;
                            }

                            CastSpell(Target.Me, enspell.Spell_Name);
                        }
                        else if (Form2.config.plAuspice && (!PL.HasStatus(StatusEffect.Auspice)) && PL.SpellAvailable(Spells.Auspice))
                        {
                            CastSpell(Target.Me, Spells.Auspice);
                        }
                        #endregion

                        // ENTRUSTED INDI SPELL CASTING, WILL BE CAST SO LONG AS ENTRUST IS ACTIVE
                        else if (Form2.config.EnableGeoSpells && PL.HasStatus((StatusEffect)584) && PL.Player.Status != 33)
                        {
                            string SpellCheckedResult = ReturnGeoSpell(Form2.config.EntrustedSpell_Spell, 1);
                            if (SpellCheckedResult == "SpellError_Cancel")
                            {
                                Form2.config.EnableGeoSpells = false;
                                MessageBox.Show("An error has occurred with Entrusted INDI spell casting, please report what spell was active at the time.");
                            }
                            else if (SpellCheckedResult == "SpellRecast" || SpellCheckedResult == "SpellUnknown")
                            {
                            }
                            else
                            {
                                if (string.IsNullOrEmpty(Form2.config.EntrustedSpell_Target))
                                {
                                    CastSpell(Monitored.Player.Name, SpellCheckedResult);
                                }
                                else
                                {
                                    CastSpell(Form2.config.EntrustedSpell_Target, SpellCheckedResult);
                                }
                            }
                        }

                        // CAST NON ENTRUSTED INDI SPELL
                        else if (Form2.config.EnableGeoSpells && !PL.HasStatus(612) && PL.Player.Status != 33 && (CheckEngagedStatus() == true || !Form2.config.IndiWhenEngaged))
                        {
                            string SpellCheckedResult = ReturnGeoSpell(Form2.config.IndiSpell_Spell, 1);

                            if (SpellCheckedResult == "SpellError_Cancel")
                            {
                                Form2.config.EnableGeoSpells = false;
                                MessageBox.Show("An error has occurred with INDI spell casting, please report what spell was active at the time.");
                            }
                            else if (SpellCheckedResult == "SpellRecast" || SpellCheckedResult == "SpellUnknown")
                            {
                            }
                            else
                            {
                                CastSpell(Target.Me, SpellCheckedResult);
                            }

                        }

                        // GEO SPELL CASTING 
                        else if (Form2.config.EnableLuopanSpells && (PL.Player.Pet.HealthPercent < 1) && (CheckEngagedStatus() == true))
                        {
                            // Use BLAZE OF GLORY if ENABLED
                            if (Form2.config.BlazeOfGlory && PL.AbilityAvailable("Blaze of Glory") && CheckEngagedStatus() == true && GEO_EnemyCheck() == true)
                            {
                                JobAbility_Wait("Blaze of Glory", "Blaze of Glory");
                            }

                            // Grab GEO spell name
                            string SpellCheckedResult = ReturnGeoSpell(Form2.config.GeoSpell_Spell, 2);

                            if (SpellCheckedResult == "SpellError_Cancel")
                            {
                                Form2.config.EnableGeoSpells = false;
                                MessageBox.Show("An error has occurred with GEO spell casting, please report what spell was active at the time.");
                            }
                            else if (SpellCheckedResult == "SpellRecast" || SpellCheckedResult == "SpellUnknown")
                            {
                                // Do nothing and continue on with the program
                            }
                            else
                            {
                                if (PL.Resources.GetSpell(SpellCheckedResult, 0).ValidTargets == 5)
                                { // PLAYER CHARACTER TARGET
                                    var target = string.IsNullOrEmpty(Form2.config.LuopanSpell_Target) ? monitoredPlayer.Name : Form2.config.LuopanSpell_Target;

                                    if (PL.HasStatus(516)) // IF ECLIPTIC IS UP THEN ACTIVATE THE BOOL
                                    {
                                        EclipticStillUp = true;
                                    }

                                    CastSpell(target, SpellCheckedResult);
                                    
                                }
                                else
                                { // ENEMY BASED TARGET NEED TO ASSURE PLAYER IS ENGAGED
                                    if (CheckEngagedStatus() == true)
                                    {

                                        int GrabbedTargetID = GrabGEOTargetID();

                                        if (GrabbedTargetID != 0)
                                        {

                                            PL.Target.SetTarget(GrabbedTargetID);
                                            await Task.Delay(TimeSpan.FromSeconds(1));

                                            if (PL.HasStatus(516)) // IF ECLIPTIC IS UP THEN ACTIVATE THE BOOL
                                            {
                                                EclipticStillUp = true;
                                            }

                                            CastSpell("<t>", SpellCheckedResult);
                                            await Task.Delay(TimeSpan.FromSeconds(4));
                                            if (Form2.config.DisableTargettingCancel == false)
                                            {
                                                await Task.Delay(TimeSpan.FromSeconds((double)Form2.config.TargetRemoval_Delay));
                                                PL.Target.SetTarget(0);
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        else if (Form2.config.autoTarget && PL.SpellAvailable(Form2.config.autoTargetSpell))
                        {
                            if (Form2.config.Hate_SpellType == 1) // PARTY BASED HATE SPELL
                            {
                                int enemyID = CheckEngagedStatus_Hate();

                                if (enemyID != 0 && enemyID != lastKnownEstablisherTarget)
                                {
                                    CastSpell(Form2.config.autoTarget_Target, Form2.config.autoTargetSpell);
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
                                    CastSpell("<t>", Form2.config.autoTargetSpell);
                                    lastKnownEstablisherTarget = enemyID;
                                    await Task.Delay(TimeSpan.FromMilliseconds(1000));

                                    if (Form2.config.DisableTargettingCancel == false)
                                    {
                                        await Task.Delay(TimeSpan.FromSeconds((double)Form2.config.TargetRemoval_Delay));
                                        PL.Target.SetTarget(0);
                                    }
                                }
                            }
                        }

                        // BARD SONGS

                        else if (Form2.config.enableSinging && !PL.HasStatus(StatusEffect.Silence) && (PL.Player.Status == 1 || PL.Player.Status == 0))
                        {
                            Run_BardSongs();

                        }


                        // so PL job abilities are in order
                        if (PL.Player.Status == 1 || PL.Player.Status == 0)
                        {
                            if (Form2.config.AfflatusSolace && (!PL.HasStatus(StatusEffect.Afflatus_Solace)) && PL.AbilityAvailable(Ability.AfflatusSolace))
                            {
                                JobAbility_Wait(Ability.AfflatusSolace, Ability.AfflatusSolace);
                            }
                            else if (Form2.config.AfflatusMisery && (!PL.HasStatus(StatusEffect.Afflatus_Misery)) && PL.AbilityAvailable(Ability.AfflatusMisery))
                            {
                                JobAbility_Wait(Ability.AfflatusMisery, Ability.AfflatusMisery);
                            }
                            else if (Form2.config.Composure && (!PL.HasStatus(StatusEffect.Composure)) && PL.AbilityAvailable(Ability.Composure))
                            {
                                JobAbility_Wait("Composure #2", Ability.Composure);
                            }
                            else if (Form2.config.LightArts && (!PL.HasStatus(StatusEffect.Light_Arts)) && (!PL.HasStatus(StatusEffect.Addendum_White)) && PL.AbilityAvailable(Ability.LightArts))
                            {
                                JobAbility_Wait("Light Arts #2", Ability.LightArts);
                            }
                            else if (Form2.config.AddendumWhite && (!PL.HasStatus(StatusEffect.Addendum_White)) && PL.CurrentSCHCharges() > 0)
                            {
                                JobAbility_Wait(Ability.AddendumWhite, Ability.AddendumWhite);
                            }
                            else if (Form2.config.Sublimation && (!PL.HasStatus(StatusEffect.Sublimation_Activated)) && (!PL.HasStatus(StatusEffect.Sublimation_Complete)) && (!PL.HasStatus(StatusEffect.Refresh)) && PL.AbilityAvailable(Ability.Sublimation))
                            {
                                JobAbility_Wait("Sublimation, Charging", Ability.Sublimation);
                            }
                            else if (Form2.config.Sublimation && ((PL.Player.MPMax - PL.Player.MP) > Form2.config.sublimationMP) && PL.HasStatus(StatusEffect.Sublimation_Complete) && PL.AbilityAvailable(Ability.Sublimation))
                            {
                                JobAbility_Wait("Sublimation, Recovery", Ability.Sublimation);
                            }
                            else if (Form2.config.DivineCaress && (Form2.config.plDebuffEnabled || Form2.config.monitoredDebuffEnabled || Form2.config.enablePartyDebuffRemoval) && PL.AbilityAvailable(Ability.DivineCaress))
                            {
                                JobAbility_Wait(Ability.DivineCaress, Ability.DivineCaress);
                            }
                            else if (Form2.config.Entrust && !PL.HasStatus((StatusEffect)584) && CheckEngagedStatus() == true && PL.AbilityAvailable(Ability.Entrust))
                            {
                                JobAbility_Wait(Ability.Entrust, Ability.Entrust);
                            }
                            else if (Form2.config.Dematerialize && CheckEngagedStatus() == true && PL.Player.Pet.HealthPercent >= 90  && PL.AbilityAvailable(Ability.Dematerialize))
                            {
                                JobAbility_Wait(Ability.Dematerialize, Ability.Dematerialize);
                            }
                            else if (Form2.config.EclipticAttrition && CheckEngagedStatus() == true && PL.Player.Pet.HealthPercent >= 90  && PL.AbilityAvailable(Ability.EclipticAttrition) && !PL.HasStatus(516) && EclipticStillUp != true)
                            {
                                JobAbility_Wait(Ability.EclipticAttrition, Ability.EclipticAttrition);
                            }
                            else if (Form2.config.LifeCycle && CheckEngagedStatus() == true && PL.Player.Pet.HealthPercent <= 30 && PL.Player.Pet.HealthPercent >= 5 && PL.Player.HPP >= 90  && PL.AbilityAvailable(Ability.LifeCycle))
                            {
                                JobAbility_Wait(Ability.LifeCycle, Ability.LifeCycle);
                            }
                            else if (Form2.config.Devotion && PL.AbilityAvailable(Ability.Devotion) && PL.Player.HPP > 80 && (!Form2.config.DevotionWhenEngaged || (Monitored.Player.Status == 1)))
                            {
                                // First Generate the current party number, this will be used
                                // regardless of the type
                                int memberOF = PLPartyRelativeToMonitored();

                                // Now generate the party
                                IEnumerable<PartyMember> cParty = Monitored.Party.GetPartyMembers().Where(p => p.Active != 0 && p.Zone == PL.Player.ZoneId);

                                // Make sure member number is not 0 (null) or 4 (void)
                                if (memberOF > 0)
                                {
                                    // Run through Each party member as we're looking for either a specifc name or if set otherwise anyone with the MP criteria in the current party.
                                    foreach (PartyMember pData in cParty)
                                    {
                                        // If party of party v1
                                        if (memberOF == 1 && pData.MemberNumber >= 0 && pData.MemberNumber <= 5)
                                        {
                                            if (!string.IsNullOrEmpty(pData.Name) && pData.Name != PL.Player.Name)
                                            {
                                                if (Form2.config.DevotionTargetType == 0)
                                                {
                                                    if (pData.Name == Form2.config.DevotionTargetName)
                                                    {
                                                        XiEntity playerInfo = PL.Entity.GetEntity((int)pData.TargetIndex);
                                                        if (playerInfo.Distance < 10 && playerInfo.Distance > 0 && pData.CurrentMP <= Form2.config.DevotionMP && pData.CurrentMPP <= 30)
                                                        {
                                                            PL.ThirdParty.SendString("/ja \"Devotion\" " + Form2.config.DevotionTargetName);
                                                            Thread.Sleep(TimeSpan.FromSeconds(2));
                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    XiEntity playerInfo = PL.Entity.GetEntity((int)pData.TargetIndex);

                                                    if ((pData.CurrentMP <= Form2.config.DevotionMP) && (playerInfo.Distance < 10) && pData.CurrentMPP <= 30)
                                                    {
                                                        PL.ThirdParty.SendString("/ja \"Devotion\" " + pData.Name);
                                                        Thread.Sleep(TimeSpan.FromSeconds(2));
                                                        break;
                                                    }
                                                }
                                            }
                                        } // If part of party 2
                                        else if (memberOF == 2 && pData.MemberNumber >= 6 && pData.MemberNumber <= 11)
                                        {
                                            if (!string.IsNullOrEmpty(pData.Name) && pData.Name != PL.Player.Name)
                                            {
                                                if (Form2.config.DevotionTargetType == 0)
                                                {
                                                    if (pData.Name == Form2.config.DevotionTargetName)
                                                    {
                                                        XiEntity playerInfo = PL.Entity.GetEntity((int)pData.TargetIndex);
                                                        if (playerInfo.Distance < 10 && playerInfo.Distance > 0 && pData.CurrentMP <= Form2.config.DevotionMP)
                                                        {
                                                            PL.ThirdParty.SendString("/ja \"Devotion\" " + Form2.config.DevotionTargetName);
                                                            Thread.Sleep(TimeSpan.FromSeconds(2));
                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    XiEntity playerInfo = PL.Entity.GetEntity((int)pData.TargetIndex);

                                                    if ((pData.CurrentMP <= Form2.config.DevotionMP) && (playerInfo.Distance < 10) && pData.CurrentMPP <= 50)
                                                    {
                                                        PL.ThirdParty.SendString("/ja \"Devotion\" " + pData.Name);
                                                        Thread.Sleep(TimeSpan.FromSeconds(2));
                                                        break;
                                                    }
                                                }
                                            }
                                        } // If part of party 3
                                        else if (memberOF == 3 && pData.MemberNumber >= 12 && pData.MemberNumber <= 17)
                                        {
                                            if (!string.IsNullOrEmpty(pData.Name) && pData.Name != PL.Player.Name)
                                            {
                                                if (Form2.config.DevotionTargetType == 0)
                                                {
                                                    if (pData.Name == Form2.config.DevotionTargetName)
                                                    {
                                                        XiEntity playerInfo = PL.Entity.GetEntity((int)pData.TargetIndex);
                                                        if (playerInfo.Distance < 10 && playerInfo.Distance > 0 && pData.CurrentMP <= Form2.config.DevotionMP)
                                                        {
                                                            PL.ThirdParty.SendString("/ja \"Devotion\" " + Form2.config.DevotionTargetName);
                                                            Thread.Sleep(TimeSpan.FromSeconds(2));
                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    XiEntity playerInfo = PL.Entity.GetEntity((int)pData.TargetIndex);

                                                    if ((pData.CurrentMP <= Form2.config.DevotionMP) && (playerInfo.Distance < 10) && pData.CurrentMPP <= 50)
                                                    {
                                                        PL.ThirdParty.SendString("/ja \"Devotion\" " + pData.Name);
                                                        Thread.Sleep(TimeSpan.FromSeconds(2));
                                                        break;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        var playerBuffOrder = Monitored.Party.GetPartyMembers().OrderBy(p => p.MemberNumber).OrderBy(p => p.Active == 0).Where(p => p.Active == 1);

                        // Auto Casting
                        foreach (var charDATA in playerBuffOrder)
                        {
                            // Grab the Storm string name to perform checks.
                            string StormSpell_Enabled = CheckStormspell(charDATA.MemberNumber);

                            // PL BASED BUFFS
                            if (PL.Player.Name == charDATA.Name)
                            {

                                if (autoHasteEnabled[charDATA.MemberNumber] && PL.SpellAvailable(Spells.Haste) && PL.Player.MP > Form2.config.mpMinCastValue && PL.CanCastOn(charDATA) && !PL.HasStatus(StatusEffect.Haste) && !PL.HasStatus(StatusEffect.Slow))
                                {
                                    hastePlayer(charDATA.MemberNumber);
                                }
                                if (autoHaste_IIEnabled[charDATA.MemberNumber] && PL.SpellAvailable(Spells.Haste_II) && PL.Player.MP > Form2.config.mpMinCastValue && PL.CanCastOn(charDATA) && !PL.HasStatus(StatusEffect.Haste) && !PL.HasStatus(StatusEffect.Slow))
                                {
                                    haste_IIPlayer(charDATA.MemberNumber);
                                }
                                if (autoAdloquium_Enabled[charDATA.MemberNumber] && PL.SpellAvailable(Spells.Adloquium) && PL.Player.MP > Form2.config.mpMinCastValue && PL.CanCastOn(charDATA) && !PL.HasStatus(StatusEffect.Regain))
                                {
                                    AdloquiumPlayer(charDATA.MemberNumber);
                                }
                                if (autoFlurryEnabled[charDATA.MemberNumber] && PL.SpellAvailable(Spells.Flurry) && PL.Player.MP > Form2.config.mpMinCastValue && PL.CanCastOn(charDATA) && !PL.HasStatus(581) && !PL.HasStatus(StatusEffect.Slow))
                                {
                                    FlurryPlayer(charDATA.MemberNumber);
                                }
                                if (autoFlurry_IIEnabled[charDATA.MemberNumber] && PL.SpellAvailable(Spells.Flurry_II) && PL.Player.MP > Form2.config.mpMinCastValue && PL.CanCastOn(charDATA) && !PL.HasStatus(581) && !PL.HasStatus(StatusEffect.Slow))
                                {
                                    Flurry_IIPlayer(charDATA.MemberNumber);
                                }
                                if (autoShell_Enabled[charDATA.MemberNumber] && PL.SpellAvailable(shell_spells[Form2.config.autoShell_Spell]) && PL.Player.MP > Form2.config.mpMinCastValue && PL.CanCastOn(charDATA) && PL.Player.Status != 33 && !PL.HasStatus(StatusEffect.Shell))
                                {
                                    shellPlayer(charDATA.MemberNumber);
                                }
                                if (autoProtect_Enabled[charDATA.MemberNumber] && PL.SpellAvailable(protect_spells[Form2.config.autoProtect_Spell]) && PL.Player.MP > Form2.config.mpMinCastValue && PL.CanCastOn(charDATA) && PL.Player.Status != 33 && !PL.HasStatus(StatusEffect.Protect))
                                {
                                    protectPlayer(charDATA.MemberNumber);
                                }
                                if (autoPhalanx_IIEnabled[charDATA.MemberNumber] && PL.SpellAvailable(Spells.Phalanx_II) && (PL.Player.MP > Form2.config.mpMinCastValue) && PL.CanCastOn(charDATA) && PL.Player.Status != 33 && !PL.HasStatus(StatusEffect.Phalanx))
                                {
                                    Phalanx_IIPlayer(charDATA.MemberNumber);
                                }
                                if (autoRegen_Enabled[charDATA.MemberNumber] && PL.SpellAvailable(Data.RegenTiers[Form2.config.autoRegen_Spell]) && (PL.Player.MP > Form2.config.mpMinCastValue) && PL.CanCastOn(charDATA) && PL.Player.Status != 33 && !PL.HasStatus(StatusEffect.Regen))
                                {
                                    Regen_Player(charDATA.MemberNumber);
                                }
                                if (autoRefreshEnabled[charDATA.MemberNumber] && PL.SpellAvailable(Data.RefreshTiers[Form2.config.autoRefresh_Spell]) && (PL.Player.MP > Form2.config.mpMinCastValue) && PL.CanCastOn(charDATA) && PL.Player.Status != 33 && !PL.HasStatus(StatusEffect.Refresh))
                                {
                                    Refresh_Player(charDATA.MemberNumber);
                                }
                                if (CheckIfAutoStormspellEnabled(charDATA.MemberNumber) && (PL.Player.MP > Form2.config.mpMinCastValue) && PL.CanCastOn(charDATA) && PL.Player.Status != 33 && !PL.HasStatus(Data.SpellEffects[StormSpell_Enabled]) && PL.SpellAvailable(StormSpell_Enabled))
                                {
                                    StormSpellPlayer(charDATA.MemberNumber, StormSpell_Enabled);
                                }
                            }
                            // MONITORED PLAYER BASED BUFFS
                            else if (Monitored.Player.Name == charDATA.Name)
                            {
                                if (autoHasteEnabled[charDATA.MemberNumber] && PL.SpellAvailable(Spells.Haste) && PL.Player.MP > Form2.config.mpMinCastValue && PL.CanCastOn(charDATA) && !monitoredStatusCheck(StatusEffect.Haste) && !monitoredStatusCheck(StatusEffect.Slow))
                                {
                                    hastePlayer(charDATA.MemberNumber);
                                }
                                if (autoHaste_IIEnabled[charDATA.MemberNumber] && PL.SpellAvailable(Spells.Haste_II) && PL.Player.MP > Form2.config.mpMinCastValue && PL.CanCastOn(charDATA) && !monitoredStatusCheck(StatusEffect.Haste) && !monitoredStatusCheck(StatusEffect.Slow))
                                {
                                    haste_IIPlayer(charDATA.MemberNumber);
                                }
                                if (autoAdloquium_Enabled[charDATA.MemberNumber] && PL.SpellAvailable(Spells.Adloquium) && PL.Player.MP > Form2.config.mpMinCastValue && PL.CanCastOn(charDATA) && !Monitored.HasStatus(StatusEffect.Regain))
                                {
                                    AdloquiumPlayer(charDATA.MemberNumber);
                                }
                                if (autoFlurryEnabled[charDATA.MemberNumber] && PL.SpellAvailable(Spells.Flurry) && PL.Player.MP > Form2.config.mpMinCastValue && PL.CanCastOn(charDATA) && !Monitored.HasStatus(581) && !monitoredStatusCheck(StatusEffect.Slow))
                                {
                                    FlurryPlayer(charDATA.MemberNumber);
                                }
                                if (autoFlurry_IIEnabled[charDATA.MemberNumber] && PL.SpellAvailable(Spells.Flurry_II) && PL.Player.MP > Form2.config.mpMinCastValue && PL.CanCastOn(charDATA) && !Monitored.HasStatus(581) && !monitoredStatusCheck(StatusEffect.Slow))
                                {
                                    Flurry_IIPlayer(charDATA.MemberNumber);
                                }
                                if (autoShell_Enabled[charDATA.MemberNumber] && PL.SpellAvailable(shell_spells[Form2.config.autoShell_Spell]) && PL.Player.MP > Form2.config.mpMinCastValue && PL.CanCastOn(charDATA) && PL.Player.Status != 33 && !monitoredStatusCheck(StatusEffect.Shell))
                                {
                                    shellPlayer(charDATA.MemberNumber);
                                }
                                if (autoProtect_Enabled[charDATA.MemberNumber] && PL.SpellAvailable(protect_spells[Form2.config.autoProtect_Spell]) && PL.Player.MP > Form2.config.mpMinCastValue && PL.CanCastOn(charDATA) && PL.Player.Status != 33 && !monitoredStatusCheck(StatusEffect.Protect))
                                {
                                    protectPlayer(charDATA.MemberNumber);
                                }
                                if (autoPhalanx_IIEnabled[charDATA.MemberNumber] && PL.SpellAvailable(Spells.Phalanx_II) && (PL.Player.MP > Form2.config.mpMinCastValue) && PL.CanCastOn(charDATA) && PL.Player.Status != 33 && !monitoredStatusCheck(StatusEffect.Phalanx))
                                {
                                    Phalanx_IIPlayer(charDATA.MemberNumber);
                                }
                                if (autoRegen_Enabled[charDATA.MemberNumber] && PL.SpellAvailable(Data.RegenTiers[Form2.config.autoRegen_Spell]) && (PL.Player.MP > Form2.config.mpMinCastValue) && PL.CanCastOn(charDATA) && PL.Player.Status != 33 && !monitoredStatusCheck(StatusEffect.Regen))
                                {
                                    Regen_Player(charDATA.MemberNumber);
                                }
                                if (autoRefreshEnabled[charDATA.MemberNumber] && PL.SpellAvailable(Data.RefreshTiers[Form2.config.autoRefresh_Spell]) && (PL.Player.MP > Form2.config.mpMinCastValue) && PL.CanCastOn(charDATA) && PL.Player.Status != 33 && !monitoredStatusCheck(StatusEffect.Refresh))
                                {
                                    Refresh_Player(charDATA.MemberNumber);
                                }
                                if (CheckIfAutoStormspellEnabled(charDATA.MemberNumber) && (PL.Player.MP > Form2.config.mpMinCastValue) && PL.CanCastOn(charDATA) && PL.Player.Status != 33 && !Monitored.HasStatus(Data.SpellEffects[StormSpell_Enabled]) && PL.SpellAvailable(StormSpell_Enabled))
                                {
                                    StormSpellPlayer(charDATA.MemberNumber, StormSpell_Enabled);
                                }
                            }
                            else
                            {
                                if (autoHasteEnabled[charDATA.MemberNumber] && PL.SpellAvailable(Spells.Haste) && PL.Player.MP > Form2.config.mpMinCastValue && PL.CanCastOn(charDATA) && playerHasteSpan[charDATA.MemberNumber].Minutes >= Form2.config.autoHasteMinutes)
                                {
                                    hastePlayer(charDATA.MemberNumber);
                                }
                                if (autoHaste_IIEnabled[charDATA.MemberNumber] && PL.SpellAvailable(Spells.Haste_II) && PL.Player.MP > Form2.config.mpMinCastValue && PL.CanCastOn(charDATA) && playerHaste_IISpan[charDATA.MemberNumber].Minutes >= Form2.config.autoHasteMinutes)
                                {
                                    haste_IIPlayer(charDATA.MemberNumber);
                                }
                                if (autoAdloquium_Enabled[charDATA.MemberNumber] && PL.SpellAvailable(Spells.Adloquium) && PL.Player.MP > Form2.config.mpMinCastValue && PL.CanCastOn(charDATA) && playerAdloquium_Span[charDATA.MemberNumber].Minutes >= Form2.config.autoAdloquiumMinutes)
                                {
                                    AdloquiumPlayer(charDATA.MemberNumber);
                                }
                                if (autoFlurryEnabled[charDATA.MemberNumber] && PL.SpellAvailable(Spells.Flurry) && PL.Player.MP > Form2.config.mpMinCastValue && PL.CanCastOn(charDATA) && playerFlurrySpan[charDATA.MemberNumber].Minutes >= Form2.config.autoHasteMinutes)
                                {
                                    FlurryPlayer(charDATA.MemberNumber);
                                }
                                if (autoFlurry_IIEnabled[charDATA.MemberNumber] && PL.SpellAvailable(Spells.Flurry_II) && PL.Player.MP > Form2.config.mpMinCastValue && PL.CanCastOn(charDATA) && playerHasteSpan[charDATA.MemberNumber].Minutes >= Form2.config.autoHasteMinutes)
                                {
                                    Flurry_IIPlayer(charDATA.MemberNumber);
                                }
                                if (autoShell_Enabled[charDATA.MemberNumber] && PL.SpellAvailable(shell_spells[Form2.config.autoShell_Spell]) && PL.Player.MP > Form2.config.mpMinCastValue && PL.CanCastOn(charDATA) && PL.Player.Status != 33 && playerShell_Span[charDATA.MemberNumber].Minutes >= Form2.config.autoShellMinutes)
                                {
                                    shellPlayer(charDATA.MemberNumber);
                                }
                                if (autoProtect_Enabled[charDATA.MemberNumber] && PL.SpellAvailable(protect_spells[Form2.config.autoProtect_Spell]) && PL.Player.MP > Form2.config.mpMinCastValue && PL.CanCastOn(charDATA) && PL.Player.Status != 33 && playerProtect_Span[charDATA.MemberNumber].Minutes >= Form2.config.autoProtect_Minutes)
                                {
                                    protectPlayer(charDATA.MemberNumber);
                                }
                                if (autoPhalanx_IIEnabled[charDATA.MemberNumber] && PL.SpellAvailable(Spells.Phalanx_II) && PL.CanCastOn(charDATA) && PL.Player.Status != 33 && playerPhalanx_IISpan[charDATA.MemberNumber].Minutes >= Form2.config.autoPhalanxIIMinutes)
                                {
                                    Phalanx_IIPlayer(charDATA.MemberNumber);
                                }
                                if (autoRegen_Enabled[charDATA.MemberNumber] && PL.SpellAvailable(Data.RegenTiers[Form2.config.autoRegen_Spell]) && (PL.Player.MP > Form2.config.mpMinCastValue) && PL.CanCastOn(charDATA) && PL.Player.Status != 33 && playerRegen_Span[charDATA.MemberNumber].Minutes >= Form2.config.autoRegen_Minutes)
                                {
                                    Regen_Player(charDATA.MemberNumber);
                                }
                                if (autoRefreshEnabled[charDATA.MemberNumber] && PL.SpellAvailable(Data.RefreshTiers[Form2.config.autoRefresh_Spell]) && (PL.Player.MP > Form2.config.mpMinCastValue) && PL.CanCastOn(charDATA) && PL.Player.Status != 33 && playerRefresh_Span[charDATA.MemberNumber].Minutes >= Form2.config.autoRefresh_Minutes)
                                {
                                    Refresh_Player(charDATA.MemberNumber);
                                }
                                if (CheckIfAutoStormspellEnabled(charDATA.MemberNumber) && (PL.Player.MP > Form2.config.mpMinCastValue) && PL.CanCastOn(charDATA) && PL.Player.Status != 33 && PL.SpellAvailable(StormSpell_Enabled) && playerStormspellSpan[charDATA.MemberNumber].Minutes >= Form2.config.autoStormspellMinutes)
                                {
                                    StormSpellPlayer(charDATA.MemberNumber, StormSpell_Enabled);
                                }
                            }
                        }
                    }
                }
            }
        }


        private bool CheckIfAutoStormspellEnabled(byte id)
        {

            if (Form2.config.autoStorm_Spell == 0)
            {
                if (autoSandstormEnabled[id])
                {
                    return true;
                }
                else if (autoWindstormEnabled[id])
                {
                    return true;
                }
                else if (autoFirestormEnabled[id])
                {
                    return true;
                }
                else if (autoRainstormEnabled[id])
                {
                    return true;
                }
                else if (autoHailstormEnabled[id])
                {
                    return true;
                }
                else if (autoThunderstormEnabled[id])
                {
                    return true;
                }
                else if (autoVoidstormEnabled[id])
                {
                    return true;
                }
                else if (autoAurorastormEnabled[id])
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else if (Form2.config.autoStorm_Spell == 1)
            {
                if (autoSandstormEnabled[id])
                {
                    return true;
                }
                else if (autoWindstormEnabled[id])
                {
                    return true;
                }
                else if (autoFirestormEnabled[id])
                {
                    return true;
                }
                else if (autoRainstormEnabled[id])
                {
                    return true;
                }
                else if (autoHailstormEnabled[id])
                {
                    return true;
                }
                else if (autoThunderstormEnabled[id])
                {
                    return true;
                }

                else if (autoVoidstormEnabled[id])
                {
                    return true;
                }
                else if (autoAurorastormEnabled[id])
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        private string CheckStormspell(byte id)
        {
            if (Form2.config.autoStorm_Spell == 0)
            {
                if (autoSandstormEnabled[id])
                {
                    return "Sandstorm";
                }
                else if (autoWindstormEnabled[id])
                {
                    return "Windstorm";
                }
                else if (autoFirestormEnabled[id])
                {
                    return "Firestorm";
                }
                else if (autoRainstormEnabled[id])
                {
                    return "Rainstorm";
                }
                else if (autoHailstormEnabled[id])
                {
                    return "Hailstorm";
                }
                else if (autoThunderstormEnabled[id])
                {
                    return "Thunderstorm";
                }
                else if (autoVoidstormEnabled[id])
                {
                    return "Voidstorm";
                }
                else if (autoAurorastormEnabled[id])
                {
                    return "Aurorastorm";
                }
                else
                {
                    return string.Empty;
                }
            }
            else if (Form2.config.autoStorm_Spell == 1)
            {
                if (autoSandstormEnabled[id])
                {
                    return "Sandstorm II";
                }
                else if (autoWindstormEnabled[id])
                {
                    return "Windstorm II";
                }
                else if (autoFirestormEnabled[id])
                {
                    return "Firestorm II";
                }
                else if (autoRainstormEnabled[id])
                {
                    return "Rainstorm II";
                }
                else if (autoHailstormEnabled[id])
                {
                    return "Hailstorm II";
                }
                else if (autoThunderstormEnabled[id])
                {
                    return "Thunderstorm II";
                }

                else if (autoVoidstormEnabled[id])
                {
                    return "Voidstorm II";
                }
                else if (autoAurorastormEnabled[id])
                {
                    return "Aurorastorm II";
                }
                else
                {
                    return string.Empty;
                }
            }
            else
            {
                return string.Empty;
            }
        }       

        private string ReturnGeoSpell(int GEOSpell_ID, int GeoSpell_Type)
        {
            // GRAB THE SPELL FROM THE CUSTOM LIST
            GeoData GeoSpell = GeomancerInfo.Where(c => c.geo_position == GEOSpell_ID).FirstOrDefault();

            if (GeoSpell_Type == 1)
            {
                var apiSpell = PL.Resources.GetSpell(GeoSpell.indi_spell, 0);
                if (PL.SpellAvailable(apiSpell.Name[0]))
                {
                    return GeoSpell.indi_spell;
                }
                else
                {
                    return "SpellNA";
                }
            }
            else if (GeoSpell_Type == 2)
            {
                var apiSpell = PL.Resources.GetSpell(GeoSpell.geo_spell, 0);

                if (PL.SpellAvailable(apiSpell.Name[0]))
                {
                    return GeoSpell.geo_spell;
                }
                else
                {
                    return "SpellNA";
                }
            }
            else
            {
                return "SpellError_Cancel";
            }
        }

        private void settingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form2 settings = new Form2();
            settings.Show();
        }

        private void player0optionsButton_Click(object sender, EventArgs e)
        {
            playerOptionsSelected = 0;
            autoHasteToolStripMenuItem.Checked = autoHasteEnabled[0];
            autoHasteIIToolStripMenuItem.Checked = autoHaste_IIEnabled[0];
            autoAdloquiumToolStripMenuItem.Checked = autoAdloquium_Enabled[0];
            autoFlurryToolStripMenuItem.Checked = autoFlurryEnabled[0];
            autoFlurryIIToolStripMenuItem.Checked = autoFlurry_IIEnabled[0];
            autoProtectToolStripMenuItem.Checked = autoProtect_Enabled[0];
            autoShellToolStripMenuItem.Checked = autoShell_Enabled[0];

            playerOptions.Show(party0, new Point(0, 0));
        }

        private void player1optionsButton_Click(object sender, EventArgs e)
        {
            playerOptionsSelected = 1;
            autoHasteToolStripMenuItem.Checked = autoHasteEnabled[1];
            autoHasteIIToolStripMenuItem.Checked = autoHaste_IIEnabled[1];
            autoAdloquiumToolStripMenuItem.Checked = autoAdloquium_Enabled[1];
            autoFlurryToolStripMenuItem.Checked = autoFlurryEnabled[1];
            autoFlurryIIToolStripMenuItem.Checked = autoFlurry_IIEnabled[1];
            autoProtectToolStripMenuItem.Checked = autoProtect_Enabled[1];
            autoShellToolStripMenuItem.Checked = autoShell_Enabled[1];
            playerOptions.Show(party0, new Point(0, 0));
        }

        private void player2optionsButton_Click(object sender, EventArgs e)
        {
            playerOptionsSelected = 2;
            autoHasteToolStripMenuItem.Checked = autoHasteEnabled[2];
            autoHasteIIToolStripMenuItem.Checked = autoHaste_IIEnabled[2];
            autoAdloquiumToolStripMenuItem.Checked = autoAdloquium_Enabled[2];
            autoFlurryToolStripMenuItem.Checked = autoFlurryEnabled[2];
            autoFlurryIIToolStripMenuItem.Checked = autoFlurry_IIEnabled[2];
            autoProtectToolStripMenuItem.Checked = autoProtect_Enabled[2];
            autoShellToolStripMenuItem.Checked = autoShell_Enabled[2];
            playerOptions.Show(party0, new Point(0, 0));
        }

        private void player3optionsButton_Click(object sender, EventArgs e)
        {
            playerOptionsSelected = 3;
            autoHasteToolStripMenuItem.Checked = autoHasteEnabled[3];
            autoHasteIIToolStripMenuItem.Checked = autoHaste_IIEnabled[3];
            autoAdloquiumToolStripMenuItem.Checked = autoAdloquium_Enabled[3];
            autoFlurryToolStripMenuItem.Checked = autoFlurryEnabled[3];
            autoFlurryIIToolStripMenuItem.Checked = autoFlurry_IIEnabled[3];
            autoProtectToolStripMenuItem.Checked = autoProtect_Enabled[3];
            autoShellToolStripMenuItem.Checked = autoShell_Enabled[3];
            playerOptions.Show(party0, new Point(0, 0));
        }

        private void player4optionsButton_Click(object sender, EventArgs e)
        {
            playerOptionsSelected = 4;
            autoHasteToolStripMenuItem.Checked = autoHasteEnabled[4];
            autoHasteIIToolStripMenuItem.Checked = autoHaste_IIEnabled[4];
            autoAdloquiumToolStripMenuItem.Checked = autoAdloquium_Enabled[4];
            autoFlurryToolStripMenuItem.Checked = autoFlurryEnabled[4];
            autoFlurryIIToolStripMenuItem.Checked = autoFlurry_IIEnabled[4];
            autoProtectToolStripMenuItem.Checked = autoProtect_Enabled[4];
            autoShellToolStripMenuItem.Checked = autoShell_Enabled[4];
            playerOptions.Show(party0, new Point(0, 0));
        }

        private void player5optionsButton_Click(object sender, EventArgs e)
        {
            playerOptionsSelected = 5;
            autoHasteToolStripMenuItem.Checked = autoHasteEnabled[5];
            autoHasteIIToolStripMenuItem.Checked = autoHaste_IIEnabled[5];
            autoAdloquiumToolStripMenuItem.Checked = autoAdloquium_Enabled[5];
            autoFlurryToolStripMenuItem.Checked = autoFlurryEnabled[5];
            autoFlurryIIToolStripMenuItem.Checked = autoFlurry_IIEnabled[5];
            autoProtectToolStripMenuItem.Checked = autoProtect_Enabled[5];
            autoShellToolStripMenuItem.Checked = autoShell_Enabled[5];
            playerOptions.Show(party0, new Point(0, 0));
        }

        private void player6optionsButton_Click(object sender, EventArgs e)
        {
            playerOptionsSelected = 6;
            autoHasteToolStripMenuItem.Checked = autoHasteEnabled[6];
            autoHasteIIToolStripMenuItem.Checked = autoHaste_IIEnabled[6];
            autoFlurryToolStripMenuItem.Checked = autoFlurryEnabled[6];
            autoFlurryIIToolStripMenuItem.Checked = autoFlurry_IIEnabled[6];
            autoProtectToolStripMenuItem.Checked = autoProtect_Enabled[6];
            autoShellToolStripMenuItem.Checked = autoShell_Enabled[6];
            playerOptions.Show(party1, new Point(0, 0));
        }

        private void player7optionsButton_Click(object sender, EventArgs e)
        {
            playerOptionsSelected = 7;
            autoHasteToolStripMenuItem.Checked = autoHasteEnabled[7];
            autoHasteIIToolStripMenuItem.Checked = autoHaste_IIEnabled[7];
            autoFlurryToolStripMenuItem.Checked = autoFlurryEnabled[7];
            autoFlurryIIToolStripMenuItem.Checked = autoFlurry_IIEnabled[7];
            autoProtectToolStripMenuItem.Checked = autoProtect_Enabled[7];
            autoShellToolStripMenuItem.Checked = autoShell_Enabled[7];
            playerOptions.Show(party1, new Point(0, 0));
        }

        private void player8optionsButton_Click(object sender, EventArgs e)
        {
            playerOptionsSelected = 8;
            autoHasteToolStripMenuItem.Checked = autoHasteEnabled[8];
            autoHasteIIToolStripMenuItem.Checked = autoHaste_IIEnabled[8];
            autoFlurryToolStripMenuItem.Checked = autoFlurryEnabled[8];
            autoFlurryIIToolStripMenuItem.Checked = autoFlurry_IIEnabled[8];
            autoProtectToolStripMenuItem.Checked = autoProtect_Enabled[8];
            autoShellToolStripMenuItem.Checked = autoShell_Enabled[8];
            playerOptions.Show(party1, new Point(0, 0));
        }

        private void player9optionsButton_Click(object sender, EventArgs e)
        {
            playerOptionsSelected = 9;
            autoHasteToolStripMenuItem.Checked = autoHasteEnabled[9];
            autoHasteIIToolStripMenuItem.Checked = autoHaste_IIEnabled[9];
            autoFlurryToolStripMenuItem.Checked = autoFlurryEnabled[9];
            autoFlurryIIToolStripMenuItem.Checked = autoFlurry_IIEnabled[9];
            autoProtectToolStripMenuItem.Checked = autoProtect_Enabled[9];
            autoShellToolStripMenuItem.Checked = autoShell_Enabled[9];
            playerOptions.Show(party1, new Point(0, 0));
        }

        private void player10optionsButton_Click(object sender, EventArgs e)
        {
            playerOptionsSelected = 10;
            autoHasteToolStripMenuItem.Checked = autoHasteEnabled[10];
            autoHasteIIToolStripMenuItem.Checked = autoHaste_IIEnabled[10];
            autoFlurryToolStripMenuItem.Checked = autoFlurryEnabled[10];
            autoFlurryIIToolStripMenuItem.Checked = autoFlurry_IIEnabled[10];
            autoProtectToolStripMenuItem.Checked = autoProtect_Enabled[10];
            autoShellToolStripMenuItem.Checked = autoShell_Enabled[10];
            playerOptions.Show(party1, new Point(0, 0));
        }

        private void player11optionsButton_Click(object sender, EventArgs e)
        {
            playerOptionsSelected = 11;
            autoHasteToolStripMenuItem.Checked = autoHasteEnabled[11];
            autoHasteIIToolStripMenuItem.Checked = autoHaste_IIEnabled[11];
            autoFlurryToolStripMenuItem.Checked = autoFlurryEnabled[11];
            autoFlurryIIToolStripMenuItem.Checked = autoFlurry_IIEnabled[11];
            autoProtectToolStripMenuItem.Checked = autoProtect_Enabled[11];
            autoShellToolStripMenuItem.Checked = autoShell_Enabled[11];
            playerOptions.Show(party1, new Point(0, 0));
        }

        private void player12optionsButton_Click(object sender, EventArgs e)
        {
            playerOptionsSelected = 12;
            autoHasteToolStripMenuItem.Checked = autoHasteEnabled[12];
            autoHasteIIToolStripMenuItem.Checked = autoHaste_IIEnabled[12];
            autoFlurryToolStripMenuItem.Checked = autoFlurryEnabled[12];
            autoFlurryIIToolStripMenuItem.Checked = autoFlurry_IIEnabled[12];
            autoProtectToolStripMenuItem.Checked = autoProtect_Enabled[12];
            autoShellToolStripMenuItem.Checked = autoShell_Enabled[12];
            playerOptions.Show(party2, new Point(0, 0));
        }

        private void player13optionsButton_Click(object sender, EventArgs e)
        {
            playerOptionsSelected = 13;
            autoHasteToolStripMenuItem.Checked = autoHasteEnabled[13];
            autoHasteIIToolStripMenuItem.Checked = autoHaste_IIEnabled[13];
            autoFlurryToolStripMenuItem.Checked = autoFlurryEnabled[13];
            autoFlurryIIToolStripMenuItem.Checked = autoFlurry_IIEnabled[13];
            autoProtectToolStripMenuItem.Checked = autoProtect_Enabled[13];
            autoShellToolStripMenuItem.Checked = autoShell_Enabled[13];
            playerOptions.Show(party2, new Point(0, 0));
        }

        private void player14optionsButton_Click(object sender, EventArgs e)
        {
            playerOptionsSelected = 14;
            autoHasteToolStripMenuItem.Checked = autoHasteEnabled[14];
            autoHasteIIToolStripMenuItem.Checked = autoHaste_IIEnabled[14];
            autoFlurryToolStripMenuItem.Checked = autoFlurryEnabled[14];
            autoFlurryIIToolStripMenuItem.Checked = autoFlurry_IIEnabled[14];
            autoProtectToolStripMenuItem.Checked = autoProtect_Enabled[14];
            autoShellToolStripMenuItem.Checked = autoShell_Enabled[14];
            playerOptions.Show(party2, new Point(0, 0));
        }

        private void player15optionsButton_Click(object sender, EventArgs e)
        {
            playerOptionsSelected = 15;
            autoHasteToolStripMenuItem.Checked = autoHasteEnabled[15];
            autoHasteIIToolStripMenuItem.Checked = autoHaste_IIEnabled[15];
            autoFlurryToolStripMenuItem.Checked = autoFlurryEnabled[15];
            autoFlurryIIToolStripMenuItem.Checked = autoFlurry_IIEnabled[15];
            autoProtectToolStripMenuItem.Checked = autoProtect_Enabled[15];
            autoShellToolStripMenuItem.Checked = autoShell_Enabled[15];
            playerOptions.Show(party2, new Point(0, 0));
        }

        private void player16optionsButton_Click(object sender, EventArgs e)
        {
            playerOptionsSelected = 16;
            autoHasteToolStripMenuItem.Checked = autoHasteEnabled[16];
            autoHasteIIToolStripMenuItem.Checked = autoHaste_IIEnabled[16];
            autoFlurryToolStripMenuItem.Checked = autoFlurryEnabled[16];
            autoFlurryIIToolStripMenuItem.Checked = autoFlurry_IIEnabled[16];
            autoProtectToolStripMenuItem.Checked = autoProtect_Enabled[16];
            autoShellToolStripMenuItem.Checked = autoShell_Enabled[16];
            playerOptions.Show(party2, new Point(0, 0));
        }

        private void player17optionsButton_Click(object sender, EventArgs e)
        {
            playerOptionsSelected = 17;
            autoHasteToolStripMenuItem.Checked = autoHasteEnabled[17];
            autoHasteIIToolStripMenuItem.Checked = autoHaste_IIEnabled[17];
            autoFlurryToolStripMenuItem.Checked = autoFlurryEnabled[17];
            autoFlurryIIToolStripMenuItem.Checked = autoFlurry_IIEnabled[17];
            autoProtectToolStripMenuItem.Checked = autoProtect_Enabled[17];
            autoShellToolStripMenuItem.Checked = autoShell_Enabled[17];
            playerOptions.Show(party2, new Point(0, 0));
        }

        private void player0buffsButton_Click(object sender, EventArgs e)
        {
            autoOptionsSelected = 0;
            autoPhalanxIIToolStripMenuItem1.Checked = autoPhalanx_IIEnabled[0];
            autoRegenVToolStripMenuItem.Checked = autoRegen_Enabled[0];
            autoRefreshIIToolStripMenuItem.Checked = autoRefreshEnabled[0];
            SandstormToolStripMenuItem.Checked = autoSandstormEnabled[0];
            RainstormToolStripMenuItem.Checked = autoRainstormEnabled[0];
            WindstormToolStripMenuItem.Checked = autoWindstormEnabled[0];
            FirestormToolStripMenuItem.Checked = autoFirestormEnabled[0];
            HailstormToolStripMenuItem.Checked = autoHailstormEnabled[0];
            ThunderstormToolStripMenuItem.Checked = autoThunderstormEnabled[0];
            VoidstormToolStripMenuItem.Checked = autoVoidstormEnabled[0];
            AurorastormToolStripMenuItem.Checked = autoAurorastormEnabled[0];
            autoOptions.Show(party0, new Point(0, 0));
        }

        private void player1buffsButton_Click(object sender, EventArgs e)
        {
            autoOptionsSelected = 1;
            autoPhalanxIIToolStripMenuItem1.Checked = autoPhalanx_IIEnabled[1];
            autoRegenVToolStripMenuItem.Checked = autoRegen_Enabled[1];
            autoRefreshIIToolStripMenuItem.Checked = autoRefreshEnabled[1];
            SandstormToolStripMenuItem.Checked = autoSandstormEnabled[1];
            RainstormToolStripMenuItem.Checked = autoRainstormEnabled[1];
            WindstormToolStripMenuItem.Checked = autoWindstormEnabled[1];
            FirestormToolStripMenuItem.Checked = autoFirestormEnabled[1];
            HailstormToolStripMenuItem.Checked = autoHailstormEnabled[1];
            ThunderstormToolStripMenuItem.Checked = autoThunderstormEnabled[1];
            VoidstormToolStripMenuItem.Checked = autoVoidstormEnabled[1];
            AurorastormToolStripMenuItem.Checked = autoAurorastormEnabled[1];
            autoOptions.Show(party0, new Point(0, 0));
        }

        private void player2buffsButton_Click(object sender, EventArgs e)
        {
            autoOptionsSelected = 2;
            autoPhalanxIIToolStripMenuItem1.Checked = autoPhalanx_IIEnabled[2];
            autoRegenVToolStripMenuItem.Checked = autoRegen_Enabled[2];
            autoRefreshIIToolStripMenuItem.Checked = autoRefreshEnabled[2];
            SandstormToolStripMenuItem.Checked = autoSandstormEnabled[2];
            RainstormToolStripMenuItem.Checked = autoRainstormEnabled[2];
            WindstormToolStripMenuItem.Checked = autoWindstormEnabled[2];
            FirestormToolStripMenuItem.Checked = autoFirestormEnabled[2];
            HailstormToolStripMenuItem.Checked = autoHailstormEnabled[2];
            ThunderstormToolStripMenuItem.Checked = autoThunderstormEnabled[2];
            VoidstormToolStripMenuItem.Checked = autoVoidstormEnabled[2];
            AurorastormToolStripMenuItem.Checked = autoAurorastormEnabled[2];
            autoOptions.Show(party0, new Point(0, 0));
        }

        private void player3buffsButton_Click(object sender, EventArgs e)
        {
            autoOptionsSelected = 3;
            autoPhalanxIIToolStripMenuItem1.Checked = autoPhalanx_IIEnabled[3];
            autoRegenVToolStripMenuItem.Checked = autoRegen_Enabled[3];
            autoRefreshIIToolStripMenuItem.Checked = autoRefreshEnabled[3];
            SandstormToolStripMenuItem.Checked = autoSandstormEnabled[3];
            RainstormToolStripMenuItem.Checked = autoRainstormEnabled[3];
            WindstormToolStripMenuItem.Checked = autoWindstormEnabled[3];
            FirestormToolStripMenuItem.Checked = autoFirestormEnabled[3];
            HailstormToolStripMenuItem.Checked = autoHailstormEnabled[3];
            ThunderstormToolStripMenuItem.Checked = autoThunderstormEnabled[3];
            VoidstormToolStripMenuItem.Checked = autoVoidstormEnabled[3];
            AurorastormToolStripMenuItem.Checked = autoAurorastormEnabled[3];
            autoOptions.Show(party0, new Point(0, 0));
        }

        private void player4buffsButton_Click(object sender, EventArgs e)
        {
            autoOptionsSelected = 4;
            autoPhalanxIIToolStripMenuItem1.Checked = autoPhalanx_IIEnabled[4];
            autoRegenVToolStripMenuItem.Checked = autoRegen_Enabled[4];
            autoRefreshIIToolStripMenuItem.Checked = autoRefreshEnabled[4];
            SandstormToolStripMenuItem.Checked = autoSandstormEnabled[4];
            RainstormToolStripMenuItem.Checked = autoRainstormEnabled[4];
            WindstormToolStripMenuItem.Checked = autoWindstormEnabled[4];
            FirestormToolStripMenuItem.Checked = autoFirestormEnabled[4];
            HailstormToolStripMenuItem.Checked = autoHailstormEnabled[4];
            ThunderstormToolStripMenuItem.Checked = autoThunderstormEnabled[4];
            VoidstormToolStripMenuItem.Checked = autoVoidstormEnabled[4];
            AurorastormToolStripMenuItem.Checked = autoAurorastormEnabled[4];
            autoOptions.Show(party0, new Point(0, 0));
        }

        private void player5buffsButton_Click(object sender, EventArgs e)
        {
            autoOptionsSelected = 5;
            autoPhalanxIIToolStripMenuItem1.Checked = autoPhalanx_IIEnabled[5];
            autoRegenVToolStripMenuItem.Checked = autoRegen_Enabled[5];
            autoRefreshIIToolStripMenuItem.Checked = autoRefreshEnabled[5];
            SandstormToolStripMenuItem.Checked = autoSandstormEnabled[5];
            RainstormToolStripMenuItem.Checked = autoRainstormEnabled[5];
            WindstormToolStripMenuItem.Checked = autoWindstormEnabled[5];
            FirestormToolStripMenuItem.Checked = autoFirestormEnabled[5];
            HailstormToolStripMenuItem.Checked = autoHailstormEnabled[5];
            ThunderstormToolStripMenuItem.Checked = autoThunderstormEnabled[5];
            VoidstormToolStripMenuItem.Checked = autoVoidstormEnabled[5];
            AurorastormToolStripMenuItem.Checked = autoAurorastormEnabled[5];
            autoOptions.Show(party0, new Point(0, 0));
        }

        private void player6buffsButton_Click(object sender, EventArgs e)
        {
            autoOptionsSelected = 6;
            autoOptions.Show(party1, new Point(0, 0));
        }

        private void player7buffsButton_Click(object sender, EventArgs e)
        {
            autoOptionsSelected = 7;
            autoOptions.Show(party1, new Point(0, 0));
        }

        private void player8buffsButton_Click(object sender, EventArgs e)
        {
            autoOptionsSelected = 8;
            autoOptions.Show(party1, new Point(0, 0));
        }

        private void player9buffsButton_Click(object sender, EventArgs e)
        {
            autoOptionsSelected = 9;
            autoOptions.Show(party1, new Point(0, 0));
        }

        private void player10buffsButton_Click(object sender, EventArgs e)
        {
            autoOptionsSelected = 10;
            autoOptions.Show(party1, new Point(0, 0));
        }

        private void player11buffsButton_Click(object sender, EventArgs e)
        {
            autoOptionsSelected = 11;
            autoOptions.Show(party1, new Point(0, 0));
        }

        private void player12buffsButton_Click(object sender, EventArgs e)
        {
            autoOptionsSelected = 12;
            autoOptions.Show(party2, new Point(0, 0));
        }

        private void player13buffsButton_Click(object sender, EventArgs e)
        {
            autoOptionsSelected = 13;
            autoOptions.Show(party2, new Point(0, 0));
        }

        private void player14buffsButton_Click(object sender, EventArgs e)
        {
            autoOptionsSelected = 14;
            autoOptions.Show(party2, new Point(0, 0));
        }

        private void player15buffsButton_Click(object sender, EventArgs e)
        {
            autoOptionsSelected = 15;
            autoOptions.Show(party2, new Point(0, 0));
        }

        private void player16buffsButton_Click(object sender, EventArgs e)
        {
            autoOptionsSelected = 16;
            autoOptions.Show(party2, new Point(0, 0));
        }

        private void player17buffsButton_Click(object sender, EventArgs e)
        {
            autoOptionsSelected = 17;
            autoOptions.Show(party2, new Point(0, 0));
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
            autoHasteEnabled[playerOptionsSelected] = !autoHasteEnabled[playerOptionsSelected];
            autoHaste_IIEnabled[playerOptionsSelected] = false;
            autoFlurryEnabled[playerOptionsSelected] = false;
            autoFlurry_IIEnabled[playerOptionsSelected] = false;
        }

        private void autoHasteIIToolStripMenuItem_Click(object sender, EventArgs e)
        {
            autoHaste_IIEnabled[playerOptionsSelected] = !autoHaste_IIEnabled[playerOptionsSelected];
            autoHasteEnabled[playerOptionsSelected] = false;
            autoFlurryEnabled[playerOptionsSelected] = false;
            autoFlurry_IIEnabled[playerOptionsSelected] = false;
        }

        private void autoAdloquiumToolStripMenuItem_Click(object sender, EventArgs e)
        {
            autoAdloquium_Enabled[playerOptionsSelected] = !autoAdloquium_Enabled[playerOptionsSelected];
        }

        private void autoFlurryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            autoFlurryEnabled[playerOptionsSelected] = !autoFlurryEnabled[playerOptionsSelected];
            autoHasteEnabled[playerOptionsSelected] = false;
            autoHaste_IIEnabled[playerOptionsSelected] = false;
            autoFlurry_IIEnabled[playerOptionsSelected] = false;
        }

        private void autoFlurryIIToolStripMenuItem_Click(object sender, EventArgs e)
        {
            autoFlurry_IIEnabled[playerOptionsSelected] = !autoFlurry_IIEnabled[playerOptionsSelected];
            autoHasteEnabled[playerOptionsSelected] = false;
            autoFlurryEnabled[playerOptionsSelected] = false;
            autoHaste_IIEnabled[playerOptionsSelected] = false;
        }

        private void autoProtectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            autoProtect_Enabled[playerOptionsSelected] = !autoProtect_Enabled[playerOptionsSelected];
        }

        private void enableDebuffRemovalToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string generated_name = Monitored.Party.GetPartyMembers()[playerOptionsSelected].Name.ToLower();
            characterNames_naRemoval.Add(generated_name);
        }

        private void autoShellToolStripMenuItem_Click(object sender, EventArgs e)
        {
            autoShell_Enabled[playerOptionsSelected] = !autoShell_Enabled[playerOptionsSelected];
        }

        private void autoHasteToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            autoHasteEnabled[autoOptionsSelected] = !autoHasteEnabled[autoOptionsSelected];
            autoHaste_IIEnabled[playerOptionsSelected] = false;
            autoFlurryEnabled[playerOptionsSelected] = false;
            autoFlurry_IIEnabled[playerOptionsSelected] = false;
        }

        private void autoPhalanxIIToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            autoPhalanx_IIEnabled[autoOptionsSelected] = !autoPhalanx_IIEnabled[autoOptionsSelected];
        }

        private void autoRegenVToolStripMenuItem_Click(object sender, EventArgs e)
        {
            autoRegen_Enabled[autoOptionsSelected] = !autoRegen_Enabled[autoOptionsSelected];
        }

        private void autoRefreshIIToolStripMenuItem_Click(object sender, EventArgs e)
        {
            autoRefreshEnabled[autoOptionsSelected] = !autoRefreshEnabled[autoOptionsSelected];
        }

        private void hasteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            hastePlayer(playerOptionsSelected);
        }

        private void followToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form2.config.autoFollowName = Monitored.Party.GetPartyMembers()[playerOptionsSelected].Name;
        }

        private void stopfollowToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form2.config.autoFollowName = string.Empty;
        }

        private void EntrustTargetToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form2.config.EntrustedSpell_Target = Monitored.Party.GetPartyMembers()[playerOptionsSelected].Name;
        }

        private void GeoTargetToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form2.config.LuopanSpell_Target = Monitored.Party.GetPartyMembers()[playerOptionsSelected].Name;
        }

        private void DevotionTargetToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form2.config.DevotionTargetName = Monitored.Party.GetPartyMembers()[playerOptionsSelected].Name;
        }

        private void HateEstablisherToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form2.config.autoTarget_Target = Monitored.Party.GetPartyMembers()[playerOptionsSelected].Name;
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

        private void setAllStormsFalse(byte autoOptionsSelected)
        {
            // MessageBox.Show("SONG DATA: " + activeStorm + " " + autoOptionsSelected);

            autoSandstormEnabled[autoOptionsSelected] = false;
            autoRainstormEnabled[autoOptionsSelected] = false;
            autoFirestormEnabled[autoOptionsSelected] = false;
            autoWindstormEnabled[autoOptionsSelected] = false;
            autoHailstormEnabled[autoOptionsSelected] = false;
            autoThunderstormEnabled[autoOptionsSelected] = false;
            autoVoidstormEnabled[autoOptionsSelected] = false;
            autoAurorastormEnabled[autoOptionsSelected] = false;
        }

        private void SandstormToolStripMenuItem_Click(object sender, EventArgs e)
        {
            bool currentStatus = autoSandstormEnabled[autoOptionsSelected];
            setAllStormsFalse(autoOptionsSelected);
            autoSandstormEnabled[autoOptionsSelected] = !currentStatus;
        }

        private void RainstormToolStripMenuItem_Click(object sender, EventArgs e)
        {
            bool currentStatus = autoRainstormEnabled[autoOptionsSelected];
            setAllStormsFalse(autoOptionsSelected);
            autoRainstormEnabled[autoOptionsSelected] = !currentStatus;
        }

        private void WindstormToolStripMenuItem_Click(object sender, EventArgs e)
        {
            bool currentStatus = autoWindstormEnabled[autoOptionsSelected];
            setAllStormsFalse(autoOptionsSelected);
            autoWindstormEnabled[autoOptionsSelected] = !currentStatus;
        }

        private void FirestormToolStripMenuItem_Click(object sender, EventArgs e)
        {
            bool currentStatus = autoFirestormEnabled[autoOptionsSelected];
            setAllStormsFalse(autoOptionsSelected);
            autoFirestormEnabled[autoOptionsSelected] = !currentStatus;
        }

        private void HailstormToolStripMenuItem_Click(object sender, EventArgs e)
        {
            bool currentStatus = autoHailstormEnabled[autoOptionsSelected];
            setAllStormsFalse(autoOptionsSelected);
            autoHailstormEnabled[autoOptionsSelected] = !currentStatus;
        }

        private void ThunderstormToolStripMenuItem_Click(object sender, EventArgs e)
        {
            bool currentStatus = autoThunderstormEnabled[autoOptionsSelected];
            setAllStormsFalse(autoOptionsSelected);
            autoThunderstormEnabled[autoOptionsSelected] = !currentStatus;
        }

        private void VoidstormToolStripMenuItem_Click(object sender, EventArgs e)
        {
            bool currentStatus = autoVoidstormEnabled[autoOptionsSelected];
            setAllStormsFalse(autoOptionsSelected);
            autoVoidstormEnabled[autoOptionsSelected] = !currentStatus;
        }

        private void AurorastormToolStripMenuItem_Click(object sender, EventArgs e)
        {
            bool currentStatus = autoAurorastormEnabled[autoOptionsSelected];
            setAllStormsFalse(autoOptionsSelected);
            autoAurorastormEnabled[autoOptionsSelected] = !currentStatus;
        }

        private void protectIVToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CastSpell(Monitored.Party.GetPartyMembers()[playerOptionsSelected].Name, "Protect IV");
        }

        private void protectVToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CastSpell(Monitored.Party.GetPartyMembers()[playerOptionsSelected].Name, "Protect V");
        }

        private void shellIVToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CastSpell(Monitored.Party.GetPartyMembers()[playerOptionsSelected].Name, "Shell IV");
        }

        private void shellVToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CastSpell(Monitored.Party.GetPartyMembers()[playerOptionsSelected].Name, "Shell V");
        }

        private void button3_Click(object sender, EventArgs e)
        {


            song_casting = 0;
            ForceSongRecast = true;

            if (pauseActions == false)
            {
                pauseButton.Text = "Paused!";
                pauseButton.ForeColor = Color.Red;
                actionTimer.Enabled = false;
                ActiveBuffs.Clear();
                pauseActions = true;
                if (Form2.config.FFXIDefaultAutoFollow == false)
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

                if (Form2.config.MinimiseonStart == true && WindowState != FormWindowState.Minimized)
                {
                    WindowState = FormWindowState.Minimized;
                }

                if (Form2.config.EnableAddOn && LUA_Plugin_Loaded == 0)
                {
                    if (WindowerMode == "Windower")
                    {
                        PL.ThirdParty.SendString("//lua load CurePlease_addon");
                        Thread.Sleep(1500);
                        PL.ThirdParty.SendString("//cpaddon settings " + Form2.config.ipAddress + " " + Form2.config.listeningPort);
                        Thread.Sleep(100);
                        if (Form2.config.enableHotKeys)
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
                        PL.ThirdParty.SendString("/cpaddon settings " + Form2.config.ipAddress + " " + Form2.config.listeningPort);
                        Thread.Sleep(100);
                        if (Form2.config.enableHotKeys)
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
            Form4 form4 = new Form4(this);
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
                    if (Form2.config.enableHotKeys)
                    {
                        PL.ThirdParty.SendString("/unbind ^!F1");
                        PL.ThirdParty.SendString("/unbind ^!F2");
                        PL.ThirdParty.SendString("/unbind ^!F3");
                    }
                }
                else if (WindowerMode == "Windower")
                {
                    PL.ThirdParty.SendString("//lua unload CurePlease_addon");

                    if (Form2.config.enableHotKeys)
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
            if ((setinstance2.Enabled == true) && !string.IsNullOrEmpty(Form2.config.autoFollowName) && !pauseActions)
            {
                for (int x = 0; x < 2048; x++)
                {
                    XiEntity entity = PL.Entity.GetEntity(x);

                    if (entity.Name != null && entity.Name.ToLower().Equals(Form2.config.autoFollowName.ToLower()))
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

            //// Now generate the party
            //IEnumerable<PartyMember> cParty = Monitored.Party.GetPartyMembers().Where(p => p.Active != 0 && p.Zone == PL.Player.ZoneId);

            //// Make sure member number is not 0 (null) or 4 (void)
            //if (PLParty > 0)
            //{
            //    // Run through Each party member as we're looking for either a specific name or if set
            //    // otherwise anyone with the MP criteria in the current party.
            //    foreach (PartyMember pData in cParty)
            //    {
            //        if (PLParty == 1 && pData.MemberNumber >= 0 && pData.MemberNumber <= 5 && pData.Name == Monitored.Player.Name)
            //        {
            //            return true;
            //        }
            //        else if (PLParty == 2 && pData.MemberNumber >= 6 && pData.MemberNumber <= 11 && pData.Name == Monitored.Player.Name)
            //        {
            //            return true;
            //        }
            //        else if (PLParty == 3 && pData.MemberNumber >= 12 && pData.MemberNumber <= 17 && pData.Name == Monitored.Player.Name)
            //        {
            //            return true;
            //        }
            //    }
            //}

            //return false;
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

        private void resetSongTimer_Tick(object sender, EventArgs e)
        {
            song_casting = 0;
        }      

        private bool CheckEngagedStatus()
        {
            if (Monitored == null || PL == null) { return false; }


            if (Form2.config.GeoWhenEngaged == false)
            {
                return true;
            }
            else if (Form2.config.specifiedEngageTarget == true && !string.IsNullOrEmpty(Form2.config.LuopanSpell_Target))
            {
                for (int x = 0; x < 2048; x++)
                {
                    XiEntity z = PL.Entity.GetEntity(x);
                    if (!string.IsNullOrEmpty(z.Name))
                    {
                        if (z.Name.ToLower() == Form2.config.LuopanSpell_Target.ToLower()) // A match was located so use this entity as a check.
                        {
                            if (z.Status == 1)
                            {
                                return true;
                            }
                            else
                            {
                                return false;
                            }
                        }
                    }
                }
                return false;
            }
            else
            {
                if (Monitored.Player.Status == 1)
                {
                    return true;


                }
                else
                {
                    return false;
                }
            }
        }

        private void EclipticTimer_Tick(object sender, EventArgs e)
        {
            if (Monitored == null || PL == null) { return; }

            if (PL.Player.Pet.HealthPercent >= 1)
            {
                EclipticStillUp = true;
            }
            else
            {
                EclipticStillUp = false;
            }
        }

        private bool GEO_EnemyCheck()
        {
            if (Monitored == null || PL == null) { return false; }

            // Grab GEO spell name
            string SpellCheckedResult = ReturnGeoSpell(Form2.config.GeoSpell_Spell, 2);

            if (SpellCheckedResult == "SpellError_Cancel" || SpellCheckedResult == "SpellRecast" || SpellCheckedResult == "SpellUnknown")
            {
                // Do nothing and continue on with the program
                return true;
            }
            else
            {
                if (PL.Resources.GetSpell(SpellCheckedResult, 0).ValidTargets == 5)
                {
                    return true; // SPELL TARGET IS PLAYER THEREFORE ONLY THE DEFAULT CHECK IS REQUIRED SO JUST RETURN TRUE TO VOID THIS CHECK
                }
                else
                {
                    if (Form2.config.specifiedEngageTarget == true && !string.IsNullOrEmpty(Form2.config.LuopanSpell_Target))
                    {
                        for (int x = 0; x < 2048; x++)
                        {
                            XiEntity z = PL.Entity.GetEntity(x);
                            if (!string.IsNullOrEmpty(z.Name))
                            {
                                if (z.Name.ToLower() == Form2.config.LuopanSpell_Target.ToLower()) // A match was located so use this entity as a check.
                                {
                                    if (z.Status == 1)
                                    {
                                        return true;
                                    }
                                    else
                                    {
                                        return false;
                                    }
                                }
                            }
                        }
                        return false;
                    }
                    else
                    {
                        if (Monitored.Player.Status == 1)
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                }
            }
        }

        private int CheckEngagedStatus_Hate()
        {
            if (Form2.config.AssistSpecifiedTarget == true && !string.IsNullOrEmpty(Form2.config.autoTarget_Target))
            {
                IDFound = 0;

                for (int x = 0; x < 2048; x++)
                {
                    XiEntity z = PL.Entity.GetEntity(x);

                    if (!string.IsNullOrEmpty(z.Name) && z.Name.ToLower() == Form2.config.autoTarget_Target.ToLower())
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

        private int GrabGEOTargetID()
        {
            if (Form2.config.specifiedEngageTarget == true && !string.IsNullOrEmpty(Form2.config.LuopanSpell_Target))
            {
                IDFound = 0;

                for (int x = 0; x < 2048; x++)
                {
                    XiEntity z = PL.Entity.GetEntity(x);

                    if (z.Name != null && z.Name.ToLower() == Form2.config.LuopanSpell_Target.ToLower())
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

        private int GrabDistance_GEO()
        {
            string checkedName = string.Empty;
            string name1 = string.Empty;

            if (Form2.config.specifiedEngageTarget == true && !string.IsNullOrEmpty(Form2.config.LuopanSpell_Target))
            {
                checkedName = Form2.config.LuopanSpell_Target;
            }
            else
            {
                checkedName = Monitored.Player.Name;
            }

            for (int x = 0; x < 2048; x++)
            {
                XiEntity entityGEO = PL.Entity.GetEntity(x);

                if (!string.IsNullOrEmpty(checkedName) && !string.IsNullOrEmpty(entityGEO.Name))
                {
                    name1 = entityGEO.Name;

                    if (name1 == checkedName)
                    {
                        return (int)entityGEO.Distance;
                    }
                }
            }

            return 0;
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
                                song_casting = 0;
                                ForceSongRecast = true;
                                if (Form2.config.FFXIDefaultAutoFollow == false)
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
                                song_casting = 0;
                                ForceSongRecast = true;
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
                                song_casting = 0;
                                ForceSongRecast = true;
                                if (Form2.config.FFXIDefaultAutoFollow == false)
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
                                song_casting = 0;
                                ForceSongRecast = true;
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

        public void Run_BardSongs()
        {
            PL_BRDCount = PL.Player.GetPlayerInfo().Buffs.Where(b => b == 195 || b == 196 || b == 197 || b == 198 || b == 199 || b == 200 || b == 201 || b == 214 || b == 215 || b == 216 || b == 218 || b == 219 || b == 222).Count();

            if (Form2.config.enableSinging && PL.Player.Status != 33)
            {

                debug_MSG_show = "ORDER: " + song_casting;

                SongData song_1 = SongInfo.Where(c => c.song_position == Form2.config.song1).FirstOrDefault();
                SongData song_2 = SongInfo.Where(c => c.song_position == Form2.config.song2).FirstOrDefault();
                SongData song_3 = SongInfo.Where(c => c.song_position == Form2.config.song3).FirstOrDefault();
                SongData song_4 = SongInfo.Where(c => c.song_position == Form2.config.song4).FirstOrDefault();

                SongData dummy1_song = SongInfo.Where(c => c.song_position == Form2.config.dummy1).FirstOrDefault();
                SongData dummy2_song = SongInfo.Where(c => c.song_position == Form2.config.dummy2).FirstOrDefault();

                // Check the distance of the Monitored player
                int Monitoreddistance = 50;


                XiEntity monitoredTarget = PL.Entity.GetEntity((int)Monitored.Player.TargetID);
                Monitoreddistance = (int)monitoredTarget.Distance;

                int Songs_Possible = 0;

                if (song_1.song_name.ToLower() != "blank")
                {
                    Songs_Possible++;
                }
                if (song_2.song_name.ToLower() != "blank")
                {
                    Songs_Possible++;
                }
                if (dummy1_song != null && dummy1_song.song_name.ToLower() != "blank")
                {
                    Songs_Possible++;
                }
                if (dummy2_song != null && dummy2_song.song_name.ToLower() != "blank")
                {
                    Songs_Possible++;
                }

                // List to make it easy to check how many of each buff is needed.
                List<int> SongDataMax = new List<int> { song_1.buff_id, song_2.buff_id, song_3.buff_id, song_4.buff_id };

                // Check Whether e have the songs Currently Up
                int count1_type = PL.Player.GetPlayerInfo().Buffs.Where(b => b == song_1.buff_id).Count();
                int count2_type = PL.Player.GetPlayerInfo().Buffs.Where(b => b == song_2.buff_id).Count();
                int count3_type = PL.Player.GetPlayerInfo().Buffs.Where(b => b == dummy1_song.buff_id).Count();
                int count4_type = PL.Player.GetPlayerInfo().Buffs.Where(b => b == song_3.buff_id).Count();
                int count5_type = PL.Player.GetPlayerInfo().Buffs.Where(b => b == dummy2_song.buff_id).Count();
                int count6_type = PL.Player.GetPlayerInfo().Buffs.Where(b => b == song_4.buff_id).Count();

                int MON_count1_type = Monitored.Player.GetPlayerInfo().Buffs.Where(b => b == song_1.buff_id).Count();
                int MON_count2_type = Monitored.Player.GetPlayerInfo().Buffs.Where(b => b == song_2.buff_id).Count();
                int MON_count3_type = Monitored.Player.GetPlayerInfo().Buffs.Where(b => b == dummy1_song.buff_id).Count();
                int MON_count4_type = Monitored.Player.GetPlayerInfo().Buffs.Where(b => b == song_3.buff_id).Count();
                int MON_count5_type = Monitored.Player.GetPlayerInfo().Buffs.Where(b => b == dummy2_song.buff_id).Count();
                int MON_count6_type = Monitored.Player.GetPlayerInfo().Buffs.Where(b => b == song_4.buff_id).Count();


                if (ForceSongRecast == true) { song_casting = 0; ForceSongRecast = false; }


                // SONG NUMBER #4
                if (song_casting == 3 && PL_BRDCount >= 3 && song_4.song_name.ToLower() != "blank" && count6_type < SongDataMax.Where(c => c == song_4.buff_id).Count() && Last_Song_Cast != song_4.song_name)
                {
                    if (PL_BRDCount == 3)
                    {
                        if (PL.SpellAvailable(dummy2_song.song_name))
                        {
                            CastSpell(Target.Me, dummy2_song.song_name);
                        }
                    }
                    else
                    {
                        if (PL.SpellAvailable(song_4.song_name))
                        {
                            CastSpell(Target.Me, song_4.song_name);
                            Last_Song_Cast = song_4.song_name;
                            Last_SongCast_Timer[0] = DateTime.Now;
                            playerSong4[0] = DateTime.Now;
                            song_casting = 0;
                        }
                    }

                }
                else if (song_casting == 3 && song_4.song_name.ToLower() != "blank" && count6_type >= SongDataMax.Where(c => c == song_4.buff_id).Count())
                {
                    song_casting = 0;
                }


                // SONG NUMBER #3
                else if (song_casting == 2 && PL_BRDCount >= 2 && song_3.song_name.ToLower() != "blank" && count4_type < SongDataMax.Where(c => c == song_3.buff_id).Count() && Last_Song_Cast != song_3.song_name)
                {
                    if (PL_BRDCount == 2)
                    {
                        if (PL.SpellAvailable(dummy1_song.song_name))
                        {
                            CastSpell(Target.Me, dummy1_song.song_name);
                        }
                    }
                    else
                    {
                        if (PL.SpellAvailable(song_3.song_name))
                        {
                            CastSpell(Target.Me, song_3.song_name);
                            Last_Song_Cast = song_3.song_name;
                            Last_SongCast_Timer[0] = DateTime.Now;
                            playerSong3[0] = DateTime.Now;
                            song_casting = 3;
                        }
                    }
                }
                else if (song_casting == 2 && song_3.song_name.ToLower() != "blank" && count4_type >= SongDataMax.Where(c => c == song_3.buff_id).Count())
                {
                    song_casting = 3;
                }


                // SONG NUMBER #2
                else if (song_casting == 1 && song_2.song_name.ToLower() != "blank" && count2_type < SongDataMax.Where(c => c == song_2.buff_id).Count() && Last_Song_Cast != song_4.song_name)
                {
                    if (PL.SpellAvailable(song_2.song_name))
                    {
                        CastSpell(Target.Me, song_2.song_name);
                        Last_Song_Cast = song_2.song_name;
                        Last_SongCast_Timer[0] = DateTime.Now;
                        playerSong2[0] = DateTime.Now;
                        song_casting = 2;
                    }
                }
                else if (song_casting == 1 && song_2.song_name.ToLower() != "blank" && count2_type >= SongDataMax.Where(c => c == song_2.buff_id).Count())
                {
                    song_casting = 2;
                }

                // SONG NUMBER #1
                else if ((song_casting == 0) && song_1.song_name.ToLower() != "blank" && count1_type < SongDataMax.Where(c => c == song_1.buff_id).Count() && Last_Song_Cast != song_4.song_name)
                {
                    if (PL.SpellAvailable(song_1.song_name))
                    {
                        CastSpell(Target.Me, song_1.song_name);
                        Last_Song_Cast = song_1.song_name;
                        Last_SongCast_Timer[0] = DateTime.Now;
                        playerSong1[0] = DateTime.Now;
                        song_casting = 1;
                    }

                }
                else if (song_casting == 0 && song_2.song_name.ToLower() != "blank" && count1_type >= SongDataMax.Where(c => c == song_1.buff_id).Count())
                {
                    song_casting = 1;
                }


                // ONCE ALL SONGS HAVE BEEN CAST ONLY RECAST THEM WHEN THEY MEET THE THRESHOLD SET ON SONG RECAST AND BLOCK IF IT'S SET AT LAUNCH DEFAULTS
                if (playerSong1[0] != DefaultTime && playerSong1_Span[0].Minutes >= Form2.config.recastSongTime)
                {
                    if ((Form2.config.SongsOnlyWhenNear && Monitoreddistance < 10) || Form2.config.SongsOnlyWhenNear == false)
                    {
                        if (PL.SpellAvailable(song_1.song_name))
                        {
                            CastSpell(Target.Me, song_1.song_name);
                            playerSong1[0] = DateTime.Now;
                            song_casting = 0;
                        }
                    }
                }
                else if (playerSong2[0] != DefaultTime && playerSong2_Span[0].Minutes >= Form2.config.recastSongTime)
                {
                    if ((Form2.config.SongsOnlyWhenNear && Monitoreddistance < 10) || Form2.config.SongsOnlyWhenNear == false)
                    {
                        if (PL.SpellAvailable(song_2.song_name))
                        {
                            CastSpell(Target.Me, song_2.song_name);
                            playerSong2[0] = DateTime.Now;
                            song_casting = 0;
                        }
                    }
                }
                else if (playerSong3[0] != DefaultTime && playerSong3_Span[0].Minutes >= Form2.config.recastSongTime)
                {
                    if ((Form2.config.SongsOnlyWhenNear && Monitoreddistance < 10) || Form2.config.SongsOnlyWhenNear == false)
                    {
                        if (PL.SpellAvailable(song_3.song_name))
                        {
                            CastSpell(Target.Me, song_3.song_name);
                            playerSong3[0] = DateTime.Now;
                            song_casting = 0;
                        }
                    }
                }
                else if (playerSong4[0] != DefaultTime && playerSong4_Span[0].Minutes >= Form2.config.recastSongTime)
                {
                    if ((Form2.config.SongsOnlyWhenNear && Monitoreddistance < 10) || Form2.config.SongsOnlyWhenNear == false)
                    {
                        if (PL.SpellAvailable(song_4.song_name))
                        {
                            CastSpell(Target.Me, song_4.song_name);
                            playerSong4[0] = DateTime.Now;
                            song_casting = 0;
                        }
                    }
                }


            }
        }
        private void Follow_BGW_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {

            // MAKE SURE BOTH ELITEAPI INSTANCES ARE ACTIVE, THE BOT ISN'T PAUSED, AND THERE IS AN AUTOFOLLOWTARGET NAMED
            if (PL != null && Monitored != null && !string.IsNullOrEmpty(Form2.config.autoFollowName) && !pauseActions)
            {

                if (Form2.config.FFXIDefaultAutoFollow != true)
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

                    if (Math.Truncate(followTarget.Distance) >= (int)Form2.config.autoFollowDistance && curePlease_autofollow == false)
                    {
                        // THE DISTANCE IS GREATER THAN REQUIRED SO IF AUTOFOLLOW IS NOT ACTIVE THEN DEPENDING ON THE TYPE, FOLLOW

                        // SQUARE ENIX FINAL FANTASY XI DEFAULT AUTO FOLLOW
                        if (Form2.config.FFXIDefaultAutoFollow == true && PL.AutoFollow.IsAutoFollowing != true)
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
                        else if (Form2.config.FFXIDefaultAutoFollow != true && PL.AutoFollow.IsAutoFollowing != true)
                        {
                            // IF YOU ARE TOO FAR TO FOLLOW THEN STOP AND IF ENABLED WARN THE MONITORED PLAYER
                            if (Form2.config.autoFollow_Warning == true && Math.Truncate(followTarget.Distance) >= 40 && Monitored.Player.Name != PL.Player.Name && followWarning == 0)
                            {
                                string createdTell = "/tell " + Monitored.Player.Name + " " + "You're too far to follow.";
                                PL.ThirdParty.SendString(createdTell);
                                followWarning = 1;
                                Thread.Sleep(TimeSpan.FromSeconds(0.3));
                            }
                            else if (Math.Truncate(followTarget.Distance) <= 40)
                            {
                                // ONLY TARGET AND BEGIN FOLLOW IF TARGET IS AT THE DEFINED DISTANCE
                                if (Math.Truncate(followTarget.Distance) >= (int)Form2.config.autoFollowDistance && Math.Truncate(followTarget.Distance) <= 48)
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
                                        while (Math.Truncate(followTarget.Distance) >= (int)Form2.config.autoFollowDistance)
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
                                                if (Form2.config.autoFollow_Warning == true && stuckWarning != true && FollowerTargetEntity.Name == Monitored.Player.Name && stuckCount == 10)
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
                settings = new Form2();
            }
            settings.Show();

        }

        private void ChatLogButton_Click(object sender, EventArgs e)
        {
            Form4 form4 = new Form4(this);

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
            new Form3().Show();
        }

        private void AddonReader_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            if (Form2.config.EnableAddOn == true && pauseActions == false && Monitored != null && PL != null)
            {

                bool done = false;

                UdpClient listener = new UdpClient(Convert.ToInt32(Form2.config.listeningPort));
                IPEndPoint groupEP = new IPEndPoint(IPAddress.Parse(Form2.config.ipAddress), Convert.ToInt32(Form2.config.listeningPort));
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
                        if (commands[1] == "casting" && commands.Count() == 3 && Form2.config.trackCastingPackets == true)
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
                                    song_casting = 0;
                                    ForceSongRecast = true;
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
                                    if (Form2.config.FFXIDefaultAutoFollow == false)
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


        private void FullCircle_Timer_Tick(object sender, EventArgs e)
        {

            if (PL.Player.Pet.HealthPercent >= 1)
            {
                ushort PetsIndex = PL.Player.PetIndex;

                if (Form2.config.Fullcircle_GEOTarget == true && Form2.config.LuopanSpell_Target != "")
                {
                    XiEntity PetsEntity = PL.Entity.GetEntity(PetsIndex);

                    int FullCircle_CharID = 0;

                    for (int x = 0; x < 2048; x++)
                    {
                        XiEntity entity = PL.Entity.GetEntity(x);

                        if (entity.Name != null && entity.Name.ToLower().Equals(Form2.config.LuopanSpell_Target.ToLower()))
                        {
                            FullCircle_CharID = Convert.ToInt32(entity.TargetID);
                            break;
                        }
                    }

                    if (FullCircle_CharID != 0)
                    {
                        XiEntity FullCircleEntity = PL.Entity.GetEntity(FullCircle_CharID);

                        float fX = PetsEntity.X - FullCircleEntity.X;
                        float fY = PetsEntity.Y - FullCircleEntity.Y;
                        float fZ = PetsEntity.Z - FullCircleEntity.Z;

                        float generatedDistance = (float)Math.Sqrt((fX * fX) + (fY * fY) + (fZ * fZ));

                        if (generatedDistance >= 10)
                        {
                            PL.ThirdParty.SendString("/ja \"Full Circle\" <me>");
                        }
                    }

                }
                else if (Form2.config.Fullcircle_GEOTarget == false && Monitored.Player.Status == 1)
                {


                    string SpellCheckedResult = ReturnGeoSpell(Form2.config.GeoSpell_Spell, 2);



                    if (Form2.config.Fullcircle_DisableEnemy != true || (Form2.config.Fullcircle_DisableEnemy == true && PL.Resources.GetSpell(SpellCheckedResult, 0).ValidTargets == 32))
                    {
                        XiEntity PetsEntity = Monitored.Entity.GetEntity(PetsIndex);

                        if (PetsEntity.Distance >= 10 && PetsEntity.Distance != 0 && PL.AbilityAvailable(Ability.FullCircle))
                        {
                            PL.ThirdParty.SendString("/ja \"Full Circle\" <me>");
                        }
                    }
                }
            }

            FullCircle_Timer.Enabled = false;
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

        private void JobAbility_Delay_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            Invoke((MethodInvoker)(() =>
            {
                JobAbilityLock_Check = true;
                castingLockLabel.Text = "Casting is LOCKED for a JA.";
                currentAction.Text = "Using a Job Ability: " + JobAbilityCMD;
                // This is how long we want for Job Ability/JA to resolve.
                // Since most of us are using JA0Wait, can be very short.
                Thread.Sleep(TimeSpan.FromSeconds(2));
                castingLockLabel.Text = "Casting is UNLOCKED";
                currentAction.Text = string.Empty;
                castingSpell = string.Empty;
                //JobAbilityLock_Check = false;
                JobAbilityCMD = string.Empty;
            }));
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
