using Unity.Entities;
using Unity.Physics;

namespace Gameplay.Misc.Components
{
    public struct GeneralSettings : IComponentData
    {
        public CollisionFilter LightsDetectionFilter;
        public float MaxLightsDetectionRange;
        public int MaxLights;
        public float LightsFadeInDuration;
        public float ForwardBias;
        public float MinDistanceForForwardBias;

        public BlobAssetReference<Collider> LightsCollider;
    }
}