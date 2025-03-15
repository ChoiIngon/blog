using UnityEngine;

public class Player : Actor
{
    private ShadowCast shadowCast;

    public static Player Create(TileMap tileMap)
    {
        if (null == tileMap.start)
        {
            return null;
        }

        Tile startTile = tileMap.start;

        Actor.Meta meta = new Actor.Meta();
        meta.name = "Player";
        meta.skin = GameManager.Instance.Resources.GetSkin("Player");
        meta.agility = 3;
        meta.sight = 5;
        var player = Actor.Create<Player>(meta, tileMap, new Vector3(startTile.rect.x + 1, startTile.rect.y));

        return player;
    }

    private void Update()
    {
        if (true == DungeonEventQueue.Instance.Active)
        {
            return;
        }

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
    }

    public override void Move(int x, int y)
    {
        base.Move(x, y);
        Camera.main.transform.position = new Vector3(transform.position.x, transform.position.y, Camera.main.transform.position.z);
    }
}