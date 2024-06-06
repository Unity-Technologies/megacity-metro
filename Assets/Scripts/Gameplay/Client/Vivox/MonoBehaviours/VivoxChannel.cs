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

        private Task JoinLobbyChannel()
        {
            VivoxService.Instance.LoggedOut += OnUserLoggedOut;
            VivoxService.Instance.ConnectionRecovered += OnConnectionRecovered;
            VivoxService.Instance.ConnectionRecovering += OnConnectionRecovering;
            VivoxService.Instance.ConnectionFailedToRecover += OnConnectionFailedToRecover;
            return VivoxService.Instance.JoinGroupChannelAsync(Name, ChatCapability.AudioOnly);
        }
        
        void OnDestroy()
        {
            VivoxService.Instance.ConnectionRecovered -= OnConnectionRecovered;
            VivoxService.Instance.ConnectionRecovering -= OnConnectionRecovering;
            VivoxService.Instance.ConnectionFailedToRecover -= OnConnectionFailedToRecover;
        }

        private void OnUserLoggedOut()
        {
            
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
            await JoinLobbyChannel();
        }

        public void DisconnectAllChannels()
        {
            
        }
    }
}