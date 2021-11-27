using System.Collections.Generic;

namespace Gamnet.Server
{
    public partial class Session : Gamnet.Session
    {
        private class SessionManager
        {
            private Dictionary<uint, Session> sessions = new Dictionary<uint, Session>();
            private static SessionManager instance = new SessionManager();

            public static void Add(Session session)
            {
                instance.sessions.Add(session.session_key, session);
            }

            public static void Remove(Session session)
            {
                instance.sessions.Remove(session.session_key);
            }

            public static Session Find(uint sessionKey)
            {
                Session session = null;
                if (false == instance.sessions.TryGetValue(sessionKey, out session))
                {
                    return null;
                }
                return session;
            }
        }
    }
}
