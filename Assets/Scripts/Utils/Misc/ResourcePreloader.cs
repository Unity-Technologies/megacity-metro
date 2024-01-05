using System.Collections.Generic;
using UnityEngine;
using UnityObject = UnityEngine.Object;

namespace Unity.MegacityMetro.Utils
{
    /// <summary>
    /// Creates a collection of UnityObject such as Textures in this case.
    /// </summary>
    public class ResourcePreloader : MonoBehaviour
    {
        [SerializeField] List<UnityObject> m_Resources = new List<UnityObject>();

        public void SetResources(IReadOnlyCollection<UnityObject> resources)
        {
            m_Resources.Clear();
            m_Resources.AddRange(resources);
        }
    }
}
