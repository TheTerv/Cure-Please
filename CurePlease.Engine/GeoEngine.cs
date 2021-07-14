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
    public class GeoData : List<GeoData>
    {
        public int GeoPosition { get; set; }

        public string IndiSpell { get; set; }

        public string GeoSpell { get; set; }
    }

    public class GeoEngine : IGeoEngine
    {
        private readonly ILogger<GeoEngine> _Logger;

        private GeoConfig _Config;

        // GEO ENGAGED CHECK
        private bool EclipticStillUp = false;

        public List<GeoData> GeomancerInfo = new();

        private EliteAPI _Self;
        private XiEntity _GeoTarget;

        private readonly Timer FullCircleTimer = new();
        private readonly Timer EclipticTimer = new();

        private readonly List<int> CityZoneIds = new()
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

        private XiEntity GetWhoToCastOn(string followName)
        {
            int targetId = 0;

            if (!string.IsNullOrEmpty(followName))
            {
                targetId = _Self.GetEntityIdForPlayerByName(followName);
            }
            else if (_Self.GetActivePartyMembers().Any())
            {
                targetId = (int)_Self.GetActivePartyMembers().FirstOrDefault().TargetIndex;
            }

            if (targetId > 0)
            {
                return _Self.Entity.GetEntity(targetId);
            }

            return null;
        }

        public EngineAction Run(EliteAPI pl, GeoConfig config, string followName)
        {
            EngineAction actionResult = new()
            {
                Target = Target.Me
            };

            try
            {
                _Self = pl;
                _GeoTarget = GetWhoToCastOn(followName);
                _Config = config;

                if (!CanCastInArea())
                    return null;

                // ENTRUSTED INDI SPELL CASTING, WILL BE CAST SO LONG AS ENTRUST IS ACTIVE
                // StatusEffect 584 == Entrust
                if (_Self.HasStatus((StatusEffect)584) && _Self.Player.Status != (int)EntityStatus.Healing)
                {
                    actionResult = GetEntrustSpell();
                }

                // TODO: Fix up this logic, I think something was lost in the refactoring.
                // Need to see if there's a situation where both of these JA's would be activated for the cast.
                // For now the old logic seems to be use RA on it's own, or check for FC + Cast.
                else if (_Config.RadialArcanaEnabled && (_Self.Player.MP <= _Config.RadialArcanaMP) && _Self.AbilityAvailable(Ability.RadialArcana) && !_Self.Player.Buffs.Contains((short)StatusEffect.Weakness))
                {
                    // Check if a pet is already active
                    if (_Self.Player.Pet.HealthPercent >= 1 && _Self.Player.Pet.Distance <= 9)
                    {
                        actionResult.JobAbility = Ability.RadialArcana;
                        return actionResult;
                    }
                    else if (_Self.Player.Pet.HealthPercent >= 1 && _Self.Player.Pet.Distance >= 9 && _Self.AbilityAvailable(Ability.FullCircle))
                    {
                        actionResult.JobAbility = Ability.FullCircle;
                    }

                    actionResult.Spell = ReturnGeoSpell(_Config.RadialArcanaSpell, 2);
                }
                else if (_Config.FullCircleEnabled && _Self.Player.Pet.HealthPercent != 0)
                {
                    // When out of range Distance is 59 Yalms regardless, Must be within 15 yalms to gain
                    // the effect

                    //Check if "pet" is active and out of range of the monitored player
                    if (_Self.Player.Pet.HealthPercent >= 1)
                    {
                        if (_Config.FullCircleGeoTarget == true && _Config.LuopanSpellTarget != "")
                        {
                            ushort PetsIndex = _Self.Player.PetIndex;

                            XiEntity PetsEntity = _Self.Entity.GetEntity(PetsIndex);

                            int FullCircle_CharID = 0;

                            FullCircle_CharID = _Self.GetEntityIdForPlayerByName(_Config.LuopanSpellTarget);

                            if (FullCircle_CharID > 0)
                            {
                                XiEntity FullCircleEntity = _Self.Entity.GetEntity(FullCircle_CharID);

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
                        else if (_Config.FullCircleGeoTarget == false && _GeoTarget != null && _GeoTarget.Status == (int)EntityStatus.Engaged)
                        {
                            ushort PetsIndex = _Self.Player.PetIndex;

                            XiEntity PetsEntity = _Self.Entity.GetEntity(PetsIndex);

                            if (PetsEntity.Distance >= 10)
                            {
                                FullCircleTimer.Enabled = true;
                            }
                        }
                    }
                }

                // CAST NON ENTRUSTED INDI SPELL
                else if (_Config.IndiSpellsEnabled && !_Self.HasStatus(612) && _Self.Player.Status != (int)EntityStatus.Healing && (CheckEngagedStatus() || !_Config.IndiWhenEngaged))
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
                else if (_Config.LuopanSpellsEnabled && (_Self.Player.Pet.HealthPercent < 1) && CheckEngagedStatus())
                {
                    // Use BLAZE OF GLORY if ENABLED
                    if (_Config.BlazeOfGloryEnabled && _Self.AbilityAvailable(Ability.BlazeOfGlory) && GEO_EnemyCheck())
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

                    if (_Self.Resources.GetSpell(SpellCheckedResult, 0).ValidTargets == 5)
                    {
                        if (!string.IsNullOrEmpty(_Config.LuopanSpellTarget))
                        {
                            actionResult.Target = _Config.LuopanSpellTarget;

                            if (_Self.HasStatus(516)) // IF ECLIPTIC IS UP THEN ACTIVATE THE BOOL
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

                            if (GrabbedTargetID > 0)
                            {
                                _Self.Target.SetTarget(GrabbedTargetID);

                                if (_Self.HasStatus(516)) // IF ECLIPTIC IS UP THEN ACTIVATE THE BOOL
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

        private bool CanCastInArea()
        {
            if (_Self == null)
                return false;

            return !CityZoneIds.Contains(_Self.Player.ZoneId);
        }

        private EngineAction CheckForJobAbility()
        {
            EngineAction actionResult = new()
            {
                Target = Target.Me
            };

            // If we're using Entrust, and it's available
            if (_Config.EntrustEnabled
                && !_Self.HasStatus((StatusEffect)584) //entrust
                && CheckEngagedStatus()
                && _Self.AbilityAvailable(Ability.Entrust))
            {
                if (VerifyEntrustTarget() != null)
                {
                    actionResult.JobAbility = Ability.Entrust;
                }

                return actionResult;
            }
            else if (_Config.DematerializeEnabled && CheckEngagedStatus() == true && _Self.Player.Pet.HealthPercent >= 90 && _Self.AbilityAvailable(Ability.Dematerialize))
            {
                actionResult.JobAbility = Ability.Dematerialize;
            }
            else if (_Config.EclipticAttritionEnabled && CheckEngagedStatus() == true && _Self.Player.Pet.HealthPercent >= 90 && _Self.AbilityAvailable(Ability.EclipticAttrition) && !_Self.HasStatus(516) && EclipticStillUp != true)
            {
                actionResult.JobAbility = Ability.EclipticAttrition;
            }
            else if (_Config.LifeCycleEnabled && CheckEngagedStatus() == true && _Self.Player.Pet.HealthPercent <= 30 && _Self.Player.Pet.HealthPercent >= 5 && _Self.Player.HPP >= 90 && _Self.AbilityAvailable(Ability.LifeCycle))
            {
                actionResult.JobAbility = Ability.LifeCycle;
            }

            return actionResult;
        }

        private EngineAction GetEntrustSpell()
        {
            EngineAction actionResult = new();

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
                    target = _Self.GetActivePartyMembers().FirstOrDefault(p => p.Index == 1)?.Name;
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
                target = _Self.GetActivePartyMembers().FirstOrDefault(p => p.Index == 1)?.Name;
            }

            if (!string.IsNullOrEmpty(target))
            {
                PartyMember entrustTarget = _Self.GetActivePartyMembers().FirstOrDefault(p => p.Name == target);

                // make sure target is in party
                if (entrustTarget == null)
                    return null;

                XiEntity entrustEntity = _Self.Entity.GetEntity((int)entrustTarget.TargetIndex);

                // make sure target isn't dead and is in range
                if (entrustEntity.IsDead() || entrustEntity.Distance > 21)
                    return null;
            }

            return target;
        }

        private void InitializeData()
        {
            #region Init Data

            int geo_position = 0;

            GeomancerInfo.Add(new GeoData
            {
                IndiSpell = "Indi-Voidance",
                GeoSpell = "Geo-Voidance",
                GeoPosition = geo_position,
            });
            geo_position++;

            GeomancerInfo.Add(new GeoData
            {
                IndiSpell = "Indi-Precision",
                GeoSpell = "Geo-Precision",
                GeoPosition = geo_position,
            });
            geo_position++;

            GeomancerInfo.Add(new GeoData
            {
                IndiSpell = "Indi-Regen",
                GeoSpell = "Geo-Regen",
                GeoPosition = geo_position,
            });
            geo_position++;

            GeomancerInfo.Add(new GeoData
            {
                IndiSpell = "Indi-Haste",
                GeoSpell = "Geo-Haste",
                GeoPosition = geo_position,
            });
            geo_position++;

            GeomancerInfo.Add(new GeoData
            {
                IndiSpell = "Indi-Attunement",
                GeoSpell = "Geo-Attunement",
                GeoPosition = geo_position,
            });
            geo_position++;

            GeomancerInfo.Add(new GeoData
            {
                IndiSpell = "Indi-Focus",
                GeoSpell = "Geo-Focus",
                GeoPosition = geo_position,
            });
            geo_position++;

            GeomancerInfo.Add(new GeoData
            {
                IndiSpell = "Indi-Barrier",
                GeoSpell = "Geo-Barrier",
                GeoPosition = geo_position,
            });
            geo_position++;

            GeomancerInfo.Add(new GeoData
            {
                IndiSpell = "Indi-Refresh",
                GeoSpell = "Geo-Refresh",
                GeoPosition = geo_position,
            });
            geo_position++;

            GeomancerInfo.Add(new GeoData
            {
                IndiSpell = "Indi-CHR",
                GeoSpell = "Geo-CHR",
                GeoPosition = geo_position,
            });
            geo_position++;

            GeomancerInfo.Add(new GeoData
            {
                IndiSpell = "Indi-MND",
                GeoSpell = "Geo-MND",
                GeoPosition = geo_position,
            });
            geo_position++;

            GeomancerInfo.Add(new GeoData
            {
                IndiSpell = "Indi-Fury",
                GeoSpell = "Geo-Fury",
                GeoPosition = geo_position,
            });
            geo_position++;

            GeomancerInfo.Add(new GeoData
            {
                IndiSpell = "Indi-INT",
                GeoSpell = "Geo-INT",
                GeoPosition = geo_position,
            });
            geo_position++;

            GeomancerInfo.Add(new GeoData
            {
                IndiSpell = "Indi-AGI",
                GeoSpell = "Geo-AGI",
                GeoPosition = geo_position,
            });
            geo_position++;

            GeomancerInfo.Add(new GeoData
            {
                IndiSpell = "Indi-Fend",
                GeoSpell = "Geo-Fend",
                GeoPosition = geo_position,
            });
            geo_position++;

            GeomancerInfo.Add(new GeoData
            {
                IndiSpell = "Indi-VIT",
                GeoSpell = "Geo-VIT",
                GeoPosition = geo_position,
            });
            geo_position++;

            GeomancerInfo.Add(new GeoData
            {
                IndiSpell = "Indi-DEX",
                GeoSpell = "Geo-DEX",
                GeoPosition = geo_position,
            });
            geo_position++;

            GeomancerInfo.Add(new GeoData
            {
                IndiSpell = "Indi-Acumen",
                GeoSpell = "Geo-Acumen",
                GeoPosition = geo_position,
            });
            geo_position++;

            GeomancerInfo.Add(new GeoData
            {
                IndiSpell = "Indi-STR",
                GeoSpell = "Geo-STR",
                GeoPosition = geo_position,
            });
            geo_position++;

            GeomancerInfo.Add(new GeoData
            {
                IndiSpell = "Indi-Poison",
                GeoSpell = "Geo-Poison",
                GeoPosition = geo_position,
            });
            geo_position++;

            GeomancerInfo.Add(new GeoData
            {
                IndiSpell = "Indi-Slow",
                GeoSpell = "Geo-Slow",
                GeoPosition = geo_position,
            });
            geo_position++;

            GeomancerInfo.Add(new GeoData
            {
                IndiSpell = "Indi-Torpor",
                GeoSpell = "Geo-Torpor",
                GeoPosition = geo_position,
            });
            geo_position++;

            GeomancerInfo.Add(new GeoData
            {
                IndiSpell = "Indi-Slip",
                GeoSpell = "Geo-Slip",
                GeoPosition = geo_position,
            });
            geo_position++;

            GeomancerInfo.Add(new GeoData
            {
                IndiSpell = "Indi-Languor",
                GeoSpell = "Geo-Languor",
                GeoPosition = geo_position,
            });
            geo_position++;

            GeomancerInfo.Add(new GeoData
            {
                IndiSpell = "Indi-Paralysis",
                GeoSpell = "Geo-Paralysis",
                GeoPosition = geo_position,
            });
            geo_position++;

            GeomancerInfo.Add(new GeoData
            {
                IndiSpell = "Indi-Vex",
                GeoSpell = "Geo-Vex",
                GeoPosition = geo_position,
            });
            geo_position++;

            GeomancerInfo.Add(new GeoData
            {
                IndiSpell = "Indi-Frailty",
                GeoSpell = "Geo-Frailty",
                GeoPosition = geo_position,
            });
            geo_position++;

            GeomancerInfo.Add(new GeoData
            {
                IndiSpell = "Indi-Wilt",
                GeoSpell = "Geo-Wilt",
                GeoPosition = geo_position,
            });
            geo_position++;

            GeomancerInfo.Add(new GeoData
            {
                IndiSpell = "Indi-Malaise",
                GeoSpell = "Geo-Malaise",
                GeoPosition = geo_position,
            });
            geo_position++;

            GeomancerInfo.Add(new GeoData
            {
                IndiSpell = "Indi-Gravity",
                GeoSpell = "Geo-Gravity",
                GeoPosition = geo_position,
            });
            geo_position++;

            GeomancerInfo.Add(new GeoData
            {
                IndiSpell = "Indi-Fade",
                GeoSpell = "Geo-Fade",
                GeoPosition = geo_position,
            });

            #endregion
        }

        private string ReturnGeoSpell(int GEOSpell_ID, int GeoSpell_Type)
        {
            // GRAB THE SPELL FROM THE CUSTOM LIST
            GeoData GeoSpell = GeomancerInfo.Where(c => c.GeoPosition == GEOSpell_ID).FirstOrDefault();

            if (GeoSpell_Type == 1)
            {
                var apiSpell = _Self.Resources.GetSpell(GeoSpell.IndiSpell, 0);
                if (_Self.SpellAvailable(apiSpell.Name[0]))
                {
                    return GeoSpell.IndiSpell;
                }
                else
                {
                    // we can sometimes end up here if we try to cast the spell again too fast
                    return "SpellNA";
                }
            }
            else if (GeoSpell_Type == 2)
            {
                var apiSpell = _Self.Resources.GetSpell(GeoSpell.GeoSpell, 0);

                if (_Self.SpellAvailable(apiSpell.Name[0]))
                {
                    return GeoSpell.GeoSpell;
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
            if (_Self == null) 
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
                if (_Self.Resources.GetSpell(SpellCheckedResult, 0).ValidTargets == 5)
                {
                    return true; // SPELL TARGET IS PLAYER THEREFORE ONLY THE DEFAULT CHECK IS REQUIRED SO JUST RETURN TRUE TO VOID THIS CHECK
                }
                else
                {
                    if (_Config.SpecifiedEngageTarget && !string.IsNullOrEmpty(_Config.LuopanSpellTarget))
                    {
                        var targetId = _Self.GetEntityIdForPlayerByName(_Config.LuopanSpellTarget);
                        var luopan = _Self.Entity.GetEntity(targetId);

                        return luopan != null && luopan.Status == (int)EntityStatus.Engaged;
                    }

                    return _GeoTarget != null && _GeoTarget.Status == (int)EntityStatus.Engaged;
                }
            }
        }

        private bool CheckEngagedStatus()
        {
            if (_Self == null)
            { 
                return false;
            }

            if (!_Config.GeoWhenEngaged)
            {
                return true;
            }
            
            if (_Config.SpecifiedEngageTarget && !string.IsNullOrEmpty(_Config.LuopanSpellTarget))
            {
                var targetId = _Self.GetEntityIdForPlayerByName(_Config.LuopanSpellTarget);
                var luopan = _Self.Entity.GetEntity(targetId);

                return luopan.Status == (int)EntityStatus.Engaged;
            }

            return _GeoTarget != null && _GeoTarget.Status == (int)EntityStatus.Engaged && _Self.GetActivePartyMembers().FirstOrDefault(p => p.Name == _GeoTarget.Name) != null;
        }

        private int GrabGEOTargetID()
        {
            if (_Config.SpecifiedEngageTarget && !string.IsNullOrEmpty(_Config.LuopanSpellTarget))
            {
                return _Self.GetEntityIdForPlayerByName(_Config.LuopanSpellTarget);
            }
            else
            {
                if (_GeoTarget != null && _GeoTarget.Status == (int)EntityStatus.Engaged)
                {
                    XiEntity entity = _Self.Entity.GetEntity(Convert.ToInt32(_GeoTarget.TargetingIndex));
                    return Convert.ToInt32(entity.TargetID);
                }

                return 0;
            }
        }

        private void FullCircle_Timer_Tick(object sender, EventArgs e)
        {
            if (_Self == null)
            {
                return;
            }

            if (_Self.Player.Pet.HealthPercent >= 1)
            {
                ushort PetsIndex = _Self.Player.PetIndex;

                if (_Config.FullCircleGeoTarget && !string.IsNullOrEmpty(_Config.LuopanSpellTarget))
                {
                    XiEntity PetsEntity = _Self.Entity.GetEntity(PetsIndex);

                    int FullCircle_CharID = 0;

                    var targetId = _Self.GetEntityIdForPlayerByName(_Config.LuopanSpellTarget);
                    var luopan = _Self.Entity.GetEntity(targetId);
                    FullCircle_CharID = (int)luopan.TargetID;

                    if (FullCircle_CharID != 0)
                    {
                        XiEntity FullCircleEntity = _Self.Entity.GetEntity(FullCircle_CharID);

                        float fX = PetsEntity.X - FullCircleEntity.X;
                        float fY = PetsEntity.Y - FullCircleEntity.Y;
                        float fZ = PetsEntity.Z - FullCircleEntity.Z;

                        float generatedDistance = (float)Math.Sqrt((fX * fX) + (fY * fY) + (fZ * fZ));

                        if (generatedDistance >= 10)
                        {
                            _Self.ThirdParty.SendString("/ja \"Full Circle\" <me>");
                        }
                    }
                }
                else if (!_Config.FullCircleGeoTarget && _GeoTarget.Status == (int)EntityStatus.Engaged)
                {
                    string SpellCheckedResult = ReturnGeoSpell(_Config.GeoSpell, 2);

                    if (!_Config.FullCircleDisableEnemy || (_Config.FullCircleDisableEnemy && _Self.Resources.GetSpell(SpellCheckedResult, 0).ValidTargets == 32))
                    {
                        XiEntity PetsEntity = _Self.Entity.GetEntity(PetsIndex);

                        if (PetsEntity.Distance >= 10 && PetsEntity.Distance != 0 && _Self.AbilityAvailable(Ability.FullCircle))
                        {
                            _Self.ThirdParty.SendString("/ja \"Full Circle\" <me>");
                        }
                    }
                }
            }

            FullCircleTimer.Enabled = false;
        }

        private void EclipticTimer_Tick(object sender, EventArgs e)
        {
            if (_Self == null)
            { 
                return; 
            }

            EclipticStillUp = _Self.Player.Pet.HealthPercent >= 1;
        }
    }
}
