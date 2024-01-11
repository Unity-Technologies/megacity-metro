using System.Collections.Generic;
using Unity.Entities;
using Unity.MegacityMetro;
using Unity.MegacityMetro.UGS;
using Unity.NetCode;
using UnityEngine;

namespace Gameplay.Netcode
{
    /// <summary>
    /// Quick and dirty UI to show the status of thin client worlds in a build.
    /// Should only be visible to developers (when using debug flows).
    /// </summary>
    public class ThinClientDebugDisplay : MonoBehaviour
    {
        private readonly Dictionary<ulong, EntityQuery> m_CachedQueries = new (0);
        
        private void OnGUI()
        {
            if (!CommandLineConfig.AutomaticallyAddThinClients)
                return;

            var matchMakingConnector = MatchMakingConnector.Instance;
            var mode = CommandLineConfig.AutomaticallyMatchmake
                ? $"Automatic[{(string.IsNullOrEmpty(matchMakingConnector.IP) ? "searching..." : $"{matchMakingConnector.IP} : {matchMakingConnector.Port}")}]"
                : $"Manual[{(string.IsNullOrEmpty(matchMakingConnector.IP) ? "waiting for user..." : $"{matchMakingConnector.IP} : {matchMakingConnector.Port}")}]";
            GUILayout.Box($"{CommandLineConfig.TargetThinClientWorldCount} Thin Clients Requested (via {CommandLineConfig.ThinClientArg}): {mode} | Success: {matchMakingConnector.HasMatchmakingSuccess}, ClientIsInGame: {matchMakingConnector.ClientIsInGame}");
            foreach (var world in World.All)
            {
                if (!world.IsThinClient()) continue;
                if (!m_CachedQueries.TryGetValue(world.SequenceNumber, out var query))
                {
                    world.EntityManager.CompleteAllTrackedJobs();
                    query = world.EntityManager.CreateEntityQuery(typeof(NetworkId));
                    m_CachedQueries[world.SequenceNumber] = query;
                }

                GUI.color = query.IsEmpty ? Color.red : Color.green;
                GUILayout.Label(world.Name);
            }
        }

        private void OnDestroy()
        {
            // EntityQueries get disposed when the Worlds do.
            // This is necessary only for the "disable domain reloads when entering playmode" Editor optimization.
            m_CachedQueries.Clear();
        }
    }
}