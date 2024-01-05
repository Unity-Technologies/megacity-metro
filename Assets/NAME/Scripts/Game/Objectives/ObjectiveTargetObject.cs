using System;
using UnityEngine;

namespace Unity.NAME.Game
{
    // This class represents an "objective target object" which is what the player needs to Gather/Interact with
    // in order to complete the objective.
    // If the objective is to "Collect all the Coins", that class is the representation of a single Coin

    public abstract class ObjectiveTargetObject : MonoBehaviour
    {
        [Header("Gameplay")]
        [Tooltip("Which game mode are you playing?")]
        public GameMode GameMode;

        [Tooltip("The amount of time the pickup gives in secs")]
        public float TimeGained;

        [Tooltip("Layers to trigger with")]
        public LayerMask LayerMask;

        [Tooltip("The point at which the collect VFX is spawned")]
        public Transform CollectVFXSpawnPoint;

        [Header("Sounds")]

        [Tooltip("Sound played when receiving damages")]
        public AudioClip CollectSound;

        public static event Action<ObjectiveTargetObject> OnRegisterObjectiveTargetObject;
        public static event Action<ObjectiveTargetObject> OnUnregisterObjectiveTargetObject;

        void Start()
        {
            OnRegisterObjectiveTargetObject?.Invoke(this);
        }

        void OnDestroy()
        {
            OnUnregisterObjectiveTargetObject?.Invoke(this);
        }
    }
}
