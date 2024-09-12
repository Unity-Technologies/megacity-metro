using Unity.Entities;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Unity.MegacityMetro.Gameplay
{
    public class BlimpAuthoring : MonoBehaviour
    {
        public float Radius = 80.0f;
        public float CircleLookAheadValue = 0.1f;
        public float RoationSpeed = 1f;

#if UNITY_EDITOR
        private Color GizmoColor = new Color(0.85f, 0.85f, 0.1f, 0.5f);

        private void OnDrawGizmos()
        {
            // the blimp actually travels on this radius, so we want to display this for the user
            // who is placing the entity!
            Handles.matrix = transform.localToWorldMatrix;
            Handles.color = GizmoColor;
            Handles.DrawWireDisc(Vector3.zero, Vector3.up, Radius, 3);
        }
#endif

        [BakingVersion("megacity-metro", 1)]
        public class BlimpBaker : Baker<BlimpAuthoring>
        {
            public override void Bake(BlimpAuthoring authoring)
            {
                var entity = GetEntity(authoring.gameObject, TransformUsageFlags.Dynamic);
                AddComponent(entity, new BlimpComponent
                {
                    StartingPosition = authoring.transform.position,
                    TravelRadius = authoring.Radius,
                    RandomStartPoint = Random.Range(0.0f, 360.0f),
                    LookAheadValue = authoring.CircleLookAheadValue,
                    RotationSpeed = authoring.RoationSpeed / authoring.Radius,
                    Rotation = 0
                });
            }
        }
    }
}