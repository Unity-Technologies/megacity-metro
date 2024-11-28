using System.Text;
using UnityEngine;

namespace Gameplay.Mix.Multiplayer.SDK.Server
{
    [CreateAssetMenu(order = 0, menuName = "Unity/UGS/Settings", fileName = "new GameServerInfo")]
    public class GameServerInfo_Data : ScriptableObject
    {
        [field: SerializeField] public GameServerInfo Data { get; private set; }
    }

    [System.Serializable]
    public class GameServerInfo
    {
        // Workaround for current Lobby limit returning error:
        //
        //  SessionException: request failed validation
        //  maxPlayers in body should be less than or equal to 100
        //
        public int MaxPlayers = 100;
        public string ServerName = "Unity Dedicated Game Server";
        public string GameType = "n/a";
        public string Map = "n/a";
        public string BuildID = "0";

        [Tooltip("Some server need some time before they can begin to accept connections, " +
            "this is the wait time until we start accepting the first connection.")]
        public int ServerStartupBufferMS = 5000;

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("GameServerInfo:");
            sb.AppendLine($"- MaxPlayers: {MaxPlayers}");
            sb.AppendLine($"- ServerName: {ServerName}");
            sb.AppendLine($"- GameType: {GameType}");
            sb.AppendLine($"- BuildID: {BuildID}");
            sb.AppendLine($"- Map: {Map}");
            sb.AppendLine($"- ServerStartupBufferMS: {ServerStartupBufferMS}");

            return sb.ToString();
        }
    }
}
