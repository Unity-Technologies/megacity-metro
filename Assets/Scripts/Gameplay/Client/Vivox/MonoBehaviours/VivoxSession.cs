using System.Threading.Tasks;
using Unity.Services.Vivox;
using UnityEngine;

namespace Unity.MegacityMetro.Gameplay
{
    /// <summary>
    /// Manages Vivox session
    /// </summary>
    public class VivoxSession : MonoBehaviour
    {
        public bool IsConnecting { get; private set; }
        public bool IsLogged => VivoxService.Instance.IsLoggedIn;

        public async Task Login(string displayName)
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

            //closing prior connection
            await ClosingClientConnection();
            await LoginToVivoxAsync(displayName);
        }
        
        private async Task LoginToVivoxAsync(string displayName)
        {
            IsConnecting = true;
            LoginOptions options = new LoginOptions();
            options.DisplayName = displayName;
            options.ParticipantUpdateFrequency = ParticipantPropertyUpdateFrequency.FivePerSecond;
            Debug.Log($"[VIVOX] Starting login user {displayName}");
            await VivoxService.Instance.LoginAsync(options);
            IsConnecting = false;
        }

        public async Task ClosingClientConnection()
        {
            if (VivoxService.Instance.IsLoggedIn && VivoxService.Instance.ActiveChannels.Count > 0)
            {
                await VivoxService.Instance.LeaveAllChannelsAsync();
                await VivoxService.Instance.LogoutAsync();
            }
        }
    }
}