﻿using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using UnityEngine;

namespace Gamnet
{
    // Gamnet의 이벤트와 로직은 메인스레드에서 모두 처리한다.
    // NetworkIO 등 비동기 영역에서 발생하는 이벤트들은 SessionEvent 들을 이용해 메인스테드로 큐잉한다.
    public partial class Session
    {
        public abstract class SessionEvent
        {
            protected Session session;
            #if UNITY_EDITOR || USE_DEBUGGING
            public readonly string CallStack;
            #endif

            public SessionEvent(Session session)
            {
                this.session = session;
                #if UNITY_EDITOR || USE_DEBUGGING
                CallStack = StackTraceUtility.ExtractStackTrace();
                #endif
            }

            public abstract void OnEvent();
        };

        public class ActionEvent : SessionEvent
        {
            private Action action;
            public ActionEvent(Session session, Action action) : base(session)
            {
                this.action = action;
            }

            public override void OnEvent()
            {
                action();
            }
        }

        protected virtual void OnConnect()
        {
            throw new System.NotImplementedException("Session.OnConnect is not implemented");
        }

        protected virtual void OnResume()
        {
            throw new System.NotImplementedException("Session.OnResume is not implemented");
        }

        protected virtual void OnPause()
        {
            throw new System.NotImplementedException("Session.OnPause is not implemented");
        }

        protected virtual void OnClose()
        {
            throw new System.NotImplementedException("Session.OnClose is not implemented");
        }

        public class ErrorEvent : SessionEvent
        {
            public ErrorEvent(Session session, System.Exception exception) : base(session)
            {
                this.exception = exception;
            }
            public System.Exception exception;
            public override void OnEvent()
            {
#if UNITY_EDITOR || USE_DEBUGGING
                Log.Write(Log.LogLevel.ERR, exception.StackTrace.ToString());
#endif
                session.OnError(exception);
                session.Close();
            }
        }

        protected virtual void OnError(System.Exception e)
        {
            throw new System.NotImplementedException("Session.OnError is not implemented");
        }

        protected virtual void OnReceive(Packet packet)
        {
            throw new System.NotImplementedException("Session.OnReceive is not implemented");
        }

        public class EventLoop
        {
            private ConcurrentQueue<SessionEvent> eventQueue = new ConcurrentQueue<SessionEvent>();
            private static EventLoop instance = new EventLoop();

            public static void Update()
            {
                SessionEvent evt;
                while (true == instance.eventQueue.TryDequeue(out evt))
                {
                    //try
                    //{
                        evt.OnEvent();
                    //}
                    //catch (System.Exception e)
                    //{
                    //    Debug.LogError($"{e.GetType().Name}(Event:{evt.GetType().Name}, Message:{e.Message})");
//#if UNITY_EDITOR || USE_DEBUGGING
                    //    Debug.Log($"[Async Debug Info] Event Caller Location :\n{evt.CallStack}");
//#endif              //
                    //}
                }
            }

            public static void EnqueuEvent(SessionEvent evt)
            {
                instance.eventQueue.Enqueue(evt);
            }
        }
    }
}
