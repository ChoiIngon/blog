using Gamnet;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Breakout.Server
{
    public class Session : Gamnet.Server.Session
    {
        public Bar bar;
        public Ball ball;
        public Room room;

        protected override void OnConnect()
        {
            Debug.Log("onConnect");
        }

        protected override void OnPause()
        {
            if (null == room)
            {
                return;
            }

            room.RemoveUser(this);
        }

        protected override void OnResume()
        {
        }

        protected override void OnClose()
        {
            if (null == room)
            {
                return;
            }

            room.RemoveUser(this);
        }

        public void Send(object msg)
        {
            FieldInfo fieldInfo = msg.GetType().GetField("PACKET_ID");
            uint packetId = (uint)fieldInfo.GetValue(msg);
            Gamnet.Packet packet = new Gamnet.Packet();
            packet.Id = packetId;
            packet.Serialize(msg);
            base.Send(packet);
        }
    }
}