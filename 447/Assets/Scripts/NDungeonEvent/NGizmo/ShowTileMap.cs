using System.Collections;
using UnityEngine;

namespace NDungeonEvent.NGizmo
{
    public class ShowTileMap : DungeonEvent
    {
        private TileMap tileMap;
        private bool visible;

        public ShowTileMap(TileMap tileMap, bool visible)
        {
            this.tileMap = tileMap;
            this.visible = visible;
        }

        public IEnumerator OnEvent()
        {
            int tileCount = 0;
            for (int i = 0; i < tileMap.width * tileMap.height; i++)
            {
                Tile tile = tileMap.GetTile(i);
                if (null == tile)
                {
                    continue;
                }
                tileCount++;
            }

            for (int i = 0; i < tileMap.width * tileMap.height; i++)
            {
                Tile tile = tileMap.GetTile(i);
                if (null == tile)
                {
                    continue;
                }

                tile.Visible(visible);
                
                if(false == visible)
                {
                    tile.spriteRenderer.color = new Color(1.0f, 1.0f, 1.0f, 0.0f);
                    if (null != tile.dungeonObject)
                    {
                        tile.dungeonObject.spriteRenderer.color = new Color(1.0f, 1.0f, 1.0f, 0.0f);
                    }
                }

                yield return new WaitForSeconds(GameManager.Instance.tickTime / tileCount);
            }
        }
    }
}