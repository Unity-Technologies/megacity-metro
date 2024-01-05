using System.Collections.Generic;
using UnityEngine;

namespace Unity.NAME.Game
{
    // This is the manager that handles the Objectives
    // It doesn't do much besides keeping a list of all the active objectives

    public class ObjectiveManager : MonoBehaviour, IGameService
    {
        public List<Objective> Objectives { get; private set; } = new List<Objective>();

        protected virtual void OnRegisterObjective(Objective objective)
        {
            Objectives.Add(objective);
        }

        protected virtual void OnUnregisterObjective(Objective objective)
        {
            if (Objectives.Contains(objective))
                Objectives.Remove(objective);
        }

        protected virtual void Awake()
        {
            ServiceLocator.RegisterService(this);
        }

        public void InitializeService() {}
        public void ShutdownService() {}

        public bool AreAllObjectivesCompleted()
        {
            if (Objectives.Count == 0)
                return false;

            foreach (var objective in Objectives)
            {
                // pass every objectives to check if they have been completed
                if (objective.IsBlocking())
                {
                    // break the loop as soon as we find one uncompleted objective
                    return false;
                }
            }

            // found no uncompleted objective
            return true;
        }

        protected virtual void OnDestroy()
        {
            ServiceLocator.UnregisterService(this);
        }
    }
}
