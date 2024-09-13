using UnityEngine;

namespace Unity.MegacityMetro.Common
{
    public class PersistentObject : MonoBehaviour
    {
        private static PersistentObject Instance { get; set; }
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            } 
            else
            {
                Destroy(gameObject);
            }
        }
    }
}