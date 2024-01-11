using System;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MegacityMetro.Pooling
{
    [Serializable]
    public enum PoolType
    {
        VehicleFX_Xencor = 0,
        VehicleFX_Bion,
        VehicleFX_BigSun,
    }

    public class PoolManager : MonoBehaviour
    {
        [Serializable]
        public class PooledGameObjectInstance
        {
            public PoolType PoolType;
            public int IndexInPool;
            public GameObjectPoolElement Instance;
        }

        [Serializable]
        public class PooledGameObjectInstancePool : BasePool<PooledGameObjectInstance>
        {
            protected override PooledGameObjectInstance CreateNewInstance()
            {
                return new PooledGameObjectInstance();
            }

            protected override void CleanupInstance(PooledGameObjectInstance instance)
            {
            }

            protected override void ActivateInstance(PooledGameObjectInstance instance)
            {
            }

            protected override void DeactivateInstance(PooledGameObjectInstance instance)
            {
                instance = default;
            }

            protected override bool IsInstanceValid(PooledGameObjectInstance instance)
            {
                return true;
            }
        }

        private static PoolManager _instance;
        public static PoolManager Instance
        {
            get
            {
                // Handle creation
                if (_instance == null)
                {
                    _instance = (Resources.Load("PoolManager") as GameObject).GetComponent<PoolManager>();
                    
                    // Init pools
                    foreach (var pool in _instance._pools)
                    {
                        pool.Init();
                    }
                    _instance._pooledGameObjectInstancePool.Init();
                }
                return _instance;
            }
        }

        [Header("Pools")] 
        [SerializeField] 
        private GameObjectPool[] _pools;

        [SerializeField] 
        private PooledGameObjectInstancePool _pooledGameObjectInstancePool = new PooledGameObjectInstancePool();
        

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        public static void ResetStaticData()
        {
            _instance = null;
        }
        
        private void OnDisable()
        {
            foreach (var pool in _pools)
            {
                pool.Cleanup();
            }
            _pooledGameObjectInstancePool.Cleanup();
        }

        public int SpawnFromPool(PoolType type)
        {
            return SpawnFromPool(type, float3.zero, quaternion.identity);
        }

        public int SpawnFromPool(PoolType type, float3 position)
        {
            return SpawnFromPool(type, position, quaternion.identity);
        }

        public int SpawnFromPool(PoolType type, float3 position, quaternion rotation)
        {
            if (GetPool(type, out GameObjectPool pool))
            {
                int indexInPool = pool.Spawn(position, rotation, out GameObjectPoolElement instance);
                
                int indexInManager = _pooledGameObjectInstancePool.Spawn(out PooledGameObjectInstance managerInstance);
                if (managerInstance != null)
                {
                    managerInstance.PoolType = type;
                    managerInstance.IndexInPool = indexInPool;
                    managerInstance.Instance = instance;
                }

                return indexInManager;
            }

            return -1;
        }

        public void RecycleFromPool(int index)
        {
            if (_pooledGameObjectInstancePool.GetInstance(index, out PooledGameObjectInstance managerInstance))
            {
                if (GetPool(managerInstance.PoolType, out GameObjectPool pool))
                {
                    pool.Recycle(managerInstance.IndexInPool);
                }
            }
            
            _pooledGameObjectInstancePool.Recycle(index);
        }

        public bool GetPool(PoolType type, out GameObjectPool pool)
        {
            for (int i = 0; i < _pools.Length; i++)
            {
                GameObjectPool tmpPool = _pools[i];
                if (tmpPool != null && tmpPool.Type == type)
                {
                    pool = tmpPool;
                    return true;
                }
            }

            pool = null;
            return false;
        }

        public bool GetInstanceGameObject(int index, out GameObjectPoolElement instance)
        {
            if (_pooledGameObjectInstancePool.GetInstance(index, out PooledGameObjectInstance managerInstance) && managerInstance.Instance != null)
            {
                instance = managerInstance.Instance;
                return true;
            }
            
            instance = null;
            return false;
        }

        public void SetInstancePosition(int index, float3 position)
        {
            _pooledGameObjectInstancePool.GetInstance(index, out PooledGameObjectInstance tmp);
            if (_pooledGameObjectInstancePool.GetInstance(index, out PooledGameObjectInstance managerInstance) && managerInstance.Instance != null)
            {
                managerInstance.Instance.Transform.position = position;
            }
        }

        public void SetInstanceRotation(int index, quaternion rotation)
        {
            if (_pooledGameObjectInstancePool.GetInstance(index, out PooledGameObjectInstance managerInstance) && managerInstance.Instance != null)
            {
                managerInstance.Instance.Transform.rotation = rotation;
            }
        }
    }
}