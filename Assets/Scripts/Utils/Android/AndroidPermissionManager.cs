using UnityEngine;
#if UNITY_ANDROID
using UnityEngine.Android;
#endif

namespace MegacityMetro.Utils.Android
{
    /// <summary>
    /// Manages Android permissions
    /// </summary>
    public class AndroidPermissionManager : MonoBehaviour
    {
#if UNITY_ANDROID
        private void Start()
        {
            if (Permission.HasUserAuthorizedPermission(Permission.Microphone))
            {
                Debug.Log("User authorized use of the microphone.");
            }
            else
            {
                // We do not have permission to use the microphone.
                // Ask for permission or proceed without the functionality enabled.
                Permission.RequestUserPermission(Permission.Microphone);
            }
        }
#endif
    }
}
