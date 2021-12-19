using System;
using System.Collections.Generic;
using UnityEngine;

namespace Breakout.Server
{
    class Main : Gamnet.Util.MonoSingleton<Main>
    {
        private Gamnet.Server.Acceptor<Session> acceptor = new Gamnet.Server.Acceptor<Session>();

        [Serializable]
        public class Prefabs
        {
            public Server.Room room;
            public Ball ball;
            public Bar bar;
            public Block block;
        }
        public Prefabs prefabs = new Prefabs();

        public static class Room
        {
            private static Dictionary<uint, Server.Room> rooms = new Dictionary<uint, Server.Room>();

            public static Server.Room Find(uint roomId)
            {
                Server.Room room = null;
                if (false == rooms.TryGetValue(roomId, out room))
                {
                    room = Instantiate<Server.Room>(Main.Instance.prefabs.room);
                    room.Init();
                    room.Id = roomId;
                    room.name = $"Room_{roomId}";
                    room.transform.position = new Vector3((roomId - 1)* 20, 100, 0);
                    room.transform.SetParent(Main.Instance.transform);
                    rooms.Add(room.Id, room);
                }

                return room;
            }

            public static void Remove(uint roomId)
            {
                rooms.Remove(roomId);
            }
        }

        private void Start()
        {
            Gamnet.Util.Debug.Init();
            Gamnet.Log.Init("log", "BreakOut", 1);
            acceptor.Init(4000, 500);
            Debug.Log("Server Start");
        }

        private void Update()
        {
            Gamnet.Session.EventLoop.Update();
        }
    }
}
