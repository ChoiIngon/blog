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

        public void CreateBlocks()
        {
            uint blockId = 1;
            for (float x = -8f; x <= 8f; x += 2f)
            {
                for (int y = 3; y < 12; y++)
                {
                    Block block = Instantiate<Block>(Main.Instance.prefabs.block);
                    block.Init(this, Block.metaData[Random.Range(0, Block.metaData.Length)].type);
                    block.id = blockId++;
                    block.name = $"serverblock_{block.id}";
                    block.gameObject.AddComponent<Block.Collider>();
                    block.transform.SetParent(transform);
                    block.transform.localPosition = new Vector3(x, y, 0);
                    blocks.Add(block.id, block);
                }
            }
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

        public override void SyncWorld()
        {
            Packet.MsgSvrCli_SyncWorld_Ntf ntf = new Packet.MsgSvrCli_SyncWorld_Ntf();

            foreach (Session session in sessions)
            {
                if (null != session.bar)
                {
                    Packet.Object obj = new Packet.Object();
                    obj.id = session.bar.id;
                    obj.localPosition = session.bar.transform.localPosition;
                    obj.rotation = session.bar.transform.rotation;
                    //obj.velocity = session.bar.rigidBody.velocity;
                    ntf.objects.Add(obj);
                }

                if (null != session.ball)
                {
                    Packet.Object obj = new Packet.Object();
                    obj.id = session.ball.id;
                    obj.localPosition = session.ball.transform.localPosition;
                    obj.rotation = session.ball.transform.rotation;
                    obj.velocity = session.ball.rigidBody.velocity;
                    ntf.objects.Add(obj);
                }
            }

            foreach (Session session in sessions)
            {
                session.Send(ntf);
            }
        }

        public override void SyncBlock(Block block)
        {
            Packet.MsgSvrCli_BlockHit_Ntf ntf = new Packet.MsgSvrCli_BlockHit_Ntf();
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
