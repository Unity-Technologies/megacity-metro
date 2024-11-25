#if UNITY_SERVER || ENABLE_UCS_SERVER
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Gameplay.Mix.Multiplayer.SDK.Server;
using Unity.Entities;
using Unity.NetCode;
using Unity.Services.Authentication;
using Unity.Services.Authentication.Server;
using Unity.Services.Core;
using Unity.Services.Multiplayer;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Services.MultiplayerSDK.Utils;

namespace Unity.Services.MultiplayerSDK.Server 
{
    public enum ServerState
    {
        Boot,
        Executing,
        Game
    }

    public class ServerManager
    {
        const string k_SessionType = "NFESession";
        const string k_SessionName = "ServerSession";

        public ServerState ServerState { get; private set; } = ServerState.Boot;

        public IServerSession Session { get; private set; }        
        internal IMultiplaySessionManager SessionManager { get; private set; }
        internal SessionEventManager SessionEventManager { get; private set; }

        // Use Service Account credentials to test locally
        internal string apiKeyId = "";
        internal string apiKeySecret = "";

        internal readonly Dispatcher Dispatcher;

        internal GameServerInfo ServicesData;

        public ServerManager(Dispatcher dispatcher, GameServerInfo_Data servicesData)
        {
            Dispatcher = dispatcher;

            ServicesData = servicesData.Data;
            // Debug.Log($"Game service data: {JsonUtility.ToJson(ServicesData, true)}");
            
            SessionEventManager = new SessionEventManager();
            
            SessionEventManager.PlayerJoined += id => {
                CheckAllocationPayload();
            };

            SessionEventManager.PlayerLeft += id => {
                CheckAllocationPayload();
            };
        }

        public void SetCredentials(string id, string secret)
        {
            if (String.IsNullOrEmpty(id) || String.IsNullOrEmpty(secret))
            {
                Debug.Log("Incomplete credentials..");
                return;
            }

            Debug.Log("Setting credentials..");
            apiKeyId = id;
            apiKeySecret = secret;
        }

        private async Task CheckAllocationPayload()
        {
            if (SessionManager != null)
            {
                var payload = await SessionManager.GetAllocationPayloadFromPlainTextAsync();
                Debug.Log($"[ServerManager] SessionManager payload: {payload}");
            }
        }

        public async Task StartAsync()
        {
            ServerState = ServerState.Executing;

            await UnityServices.InitializeAsync();

            if (!ServerAuthenticationService.Instance.IsAuthorized)
            {
                // Prepare game server session (i.e. listen for allocation)
                try
                {
                    var isHostedOnMultiplay = true;
                    try
                    {
                        await ServerAuthenticationService.Instance.SignInFromServerAsync();
                    }
                    catch (Exception ex)
                    {
                        Debug.Log("Not hosted on Multiplay..");
                        isHostedOnMultiplay = false;
                    }

                    // We are in a Multiplay server
                    //
                    // Note: This also returns an service so is not useful
                    // if (UnityServices.Instance.GetMultiplayService() != null)
                    if (isHostedOnMultiplay)
                    {
                        // Prepare callbacks to track server lifecycle
                        var serverCallbacks = new MultiplaySessionManagerEventCallbacks();
                        serverCallbacks.Allocated += async(allocation) =>
                        {
                            Debug.Log($"[ServerManager] Allocation: Server ID {allocation.ServerId} allocated with allocation ID: {allocation.ID}");
                            try
                            {
                                CheckAllocationPayload();
                            }
                            catch (Exception e)
                            {
                                Debug.LogException(e);
                            }

                            // Allocation data
                            Debug.Log($"[ServerManager] SessionManager: Server ID {SessionManager.ServerId} / IP {SessionManager.IpAddress} / ConnectionPort {SessionManager.ConnectionPort} / QueryPort {SessionManager.QueryPort}");

                            // Session data
                            Session = SessionManager.Session;

                            // Session events
                            SessionEventManager.Session = SessionManager.Session;

                            Debug.Log($"[ServerManager] Session: ID {Session.Id} / Host {Session.Host} / IsHost {Session.IsHost}");
                        };

                        serverCallbacks.Deallocated += (deallocation) =>
                        {
                            Debug.Log($"[ServerManager] Deallocation: Server {deallocation.ServerId} deallocated with ID: {deallocation.ID}");
                        };

                        serverCallbacks.StateChanged += (state) =>
                        {
                            Debug.Log($"[ServerManager] Server state changed to {state}");
                        };

                        serverCallbacks.PlayerReadinessChanged += (isReady) =>
                        {
                            Debug.Log($"[ServerManager] Player readiness changed to {isReady}");
                        };


                        Debug.Log($"Start Multiplay server with build GUID {Application.buildGUID}");

                        // The server should respond to query requests prior to the server being allocated.
                        // Accordingly, the SessionManager will start an SQP server immediately.
                        SessionManager = await MultiplayerServerService.Instance.StartMultiplaySessionManagerAsync(
                            new MultiplaySessionManagerOptions()
                            {
                                // Note: SQP data will be combination of
                                // SessionOptions and MultiplayServerOptions
                                SessionOptions = new SessionOptions()
                                    {
                                        Type = k_SessionType,
                                        MaxPlayers = ServicesData.MaxPlayers,
                                        SessionProperties = SessionEventManager.LocalSessionProperties,
                                        PlayerProperties = SessionEventManager.LocalPlayerProperties,
                                    }.WithDirectNetwork()
                                    .WithBackfillingConfiguration(enable: true, autoStart: true),
                                    
                                MultiplayServerOptions = new MultiplayServerOptions(
                                    ServicesData.ServerName,    // server_name
                                    ServicesData.GameType,      // game_type
                                    Application.buildGUID,      // build_id - use ID for the game build itself (versus _hosting_ build) for tracking back to CI
                                    ServicesData.Map            // map
                                ),
                                Callbacks = serverCallbacks
                            });

                        SessionEventManager.JoinSession();

                        Debug.Log("Successfully started a Multiplay server; session will be set once server is allocated..");
                    }
                    else if (!String.IsNullOrEmpty(apiKeyId) && !String.IsNullOrEmpty(apiKeySecret))
                    {   
                        Debug.Log("Found service account credentials..");
                        await ServerAuthenticationService.Instance.SignInWithServiceAccountAsync(apiKeyId, apiKeySecret);

                        // Mimic sessions locally
                        //
                        // Note: Backfill is not supported by WithDirectNetwork() locally because
                        // Matchmaker has no knowledge of this game server (i.e. your computer)
                        Session = await MultiplayerServerService.Instance.CreateSessionAsync(
                            new SessionOptions()
                            {
                                Type = k_SessionType,
                                Name = k_SessionName,
                                MaxPlayers = ServicesData.MaxPlayers,
                                SessionProperties = SessionEventManager.LocalSessionProperties,
                                PlayerProperties = SessionEventManager.LocalPlayerProperties,
                            }.WithDirectNetwork()
                        );

                        SessionEventManager.JoinSession();

                        Debug.Log($"Successfully started a server session with ID {Session.Id}");
                    }
                    else
                    {
                        throw new Exception("Missing credentials for server authentication");
                    }

                    // TBD: Set this now or once server is allocated (if Multiplay)?
                    ServerState = ServerState.Game;
                }
                catch (Exception ex)
                {
                    Debug.Log($"Could not start a server session: {ex.Message}");

                    throw ex;
                }
            }
        }

        public async Task LeaveSessionAsync()
        {
            ServerState = ServerState.Executing;

            try
            {
                await Session.LeaveAsync();
                Session = null;
                ServerState = ServerState.Boot;
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        private void Cleanup()
        {
            if (Session != null && Session.State == SessionState.Connected)
            {
                Session?.LeaveAsync();
            }
        }

        private void OnDestroy() 
        {
            Dispatcher.Dispatch(LeaveSessionAsync);
        }
    }
}
#endif