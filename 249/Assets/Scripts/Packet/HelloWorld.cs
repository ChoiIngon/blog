using System;
using System.Collections;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using UnityEngine;

namespace Assets.Scripts.Packet
{
    class HelloWorld : Gamnet.PacketHandler<Gamnet.ServerSession>
    {
        public class Message1
        {
            public const uint MSG_ID = 1;
            public string greeting;
        }

        public class Message2
        {
            public const uint MSG_ID = 2;
            public string greeting;
        }

        public HelloWorld()
        {
            Debug.Log("HelloWorld");
        }

        public override uint Id()
        {
            return Message1.MSG_ID;
        }

        public override IEnumerator OnReceive(Gamnet.ServerSession session, Gamnet.Packet packet)
        {
            Message1 req = packet.Deserialize<Message1>();
            Gamnet.Log.Write(Gamnet.Log.LogLevel.DEV, req.greeting);

            var asyncTask = new Gamnet.Async.AsyncTask(session, () =>
            {
                // verrrry long term task
                Thread.Sleep(1000);
            });

            yield return asyncTask; // suspend. but resume again when the task finish.
            if (null != asyncTask.Exception) // check result of task. if null. success.
            {
                throw asyncTask.Exception;
            }

            Message1 ans = new Message1();
            ans.greeting = "Thanks";

            Gamnet.Packet ansPacket = new Gamnet.Packet();
            ansPacket.Clear();
            ansPacket.Id = 1;
            ansPacket.Serialize(ans);
            session.AsyncSend(ansPacket);

            var asyncReceive = new Gamnet.Async.AsyncReceive(session, Message2.MSG_ID, 1);
            yield return asyncReceive;
            if (null != asyncReceive.Exception)
            {
                Gamnet.Log.Write(Gamnet.Log.LogLevel.DEV, asyncReceive.Exception.ToString());
                yield break;
            }

            Message2 ntf = asyncReceive.Packet.Deserialize<Message2>();
            Gamnet.Log.Write(Gamnet.Log.LogLevel.DEV, ntf.greeting);
        }

        [Gamnet.TestMethod]
        public void Test_HelloWorld(Gamnet.Client client)
        {
            client.session.RegisterHandler<Message1>(Message1.MSG_ID, (Message1 ans) =>
            {
                client.session.UnregisterHandler(Message1.MSG_ID);

                Message2 ntf = new Message2();
                ntf.greeting = "fin";
                Gamnet.Packet ntfPacket = new Gamnet.Packet();
                ntfPacket.Id = Message2.MSG_ID;
                ntfPacket.Serialize(ntf);
                client.session.AsyncSend(ntfPacket);
            });

            Message1 req = new Message1();
            req.greeting = "Hello";

            Gamnet.Packet packet = new Gamnet.Packet();
            packet.Id = Message1.MSG_ID;
            packet.Serialize(req);
            client.session.AsyncSend(packet);
        }
    }
}
