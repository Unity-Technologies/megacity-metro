using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Unity.NAME.Game
{
    // This is the abstract class for an objective
    // In order to create a new objective type, a new class needs to be created, inheriting from this one
    // There are 2 examples provided:
    // - ObjectiveWaypoints
    // - ObjectiveCollectPickups

    public abstract class Objective : MonoBehaviour
    {
        [Tooltip("Which game mode are you playing?")]
        public GameMode GameMode;
        [Tooltip("Name of the target object the player will collect/crash/complete for this objective")]
        public string TargetName;
        [Tooltip("Short text explaining the objective that will be shown on screen")]
        public string Title;
        [Tooltip("Short text explaining the objective that will be shown on screen")]
        public string Description;
        [Tooltip("Whether the objective is required to win or not")]
        public bool IsOptional;
        [Tooltip("Delay before the objective becomes visible")]
        public float DelayVisible;
        [Tooltip("General message to describe the objective")]
        public string ObjectiveMessage;

        [Header("Requirements")]
        [Min(0), Tooltip("If there is a time limit, how long in secs? (0 = No Time Limit")]
        public int TotalTimeInSecs = 0;

        public bool IsCompleted { get; protected set; }
        public bool IsBlocking() => !(IsOptional || IsCompleted);

        public UnityAction<UnityActionUpdateObjective> OnUpdateObjective;

        public static event Action<Objective> OnObjectiveAdded;
        public static event Action<Objective> OnObjectiveRemoved;
        protected int PickupTotal;

        public List<ObjectiveTargetObject> Pickups { get; } = new List<ObjectiveTargetObject>();
        public int NumberOfPickupsTotal { get; private set; }
        public int NumberOfPickupsRemaining => Pickups.Count;

        public int NumberOfActivePickupsRemaining() => Pickups.FindAll(x => x.enabled).Count;
        protected abstract void ReachCheckpoint(int remaining);
        public abstract string GetUpdatedCounterAmount();

        protected void Register() => OnObjectiveAdded?.Invoke(this);
        protected void Unregister() => OnObjectiveRemoved?.Invoke(this);

        void OnEnable()
        {
            ObjectiveTargetObject.OnRegisterObjectiveTargetObject += RegisterPickup;
            ObjectiveTargetObject.OnUnregisterObjectiveTargetObject += UnregisterPickup;
        }

        protected virtual void Start()
        {
            if (TotalTimeInSecs > 0.0f)
            {
                SetTimeEvent evt = Events.SetTimeEvent;
                evt.Time = TotalTimeInSecs;
                EventManager.Broadcast(evt);
            }
        }

        public void UpdateObjective(string descriptionText, string counterText, string notificationText)
        {
            OnUpdateObjective?.Invoke(new UnityActionUpdateObjective(this, descriptionText, counterText, false,
                notificationText));
        }

        public void CompleteObjective(string descriptionText, string counterText, string notificationText)
        {
            IsCompleted = true;
            UpdateObjective(descriptionText, counterText, notificationText);
        }

        public void RegisterPickup(ObjectiveTargetObject pickup)
        {
            if (pickup.GameMode != GameMode)
                return;

            Pickups.Add(pickup);
            NumberOfPickupsTotal++;
        }

        public void UnregisterPickup(ObjectiveTargetObject pickupCollected)
        {
            if (pickupCollected.GameMode != GameMode)
                return;

            // removes the pickup from the list, so that we can keep track of how many are left on the map
            ReachCheckpoint(NumberOfPickupsRemaining - 1);
            Pickups.Remove(pickupCollected);
        }

        public void ResetPickups()
        {
            for (int i = 0; i < Pickups.Count; i++)
            {
                Pickups[i].enabled = true;
            }
        }

        void OnDisable()
        {
            ObjectiveTargetObject.OnRegisterObjectiveTargetObject -= RegisterPickup;
            ObjectiveTargetObject.OnUnregisterObjectiveTargetObject -= UnregisterPickup;
        }

        void OnDestroy()
        {
            Unregister();
        }
    }

    public class UnityActionUpdateObjective
    {
        public Objective Objective;
        public string DescriptionText;
        public string CounterText;
        public bool IsComplete;
        public string NotificationText;

        public UnityActionUpdateObjective(Objective objective, string descriptionText, string counterText, bool isComplete, string notificationText)
        {
            this.Objective = objective;
            this.DescriptionText = descriptionText;
            this.CounterText = counterText;
            this.IsComplete = isComplete;
            this.NotificationText = notificationText;
        }
    }
}
