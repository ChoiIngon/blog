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
            public Button close;
            public InputField roomId;
        }

        public UI ui = new UI();
        public Bar bar;
        public Ball ball;
        public Dictionary<uint, Block> blocks = new Dictionary<uint, Block>();
        public Dictionary<uint, GameObject> objects = new Dictionary<uint, GameObject>();
        public Room room;

        public Plane backPlane = new Plane(Vector3.forward, 0);
        void Start()
        {
            Gamnet.Util.Debug.Init();

            room = gameObject.AddComponent<Room>();
            room.Init();

            Network.Connect();
            Network.OnConnectEvent += OnConnect;
            Network.RegisterHandler<Packet.MsgSvrCli_Join_Ans>(OnRecv_Join_Ans);
            Network.RegisterHandler<Packet.MsgSvrCli_Ready_Ntf>(OnRecv_Ready_Ntf);
            Network.RegisterHandler<Packet.MsgSvrCli_SyncWorld_Ntf>(OnRecv_SyncWorld_Ntf);
            Network.RegisterHandler<Packet.MsgSvrCli_BlockHit_Ntf>(OnRecv_BlockHit_Ntf);

            ui.start.gameObject.SetActive(false);
            ui.roomId.gameObject.SetActive(false);

            ui.start.onClick.AddListener(() =>
            {
                ui.start.gameObject.SetActive(false);
                ui.roomId.gameObject.SetActive(false);

                Packet.MsgCliSvr_Join_Req req = new Packet.MsgCliSvr_Join_Req();

                Text roomId = ui.roomId.transform.Find("Text").GetComponent<Text>();
                if ("" == roomId.text)
                {
                    roomId = ui.roomId.transform.Find("Placeholder").GetComponent<Text>();
                }
                req.roomId = UInt32.Parse(roomId.text);
                Network.Send(req);
            });

            ui.close.onClick.AddListener(() =>
            {
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
                    ball.transform.SetParent(transform);
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
                        /*
                        if (transform.position.x < worldPosition.x)
                        {
                            transform.position = new Vector3(transform.position.x + moveSpeed * Time.deltaTime, transform.position.y, transform.position.z);
                        }

                        if (transform.position.x > worldPosition.x)
                        {
                            transform.position = new Vector3(transform.position.x - moveSpeed * Time.deltaTime, transform.position.y, transform.position.z);
                        }
                        */
                        // 월드 좌표를 로컬 좌표로 이동
                        bar.position = room.transform.InverseTransformPoint(worldPosition);

                        Packet.MsgCliSvr_SyncBar_Ntf ntf = new Packet.MsgCliSvr_SyncBar_Ntf();
                        ntf.localPosition = bar.position;
                        Network.Send(ntf);
                    }
                }
                //bar.rigidBody.velocity = Vector3.zero;
            }
        }
        public void OnConnect()
        {
            ui.start.gameObject.SetActive(true);
            ui.roomId.gameObject.SetActive(true);
        }

        public void OnRecv_Join_Ans(Packet.MsgSvrCli_Join_Ans ans)
        {
            Bar bar = Instantiate<Bar>(barPrefab);
            bar.Init(room);
            bar.id = ans.bar.id;
            //bar.rigidBody.velocity = ans.bar.velocity;
            bar.transform.localPosition = ans.bar.localPosition;
            bar.transform.rotation = ans.bar.rotation;
            bar.transform.SetParent(transform);
            objects.Add(bar.id, bar.gameObject);
            this.bar = bar;

            Ball ball = Instantiate<Ball>(ballPrefab);
            ball.Init(room);
            ball.id = ans.ball.id;
            ball.rigidBody.velocity = ans.ball.velocity;
            ball.transform.localPosition = ans.ball.localPosition;
            ball.transform.rotation = ans.ball.rotation;
            ball.transform.SetParent(transform);
            objects.Add(ball.id, ball.gameObject);
            this.ball = ball;

            bar.AttachBall(ball);
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
        }

        public void OnRecv_SyncWorld_Ntf(Packet.MsgSvrCli_SyncWorld_Ntf ntf)
        {
            foreach (Packet.Object obj in ntf.objects)
            {
                GameObject go = null;
                if (false == this.objects.TryGetValue(obj.id, out go))
                {
                    continue;
                }

                go.transform.localPosition = obj.localPosition;
                go.transform.rotation = obj.rotation;
                Rigidbody rb = go.GetComponent<Rigidbody>();
                if (null != rb)
                {
                    rb.velocity = obj.velocity;
                }
            }
        }

        public void OnRecv_BlockHit_Ntf(Packet.MsgSvrCli_BlockHit_Ntf ntf)
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
    }
}
