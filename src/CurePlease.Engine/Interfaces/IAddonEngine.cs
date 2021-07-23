using CurePlease.Model.Config;
using EliteMMO.API;
using System;

namespace CurePlease.Engine
{
    public interface IAddonEngine
    {
        void Setup(ThirdPartyTools thirdParty, MySettings config, Action<IAsyncResult> callback, string clientMode);
        void UnloadAddon(string clientMode);
    }
}