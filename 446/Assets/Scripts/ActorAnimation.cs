using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActorAnimation : MonoBehaviour
{
    public enum Action
    {
        Idle,
        Walk
    }

    public static class Direction
    {
        public const int Up = 0;
        public const int Down = 1;
        public const int Left = 2;
        public const int Right = 3;
        public const int Max = 4;
    }

    public class SpriteSheet
    {
        public bool loop;
        public float loopTime;
        public List<Sprite> sprites;

        public SpriteSheet(List<Sprite> sprites, float loopTime, bool loop = true)
        {
            this.loopTime = loopTime;
            this.sprites = sprites;
            this.loop = loop;
        }

        public SpriteSheet(string[] spriteNames, float loopTime, bool loop = true)
        {
            List<Sprite> sprites = new List<Sprite>();
            foreach (string spriteName in spriteNames)
            {
                sprites.Add(GameManager.Instance.resources.GetSprite(spriteName));
            }
            this.loopTime = loopTime;
            this.sprites = sprites;
            this.loop = loop;
        }
    }

    public class Skin : ScriptableObject
    {
        public SpriteSheet[] idle = new SpriteSheet[Direction.Max];
        public SpriteSheet[] walk = new SpriteSheet[Direction.Max];

        public void SetIdleSprites(int direction, SpriteSheet spriteSheet)
        {
            idle[direction] = spriteSheet;
        }

        public void SetWalkSprites(int direction, SpriteSheet spriteSheet)
        {
            walk[direction] = spriteSheet;
        }
    }

    private SpriteRenderer spriteRenderer;
    private int _direction;
    private Action action;
    private Coroutine coroutine;

    public Skin skin;
    public int direction {
        set
        {
            if (_direction == value)
            {
                return;
            }
            
            _direction = value;
            Play(action);
        }
    }

    public void Play(Action action)
    {
        this.action = action;

        if (null == spriteRenderer)
        {
            spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
            if (null == spriteRenderer)
            {
                spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
            }
        }

        Stop();

        SpriteSheet spriteSheet = null;
        switch (action)
        {
            case Action.Idle:
                spriteSheet = skin.idle[_direction];
                break;
            case Action.Walk:
                spriteSheet = skin.walk[_direction];
                break;
        }

        coroutine = StartCoroutine(PlayAnimation(spriteSheet));
    }

    public void Stop()
    {
        if (null == coroutine)
        {
            return;
        }

        StopCoroutine(coroutine);
        coroutine = null;
    }

    private IEnumerator PlayAnimation(SpriteSheet spriteSheet)
    {
        if (0 == spriteSheet.sprites.Count)
        {
            yield break;
        }

        do
        {
            for (int i = 0; i < spriteSheet.sprites.Count; i++)
            {
                spriteRenderer.sprite = spriteSheet.sprites[i];
                yield return new WaitForSeconds(spriteSheet.loopTime / spriteSheet.sprites.Count);
            }
        } while (spriteSheet.loop);
    }
}
