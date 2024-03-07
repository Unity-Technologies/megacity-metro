using System;
using System.ComponentModel;
using Unity.Services.Vivox;
using UnityEngine;
using VivoxUnity;
using static Unity.MegacityMetro.Gameplay.VivoxEvents;

namespace Unity.MegacityMetro.Gameplay
{
    /// <summary>
    /// Manages Vivox channels
    /// </summary>
    public class VivoxChannel : MonoBehaviour
    {
        public event VivoxEvents.ParticipantValueChangedHandler OnSpeechDetectedEvent;
        public event VivoxEvents.ParticipantValueUpdatedHandler OnAudioEnergyChangedEvent;
        public event VivoxEvents.ChannelTextMessageChangedHandler OnTextMessageLogReceivedEvent;
        public event VivoxEvents.ParticipantStatusChangedHandler OnParticipantAddedEvent;
        public event VivoxEvents.ParticipantStatusChangedHandler OnParticipantRemovedEvent;
        public string Name => "MultipleUserChannel";
        private IReadOnlyDictionary<ChannelId, IChannelSession> ActiveChannels => m_Session?.ActiveChannels;

        private VivoxSession m_Session;
        private Client m_Client => VivoxService.Instance.Client;

        private void Start()
        {
            m_Session = GetComponent<VivoxSession>();
        }

        public void JoinChannel()
        {
            if (m_Session.LoginState == LoginState.LoggedIn)
            {
                var transmissionSwitch = true;
                var chatCapability = ChatCapability.TextAndAudio;
                var channel = new Channel(Name);

                var channelSession = m_Session.LoginSession.GetChannelSession(channel);
                channelSession.PropertyChanged += OnChannelPropertyChanged;
                channelSession.Participants.AfterKeyAdded += OnParticipantAdded;
                channelSession.Participants.BeforeKeyRemoved += OnParticipantRemoved;
                channelSession.Participants.AfterValueUpdated += OnParticipantValueUpdated;
                channelSession.MessageLog.AfterItemAdded += OnMessageLogRecieved;
                channelSession.BeginConnect(chatCapability != ChatCapability.TextOnly,
                    chatCapability != ChatCapability.AudioOnly, transmissionSwitch, channelSession.GetConnectToken(),
                    ar =>
                    {
                        try
                        {
                            channelSession.EndConnect(ar);
                        }
                        catch (Exception e)
                        {
                            Debug.LogException(e);
                        }
                    });
            }
        }

        public void DisconnectAllChannels()
        {
            if (ActiveChannels?.Count > 0)
            {
                foreach (var channelSession in ActiveChannels)
                {
                    channelSession?.Disconnect();
                }
            }
        }

        private void OnParticipantAdded(object sender, KeyEventArg<string> keyEventArg)
        {
            ValidateArgs(new[] {sender, keyEventArg});

            // INFO: sender is the dictionary that changed and trigger the event.  Need to cast it back to access it.
            var source = (IReadOnlyDictionary<string, IParticipant>) sender;
            // Look up the participant via the key.
            var participant = source[keyEventArg.Key];
            var username = participant.Account.Name;
            var channel = participant.ParentChannelSession.Key;
            var channelSession = participant.ParentChannelSession;

            // Trigger callback
            OnParticipantAddedEvent?.Invoke(username, channel, participant);
        }

        private void OnParticipantRemoved(object sender, KeyEventArg<string> keyEventArg)
        {
            ValidateArgs(new[] {sender, keyEventArg});

            // INFO: sender is the dictionary that changed and trigger the event.  Need to cast it back to access it.
            var source = (IReadOnlyDictionary<string, IParticipant>) sender;
            // Look up the participant via the key.
            var participant = source[keyEventArg.Key];
            var username = participant.Account.Name;
            var channel = participant.ParentChannelSession.Key;
            var channelSession = participant.ParentChannelSession;

            if (participant.IsSelf)
            {
                // Now that we are disconnected, unsubscribe.
                channelSession.PropertyChanged -= OnChannelPropertyChanged;
                channelSession.Participants.AfterKeyAdded -= OnParticipantAdded;
                channelSession.Participants.BeforeKeyRemoved -= OnParticipantRemoved;
                channelSession.Participants.AfterValueUpdated -= OnParticipantValueUpdated;
                channelSession.MessageLog.AfterItemAdded -= OnMessageLogRecieved;

                // Remove session.
                var user = m_Client.GetLoginSession(m_Session.Account);
                user.DeleteChannelSession(channelSession.Channel);
            }

            // Trigger callback
            OnParticipantRemovedEvent?.Invoke(username, channel, participant);
        }

        private static void ValidateArgs(object[] objs)
        {
            foreach (var obj in objs)
            {
                if (obj == null)
                {
                    throw new ArgumentNullException(obj.GetType().ToString(), "Specify a non-null/non-empty argument.");
                }
            }
        }

        private void OnParticipantValueUpdated(object sender, ValueEventArg<string, IParticipant> valueEventArg)
        {
            ValidateArgs(new[] {sender, valueEventArg});

            var source = (IReadOnlyDictionary<string, IParticipant>) sender;
            // Look up the participant via the key.
            var participant = source[valueEventArg.Key];

            var username = valueEventArg.Value.Account.Name;
            var channel = valueEventArg.Value.ParentChannelSession.Key;
            var property = valueEventArg.PropertyName;

            switch (property)
            {
                case "SpeechDetected":
                {
                    OnSpeechDetectedEvent?.Invoke(username, channel, valueEventArg.Value.SpeechDetected);
                    break;
                }
                case "AudioEnergy":
                {
                    OnAudioEnergyChangedEvent?.Invoke(username, channel, valueEventArg.Value.AudioEnergy);
                    break;
                }
            }
        }

        private void OnChannelPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            ValidateArgs(new[] {sender, propertyChangedEventArgs});
            var channelSession = (IChannelSession) sender;

            // IF the channel has removed audio, make sure all the VAD indicators aren't showing speaking.
            if (propertyChangedEventArgs.PropertyName == "AudioState" &&
                channelSession.AudioState == ConnectionState.Disconnected)
            {
                foreach (var participant in channelSession.Participants)
                {
                    OnSpeechDetectedEvent?.Invoke(participant.Account.Name, channelSession.Channel, false);
                }
            }

            // IF the channel has fully disconnected, unsubscribe and remove.
            if ((propertyChangedEventArgs.PropertyName == "AudioState" ||
                 propertyChangedEventArgs.PropertyName == "TextState") &&
                channelSession.AudioState == ConnectionState.Disconnected &&
                channelSession.TextState == ConnectionState.Disconnected)
            {
                // Now that we are disconnected, unsubscribe.
                channelSession.PropertyChanged -= OnChannelPropertyChanged;
                channelSession.Participants.AfterKeyAdded -= OnParticipantAdded;
                channelSession.Participants.BeforeKeyRemoved -= OnParticipantRemoved;
                channelSession.Participants.AfterValueUpdated -= OnParticipantValueUpdated;
                channelSession.MessageLog.AfterItemAdded -= OnMessageLogRecieved;

                // Remove session.
                var user = m_Client.GetLoginSession(m_Session.Account);
                user.DeleteChannelSession(channelSession.Channel);
            }
        }

        private void OnMessageLogRecieved(object sender, QueueItemAddedEventArgs<IChannelTextMessage> textMessage)
        {
            ValidateArgs(new[] {sender, textMessage});
            var channelTextMessage = textMessage.Value;
            OnTextMessageLogReceivedEvent?.Invoke(channelTextMessage.Sender.DisplayName, channelTextMessage);
        }
    }
}