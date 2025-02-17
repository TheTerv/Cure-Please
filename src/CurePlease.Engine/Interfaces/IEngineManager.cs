﻿using CurePlease.Model;
using CurePlease.Model.Config;
using EliteMMO.API;
using System;
using System.Collections.Generic;

namespace CurePlease.Engine
{
    public interface IEngineManager
    {
        EngineAction RunGeoEngine(EliteAPI pl, GeoConfig config, string followName);

        void SetupFollow(EliteAPI pl, MySettings config);
        void StartFollowing();
        void StopFollowing();
        bool IsMoving();

        void SetupAddon(ThirdPartyTools thirdParty, MySettings config, Action<IAsyncResult> callback, string clientMode);
        void UnloadAddon(string clientMode);

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
