using System.Collections;
using System.Collections.Generic;
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

    void Start()
    {
        state = GameState.Init;
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
    }

    public void Play()
    {
        state = GameState.Play;

        bar.DetachBall();

        ball.SetDirection(Vector3.up + new Vector3(Random.Range(-0.5f, 0.5f), 0, 0));
    }
}