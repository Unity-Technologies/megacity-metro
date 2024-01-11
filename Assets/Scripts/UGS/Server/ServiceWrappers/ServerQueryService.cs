#if UNITY_STANDALONE
using System;
using System.Threading;
using System.Threading.Tasks;

using Unity.Services.Multiplay;
using Debug = UnityEngine.Debug;

namespace Unity.Services.Samples.GameServerHosting
{
    public class ServerQueryService : IDisposable
    {
        public bool IsInitialized => m_MultiplayService != null;
        IMultiplayService m_MultiplayService;
        IServerQueryHandler m_ServerQueryHandler;
        CancellationTokenSource m_ServerCheckCancel;

        public ServerQueryService()
        {
            try
            {
                m_MultiplayService = MultiplayService.Instance;
                m_ServerCheckCancel = new CancellationTokenSource();
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[GSH] Error creating Server Query service.\n{ex}");
            }
        }

        public async Task BeginServerQueryHandler(int maxPlayers = 10, string serverName = "n/a", string gameType = "n/a", string map = "n/a")
        {
            if (m_MultiplayService == null)
                return;
            try
            {
                m_ServerQueryHandler = await m_MultiplayService.StartServerQueryHandlerAsync((ushort)maxPlayers,
                    serverName, gameType, "0", map);

#pragma warning disable 4014
                ServerQueryLoop(m_ServerCheckCancel.Token);
#pragma warning restore 4014
            }
            catch (Exception ex)
            {
                Debug.LogError($"[GSH] Error Starting Server Query service.\n{ex}");
            }
        }

        public void SetServerName(string name)
        {
            m_ServerQueryHandler.ServerName = name;
        }

        public void SetBuildID(string id)
        {
            m_ServerQueryHandler.BuildId = id;
        }

        public void SetMaxPlayers(int players)
        {
            m_ServerQueryHandler.MaxPlayers = (ushort)players;
        }

        public void SetPlayerCount(int count)
        {
            m_ServerQueryHandler.CurrentPlayers = (ushort)count;
        }

        public void SetMap(string newMap)
        {
            m_ServerQueryHandler.Map = newMap;
        }

        public void SetMode(string mode)
        {
            m_ServerQueryHandler.GameType = mode;
        }

        async Task ServerQueryLoop(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                // Prompt the handler to deal with any incoming request packets.
                // Ensure the delay here is sub 1 second, to ensure that incoming packets are not dropped.
                m_ServerQueryHandler.UpdateServerCheck();
                await Task.Delay(100);
            }
        }

        public void Dispose()
        {
            if (m_ServerCheckCancel != null)
                m_ServerCheckCancel.Cancel();
        }
    }
}
#endif