using System;
using System.Threading.Tasks;
using UnityEngine;

namespace Unity.Services.Samples.GameServerHosting
{
    public class GameServerManager : MonoBehaviour
    {
        [field: SerializeField] public GameServerInfo_Data serverInfo { get; private set; }
        public GameHostingServices gameHostingServices { get; private set; }
        private PlayerAuthentication m_AuthService;
        
        public void Init(GameServerInfo_Data info)
        {
            DontDestroyOnLoad(gameObject);
            serverInfo = info;
            m_AuthService = new PlayerAuthentication();
        }

        public async Task<bool> InitServices(string profileName = null)
        {
            try
            {
                await m_AuthService.Init(profileName);
                gameHostingServices = new GameHostingServices();
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[GSH] Game Service Initialization failed, continuing without.\n{ex}");
            }

            return false;
        }

        public async Task TryStartServer(string endpoint)
        {
            try
            {
                if (await gameHostingServices.StartGameServicesAsync(endpoint, serverInfo.Data))
                {
                    Debug.Log($"[GSH] Started Game Server Hosting on {endpoint} with:\n{serverInfo.Data}!");
                }
                else
                {
                    Debug.LogWarning($"[GSH] Failed to start Game server on {endpoint}");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[GSH] Exception when starting Game server on {endpoint}.\n{ex}");
            }
        }

        private void OnDestroy()
        {
            gameHostingServices?.Dispose();
        }
    }
}
