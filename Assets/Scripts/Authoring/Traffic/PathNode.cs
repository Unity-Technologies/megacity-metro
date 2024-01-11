using System;
using Unity.Mathematics;
using UnityEngine;

namespace Unity.MegacityMetro.Traffic
{
    /// <summary>
    /// Creates a node that is used from the Path.
    /// </summary>
    [ExecuteInEditMode]
    public class PathNode : MonoBehaviour
    {
        private void OnDestroy()
        {
            var parent = transform.parent.GetComponent<Path>();
            if (parent != null)
            {
                parent.NodeDeleted();
            }
        }
    }

    [Serializable]
    public struct RoadWaypoint
    {
        public float3 position;
    }
}