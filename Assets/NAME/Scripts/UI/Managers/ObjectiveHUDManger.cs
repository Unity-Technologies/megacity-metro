using Unity.NAME.Game;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.NAME.UI
{
    // This is the manager that displays the objectives in the Top Left corner of the screen

    public class ObjectiveHUDManger : ObjectiveManager
    {
        [Tooltip("UI panel containing the layoutGroup for displaying objectives")]
        public UITable ObjectivePanel;
        [Tooltip("Prefab for the primary objectives")]
        public PoolObject PrimaryObjectivePrefab;
        [Tooltip("Prefab for the primary objectives")]
        public PoolObject SecondaryObjectivePrefab;

        Dictionary<Objective, ObjectiveToast> m_ObjectivesDictionary;

        protected override void Awake()
        {
            base.Awake();

            m_ObjectivesDictionary = new Dictionary<Objective, ObjectiveToast>();

            Objective.OnObjectiveAdded += OnRegisterObjective;
            Objective.OnObjectiveRemoved += OnUnregisterObjective;
        }

        protected override void OnRegisterObjective(Objective objective)
        {
            base.OnRegisterObjective(objective);

            objective.OnUpdateObjective += OnUpdateObjective;

            // instanciate the UI element for the new objective
            GameObject objectiveUiInstance = objective.IsOptional ? SecondaryObjectivePrefab.GetObject(true, ObjectivePanel.transform) : PrimaryObjectivePrefab.GetObject(true, ObjectivePanel.transform);

            if (!objective.IsOptional)
                objectiveUiInstance.transform.SetSiblingIndex(0);

            ObjectiveToast toast = objectiveUiInstance.GetComponent<ObjectiveToast>();

            // initialize the element and give it the objective description
            toast.Initialize(objective.Title, objective.Description, objective.GetUpdatedCounterAmount(), objective.IsOptional, objective.DelayVisible);

            m_ObjectivesDictionary.Add(objective, toast);

            ObjectivePanel.UpdateTable(toast.gameObject);
        }

        protected override void OnUnregisterObjective(Objective objective)
        {
            base.OnUnregisterObjective(objective);

            objective.OnUpdateObjective -= OnUpdateObjective;

            // if the objective if in the list, make it fade out, and remove it from the list
            if (m_ObjectivesDictionary != null && m_ObjectivesDictionary.TryGetValue(objective, out ObjectiveToast toast))
            {
                toast.Complete();
                m_ObjectivesDictionary.Remove(objective);
            }
        }

        public void OnUpdateObjective(UnityActionUpdateObjective updateObjective)
        {
            if (m_ObjectivesDictionary != null && m_ObjectivesDictionary.TryGetValue(updateObjective.Objective, out ObjectiveToast toast))
            {
                // set the new updated description for the objective, and forces the content size fitter to be recalculated
                Canvas.ForceUpdateCanvases();
                if (!string.IsNullOrEmpty(updateObjective.DescriptionText))
                    toast.SetDescriptionText(updateObjective.DescriptionText);

                if (!string.IsNullOrEmpty(updateObjective.CounterText))
                    toast.CounterTextContent.text = updateObjective.CounterText;

                RectTransform toastRectTransform = toast.GetComponent<RectTransform>();
                if (toastRectTransform != null)
                    UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(toastRectTransform);
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            m_ObjectivesDictionary = null;
            Objective.OnObjectiveAdded -= OnRegisterObjective;
            Objective.OnObjectiveRemoved -= OnUnregisterObjective;
        }
    }
}
