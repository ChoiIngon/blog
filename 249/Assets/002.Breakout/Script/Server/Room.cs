using System.Collections.Generic;
using UnityEngine;

namespace Breakout.Server
{
    public class Room : Breakout.Room
    {
        public const float SYNC_INTERVAL = 0.5f;
        public uint Id;

        public float syncInterval;
        public List<Session> sessions = new List<Session>();
        public Dictionary<uint, Block> blocks = new Dictionary<uint, Block>();

        private void Start()
        {
            InvokeRepeating("SyncWorld", 0, SYNC_INTERVAL);
        }

        private void OnDestroy()
        {
            CancelInvoke();

            foreach (var itr in blocks)
            {
                Block block = itr.Value;
                GameObject.Destroy(block.gameObject);
            }

            blocks.Clear();
        }

        public void Ready()
        {
            uint blockId = 1;
            for (float x = -8f; x <= 8f; x += 2f)
            {
                for (int y = 3; y < 12; y++)
                {
                    Block block = Instantiate<Block>(Main.Instance.prefabs.block);
                    block.Init(this, Block.metaData[Random.Range(0, Block.metaData.Length)].type);
                    block.id = blockId++;
                    block.name = $"block_{block.id}";
                    block.gameObject.AddComponent<Block.Collider>();
                    block.transform.SetParent(transform);
                    block.transform.localPosition = new Vector3(x, y, 0);
                    blocks.Add(block.id, block);
                }
            }

            Packet.MsgSvrCli_Ready_Ntf ntf = new Packet.MsgSvrCli_Ready_Ntf();
            for(int i=0; i<sessions.Count; i++)
            {
                Session session = sessions[i];

                Bar bar = GameObject.Instantiate<Bar>(Main.Instance.prefabs.bar);
                bar.Init(this);
                bar.id = Room.objectId++;
                bar.transform.SetParent(transform);
                bar.destination = start_position[i];
                bar.transform.localPosition = start_position[i];

                Ball ball = GameObject.Instantiate<Ball>(Main.Instance.prefabs.ball);
                ball.Init(this);
                ball.id = Room.objectId++;
                ball.transform.SetParent(transform);
                ball.transform.localPosition = new Vector3(start_position[i].x, start_position[i].y + 1, start_position[i].z);

                session.room = this;
                session.bar = bar;
                session.ball = ball;

                Packet.Player player = new Packet.Player();
                player.playerNum = i + 1;
                player.bar.id = bar.id;
                player.bar.rotation = bar.transform.rotation;
                player.bar.localPosition = bar.transform.localPosition;
                player.bar.velocity = Vector3.zero;

                player.ball.id = ball.id;
                player.ball.rotation = ball.transform.rotation;
                player.ball.localPosition = ball.transform.localPosition;
                player.ball.velocity = ball.rigidBody.velocity;

                ball.transform.SetParent(bar.transform);
                ntf.players.Add(player);
            }

            foreach (var itr in blocks)
            {
                Block block = itr.Value;
                ntf.blocks.Add(new Packet.Block { id = block.id, type = block.meta.type, localPosition = block.transform.localPosition });
            }

            for(int i=0; i<sessions.Count; i++)
            {
                ntf.playerNum = i + 1;

                Session session = sessions[i];
                session.Send(ntf);
            }

            state = Room.State.Ready;
        }

        public void AddUser(Session session)
        {
            sessions.Add(session);
        }

        public void RemoveUser(Session session)
        {
            Packet.MsgSvrCli_DestroyObject_Ntf ntf = new Packet.MsgSvrCli_DestroyObject_Ntf();
            ntf.objectIds.Add(session.bar.id);
            ntf.objectIds.Add(session.ball.id);

            session.room = null;
            session.bar.transform.SetParent(null);
            session.ball.transform.SetParent(null);

            GameObject.Destroy(session.bar.gameObject);
            GameObject.Destroy(session.ball.gameObject);

            sessions.Remove(session);

            foreach (Session s in sessions)
            {
                s.Send(ntf);
            }

            if (0 == sessions.Count)
            {
                GameObject.Destroy(gameObject);
            }
        }

        public void SyncWorld()
        {
            foreach (Session session in sessions)
            {
                SyncBall(session.ball);
            }
        }

        public override void SyncBall(Ball ball)
        {
            Packet.MsgSvrCli_SyncBall_Ntf ntf = new Packet.MsgSvrCli_SyncBall_Ntf();

            Packet.Ball obj = new Packet.Ball();
            obj.id = ball.id;
            obj.localPosition = ball.transform.localPosition;
            obj.rotation = ball.transform.rotation;
            obj.velocity = ball.rigidBody.velocity;
            ntf.ball = obj;

            foreach (Session session in sessions)
            {
                session.Send(ntf);
            }
        }

        public override void SyncBlock(Block block)
        {
            Packet.MsgSvrCli_SyncBlock_Ntf ntf = new Packet.MsgSvrCli_SyncBlock_Ntf();
            ntf.id = block.id;
            ntf.durability = block.durability;

            foreach (Session session in sessions)
            {
                session.Send(ntf);
            }

            if (0 == block.durability)
            {
                blocks.Remove(block.id);
            }
        }
    }
}
