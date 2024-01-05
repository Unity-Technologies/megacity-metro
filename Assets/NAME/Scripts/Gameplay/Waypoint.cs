using Unity.NAME.Game;
using UnityEngine;

namespace Unity.NAME.Gameplay
{
    // This class inherits from ObjectiveTargetObject and represents a PickupObject.
    public class Waypoint : ObjectiveTargetObject
    {
        [Header("PickupObject")]

        [Tooltip("New Gameobject (a VFX for example) to spawn when you trigger this PickupObject")]
        public GameObject SpawnPrefabOnPickup;

        [Tooltip("Destroy the spawned spawnPrefabOnPickup gameobject after this delay time. Time is in seconds.")]
        public float DestroySpawnPrefabDelay = 10;

        [Tooltip("Destroy this gameobject after collectDuration seconds")]
        public float CollectDuration = 0f;

        void OnCollect()
        {
            if (CollectSound)
            {
                AudioUtility.CreateSFX(CollectSound, transform.position, AudioUtility.AudioGroups.Pickup, 0f);
            }

            if (SpawnPrefabOnPickup)
            {
                var vfx = Instantiate(SpawnPrefabOnPickup, CollectVFXSpawnPoint.position, Quaternion.identity);
                Destroy(vfx, DestroySpawnPrefabDelay);
            }

            //TimeManager.OnAdjustTime(TimeGained);
            Destroy(gameObject, CollectDuration);
        }

        void OnTriggerEnter(Collider other)
        {
            if ((LayerMask.value & 1 << other.gameObject.layer) > 0 && other.gameObject.CompareTag("Player"))
            {
                OnCollect();
            }
        }
    }
}
