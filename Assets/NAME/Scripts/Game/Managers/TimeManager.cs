using UnityEngine;

namespace Unity.NAME.Game
{
    // A simple manager to handle the objectives that are time based

    public class TimeManager : MonoBehaviour, IGameService
    {
        public float TotalTime { get; protected set; }
        public float TimeRemaining { get; protected set; }
        public bool IsOver { get; protected set; } = false;

        public bool TimeSet { get; protected set; } = false;

        public virtual void StartTimer() { TimerHasStarted = true; }
        public virtual void StopTimer() { TimerHasStarted = false; }

        public bool TimerHasStarted { get; protected set; } = false;

        public void InitializeService() {}

        public void ShutdownService() {}

        protected virtual void Awake()
        {
            ServiceLocator.RegisterService(this);
        }

        protected virtual void Update()
        {
            if (TimeSet && !IsOver && TimerHasStarted)
            {
                TimeRemaining = Mathf.Max(TimeRemaining - Time.deltaTime, 0f);
                IsOver = TimeRemaining < Mathf.Epsilon;
            }
        }

        protected virtual void OnDestroy()
        {
            ServiceLocator.UnregisterService(this);
        }
    }
}
