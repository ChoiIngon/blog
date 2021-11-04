using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gamnet
{
    public class SessionManager
    {
        private Dictionary<uint, ServerSession> sessions = new Dictionary<uint, ServerSession>();
        public void Add(ServerSession session)
        {
            sessions.Add(session.session_key, session);
        }

        public void Remove(ServerSession session)
        {
            sessions.Remove(session.session_key);
        }

        public virtual void Dispatch(ServerSession session, Packet packet)
        {
        }
    }
}
