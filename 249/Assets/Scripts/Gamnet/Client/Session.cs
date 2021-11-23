using Gamnet.SystemPacket;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

namespace Gamnet.Client
{
    public partial class Session : Gamnet.Session
    {
        public uint session_key { get; private set; }
        public string session_token { get; private set; }

        private abstract class IPacketHandler
        {
            public abstract void OnReceive(Packet packet);
        }

        private class PacketHandler<T> : IPacketHandler where T : new()
        {
            private Session session;
            public PacketHandler(Session session)
            {
                this.session = session;
            }
            public Action<T> onReceive;
            public override void OnReceive(Packet packet)
            {
                if (null == onReceive)
                {
                    return;
                }

                BinaryFormatter bf = new BinaryFormatter();
                packet.buffer.ms.Position = Packet.HEADER_SIZE;
                T msg = (T)bf.Deserialize(packet.buffer.ms);
                System.Type type = msg.GetType();
                onReceive(msg);
            }
        }

        public Action OnConnectEvent;
        public Action OnCloseEvent;
        public Action OnPauseEvent;
        public Action OnResumeEvent;
        public Action<System.Exception> OnErrorEvent;

        private UInt32 recv_packet_seq = 0;
        private Connector connector;
        private Dictionary<uint, IPacketHandler> handlers = new Dictionary<uint, IPacketHandler>();

        public Session()
        {
            session_key = 0;
            this.connector = new Connector(this);

            RegisterHandler<MsgSvrCli_EstablishSessionLink_Ans>(SystemPacket.MsgSvrCli_EstablishSessionLink_Ans.MSG_ID, Recv_EstabilshSessionLink_Ans);
            RegisterHandler<MsgSvrCli_DestroySessionLink_Ans>(SystemPacket.MsgSvrCli_DestroySessionLink_Ans.MSG_ID, Recv_DestroySessionLink_Ans);
            RegisterHandler<MsgSvrCli_RecoverSessionLink_Ans>(SystemPacket.MsgSvrCli_RecoverSessionLink_Ans.MSG_ID, Recv_RecoverSessionLink_Ans);
            RegisterHandler<MsgSvrCli_HeartBeat_Ans>(SystemPacket.MsgSvrCli_HeartBeat_Ans.MSG_ID, Recv_HeartBeat_Ans);
            RegisterHandler<MsgSvrCli_ReliableAck_Ntf>(SystemPacket.MsgSvrCli_ReliableAck_Ntf.MSG_ID, Recv_ReliableAck_Ntf);
        }

        public void AsyncConnect(string host, int port, int timeout_sec = 5)
        {
            this.connector.AsyncConnect(host, port, timeout_sec);
        }

        public void RegisterHandler<T>(uint msgId, Action<T> handler) where T : new()
        {
            PacketHandler<T> packetHandler = null;

            if (true == handlers.ContainsKey(msgId))
            {
                packetHandler = (PacketHandler<T>)handlers[msgId];
            }
            else
            {
                packetHandler = new PacketHandler<T>(this);
                handlers.Add(msgId, packetHandler);
            }
            packetHandler.onReceive += handler;
        }

        public void UnregisterHandler(uint msgId)
        {
            handlers.Remove(msgId);
        }

        public void Pause()
        {
            SocketClose();
        }

		public void Resume()
        {
            connector.AsyncReconnect();
        }

        protected override void OnConnect()
        {
            OnConnectEvent?.Invoke();
        }

        protected override void OnReceive(Packet packet)
        {
            try
            {
                if (false == handlers.ContainsKey(packet.Id))
                {
                    throw new System.Exception("can't find registered msg(id:" + packet.Id + ")");
                }

                if (false == packet.IsReliable || recv_packet_seq < packet.Seq)
                {
                    IPacketHandler handler = handlers[packet.Id];

                    handler.OnReceive(packet);

                    recv_packet_seq = Math.Max(recv_packet_seq, packet.Seq);
                    if (true == packet.IsReliable)
                    {
                        //Send_ReliableAck_Ntf(_recv_seq);
                    }
                }
            }
            catch (System.Exception e)
            {
                Error(e);
            }
        }

        protected override void OnPause()
        {
            socket.Close();
        }

        protected override void OnResume()
        {
            connector.AsyncReconnect();
        }

        protected override void OnClose()
        {
            OnCloseEvent?.Invoke();
        }

        protected override void OnError(Exception e)
        {
            Log.Write(Log.LogLevel.ERR, e.ToString());
            OnErrorEvent?.Invoke(e);
        }

        #region Close
        public override void Close()
        {
            if (0 != session_key)
            {
                Send_DestroySessionLink_Req();
                return;
            }
            SocketClose();
        }

        private void Send_DestroySessionLink_Req()
        {
            if (false == socket.Connected)
            {
                return;
            }
            SystemPacket.MsgCliSvr_DestroySessionLink_Req req = new SystemPacket.MsgCliSvr_DestroySessionLink_Req();

            Gamnet.Packet packet = new Gamnet.Packet();
            packet.Id = SystemPacket.MsgCliSvr_DestroySessionLink_Req.MSG_ID;
            packet.Serialize(req);
            AsyncSend(packet);
        }

        private void Recv_DestroySessionLink_Ans(MsgSvrCli_DestroySessionLink_Ans ans)
        {
            if (0 != ans.error_code)
            {
                Debug.LogError("connect fail(error_code:" + ans.error_code + ")");
                Error(null);
                return;
            }

            session_key = 0;
            session_token = "";
            SocketClose();
        }

        private void SocketClose()
        {
            if (null == socket)
            {
                return;
            }

            if (false == socket.Connected)
            {
                return;
            }

            try
            {
                socket.BeginDisconnect(false, new AsyncCallback(SocketCloseCallback), socket);
            }
            catch (SocketException e)
            {
                Debug.LogError("[Session.Disconnect] exception:" + e.ToString());
                Error(e);
            }
            catch (ObjectDisposedException e)
            {
                Debug.LogError("[Session.Disconnect] exception:" + e.ToString());
            }
        }

        private void SocketCloseCallback(IAsyncResult result)
        {
            try
            {
                socket.EndDisconnect(result);
                if (0 != session_key)
                {
                    EventLoop.EnqueuEvent(new PauseEvent(this));
                }
                else
                {
                    EventLoop.EnqueuEvent(new CloseEvent(this));
                }
            }
            catch (SocketException e)
            {
                Debug.Log($"[{Gamnet.Util.Debug.__FUNC__()}] session_state:" + state.ToString() + ", exception:" + e.ToString());
            }
        }
        #endregion

        public void Error(System.Exception e)
        {
            ErrorEvent evt = new ErrorEvent(this, e);
            evt.exception = e;
            EventLoop.EnqueuEvent(evt);
        }
    }
}
