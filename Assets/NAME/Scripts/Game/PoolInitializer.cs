using UnityEngine;

namespace Unity.NAME.Game
{
    // The class that is responsible for instantiating the pools
    // If that component is not present in the scene, the pool creation will be done when an object is requested

    public class PoolInitializer : MonoBehaviour
    {
        public PoolObject[] Pools = new PoolObject[0];
        public Transform Parent;

        public void OnEnable()
        {
            Initialize();
        }

        public void OnDisable()
        {
            DestroyAll();
        }

        public void Initialize()
        {
            if (Parent == null)
                Parent = transform;

            DestroyAll();

            for (int i = 0; i < Pools.Length; i++)
                Pools[i].InitializePool(Parent);
        }

        public void DestroyAll()
        {
            for (int i = 0; i < Pools.Length; i++)
                Pools[i].DestroyAll();
        }
    }
}
