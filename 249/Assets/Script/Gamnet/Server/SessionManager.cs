using System.Collections.Generic;

namespace Gamnet.Server
{
    public partial class Session : Gamnet.Session
    {
        private static class SessionManager
        {
            public static int keepalive_time = 5;
            private static Dictionary<uint, Session> sessions = new Dictionary<uint, Session>();

            public static void Add(Session session)
            {
                sessions.Add(session.session_key, session);
            }

            public static void Remove(Session session)
            {
                sessions.Remove(session.session_key);
            }

            public static Session Find(uint sessionKey)
            {
                Session session = null;
                if (false == sessions.TryGetValue(sessionKey, out session))
                {
                    return null;
                }
                return session;
            }

            public static int Count
            {
                get
                {
                    return sessions.Count;
                }
            }
        }
    }
}
