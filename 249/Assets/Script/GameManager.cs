using UnityEngine;

public class GameManager : Gamnet.Util.MonoSingleton<GameManager>
{
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

    void Start()
    {
        state = GameState.Init;
        Init();
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
        for (int x = -8; x <= 8; x += 2)
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
}