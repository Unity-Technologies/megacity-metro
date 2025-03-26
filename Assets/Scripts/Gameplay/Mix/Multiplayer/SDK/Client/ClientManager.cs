using System;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Multiplayer;
using Unity.Services.MultiplayerSDK.Utils;
using UnityEngine;
using Unity.Networking.Transport;
using Unity.Services.Samples;

namespace Unity.Services.MultiplayerSDK.Client
{
    public enum GameState
    {
        Boot,
        Login,
        Menu,
        Executing,
        Game
    }

    public class ClientManager
    {
        private const string k_SessionType = "MegacityMetroSession";
        private const int k_MaxPlayers = 150; // TODO: Pull the same values as the server

        private ISession _session;
        private CustomEntitiesNetworkHandler m_Connector;
        public NetworkEndpoint ConnectionEndpoint => m_Connector.ConnectionEndpoint;

        public GameState GameState { get; private set; } = GameState.Boot;
        
        private string _listenIp;
        private string _publishIp;
        private int _port;

        public ClientManager()
        {
            m_Connector = new CustomEntitiesNetworkHandler();
            _listenIp = "0.0.0.0";
            _publishIp = GetLocalIPAddress();
            _port = 0;
        }

        public void Start()
        {
            GameState = GameState.Login;
        }

        public async Task LoginAsync(PlayerProfile player)
        {
            var profile = player.UASId;
            GameState = GameState.Executing;

            try
            {
                if (AuthenticationService.Instance.IsSignedIn)
                {
                    Debug.Log($"[ClientManager] Already signed in as {player}");
                }
                else
                {
                    if (!string.IsNullOrEmpty(profile))
                    {
                        AuthenticationService.Instance.SwitchProfile(profile);
                    }

                    await AuthenticationService.Instance.SignInAnonymouslyAsync();

                    Debug.Log($"[ClientManager] Signed into Unity Services as {player}");
                }

                SetupMenu();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                GameState = GameState.Boot;
            }
        }

        public async Task JoinSessionAsync(string sessionId)
        {
            GameState = GameState.Executing;

            try
            {
                _session = await MultiplayerService.Instance.JoinSessionByIdAsync(sessionId, GetJoinSessionOptions());
                SetupGame();
            }
            catch (SessionException e)
            {
                Debug.LogError($"Join session by Id failed. {e.Error}: {e.Message}");
            }
        }

        public async Task MatchmakeAsync()
        {
            GameState = GameState.Executing;

            try
            {
                _session = await MultiplayerService.Instance.MatchmakeSessionAsync(
                    new MatchmakerOptions { QueueName = "default" },
                    GetIPSessionOptions());

                SetupGame();
            }
            catch (SessionException e)
            {
                var suggestion = e.Message;

                switch (e.Error)
                {
                    case SessionError.MatchmakerAssignmentTimeout:
                        suggestion = "Ensure that Multiplay fleet, machine, and server(s) are all online. If so, check server logs to identify the root cause.";
                        break;

                    default:
                        break;
                }

                Debug.Log(e);
                SetupMenu();
                throw new Exception(suggestion);
            }
        }

        public async Task LeaveSessionAsync()
        {
            GameState = GameState.Executing;

            if (_session == null)
                return;

            try
            {
                await ClearSession();
                await m_Connector.StopAsync();
                SetupMenu();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }
 
        private SessionOptions GetIPSessionOptions()
        {
            return GetSessionOptions()
                .WithDirectNetwork(_listenIp, _publishIp, _port);
        }

        private SessionOptions GetSessionOptions()
        {
            var options = new SessionOptions() { Type = k_SessionType, MaxPlayers = k_MaxPlayers };
            options.WithNetworkHandler(m_Connector);
            return options;
        }

        private JoinSessionOptions GetJoinSessionOptions()
        {
            var options = new JoinSessionOptions() { Type = k_SessionType };
            options.WithNetworkHandler(m_Connector);
            return options;
        }
        
        public void SetupMenu()
        {
            GameState = GameState.Menu;
        }

        private void SetupGame()
        {
            GameState = GameState.Game;
        }
        
        private async Task ClearSession()
        {
            if (_session != null)
            {
                Debug.Log("Leave session...");
                await _session.LeaveAsync();
                _session = null;
            }
            else
            {
                Debug.Log("No session found...");
            }
        }

        private string GetLocalIPAddress()
        {
#if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX || PLATFORM_STANDALONE_OSX
        foreach (var netInterface in System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces())
        {
            if (netInterface.NetworkInterfaceType == System.Net.NetworkInformation.NetworkInterfaceType.Wireless80211 ||
                netInterface.NetworkInterfaceType == System.Net.NetworkInformation.NetworkInterfaceType.Ethernet)
            {
                foreach (var addrInfo in netInterface.GetIPProperties().UnicastAddresses)
                {
                    if (addrInfo.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                    {
                        return addrInfo.Address.ToString();
                    }
                }
            }
        }
#else
            var host = System.Net.Dns.GetHostEntry(System.Net.Dns.GetHostName());

            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
#endif

            return "127.0.0.1";
        }
    }
}
