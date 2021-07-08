using CurePlease.Model;
using CurePlease.Model.Config;
using EliteMMO.API;

namespace CurePlease.Engine
{
    public interface ICureEngine
    {
        EngineAction Run(EliteAPI pl, CureConfig Config, bool[] enabledMembers, bool[] highPriorityMembers);
    }
}