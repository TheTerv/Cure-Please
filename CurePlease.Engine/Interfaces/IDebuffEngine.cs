using CurePlease.Model;
using CurePlease.Model.Config;
using EliteMMO.API;
using System.Collections.Generic;

namespace CurePlease.Engine
{
    public interface IDebuffEngine
    {
        EngineAction Run(EliteAPI pl, DebuffConfig Config);

        void UpdateDebuffs(string memberName, IEnumerable<short> debuffs);

        void ToggleSpecifiedMember(string memberName);
    }
}