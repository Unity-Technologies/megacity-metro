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
        public ISession Session { get; private set; }
        public GameState GameState { get; private set; } = GameState.Boot;

        private CustomEntitiesNetworkHandler m_Connector;
        public NetworkEndpoint ConnectionEndpoint => m_Connector.ConnectionEndpoint;

        internal readonly Dispatcher Dispatcher;

        public NetworkType NetworkType = NetworkType.Direct;
        public string ListenIp;
        public string PublishIp;
        public int Port;
        public string Region;

        public bool UseCustomNetworkHandler;

        const string k_SessionType = "MegacityMetroSession";
        const int k_MaxPlayers = 100; // TODO: Pull the same values as the server

        public ClientManager(Dispatcher dispatcher)
        {
            m_Connector = new CustomEntitiesNetworkHandler();
            Dispatcher = dispatcher;
            ListenIp = "0.0.0.0";
            Port = 0;
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

        public async Task MatchmakeAsync()
        {
            GameState = GameState.Executing;

            try
            {
                Session = await MultiplayerService.Instance.MatchmakeSessionAsync(
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

            if (Session == null)
                return;

            try
            {
                await ClearSession();
                SetupMenu();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        // TODO: Combine/replace with Cleanup()
        public async Task ClearSession()
        {
            if (Session != null)
            {
                Debug.Log("Leave session...");
                await Session.LeaveAsync();
                Session = null;
            }
            else
            {
                Debug.Log("No session found...");
            }
        }

        SessionOptions GetIPSessionOptions()
        {
            return GetSessionOptions()
                .WithDirectNetwork(ListenIp, PublishIp, Port);
        }

        SessionOptions GetRelaySessionOptions()
        {
            return GetSessionOptions().WithRelayNetwork(Region);
        }

        private SessionOptions GetSessionOptions()
        {
            var options = new SessionOptions() { Type = k_SessionType, MaxPlayers = k_MaxPlayers };
            options.WithNetworkHandler(m_Connector);
            return options;
        }

        private JoinSessionOptions GetJoinSessionOptions()
        {
            return new JoinSessionOptions() { Type = k_SessionType };
        }

        // TODO: Use this instead of new method?
        private void Cleanup()
        {
            if (Session != null && Session.State == SessionState.Connected)
            {
                Session?.LeaveAsync();
            }
        }

        private void SetupMenu()
        {
            GameState = GameState.Menu;
        }

        private void SetupGame()
        {
            GameState = GameState.Game;
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