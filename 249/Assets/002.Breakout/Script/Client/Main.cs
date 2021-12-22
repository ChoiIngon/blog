using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Breakout.Client
{
    public class Main : Gamnet.Util.MonoSingleton<Main>
    {
        private const int MOUSE_BUTTON_LEFT = 0;

        public Bar barPrefab;
        public Ball ballPrefab;
        public Block blockPrefab;

        [Serializable]
        public class UI
        {
            public Button start;
            public Button leave;
            public InputField roomId;
        }

        public UI ui = new UI();
        public Bar bar;
        public Ball ball;
        public Dictionary<uint, Block> blocks = new Dictionary<uint, Block>();
        public Dictionary<uint, GameObject> objects = new Dictionary<uint, GameObject>();
        public Room room;

        private Plane backPlane = new Plane(Vector3.forward, 0);

        void Start()
        {
            Gamnet.Util.Debug.Init();

            room = gameObject.AddComponent<Room>();
            room.Init();

            Network.OnConnectEvent += OnConnect;

            Network.RegisterHandler<Packet.MsgSvrCli_Join_Ans>(OnRecv_Join_Ans);
            Network.RegisterHandler<Packet.MsgSvrCli_Ready_Ntf>(OnRecv_Ready_Ntf);
            Network.RegisterHandler<Packet.MsgSvrCli_SyncBall_Ntf>(OnRecv_SyncWorld_Ntf);
            Network.RegisterHandler<Packet.MsgSvrCli_SyncBlock_Ntf>(OnRecv_SyncBlock_Ntf);
            Network.RegisterHandler<Packet.MsgSvrCli_SyncBar_Ntf>(OnRecv_SyncBar_Ntf);
            Network.RegisterHandler<Packet.MsgSvrCli_DestroyObject_Ntf>(OnRecv_DestroyObject_Ntf);
            Network.RegisterHandler<Packet.MsgSvrCli_Start_Ntf>(OnRecv_Start_Ntf);

            ui.start.gameObject.SetActive(true);
            ui.roomId.gameObject.SetActive(true);

            ui.start.onClick.AddListener(() =>
            {
                ui.start.gameObject.SetActive(false);
                ui.roomId.gameObject.SetActive(false);
                Network.Connect();
            });

            ui.leave.onClick.AddListener(() =>
            {
                ui.start.gameObject.SetActive(true);
                ui.roomId.gameObject.SetActive(true);

                foreach (var itr in blocks)
                {
                    Block block = itr.Value;
                    GameObject.Destroy(block.gameObject);
                }
                blocks.Clear();

                foreach (var itr in objects)
                {
                    GameObject obj = itr.Value;
                    GameObject.Destroy(obj);
                }
                objects.Clear();

                room.state = Room.State.Init;
                Network.Close();
            });
        }

        private void Update()
        {
            Gamnet.Session.EventLoop.Update();

            if (Room.State.Ready == room.state)
            {
                if (true == Input.GetMouseButtonUp(MOUSE_BUTTON_LEFT))
                {
                    room.state = Room.State.Play;
                    ball.transform.SetParent(room.transform);
                    ball.rigidBody.useGravity = false;
                    Packet.MsgCliSvr_Start_Ntf ntf = new Packet.MsgCliSvr_Start_Ntf();
                    Network.Send(ntf);
                }
            }

            // https://gamedevbeginner.com/how-to-convert-the-mouse-position-to-world-space-in-unity-2d-3d/#screen_to_world_3d
            if (Room.State.Init != room.state)
            {
                if (true == Input.GetMouseButton(MOUSE_BUTTON_LEFT))
                {
                    /*
                    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                    RaycastHit hit;
                    if (true == Physics.Raycast(ray, out hit, Mathf.Infinity))
                    {
                        Bar bar = hit.transform.GetComponent<Bar>();
                        if (null != bar)
                        {
                            isTouched = true;
                        }
                    }
                    */
                    float distance;
                    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                    if (true == backPlane.Raycast(ray, out distance))
                    {
                        Vector3 worldPosition = ray.GetPoint(distance);
                        // 월드 좌표를 로컬 좌표로 이동
                        bar.destination = room.transform.InverseTransformPoint(worldPosition);

                        Packet.MsgCliSvr_SyncBar_Ntf ntf = new Packet.MsgCliSvr_SyncBar_Ntf();
                        ntf.destination = bar.destination;
                        Network.Send(ntf);
                    }
                }
                //bar.rigidBody.velocity = Vector3.zero;
            }
        }

        public void OnConnect()
        {
            Packet.MsgCliSvr_Join_Req req = new Packet.MsgCliSvr_Join_Req();

            Text roomId = ui.roomId.transform.Find("Text").GetComponent<Text>();
            if ("" == roomId.text)
            {
                roomId = ui.roomId.transform.Find("Placeholder").GetComponent<Text>();
            }
            req.roomId = UInt32.Parse(roomId.text);
            Network.Send(req);
        }

        public void OnRecv_Join_Ans(Packet.MsgSvrCli_Join_Ans ans)
        {
        }

        public void OnRecv_Ready_Ntf(Packet.MsgSvrCli_Ready_Ntf ntf)
        {
            room.state = Room.State.Ready;

            foreach (Packet.Block block in ntf.blocks)
            {
                Block localBlock = Instantiate<Block>(blockPrefab);
                localBlock.Init(room, block.type);
                localBlock.id = block.id;
                localBlock.name = $"block_{block.id}";
                localBlock.transform.SetParent(transform);
                localBlock.transform.localPosition = block.localPosition;
                blocks.Add(block.id, localBlock);
            }

            foreach (Packet.Player player in ntf.players)
            {
                Bar bar = Instantiate<Bar>(barPrefab);
                bar.Init(room);
                bar.id = player.bar.id;
                bar.transform.SetParent(transform);
                bar.destination = player.bar.localPosition;
                bar.transform.rotation = player.bar.rotation;
                bar.transform.localPosition = player.bar.localPosition;
                objects.Add(bar.id, bar.gameObject);

                Ball ball = Instantiate<Ball>(ballPrefab);
                ball.Init(room);
                ball.id = player.ball.id;
                ball.transform.SetParent(transform);
                ball.rigidBody.velocity = player.ball.velocity;
                ball.transform.rotation = player.ball.rotation;
                ball.transform.localPosition = player.ball.localPosition;
                objects.Add(ball.id, ball.gameObject);
                ball.transform.SetParent(bar.transform);

                if (player.playerNum == ntf.playerNum)
                {
                    this.bar = bar;
                    this.ball = ball;
                }
                else
                {
                    {
                        Renderer renderer = bar.GetComponent<Renderer>();
                        renderer.material.SetColor("_Color", Color.red);
                    }
                    {
                        Renderer renderer = ball.GetComponent<Renderer>();
                        renderer.material.SetColor("_Color", Color.red);
                    }
                }
            }
        }

        public void OnRecv_SyncWorld_Ntf(Packet.MsgSvrCli_SyncBall_Ntf ntf)
        {
            GameObject go = null;
            if (false == this.objects.TryGetValue(ntf.ball.id, out go))
            {
                return;
            }

            Ball ball = go.GetComponent<Ball>();
            if (null == ball)
            {
                return;
            }

            ball.transform.localPosition = ntf.ball.localPosition;
            ball.transform.rotation = ntf.ball.rotation;
            ball.rigidBody.velocity = ntf.ball.velocity;
        }

        public void OnRecv_SyncBlock_Ntf(Packet.MsgSvrCli_SyncBlock_Ntf ntf)
        {
            Block block = null;
            if (false == blocks.TryGetValue(ntf.id, out block))
            {
                return;
            }

            block.transform.SetParent(null);
            GameObject.Destroy(block.gameObject);
            blocks.Remove(ntf.id);
        }

        public void OnRecv_SyncBar_Ntf(Packet.MsgSvrCli_SyncBar_Ntf ntf)
        {
            GameObject go;
            if (false == objects.TryGetValue(ntf.objectId, out go))
            {
                return;
            }

            Bar bar = go.GetComponent<Bar>();
            bar.destination = ntf.destination;
        }

        public void OnRecv_Start_Ntf(Packet.MsgSvrCli_Start_Ntf ntf)
        {
            GameObject go;
            if (false == objects.TryGetValue(ntf.objectId, out go))
            {
                return;
            }

            Ball ball = go.GetComponent<Ball>();
            if (null == ball)
            {
                return;
            }

            ball.transform.SetParent(room.transform);
            ball.rigidBody.useGravity = false;
        }

        public void OnRecv_DestroyObject_Ntf(Packet.MsgSvrCli_DestroyObject_Ntf ntf)
        {
            foreach (uint id in ntf.objectIds)
            {
                GameObject go = null;
                if(true == objects.TryGetValue(id, out go))
                {
                    GameObject.Destroy(go);
                    objects.Remove(id);
                }
            }
        }
    }
}
