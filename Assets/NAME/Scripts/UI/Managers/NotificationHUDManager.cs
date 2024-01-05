using Unity.NAME.Game;
using UnityEngine;

namespace Unity.NAME.UI
{
    // This is the manager that handles the notifications in the bottom left corner of the screen
    // when the status of the objective changes (i.e. one remaining pickup for example)

    public class NotificationHUDManager : MonoBehaviour
    {
        [Tooltip("UI panel containing the layoutGroup for displaying notifications")]
        public UITable NotificationPanel;
        [Tooltip("Prefab for the notifications")]
        public PoolObject NotificationPrefab;

        public void RegisterObjective(Objective objective) => objective.OnUpdateObjective += OnUpdateObjective;
        public void UnregisterObjective(Objective objective) => objective.OnUpdateObjective -= OnUpdateObjective;

        void OnEnable()
        {
            Objective.OnObjectiveAdded += RegisterObjective;
            Objective.OnObjectiveRemoved += UnregisterObjective;
        }

        void OnUpdateObjective(UnityActionUpdateObjective updateObjective)
        {
            if (!string.IsNullOrEmpty(updateObjective.NotificationText))
                CreateNotification(updateObjective.NotificationText);
        }

        public void CreateNotification(string text)
        {
            if (NotificationPanel == null)
                return;

            GameObject notificationInstance = NotificationPrefab.GetObject(true, NotificationPanel.transform);
            notificationInstance.transform.SetSiblingIndex(0);

            NotificationToast toast = notificationInstance.GetComponent<NotificationToast>();
            toast.Initialize(text);
            NotificationPanel.UpdateTable(notificationInstance);
        }

        void OnDisable()
        {
            Objective.OnObjectiveAdded -= RegisterObjective;
            Objective.OnObjectiveRemoved -= UnregisterObjective;
        }
    }
}
