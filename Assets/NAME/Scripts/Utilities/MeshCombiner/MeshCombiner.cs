using System.Collections.Generic;
using UnityEngine;

namespace Unity.NAME.Utilities
{
    // A component that allows to combine 3D meshes at runtime

    public class MeshCombiner : MonoBehaviour
    {
        public List<GameObject> CombineParents = new List<GameObject>();

        [Header("Grid parameters")]
        public bool UseGrid = false;
        public Vector3 GridCenter;
        public Vector3 GridExtents = new Vector3(10, 10, 10);
        public Vector3Int GridResolution = new Vector3Int(2, 2, 2);
        public Color GridPreviewColor = Color.green;

        private void Start()
        {
            Combine();
        }

        public void Combine()
        {
            List<MeshRenderer> validRenderers = new List<MeshRenderer>();
            foreach (GameObject combineParent in CombineParents)
            {
                validRenderers.AddRange(combineParent.GetComponentsInChildren<MeshRenderer>());
            }

            if (UseGrid)
            {
                for (int i = 0; i < GetGridCellCount(); i++)
                {
                    if (GetGridCellBounds(i, out Bounds bounds))
                    {
                        CombineAllInBounds(bounds, validRenderers);
                    }
                }
            }
            else
            {
                MeshCombineUtility.Combine(validRenderers, MeshCombineUtility.RendererDisposeMethod.DestroyRendererAndFilter, "Level_Combined");
            }
        }

        void CombineAllInBounds(Bounds bounds, List<MeshRenderer> validRenderers)
        {
            List<MeshRenderer> renderersForThisCell = new List<MeshRenderer>();

            for (int i = validRenderers.Count - 1; i >= 0; i--)
            {
                MeshRenderer m = validRenderers[i];
                if (bounds.Intersects(m.bounds))
                {
                    renderersForThisCell.Add(m);
                    validRenderers.Remove(m);
                }
            }

            if (renderersForThisCell.Count > 0)
            {
                MeshCombineUtility.Combine(renderersForThisCell, MeshCombineUtility.RendererDisposeMethod.DestroyRendererAndFilter, "Level_Combined");
            }
        }

        int GetGridCellCount()
        {
            return GridResolution.x * GridResolution.y * GridResolution.z;
        }

        public bool GetGridCellBounds(int index, out Bounds bounds)
        {
            bounds = default;
            if (index < 0 || index >= GetGridCellCount())
                return false;

            int xCoord = index / (GridResolution.y * GridResolution.z);
            int yCoord = (index / GridResolution.z) % GridResolution.y;
            int zCoord = index % GridResolution.z;

            Vector3 gridBottomCorner = GridCenter - (GridExtents * 0.5f);
            Vector3 cellSize = new Vector3(GridExtents.x / (float)GridResolution.x, GridExtents.y / (float)GridResolution.y, GridExtents.z / (float)GridResolution.z);
            Vector3 cellCenter = gridBottomCorner + (new Vector3((xCoord * cellSize.x) + (cellSize.x * 0.5f), (yCoord * cellSize.y) + (cellSize.y * 0.5f), (zCoord * cellSize.z) + (cellSize.z * 0.5f)));

            bounds.center = cellCenter;
            bounds.size = cellSize;

            return true;
        }

        private void OnDrawGizmosSelected()
        {
            if (UseGrid)
            {
                Gizmos.color = GridPreviewColor;

                for (int i = 0; i < GetGridCellCount(); i++)
                {
                    if (GetGridCellBounds(i, out Bounds bounds))
                    {
                        Gizmos.DrawWireCube(bounds.center, bounds.size);
                    }
                }
            }
        }
    }
}
