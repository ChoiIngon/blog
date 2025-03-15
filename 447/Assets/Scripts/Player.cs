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
}