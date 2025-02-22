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
        this.sight = 5;
        
        this.skin = GameManager.Instance.resources.GetSkin("Player");
        this.direction = Direction.Down;
        this.SetAction(Action.Idle);

        Move((int)transform.position.x, (int)transform.position.y);
    }

    public override bool Move(int x, int y)
    {
        Dungeon dungeon = GameManager.Instance.dungeon;
        if (false == base.Move(x, y))
        {
            var tile = dungeon.GetTile(x, y);
            if (null == tile.actor)
            {
                return false;
            }

            Attack(tile.actor);
            return true;
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
            return true;
        }

        return true;
    }

    public override void Attack(Actor actor)
    {
        base.Attack(actor);

        actor.health.value -= 1;
        if (0 >= actor.health.value)
        {
            Monster monster = actor as Monster;
            GameManager.Instance.dungeon.monsterManager.Remove(monster);
        }
    }

    public void Clear()
    {
        shadowcast = null;
    }
    
    void Update()
    {
        if (0 < GameManager.Instance.dungeon.turnManager.actions.Count)
        {
            return;
        }

        if (true == Input.GetKeyDown(KeyCode.UpArrow))
        {
            // Move((int)transform.position.x, (int)transform.position.y + 1);
            GameManager.Instance.dungeon.turnManager.actions.Add(new TurnManager.Move(this, (int)transform.position.x, (int)transform.position.y + 1));
            GameManager.Instance.dungeon.monsterManager.Update();
        }

        if (true == Input.GetKeyDown(KeyCode.DownArrow))
        {
            // Move((int)transform.position.x, (int)transform.position.y - 1);
            GameManager.Instance.dungeon.turnManager.actions.Add(new TurnManager.Move(this, (int)transform.position.x, (int)transform.position.y - 1));
            GameManager.Instance.dungeon.monsterManager.Update();
        }

        if (true == Input.GetKeyDown(KeyCode.LeftArrow))
        {
            // Move((int)transform.position.x - 1, (int)transform.position.y);
            GameManager.Instance.dungeon.turnManager.actions.Add(new TurnManager.Move(this, (int)transform.position.x - 1, (int)transform.position.y));
            GameManager.Instance.dungeon.monsterManager.Update();
        }

        if (true == Input.GetKeyDown(KeyCode.RightArrow))
        {
            // Move((int)transform.position.x + 1, (int)transform.position.y);
            GameManager.Instance.dungeon.turnManager.actions.Add(new TurnManager.Move(this, (int)transform.position.x + 1, (int)transform.position.y));
            GameManager.Instance.dungeon.monsterManager.Update();
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

    private IEnumerator MoveCoroutine(List<Data.Tile> tiles)
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
