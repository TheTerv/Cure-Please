using CurePlease.Model.Config;
using EliteMMO.API;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace CurePlease.Engine
{
    public class AddonEngine : IAddonEngine
    {
        private UdpClient _AddonClient;
        private bool _LUA_Plugin_Loaded;
        private int _Port;
        private IPAddress _IPAddress;
        private ThirdPartyTools _ThirdParty;
        private MySettings _Config;
        // temporary solution while we decomp UI from the action
        private Action<IAsyncResult> _OnAddonDataReceived;

        public void Setup(ThirdPartyTools thirdParty, MySettings config, Action<IAsyncResult> callback, string clientMode)
        {
            _ThirdParty = thirdParty;
            _Config = config;
            _OnAddonDataReceived = callback;
            _IPAddress = IPAddress.Parse(config.ipAddress);
            _Port = int.Parse(config.listeningPort);

            LoadAddon(clientMode);
            LoadClient();
        }

        public void UnloadAddon(string clientMode)
        {
            string preChar = GetPreChar(clientMode);

            _ThirdParty.SendString($"{preChar}lua unload CurePlease");

            if (_Config.enableHotKeys)
            {
                _ThirdParty.SendString($"{preChar}unbind ^!F1");
                _ThirdParty.SendString($"{preChar}unbind ^!F2");
                _ThirdParty.SendString($"{preChar}unbind ^!F3");
            }

            _LUA_Plugin_Loaded = false;

            CloseClient();
        }

        private void LoadAddon(string clientMode)
        {
            string preChar = GetPreChar(clientMode);

            if (!_LUA_Plugin_Loaded)
            {
                _ThirdParty.SendString($"{preChar}lua load CurePlease");
                Thread.Sleep(1500);

                _ThirdParty.SendString($"{preChar}cpaddon settings {_IPAddress} {_Port}");
                Thread.Sleep(100);

                _ThirdParty.SendString($"{preChar}cpaddon verify");

                if (_Config.enableHotKeys)
                {
                    _ThirdParty.SendString($"{preChar}bind ^!F1 cureplease toggle");
                    _ThirdParty.SendString($"{preChar}bind ^!F2 cureplease start");
                    _ThirdParty.SendString($"{preChar}bind ^!F3 cureplease pause");
                }

                _LUA_Plugin_Loaded = true;
            }
        }

        private static string GetPreChar(string clientMode)
        {
            return clientMode == "Ashita" 
                ? "/" 
                : clientMode == "Windower" 
                    ? "//" 
                    : throw new Exception("Somehow we don't know if this is Windower or Ashita when loading Addon!");
        }

        // If the port is already in use, lets try another port
        private void LoadClient()
        {
            string result = string.Empty;

            try
            {
                if (_AddonClient == null)
                { 
                    _AddonClient = new UdpClient(_Port);
                    _AddonClient.BeginReceive(new AsyncCallback(_OnAddonDataReceived), _AddonClient);
                }
            }
            catch (SocketException se)
            {
                result = $"Socket port #{_Port} was already in use. Automatically bumping the port # up and trying again";

                if (se.Message.Contains("Only one usage of each socket address"))
                {
                    _Port += 2;
                    LoadClient();
                }
            }

            //return $"LUA Addon loaded. ( {_IPAddress} - {_Port} )";
        }

        private void CloseClient()
        {
            _AddonClient.Close();
            _AddonClient = null;
        }
    }
}
