using System.Collections;
using UnityEngine;

namespace Unity.NAME.Game
{
    // An objective example

    public class ObjectiveCollectPickups : Objective
    {
        [Tooltip("Choose whether you need to collect all pickups or only a minimum amount")]
        public bool MustCollectAllPickups = true;

        [Tooltip("If MustCollectAllPickups is false, this is the amount of pickups required")]
        public int PickupsToCompleteObjective = 5;

        [Header("Notification")]
        [Tooltip("Start sending notification about remaining pickups when this amount of pickups is left")]
        public int NotificationPickupsRemainingThreshold = 1;

        public override string GetUpdatedCounterAmount() => PickupTotal + " / " + PickupsToCompleteObjective;

        protected override void Start()
        {
            base.Start();

            StartCoroutine(DelayedRegistration());
        }

        IEnumerator DelayedRegistration()
        {
            yield return new WaitForEndOfFrame();

            Title = "Collect " +
                (MustCollectAllPickups ? "all the" : PickupsToCompleteObjective.ToString()) + " " +
                TargetName + "s";

            if (MustCollectAllPickups)
                PickupsToCompleteObjective = NumberOfPickupsTotal;

            Register();
        }

        protected override void ReachCheckpoint(int remaining)
        {
            if (IsCompleted)
                return;

            if (MustCollectAllPickups)
                PickupsToCompleteObjective = NumberOfPickupsTotal;

            PickupTotal = NumberOfPickupsTotal - remaining;
            int targetRemaining = MustCollectAllPickups ? remaining : PickupsToCompleteObjective - PickupTotal;

            // update the objective text according to how many enemies remain to kill
            if (targetRemaining == 0)
            {
                CompleteObjective(string.Empty, GetUpdatedCounterAmount(),
                    "Objective complete: " + Title);
            }
            else if (targetRemaining == 1)
            {
                string notificationText = NotificationPickupsRemainingThreshold >= targetRemaining
                    ? "One " + TargetName + " left"
                    : string.Empty;
                UpdateObjective(string.Empty, GetUpdatedCounterAmount(), notificationText);
            }
            else if (targetRemaining > 1)
            {
                // create a notification text if needed, if it stays empty, the notification will not be created
                string notificationText = NotificationPickupsRemainingThreshold >= targetRemaining
                    ? targetRemaining + " " + TargetName + "s to collect left"
                    : string.Empty;

                UpdateObjective(string.Empty, GetUpdatedCounterAmount(), notificationText);
            }
        }
    }
}
