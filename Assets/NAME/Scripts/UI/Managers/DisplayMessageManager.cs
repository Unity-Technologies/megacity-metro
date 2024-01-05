using System;
using System.Collections.Generic;
using Unity.NAME.Game;
using UnityEngine;

namespace Unity.NAME.UI
{
    // This is the manager that handles the messages in the center of the screen when an Objective begins
    // It is also responsible for the GameState messages (Win or Lose)

    public class DisplayMessageManager : MonoBehaviour
    {
        public UITable DisplayMessageRect;
        public PoolObject MessagePrefab;

        // Request timestamp / Delay before display / Message / Notification
        List<Tuple<float, float, string, NotificationToast>> m_PendingMessages;

        void Awake()
        {
            EventManager.AddListener<DisplayMessageEvent>(OnDisplayMessageEvent);
            m_PendingMessages = new List<Tuple<float, float, string, NotificationToast>>();
        }

        void OnDisplayMessageEvent(DisplayMessageEvent evt)
        {
            NotificationToast notification = MessagePrefab.GetObject(true, DisplayMessageRect.transform).GetComponent<NotificationToast>();
            m_PendingMessages.Add(new Tuple<float, float, string, NotificationToast>(Time.time, evt.DelayBeforeDisplay, evt.MessageText, notification));
        }

        void Update()
        {
            foreach (var message in m_PendingMessages)
            {
                if (Time.time - message.Item1 > message.Item2)
                {
                    message.Item4.Initialize(message.Item3);
                    DisplayMessage(message.Item4);
                }
            }

            // Clear deprecated messages
            m_PendingMessages.RemoveAll(x => x.Item4.Initialized);
        }

        void DisplayMessage(NotificationToast notification)
        {
            DisplayMessageRect.UpdateTable(notification.gameObject);
            StartCoroutine(MessagePrefab.ReturnWithDelay(notification.gameObject, notification.TotalRunTime));
        }

        void OnDestroy()
        {
            EventManager.RemoveListener<DisplayMessageEvent>(OnDisplayMessageEvent);
        }
    }
}
