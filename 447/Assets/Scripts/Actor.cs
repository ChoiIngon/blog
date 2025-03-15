using System.Collections;
using UnityEngine;

public class Actor : MonoBehaviour
{
    public const int SortingOrder = DungeonObject.SortingOrder + 1;
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

    public Meta meta;
    public int health = 0;
    public int mana = 0;
    public int stamina = 0;

    public SpriteRenderer spriteRenderer { get; protected set; }
    public Action action { get; private set; }
    public int direction { get; protected set; } = Direction.Down;

    public TileMap tileMap { get; protected set; }
    public Tile tile { get; private set; }
    
    private Coroutine animationCoroutine;

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

    public virtual void Attack(Actor target)
    {
        this.direction = GetDirection(target.transform.position);
        SetAction(Action.Attack);
    }

    public virtual void Move(int x, int y)
    {
        this.direction = GetDirection(new Vector3 (x, y));
        //SetAction(Action.Walk);

        var nextTile = tileMap.GetTile(x, y);
        if (null == nextTile)
        {
            return;
        }

        if (Tile.Type.Wall == nextTile.type)
        {
            return;
        }

        if (null != nextTile.dungeonObject && true == nextTile.dungeonObject.block)
        {
            return;
        }

        if (null != nextTile.actor)
        {
            return;
        }

        transform.position = new Vector3(x, y);

        if (null != tile)   // 기존에 올라가 있던 타일이 있다면 타일이 가지고 있던 actor를 null로 만들어 준다
        {
            tile.actor = null;
        }

        tile = nextTile;
        nextTile.actor = this;
    }
        
    public virtual void Destroy()
    { 
        if (tile != null)
        {
            tile.actor = null;
            tile = null;
        }

        gameObject.transform.parent = null;
        GameObject.DestroyImmediate(gameObject);
    }

    public IEnumerator SetAction(Action action)
    {
        if (null == meta.skin)
        {
            yield break;
        }

        StopAnimation();

        this.action = action;
        Skin.SpriteSheet spriteSheet = meta.skin.GetSpriteSheet(action, direction);
        if (null == spriteSheet)
        {
            yield break;
        }

        animationCoroutine = StartCoroutine(PlayAnimation(spriteSheet));
        yield return animationCoroutine;
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
    }

    private int GetDirection(Vector3 position)
    {
        if (transform.position.x < position.x)
        {
            return Direction.Right;
        }

        if (position.x < transform.position.x)
        {
            return Direction.Left;
        }

        if (transform.position.y < position.y)
        {
            return Direction.Up;
        }

        if (position.y < transform.position.y)
        {
            return Direction.Down;
        }

        return this.direction;
    }

    public class Meta
    {
        public string name;
        public Skin skin;

        public int health = 0;
        public int mana = 0;
        public int stamina = 0;
        public int agility = 0;
        public int sight = 0;
        public int strangth = 0;    // 물리 대미지에 영향
        public int defense = 0;     // 방어력
        public int intelligence = 0;    // 마법 공격력에 영향
        public int perception = 0; // 지각, 함정 탐지 등에 영향
        public int resistance = 0;
        public int luck = 0;
    }

    public static T Create<T>(Meta meta, TileMap tileMap, Vector3 position) where T : Actor
    {
        GameObject go = new GameObject(meta.name);
        go.transform.parent = tileMap.gameObject.transform;
        var actor = go.AddComponent<T>();
        actor.spriteRenderer = go.AddComponent<SpriteRenderer>();
        actor.spriteRenderer.sortingOrder = SortingOrder;

        actor.meta = meta;
        actor.health = meta.health;
        actor.mana = meta.mana;
        actor.stamina = meta.stamina;

        actor.tileMap = tileMap;
        actor.direction = Direction.Down;
        actor.transform.position = position;

        actor.Move((int)position.x, (int)position.y);
        return actor;
    }
}