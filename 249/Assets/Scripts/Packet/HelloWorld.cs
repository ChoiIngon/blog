using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Packet
{
    class HelloWorld : Gamnet.PacketHandler<Gamnet.ServerSession>
    {
        public override uint Id()
        {
            return 1;
        }

        public override IEnumerator<Gamnet.ServerSession> OnReceive(Gamnet.ServerSession session, Gamnet.Packet packet)
        {
            BinaryFormatter bf = new BinaryFormatter();
            packet.buffer.ms.Position = Gamnet.Packet.HEADER_SIZE;
            Assets.Scripts.Message message = (Assets.Scripts.Message)bf.Deserialize(packet.buffer.ms);

            Gamnet.Log.Write(Gamnet.Log.LogLevel.DEV, message.greeting);
            
            message.greeting = "Thanks";
            System.IO.MemoryStream ms = new System.IO.MemoryStream();
            bf.Serialize(ms, message);
            Gamnet.Packet ans = new Gamnet.Packet();
            ans.Id = 1;
            ans.Write(ms.GetBuffer());
            session.AsyncSend(ans);
            yield break;
        }
    }
}
