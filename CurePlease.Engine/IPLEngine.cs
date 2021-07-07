using CurePlease.Model;
using CurePlease.Model.Config;
using EliteMMO.API;

namespace CurePlease.Engine
{
    public interface IPLEngine
    {
        EngineAction Run(EliteAPI pl, EliteAPI monitored, PLConfig Config);
    }
}