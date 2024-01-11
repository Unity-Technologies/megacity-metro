using System.Collections.Generic;
using UnityEngine;

namespace Unity.MegacityMetro.UI
{
    /// <summary>
    /// UI element for switching between screen resolutions
    /// </summary>
    public class UIScreenResolution : MonoBehaviour
    {
        // Define multiple resolutions to switch between
        [System.Serializable]
        public struct Resolution
        {
            public int width;
            public int height;
            
            public override string ToString()
            {
                return $"{width}x{height}";
            }
        }

        [SerializeField] private List<Resolution> resolutions;

        private int currentResolutionIndex;

        public int CurrentResolutionIndex => currentResolutionIndex;

        private void Start()
        {
            // Ignore this script if we're on mobile
#if UNITY_STANDALONE
            // Get the screen's aspect ratio
            var screenAspectRatio = Screen.width / (float) Screen.height;
            // Find the best resolution based on the screen's aspect ratio
            var bestAspectRatioDifference = float.MaxValue;

            for (int i = 0; i < resolutions.Count; i++)
            {
                var resolution = resolutions[i];
                var resolutionAspectRatio = resolution.width / (float) resolution.height;
                var aspectRatioDifference = Mathf.Abs(screenAspectRatio - resolutionAspectRatio);

                if (aspectRatioDifference < bestAspectRatioDifference)
                {
                    bestAspectRatioDifference = aspectRatioDifference;
                    currentResolutionIndex = i;
                }
            }
#endif
        }

        public void SetResolution(string value)
        {
            for (var i = 0; i < resolutions.Count; i++)
            {
                if (resolutions[i].ToString().Contains(value))
                    SetResolution(i);
            }
        }

        private void SetResolution(int index)
        {
            // Update the current resolution index
            currentResolutionIndex = index;
            // Get the current resolution
            var currentResolution = resolutions[CurrentResolutionIndex];
            // Set the screen resolution and fullscreen mode
            var isFullscreen = Screen.fullScreen;
            Screen.SetResolution(currentResolution.width, currentResolution.height, isFullscreen);
        }

        public void SetScreenMode(string value)
        {
            if (value == FullScreenMode.Windowed.ToString().ToLower())
            {
                Screen.fullScreen = false;
                Screen.fullScreenMode = FullScreenMode.Windowed;
            }
            else
            {
                Screen.fullScreen = true;
                Screen.fullScreenMode = FullScreenMode.FullScreenWindow;
            }
        }

        public List<string> GetResolutionOptions()
        {
            var value = new List<string>();
            foreach (var resolution in resolutions)
            {
                value.Add($"{resolution.width}x{resolution.height}");
            }

            return value;
        }

        public List<string> GetResolutionModes()
        {
            var value = new List<string>
            {
                $"{FullScreenMode.Windowed}",
                $"{FullScreenMode.FullScreenWindow}"
            };
            return value;
        }
    }
}