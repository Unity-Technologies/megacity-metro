using System;
using UnityEngine;

namespace Unity.NAME.Game
{
    [DefaultExecutionOrder(100)]
    public class ThirdPersonCamera : BaseCamera
    {
        // Against our conventions, using fields instead of properties in order
        // to have more performant and cleaner Update() code.
        [NonSerialized]
        public Vector3 HorizontalVectorReference;

        [NonSerialized]
        public Vector3 VerticalVectorReference;

        void OnEnable()
        {
            CameraService.RegisterCamera(this);
        }

        void OnDisable()
        {
            CameraService?.UnregisterCamera(this);
        }

        void Update()
        {
            HorizontalVectorReference.Set(transform.right.x, 0.0f, transform.right.z);
            HorizontalVectorReference.Normalize();

            VerticalVectorReference.Set(transform.forward.x, 0.0f, transform.forward.z);
            VerticalVectorReference.Normalize();
        }
    }
}
