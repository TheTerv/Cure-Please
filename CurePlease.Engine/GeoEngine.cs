using CurePlease.Model;
using CurePlease.Model.Config;
using CurePlease.Model.Constants;
using CurePlease.Utilities;
using EliteMMO.API;
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

    public class GeoEngine
    {
        private GeoConfig _config;

        // GEO ENGAGED CHECK
        public bool targetEngaged = false;
        private bool EclipticStillUp = false;

        public List<GeoData> GeomancerInfo = new List<GeoData>();

        private EliteAPI PL;
        private EliteAPI Monitored;

        private Timer FullCircleTimer = new Timer();
        private Timer EclipticTimer = new Timer();

        public GeoEngine(EliteAPI pl, EliteAPI mon)
        {
            PL = pl;
            Monitored = mon;

            InitializeData();

            FullCircleTimer.Interval = 5000;
            FullCircleTimer.Elapsed += FullCircle_Timer_Tick;

            EclipticTimer.Interval = 1000;
            EclipticTimer.Elapsed += EclipticTimer_Tick;
        }

        public EngineAction Run(GeoConfig Config)
        {
            _config = Config;

            EngineAction actionResult = new EngineAction
            {
                Target = Target.Me
            };

            if(Config.EntrustEnabled && !PL.HasStatus((StatusEffect)584) && CheckEngagedStatus() == true && PL.AbilityAvailable(Ability.Entrust))
            {
                actionResult.JobAbility = Ability.Entrust;
                return actionResult;
            }
            else if (Config.DematerializeEnabled && CheckEngagedStatus() == true && PL.Player.Pet.HealthPercent >= 90 && PL.AbilityAvailable(Ability.Dematerialize))
            {
                actionResult.JobAbility = Ability.Dematerialize;
                return actionResult;
            }
            else if (Config.EclipticAttritionEnabled && CheckEngagedStatus() == true && PL.Player.Pet.HealthPercent >= 90 && PL.AbilityAvailable(Ability.EclipticAttrition) && !PL.HasStatus(516) && EclipticStillUp != true)
            {
                actionResult.JobAbility = Ability.EclipticAttrition;
                return actionResult;
            }
            else if (Config.LifeCycleEnabled && CheckEngagedStatus() == true && PL.Player.Pet.HealthPercent <= 30 && PL.Player.Pet.HealthPercent >= 5 && PL.Player.HPP >= 90 && PL.AbilityAvailable(Ability.LifeCycle))
            {
                actionResult.JobAbility = Ability.LifeCycle;
                return actionResult;
            }

            // TODO: Fix up this logic, I think something was lost in the refactoring.
            // Need to see if there's a situation where both of these JA's would be activated for the cast.
            // For now the old logic seems to be use RA on it's own, or check for FC + Cast.
            if (Config.RadialArcanaEnabled && (PL.Player.MP <= Config.RadialArcanaMP) && PL.AbilityAvailable(Ability.RadialArcana) && !PL.Player.Buffs.Contains((short)StatusEffect.Weakness))
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

                actionResult.Spell = ReturnGeoSpell(Config.RadialArcanaSpell, 2);
            }
            else if (Config.FullCircleEnabled)
            {
                // When out of range Distance is 59 Yalms regardless, Must be within 15 yalms to gain
                // the effect

                //Check if "pet" is active and out of range of the monitored player
                if (PL.Player.Pet.HealthPercent >= 1)
                {
                    if (Config.FullCircleGeoTarget == true && Config.LuopanSpellTarget != "")
                    {

                        ushort PetsIndex = PL.Player.PetIndex;

                        XiEntity PetsEntity = PL.Entity.GetEntity(PetsIndex);

                        int FullCircle_CharID = 0;

                        for (int x = 0; x < 2048; x++)
                        {
                            XiEntity entity = PL.Entity.GetEntity(x);

                            if (entity.Name != null && entity.Name.ToLower().Equals(Config.LuopanSpellTarget.ToLower()))
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
                    else if (Config.FullCircleGeoTarget == false && Monitored.Player.Status == 1)
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
            // ENTRUSTED INDI SPELL CASTING, WILL BE CAST SO LONG AS ENTRUST IS ACTIVE
            else if (Config.GeoSpellsEnabled && PL.HasStatus((StatusEffect)584) && PL.Player.Status != 33)
            {
                string SpellCheckedResult = ReturnGeoSpell(Config.EntrustSpell, 1);
                if (SpellCheckedResult == "SpellError_Cancel")
                {
                    Config.GeoSpellsEnabled = false;
                    actionResult.Error = "An error has occurred with Entrusted INDI spell casting, please report what spell was active at the time.";
                }
                else if (SpellCheckedResult == "SpellRecast" || SpellCheckedResult == "SpellUnknown")
                {
                }
                else
                {
                    actionResult.Target = string.IsNullOrEmpty(Config.EntrustSpellTarget) ? Monitored.Player.Name : Config.EntrustSpellTarget;
                    actionResult.Spell = SpellCheckedResult;
                }
            }
            // CAST NON ENTRUSTED INDI SPELL
            else if (Config.GeoSpellsEnabled && !PL.HasStatus(612) && PL.Player.Status != 33 && (CheckEngagedStatus() == true || !Config.IndiWhenEngaged))
            {
                string SpellCheckedResult = ReturnGeoSpell(Config.IndiSpell, 1);

                if (SpellCheckedResult == "SpellError_Cancel")
                {
                    Config.GeoSpellsEnabled = false;
                    actionResult.Error = "An error has occurred with INDI spell casting, please report what spell was active at the time.";
                }
                else if (SpellCheckedResult == "SpellRecast" || SpellCheckedResult == "SpellUnknown")
                {
                }
                else
                {
                    actionResult.Spell = SpellCheckedResult;
                }

            }
            // GEO SPELL CASTING 
            else if (Config.LuopanSpellsEnabled && (PL.Player.Pet.HealthPercent < 1) && (CheckEngagedStatus() == true))
            {
                // Use BLAZE OF GLORY if ENABLED
                if (Config.BlazeOfGloryEnabled && PL.AbilityAvailable(Ability.BlazeOfGlory) && CheckEngagedStatus() == true && GEO_EnemyCheck() == true)
                {
                    actionResult.JobAbility = Ability.BlazeOfGlory;
                    return actionResult;
                }

                // Grab GEO spell name
                string SpellCheckedResult = ReturnGeoSpell(Config.GeoSpell, 2);

                if (SpellCheckedResult == "SpellError_Cancel")
                {
                    Config.GeoSpellsEnabled = false;
                    actionResult.Error = "An error has occurred with GEO spell casting, please report what spell was active at the time.";
                }
                else if (SpellCheckedResult == "SpellRecast" || SpellCheckedResult == "SpellUnknown")
                {
                    // Do nothing and continue on with the program
                }
                else
                {
                    if (PL.Resources.GetSpell(SpellCheckedResult, 0).ValidTargets == 5)
                    { // PLAYER CHARACTER TARGET
                        actionResult.Target = string.IsNullOrEmpty(Config.LuopanSpellTarget) ? Monitored.Player.Name : Config.LuopanSpellTarget;

                        if (PL.HasStatus(516)) // IF ECLIPTIC IS UP THEN ACTIVATE THE BOOL
                        {
                            EclipticStillUp = true;
                        }

                        actionResult.Spell = SpellCheckedResult;

                    }
                    else
                    { // ENEMY BASED TARGET NEED TO ASSURE PLAYER IS ENGAGED
                        if (CheckEngagedStatus() == true)
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
            }

            return actionResult;
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

        private bool GEO_EnemyCheck()
        {
            if (Monitored == null || PL == null) { return false; }

            // Grab GEO spell name
            string SpellCheckedResult = ReturnGeoSpell(_config.GeoSpell, 2);

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
                    if (_config.SpecifiedEngageTarget && !string.IsNullOrEmpty(_config.LuopanSpellTarget))
                    {
                        for (int x = 0; x < 2048; x++)
                        {
                            XiEntity z = PL.Entity.GetEntity(x);
                            if (!string.IsNullOrEmpty(z.Name))
                            {
                                if (z.Name.ToLower() == _config.LuopanSpellTarget.ToLower()) // A match was located so use this entity as a check.
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

        private bool CheckEngagedStatus()
        {
            if (Monitored == null || PL == null) { return false; }


            if (!_config.GeoWhenEngaged)
            {
                return true;
            }
            else if (_config.SpecifiedEngageTarget && !string.IsNullOrEmpty(_config.LuopanSpellTarget))
            {
                for (int x = 0; x < 2048; x++)
                {
                    XiEntity z = PL.Entity.GetEntity(x);
                    if (!string.IsNullOrEmpty(z.Name))
                    {
                        if (z.Name.ToLower() == _config.LuopanSpellTarget.ToLower()) // A match was located so use this entity as a check.
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

        private int GrabGEOTargetID()
        {
            if (_config.SpecifiedEngageTarget && !string.IsNullOrEmpty(_config.LuopanSpellTarget))
            {
                for (int x = 0; x < 2048; x++)
                {
                    XiEntity z = PL.Entity.GetEntity(x);

                    if (z.Name != null && z.Name.ToLower() == _config.LuopanSpellTarget.ToLower())
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

            if (_config.SpecifiedEngageTarget && !string.IsNullOrEmpty(_config.LuopanSpellTarget))
            {
                checkedName = _config.LuopanSpellTarget;
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

        private void FullCircle_Timer_Tick(object sender, EventArgs e)
        {

            if (PL.Player.Pet.HealthPercent >= 1)
            {
                ushort PetsIndex = PL.Player.PetIndex;

                if (_config.FullCircleGeoTarget && !string.IsNullOrEmpty(_config.LuopanSpellTarget))
                {
                    XiEntity PetsEntity = PL.Entity.GetEntity(PetsIndex);

                    int FullCircle_CharID = 0;

                    for (int x = 0; x < 2048; x++)
                    {
                        XiEntity entity = PL.Entity.GetEntity(x);

                        if (entity.Name != null && entity.Name.ToLower().Equals(_config.LuopanSpellTarget.ToLower()))
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
                else if (!_config.FullCircleGeoTarget && Monitored.Player.Status == 1)
                {


                    string SpellCheckedResult = ReturnGeoSpell(_config.GeoSpell, 2);



                    if (!_config.FullCircleDisableEnemy || (_config.FullCircleDisableEnemy && PL.Resources.GetSpell(SpellCheckedResult, 0).ValidTargets == 32))
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
    }
}
