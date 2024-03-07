#if UNITY_STANDALONE
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;
using Unity.Services.Matchmaker;
using Unity.Services.Matchmaker.Models;

namespace Unity.Services.Samples.GameServerHosting
{
    /// <summary>
    /// Allows the server to accept players via matchmaking after the game has started.
    /// </summary>
    public class BackfillService : IDisposable
    {
        public bool Backfilling { get; private set; }
        public Action OnBackfillEnded;

        private readonly TimeSpan k_PlayerMissingTimeout = TimeSpan.FromSeconds(30);
        private const int k_TicketCheckMs = 1000;

        private CreateBackfillTicketOptions m_CreateBackfillOptions;
        private BackfillTicket m_LocalBackfillTicket;
        private bool m_LocalDataDirty;
        private int m_MaxPlayers;
        private int MatchPlayerCount => m_LocalBackfillTicket?.Properties.MatchProperties.Players.Count ?? 0;

        // Used to track players expected to connect to the game server and identify the ones that are missing
        private Dictionary<string, DateTime> m_MissingPlayers = new();

        private GameHostingServices m_GameHostingServices;
        
        public BackfillService(GameHostingServices gameHostingServices)
        {
            m_GameHostingServices = gameHostingServices;
        }

        public void CreateBackfillTicketFromPayload(string connection, string queueName, MatchProperties matchmakerPayloadProperties, int maxPlayers)
        {
            m_MaxPlayers = maxPlayers;
            var backfillProperties = new BackfillTicketProperties(matchmakerPayloadProperties);
            m_LocalBackfillTicket = new BackfillTicket { Id = matchmakerPayloadProperties.BackfillTicketId, Properties = backfillProperties };

            m_CreateBackfillOptions = new CreateBackfillTicketOptions
            {
                Connection = connection,
                QueueName = queueName,
                Properties = backfillProperties
            };

            Debug.Log($"[Backfill Service] {connection} : Queue {queueName} {maxPlayers});");
        }

        public async Task TryBeginBackfilling()
        {
            Debug.Log($"[Backfill] Starting TryBeginBackfilling");

            // SetStagingEnvironment(); for internal unity testing only.
            if (IsMatchFull())
            {
                Debug.LogWarning($"[Backfill] Match is already at {MatchPlayerCount}/{m_MaxPlayers}, can't start backfill.");
                return;
            }

            //Create a ticket if we don't have one already (via Allocation)
            if (string.IsNullOrEmpty(m_LocalBackfillTicket.Id))
                m_LocalBackfillTicket.Id = await MatchmakerService.Instance.CreateBackfillTicketAsync(m_CreateBackfillOptions);

            var modelAsJson = JsonConvert.SerializeObject(m_LocalBackfillTicket, Formatting.Indented);

            Debug.Log($"[Backfill] {nameof(TryBeginBackfilling)} : {Environment.NewLine} {modelAsJson});");

            if (!Backfilling)
            {
                Backfilling = true;

#pragma warning disable 4014
                BackfillLoop();
#pragma warning restore 4014
            }
        }

        /// <summary>
        /// The matchmaker maintains the state of the match on the backend.
        /// As such we just need to manually handle cases where the player's join/leave outside of the service.
        /// </summary>
        public void AddPlayerToMatch(string id)
        {
            m_MissingPlayers.Remove(id);

            if (!Backfilling)
            {
                Debug.LogWarning("[Backfill] Can't add users to the backfill ticket before it's been created");
                return;
            }

            if (GetPlayerById(id) != null)
            {
                Debug.LogWarning($"[Backfill] User:{id} already in Match. Ignoring add.");
                return;
            }

            var matchmakerPlayer = new Player(id);
            Debug.LogWarning($"[Backfill] Adding a User:{id}. Matchmaker{matchmakerPlayer}");
            m_LocalBackfillTicket.Properties.MatchProperties.Players.Add(matchmakerPlayer);
            m_LocalBackfillTicket.Properties.MatchProperties.Teams[0].PlayerIds.Add(matchmakerPlayer.Id);
            m_LocalDataDirty = true;
        }

        public void RemovePlayerFromMatch(string userID)
        {
            m_MissingPlayers.Remove(userID);
            
            if (!Backfilling)
            {
                Debug.LogWarning("[Backfill] Can't remove users from the backfill ticket before it's been created");
                return;
            }

            var playerToRemove = GetPlayerById(userID);
            if (playerToRemove == null)
            {
                Debug.LogWarning($"[Backfill] No user by the ID: {userID} in local backfill Data.");
                return;
            }

            Debug.LogWarning($"[Backfill] Removing a User:{userID}. Matchmaker{playerToRemove}");
            m_LocalBackfillTicket.Properties.MatchProperties.Players.Remove(playerToRemove);

            //We Only have one team in this game, so this simplifies things here
            m_LocalBackfillTicket.Properties.MatchProperties.Teams[0].PlayerIds.Remove(userID);
            m_LocalDataDirty = true;
        }

        public async Task StopBackfill()
        {
            if (!Backfilling)
            {
                Debug.LogError("[Backfill] Can't stop backfilling before we start.");
                return;
            }

            var modelAsJson = JsonConvert.SerializeObject(m_LocalBackfillTicket, Formatting.Indented);

            Debug.Log($"[Backfill] StopBackfill {nameof(StopBackfill)} : {Environment.NewLine} {modelAsJson});");
            await MatchmakerService.Instance.DeleteBackfillTicketAsync(m_LocalBackfillTicket.Id);
            Backfilling = false;
            m_LocalBackfillTicket.Id = null;
        }

        public bool IsMatchFull()
        {
            return MatchPlayerCount >= m_MaxPlayers;
        }

        Player GetPlayerById(string userID)
        {
            return m_LocalBackfillTicket.Properties.MatchProperties.Players.FirstOrDefault(p => p.Id.Equals(userID));
        }

        /// <summary>
        /// Generally it's a good idea to get the latest state of the backfill before modifying it and updating
        /// </summary>
        async Task BackfillLoop()
        {
            while (Backfilling)
            {
                try
                {
                    if (m_LocalDataDirty)
                    {
                        await MatchmakerService.Instance.UpdateBackfillTicketAsync(m_LocalBackfillTicket.Id, m_LocalBackfillTicket);
                        m_LocalDataDirty = false;
                    }
                    else
                    {
                        m_LocalBackfillTicket = await MatchmakerService.Instance.ApproveBackfillTicketAsync(m_LocalBackfillTicket.Id);
                    }

                    if (IsMatchFull())
                    {
                        await StopBackfill();
                        break;
                    }

                    UpdateMissingPlayers();
                    if (m_LocalBackfillTicket.Properties.MatchProperties.Players.Count == 0)
                    {
                        Debug.LogWarning($"[Backfill] No players in backfill ticket. Stopping backfill.");
                        OnBackfillEnded?.Invoke();
                        break;
                    }
                }
                catch (MatchmakerServiceException e)
                {
                    if (e.Reason == MatchmakerExceptionReason.EntityNotFound)
                    {
                        Debug.LogWarning($"[Backfill] Backfill ticket missing. Stopping backfill.");
                        OnBackfillEnded?.Invoke();
                        break;
                    }

                    Debug.LogError($"MatchmakerServiceException: Reason: {e.Reason} Message: {e.Message}");
                    Debug.LogException(e);
                }

                //Backfill Docs recommend a once-per-second approval for backfill tickets
                await Task.Delay(k_TicketCheckMs);
            }
        }

        private void UpdateMissingPlayers()
        {
            var now = DateTime.UtcNow;
            var missingFromBackfill = CompareBackfillWithConnectedPlayerIds();
            foreach (var playerId in missingFromBackfill)
            {
                if (m_MissingPlayers.TryGetValue(playerId, out var missingSince))
                {
                    if (now - missingSince > k_PlayerMissingTimeout)
                    {
                        Debug.LogWarning($"[Backfill] Player {playerId} has been missing for {k_PlayerMissingTimeout.TotalSeconds} seconds. Removing from backfill.");
                        RemovePlayerFromMatch(playerId);
                    }
                }
                else
                {
                    Debug.Log($"[Backfill] Player {playerId} is missing from backfill.");
                    m_MissingPlayers[playerId] = now;
                }
            }
        }

        private List<string> CompareBackfillWithConnectedPlayerIds()
        {
            var backfillPlayerIds = m_LocalBackfillTicket.Properties.MatchProperties.Players.Select(p => p.Id).ToList();
            return backfillPlayerIds.Except(m_GameHostingServices.ConnectedPlayerIds).ToList();
        }

        public void Dispose()
        {
#pragma warning disable 4014
            StopBackfill();
#pragma warning restore 4014
        }
    }
}
#endif
