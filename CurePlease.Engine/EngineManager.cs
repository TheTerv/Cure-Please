using CurePlease.Model;
using CurePlease.Model.Config;
using EliteMMO.API;
using System.Collections.Generic;

namespace CurePlease.Engine
{
    public class EngineManager : IEngineManager
    {
        private readonly IFollowEngine _FollowEngine;
        private readonly IGeoEngine _GeoEngine;
        private readonly IBuffEngine _BuffEngine;
        private readonly ICureEngine _CureEngine;
        private readonly IDebuffEngine _DebuffEngine;
        private readonly IPLEngine _PLEngine;
        private readonly ISongEngine _SongEngine;

        public EngineManager(IFollowEngine followEngine, IGeoEngine geoEngine, IBuffEngine buffEngine, ICureEngine cureEngine, IDebuffEngine debuffEngine, IPLEngine plEngine, ISongEngine songEngine)
        {
            _FollowEngine = followEngine;
            _GeoEngine = geoEngine;
            _BuffEngine = buffEngine;
            _CureEngine = cureEngine;
            _DebuffEngine = debuffEngine;
            _PLEngine = plEngine;
            _SongEngine = songEngine;
        }

        public EngineAction RunGeoEngine(EliteAPI pl, EliteAPI monitored, GeoConfig config)
        {
            return _GeoEngine.Run(pl, monitored, config);
        }

        public EngineAction RunBuffEngine(EliteAPI pl)
        {
            return _BuffEngine.Run(pl);
        }

        public void StartFollowing()
        {
            _FollowEngine.Start();
        }

        public void StopFollowing()
        {
            _FollowEngine.Stop();
        }

        public void SetupFollow(EliteAPI pl, MySettings config)
        {
            _FollowEngine.Setup(pl, config);
        }

        public bool IsMoving()
        {
            return _FollowEngine.IsMoving();
        }

        public void ToggleAutoBuff(string memberName, string spellName)
        {
            _BuffEngine.ToggleAutoBuff(memberName, spellName);
        }

        public bool BuffEnabled(string memberName, string spellName)
        {
            return _BuffEngine.BuffEnabled(memberName, spellName);
        }

        public void UpdateBuffs(string memberName, IEnumerable<short> buffs)
        {
            _BuffEngine.UpdateBuffs(memberName, buffs);
        }

        public EngineAction RunCureEngine(EliteAPI pl, CureConfig config, bool[] enabledMembers, bool[] highPriorityMembers)
        {
            return _CureEngine.Run(pl, config, enabledMembers, highPriorityMembers);
        }

        public EngineAction RunDebuffEngine(EliteAPI pl, DebuffConfig config)
        {
            return _DebuffEngine.Run(pl, config);
        }

        public void UpdateDebuffs(string memberName, IEnumerable<short> debuffs)
        {
            _DebuffEngine.UpdateDebuffs(memberName, debuffs);
        }

        public void ToggleDebuffOnSpecifiedMember(string memberName)
        {
            _DebuffEngine.ToggleSpecifiedMember(memberName);
        }

        public EngineAction RunPLEngine(EliteAPI pl, EliteAPI monitored, PLConfig config)
        {
            return _PLEngine.Run(pl, monitored, config);
        }

        public EngineAction RunSongEngine(EliteAPI pl, EliteAPI monitored, SongConfig config)
        {
            return _SongEngine.Run(pl, monitored, config);
        }
    }
}
