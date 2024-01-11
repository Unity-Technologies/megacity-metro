using VivoxUnity;

namespace Unity.MegacityMetro.Gameplay
{
    public enum ChatCapability
    {
        TextOnly,
        AudioOnly,
        TextAndAudio
    }

    /// <summary>
    /// Manages Vivox channels
    /// </summary>
    public class VivoxEvents
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

        public delegate void ChannelTextMessageChangedHandler(string sender, IChannelTextMessage channelTextMessage);
        public delegate void LoginStatusChangedHandler();
        public delegate void ParticipantStatusChangedHandler(string username, ChannelId channel, IParticipant participant);
        public delegate void ParticipantValueChangedHandler(string username, ChannelId channel, bool value);
        public delegate void ParticipantValueUpdatedHandler(string username, ChannelId channel, double value);
        public delegate void RecoveryStateChangedHandler(ConnectionRecoveryState recoveryState);
    }
}