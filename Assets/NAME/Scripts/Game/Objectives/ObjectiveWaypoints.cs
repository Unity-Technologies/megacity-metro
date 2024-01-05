using System.Collections;
using UnityEngine;

namespace Unity.NAME.Game
{
    // An objective example

    public class ObjectiveWaypoints : Objective
    {
        [Tooltip("Choose whether you need to go through all waypoints or only a minimum amount")]
        public bool MustGoThroughAllWaypoints = true;

        [Tooltip("If MustGoThroughAllWaypoints is false, this is the amount of waypoints required")]
        public int WaypointsToCompleteObjective = 5;

        [Header("Notification")]
        [Tooltip("Start sending notification about remaining pickups when this amount of pickups is left")]
        public int NotificationWaypointsRemainingThreshold = 1;

        public override string GetUpdatedCounterAmount() => PickupTotal + " / " + WaypointsToCompleteObjective;

        protected override void Start()
        {
            base.Start();

            StartCoroutine(DelayedRegistration());
        }

        IEnumerator DelayedRegistration()
        {
            yield return new WaitForEndOfFrame();

            Title = "Go Through " +
                (MustGoThroughAllWaypoints ? "all the" : WaypointsToCompleteObjective.ToString()) + " " +
                TargetName + "s";

            if (MustGoThroughAllWaypoints)
                WaypointsToCompleteObjective = NumberOfPickupsTotal;

            Register();
        }

        protected override void ReachCheckpoint(int remaining)
        {
            if (IsCompleted)
                return;

            if (MustGoThroughAllWaypoints)
                WaypointsToCompleteObjective = NumberOfPickupsTotal;

            PickupTotal = NumberOfPickupsTotal - remaining;
            int targetRemaining = MustGoThroughAllWaypoints ? remaining : WaypointsToCompleteObjective - PickupTotal;

            // update the objective text according to how many enemies remain to kill
            if (targetRemaining == 0)
            {
                CompleteObjective(string.Empty, GetUpdatedCounterAmount(),
                    "Objective complete: " + Title);
            }
            else if (targetRemaining == 1)
            {
                string notificationText = NotificationWaypointsRemainingThreshold >= targetRemaining
                    ? "One " + TargetName + " left"
                    : string.Empty;
                UpdateObjective(string.Empty, GetUpdatedCounterAmount(), notificationText);
            }
            else if (targetRemaining > 1)
            {
                // create a notification text if needed, if it stays empty, the notification will not be created
                string notificationText = NotificationWaypointsRemainingThreshold >= targetRemaining
                    ? targetRemaining + " " + TargetName + "s to collect left"
                    : string.Empty;

                UpdateObjective(string.Empty, GetUpdatedCounterAmount(), notificationText);
            }
        }
    }
}
