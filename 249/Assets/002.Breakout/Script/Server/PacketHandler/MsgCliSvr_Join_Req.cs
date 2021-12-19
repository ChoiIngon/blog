using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Breakout.Server
{
    class MsgCliSvr_Join_Req : Gamnet.Server.PacketHandler<Session>
    {
        public override uint Id()
        {
            return Packet.MsgCliSvr_Join_Req.PACKET_ID;
        }

        public override IEnumerator OnReceive(Session session, Gamnet.Packet packet)
        {
            Packet.MsgCliSvr_Join_Req req = packet.Deserialize<Packet.MsgCliSvr_Join_Req>();
            Packet.MsgSvrCli_Join_Ans ans = new Packet.MsgSvrCli_Join_Ans();
            ans.errorCode = Packet.ErrorCode.Success;

            Room room = Main.Room.Find(req.roomId);
            room.AddUser(session);

            Bar bar = GameObject.Instantiate<Bar>(Main.Instance.prefabs.bar);
            bar.id = Room.objectId++;
            bar.transform.SetParent(room.transform);
            bar.Init(room);

            Ball ball = GameObject.Instantiate<Ball>(Main.Instance.prefabs.ball);
            ball.id = Room.objectId++;
            ball.transform.SetParent(room.transform);
            ball.Init(room);

            session.room = room;
            session.bar = bar;
            session.ball = ball;

            ans.ball.id = session.ball.id;
            ans.ball.localPosition = session.ball.transform.localPosition;
            ans.ball.rotation = session.ball.transform.rotation;
            ans.ball.velocity = session.ball.rigidBody.velocity;

            ans.bar.id = session.bar.id;
            ans.bar.localPosition = session.bar.transform.localPosition;
            ans.bar.rotation = session.bar.transform.rotation;
            //ans.bar.velocity = session.bar.rigidBody.velocity;

            session.Send(ans);

            session.bar.AttachBall(session.ball);

            if (1 == room.sessions.Count)
            {
                Main.Room.Remove(room.Id);

                Packet.MsgSvrCli_Ready_Ntf ntf = new Packet.MsgSvrCli_Ready_Ntf();
                room.CreateBlocks();
                foreach (var itr in room.blocks)
                {
                    Block block = itr.Value;
                    ntf.blocks.Add(new Packet.Block { id=block.id, type = block.meta.type, localPosition = block.transform.localPosition });
                }
                room.state = Room.State.Ready;

                session.Send(ntf);
            }

            yield break;
        }
    }
}
