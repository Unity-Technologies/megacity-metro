using System.Collections.Generic;
using UnityEngine;

namespace Unity.MegacityMetro.Gameplay
{
    /// <summary>
    /// Generates random positions around a transform
    /// </summary>
    public class RandomPositionGenerator : MonoBehaviour
    {
        [SerializeField, Range(10, 25)] 
        private int m_DistancePerPoint = 20;
        [SerializeField, Range(10, 64)] 
        private int m_MaxPoints = 14;
        [SerializeField, Range(10, 200)] 
        private float m_Radius = 30f;
        [SerializeField] 
        private bool m_ShowFill;
        
        private List<Vector3> m_Positions = new List<Vector3>();
        
        
        [ContextMenu("GeneratePoints")]
        private void GeneratePoints()
        {
            m_Positions.Clear();
            var children = GetComponentsInChildren<Transform>(true);
            foreach (var child in children)
            {
                if(child != transform)
                    DestroyImmediate(child.gameObject);
            }

            for (int i = 0; i < m_MaxPoints; i++)
            {
                var position = Random.insideUnitSphere * m_Radius;
                position += transform.position;
                // Check distance between points
                bool isValid = true;
                foreach (var existingPosition in m_Positions)
                {
                    if (Vector3.Distance(position, existingPosition) < m_DistancePerPoint)
                    {
                        isValid = false;
                        break;
                    }
                }

                // If the point is valid, add it to the list with a random Y rotation
                if (isValid)
                {
                    m_Positions.Add(position);
                }
            }

            for (int i = 0; i < m_Positions.Count; i++)
            {
                var position = m_Positions[i]; 
                
                var newPoint = new GameObject($"Point {i + 1}");
                newPoint.AddComponent<SpawnPoint>();
                newPoint.transform.SetParent(transform);
                newPoint.transform.position = position;
                newPoint.transform.rotation = Quaternion.Euler(0, Random.Range(0f, 360f), 0);

            }
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.cyan;
            if(m_ShowFill)
                Gizmos.DrawSphere(transform.position, m_Radius);
            else
                Gizmos.DrawWireSphere(transform.position, m_Radius);
        }
    }
}
