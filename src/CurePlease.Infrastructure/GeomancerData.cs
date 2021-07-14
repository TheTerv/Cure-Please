using System.Collections.Generic;

namespace CurePlease.Infrastructure
{
    public class GeomancerData
    {
        public int GeoPosition { get; set; }

        public string IndiSpell { get; set; }

        public string GeoSpell { get; set; }

        public static List<GeomancerData> GeomancerInfo = new();

        static GeomancerData()
        {
            InitializeData();
        }

        private static void InitializeData()
        {
            #region Init Data

            int geo_position = 0;

            GeomancerInfo.Add(new GeomancerData
            {
                IndiSpell = "Indi-Voidance",
                GeoSpell = "Geo-Voidance",
                GeoPosition = geo_position,
            });
            geo_position++;

            GeomancerInfo.Add(new GeomancerData
            {
                IndiSpell = "Indi-Precision",
                GeoSpell = "Geo-Precision",
                GeoPosition = geo_position,
            });
            geo_position++;

            GeomancerInfo.Add(new GeomancerData
            {
                IndiSpell = "Indi-Regen",
                GeoSpell = "Geo-Regen",
                GeoPosition = geo_position,
            });
            geo_position++;

            GeomancerInfo.Add(new GeomancerData
            {
                IndiSpell = "Indi-Haste",
                GeoSpell = "Geo-Haste",
                GeoPosition = geo_position,
            });
            geo_position++;

            GeomancerInfo.Add(new GeomancerData
            {
                IndiSpell = "Indi-Attunement",
                GeoSpell = "Geo-Attunement",
                GeoPosition = geo_position,
            });
            geo_position++;

            GeomancerInfo.Add(new GeomancerData
            {
                IndiSpell = "Indi-Focus",
                GeoSpell = "Geo-Focus",
                GeoPosition = geo_position,
            });
            geo_position++;

            GeomancerInfo.Add(new GeomancerData
            {
                IndiSpell = "Indi-Barrier",
                GeoSpell = "Geo-Barrier",
                GeoPosition = geo_position,
            });
            geo_position++;

            GeomancerInfo.Add(new GeomancerData
            {
                IndiSpell = "Indi-Refresh",
                GeoSpell = "Geo-Refresh",
                GeoPosition = geo_position,
            });
            geo_position++;

            GeomancerInfo.Add(new GeomancerData
            {
                IndiSpell = "Indi-CHR",
                GeoSpell = "Geo-CHR",
                GeoPosition = geo_position,
            });
            geo_position++;

            GeomancerInfo.Add(new GeomancerData
            {
                IndiSpell = "Indi-MND",
                GeoSpell = "Geo-MND",
                GeoPosition = geo_position,
            });
            geo_position++;

            GeomancerInfo.Add(new GeomancerData
            {
                IndiSpell = "Indi-Fury",
                GeoSpell = "Geo-Fury",
                GeoPosition = geo_position,
            });
            geo_position++;

            GeomancerInfo.Add(new GeomancerData
            {
                IndiSpell = "Indi-INT",
                GeoSpell = "Geo-INT",
                GeoPosition = geo_position,
            });
            geo_position++;

            GeomancerInfo.Add(new GeomancerData
            {
                IndiSpell = "Indi-AGI",
                GeoSpell = "Geo-AGI",
                GeoPosition = geo_position,
            });
            geo_position++;

            GeomancerInfo.Add(new GeomancerData
            {
                IndiSpell = "Indi-Fend",
                GeoSpell = "Geo-Fend",
                GeoPosition = geo_position,
            });
            geo_position++;

            GeomancerInfo.Add(new GeomancerData
            {
                IndiSpell = "Indi-VIT",
                GeoSpell = "Geo-VIT",
                GeoPosition = geo_position,
            });
            geo_position++;

            GeomancerInfo.Add(new GeomancerData
            {
                IndiSpell = "Indi-DEX",
                GeoSpell = "Geo-DEX",
                GeoPosition = geo_position,
            });
            geo_position++;

            GeomancerInfo.Add(new GeomancerData
            {
                IndiSpell = "Indi-Acumen",
                GeoSpell = "Geo-Acumen",
                GeoPosition = geo_position,
            });
            geo_position++;

            GeomancerInfo.Add(new GeomancerData
            {
                IndiSpell = "Indi-STR",
                GeoSpell = "Geo-STR",
                GeoPosition = geo_position,
            });
            geo_position++;

            GeomancerInfo.Add(new GeomancerData
            {
                IndiSpell = "Indi-Poison",
                GeoSpell = "Geo-Poison",
                GeoPosition = geo_position,
            });
            geo_position++;

            GeomancerInfo.Add(new GeomancerData
            {
                IndiSpell = "Indi-Slow",
                GeoSpell = "Geo-Slow",
                GeoPosition = geo_position,
            });
            geo_position++;

            GeomancerInfo.Add(new GeomancerData
            {
                IndiSpell = "Indi-Torpor",
                GeoSpell = "Geo-Torpor",
                GeoPosition = geo_position,
            });
            geo_position++;

            GeomancerInfo.Add(new GeomancerData
            {
                IndiSpell = "Indi-Slip",
                GeoSpell = "Geo-Slip",
                GeoPosition = geo_position,
            });
            geo_position++;

            GeomancerInfo.Add(new GeomancerData
            {
                IndiSpell = "Indi-Languor",
                GeoSpell = "Geo-Languor",
                GeoPosition = geo_position,
            });
            geo_position++;

            GeomancerInfo.Add(new GeomancerData
            {
                IndiSpell = "Indi-Paralysis",
                GeoSpell = "Geo-Paralysis",
                GeoPosition = geo_position,
            });
            geo_position++;

            GeomancerInfo.Add(new GeomancerData
            {
                IndiSpell = "Indi-Vex",
                GeoSpell = "Geo-Vex",
                GeoPosition = geo_position,
            });
            geo_position++;

            GeomancerInfo.Add(new GeomancerData
            {
                IndiSpell = "Indi-Frailty",
                GeoSpell = "Geo-Frailty",
                GeoPosition = geo_position,
            });
            geo_position++;

            GeomancerInfo.Add(new GeomancerData
            {
                IndiSpell = "Indi-Wilt",
                GeoSpell = "Geo-Wilt",
                GeoPosition = geo_position,
            });
            geo_position++;

            GeomancerInfo.Add(new GeomancerData
            {
                IndiSpell = "Indi-Malaise",
                GeoSpell = "Geo-Malaise",
                GeoPosition = geo_position,
            });
            geo_position++;

            GeomancerInfo.Add(new GeomancerData
            {
                IndiSpell = "Indi-Gravity",
                GeoSpell = "Geo-Gravity",
                GeoPosition = geo_position,
            });
            geo_position++;

            GeomancerInfo.Add(new GeomancerData
            {
                IndiSpell = "Indi-Fade",
                GeoSpell = "Geo-Fade",
                GeoPosition = geo_position,
            });

            #endregion
        }
    }
}
