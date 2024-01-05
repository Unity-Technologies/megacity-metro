using System.Collections;
using System.Collections.Generic;
using Unity.NAME.Utilities;
using UnityEngine;

namespace Unity.NAME.Game
{
    // A pool object representation
    // It uses a Queue and a specific count for spawned entities
    // The first time a PoolObject is requested, the pool will be created

    [CreateAssetMenu(menuName = "Pool/Object")]
    public class PoolObject : ScriptableObject
    {
        [Tooltip("Spawn this many objects at the beginning of the game.")]
        public int SpawnCount = 5;
        [Tooltip("The prefab that this pool will spawn")]
        public GameObject ObjectToSpawn;

        private Queue<GameObject> m_Pool;
        private Transform m_Parent;

        public void InitializePool(Transform parent)
        {
            m_Parent = parent;
            m_Pool = new Queue<GameObject>();
            for (int i = 0; i < SpawnCount; i++)
            {
                GameObject spawnedObject = Instantiate(ObjectToSpawn);
                spawnedObject.name = ObjectToSpawn.name + "_" + m_Pool.Count;
                if (m_Parent)
                    spawnedObject.transform.SetParent(m_Parent, false);
                spawnedObject.SetActive(false);
                m_Pool.Enqueue(spawnedObject);
            }
        }

        public GameObject GetObject(bool active, Transform newParent)
        {
            if (!Application.isPlaying)
            {
                DebugUtils.Assert(false, $"PoolObject should not be used when the game is running.");
                return null;
            }

            if (m_Pool == null || m_Pool.Count == 0)
            {
                InitializePool(newParent);
            }

            GameObject pooledObj = m_Pool.Dequeue();
            pooledObj.SetActive(active);
            m_Pool.Enqueue(pooledObj);

            if (newParent)
                pooledObj.transform.SetParent(newParent);

            return pooledObj;
        }

        public void ReturnObject(GameObject obj)
        {
            if (m_Parent)
                obj.transform.SetParent(m_Parent);

            obj.SetActive(false);
        }

        public void ReturnAll()
        {
            for (int i = 0; i < m_Pool.Count; i++)
            {
                GameObject spawnedObj = m_Pool.Dequeue();
                ReturnObject(spawnedObj);
                m_Pool.Enqueue(spawnedObj);
            }
        }

        public void DestroyAll()
        {
            if (m_Pool == null)
                return;

            for (int i = 0; i < m_Pool.Count; i++)
            {
                GameObject spawnedObj = m_Pool.Dequeue();
                if (Application.isPlaying)
                    Destroy(spawnedObj);
                else
                    DestroyImmediate(spawnedObj);
            }

            m_Pool = null;
        }

        public IEnumerator ReturnWithDelay(GameObject obj, float delay)
        {
            yield return new WaitForSeconds(delay);
            ReturnObject(obj);
        }

        public IEnumerator DisableWithDelay(GameObject obj, float delay)
        {
            yield return new WaitForSeconds(delay);
            obj.SetActive(false);
        }
    }
}
