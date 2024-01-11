using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Unity.MegacityMetro.Gameplay
{
    public enum ServerLocation
    {
        Local = 0,
        America = 1,
        Europe = 2,
        Asia = 3
    }

    [System.Serializable]
    public struct ServerInfo
    {
        public string IP;
        public ServerLocation Location;
    }

    /// <summary>
    /// Settings for the multiplayer server
    /// </summary>
    [CreateAssetMenu(fileName = "MultiplayerServerSettings", menuName = "Gameplay/Settings/MultiplayerServerSettings", order = 1)]
    public class MultiplayerServerSettings : ScriptableObject
    {
        [SerializeField]
        private ServerInfo[] ServerList;
#if UNITY_EDITOR
        [HideInInspector]
        public bool IsAutoloading;
        [field: SerializeField] public GameMode AutoRunGameModeInEditorMain { private set; get; }
#endif

        public List<string> GetUIChoices()
        {
            var list = new List<string>();
            foreach(var server in ServerList)
            {
                if (!Debug.isDebugBuild && server.Location == ServerLocation.Local)
                    continue;
                list.Add(server.Location.ToString());
            }
            return list;
        }

        public string GetIPByName(string newValue)
        {
            var server = ServerList.ToList().Find(c => c.Location.ToString().Equals(newValue));
            return server.IP;
        }
    }
}
