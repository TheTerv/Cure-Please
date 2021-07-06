using CurePlease.Model.Config;
using EliteMMO.API;
using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace CurePlease.Engine
{
    public class AddonEngine
    {
        private static bool LUA_Plugin_Loaded;

        private static MySettings _Config;

        public static void LoadAddonInClient(MySettings config, ThirdPartyTools thirdParty, string clientMode)
        {
            _Config = config;

            if (_Config.EnableAddOn && !LUA_Plugin_Loaded)
            {
                if (clientMode == "Windower")
                {
                    thirdParty.SendString("//lua load CurePlease");
                    Thread.Sleep(1500);
                    thirdParty.SendString("//cpaddon settings " + _Config.ipAddress + " " + _Config.listeningPort);
                    Thread.Sleep(100);
                    thirdParty.SendString("//cpaddon verify");

                    if (_Config.enableHotKeys)
                    {
                        thirdParty.SendString("//bind ^!F1 cureplease toggle");
                        thirdParty.SendString("//bind ^!F2 cureplease start");
                        thirdParty.SendString("//bind ^!F3 cureplease pause");
                    }
                }
                else if (clientMode == "Ashita")
                {
                    thirdParty.SendString("/addon load CurePlease");
                    Thread.Sleep(1500);
                    thirdParty.SendString("/cpaddon settings " + _Config.ipAddress + " " + _Config.listeningPort);
                    Thread.Sleep(100);
                    thirdParty.SendString("/cpaddon verify");

                    if (_Config.enableHotKeys)
                    {
                        thirdParty.SendString("/bind ^!F1 /cureplease toggle");
                        thirdParty.SendString("/bind ^!F2 /cureplease start");
                        thirdParty.SendString("/bind ^!F3 /cureplease pause");
                    }
                }

                LUA_Plugin_Loaded = true;
            }
        }

        public static void UnloadAddonInClient(MySettings config, ThirdPartyTools thirdParty, string clientMode)
        {
            _Config = config;

            if (clientMode == "Ashita")
            {
                thirdParty.SendString("/addon unload CurePlease");
                if (_Config.enableHotKeys)
                {
                    thirdParty.SendString("/unbind ^!F1");
                    thirdParty.SendString("/unbind ^!F2");
                    thirdParty.SendString("/unbind ^!F3");
                }
            }
            else if (clientMode == "Windower")
            {
                thirdParty.SendString("//lua unload CurePlease");

                if (_Config.enableHotKeys)
                {
                    thirdParty.SendString("//unbind ^!F1");
                    thirdParty.SendString("//unbind ^!F2");
                    thirdParty.SendString("//unbind ^!F3");
                }

            }
        }
    }
}
