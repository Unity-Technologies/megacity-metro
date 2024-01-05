using Unity.NAME.Game;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.Playables;

namespace Unity.NAME.UI
{
    // This is the manager that handles the Start Countdown and the time during gameplay

    public class TimerHUDManager : TimeManager
    {
        [Tooltip("The Countdown PlayableDirector used when the Objectives use a Timer")]
        public PlayableDirector StartCountdown;

        [Tooltip("The Timer Text")] public TextMeshProUGUI TimerText;

        protected override void Awake()
        {
            base.Awake();

            EventManager.AddListener<SetTimeEvent>(OnSetTimeEvent);
            EventManager.AddListener<AdjustTimeEvent>(OnAdjustTimeEvent);
            TimerText.text = "";
        }

        public void OnCountdownFinished() => TimerHasStarted = true;

        public override void StartTimer() => StartCountdown.gameObject.SetActive(TimeSet);

        public override void StopTimer() => StartCountdown.gameObject.SetActive(false);

        void OnAdjustTimeEvent(AdjustTimeEvent evt)
        {
            TimeRemaining += evt.DeltaTime;
        }

        void OnSetTimeEvent(SetTimeEvent evt)
        {
            TimeSet = true;
            IsOver = false;
            TotalTime = evt.Time;
            TimeRemaining = TotalTime;
        }

        protected override void Update()
        {
            base.Update();

            if (TimeSet)
            {
                TimerText.gameObject.SetActive(true);
                int timeRemaining = (int)Math.Ceiling(TimeRemaining);
                TimerText.text = string.Format("{0}:{1:00}", timeRemaining / 60, timeRemaining % 60);
            }
            else
            {
                TimerText.gameObject.SetActive(false);
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            EventManager.RemoveListener<SetTimeEvent>(OnSetTimeEvent);
            EventManager.RemoveListener<AdjustTimeEvent>(OnAdjustTimeEvent);
        }
    }
}
