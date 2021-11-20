using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gamnet.Server
{
    public class SessionManager
    {
        private Dictionary<uint, Session> sessions = new Dictionary<uint, Session>();

        public void Add(Session session)
        {
            sessions.Add(session.session_key, session);
        }

        public void Remove(Session session)
        {
            sessions.Remove(session.session_key);
        }
    }
}
