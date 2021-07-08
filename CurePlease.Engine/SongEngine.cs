using CurePlease.Model;
using CurePlease.Model.Config;
using CurePlease.Model.Constants;
using CurePlease.Utilities;
using EliteMMO.API;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using static EliteMMO.API.EliteAPI;

namespace CurePlease.Engine
{
    public class SongEngine : ISongEngine
    {
        private class SongData : List<SongData>
        {
            public string Type { get; set; }
            public int Position { get; set; }
            public string Name { get; set; }
            public int BuffId { get; set; }
        }

        private EliteAPI PL { get; set; }

        // BARD SONG VARIABLES
        private int song_casting = 0;

        private int PL_BRDCount = 0;
        private bool ForceSongRecast = false;
        private string Last_Song_Cast = string.Empty;

        private readonly List<SongData> SongInfo = new();

        private readonly List<int> known_song_buffs = new();

        private readonly DateTime DefaultTime = new(1970, 1, 1);

        private readonly DateTime[] playerSong1 = new DateTime[]
        {
            new DateTime(1970, 1, 1, 0, 0, 0)
        };

        private readonly DateTime[] playerSong2 = new DateTime[]
        {
            new DateTime(1970, 1, 1, 0, 0, 0)
        };

        private readonly DateTime[] playerSong3 = new DateTime[]
        {
            new DateTime(1970, 1, 1, 0, 0, 0)
        };

        private readonly DateTime[] playerSong4 = new DateTime[]
        {
            new DateTime(1970, 1, 1, 0, 0, 0)
        };

        private readonly DateTime[] Last_SongCast_Timer = new DateTime[]
        {
            new DateTime(1970, 1, 1, 0, 0, 0)
        };

        private readonly DateTime[] playerPianissimo1_1 = new DateTime[]
        {
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0)
        };

        private readonly DateTime[] playerPianissimo2_1 = new DateTime[]
        {
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0)
        };

        private readonly DateTime[] playerPianissimo1_2 = new DateTime[]
        {
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0)
        };

        private readonly DateTime[] playerPianissimo2_2 = new DateTime[]
        {
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0)
        };

        private readonly TimeSpan[] playerSong1_Span = new TimeSpan[]
        {
            new TimeSpan()
        };

        private readonly TimeSpan[] playerSong2_Span = new TimeSpan[]
        {
            new TimeSpan()
        };

        private readonly TimeSpan[] playerSong3_Span = new TimeSpan[]
        {
            new TimeSpan()
        };

        private readonly TimeSpan[] playerSong4_Span = new TimeSpan[]
        {
            new TimeSpan()
        };

        private readonly TimeSpan[] Last_SongCast_Timer_Span = new TimeSpan[]
        {
            new TimeSpan()
        };

        private readonly TimeSpan[] pianissimo1_1_Span = new TimeSpan[]
        {
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
        };

        private readonly TimeSpan[] pianissimo2_1_Span = new TimeSpan[]
        {
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
        };

        private readonly TimeSpan[] pianissimo1_2_Span = new TimeSpan[]
        {
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
        };

        private readonly TimeSpan[] pianissimo2_2_Span = new TimeSpan[]
        {
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
        };

        private readonly Timer resetSongTimer = new();

        private readonly ILogger<SongEngine> _Logger;

        public SongEngine(ILogger<SongEngine> logger)
        {
            _Logger = logger;

            InitializeData();

            resetSongTimer.Enabled = true;
            resetSongTimer.Interval = 60000;
            resetSongTimer.Elapsed += ResetSongTimer_Tick;
        }

        private void InitializeData()
        {
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
                Type = "Blank",
                Name = "Blank",
                Position = position,
                BuffId = 0
            });
            position++;

            SongInfo.Add(new SongData
            {
                Type = "Minne",
                Name = "Knight's Minne",
                Position = position,
                BuffId = 197
            });
            position++;

            SongInfo.Add(new SongData
            {
                Type = "Minne",
                Name = "Knight's Minne II",
                Position = position,
                BuffId = 197
            });
            position++;

            SongInfo.Add(new SongData
            {
                Type = "Minne",
                Name = "Knight's Minne III",
                Position = position,
                BuffId = 197
            });
            position++;

            SongInfo.Add(new SongData
            {
                Type = "Minne",
                Name = "Knight's Minne IV",
                Position = position,
                BuffId = 197
            });
            position++;

            SongInfo.Add(new SongData
            {
                Type = "Minne",
                Name = "Knight's Minne V",
                Position = position,
                BuffId = 197
            });
            position++;

            SongInfo.Add(new SongData
            {
                Type = "Blank",
                Name = "Blank",
                Position = position,
                BuffId = 0
            });
            position++;

            SongInfo.Add(new SongData
            {
                Type = "Minuet",
                Name = "Valor Minuet",
                Position = position,
                BuffId = 198
            });
            position++;

            SongInfo.Add(new SongData
            {
                Type = "Minuet",
                Name = "Valor Minuet II",
                Position = position,
                BuffId = 198
            });
            position++;

            SongInfo.Add(new SongData
            {
                Type = "Minuet",
                Name = "Valor Minuet III",
                Position = position,
                BuffId = 198
            });
            position++;

            SongInfo.Add(new SongData
            {
                Type = "Minuet",
                Name = "Valor Minuet IV",
                Position = position,
                BuffId = 198
            });
            position++;

            SongInfo.Add(new SongData
            {
                Type = "Minuet",
                Name = "Valor Minuet V",
                Position = position,
                BuffId = 198
            });
            position++;

            SongInfo.Add(new SongData
            {
                Type = "Blank",
                Name = "Blank",
                Position = position,
                BuffId = 0
            });
            position++;

            SongInfo.Add(new SongData
            {
                Type = "Paeon",
                Name = "Army's Paeon",
                Position = position,
                BuffId = 195
            });
            position++;

            SongInfo.Add(new SongData
            {
                Type = "Paeon",
                Name = "Army's Paeon II",
                Position = position,
                BuffId = 195
            });
            position++;

            SongInfo.Add(new SongData
            {
                Type = "Paeon",
                Name = "Army's Paeon III",
                Position = position,
                BuffId = 195
            });
            position++;

            SongInfo.Add(new SongData
            {
                Type = "Paeon",
                Name = "Army's Paeon IV",
                Position = position,
                BuffId = 195
            });
            position++;

            SongInfo.Add(new SongData
            {
                Type = "Paeon",
                Name = "Army's Paeon V",
                Position = position,
                BuffId = 195
            });
            position++;

            SongInfo.Add(new SongData
            {
                Type = "Paeon",
                Name = "Army's Paeon VI",
                Position = position,
                BuffId = 195
            });
            position++;

            SongInfo.Add(new SongData
            {
                Type = "Blank",
                Name = "Blank",
                Position = position,
                BuffId = 0
            });
            position++;

            SongInfo.Add(new SongData
            {
                Type = "Madrigal",
                Name = "Sword Madrigal",
                Position = position,
                BuffId = 199
            });
            position++;
            SongInfo.Add(new SongData
            {
                Type = "Madrigal",
                Name = "Blade Madrigal",
                Position = position,
                BuffId = 199
            });
            position++;

            SongInfo.Add(new SongData
            {
                Type = "Blank",
                Name = "Blank",
                Position = position,
                BuffId = 0
            });
            position++;

            SongInfo.Add(new SongData
            {
                Type = "Prelude",
                Name = "Hunter's Prelude",
                Position = position,
                BuffId = 200
            });
            position++;

            SongInfo.Add(new SongData
            {
                Type = "Prelude",
                Name = "Archer's Prelude",
                Position = position,
                BuffId = 200
            });
            position++;

            SongInfo.Add(new SongData
            {
                Type = "Blank",
                Name = "Blank",
                Position = position,
                BuffId = 0
            });
            position++;

            SongInfo.Add(new SongData
            {
                Type = "Etude",
                Name = "Sinewy Etude",
                Position = position,
                BuffId = 215
            });
            position++;

            SongInfo.Add(new SongData
            {
                Type = "Etude",
                Name = "Dextrous Etude",
                Position = position,
                BuffId = 215
            });
            position++;

            SongInfo.Add(new SongData
            {
                Type = "Etude",
                Name = "Vivacious Etude",
                Position = position,
                BuffId = 215
            });
            position++;

            SongInfo.Add(new SongData
            {
                Type = "Etude",
                Name = "Quick Etude",
                Position = position,
                BuffId = 215
            });
            position++;

            SongInfo.Add(new SongData
            {
                Type = "Etude",
                Name = "Learned Etude",
                Position = position,
                BuffId = 215
            });
            position++;

            SongInfo.Add(new SongData
            {
                Type = "Etude",
                Name = "Spirited Etude",
                Position = position,
                BuffId = 215
            });
            position++;

            SongInfo.Add(new SongData
            {
                Type = "Etude",
                Name = "Enchanting Etude",
                Position = position,
                BuffId = 215
            });
            position++;

            SongInfo.Add(new SongData
            {
                Type = "Etude",
                Name = "Herculean Etude",
                Position = position,
                BuffId = 215
            });
            position++;

            SongInfo.Add(new SongData
            {
                Type = "Etude",
                Name = "Uncanny Etude",
                Position = position,
                BuffId = 215
            });
            position++;

            SongInfo.Add(new SongData
            {
                Type = "Etude",
                Name = "Vital Etude",
                Position = position,
                BuffId = 215
            });
            position++;

            SongInfo.Add(new SongData
            {
                Type = "Etude",
                Name = "Swift Etude",
                Position = position,
                BuffId = 215
            });
            position++;

            SongInfo.Add(new SongData
            {
                Type = "Etude",
                Name = "Sage Etude",
                Position = position,
                BuffId = 215
            });
            position++;

            SongInfo.Add(new SongData
            {
                Type = "Etude",
                Name = "Logical Etude",
                Position = position,
                BuffId = 215
            });
            position++;

            SongInfo.Add(new SongData
            {
                Type = "Etude",
                Name = "Bewitching Etude",
                Position = position,
                BuffId = 215
            });
            position++;

            SongInfo.Add(new SongData
            {
                Type = "Blank",
                Name = "Blank",
                Position = position,
                BuffId = 0
            });
            position++;

            SongInfo.Add(new SongData
            {
                Type = "Mambo",
                Name = "Sheepfoe Mambo",
                Position = position,
                BuffId = 201
            });
            position++;

            SongInfo.Add(new SongData
            {
                Type = "Mambo",
                Name = "Dragonfoe Mambo",
                Position = position,
                BuffId = 201
            });
            position++;

            SongInfo.Add(new SongData
            {
                Type = "Blank",
                Name = "Blank",
                Position = position,
                BuffId = 0
            });
            position++;

            SongInfo.Add(new SongData
            {
                Type = "Ballad",
                Name = "Mage's Ballad",
                Position = position,
                BuffId = 196
            });
            position++;

            SongInfo.Add(new SongData
            {
                Type = "Ballad",
                Name = "Mage's Ballad II",
                Position = position,
                BuffId = 196
            });
            position++;

            SongInfo.Add(new SongData
            {
                Type = "Ballad",
                Name = "Mage's Ballad III",
                Position = position,
                BuffId = 196
            });
            position++;

            SongInfo.Add(new SongData
            {
                Type = "Blank",
                Name = "Blank",
                Position = position,
                BuffId = 0
            });
            position++;

            SongInfo.Add(new SongData
            {
                Type = "March",
                Name = "Advancing March",
                Position = position,
                BuffId = 214
            });
            position++;

            SongInfo.Add(new SongData
            {
                Type = "March",
                Name = "Victory March",
                Position = position,
                BuffId = 214
            });
            position++;

            SongInfo.Add(new SongData
            {
                Type = "March",
                Name = "Honor March",
                Position = position,
                BuffId = 214
            });
            position++;

            SongInfo.Add(new SongData
            {
                Type = "Blank",
                Name = "Blank",
                Position = position,
                BuffId = 0
            });
            position++;

            SongInfo.Add(new SongData
            {
                Type = "Carol",
                Name = "Fire Carol",
                Position = position
            });
            position++;

            SongInfo.Add(new SongData
            {
                Type = "Carol",
                Name = "Fire Carol II",
                Position = position,
                BuffId = 216
            });
            position++;

            SongInfo.Add(new SongData
            {
                Type = "Carol",
                Name = "Ice Carol",
                Position = position,
                BuffId = 216
            });
            position++;

            SongInfo.Add(new SongData
            {
                Type = "Carol",
                Name = "Ice Carol II",
                Position = position,
                BuffId = 216
            });
            position++;

            SongInfo.Add(new SongData
            {
                Type = "Carol",
                Name = " Wind Carol",
                Position = position,
                BuffId = 216
            });
            position++;

            SongInfo.Add(new SongData
            {
                Type = "Carol",
                Name = "Wind Carol II",
                Position = position,
                BuffId = 216
            });
            position++;

            SongInfo.Add(new SongData
            {
                Type = "Carol",
                Name = "Earth Carol",
                Position = position,
                BuffId = 216
            });
            position++;

            SongInfo.Add(new SongData
            {
                Type = "Carol",
                Name = "Earth Carol II",
                Position = position,
                BuffId = 216
            });
            position++;

            SongInfo.Add(new SongData
            {
                Type = "Carol",
                Name = "Lightning Carol",
                Position = position,
                BuffId = 216
            });
            position++;

            SongInfo.Add(new SongData
            {
                Type = "Carol",
                Name = "Lightning Carol II",
                Position = position,
                BuffId = 216
            });
            position++;

            SongInfo.Add(new SongData
            {
                Type = "Carol",
                Name = "Water Carol",
                Position = position,
                BuffId = 216
            });
            position++;

            SongInfo.Add(new SongData
            {
                Type = "Carol",
                Name = "Water Carol II",
                Position = position,
                BuffId = 216
            });
            position++;

            SongInfo.Add(new SongData
            {
                Type = "Carol",
                Name = "Light Carol",
                Position = position,
                BuffId = 216
            });
            position++;

            SongInfo.Add(new SongData
            {
                Type = "Carol",
                Name = "Light Carol II",
                Position = position,
                BuffId = 216
            });
            position++;

            SongInfo.Add(new SongData
            {
                Type = "Carol",
                Name = "Dark Carol",
                Position = position,
                BuffId = 216
            });
            position++;

            SongInfo.Add(new SongData
            {
                Type = "Carol",
                Name = "Dark Carol II",
                Position = position,
                BuffId = 216
            });
            position++;

            SongInfo.Add(new SongData
            {
                Type = "Blank",
                Name = "Blank",
                Position = position,
                BuffId = 0
            });
            position++;

            SongInfo.Add(new SongData
            {
                Type = "Hymnus",
                Name = "Godess's Hymnus",
                Position = position,
                BuffId = 218
            });
            position++;

            SongInfo.Add(new SongData
            {
                Type = "Blank",
                Name = "Blank",
                Position = position,
                BuffId = 0
            });
            position++;

            SongInfo.Add(new SongData
            {
                Type = "Scherzo",
                Name = "Sentinel's Scherzo",
                Position = position,
                BuffId = 222
            });
        }

        public EngineAction Run(EliteAPI pl, EliteAPI monitored, SongConfig Config)
        {
            EngineAction actionResult = new()
            {
                Target = Target.Me
            };

            try
            {
                PL_BRDCount = PL.Player.GetPlayerInfo().Buffs.Where(b => b == 195 || b == 196 || b == 197 || b == 198 || b == 199 || b == 200 || b == 201 || b == 214 || b == 215 || b == 216 || b == 218 || b == 219 || b == 222).Count();

                if (Config.SingingEnabled && PL.Player.Status != 33)
                {
                    int songs_currently_up = PL.Player.GetPlayerInfo().Buffs.Where(b => b == 197 || b == 198 || b == 195 || b == 199 || b == 200 || b == 215 || b == 196 || b == 214 || b == 216 || b == 218 || b == 222).Count();

                    // TODO: Add support for multiple JAs per cast, or else this will never work.
                    // For now we just return the action with only the ability in it.
                    // ie. If N + T were up, this would return once with Troub, then next cycle return Night,
                    // then finally return the song cast.
                    if (Config.TroubadourEnabled && PL.AbilityAvailable(Ability.Troubadour) && songs_currently_up == 0)
                    {
                        actionResult.JobAbility = Ability.Troubadour;
                        return actionResult;
                    }
                    else if (Config.NightingaleEnabled && PL.AbilityAvailable(Ability.Nightingale) && songs_currently_up == 0)
                    {
                        actionResult.JobAbility = Ability.Nightingale;
                        return actionResult;
                    }

                    SongData song_1 = SongInfo.Where(c => c.Position == Config.Song1).FirstOrDefault();
                    SongData song_2 = SongInfo.Where(c => c.Position == Config.Song2).FirstOrDefault();
                    SongData song_3 = SongInfo.Where(c => c.Position == Config.Song3).FirstOrDefault();
                    SongData song_4 = SongInfo.Where(c => c.Position == Config.Song4).FirstOrDefault();

                    SongData dummy1_song = SongInfo.Where(c => c.Position == Config.Dummy1).FirstOrDefault();
                    SongData dummy2_song = SongInfo.Where(c => c.Position == Config.Dummy2).FirstOrDefault();

                    // Check the distance of the Monitored player
                    int Monitoreddistance = 50;


                    XiEntity monitoredTarget = PL.Entity.GetEntity((int)monitored.Player.TargetID);
                    Monitoreddistance = (int)monitoredTarget.Distance;

                    int Songs_Possible = 0;

                    if (song_1.Name.ToLower() != "blank")
                    {
                        Songs_Possible++;
                    }
                    if (song_2.Name.ToLower() != "blank")
                    {
                        Songs_Possible++;
                    }
                    if (dummy1_song != null && dummy1_song.Name.ToLower() != "blank")
                    {
                        Songs_Possible++;
                    }
                    if (dummy2_song != null && dummy2_song.Name.ToLower() != "blank")
                    {
                        Songs_Possible++;
                    }

                    // List to make it easy to check how many of each buff is needed.
                    List<int> SongDataMax = new() { song_1.BuffId, song_2.BuffId, song_3.BuffId, song_4.BuffId };

                    // Check Whether e have the songs Currently Up
                    int count1_type = PL.Player.GetPlayerInfo().Buffs.Where(b => b == song_1.BuffId).Count();
                    int count2_type = PL.Player.GetPlayerInfo().Buffs.Where(b => b == song_2.BuffId).Count();
                    int count3_type = PL.Player.GetPlayerInfo().Buffs.Where(b => b == dummy1_song.BuffId).Count();
                    int count4_type = PL.Player.GetPlayerInfo().Buffs.Where(b => b == song_3.BuffId).Count();
                    int count5_type = PL.Player.GetPlayerInfo().Buffs.Where(b => b == dummy2_song.BuffId).Count();
                    int count6_type = PL.Player.GetPlayerInfo().Buffs.Where(b => b == song_4.BuffId).Count();

                    int MON_count1_type = monitored.Player.GetPlayerInfo().Buffs.Where(b => b == song_1.BuffId).Count();
                    int MON_count2_type = monitored.Player.GetPlayerInfo().Buffs.Where(b => b == song_2.BuffId).Count();
                    int MON_count3_type = monitored.Player.GetPlayerInfo().Buffs.Where(b => b == dummy1_song.BuffId).Count();
                    int MON_count4_type = monitored.Player.GetPlayerInfo().Buffs.Where(b => b == song_3.BuffId).Count();
                    int MON_count5_type = monitored.Player.GetPlayerInfo().Buffs.Where(b => b == dummy2_song.BuffId).Count();
                    int MON_count6_type = monitored.Player.GetPlayerInfo().Buffs.Where(b => b == song_4.BuffId).Count();

                    if (ForceSongRecast == true) { song_casting = 0; ForceSongRecast = false; }


                    // SONG NUMBER #4
                    if (song_casting == 3 && PL_BRDCount >= 3 && song_4.Name.ToLower() != "blank" && count6_type < SongDataMax.Where(c => c == song_4.BuffId).Count() && Last_Song_Cast != song_4.Name)
                    {
                        if (PL_BRDCount == 3)
                        {
                            if (PL.SpellAvailable(dummy2_song.Name))
                            {
                                actionResult.Spell = dummy2_song.Name;
                            }
                        }
                        else
                        {
                            if (PL.SpellAvailable(song_4.Name))
                            {
                                actionResult.Spell = song_4.Name;

                                Last_Song_Cast = song_4.Name;
                                Last_SongCast_Timer[0] = DateTime.Now;
                                playerSong4[0] = DateTime.Now;
                                song_casting = 0;
                            }
                        }
                    }
                    else if (song_casting == 3 && song_4.Name.ToLower() != "blank" && count6_type >= SongDataMax.Where(c => c == song_4.BuffId).Count())
                    {
                        song_casting = 0;
                    }


                    // SONG NUMBER #3
                    else if (song_casting == 2 && PL_BRDCount >= 2 && song_3.Name.ToLower() != "blank" && count4_type < SongDataMax.Where(c => c == song_3.BuffId).Count() && Last_Song_Cast != song_3.Name)
                    {
                        if (PL_BRDCount == 2)
                        {
                            if (PL.SpellAvailable(dummy1_song.Name))
                            {
                                actionResult.Spell = dummy1_song.Name;
                            }
                        }
                        else
                        {
                            if (PL.SpellAvailable(song_3.Name))
                            {
                                actionResult.Spell = song_3.Name;

                                Last_Song_Cast = song_3.Name;
                                Last_SongCast_Timer[0] = DateTime.Now;
                                playerSong3[0] = DateTime.Now;
                                song_casting = 3;
                            }
                        }
                    }
                    else if (song_casting == 2 && song_3.Name.ToLower() != "blank" && count4_type >= SongDataMax.Where(c => c == song_3.BuffId).Count())
                    {
                        song_casting = 3;
                    }


                    // SONG NUMBER #2
                    else if (song_casting == 1 && song_2.Name.ToLower() != "blank" && count2_type < SongDataMax.Where(c => c == song_2.BuffId).Count() && Last_Song_Cast != song_4.Name)
                    {
                        if (PL.SpellAvailable(song_2.Name))
                        {
                            actionResult.Spell = song_2.Name;

                            Last_Song_Cast = song_2.Name;
                            Last_SongCast_Timer[0] = DateTime.Now;
                            playerSong2[0] = DateTime.Now;
                            song_casting = 2;
                        }
                    }
                    else if (song_casting == 1 && song_2.Name.ToLower() != "blank" && count2_type >= SongDataMax.Where(c => c == song_2.BuffId).Count())
                    {
                        song_casting = 2;
                    }

                    // SONG NUMBER #1
                    else if ((song_casting == 0) && song_1.Name.ToLower() != "blank" && count1_type < SongDataMax.Where(c => c == song_1.BuffId).Count() && Last_Song_Cast != song_4.Name)
                    {
                        if (PL.SpellAvailable(song_1.Name))
                        {
                            actionResult.Spell = song_1.Name;

                            Last_Song_Cast = song_1.Name;
                            Last_SongCast_Timer[0] = DateTime.Now;
                            playerSong1[0] = DateTime.Now;
                            song_casting = 1;
                        }

                    }
                    else if (song_casting == 0 && song_2.Name.ToLower() != "blank" && count1_type >= SongDataMax.Where(c => c == song_1.BuffId).Count())
                    {
                        song_casting = 1;
                    }


                    // ONCE ALL SONGS HAVE BEEN CAST ONLY RECAST THEM WHEN THEY MEET THE THRESHOLD SET ON SONG RECAST AND BLOCK IF IT'S SET AT LAUNCH DEFAULTS
                    if (playerSong1[0] != DefaultTime && playerSong1_Span[0].Minutes >= Config.SongRecastMinutes)
                    {
                        if ((Config.SingOnlyWhenNear && Monitoreddistance < 10) || Config.SingOnlyWhenNear == false)
                        {
                            if (PL.SpellAvailable(song_1.Name))
                            {
                                actionResult.Spell = song_1.Name;

                                playerSong1[0] = DateTime.Now;
                                song_casting = 0;
                            }
                        }
                    }
                    else if (playerSong2[0] != DefaultTime && playerSong2_Span[0].Minutes >= Config.SongRecastMinutes)
                    {
                        if ((Config.SingOnlyWhenNear && Monitoreddistance < 10) || Config.SingOnlyWhenNear == false)
                        {
                            if (PL.SpellAvailable(song_2.Name))
                            {
                                actionResult.Spell = song_2.Name;

                                playerSong2[0] = DateTime.Now;
                                song_casting = 0;
                            }
                        }
                    }
                    else if (playerSong3[0] != DefaultTime && playerSong3_Span[0].Minutes >= Config.SongRecastMinutes)
                    {
                        if ((Config.SingOnlyWhenNear && Monitoreddistance < 10) || Config.SingOnlyWhenNear == false)
                        {
                            if (PL.SpellAvailable(song_3.Name))
                            {
                                actionResult.Spell = song_3.Name;
                                playerSong3[0] = DateTime.Now;
                                song_casting = 0;
                            }
                        }
                    }
                    else if (playerSong4[0] != DefaultTime && playerSong4_Span[0].Minutes >= Config.SongRecastMinutes)
                    {
                        if ((Config.SingOnlyWhenNear && Monitoreddistance < 10) || Config.SingOnlyWhenNear == false)
                        {
                            if (PL.SpellAvailable(song_4.Name))
                            {
                                actionResult.Spell = song_4.Name;
                                playerSong4[0] = DateTime.Now;
                                song_casting = 0;
                            }
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                _Logger.LogError("Unexpected issue occurred while running song engine", ex);
            }

            return actionResult;
        }

        private void UpdateTimers()
        {
            var currentTime = DateTime.Now;
            // Calculate time since Songs were cast on particular player
            playerSong1_Span[0] = currentTime.Subtract(playerSong1[0]);
            playerSong2_Span[0] = currentTime.Subtract(playerSong2[0]);
            playerSong3_Span[0] = currentTime.Subtract(playerSong3[0]);
            playerSong4_Span[0] = currentTime.Subtract(playerSong4[0]);

            Last_SongCast_Timer_Span[0] = currentTime.Subtract(Last_SongCast_Timer[0]);

            // Calculate time since Piannisimo Songs were cast on particular player
            pianissimo1_1_Span[0] = currentTime.Subtract(playerPianissimo1_1[0]);
            pianissimo2_1_Span[0] = currentTime.Subtract(playerPianissimo2_1[0]);
            pianissimo1_2_Span[0] = currentTime.Subtract(playerPianissimo1_2[0]);
            pianissimo2_2_Span[0] = currentTime.Subtract(playerPianissimo2_2[0]);
        }

        private void ResetSongTimer_Tick(object sender, EventArgs e)
        {
            song_casting = 0;
        }
    }
}
