using System;
using System.ComponentModel;
using System.Linq;
using Unity.Services.Vivox;
using UnityEngine;
using VivoxUnity;
using static Unity.MegacityMetro.Gameplay.VivoxEvents;

namespace Unity.MegacityMetro.Gameplay
{
    /// <summary>
    /// Manages Vivox session
    /// </summary>
    public class VivoxSession : MonoBehaviour
    {
        public event VivoxEvents.RecoveryStateChangedHandler OnRecoveryStateChangedEvent;
        public event VivoxEvents.LoginStatusChangedHandler OnUserLoggedInEvent;
        public event VivoxEvents.LoginStatusChangedHandler OnUserLoggedOutEvent;
        public LoginState LoginState { get; private set; }
        public ILoginSession LoginSession;
        public Account Account => m_Account;
        public bool IsConnecting { get; private set; }
        public bool IsLogged { get; private set; }

        public IReadOnlyDictionary<ChannelId, IChannelSession> ActiveChannels => LoginSession?.ChannelSessions;

        private Account m_Account;

        public void Login(string displayName)
        {
            if (IsConnecting)
                return;

            if (string.IsNullOrEmpty(displayName))
            {
                Debug.LogWarning("[VIVOX] The user's DisplayName is missing or invalid. Vivox will utilize the default value as a fallback.");
                displayName = "Player";
            }

            if (VivoxService.Instance == null)
            {
                Debug.Log($"VivoxService.Instance is null: {VivoxService.Instance}");
                return;
            }

            Debug.Log($"[VIVOX] Player {displayName} is trying to login");
            Debug.Log($"[VIVOX] Issuer {VivoxService.Instance.Issuer}");
            Debug.Log($"[VIVOX] PlayerId {VivoxService.Instance.PlayerId ?? Guid.NewGuid().ToString()}");
            Debug.Log($"[VIVOX] Domain {VivoxService.Instance.Domain}");
            Debug.Log($"[VIVOX] EnvironmentID {VivoxService.Instance.EnvironmentId}");


            IsConnecting = true;
            m_Account = new Account(displayName);
            LoginSession = VivoxService.Instance.Client.GetLoginSession(m_Account);
            LoginSession.PropertyChanged += OnLoginSessionPropertyChanged;
            LoginSession.BeginLogin(LoginSession.GetLoginToken(), SubscriptionMode.Accept, null, null, null, ar =>
            {
                try
                {
                    LoginSession.EndLogin(ar);
                }
                catch (Exception e)
                {
                    // Unbind if we failed to login.
                    Debug.LogException(e);
                    LoginSession.PropertyChanged -= OnLoginSessionPropertyChanged;
                }
            });
        }

        private void OnLoginSessionPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            if (propertyChangedEventArgs.PropertyName == "RecoveryState")
            {
                OnRecoveryStateChangedEvent?.Invoke(LoginSession.RecoveryState);
                return;
            }

            if (propertyChangedEventArgs.PropertyName != "State")
            {
                return;
            }

            var loginSession = (ILoginSession) sender;
            LoginState = loginSession.State;
            switch (LoginState)
            {
                case LoginState.LoggedIn:
                {
                    IsLogged = true;
                    OnUserLoggedInEvent?.Invoke();
                    break;
                }
                case LoginState.LoggedOut:
                {
                    OnUserLoggedOutEvent?.Invoke();
                    LoginSession.PropertyChanged -= OnLoginSessionPropertyChanged;
                    break;
                }
            }

            IsConnecting = false;
        }

        public void Logout()
        {
            if (LoginSession != null && LoginSession.State == LoginState.LoggedIn)
                LoginSession.Logout();
        }

        public void ClosingClientConnection()
        {
            Client.Cleanup();

            var client = VivoxService.Instance.Client;
            if (client != null)
            {
                IsLogged = false;
                client.Uninitialize();
            }
        }

        public IChannelSession GetChannelSessionByName(string channelName)
        {
            return ActiveChannels.FirstOrDefault(ac => ac.Channel.Name == channelName);
        }
    }
}