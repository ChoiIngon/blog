using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gamnet
{
    public interface ISessionManager
    {
        void Add(Session session);
        void Remove(Session session);
    }

    public class SessionManager<T> : ISessionManager where T : Session
    {
        private Dictionary<uint, T> sessions = new Dictionary<uint, T>();
        public void Add(Session session)
        {
            T t_session = session as T;
            sessions.Add(session.session_key, t_session);
        }

        public void Remove(Session session)
        {
            sessions.Remove(session.session_key);
        }
    }
}
