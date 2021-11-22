using System;
using System.Collections;
using UnityEngine;

namespace Gamnet.Server
{
    public class Session : Gamnet.Session
    {
        public static UInt32 SESSION_KEY = 0;
        public static SessionManager session_manager = new SessionManager();
        public IDispatcher dispatcher;
        public string session_token;
        public bool enable_handover { get; private set; }

        public Session() : base(++SESSION_KEY)
        {
            enable_handover = false;
        }

        protected override void OnReceive(Packet packet)
        {
            dispatcher.OnReceive(this, packet);
        }

        protected override void OnPassiveClose()
        {
            OnPause();
        }

        protected override void OnEnableHandOver(bool flag)
        {
            enable_handover = flag;
        }

        public new class CreateEvent : SessionEvent
        {
            public CreateEvent(Session session) : base(session) { }
            public override void OnEvent()
            {
                Server.Session serverSession = session as Server.Session;
                Server.Session.session_manager.Add(serverSession);
                serverSession.OnCreate();
            }
        }

        public new class DestoryEvent : SessionEvent
        {
            public DestoryEvent(Session session) : base(session) { }
            public override void OnEvent()
            {
                Server.Session serverSession = session as Server.Session;
                serverSession.OnDestory();
                Server.Session.session_manager.Remove(serverSession);
            }
        }

        public class PacketHandler_EnableHandOver<SESSION_T> : PacketHandler<SESSION_T> where SESSION_T : Server.Session
        {
            public override uint Id()
            {
                return Gamnet.SystemPacket.MsgCliSvr_EnableHandOver_Req.MSG_ID;
            }

            public override IEnumerator OnReceive(SESSION_T session, Gamnet.Packet packet)
            {
                Gamnet.SystemPacket.MsgCliSvr_EnableHandOver_Req req = packet.Deserialize<Gamnet.SystemPacket.MsgCliSvr_EnableHandOver_Req>();
                Gamnet.SystemPacket.MsgSvrCli_EnableHandOver_Ans ans = new Gamnet.SystemPacket.MsgSvrCli_EnableHandOver_Ans();
                ans.error_code = 0;
                ans.flag = session.enable_handover;

                try
                {
                    if (session.enable_handover == req.flag)
                    {
                        ans.flag = req.flag;
                    }
                    else
                    {
                        if (true == req.flag)
                        {
                            if (string.Empty != session.session_token)
                            {
                                throw new System.Exception();
                            }
                        }
                    }

                    

                    session.session_token = System.Guid.NewGuid().ToString();
                    session.OnEnableHandOver(true);

                    ans.session_key = session.session_key;
                    ans.session_token = session.session_token;
                }
                catch (System.Exception e)
                {
                    Debug.LogError(e.ToString());
                }

                Gamnet.Packet ansPacket = new Gamnet.Packet();
                ansPacket.Id = Gamnet.SystemPacket.MsgSvrCli_EnableHandOver_Ans.MSG_ID;
                ansPacket.Serialize(ans);
                session.AsyncSend(ansPacket);
                yield break;
            }
        }

        public class PacketHandler_Reconnect<SESSION_T> : PacketHandler<SESSION_T> where SESSION_T : Server.Session
        {
            public override uint Id()
            {
                return Gamnet.SystemPacket.MsgCliSvr_Reconnect_Req.MSG_ID;
            }

            public override IEnumerator OnReceive(SESSION_T session, Gamnet.Packet packet)
            {
                Gamnet.SystemPacket.MsgCliSvr_Reconnect_Req req = packet.Deserialize<Gamnet.SystemPacket.MsgCliSvr_Reconnect_Req>();
                Gamnet.SystemPacket.MsgSvrCli_Reconnect_Ans ans = new Gamnet.SystemPacket.MsgSvrCli_Reconnect_Ans();
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
        }

        public class PacketHandler_Close<SESSION_T> : PacketHandler<SESSION_T> where SESSION_T : Server.Session
        {
            public override uint Id()
            {
                return Gamnet.SystemPacket.MsgCliSvr_Close_Req.MSG_ID;
            }

            public override IEnumerator OnReceive(SESSION_T session, Gamnet.Packet packet)
            {
                Gamnet.SystemPacket.MsgCliSvr_Close_Req req = packet.Deserialize<Gamnet.SystemPacket.MsgCliSvr_Close_Req>();
                Gamnet.SystemPacket.MsgSvrCli_Close_Ans ans = new Gamnet.SystemPacket.MsgSvrCli_Close_Ans();
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
        }
    }
}
