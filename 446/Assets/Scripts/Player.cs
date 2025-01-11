using Data;
using System.Collections;
using UnityEngine;

public class Player : MonoBehaviour
{
    public static class Direction
    {
        public const int front = 0;
        public const int back = 1;
        public const int right = 2;
        public const int left = 3;
    }

    public int direction = Direction.front;
    public SpriteRenderer spriteRenderer;
    public Data.ShadowCast sight;
    public int sightRange;

    public ActorAnimation actorAnimation;

    private IEnumerator Start()
    {
        actorAnimation = gameObject.AddComponent<ActorAnimation>();
        spriteRenderer = gameObject.AddComponent<SpriteRenderer>();

        yield return new WaitForEndOfFrame();

        spriteRenderer.sortingOrder = Dungeon.SortingOrder.Character;
        actorAnimation.skin = GameManager.Instance.resources.GetSkin("Player");
        actorAnimation.direction = ActorAnimation.Direction.Down;
        actorAnimation.Play(ActorAnimation.Action.Idle);

        sightRange = GameManager.Instance.maxRoomSize;

        Move((int)transform.position.x, (int)transform.position.y);
    }

    public void Move(int x, int y)
    {
        Dungeon dungeon = GameManager.Instance.dungeon;

        var destTile = dungeon.data.GetTile(x, y);
        if (null == destTile)
        {
            return;
        }
        
        if (Data.Tile.Type.Wall == destTile.type)
        {
            return;
        }

        if (null != sight)
        {
            foreach (var tileData in sight.tiles)
            {
                var tile = dungeon.tiles[tileData.index];
                tile.Visible(false);
            }
        }

        if (transform.position.x < x)
        {
            actorAnimation.direction = ActorAnimation.Direction.Right;
        }

        if (x < transform.position.x)
        {
            actorAnimation.direction = ActorAnimation.Direction.Left;
        }

        if (transform.position.y < y)
        {
            actorAnimation.direction = ActorAnimation.Direction.Up;
        }

        if (y < transform.position.y)
        {
            actorAnimation.direction = ActorAnimation.Direction.Down;
        }

        transform.position = new Vector3(x, y);
        Camera.main.transform.position = new Vector3(transform.position.x, transform.position.y, Camera.main.transform.position.z);

        sight = dungeon.data.CastLight(x, y, sightRange);
        {
            foreach (var tileData in sight.tiles)
            {
                var tile = dungeon.tiles[tileData.index];
                tile.Visible(true);
            }
        }

        if (dungeon.data.end == destTile)
        {
            GameManager.Instance.CreateDungeon();
        }
    }
    
    public void Clear()
    {
        sight = null;
    }

    Coroutine move = null;
    void Update()
    {
        if (true == Input.GetKeyDown(KeyCode.UpArrow))
        {
            Move((int)transform.position.x, (int)transform.position.y + 1);
        }

        if (true == Input.GetKeyDown(KeyCode.DownArrow))
        {
            Move((int)transform.position.x, (int)transform.position.y - 1);
        }

        if (true == Input.GetKeyDown(KeyCode.LeftArrow))
        {
            Move((int)transform.position.x - 1, (int)transform.position.y);
        }

        if (true == Input.GetKeyDown(KeyCode.RightArrow))
        {
            Move((int)transform.position.x + 1, (int)transform.position.y);
        }

        if (true == Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (false == Physics.Raycast(ray, out hit))
            {
                return;
            }

            if (null == hit.transform)
            {
                return;
            }

            if (null != move)
            {
                StopCoroutine(move);
            }

            var dungeon = GameManager.Instance.dungeon.data;
            var from = dungeon.GetTile((int)transform.position.x, (int)transform.position.y);
            var to = dungeon.GetTile((int)hit.point.x, (int)hit.point.y);
            var path = dungeon.FindPath(from, to);
            move = StartCoroutine(MoveCoroutine(path));
        }
    }

    private IEnumerator MoveCoroutine(AStarPathFinder pathFinder)
    {
        actorAnimation.Play(ActorAnimation.Action.Walk);
        foreach(var tile in pathFinder.tiles)
        {
            Move((int)tile.rect.x, (int)tile.rect.y);
            yield return new WaitForSeconds(GameManager.TurnPassSpeed);
        }

        actorAnimation.Play(ActorAnimation.Action.Idle);
        move = null;
        yield break;
    }

}
