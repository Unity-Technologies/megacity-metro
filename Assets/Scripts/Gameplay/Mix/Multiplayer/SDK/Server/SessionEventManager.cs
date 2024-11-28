#if UNITY_SERVER || ENABLE_UCS_SERVER
using System;
using System.Collections.Generic;
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
        public event Action<string> PlayerLeft;
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
                    _session.PlayerLeft -= PlayerLeft;
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
                    _session.PlayerLeft += PlayerLeft;
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
            PlayerLeft += OnPlayerLeft;
            SessionPropertiesChanged += OnSessionPropertiesChanged;
            SessionPlayerPropertiesChanged += OnSessionPlayerPropertiesChanged;
            RemovedFromSession += OnRemovedFromSession;
            SessionDeleted += OnSessionDeleted;
        }

        // Invoke() must be called from within class containing the actions
        public void JoinSession()
        {
            SessionJoined?.Invoke();
            LocalPlayerProperties.Clear();            
        }

        private void OnSessionJoined()
        {
            Debug.Log("[SessionEventManager] SessionJoined");
        }

        private void OnSessionLeft()
        {
            Debug.Log("[SessionEventManager] SessionLeft");
        }

        private void OnSessionChanged()
        {
            Debug.Log("[SessionEventManager] SessionChanged");
        }

        private void OnSessionStateChanged(SessionState state)
        {
            Debug.Log($"[SessionEventManager] SessionStateChanged: {state}");
        }

        private void OnPlayerJoined(string id)
        {
            Debug.Log($"[SessionEventManager] PlayerJoined: {id}");
        }

        private void OnPlayerLeft(string id)
        {
            Debug.Log($"[SessionEventManager] PlayerLeft: {id}");
        }

        private void OnSessionPropertiesChanged()
        {
            Debug.Log("[SessionEventManager] SessionSessionPropertiesChanged");
        }

        private void OnSessionPlayerPropertiesChanged()
        {
            Debug.Log("[SessionEventManager] SessionPlayerPropertiesChanged");
        }

        private void OnRemovedFromSession()
        {
            Debug.Log($"[SessionEventManager] SessionRemovedFromSession");
            LocalSessionProperties.Clear();
            Session = null;
        }

        private void OnSessionDeleted()
        {
            Debug.Log($"[SessionEventManager] SessionDeleted");
        }
    }
}
#endif