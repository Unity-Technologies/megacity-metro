#if UNITY_SERVER || ENABLE_UCS_SERVER
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Multiplayer;
using UnityEngine;

namespace Unity.Services.MultiplayerSDK.Server
{
    public class SessionEventManager
    {
        public event Action SessionJoined;
        public event Action SessionLeft;
        public event Action SessionChanged;
        public event Action<SessionState> SessionStateChanged;
        public event Action<string> PlayerJoined;
        public event Action<string> PlayerLeaving;
        public event Action SessionPropertiesChanged;
        public event Action SessionPlayerPropertiesChanged;
        public event Action RemovedFromSession;
        public event Action SessionDeleted;

        IServerSession _session;
        public IServerSession Session
        {
            get => _session;
            internal set
            {
                if (_session != null)
                {
                    _session.Changed -= SessionChanged;
                    _session.StateChanged -= SessionStateChanged;
                    _session.PlayerJoined -= PlayerJoined;
                    _session.PlayerLeaving -= PlayerLeaving;
                    _session.SessionPropertiesChanged -= SessionPropertiesChanged;
                    _session.PlayerPropertiesChanged -= SessionPlayerPropertiesChanged;
                    _session.RemovedFromSession -= RemovedFromSession;
                    _session.Deleted -= SessionDeleted;
                }

                _session = value;

                if (_session != null)
                {
                    _session.Changed += SessionChanged;
                    _session.StateChanged += SessionStateChanged;
                    _session.PlayerJoined += PlayerJoined;
                    _session.PlayerLeaving += PlayerLeaving;
                    _session.SessionPropertiesChanged += SessionPropertiesChanged;
                    _session.PlayerPropertiesChanged += SessionPlayerPropertiesChanged;
                    _session.RemovedFromSession += RemovedFromSession;
                    _session.Deleted += SessionDeleted;
                }
            }
        }
        
        // TODO: Review implementation (see MultiplayerPong)
        public IReadOnlyDictionary<string, SessionProperty> SessionProperties => _session?.Properties;
        public IReadOnlyDictionary<string, PlayerProperty> PlayerProperties => _session?.CurrentPlayer?.Properties;
        public Dictionary<string, PlayerProperty> LocalPlayerProperties { get; set; }
        public Dictionary<string, SessionProperty> LocalSessionProperties { get; set; }

        public SessionEventManager()
        {
            LocalPlayerProperties = new Dictionary<string, PlayerProperty>();
            LocalSessionProperties = new Dictionary<string, SessionProperty>();

            SessionJoined += OnSessionJoined;
            SessionLeft += OnSessionLeft;
            SessionChanged += OnSessionChanged;
            SessionStateChanged += OnSessionStateChanged;
            PlayerJoined += OnPlayerJoined;
            PlayerLeaving += OnPlayerLeaving;
            SessionPropertiesChanged += OnSessionPropertiesChanged;
            SessionPlayerPropertiesChanged += OnSessionPlayerPropertiesChanged;
            RemovedFromSession += OnRemovedFromSession;
            SessionDeleted += OnSessionDeleted;
        }
        
        public void RemovePlayer(string playerId)
        {
            Debug.Log($"[SessionEventManager]{DateTime.Now} Removing player {playerId}");
            Task.Run(async () =>
            {
                try
                {
                    await Session.AsHost().RemovePlayerAsync(playerId);
                }
                catch (Exception e)
                {
                    Debug.Log($"[SessionEventManager]{DateTime.Now} Failed to remove player {playerId} from session: {e.Message}. Might be already removed.");
                }
            });
        }

        // Invoke() must be called from within class containing the actions
        public void JoinSession()
        {
            SessionJoined?.Invoke();
            LocalPlayerProperties.Clear();            
        }

        private void OnSessionJoined()
        {
            Debug.Log($"[SessionEventManager]{DateTime.Now} SessionJoined");
        }

        private void OnSessionLeft()
        {
            Debug.Log($"[SessionEventManager]{DateTime.Now} SessionLeft");
        }

        private void OnSessionChanged()
        {
            //Debug.Log("[SessionEventManager] SessionChanged");
        }

        private void OnSessionStateChanged(SessionState state)
        {
            Debug.Log($"[SessionEventManager]{DateTime.Now} SessionStateChanged: {state}");
        }

        private void OnPlayerJoined(string id)
        {
            Debug.Log($"[SessionEventManager]{DateTime.Now} PlayerJoined: {id}");
        }

        private void OnPlayerLeaving(string id)
        {
            Debug.Log($"[SessionEventManager]{DateTime.Now} PlayerLeaving: {id}");
        }

        private void OnSessionPropertiesChanged()
        {
            Debug.Log($"[SessionEventManager]{DateTime.Now}SessionSessionPropertiesChanged");
        }

        private void OnSessionPlayerPropertiesChanged()
        {
            Debug.Log($"[SessionEventManager]{DateTime.Now} SessionPlayerPropertiesChanged");
        }

        private void OnRemovedFromSession()
        {
            Debug.Log($"[SessionEventManager]{DateTime.Now} SessionRemovedFromSession");
            LocalSessionProperties.Clear();
            Session = null;
        }

        private void OnSessionDeleted()
        {
            Debug.Log($"[SessionEventManager]{DateTime.Now} SessionDeleted");
        }
    }
}
#endif