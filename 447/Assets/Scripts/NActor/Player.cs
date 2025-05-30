using NDungeonEvent.NActor;
using UnityEngine;

public class Player : Actor
{
    private ShadowCast shadowCast;
    public bool hasKey;

    public static Player Create(TileMap tileMap)
    {
        if (null == tileMap.startTile)
        {
            return null;
        }

        Tile startTile = tileMap.startTile;

        Actor.Meta meta = new Actor.Meta();
        meta.name = "Player";
        meta.skin = GameManager.Instance.Resources.GetSkin("Actor");
        meta.agility = 6;
        meta.sight = 6;

        Vector3 position = new Vector3(startTile.rect.x + 1, startTile.rect.y);
        var player = Actor.Create<Player>(meta, tileMap, position);
        player.hasKey = false;
        DungeonEventQueue.Instance.Enqueue(new NDungeonEvent.NActor.RefreshFoV(player));
        return player;
    }

    private void Update()
    {
        if (true == DungeonEventQueue.Instance.Active)
        {
            return;
        }

        Vector3 offset = Vector3.zero;
        if (true == Input.GetKeyDown(KeyCode.UpArrow))
        {
            offset = new Vector3( 0, 1);
        }

        if (true == Input.GetKeyDown(KeyCode.DownArrow))
        {
            offset = new Vector3( 0, -1);
        }

        if (true == Input.GetKeyDown(KeyCode.LeftArrow))
        {
            offset = new Vector3(-1, 0);
        }

        if (true == Input.GetKeyDown(KeyCode.RightArrow))
        {
            offset = new Vector3( 1, 0);
        }

        if (Vector3.zero != offset)
        {
            Tile tile = tileMap.GetTile((int)transform.position.x + (int)offset.x, (int)transform.position.y + (int)offset.y);
            if (null != tile && null != tile.actor)
            {
                Attack(tile.actor);
            }
            else if (null != tile && null != tile.dungeonObject)
            {
                for (int i = 0; i < (int)DungeonObject.Interaction.Max; i++)
                {
                    if (null == tile.dungeonObject)
                    {
                        continue;
                    }
                    
                    var interection = tile.dungeonObject.GetInteraction((DungeonObject.Interaction)i);
                    if (null == interection)
                    {
                        continue;
                    }

                    interection(this);
                }
				Move((int)tile.rect.x, (int)tile.rect.y);
			}
            else
            {
				Move((int)tile.rect.x, (int)tile.rect.y);
			}
            
            tileMap.monsters.Update(this);
            DungeonEventQueue.Instance.Enqueue(new NDungeonEvent.NActor.RefreshFoV(this));
        }
    }

    public override void Move(int x, int y)
    {
        base.Move(x, y);
        Rect cameraBoundary = new Rect(position.x, position.y, meta.sight * 2, meta.sight * 2);
        GameManager.AdjustOrthographicCamera(cameraBoundary);
    }

    public override void Attack(Actor target)
    {
        base.Attack(target);

        target.health -= 1;

        DungeonEventQueue.Instance.Enqueue(new NDungeonEvent.NActor.Attack(this, target, target.health, 1));

        if (0 >= target.health)
        {
            if (null != target.tile)
            {
                target.tile.actor = null;
            }

            tileMap.monsters.Remove((Monster)target);
            DungeonEventQueue.Instance.Enqueue(new NDungeonEvent.NActor.Destroy(target));
        }
    }

    public void RefreshFoV()
    {
        if (null != shadowCast)
        {
            foreach (var tile in shadowCast.tiles)
            {
                tile.Visible(false);
            }
        }

        int x = (int)transform.position.x;
        int y = (int)transform.position.y;

        shadowCast = tileMap.CastLight(x, y, meta.sight);
        if(null != shadowCast)
        {
            foreach (var tile in shadowCast.tiles)
            {
                tile.Visible(true);
            }
        }
    }
}