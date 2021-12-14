using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class GameManager : Gamnet.Util.MonoSingleton<GameManager>
{
    private Gamnet.Server.Acceptor<Breakout.Server.Session> acceptor = new Gamnet.Server.Acceptor<Breakout.Server.Session>();
    public enum GameState
    {
        Init,
        Ready,
        Play
    }

    public GameState state;
    public Bar bar;
    public Ball ball;
    public Block[] blockPrefabs;
    public Transform blocks;
    public Text myIP;
    public Gamnet.Client.Session session;
    public Button start;

    void Start()
    {
        Gamnet.Util.Debug.Init();
        Gamnet.Log.Init("log", "BreakOut", 1);
        acceptor.Init(4000, 500);
        StartCoroutine(GetMyIP());
        state = GameState.Init;
        Init();

        session = new Gamnet.Client.Session();
        session.AsyncConnect("127.0.0.1", 4000);
        session.OnConnectEvent += () => {
            start.gameObject.SetActive(true);
        };

        session.OnCloseEvent += () =>
        {
            Debug.Log("client session close");
        };

        start.gameObject.SetActive(false);
        start.onClick.AddListener(() =>
        {
            start.gameObject.SetActive(false);
            Packet.MsgCliSvr_Join_Req req = new Packet.MsgCliSvr_Join_Req();
            req.roomId = 1;

            Gamnet.Packet packet = new Gamnet.Packet();
            packet.Id = Packet.MsgCliSvr_Join_Req.PACKET_ID;
            packet.Serialize(req);
            session.Send(packet);
            session.RegisterHandler<Packet.MsgSvrCli_Join_Ans>(Packet.MsgSvrCli_Join_Ans.PACKET_ID, OnMsgSvrCli_Join_Ans);
        });
    }

    public void Init()
    {
        state = GameState.Init;

        bar.Init();
        ball.Init();
    }

    public void Ready()
    {
        state = GameState.Ready;

        bar.AttachBall();

        CreateBlocks();
    }

    public void Play()
    {
        state = GameState.Play;

        bar.DetachBall();
        ball.SetDirection(Vector3.up + new Vector3(Random.Range(-0.5f, 0.5f), 0, 0));
    }

    public void CreateBlocks()
    {
        for (float x = -8f; x <= 8f; x += 2f)
        {
            for (int y = 3; y < 12; y++)
            {
                Block block = Instantiate<Block>(blockPrefabs[Random.Range(0, blockPrefabs.Length)]);
                block.name = $"block_{x}";
                block.transform.SetParent(blocks);
                block.transform.localPosition = new Vector3(x, y, 0);
            }
        }
    }

    public IEnumerator GetMyIP()
    {
        //https://ipinfo.io/ip",
		//"http://bot.whatismyipaddress.com",
		//"http://ipecho.net/plain",
		//"http://ipv4.icanhazip.com"

        using (UnityWebRequest request = UnityWebRequest.Get("https://ipinfo.io/ip"))
        {
            yield return request.SendWebRequest();

            if (true == request.isNetworkError)
            {
                myIP.text = "---.---.---.---";
                yield break;
            }
            myIP.text = Encoding.Default.GetString(request.downloadHandler.data);
        }
    }

    private void Update()
    {
        Gamnet.Session.EventLoop.Update();
    }

    private void OnMsgSvrCli_Join_Ans(Packet.MsgSvrCli_Join_Ans ans)
    {
    }
}