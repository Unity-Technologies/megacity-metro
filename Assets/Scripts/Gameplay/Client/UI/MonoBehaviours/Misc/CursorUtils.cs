using UnityEngine;

namespace Unity.MegacityMetro.UI
{
    /// <summary>
    /// Utility class for managing the cursor.
    /// </summary>
    public static class CursorUtils
    {
        public static void ShowCursor()
        {
            // TODO: Ignore this script if we're on mobile
#if !(UNITY_ANDROID || UNITY_IPHONE)
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
#endif
        }

        public static void HideCursor()
        {
            // TODO: Ignore this script if we're on mobile
#if !(UNITY_ANDROID || UNITY_IPHONE)
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
#endif
        }
    }
}
