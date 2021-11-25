using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gamnet.Server
{
    public partial class Session : Gamnet.Session
    {
        public class PacketHandler_EstablishSessionLink<SESSION_T> : PacketHandler<SESSION_T> where SESSION_T : Server.Session
        {
            public override uint Id()
            {
                return Gamnet.SystemPacket.MsgCliSvr_EstablishSessionLink_Req.MSG_ID;
            }

            public override IEnumerator OnReceive(SESSION_T session, Gamnet.Packet packet)
            {
                Debug.Assert(Gamnet.Util.Debug.IsMainThread());
                //Debug.Log($"Gamnet.Server.Session.PacketHandler_EstablishSessionLink.OnReceive");
                SystemPacket.MsgCliSvr_EstablishSessionLink_Req req = packet.Deserialize<SystemPacket.MsgCliSvr_EstablishSessionLink_Req>();
                SystemPacket.MsgSvrCli_EstablishSessionLink_Ans ans = new SystemPacket.MsgSvrCli_EstablishSessionLink_Ans();

                ans.error_code = 0;
                try
                {
                    if (string.Empty != session.session_token)
                    {
                        throw new System.InvalidOperationException($"already linked session");
                    }

                    session.session_token = System.Guid.NewGuid().ToString();
                    session.link_establish = true;
                    session.OnConnect();
                }
                catch (System.Exception e)
                {
                    ans.error_code = e.HResult;
                    Debug.LogError(e.ToString());
                }

                ans.session_key = session.session_key;
                ans.session_token = session.session_token;

                Gamnet.Packet ansPacket = new Gamnet.Packet();
                ansPacket.Id = Gamnet.SystemPacket.MsgSvrCli_EstablishSessionLink_Ans.MSG_ID;
                ansPacket.Serialize(ans);
                session.Send(ansPacket);
                yield break;
            }

            public override bool IsSystemPacket
            {
                get { return true; }
            }
        }

        public class PacketHandler_RecoverSessionLink<SESSION_T> : PacketHandler<SESSION_T> where SESSION_T : Server.Session
        {
            public override uint Id()
            {
                return Gamnet.SystemPacket.MsgCliSvr_RecoverSessionLink_Req.MSG_ID;
            }

            public override IEnumerator OnReceive(SESSION_T session, Gamnet.Packet packet)
            {
                // Debug.Log($"Gamnet.Server.Session.PacketHandler_RecoverSessionLink.OnReceive");
                Gamnet.SystemPacket.MsgCliSvr_RecoverSessionLink_Req req = packet.Deserialize<Gamnet.SystemPacket.MsgCliSvr_RecoverSessionLink_Req>();
                Gamnet.SystemPacket.MsgSvrCli_RecoverSessionLink_Ans ans = new Gamnet.SystemPacket.MsgSvrCli_RecoverSessionLink_Ans();
                Gamnet.Packet ansPacket = new Gamnet.Packet();
                ansPacket.Id = Gamnet.SystemPacket.MsgSvrCli_RecoverSessionLink_Ans.MSG_ID;
                ans.error_code = 0;
                try
                {
                    Session prevSession = Session.SessionManager.Find(req.session_key);
                    if (null == prevSession)
                    {
                        throw new System.Exception();
                    }

                    if (prevSession.session_token != req.session_token)
                    {
                        throw new System.Exception();
                    }

                    prevSession.socket = session.socket;
                    prevSession.receiver = session.receiver;
                    prevSession.receiver.session = prevSession;
                    prevSession.link_establish = true;
                    session.socket = null;
                    session.receiver = null;
                    Session.SessionManager.Remove(session);

                    List<Packet> unsendPacketQueue = new List<Packet>();
                    foreach (Packet unsentPacket in prevSession.send_queue)
                    {
                        unsendPacketQueue.Add(unsentPacket);
                    }

                    prevSession.send_queue_index = 0;
                    prevSession.send_queue.Clear();

                    prevSession.OnResume();
                    ansPacket.Serialize(ans);
                    prevSession.Send(ansPacket);
                    foreach (Packet unsentPacket in unsendPacketQueue)
                    {
                        prevSession.Send(unsentPacket);
                    }
                    yield break;
                }
                catch (System.Exception e)
                {
                    Debug.LogError(e.ToString());
                }

                ansPacket.Serialize(ans);
                session.Send(ansPacket);
                yield break;
            }
            public override bool IsSystemPacket
            {
                get { return true; }
            }
        }

        public class PacketHandler_DestroySessionLink<SESSION_T> : PacketHandler<SESSION_T> where SESSION_T : Server.Session
        {
            public override uint Id()
            {
                return Gamnet.SystemPacket.MsgCliSvr_DestroySessionLink_Req.MSG_ID;
            }

            public override IEnumerator OnReceive(SESSION_T session, Gamnet.Packet packet)
            {
                // Debug.Log($"Gamnet.Server.Session.PacketHandler_DestroySessionLink.OnReceive");
                Gamnet.SystemPacket.MsgCliSvr_DestroySessionLink_Req req = packet.Deserialize<Gamnet.SystemPacket.MsgCliSvr_DestroySessionLink_Req>();
                Gamnet.SystemPacket.MsgSvrCli_DestroySessionLink_Ans ans = new Gamnet.SystemPacket.MsgSvrCli_DestroySessionLink_Ans();
                ans.error_code = 0;
                try
                {
                    if (false == session.link_establish)
                    {
                        throw new System.InvalidOperationException($"not linked session");
                    }

                    session.link_establish = false;
                }
                catch (System.Exception e)
                {
                    ans.error_code = e.HResult;
                    Debug.LogError(e.ToString());
                }

                Gamnet.Packet ansPacket = new Gamnet.Packet();
                ansPacket.Id = Gamnet.SystemPacket.MsgSvrCli_DestroySessionLink_Ans.MSG_ID;
                ansPacket.Serialize(ans);
                session.Send(ansPacket);
                yield break;
            }

            public override bool IsSystemPacket
            {
                get { return true; }
            }
        }

        public class PacketHandler_HeartBeat<SESSION_T> : PacketHandler<SESSION_T> where SESSION_T : Server.Session
        {
            public override uint Id()
            {
                return Gamnet.SystemPacket.MsgCliSvr_HeartBeat_Req.MSG_ID;
            }

            public override IEnumerator OnReceive(SESSION_T session, Gamnet.Packet packet)
            {
                Gamnet.SystemPacket.MsgCliSvr_HeartBeat_Req req = packet.Deserialize<Gamnet.SystemPacket.MsgCliSvr_HeartBeat_Req>();
                Gamnet.SystemPacket.MsgSvrCli_HeartBeat_Ans ans = new Gamnet.SystemPacket.MsgSvrCli_HeartBeat_Ans();
                ans.error_code = 0;
                try
                {
                }
                catch (System.Exception e)
                {
                    Debug.LogError(e.ToString());
                }
                yield break;
            }
            public override bool IsSystemPacket
            {
                get { return true; }
            }
        }

        public class PacketHandler_ReliableAck<SESSION_T> : PacketHandler<SESSION_T> where SESSION_T : Server.Session
        {
            public override uint Id()
            {
                return Gamnet.SystemPacket.MsgSvrCli_ReliableAck_Ntf.MSG_ID;
            }

            public override IEnumerator OnReceive(SESSION_T session, Gamnet.Packet packet)
            {
                Gamnet.SystemPacket.MsgCliSvr_ReliableAck_Ntf req = packet.Deserialize<Gamnet.SystemPacket.MsgCliSvr_ReliableAck_Ntf>();
                try
                {
                }
                catch (System.Exception e)
                {
                    Debug.LogError(e.ToString());
                }
                yield break;
            }
            public override bool IsSystemPacket
            {
                get { return true; }
            }
        }
    }
}
