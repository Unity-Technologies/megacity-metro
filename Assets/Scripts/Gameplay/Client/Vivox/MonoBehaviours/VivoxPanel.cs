using UnityEngine;
using UnityEngine.UIElements;
using VivoxUnity;

namespace Unity.MegacityMetro.Gameplay
{
    /// <summary>
    /// Adds events to Vivox UI
    /// </summary>
    public class VivoxPanel : MonoBehaviour
    {
        private VivoxSession VivoxSession => VivoxManager.Instance.Session;
        private VivoxChannel VivoxChannel => VivoxManager.Instance.Channel;
        private VivoxDevicesVolume VivoxDevices => VivoxManager.Instance.Devices;
        private VisualElement m_VoiceChat;

        private void Awake()
        {
            var root = GetComponent<UIDocument>().rootVisualElement;
            m_VoiceChat = root.Q<VisualElement>("chat-voice-panel");
            m_VoiceChat.style.visibility = Visibility.Hidden;
        }

        private void Start()
        {
            if(VivoxManager.Instance == null || VivoxSession == null)
                return;
            
            VivoxSession.OnUserLoggedInEvent += ShowPanel;
            VivoxSession.OnUserLoggedInEvent += OnUserLoggedIn;
            VivoxSession.OnUserLoggedOutEvent += OnUserLoggedOut;
            VivoxSession.OnUserLoggedOutEvent += HidePanel;

            if (VivoxSession.LoginState == LoginState.LoggedIn)
            {
                OnUserLoggedIn();
            }
            else
            {
                OnUserLoggedOut();
            }
        }

        private void OnDestroy()
        {
            if (VivoxManager.Instance == null || VivoxSession == null)
                return;
            VivoxSession.OnUserLoggedInEvent -= OnUserLoggedIn;
            VivoxSession.OnUserLoggedOutEvent -= OnUserLoggedOut;
            VivoxSession.OnUserLoggedOutEvent -= HidePanel;
            VivoxSession.OnUserLoggedInEvent -= ShowPanel;
        }

        private void ShowPanel()
        {
            m_VoiceChat.style.visibility = Visibility.Visible;
            VivoxDevices.SetMicrophoneMute(true);
        }

        private void HidePanel()
        {
            m_VoiceChat.style.visibility = Visibility.Hidden;
            VivoxDevices.SetMicrophoneMute(true);
        }

        private void OnUserLoggedIn()
        {
            var lobbyChannel = VivoxSession.GetChannelSessionByName(VivoxChannel.Name);
            if (VivoxSession.ActiveChannels.Count == 0 || lobbyChannel == null)
            {
                // Do nothing, participant added will take care of this
                VivoxChannel.JoinChannel();
            }
            else
            {
                if (lobbyChannel.AudioState == ConnectionState.Disconnected)
                {
                    // Ask for hosts since we're already in the channel and part added won't be triggered.
                    lobbyChannel.BeginSetAudioConnected(true, true,
                        ar => { Debug.Log("Now transmitting into lobby channel"); });
                }
            }
        }

        private void OnUserLoggedOut()
        {
            VivoxChannel.DisconnectAllChannels();
        }
    }
}
