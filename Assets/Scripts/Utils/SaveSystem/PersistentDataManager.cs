using UnityEngine;

namespace Unity.MegacityMetro.Utils
{
    public class PersistentDataManager : MonoBehaviour
    {
        [SerializeField] private UserPreferencesSO userPreferences;
        public static PersistentDataManager Instance { get; private set; }
        
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;

                // If the user preferences are not set, load them from the resources folder
                if (userPreferences == null)
                {
                    userPreferences = Resources.Load<UserPreferencesSO>("UserPreferences");
                }
                    
            } 
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            if (TryGetUserSettingsData(out var preferences))
            {
                userPreferences.UserSettings = preferences;
            }
            else
            {
                SaveUserSettingsData(userPreferences.UserSettings);
            }
        }

        private void SaveUserSettingsData(UserSettingsData userSettings)
        {
            var json = JsonUtility.ToJson(userSettings);
            FileManager.WriteToFile("user_preferences.json", json);
        }

        private bool TryGetUserSettingsData(out UserSettingsData preferences)
        {
            if (FileManager.LoadFromFile("user_preferences.json", out var json))
            {
                preferences = JsonUtility.FromJson<UserSettingsData>(json);
                return true;
            }
            
            preferences = userPreferences.UserSettings;
            return false;
        }

        public void SaveGraphicsSettings(GraphicsSettingsData graphicsSettingsData)
        {
            userPreferences.UserSettings.GraphicsSettings = graphicsSettingsData;
            SaveUserSettingsData(userPreferences.UserSettings);
        }

        public GraphicsSettingsData GetGraphicsSettings()
        {
            return userPreferences.UserSettings.GraphicsSettings;
        }
        
        public void SaveAudioSettings(AudioSettingsData audioSettingsData)
        {
            userPreferences.UserSettings.AudioSettings = audioSettingsData;
            SaveUserSettingsData(userPreferences.UserSettings);
        }
        
        public AudioSettingsData GetAudioSettings()
        {
            return userPreferences.UserSettings.AudioSettings;
        }
        
        public void SaveControlSettings(ControlSettingsData controlSettingsData)
        {
            userPreferences.UserSettings.ControlSettings = controlSettingsData;
            SaveUserSettingsData(userPreferences.UserSettings);
        }
        
        public ControlSettingsData GetControlSettings()
        {
            return userPreferences.UserSettings.ControlSettings;
        }
    }
}