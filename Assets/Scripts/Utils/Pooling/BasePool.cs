using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MegacityMetro.Pooling
{
    public abstract class BasePool<T> 
    {
        public int InitialSize;
        public bool CanGrow;
        
        protected T[] PooledObjects;
        protected Queue<int> AvailableIndices;

        protected abstract T CreateNewInstance();
        protected abstract void CleanupInstance(T instance);
        protected abstract void ActivateInstance(T instance);
        protected abstract void DeactivateInstance(T instance);
        protected abstract bool IsInstanceValid(T instance);

        public void Init()
        {
            Cleanup();
         
            PooledObjects = new T[InitialSize];
            AvailableIndices = new Queue<int>();
            for (var i = 0; i < PooledObjects.Length; i++)
            {
                AvailableIndices.Enqueue(i);
                PooledObjects[i] = CreateNewInstance();
            }
        }

        public void Cleanup()
        {
            if (PooledObjects != null)
            {
                for (int i = 0; i < PooledObjects.Length; i++)
                {
                    CleanupInstance(PooledObjects[i]);
                }
            }
        }

        public void Grow(int addedCapacity)
        {
            int preGrowCapacity = PooledObjects.Length;
            Array.Resize(ref PooledObjects, PooledObjects.Length + addedCapacity);
            
            for (var i = preGrowCapacity; i < preGrowCapacity + addedCapacity; i++)
            {
                AvailableIndices.Enqueue(i);
                PooledObjects[i] = CreateNewInstance();
            }
        }

        public int Spawn(out T instance, bool allowActivation = true)
        {
            // Check if any available indexes exist. Grow if not
            if (!AvailableIndices.TryDequeue(out int availableIndex))
            {
                if (CanGrow)
                {
                    Grow(math.max(5, (int)math.round(PooledObjects.Length * 0.25f)));

                    // Get new availableIndex after growing
                    AvailableIndices.TryDequeue(out availableIndex);
                }
                else
                {
                    instance = default;
                    return -1;
                }
            }

            // Try to get the GameObject at the next available index
            if (IsIndexValid(availableIndex))
            {
                instance = PooledObjects[availableIndex];
                if (IsInstanceValid(instance))
                {
                    if (allowActivation)
                    {
                        ActivateInstance(instance);
                    }
                    return availableIndex;
                }
                else
                {
                    Debug.LogWarning("PoolManager tried spawning a null GameObject.");
                }
            }
            else
            {
                Debug.LogWarning("PoolManager dequeued invalid available index.");
            }

            instance = default;
            return -1;
        }

        public void Recycle(int index)
        {
            if (GetInstance(index, out T instance))
            {
                DeactivateInstance(instance);
                AvailableIndices.Enqueue(index);
            }
        }

        public bool GetInstance(int index, out T instance)
        {
            if (!IsIndexValid(index))
            {
                instance = default;
                return false;
            }

            instance = PooledObjects[index];
            return IsInstanceValid(instance);
        }

        private bool IsIndexValid(int index)
        {
            return index < PooledObjects.Length && index >= 0;
        }
    }
}