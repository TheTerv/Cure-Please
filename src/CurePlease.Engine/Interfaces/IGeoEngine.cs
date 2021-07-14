using CurePlease.Model;
using CurePlease.Model.Config;
using EliteMMO.API;

namespace CurePlease.Engine
{
    public interface IGeoEngine
    {
        EngineAction Run(EliteAPI pl, GeoConfig Config, string followName);
    }
}
