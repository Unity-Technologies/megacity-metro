using System.Text;
using UnityEngine;

namespace Unity.Services.Samples.GameServerHosting
{
    [CreateAssetMenu(order = 0, menuName = "Unity/UGS/Settings", fileName = "new GameServerInfo")]
    public class GameServerInfo_Data : ScriptableObject
    {
        [field: SerializeField] public GameServerInfo Data { get; private set; }
    }

    [System.Serializable]
    public class GameServerInfo
    {
        public string Name = "Unity Dedicated Game Server";
        public string GameMode = "n/a";
        public string Map = "n/a";
        public string BuildID = "0";
        public int MaxUsers = 512;
        [Tooltip("Some server need some time before they can begin to accept connections, " +
            "this is the wait time until we start accepting the first connection.")]
        public int ServerStartupBufferMS = 5000;

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("GameInfo: ");
            sb.AppendLine($"- Name:     {Name}");
            sb.AppendLine($"- GameMode: {GameMode}");
            sb.AppendLine($"- Map:      {Map}");
            sb.AppendLine($"- BuildID:  {BuildID}");
            sb.AppendLine($"- MaxUsers: {MaxUsers}");
            sb.AppendLine($"- ServerStartupBufferMS: {ServerStartupBufferMS}");

            return sb.ToString();
        }
    }
}
