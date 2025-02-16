using Data;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : Actor
{
    private Data.ShadowCast shadowcast;
    private Coroutine move = null;

    private void Start()
    {
        spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        spriteRenderer.sortingOrder = Dungeon.SortingOrder.Actor;

        this.agility = 3;
        this.sight = GameManager.Instance.maxRoomSize;
        
        this.skin = GameManager.Instance.resources.GetSkin("Player");
        this.direction = Direction.Down;
        this.SetAction(Action.Idle);

        Move((int)transform.position.x, (int)transform.position.y);
    }

    public override void Move(int x, int y)
    {
        base.Move(x, y);
        
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

        Camera.main.transform.position = new Vector3(transform.position.x, transform.position.y, Camera.main.transform.position.z);

        if (null != shadowcast)
        {
            foreach (var tileData in shadowcast.tiles)
            {
                var tile = dungeon.tiles[tileData.index];
                tile.Visible(false);
            }
        }

        shadowcast = dungeon.data.CastLight(x, y, sight);
        {
            foreach (var tileData in shadowcast.tiles)
            {
                var tile = dungeon.tiles[tileData.index];
                tile.Visible(true);
            }
        }

        var dest = dungeon.data.GetTile(x, y);
        
        if (dungeon.data.end == dest)
        {
            GameManager.Instance.CreateDungeon();
            return;
        }

        MonsterManager.Instance.Update();
    }
    
    public void Clear()
    {
        shadowcast = null;
    }
    
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
            if (0 < path.tiles.Count)
            {
                path.tiles.RemoveAt(0);
                move = StartCoroutine(MoveCoroutine(path.tiles));
            }
        }
    }

    private IEnumerator MoveCoroutine(List<Tile> tiles)
    {
        foreach(var tile in tiles)
        {
            Move((int)tile.rect.x, (int)tile.rect.y);
            yield return new WaitForSeconds(GameManager.TurnPassSpeed);
        }

        move = null;
        yield break;
    }
}
