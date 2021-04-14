
namespace CurePlease.Model.Config
{
    public class GeoConfig
    {
        public bool LuopanSpellsEnabled { get; set; }
        public bool GeoSpellsEnabled { get; set; }
        public bool GeoWhenEngaged { get; set; }
        public bool EntrustEnabled { get; set; }

        public bool RadialArcanaEnabled { get; set; }
        public decimal RadialArcanaMP { get; set; }
        public int RadialArcanaSpell { get; set; }
        public int GeoSpell { get; set; }
        public int EntrustSpell { get; set; }
        public string EntrustSpellTarget { get; set; }
        public int IndiSpell { get; set; }
        public bool FullCircleEnabled { get; set; }
        public bool FullCircleGeoTarget { get; set; }
        public bool FullCircleDisableEnemy { get; set; }
        public string LuopanSpellTarget { get; set; }

        public bool SpecifiedEngageTarget { get; set; }
        public bool IndiWhenEngaged { get; set; }
        public bool DematerializeEnabled { get; set; }
        public bool EclipticAttritionEnabled { get; set; }
        public bool LifeCycleEnabled { get; set; }
        public bool BlazeOfGloryEnabled { get; set; }
    }
}
