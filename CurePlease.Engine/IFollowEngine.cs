using CurePlease.Model.Config;
using EliteMMO.API;

namespace CurePlease.Engine
{
    public interface IFollowEngine
    {
        void Start();
        void Setup(EliteAPI pL, EliteAPI monitored, MySettings config);
        void Stop();
        bool IsMoving();
    }
}
