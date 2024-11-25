using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Unity.Services.MultiplayerSDK.Utils
{
    public class Dispatcher
    {
        readonly Stack<Func<Task>> m_Tasks = new Stack<Func<Task>>();
        readonly Stack<Action> m_Actions = new Stack<Action>();
        readonly Stack<Exception> m_Exceptions = new Stack<Exception>();

        public void Update()
        {
            while (m_Actions.Count > 0)
            {
                Execute(m_Actions.Pop());
            }

            while (m_Tasks.Count > 0)
            {
                Execute(m_Tasks.Pop());
            }

            while (m_Exceptions.Count > 0)
            {
                Execute(m_Exceptions.Pop());
            }
        }

        public void Dispatch(Action action)
        {
            m_Actions.Push(action);
        }

        public void Dispatch(Func<Task> func)
        {
            m_Tasks.Push(func);
        }

        public void Dispatch(Exception exception)
        {
            m_Exceptions.Push(exception);
        }

        void Execute(Action action)
        {
            try
            {
                action();
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
            }
        }

        async void Execute(Func<Task> func)
        {
            try
            {
                await func();
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
            }
        }

        void Execute(Exception exception)
        {
            throw exception;
        }
    }
}
