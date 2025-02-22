using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class Actor : MonoBehaviour
{
    public enum Action
    {
        Idle,
        Walk,
        Attack,
        Max
    }

    public static class Direction
    {
        public const int Up = 0;
        public const int Down = 1;
        public const int Left = 2;
        public const int Right = 3;
        public const int Max = 4;
    }

    public class Stat
    {
        public int value;
        public int max;
    }

    public Stat health = new Stat();
    public Stat mana = new Stat();
    public Stat stamina = new Stat();

    public int strangth = 0;    // 물리 대미지에 영향
    public int defense = 0;     // 방어력
    public int intelligence = 0;    // 마법 공격력에 영향
    public int agility = 0;     // 물리 공격 성공 여부, 회피 여부에 영향, 치명타, 공격 순서에 영향

    public int perception = 0; // 지각, 함정 탐지 등에 영향
    public int sight = 0; // 시야. 지각이 높아질 수록 시야도 커진다.
    public int resistance = 0;
        
    public int luck = 0;

    public Skin skin;

    public SpriteRenderer spriteRenderer { get; protected set; }
    public Action action { get; private set; }
    public int direction { get; protected set; } = Direction.Down;
    public Dungeon.Tile occupyTile { get; private set; }
    private Coroutine animationCoroutine;

    public class Skin : ScriptableObject
    {
        public class SpriteSheet
        {
            public bool loop; // 반복 재생 여부
            public float playTime;  // 애니메이션 재생 시간
            public List<Sprite> sprites;

            public SpriteSheet(string[] spriteNames, float playTime, bool loop = true)
            {
                List<Sprite> sprites = new List<Sprite>();
                foreach (string spriteName in spriteNames)
                {
                    sprites.Add(GameManager.Instance.resources.GetSprite(spriteName));
                }
                this.playTime = playTime;
                this.sprites = sprites;
                this.loop = loop;
            }
        }

        public List<List<SpriteSheet>> actionSpriteSheets;

        public Skin()
        {
            actionSpriteSheets = new List<List<SpriteSheet>>();
            for (int i = 0; i < (int)Action.Max; i++)
            {
                List<SpriteSheet> spriteSheets = new List<SpriteSheet>();
                for (int j = 0; j < Direction.Max; j++)
                {
                    spriteSheets.Add(null);
                }
                actionSpriteSheets.Add(spriteSheets);
            }
        }

        public void AddSpriteSheet(Action action, int direction, SpriteSheet spriteSheet)
        {
            var spriteSheets = actionSpriteSheets[(int)action];
            spriteSheets[direction] = spriteSheet;
        }

        public SpriteSheet GetSpriteSheet(Action action, int direction)
        {
            var spriteSheets = actionSpriteSheets[(int)action];
            return spriteSheets[direction];
        }
    }

    public virtual void Attack(Actor actor)
    {
        if (transform.position.x < actor.transform.position.x)
        {
            this.direction = Direction.Right;
        }

        if (actor.transform.position.x < transform.position.x)
        {
            this.direction = Direction.Left;
        }

        if (transform.position.y < actor.transform.position.y)
        {
            this.direction = Direction.Up;
        }

        if (actor.transform.position.y < transform.position.y)
        {
            this.direction = Direction.Down;
        }

        SetAction(Action.Attack);
    }

    public virtual bool Move(int x, int y)
    {
        if (transform.position.x < x)
        {
            this.direction = Direction.Right;
        }

        if (x < transform.position.x)
        {
            this.direction = Direction.Left;
        }

        if (transform.position.y < y)
        {
            this.direction = Direction.Up;
        }

        if (y < transform.position.y)
        {
            this.direction = Direction.Down;
        }

        Dungeon dungeon = GameManager.Instance.dungeon;

        var tile = dungeon.GetTile(x, y);
        if (null == tile)
        {
            return false;
        }

        if (Data.Tile.Type.Wall == tile.data.type)
        {
            return false;
        }

        if (null != tile.actor)
        {
            return false;
        }

        transform.position = new Vector3(x, y);

        if (null != occupyTile)
        {
            occupyTile.actor = null;
        }

        occupyTile = tile;
        tile.actor = this;

        SetAction(Action.Walk);
        return true;
    }

    public virtual void Destroy()
    {
        if (occupyTile != null)
        {
            occupyTile.actor = null;
            occupyTile = null;
        }

        gameObject.transform.parent = null;
        GameObject.DestroyImmediate(gameObject);
    }

    public void SetAction(Action action)
    {
        if (null == skin)
        {
            return;
        }

        if (null == spriteRenderer)
        {
            spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
            if (null == spriteRenderer)
            {
                spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
            }
        }

        StopAnimation();

        this.action = action;
        Skin.SpriteSheet spriteSheet = skin.GetSpriteSheet(action, direction);
        if (null == spriteSheet)
        {
            return;
        }

        animationCoroutine = StartCoroutine(PlayAnimation(spriteSheet));
    }

    public void Visible(bool flag)
    {
        Color color = spriteRenderer.color;
        float alpha = 1.0f;
        if (false == flag)
        {
            alpha = 0.0f;
        }
        color = new Color(color.r, color.g, color.b, alpha);
        spriteRenderer.color = color;
    }

    private void StopAnimation()
    {
        if (null == animationCoroutine)
        {
            return;
        }

        StopCoroutine(animationCoroutine);
        animationCoroutine = null;
    }

    private IEnumerator PlayAnimation(Skin.SpriteSheet spriteSheet)
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
                yield return new WaitForSeconds(spriteSheet.playTime / spriteSheet.sprites.Count);
            }
        } while (spriteSheet.loop);

        SetAction(Action.Idle);
    }
}
