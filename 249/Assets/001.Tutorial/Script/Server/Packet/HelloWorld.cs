﻿using System.Collections;
using System.Threading;
using UnityEngine;
using UnityServer.Common.Packet;

namespace UnityServer.Packet
{
    class HelloWorld : Gamnet.Server.PacketHandler<Server.Main.Session>
    {
        public override uint Id()
        {
            return MsgCliSvr_Greeting_Req.MSG_ID;
        }

        public override IEnumerator OnReceive(Server.Main.Session session, Gamnet.Packet packet)
        {
            Debug.Log($"UnityServer.Server.Packet.HelloWorld.OnReceive");
            MsgCliSvr_Greeting_Req req = packet.Deserialize<MsgCliSvr_Greeting_Req>();
            {   // verrrry long term task
                var asyncTask = new Gamnet.Async.AsyncTask(session, () =>
                {
                    Thread.Sleep(10);
                });
                yield return asyncTask; // suspend. but resume again when the task finish.
                if (null != asyncTask.Exception) // check result of task. if null. success.
                {
                    throw asyncTask.Exception;
                }
            }

            MsgSvrCli_Greeting_Ans ans = new MsgSvrCli_Greeting_Ans();
            ans.text = "ACK";
            session.Send< MsgSvrCli_Greeting_Ans>(ans);

            // wait other message async
            const int waitTimeoutSec = 60;
            var asyncReceive = new Gamnet.Async.AsyncReceive(session, MsgCliSvr_Greeting_Ntf.MSG_ID, waitTimeoutSec);
            yield return asyncReceive; // suspend. it would resume when MsgCliSvr_Greeting_Ntf arrives or timeout
            if (null != asyncReceive.Exception)
            {
                Gamnet.Log.Write(Gamnet.Log.LogLevel.DEV, asyncReceive.Exception.ToString());
                yield break;
            }

            MsgCliSvr_Greeting_Ntf ntf = asyncReceive.Packet.Deserialize<MsgCliSvr_Greeting_Ntf>();
        }

        [Gamnet.Server.TestMethod]
        public void Test_HelloWorld(UnityServer.SimulationClient client)
        {
            client.session.RegisterHandler(MsgSvrCli_Greeting_Ans.MSG_ID, (MsgSvrCli_Greeting_Ans ans) =>
            {
                client.number = 2;
                client.session.UnregisterHandler(MsgSvrCli_Greeting_Ans.MSG_ID);

                MsgCliSvr_Greeting_Ntf ntf = new MsgCliSvr_Greeting_Ntf();
                ntf.text = "FIN_" + client.number.ToString();

                Gamnet.Packet ntfPacket = new Gamnet.Packet();
                ntfPacket.Id = MsgCliSvr_Greeting_Ntf.MSG_ID;
                ntfPacket.Serialize(ntf);
                client.session.Send(ntfPacket);
                client.MoveNext();
            });

            client.number = 1;
            MsgCliSvr_Greeting_Req req = new MsgCliSvr_Greeting_Req();
            req.text = "SIN_" + client.number.ToString();

            Gamnet.Packet packet = new Gamnet.Packet();
            packet.Id = MsgCliSvr_Greeting_Req.MSG_ID;
            packet.Serialize(req);
            packet.IsReliable = true;
            Debug.Log($"HelloWorld.MsgCliSvr_Greeting_Req");
            client.session.Send(packet);
        }

        [Gamnet.Server.TestMethod]
        public void Test_PauseAndResume(UnityServer.SimulationClient client)
        {
            client.session.Pause();
            client.MoveNext();
        }
    }
}
