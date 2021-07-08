using CurePlease.Model;
using CurePlease.Model.Config;
using EliteMMO.API;
using System.Collections.Generic;

namespace CurePlease.Engine
{
    public interface IEngineManager
    {
        EngineAction RunGeoEngine(EliteAPI pl, EliteAPI monitored, GeoConfig config);

        void SetupFollow(EliteAPI pl, MySettings config);
        void StartFollowing();
        void StopFollowing();
        bool IsMoving();

        EngineAction RunBuffEngine(EliteAPI pl);
        void ToggleAutoBuff(string memberName, string spellName);
        bool BuffEnabled(string memberName, string spellName);
        void UpdateBuffs(string memberName, IEnumerable<short> buffs);

        EngineAction RunCureEngine(EliteAPI pl, CureConfig config, bool[] enabledMembers, bool[] highPriorityMembers);

        EngineAction RunDebuffEngine(EliteAPI pl, DebuffConfig config);
        void UpdateDebuffs(string memberName, IEnumerable<short> debuffs);
        void ToggleDebuffOnSpecifiedMember(string memberName);

        EngineAction RunPLEngine(EliteAPI pl, EliteAPI monitored, PLConfig config);

        EngineAction RunSongEngine(EliteAPI pl, EliteAPI monitored, SongConfig config);
    }
}
