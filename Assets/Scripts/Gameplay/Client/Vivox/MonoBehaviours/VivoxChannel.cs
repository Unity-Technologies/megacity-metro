using System.Threading.Tasks;
using Unity.Services.Vivox;
using UnityEngine;

namespace Unity.MegacityMetro.Gameplay
{
    /// <summary>
    /// Manages Vivox channels
    /// </summary>
    public class VivoxChannel : MonoBehaviour
    {
        public string Name => "MultipleUserChannel";
        private bool m_Joined;
        private bool m_Connecting;

        private Task JoinLobbyChannel()
        {
            VivoxService.Instance.LoggedOut += OnUserLoggedOut;
            VivoxService.Instance.ConnectionRecovered += OnConnectionRecovered;
            VivoxService.Instance.ConnectionRecovering += OnConnectionRecovering;
            VivoxService.Instance.ConnectionFailedToRecover += OnConnectionFailedToRecover;
            return VivoxService.Instance.JoinGroupChannelAsync(Name, ChatCapability.AudioOnly);
        }
        
        private void OnDestroy()
        {
            if (VivoxService.Instance != null)
            {
                if (m_Joined)
                {
                    VivoxService.Instance.LeaveChannelAsync(Name);
                    m_Joined = false;
                    m_Connecting = false;
                }

                VivoxService.Instance.ConnectionRecovered -= OnConnectionRecovered;
                VivoxService.Instance.ConnectionRecovering -= OnConnectionRecovering;
                VivoxService.Instance.ConnectionFailedToRecover -= OnConnectionFailedToRecover;   
            }
        }

        private void OnUserLoggedOut()
        {
            VivoxService.Instance.LoggedOut -= OnUserLoggedOut;
            VivoxService.Instance.ConnectionRecovered -= OnConnectionRecovered;
            VivoxService.Instance.ConnectionRecovering -= OnConnectionRecovering;
            VivoxService.Instance.ConnectionFailedToRecover -= OnConnectionFailedToRecover;
        }

        private void OnConnectionRecovering()
        {
            //sets UI with .text = "Connection Recovering";
        }

        void OnConnectionRecovered()
        {
            //sets UI with .text = "Connection Recovered";
        }

        void OnConnectionFailedToRecover()
        {
            //sets UI with .text = "Connection Failed to Recover";
        }

        public async void JoinChannel()
        {
            if(m_Connecting)
                return;
            
            m_Connecting = true;
            // double check all channels are inactive before joining to a channel
            await VivoxService.Instance.LeaveAllChannelsAsync();
            await JoinLobbyChannel();
            m_Joined = true;
        }
    }
}