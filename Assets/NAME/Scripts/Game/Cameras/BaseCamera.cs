using UnityEngine;

namespace Unity.NAME.Game
{
    // An abstract class for the representation of a base camera

    public abstract class BaseCamera : MonoBehaviour
    {
        protected ICameraService CameraService;

        protected virtual void Awake()
        {
            CameraService = ServiceLocator.GetService<ICameraService>();
        }
    }
}
