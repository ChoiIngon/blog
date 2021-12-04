using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace UnityServer.Server
{
    public class Session : Gamnet.Server.Session
    {
        public Room room;
        protected override void OnConnect()
        {
            Debug.Log($"{Gamnet.Util.Debug.__FUNC__()}");
        }

        protected override void OnClose()
        {
            if (null != room)
            {
                room.transform.SetParent(null);
                GameObject.Destroy(room.gameObject);
                room = null;
            }

            Debug.Log($"{Gamnet.Util.Debug.__FUNC__()}");
        }

        protected override void OnResume()
        {
            Debug.Log($"{Gamnet.Util.Debug.__FUNC__()}");
        }

        protected override void OnPause()
        {
            Debug.Log($"{Gamnet.Util.Debug.__FUNC__()}");
        }

        public void Send<MSG_T>(MSG_T msg)
        {
            FieldInfo fieldInfo = msg.GetType().GetField("MSG_ID");
            uint packetId = (uint)fieldInfo.GetValue(msg);
            Gamnet.Packet packet = new Gamnet.Packet();
            packet.Id = packetId;
            packet.Serialize(msg);
            base.Send(packet);
        }
    }
}