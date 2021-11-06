using System.Collections;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using UnityEngine;

namespace Assets.Scripts.Packet
{
    class HelloWorld : Gamnet.PacketHandler<Gamnet.ServerSession>
    {
        public override uint Id()
        {
            return 1;
        }

        public override IEnumerator OnReceive(Gamnet.ServerSession session, Gamnet.Packet packet)
        {
            Assets.Scripts.Message req = packet.Deserialize<Assets.Scripts.Message>();

            Gamnet.Log.Write(Gamnet.Log.LogLevel.DEV, req.greeting);

            var asyncTask = new Gamnet.Async.AsyncTask(session, () =>
            {
                // verrrry long term task
                Thread.Sleep(1000);
            });
            yield return asyncTask; // return. but resume again when the task finish.

            if (null != asyncTask.Exception) // check result of task. if null. success.
            {
                throw asyncTask.Exception;
            }

            Assets.Scripts.Message ans = new Assets.Scripts.Message();
            ans.greeting = "Thanks";

            packet.Clear();
            packet.Id = 1;
            packet.Serialize(ans);
            session.AsyncSend(packet);

            var asyncReceive = new Gamnet.Async.AsyncReceive(session, 2, 1);
            yield return asyncReceive;
            if (null != asyncReceive.Exception)
            {
                Gamnet.Log.Write(Gamnet.Log.LogLevel.DEV, "timeout");
            }

            Assets.Scripts.Message req2 = asyncReceive.Packet.Deserialize<Assets.Scripts.Message>();
            Gamnet.Log.Write(Gamnet.Log.LogLevel.DEV, req2.greeting);
        }
    }
}
