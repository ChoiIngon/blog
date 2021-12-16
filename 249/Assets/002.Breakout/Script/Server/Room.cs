using System.Collections.Generic;
using UnityEngine;

namespace Breakout.Server
{
    public class Room : MonoBehaviour
    {
        public uint Id;
        public List<Session> sessions = new List<Session>();
        public void CreateBlocks()
        {
        }

        public void AddUser(Session session)
        {
            session.room = this;
            sessions.Add(session);

            
        }
    }
}
