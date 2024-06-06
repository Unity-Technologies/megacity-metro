using System;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.UIElements;
using Unity.Services.Vivox;
using Random = Unity.Mathematics.Random;

namespace Unity.MegacityMetro.Gameplay
{
    /// <summary>
    /// Detects the talker and updates UI Vivox player's name. 
    /// </summary>
    public class VivoxPlayerList : MonoBehaviour
    {
        [SerializeField] public Sprite m_MicrophoneOn;
        [SerializeField] public Sprite m_MicrophoneOff;
        [SerializeField] public Vector2 m_MinMaxScaleIndicatorSpeaker;

        private VivoxParticipant lastParticipant;

        private Button m_MicrophoneButton;
        private VisualElement m_MicrophoneIcon;
        private Random m_Random;
        private Label m_SpeakerName;
        private VisualElement m_SpeakerSignal;
        private bool IsMicrophoneMuted = false;
        private Dictionary<string, VivoxParticipant> m_Participants;

        private void OnEnable()
        {
            var root = GetComponent<UIDocument>().rootVisualElement;
            var container = root.Q<VisualElement>("chat-voice-panel");
            m_Participants = new Dictionary<string, VivoxParticipant>();
            m_SpeakerName = container.Q<Label>("vivox-name");
            m_MicrophoneButton = container.Q<Button>("vivox-button");
            m_SpeakerSignal = m_MicrophoneButton.Q<VisualElement>("speaker-signal");
            m_MicrophoneIcon = m_MicrophoneButton.Q<VisualElement>("vivox-icon");
            m_Random = new Random((uint) Time.realtimeSinceStartup + (uint) DateTime.Now.Ticks);
            m_MicrophoneButton.clicked += OnMuteToggle;
        }

        private void Update()
        {
            if (VivoxManager.Instance == null || VivoxManager.Instance.Devices == null)
                return;

            if (IsMicrophoneMuted != VivoxManager.Instance.Devices.Muted)
            {
                IsMicrophoneMuted = VivoxManager.Instance.Devices.Muted;
                var style = new StyleBackground(IsMicrophoneMuted ? m_MicrophoneOff : m_MicrophoneOn);
                m_MicrophoneIcon.style.backgroundImage = style;
                m_MicrophoneIcon.EnableInClassList("icon-magenta", IsMicrophoneMuted);
                m_MicrophoneButton.EnableInClassList("vivox-button-magenta", IsMicrophoneMuted);
                m_SpeakerSignal.style.display = IsMicrophoneMuted ? DisplayStyle.None : DisplayStyle.Flex;
            }
        }

        public void Init()
        {
            if (VivoxManager.Instance != null && VivoxManager.Instance.Channel != null)
            {
                VivoxManager.Instance.Service.ParticipantAddedToChannel += OnParticipantAdded;
                VivoxManager.Instance.Service.ParticipantRemovedFromChannel += OnParticipantRemoved;
            }
        }

        private void OnDestroy()
        {
            if (VivoxManager.Instance != null && VivoxManager.Instance.Channel != null)
            {
                VivoxManager.Instance.Service.ParticipantAddedToChannel -= OnParticipantAdded;
                VivoxManager.Instance.Service.ParticipantRemovedFromChannel -= OnParticipantRemoved;
            }
        }

        private void OnParticipantAdded(VivoxParticipant participant)
        {
            if(!m_Participants.ContainsKey(participant.PlayerId))
                m_Participants.Add(participant.PlayerId, participant);
            participant.ParticipantSpeechDetected += ParticipantOnParticipantSpeechDetected;
        }

        private void OnParticipantRemoved(VivoxParticipant participant)
        {
            if (m_Participants.ContainsKey(participant.PlayerId))
                m_Participants.Remove(participant.PlayerId);
            participant.ParticipantSpeechDetected -= ParticipantOnParticipantSpeechDetected;
        }

        public void OnMuteToggle()
        { 
            VivoxManager.Instance?.Devices.SetMicrophoneMute(!VivoxManager.Instance.Devices.Muted);
        }

        private void ParticipantOnParticipantSpeechDetected()
        {
            foreach(KeyValuePair<string, VivoxParticipant> item in m_Participants)
            {
                if (item.Value.SpeechDetected)
                {
                    var participant = item.Value;

                    if (participant != null && lastParticipant == null && participant.SpeechDetected)
                    {
                        lastParticipant = participant;
                        m_SpeakerName.text = participant.DisplayName;
                        AnimateIcon();
                    }
                    else if (participant != null && lastParticipant == participant && !participant.SpeechDetected)
                    {
                        lastParticipant = null;
                        m_SpeakerSignal.style.scale = new StyleScale(new Vector2(1, 0.5f));
                        m_SpeakerSignal.style.opacity = 0;
                    }
                    
                    break;
                }
            }
        }

        private void AnimateIcon()
        {
            if (lastParticipant == null)
            {
                return;
            }

            var currentScale = (Vector2) m_SpeakerSignal.transform.scale;
            var nextScale = new Vector2(1, m_Random.NextFloat(m_MinMaxScaleIndicatorSpeaker.x, m_MinMaxScaleIndicatorSpeaker.y));
            m_SpeakerSignal.style.opacity = 1;
            m_SpeakerSignal.experimental.animation.Start(currentScale, nextScale, 100, (element, value) =>
            {
                element.transform.scale = value;
            }).OnCompleted(AnimateIcon);
        }
    }
}