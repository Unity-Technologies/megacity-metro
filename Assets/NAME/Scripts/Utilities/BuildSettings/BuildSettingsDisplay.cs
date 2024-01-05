using UnityEngine;

namespace Unity.NAME.Utilities
{
    public class BuildSettingsDisplay : MonoBehaviour
    {
        public TextMesh BuildName;

        public BuildSettings BuildSettings;

        void Awake()
        {
            BuildName.text = $"{BuildSettings.BuildName}\n{BuildSettings.BuildType}\n{BuildSettings.ShaderType}\n";
            BuildName.text += $"Resolution: {Screen.width} {Screen.height}";
            gameObject.SetActive(false);
        }
    }
}
