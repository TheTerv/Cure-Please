﻿using CurePlease.Model;
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
    public class GeoData : List<GeoData>
    {
        public int geo_position { get; set; }

        public string indi_spell { get; set; }

        public string geo_spell { get; set; }
    }

    public class GeoEngine : IGeoEngine
    {
        private readonly ILogger<GeoEngine> _Logger;

        private GeoConfig _Config;

        // GEO ENGAGED CHECK
        public bool targetEngaged = false;
        private bool EclipticStillUp = false;

        public List<GeoData> GeomancerInfo = new List<GeoData>();

        private EliteAPI PL;
        private EliteAPI Monitored;

        private Timer FullCircleTimer = new Timer();
        private Timer EclipticTimer = new Timer();

        private List<int> CityZoneIds = new List<int>
        {
            26, // Tavnazian Safehold

            50, // Aht Urhgan Whitegate

            53, // Nashmau

            224, // Bastok-Jeuno Airship

            230, // Southern San D'oria 230
            231, // Northern San D'oria 231
            232, // Port San D'oria 232
            233, // Chateau D'oraguille 233
            234, // Bastok Mines 234
            235, // Bastok Markets 235
            236, // Port Bastok 236
            237, // Metalworks 237
            238, // Windurst Waters 238
            239, // Windurst Walls 239
            240, // Port Windurst 240
            241, // Windurst Woods 241
            242, // Heaven Tower 242
            243, // Ru'lude Gardnes 243
            244, // Upper Jeuno 244
            245, // Lower Jeuno 245
            246, // Port Jeuno 246

            248, // Selbina 248
            249, // Mhaura 249
            250, // Kazham 250

            252, // Norg 252

            256, // Western is 256
            257 // Eastern is 257
        };

        public GeoEngine(ILogger<GeoEngine> logger)
        {
            _Logger = logger;

            InitializeData();

            FullCircleTimer.Interval = 5000;
            FullCircleTimer.Elapsed += FullCircle_Timer_Tick;

            EclipticTimer.Interval = 1000;
            EclipticTimer.Elapsed += EclipticTimer_Tick;
        }

        private bool CanCastInArea()
        {
            if (PL == null)
                return false;

            return !CityZoneIds.Contains(PL.Player.ZoneId);
        }

        public EngineAction Run(EliteAPI pl, EliteAPI monitored, GeoConfig Config)
        {
            EngineAction actionResult = new EngineAction
            {
                Target = Target.Me
            };

            try
            {
                PL = pl;
                Monitored = monitored;
                _Config = Config;

                if (!CanCastInArea())
                    return null;

                // ENTRUSTED INDI SPELL CASTING, WILL BE CAST SO LONG AS ENTRUST IS ACTIVE
                // StatusEffect 584 == Entrust
                if (PL.HasStatus((StatusEffect)584) && PL.Player.Status != (int)EntityStatus.Healing)
                {
                    actionResult = GetEntrustSpell();
                }

                // TODO: Fix up this logic, I think something was lost in the refactoring.
                // Need to see if there's a situation where both of these JA's would be activated for the cast.
                // For now the old logic seems to be use RA on it's own, or check for FC + Cast.
                else if (_Config.RadialArcanaEnabled && (PL.Player.MP <= _Config.RadialArcanaMP) && PL.AbilityAvailable(Ability.RadialArcana) && !PL.Player.Buffs.Contains((short)StatusEffect.Weakness))
                {
                    // Check if a pet is already active
                    if (PL.Player.Pet.HealthPercent >= 1 && PL.Player.Pet.Distance <= 9)
                    {
                        actionResult.JobAbility = Ability.RadialArcana;
                        return actionResult;
                    }
                    else if (PL.Player.Pet.HealthPercent >= 1 && PL.Player.Pet.Distance >= 9 && PL.AbilityAvailable(Ability.FullCircle))
                    {
                        actionResult.JobAbility = Ability.FullCircle;
                    }

                    actionResult.Spell = ReturnGeoSpell(_Config.RadialArcanaSpell, 2);
                }
                else if (_Config.FullCircleEnabled && PL.Player.Pet.HealthPercent != 0)
                {
                    // When out of range Distance is 59 Yalms regardless, Must be within 15 yalms to gain
                    // the effect

                    //Check if "pet" is active and out of range of the monitored player
                    if (PL.Player.Pet.HealthPercent >= 1)
                    {
                        if (_Config.FullCircleGeoTarget == true && _Config.LuopanSpellTarget != "")
                        {
                            ushort PetsIndex = PL.Player.PetIndex;

                            XiEntity PetsEntity = PL.Entity.GetEntity(PetsIndex);

                            int FullCircle_CharID = 0;

                            for (int x = 0; x < 2048; x++)
                            {
                                XiEntity entity = PL.Entity.GetEntity(x);

                                if (entity.Name != null && entity.Name.ToLower().Equals(_Config.LuopanSpellTarget.ToLower()))
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
                                    FullCircleTimer.Enabled = true;
                                }
                            }

                        }
                        else if (_Config.FullCircleGeoTarget == false && Monitored.Player.Status == (int)EntityStatus.Engaged)
                        {
                            ushort PetsIndex = PL.Player.PetIndex;

                            XiEntity PetsEntity = Monitored.Entity.GetEntity(PetsIndex);

                            if (PetsEntity.Distance >= 10)
                            {
                                FullCircleTimer.Enabled = true;
                            }
                        }
                    }
                }

                // CAST NON ENTRUSTED INDI SPELL
                else if (_Config.IndiSpellsEnabled && !PL.HasStatus(612) && PL.Player.Status != (int)EntityStatus.Healing && (CheckEngagedStatus() || !_Config.IndiWhenEngaged))
                {
                    string SpellCheckedResult = ReturnGeoSpell(_Config.IndiSpell, 1);

                    if (SpellCheckedResult == "SpellRecast")
                        return null;

                    if (SpellCheckedResult == "SpellError_Cancel" || SpellCheckedResult == "SpellNA" || SpellCheckedResult == "SpellUnknown")
                    {
                        _Config.IndiSpellsEnabled = false;
                        actionResult.Error = "An error has occurred with INDI spell casting, please report what spell was active at the time.";
                        return null;
                    }

                    actionResult.Spell = SpellCheckedResult;
                }

                // GEO SPELL CASTING
                else if (_Config.LuopanSpellsEnabled && (PL.Player.Pet.HealthPercent < 1) && CheckEngagedStatus())
                {
                    // Use BLAZE OF GLORY if ENABLED
                    if (_Config.BlazeOfGloryEnabled && PL.AbilityAvailable(Ability.BlazeOfGlory) && GEO_EnemyCheck())
                    {
                        actionResult.JobAbility = Ability.BlazeOfGlory;
                        return actionResult;
                    }

                    // Grab GEO spell name
                    string SpellCheckedResult = ReturnGeoSpell(_Config.GeoSpell, 2);

                    if (SpellCheckedResult == "SpellRecast" || SpellCheckedResult == "SpellUnknown" || SpellCheckedResult == "SpellNA")
                        return actionResult;

                    if (SpellCheckedResult == "SpellError_Cancel")
                    {
                        _Config.IndiSpellsEnabled = false;
                        actionResult.Error = "An error has occurred with GEO spell casting, please report what spell was active at the time.";
                    }

                    if (PL.Resources.GetSpell(SpellCheckedResult, 0).ValidTargets == 5)
                    {
                        if (!string.IsNullOrEmpty(_Config.LuopanSpellTarget))
                        {
                            actionResult.Target = _Config.LuopanSpellTarget;

                            if (PL.HasStatus(516)) // IF ECLIPTIC IS UP THEN ACTIVATE THE BOOL
                            {
                                EclipticStillUp = true;
                            }

                            actionResult.Spell = SpellCheckedResult;
                        }
                    }
                    else
                    {
                        // ENEMY BASED TARGET NEED TO ASSURE PLAYER IS ENGAGED
                        if (CheckEngagedStatus())
                        {
                            int GrabbedTargetID = GrabGEOTargetID();

                            if (GrabbedTargetID != 0)
                            {
                                PL.Target.SetTarget(GrabbedTargetID);

                                if (PL.HasStatus(516)) // IF ECLIPTIC IS UP THEN ACTIVATE THE BOOL
                                {
                                    EclipticStillUp = true;
                                }

                                actionResult.Target = "<t>";
                                actionResult.Spell = SpellCheckedResult;
                            }
                        }
                    }
                }

                // if we have no spells to cast, see if we can trigger a JA
                if (string.IsNullOrWhiteSpace(actionResult.Spell))
                {
                    actionResult = CheckForJobAbility();
                }
            }
            catch(Exception ex)
            {
                _Logger.LogError("Unexpected issue occurred while running geo engine", ex);
            }

            return actionResult;
        }

        private EngineAction CheckForJobAbility()
        {
            EngineAction actionResult = new EngineAction
            {
                Target = Target.Me
            };

            // If we're using Entrust, and it's available
            if (_Config.EntrustEnabled
                && !PL.HasStatus((StatusEffect)584) //entrust
                && CheckEngagedStatus()
                && PL.AbilityAvailable(Ability.Entrust))
            {
                if (VerifyEntrustTarget() != null)
                {
                    actionResult.JobAbility = Ability.Entrust;
                }

                return actionResult;
            }
            else if (_Config.DematerializeEnabled && CheckEngagedStatus() == true && PL.Player.Pet.HealthPercent >= 90 && PL.AbilityAvailable(Ability.Dematerialize))
            {
                actionResult.JobAbility = Ability.Dematerialize;
            }
            else if (_Config.EclipticAttritionEnabled && CheckEngagedStatus() == true && PL.Player.Pet.HealthPercent >= 90 && PL.AbilityAvailable(Ability.EclipticAttrition) && !PL.HasStatus(516) && EclipticStillUp != true)
            {
                actionResult.JobAbility = Ability.EclipticAttrition;
            }
            else if (_Config.LifeCycleEnabled && CheckEngagedStatus() == true && PL.Player.Pet.HealthPercent <= 30 && PL.Player.Pet.HealthPercent >= 5 && PL.Player.HPP >= 90 && PL.AbilityAvailable(Ability.LifeCycle))
            {
                actionResult.JobAbility = Ability.LifeCycle;
            }

            return actionResult;
        }

        private EngineAction GetEntrustSpell()
        {
            EngineAction actionResult = new EngineAction();

            string SpellCheckedResult = ReturnGeoSpell(_Config.EntrustSpell, 1);

            if (SpellCheckedResult == "SpellRecast" || SpellCheckedResult == "SpellUnknown")
                return actionResult;

            if (SpellCheckedResult == "SpellError_Cancel")
            {
                _Config.IndiSpellsEnabled = false;
                actionResult.Error = "An error has occurred with Entrusted INDI spell casting, please report what spell was active at the time.";
            }
            else
            {
                var target = _Config.EntrustSpellTarget;

                if (string.IsNullOrWhiteSpace(target))
                {
                    // use P1
                    target = PL.GetActivePartyMembers().FirstOrDefault(p => p.Index == 1)?.Name;
                }

                if (!string.IsNullOrWhiteSpace(VerifyEntrustTarget()))
                {
                    actionResult.Target = target;
                    actionResult.Spell = SpellCheckedResult;
                }
            }

            return actionResult;
        }

        private string VerifyEntrustTarget()
        {
            var target = _Config.EntrustSpellTarget;

            if (string.IsNullOrWhiteSpace(target))
            {
                // nothing in config, so let's default to P1
                target = PL.GetActivePartyMembers().FirstOrDefault(p => p.Index == 1)?.Name;
            }

            if (!string.IsNullOrEmpty(target))
            {
                PartyMember entrustTarget = PL.GetActivePartyMembers().FirstOrDefault(p => p.Name == target);

                // make sure target is in party
                if (entrustTarget == null)
                    return null;

                XiEntity entrustEntity = PL.Entity.GetEntity((int)entrustTarget.TargetIndex);

                // make sure target isn't dead and is in range
                if (entrustEntity.IsDead() || entrustEntity.Distance > 21)
                    return null;
            }

            return target;
        }

        private void InitializeData()
        {
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
                    // we can sometimes end up here if we try to cast the spell again too fast
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
                    // we can sometimes end up here if we try to cast the spell again too fast
                    return "SpellNA";
                }
            }
            else
            {
                return "SpellError_Cancel";
            }
        }

        private bool GEO_EnemyCheck()
        {
            if (PL == null) 
            { 
                return false;
            }

            // Grab GEO spell name
            string SpellCheckedResult = ReturnGeoSpell(_Config.GeoSpell, 2);

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
                    if (_Config.SpecifiedEngageTarget && !string.IsNullOrEmpty(_Config.LuopanSpellTarget))
                    {
                        for (int x = 0; x < 2048; x++)
                        {
                            XiEntity z = PL.Entity.GetEntity(x);
                            if (!string.IsNullOrEmpty(z.Name))
                            {
                                if (z.Name.ToLower() == _Config.LuopanSpellTarget.ToLower()) // A match was located so use this entity as a check.
                                {
                                    if (z.Status == (int)EntityStatus.Engaged)
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

                    return Monitored != null && Monitored.Player.Status == (int)EntityStatus.Engaged;
                }
            }
        }

        private bool CheckEngagedStatus()
        {
            if (PL == null)
            { 
                return false;
            }

            if (!_Config.GeoWhenEngaged)
            {
                return true;
            }
            
            if (_Config.SpecifiedEngageTarget && !string.IsNullOrEmpty(_Config.LuopanSpellTarget))
            {
                for (int x = 0; x < 2048; x++)
                {
                    XiEntity z = PL.Entity.GetEntity(x);
                    if (!string.IsNullOrEmpty(z.Name))
                    {
                        if (z.Name.ToLower() == _Config.LuopanSpellTarget.ToLower()) // A match was located so use this entity as a check.
                        {
                            return z.Status == (int)EntityStatus.Engaged;
                        }
                    }
                }

                return false;
            }

            return Monitored != null && Monitored.Player.Status == (int)EntityStatus.Engaged && PL.GetActivePartyMembers().FirstOrDefault(p => p.Name == Monitored.Player.Name) != null;
        }

        private int GrabGEOTargetID()
        {
            if (_Config.SpecifiedEngageTarget && !string.IsNullOrEmpty(_Config.LuopanSpellTarget))
            {
                for (int x = 0; x < 2048; x++)
                {
                    XiEntity z = PL.Entity.GetEntity(x);

                    if (z.Name != null && z.Name.ToLower() == _Config.LuopanSpellTarget.ToLower())
                    {
                        if (z.Status == (int)EntityStatus.Engaged)
                        {
                            return z.TargetingIndex;
                        }

                        return 0;
                    }
                }

                return 0;
            }
            else
            {
                if (Monitored != null && Monitored.Player.Status == (int)EntityStatus.Engaged)
                {
                    TargetInfo target = Monitored.Target.GetTargetInfo();
                    XiEntity entity = Monitored.Entity.GetEntity(Convert.ToInt32(target.TargetIndex));
                    return Convert.ToInt32(entity.TargetID);
                }

                return 0;
            }
        }

        private void FullCircle_Timer_Tick(object sender, EventArgs e)
        {
            if (PL == null)
            {
                return;
            }

            if (PL.Player.Pet.HealthPercent >= 1)
            {
                ushort PetsIndex = PL.Player.PetIndex;

                if (_Config.FullCircleGeoTarget && !string.IsNullOrEmpty(_Config.LuopanSpellTarget))
                {
                    XiEntity PetsEntity = PL.Entity.GetEntity(PetsIndex);

                    int FullCircle_CharID = 0;

                    for (int x = 0; x < 2048; x++)
                    {
                        XiEntity entity = PL.Entity.GetEntity(x);

                        if (entity.Name != null && entity.Name.ToLower().Equals(_Config.LuopanSpellTarget.ToLower()))
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
                else if (!_Config.FullCircleGeoTarget && Monitored.Player.Status == (int)EntityStatus.Engaged)
                {
                    string SpellCheckedResult = ReturnGeoSpell(_Config.GeoSpell, 2);

                    if (!_Config.FullCircleDisableEnemy || (_Config.FullCircleDisableEnemy && PL.Resources.GetSpell(SpellCheckedResult, 0).ValidTargets == 32))
                    {
                        XiEntity PetsEntity = Monitored.Entity.GetEntity(PetsIndex);

                        if (PetsEntity.Distance >= 10 && PetsEntity.Distance != 0 && PL.AbilityAvailable(Ability.FullCircle))
                        {
                            PL.ThirdParty.SendString("/ja \"Full Circle\" <me>");
                        }
                    }
                }
            }

            FullCircleTimer.Enabled = false;
        }

        private void EclipticTimer_Tick(object sender, EventArgs e)
        {
            if (PL == null)
            { 
                return; 
            }

            EclipticStillUp = PL.Player.Pet.HealthPercent >= 1;
        }
    }
}
