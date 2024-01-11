#if UNITY_STANDALONE
using System;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Unity.Services.Matchmaker.Models;
using Debug = UnityEngine.Debug;

using Unity.Services.Multiplay;
namespace Unity.Services.Samples.GameServerHosting
{
    /// <summary>
    /// The server Allocation service allows the Game Server to interface with some Hosting Service Events,
    /// Allocation Information and the Config file that Multiplay creates for you.
    /// It also allows you to flag the server Ready / Unready for new players.
    /// </summary>
    public class AllocationPayloadService : IDisposable
    {
        public bool IsInitialized => m_MultiplayService != null;

        IMultiplayService m_MultiplayService;
        MultiplayEventCallbacks m_Servercallbacks;
        IServerEvents m_ServerEvents;
        string m_AllocationId;

        public AllocationPayloadService()
        {
            try
            {
                m_MultiplayService = MultiplayService.Instance;
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[GSH] Error creating Allocation service.\n{ex}");
            }
        }

        /// <summary>
        /// Should be wrapped in a timeout function
        /// </summary>
        public async Task<MatchmakingResults> SubscribeAndAwaitMatchmakerAllocation()
        {
            if (m_MultiplayService == null)
                return null;
            m_AllocationId = null;
            m_Servercallbacks = new MultiplayEventCallbacks();
            m_Servercallbacks.Allocate += OnMultiplayAllocation;
            m_ServerEvents = await m_MultiplayService.SubscribeToServerEventsAsync(m_Servercallbacks);

            var allocationID = await AwaitAllocationID();
            var mmPayload = await GetMatchmakerAllocationPayloadAsync();

            return mmPayload;
        }

        async Task<string> AwaitAllocationID()
        {
            var config = m_MultiplayService.ServerConfig;
            Debug.Log($"[GSH] Awaiting Allocation. Server Config is:\n" +
                $"-ServerID: {config.ServerId}\n" +
                $"-AllocationID: {config.AllocationId}\n" +
                $"-Port: {config.Port}\n" +
                $"-QPort: {config.QueryPort}\n" +
                $"-logs: {config.ServerLogDirectory}");

            //Waiting on OnMultiplayAllocation() event (Probably wont ever happen in a matchmaker scenario)
            while (string.IsNullOrEmpty(m_AllocationId))
            {
                var configID = config.AllocationId;

                if (!string.IsNullOrEmpty(configID) && string.IsNullOrEmpty(m_AllocationId))
                {
                    Debug.Log($"[GSH] Config had AllocationID: {configID}");
                    m_AllocationId = configID;
                }

                await Task.Delay(100);
            }

            return m_AllocationId;
        }

        /// <summary>
        /// Get the Multiplay Allocation Payload for Matchmaker (using Multiplay SDK)
        /// </summary>
        /// <returns></returns>
        async Task<MatchmakingResults> GetMatchmakerAllocationPayloadAsync()
        {
            Debug.Log("[AllocationPayloadService] GetMatchmakerAllocationPayloadAsync");
            var payloadAllocation = await MultiplayService.Instance.GetPayloadAllocationFromJsonAs<MatchmakingResults>();
            var modelAsJson = JsonConvert.SerializeObject(payloadAllocation, Formatting.Indented);
            Debug.Log(nameof(GetMatchmakerAllocationPayloadAsync) + ":" + Environment.NewLine + modelAsJson);
            return payloadAllocation;
        }

        void OnMultiplayAllocation(MultiplayAllocation allocation)
        {
            Debug.Log($"[GSH] OnAllocation: {allocation.AllocationId}");
            if (string.IsNullOrEmpty(allocation.AllocationId))
                return;
            m_AllocationId = allocation.AllocationId;
        }

        void OnMultiplayDeAllocation(MultiplayDeallocation deallocation)
        {
            Debug.Log(
                $"[GSH] Deallocated : ID: {deallocation.AllocationId}\nEvent: {deallocation.EventId}\nServer{deallocation.ServerId}");
        }

        void OnMultiplayError(MultiplayError error)
        {
            Debug.Log($"[GSH] MultiplayError : {error.Reason}\n{error.Detail}");
        }

        public async Task ReadyServer()
        {
            if (m_MultiplayService == null)
                return;
            Debug.Log($"[GSH] Server is ready for connections.");
            await m_MultiplayService.ReadyServerForPlayersAsync();
        }

        public async Task UnreadyServer()
        {
            if (m_MultiplayService == null)
                return;
            Debug.Log($"[GSH] Server is not ready for connections.");

            await m_MultiplayService.UnreadyServerAsync();
        }

        public void Dispose()
        {
            if (m_Servercallbacks != null)
            {
                m_Servercallbacks.Allocate -= OnMultiplayAllocation;
                m_Servercallbacks.Deallocate -= OnMultiplayDeAllocation;
                m_Servercallbacks.Error -= OnMultiplayError;
            }

            m_ServerEvents?.UnsubscribeAsync();
        }
    }
}
#endif