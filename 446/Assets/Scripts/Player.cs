using Data;
using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

public class Player : MonoBehaviour
{
    public DungeonGizmo.Point gizmo;
    public Data.ShadowCast sight;

    public int sightRange;

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

        transform.position = new Vector3(x, y);
        gizmo.SetPosition(new Vector3(transform.position.x + 0.5f, transform.position.y + 0.5f));
        Camera.main.transform.position = new Vector3(transform.position.x, transform.position.y, Camera.main.transform.position.z);

        sight = dungeon.data.CastLight(x, y, sightRange);
        {
            foreach (var tileData in sight.tiles)
            {
                var tile = dungeon.tiles[tileData.index];
                tile.Visible(true);
            }
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
        foreach(var tile in pathFinder.tiles)
        {
            Move((int)tile.rect.x, (int)tile.rect.y);
            yield return new WaitForSeconds(0.1f);
        }

        move = null;
        yield break;
    }

}
