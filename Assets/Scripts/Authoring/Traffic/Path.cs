using System.Collections.Generic;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;

namespace Unity.MegacityMetro.Traffic
{
    /// <summary>
    /// Allows to connect and draw the path based on nodes placed in the scene.
    /// </summary>
    [ExecuteInEditMode]
#if UNITY_EDITOR
    [CanEditMultipleObjects]
#endif
    public class Path : MonoBehaviour
    {
        static readonly float defaultPathLength = 100.0f;
        public float width = 16;
        public float height = 16;

        public bool isReversed = false;
        public bool isOnRamp = false;
        public float percentageChanceForOnRamp = 0.0f;
        public float minSpeed = Constants.VehicleSpeedMin;
        public float maxSpeed = Constants.VehicleSpeedMax;

        TrafficConfigAuthoring _rootObject = null;

        [SerializeField] [HideInInspector] private int version = 0;

        [HideInInspector] public List<RoadWaypoint> Waypoints = new();

#if UNITY_EDITOR
        private static bool showColoured;

        public static void SetShowColoured(bool v)
        {
            showColoured = v;
        }

        public static bool GetShowColoured()
        {
            return showColoured;
        }
#endif

        public PathNode[] GetNodes()
        {
            return GetComponentsInChildren<PathNode>();
        }

        public int GetNumNodes()
        {
            return Waypoints.Count;
        }

        private void Awake()
        {
            if (version == 0)
            {
                PathNode[] children = GetNodes();

                if (children.Length > 0)
                {
                    Waypoints.Clear();

                    for (int a = 0; a < children.Length; a++)
                    {
                        Waypoints.Add(new RoadWaypoint
                            {position = children[a].transform.position - transform.position});
                    }
                }

                version = 1;
            }

            Reset();
        }

        private void Reset()
        {
            if (GetNumNodes() < 2)
            {
                AddNodes(2 - GetNumNodes(), -1);
            }
        }

        public void AddNewNode(int index)
        {
            AddNodes(1, index);
        }

        public void NodeDeleted()
        {
        }

        private void InsertNodeAtEnd(int count)
        {
            Vector3 lastPosition = Vector3.zero;
            Vector3 nextDirection = transform.forward;

            if (GetNumNodes() >= 2)
            {
                nextDirection = Waypoints[GetNumNodes() - 1].position - Waypoints[GetNumNodes() - 2].position;
                nextDirection.Normalize();
            }

            if (GetNumNodes() > 0)
            {
                lastPosition = Waypoints[GetNumNodes() - 1].position;
                lastPosition += nextDirection * defaultPathLength;
            }

            for (int i = 0; i < count; i++)
            {
                Waypoints.Add(new RoadWaypoint {position = lastPosition});
                lastPosition += nextDirection * defaultPathLength;
            }
        }

        private void AddNodes(int count, int index)
        {
            if (index == -1 || index == GetNumNodes() - 1)
            {
                InsertNodeAtEnd(count);
            }
            else
            {
                // insert at halfway along the path given by index
                bool oldReverse = isReversed;
                isReversed = false;
                float3 newPos = GetWorldPosition(((float) index) + 0.5f) - new float3(transform.position);
                Waypoints.Insert(index + 1, new RoadWaypoint {position = newPos});
                isReversed = oldReverse;
            }
        }

#if UNITY_EDITOR
        [DrawGizmo(GizmoType.Active | GizmoType.NotInSelectionHierarchy
                                    | GizmoType.InSelectionHierarchy | GizmoType.Pickable, typeof(Path))]
        private static void DrawGizmos(Path path, GizmoType gizmoType)
        {
            if (path._rootObject == null)
            {
                path._rootObject = path.GetComponentInParent<TrafficConfigAuthoring>();
                return;
            }

            float segmentation = 1.0f / 10f;

            if (path.Waypoints.Count < 2)
            {
                return;
            }

            Vector3 centerOfPrev = path.GetWorldPosition(0.0f);
            Vector3 centerOfNext = path.GetWorldPosition(segmentation);
            Vector3 dir = math.normalize(centerOfNext - centerOfPrev);
            Vector3 leftVec = math.cross(Vector3.up, dir);
            Vector3 upH = math.cross(dir, leftVec) * (path.height / 2.0f);
            Vector3 leftVecW = leftVec * (path.width / 2.0f);

            Vector3 pul = centerOfPrev + upH + leftVecW;
            Vector3 pur = centerOfPrev + upH - leftVecW;
            Vector3 pdr = centerOfPrev - upH - leftVecW;
            Vector3 pdl = centerOfPrev - upH + leftVecW;

            if (Selection.activeGameObject != null && Selection.activeGameObject.GetComponent<Path>() == path)
            {
                Gizmos.color = Color.white;
            }
            else
            {
                Gizmos.color = new Color32(48, 48, 48, 255);
            }

            //4 lines to make a box cap
            Gizmos.DrawLine(pul, pur);
            Gizmos.DrawLine(pur, pdr);
            Gizmos.DrawLine(pdr, pdl);
            Gizmos.DrawLine(pdl, pul);

            //Also draw path direction Arrow
            Gizmos.DrawLine(centerOfPrev, centerOfNext);
            Gizmos.DrawLine(centerOfNext, centerOfNext - dir + Vector3.up);
            Gizmos.DrawLine(centerOfNext, centerOfNext - dir - Vector3.up);
            Gizmos.DrawLine(centerOfNext, centerOfNext - dir + leftVec);
            Gizmos.DrawLine(centerOfNext, centerOfNext - dir - leftVec);

            int numNodes = path.GetNumNodes() - 1;
            for (float f = segmentation; f < numNodes; f += segmentation)
            {
                centerOfNext = path.GetWorldPosition(f);
                Vector3 up, left;
                GetOffsetFromPathSegment(path, f, out up, out left);

                Vector3 nul = centerOfNext + up + left;
                Vector3 nur = centerOfNext + up - left;
                Vector3 ndr = centerOfNext - up - left;
                Vector3 ndl = centerOfNext - up + left;

                //4 lines to make a segment
                Gizmos.DrawLine(pul, nul);
                Gizmos.DrawLine(pur, nur);
                Gizmos.DrawLine(pdr, ndr);
                Gizmos.DrawLine(pdl, ndl);

                if (showColoured)
                {
                    Vector3[] points = new Vector3[8];
                    points[0] = pul;
                    points[1] = nul;
                    points[2] = nur;
                    points[3] = pur;
                    points[4] = pdr;
                    points[5] = ndr;
                    points[6] = ndl;
                    points[7] = pdl;

                    if (path.isReversed)
                    {
                        if (path.isOnRamp)
                        {
                            Handles.color = Color.magenta;
                        }
                        else
                        {
                            Handles.color = Color.red;
                        }
                    }
                    else
                    {
                        if (path.isOnRamp)
                        {
                            Handles.color = Color.cyan;
                        }
                        else
                        {
                            Handles.color = Color.green;
                        }
                    }

                    Handles.DrawAAConvexPolygon(points);
                }

                centerOfPrev = centerOfNext;
                pul = nul;
                pur = nur;
                pdr = ndr;
                pdl = ndl;
            }

            // Do last node position (to account for subdivision differences
            {
                centerOfNext = path.GetWorldPosition(numNodes);
                Vector3 up, left;
                GetOffsetFromPathSegment(path, numNodes, out up, out left);

                Vector3 nul = centerOfNext + up + left;
                Vector3 nur = centerOfNext + up - left;
                Vector3 ndr = centerOfNext - up - left;
                Vector3 ndl = centerOfNext - up + left;

                //4 lines to make a segment
                Gizmos.DrawLine(pul, nul);
                Gizmos.DrawLine(pur, nur);
                Gizmos.DrawLine(pdr, ndr);
                Gizmos.DrawLine(pdl, ndl);

                if (showColoured)
                {
                    Vector3[] points = new Vector3[8];
                    points[0] = pul;
                    points[1] = nul;
                    points[2] = nur;
                    points[3] = pur;
                    points[4] = pdr;
                    points[5] = ndr;
                    points[6] = ndl;
                    points[7] = pdl;

                    if (path.isReversed)
                    {
                        if (path.isOnRamp)
                        {
                            Handles.color = Color.magenta;
                        }
                        else
                        {
                            Handles.color = Color.red;
                        }
                    }
                    else
                    {
                        if (path.isOnRamp)
                        {
                            Handles.color = Color.cyan;
                        }
                        else
                        {
                            Handles.color = Color.green;
                        }
                    }

                    Handles.DrawAAConvexPolygon(points);
                }

                centerOfPrev = centerOfNext;
                pul = nul;
                pur = nur;
                pdr = ndr;
                pdl = ndl;
            }


            //4 lines to make a box cap
            Gizmos.DrawLine(pul, pur);
            Gizmos.DrawLine(pur, pdr);
            Gizmos.DrawLine(pdr, pdl);
            Gizmos.DrawLine(pdl, pul);
        }

        public static void GetOffsetFromPathSegment(Path path, float curvePos, out Vector3 up, out Vector3 left)
        {
            quaternion direction = CatmullRom.DirectionToRotationWorldUp(path.GetTangent(curvePos));

            up = math.mul(direction, new float3(0, 1, 0)) * (path.height / 2.0f);
            left = math.mul(direction, new float3(1, 0, 0)) * (path.width / 2.0f);
        }

#endif

        public void GetSplineSection(int index, out float3 p0, out float3 p1, out float3 p2, out float3 p3)
        {
            GetSplineSectionInternal(index, out p0, out p1, out p2, out p3);

            p0 += new float3(transform.position);
            p1 += new float3(transform.position);
            p2 += new float3(transform.position);
            p3 += new float3(transform.position);
        }

        private void GetSplineSectionInternal(int index, out float3 p0, out float3 p1, out float3 p2, out float3 p3)
        {
            if (isReversed)
            {
                index = (GetNumNodes() - 1) - index;

                p1 = Waypoints[index].position;
                p2 = Waypoints[index - 1].position;

                if (index == GetNumNodes() - 1)
                {
                    // compute p0
                    p0 = p1 + (p1 - p2);
                }
                else
                {
                    p0 = Waypoints[index + 1].position;
                }

                if (index == 1)
                {
                    // compute p3
                    p3 = p2 + (p2 - p1);
                }
                else
                {
                    p3 = Waypoints[index - 2].position;
                }
            }
            else
            {
                p1 = Waypoints[index].position;
                p2 = Waypoints[index + 1].position;

                if (index == 0)
                {
                    // compute p0
                    p0 = p1 + (p1 - p2);
                }
                else
                {
                    p0 = Waypoints[index - 1].position;
                }

                if (index == GetNumNodes() - 2)
                {
                    // compute p3
                    p3 = p2 + (p2 - p1);
                }
                else
                {
                    p3 = Waypoints[index + 2].position;
                }
            }
        }

        private float3 GetWorldPosition(int index, float fractionAlongPath)
        {
            float3 p0, p1, p2, p3;
            GetSplineSectionInternal(index, out p0, out p1, out p2, out p3);

            return CatmullRom.GetPosition(p0, p1, p2, p3, fractionAlongPath) + new float3(transform.position);
        }

        // Modified, now expects float to be 0-(numNodes-1) and slices accordingly, this is because once paths are moved to ecs, they are always 2 points and 0-1 and this makes the transition easier
        private int GetIndexFromFraction(ref float fractionAlongPath)
        {
            int numNodes = GetNumNodes() - 1;
            float findex = fractionAlongPath;
            int index = (int) (math.floor(findex));
            if (index > numNodes - 1)
            {
                findex = index - 0.00001f;
                index = numNodes - 1;
            }

            fractionAlongPath = math.frac(findex);

            return index;
        }

        public float3 GetWorldPosition(float fractionAlongPath)
        {
            if (GetNumNodes() < 2)
            {
                return new float3();
            }

            int index = GetIndexFromFraction(ref fractionAlongPath);

            return GetWorldPosition(index, fractionAlongPath);
        }

        public float3 GetRawPosition(int index)
        {
            return Waypoints[index].position;
        }

        public float3 GetReversibleRawPosition(int index)
        {
            if (isReversed)
            {
                return Waypoints[(GetNumNodes() - 1) - index].position;
            }

            return Waypoints[index].position;
        }

        public void SetRawPosition(int index, float3 pos)
        {
            Waypoints[index] = new RoadWaypoint {position = pos};
        }

        public float3 GetTangent(float fractionAlongPath)
        {
            if (GetNumNodes() < 2)
            {
                return float3.zero;
            }

            int index = GetIndexFromFraction(ref fractionAlongPath);

            return GetTangent(index, fractionAlongPath);
        }

        private float3 GetTangent(int index, float fractionAlongPath)
        {
            float3 p0, p1, p2, p3;
            GetSplineSectionInternal(index, out p0, out p1, out p2, out p3);

            return CatmullRom.GetTangent(p0, p1, p2, p3, fractionAlongPath);
        }

        public float ComputeArcLength(int index, int subdivs)
        {
            float3 p0, p1, p2, p3;
            GetSplineSectionInternal(index, out p0, out p1, out p2, out p3);

            return CatmullRom.ComputeArcLength(p0, p1, p2, p3, subdivs);
        }
    }
}