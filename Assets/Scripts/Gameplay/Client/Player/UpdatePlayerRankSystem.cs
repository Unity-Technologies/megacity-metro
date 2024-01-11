using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;
using System.Collections.Generic;
using Unity.MegacityMetro.UI;
using Unity.NetCode.Extensions;

namespace Unity.MegacityMetro.Gameplay
{
    /// <summary>
    /// System to update the score ranking.
    /// </summary>
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
    public partial struct UpdateScoreRankingSystem : ISystem, ISystemStartStop
    {
        private EntityQuery m_PlayersQuery;
        private EntityQuery m_LocalPlayerQuery;
        public void OnCreate(ref SystemState state)
        {
            m_LocalPlayerQuery = state.GetEntityQuery(ComponentType.ReadOnly<GhostOwnerIsLocal>());
            m_PlayersQuery = state.GetEntityQuery(ComponentType.ReadOnly<PlayerScore>(), ComponentType.ReadOnly<PlayerName>());
            state.RequireForUpdate(m_PlayersQuery);
            state.RequireForUpdate(m_LocalPlayerQuery);
        }

        public void OnStartRunning(ref SystemState state)
        {
            HUD.Instance.Leaderboard.Show();
        }

        public void OnStopRunning(ref SystemState state)
        {
            HUD.Instance.Leaderboard.Hide();
        }

        public void OnUpdate(ref SystemState state)
        {
            if (m_LocalPlayerQuery.CalculateEntityCount() <= 0)
                return;
            
            var localEntityPlayer = m_LocalPlayerQuery.ToEntityArray(Allocator.Temp)[0];
            var playerCount = m_PlayersQuery.CalculateEntityCount();
            var ranking = CollectionHelper.CreateNativeArray<PlayerScoreAspect>(playerCount, state.WorldUpdateAllocator);
            var index = 0;
            foreach (var player in SystemAPI.Query<PlayerScoreAspect>())
            {
                player.SetIsLocalPlayer(ref localEntityPlayer);
                ranking[index] = player;
                index++;
            }
            ranking.Sort(new SortableScoreComparer());

            for (int i = 0; i < ranking.Length; i++)
            {
                var isTopRanked = i == 0;
                PlayerInfoController.Instance.UpdateBadge(ranking[i].Self, isTopRanked);
            }

            HUD.Instance?.Leaderboard?.SetRanking(ref ranking);
            ranking.Dispose(state.Dependency);
        }
    }

    /// <summary>
    /// Executes comparison between rank parameters
    /// </summary>
    [BurstCompile]
    public struct SortableScoreComparer : IComparer<PlayerScoreAspect>
    {
        public int Compare(PlayerScoreAspect x, PlayerScoreAspect y)
        {
            if(!y.Value.Equals(x.Value)) 
                return y.Value.CompareTo(x.Value);

            if (!y.Kills.Equals(x.Kills))
                return y.Kills.CompareTo(x.Kills);

            return x.NetworkId.CompareTo(y.NetworkId);
        }
    }
}
