using CurePlease.Model;
using CurePlease.Model.Config;
using EliteMMO.API;

namespace CurePlease.Engine
{
    public interface ISongEngine
    {
        EngineAction Run(EliteAPI pl, EliteAPI monitored, SongConfig Config);
    }
}