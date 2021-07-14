using CurePlease.Infrastructure;
using CurePlease.Model;
using CurePlease.Model.Config;
using CurePlease.Model.Constants;
using CurePlease.Utilities;
using EliteMMO.API;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Timers;
using static EliteMMO.API.EliteAPI;

namespace CurePlease.Engine
{
    public class GeoEngine : IGeoEngine
    {
        private readonly ILogger<GeoEngine> _Logger;

        private GeoConfig _Config;
        private EliteAPI _Self;
        private XiEntity _GeoTarget;

        // GEO ENGAGED CHECK
        private bool EclipticStillUp;
        private readonly Timer EclipticTimer = new();

        private bool _Running;

        public GeoEngine(ILogger<GeoEngine> logger)
        {
            _Logger = logger;

            EclipticTimer.Interval = 1000;
            EclipticTimer.Elapsed += EclipticTimer_Tick;
        }

        public EngineAction Run(EliteAPI pl, GeoConfig config, string followName)
        {
            if (_Running)
                return null;

            _Running = true;

            EngineAction actionResult = new()
            {
                Target = Target.Me
            };

            try
            {
                _Self = pl;
                _Config = config;
                _GeoTarget = GetWhoToCastGeoSpellsOn(followName);

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
                else if (_Self.AbilityAvailable(Ability.FullCircle) && _Config.FullCircleEnabled && _Self.Player.Pet.HealthPercent > 0)
                {
                    // When out of range Distance is 59 Yalms regardless, Must be within 15 yalms to gain
                    // the effect
                    ushort luopanTargetId = _Self.Player.PetIndex;
                    XiEntity luopan = _Self.Entity.GetEntity(luopanTargetId);

                    if (_Config.FullCircleGeoTarget && !string.IsNullOrWhiteSpace(_Config.LuopanSpellTarget))
                    {
                        int FullCircle_CharID = 0;

                        FullCircle_CharID = _Self.GetEntityIdForPlayerByName(_Config.LuopanSpellTarget);

                        if (FullCircle_CharID > 0)
                        {
                            XiEntity FullCircleEntity = _Self.Entity.GetEntity(FullCircle_CharID);

                            float fX = luopan.X - FullCircleEntity.X;
                            float fY = luopan.Y - FullCircleEntity.Y;
                            float fZ = luopan.Z - FullCircleEntity.Z;

                            float generatedDistance = (float)Math.Sqrt((fX * fX) + (fY * fY) + (fZ * fZ));

                            if (generatedDistance >= 10)
                            {
                                _Self.ThirdParty.SendString("/ja \"Full Circle\" <me>");
                            }
                        }
                    }
                    else if (!_Config.FullCircleGeoTarget && _GeoTarget != null && _GeoTarget.Status == (int)EntityStatus.Engaged)
                    {
                        string SpellCheckedResult = ReturnGeoSpell(_Config.GeoSpell, 2);

                        if (!_Config.FullCircleDisableEnemy || (_Config.FullCircleDisableEnemy && _Self.Resources.GetSpell(SpellCheckedResult, 0).ValidTargets == 32))
                        {
                            if (luopan.Distance >= 10 && luopan.Distance != 0)
                            {
                                _Self.ThirdParty.SendString("/ja \"Full Circle\" <me>");
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
            finally
            {
                _Running = false;
            }

            return actionResult;
        }

        private XiEntity GetWhoToCastGeoSpellsOn(string followName)
        {
            int targetId = 0;

            if (!string.IsNullOrEmpty(_Config.LuopanSpellTarget))
            {
                targetId = _Self.GetEntityIdForPlayerByName(_Config.LuopanSpellTarget);
            }
            else if (!string.IsNullOrEmpty(followName))
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

        private bool CanCastInArea()
        {
            if (_Self == null)
                return false;

            return !CityZones.CityZoneIds.Contains(_Self.Player.ZoneId);
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
            else if (_Config.EclipticAttritionEnabled && CheckEngagedStatus() == true && _Self.Player.Pet.HealthPercent >= 90 && _Self.AbilityAvailable(Ability.EclipticAttrition) && !_Self.HasStatus(516) && !EclipticStillUp)
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

        private string ReturnGeoSpell(int GEOSpell_ID, int GeoSpell_Type)
        {
            // GRAB THE SPELL FROM THE CUSTOM LIST
            GeomancerData GeoSpell = GeomancerData.GeomancerInfo.Where(c => c.GeoPosition == GEOSpell_ID).FirstOrDefault();

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
