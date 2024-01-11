using System;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.VFX;

namespace MegacityMetro.Pooling
{
    public class GameObjectPoolElement
    {
        public GameObject GameObject;
        public Transform Transform;
    }
    
    [Serializable]
    public class GameObjectPool : BasePool<GameObjectPoolElement>
    {
        public PoolType Type;
        public GameObject Prefab;

        protected override GameObjectPoolElement CreateNewInstance()
        {
            GameObject goInstance = GameObject.Instantiate(Prefab);
            GameObject.DontDestroyOnLoad(goInstance);  

            GameObjectPoolElement elementInstance = new GameObjectPoolElement
            {
                GameObject = goInstance,
                Transform = goInstance.transform,
            };
            DeactivateInstance(elementInstance);
            return elementInstance;
        }

        protected override void CleanupInstance(GameObjectPoolElement instance)
        {
            GameObject.Destroy(instance.GameObject);
        }

        protected override void ActivateInstance(GameObjectPoolElement instance)
        {
            instance.GameObject.SetActive(true);
        }

        protected override void DeactivateInstance(GameObjectPoolElement instance)
        {
            if(instance.GameObject != null)
            {
                instance.GameObject.SetActive(false);
            }
        }

        protected override bool IsInstanceValid(GameObjectPoolElement instance)
        {
            return instance != null;
        }

        public int Spawn(float3 position, out GameObjectPoolElement instance)
        {
            int index = base.Spawn(out instance);
            if (instance != null)
            {
                instance.Transform.position = position;
            }
            return index;
        }

        public int Spawn(float3 position, quaternion rotation, out GameObjectPoolElement instance)
        {
            int index = base.Spawn(out instance, false);
            if (instance != null)
            {
                instance.Transform.position = position;
                instance.Transform.rotation = rotation;
                ActivateInstance(instance);
            }
            return index;
        }

        public static bool GetPooledElement(Entity entity, DynamicInstanceLinkCleanup linkCleanup, out GameObjectPoolElement element)
        {
            if (PoolManager.Instance.GetInstanceGameObject(linkCleanup.InstanceIndex, out element))
            {
                return true;
            }

            element = default;
            return false;
        }

        public static bool GetPooledElement(Entity entity, ref ComponentLookup<DynamicInstanceLinkCleanup> linkCleanupLookup, out GameObjectPoolElement element)
        {
            if (linkCleanupLookup.TryGetComponent(entity, out DynamicInstanceLinkCleanup muzzleLink) &&
                PoolManager.Instance.GetInstanceGameObject(muzzleLink.InstanceIndex, out element))
            {
                return true;
            }

            element = default;
            return false;
        }
    }
}