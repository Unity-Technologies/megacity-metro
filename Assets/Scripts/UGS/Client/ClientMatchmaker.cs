using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Unity.Services.Matchmaker;
using Unity.Services.Matchmaker.Models;

namespace Unity.Services.Samples.GameServerHosting
{
    /// <summary>
    /// Represent errors at different stages of the ClientMatchmaker.Matchmake() function.
    /// </summary>
    public enum MatchmakerPollingResult
    {
        Success,
        TicketCreationError,
        TicketCancellationError,
        TicketRetrievalError,
        MatchAssignmentError
    }

    /// <summary>
    /// Wrapped Results for this ClientMatchmaker wrapper.
    /// Use the returned object in your own code, to bubble up errors or feedback to
    /// the UI/UX for users, or continue your game logic.
    /// </summary>
    public class MatchmakingResult
    {
        public string ip;
        public int port;
        public MatchmakerPollingResult result;
        public string resultMessage;
    }

    public class ClientMatchmaker : IDisposable
    {
        const int MaxRetry = 1;
        const int k_GetTicketCooldown = 1000;

        string m_LastUsedTicket;
        bool m_IsMatchmaking = false;

        CancellationTokenSource m_CancelToken;

        /// <summary>
        /// Create a ticket for the one user and begin matchmaking with their preferences
        /// </summary>
        /// <param name="data">The Client's preferences and ID</param>
        public async Task<MatchmakingResult> Matchmake(PlayerProfile data)
        {
            var retryCount = -1;
            MatchmakingResult result = ReturnMatchResult(MatchmakerPollingResult.TicketCancellationError, "Too many retries.");
            while (retryCount < MaxRetry)
            {
                var (tryResult, retry) = await TryMatchmaking(data);
                if (tryResult.result == MatchmakerPollingResult.Success)
                {
                    return tryResult;
                }

                if (!retry)
                {
                    return tryResult;
                }

                result = tryResult;
                retryCount++;
                Debug.Log($"[Matchmaker] Retry {retryCount} of {MaxRetry}");
                await Task.Delay(1000);
            }

            return result;
        }

        private async Task<(MatchmakingResult result, bool retry)> TryMatchmaking(PlayerProfile data)
        {
            m_CancelToken = new CancellationTokenSource();
            var players = new List<Player> { new(data.UASId) };
            try
            {
                m_IsMatchmaking = true;
                var createTicketOptions = new CreateTicketOptions();
                var createResult = await MatchmakerService.Instance.CreateTicketAsync(players, createTicketOptions);
                Debug.Log($"[Matchmaker] Ticket Result: {createResult.Id}");
                m_LastUsedTicket = createResult.Id;
            }
            catch (MatchmakerServiceException e)
            {
                return (ReturnMatchResult(MatchmakerPollingResult.TicketCreationError, e.ToString()), false);
            }

            try
            {
                //Polling Loop, cancelling should take us all the way to the method
                while (!m_CancelToken.IsCancellationRequested)
                {
                    var checkTicket = await MatchmakerService.Instance.GetTicketAsync(m_LastUsedTicket);
                    Debug.Log($"[Matchmaker] Check Ticket: {checkTicket.Value}");
                    if (checkTicket.Type == typeof(MultiplayAssignment))
                    {
                        var matchAssignment = (MultiplayAssignment)checkTicket.Value;
                        Debug.Log($"[Matchmaker] Match Assignment Ticket Status: {matchAssignment.Status}");

                        if (matchAssignment.Status == MultiplayAssignment.StatusOptions.Found)
                        {
                            return (ReturnMatchResult(MatchmakerPollingResult.Success, "", matchAssignment), false);
                        }

                        if (DoNeedRetry(matchAssignment))
                        {
                            return (ReturnMatchResult(MatchmakerPollingResult.MatchAssignmentError,
                                $" Ticket: {m_LastUsedTicket} - {matchAssignment.Status} - {matchAssignment.Message}"), true);
                        }
                        if (matchAssignment.Status != MultiplayAssignment.StatusOptions.InProgress)
                        {
                            return (ReturnMatchResult(MatchmakerPollingResult.MatchAssignmentError,
                                $" Ticket: {m_LastUsedTicket} - {matchAssignment.Status} - {matchAssignment.Message}"), false);
                        }

                        Debug.Log($"[Matchmaker] Polled Ticket: {m_LastUsedTicket} Status: {matchAssignment.Status} ");
                    }

                    await Task.Delay(k_GetTicketCooldown);
                }
            }
            catch (MatchmakerServiceException e)
            {
                return (ReturnMatchResult(MatchmakerPollingResult.TicketRetrievalError, e.ToString()), false);
            }

            return (ReturnMatchResult(MatchmakerPollingResult.TicketCancellationError, "Cancelled Matchmaking"), false);
        }

        private bool DoNeedRetry(MultiplayAssignment matchAssignment)
        {
            return matchAssignment.Status == MultiplayAssignment.StatusOptions.Failed &&
                   matchAssignment.Message.StartsWith(
                       "MultiplayAllocationError: request error: maximum capacity reached");
        }

        public bool IsMatchmaking => m_IsMatchmaking;

        public async Task CancelMatchmaking()
        {
            if (!m_IsMatchmaking)
                return;
            m_IsMatchmaking = false;
            if (m_CancelToken.Token.CanBeCanceled)
                m_CancelToken.Cancel();

            if (string.IsNullOrEmpty(m_LastUsedTicket))
                return;

            Debug.Log($"Cancelling {m_LastUsedTicket}");
            await MatchmakerService.Instance.DeleteTicketAsync(m_LastUsedTicket);
        }

        //Make sure we exit the matchmaking cycle through this method every time.
        MatchmakingResult ReturnMatchResult(MatchmakerPollingResult resultErrorType, string message = "",
            MultiplayAssignment assignment = null)
        {
            m_IsMatchmaking = false;

            if (assignment != null)
            {
                return new MatchmakingResult
                {
                    result = MatchmakerPollingResult.Success,
                    ip = assignment.Ip,
                    port = assignment.Port ?? -1,
                    resultMessage = assignment.Message
                };
            }

            return new MatchmakingResult
            {
                result = resultErrorType,
                resultMessage = message
            };
        }

        public void Dispose()
        {
#pragma warning disable 4014
            CancelMatchmaking();
#pragma warning restore 4014
            m_CancelToken?.Dispose();
        }
    }
}