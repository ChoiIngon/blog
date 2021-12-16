using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Breakout.Server
{
    class Manager : Gamnet.Util.Singleton<Manager>
    {
        public static class Room
        {
            private static Dictionary<uint, Server.Room> rooms = new Dictionary<uint, Server.Room>();

            public static Server.Room Find(uint roomId)
            {
                Server.Room room = null;
                if (false == rooms.TryGetValue(roomId, out room))
                {
                    GameObject obj = new GameObject();
                    room = obj.AddComponent<Server.Room>();
                    room.Id = roomId;
                    rooms.Add(room.Id, room);
                }

                return room;
            }

            public static void Remove(uint roomId)
            {
                rooms.Remove(roomId);
            }
        }
    }
}
