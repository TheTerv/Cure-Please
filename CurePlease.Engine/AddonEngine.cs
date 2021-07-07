using CurePlease.Model.Config;
using EliteMMO.API;
using System;
using System.Threading;

namespace CurePlease.Engine
{
    public class AddonEngine
    {
        private static bool LUA_Plugin_Loaded;

        public static void LoadAddonInClient(MySettings config, ThirdPartyTools thirdParty, string clientMode)
        {
            string preChar = GetPreChar(clientMode);

            if (config.EnableAddOn && !LUA_Plugin_Loaded)
            {
                thirdParty.SendString($"{preChar}lua load CurePlease");
                Thread.Sleep(1500);

                thirdParty.SendString($"{preChar}cpaddon settings " + config.ipAddress + " " + config.listeningPort);
                Thread.Sleep(100);

                thirdParty.SendString($"{preChar}cpaddon verify");

                if (config.enableHotKeys)
                {
                    thirdParty.SendString($"{preChar}bind ^!F1 cureplease toggle");
                    thirdParty.SendString($"{preChar}bind ^!F2 cureplease start");
                    thirdParty.SendString($"{preChar}bind ^!F3 cureplease pause");
                }

                LUA_Plugin_Loaded = true;
            }
        }

        public static void UnloadAddonInClient(bool enableHotKeys, ThirdPartyTools thirdParty, string clientMode)
        {
            string preChar = GetPreChar(clientMode);

            thirdParty.SendString($"{preChar}addon unload CurePlease");

            if (enableHotKeys)
            {
                thirdParty.SendString($"{preChar}unbind ^!F1");
                thirdParty.SendString($"{preChar}unbind ^!F2");
                thirdParty.SendString($"{preChar}unbind ^!F3");
            }

            LUA_Plugin_Loaded = false;
        }

        private static string GetPreChar(string clientMode)
        {
            return clientMode == "Ashita" 
                ? "/" 
                : clientMode == "Windower" 
                    ? "//" 
                    : throw new Exception("Somehow we don't know if this is Windower or Ashita when loading Addon!");
        }
    }
}
