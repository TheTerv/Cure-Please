using CurePlease.Model;
using EliteMMO.API;

namespace CurePlease.Utilities
{
    public static class EliteAPIExtensions
    {
        public static bool HasMPFor(this EliteAPI api, string spell)
        {
            return api.Player.MP >= Data.SpellCosts[spell];
        }
    }
}
