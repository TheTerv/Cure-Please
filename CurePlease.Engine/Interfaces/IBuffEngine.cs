using CurePlease.Model;
using EliteMMO.API;
using System.Collections.Generic;

namespace CurePlease.Engine
{
    public interface IBuffEngine
    {
        EngineAction Run(EliteAPI pL);

        void ToggleAutoBuff(string memberName, string spellName);

        bool BuffEnabled(string memberName, string spellName);

        void UpdateBuffs(string memberName, IEnumerable<short> buffs);
    }
}