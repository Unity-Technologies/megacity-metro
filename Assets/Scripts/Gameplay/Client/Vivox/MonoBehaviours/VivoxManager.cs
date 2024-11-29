using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Vivox;
using System.Runtime.InteropServices;
using Unity.Services.Authentication;
using System.Threading.Tasks;

namespace Unity.MegacityMetro.Gameplay
{
    /// <summary>
    /// Manages Vivox service
    /// </summary>
    [RequireComponent(typeof(VivoxSession))]
    [RequireComponent(typeof(VivoxChannel))]
    [RequireComponent(typeof(VivoxDevicesVolume))]
    public class VivoxManager : MonoBehaviour
    {
        public const string k_MultipleVivoxComponentDetected = "Multiple VivoxVoiceManager detected in the scene. " +
                                                               "Only one VivoxVoiceManager can exist at a time. " +
                                                               "The duplicate VivoxVoiceManager will be destroyed.";

        public const string k_SetupProjectInTheCloud = "To use Unity's dashboard services, " +
                                                       "you need to link your Unity project to a project ID. " +
                                                       "To do this, go to Project Settings to select your organization, " +
                                                       "select your project and then link a project ID. " +
                                                       "You also need to make sure your organization has access to the required products. " +
                                                       "Visit https://dashboard.unity3d.com to sign up.";
        
        //These variables should be set to the projects Vivox credentials if the authentication package is not being used
        //Credentials are available on the Vivox Developer Portal (developer.vivox.com) or the Unity Dashboard (dashboard.unity3d.com), depending on where the organization and project were made
        [SerializeField]
        string key;
        [SerializeField]
        string issuer;
        [SerializeField]
        string domain;
        [SerializeField]
        string server;
        
        public static VivoxManager Instance { get; private set; }
        public VivoxDevicesVolume Devices { get; private set; }
        public VivoxSession Session { get; private set; }
        public VivoxChannel Channel { get; private set; }
        private bool m_ServiceInitialized;
        private bool m_IsReady;

        public IVivoxService Service
        {
            get
            {
                if (!m_ServiceInitialized)
                    return null;
                return VivoxService.Instance;
            }
        }

        private void Awake()
        {
#if UNITY_SERVER
            Debug.Log("[VIVOX] Disabling Vivox on the server ---------");
            Destroy(gameObject);
            return;
#else
            if (RuntimeInformation.ProcessArchitecture == Architecture.Arm64 && Application.platform == RuntimePlatform.OSXPlayer)
            {
                Destroy(gameObject);
                return;
            }
            
            if (Instance != this && Instance != null)
            {
                Debug.LogWarning(k_MultipleVivoxComponentDetected);
                Destroy(gameObject);
                return;
            }
            
            if (string.IsNullOrEmpty(Application.cloudProjectId))
            {
                Debug.LogWarning(k_SetupProjectInTheCloud);
                Destroy(gameObject);
                return;
            }

            Instance = this;
            m_IsReady = false;
            Devices = GetComponent<VivoxDevicesVolume>();
            Session = GetComponent<VivoxSession>();
            Channel = GetComponent<VivoxChannel>();
#endif
        }

        private async void Start()
        {
            // if the Unity project is not linked to a Unity services project.
            Debug.Log($"[VIVOX] Vivox application project {Application.cloudProjectId}");
            if (string.IsNullOrEmpty(Application.cloudProjectId))
            {
                Debug.LogWarning(k_SetupProjectInTheCloud);
                return;
            }

            var options = new InitializationOptions();
            if (CheckManualCredentials())
            {
                options.SetVivoxCredentials(server, domain, issuer, key);
            }
            
            await UnityServices.InitializeAsync(options);

            if (!CheckManualCredentials() && AuthenticationService.Instance.SessionTokenExists)
            {
                AuthenticationService.Instance.ClearSessionToken();
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
            }


            if (UnityServices.State == ServicesInitializationState.Initialized && VivoxService.Instance != null)
            {
                m_IsReady = true;
                await VivoxService.Instance.InitializeAsync();
                Debug.Log($"[VIVOX] starting Vivox service");
                m_ServiceInitialized = true;
            }
            else 
            {
                Debug.Log($"[VIVOX] State {UnityServices.State} VivoxService Instance Is null: {VivoxService.Instance == null}");
            }
        }

        private async void OnDestroy()
        {
            await Logout();
            Instance = null;
        }

        public async Task Logout()
        {
            // Needed to add this to prevent some unsuccessful init, we can revisit to do better
            if (m_IsReady && Session != null && Devices != null)
            {
                Devices.SetMicrophoneMute(true);
                await Session.ClosingClientConnection();
                Debug.Log($"[VIVOX] Logout from Vivox");
            }
        }
        
        private bool CheckManualCredentials()
        {
            return !(string.IsNullOrEmpty(issuer) && string.IsNullOrEmpty(domain) && string.IsNullOrEmpty(server));
        }
    }
}
