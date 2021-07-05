using EliteMMO.API;
using static EliteMMO.API.EliteAPI;

namespace CurePlease.Utilities
{
    public static class XiEntityExtensions
    {
        public static bool IsDead(this XiEntity entity)
        {
            return entity.Status == (int)EntityStatus.Dead || entity.Status == (int)EntityStatus.DeadEngaged;
        }
    }
}
