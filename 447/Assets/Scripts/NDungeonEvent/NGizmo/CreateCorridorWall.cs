using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NDungeonEvent.NGizmo
{
    public class CreateCorridorWall : DungeonEvent
    {
        private List<Corridor> corridors;

        public CreateCorridorWall(List<Corridor> corridors)
        {
            this.corridors = corridors;
        }

        public IEnumerator OnEvent()
        {
            System.Action<int, int> IfWallChangeColor = (int x, int y) =>
            {
                Tile tile = GameManager.Instance.tileMap.GetTile(x, y);
                if (null == tile)
                {
                    return;
                }

                if (Tile.Type.Wall != tile.type)
                {
                    return;
                }

                DungeonGizmo.Rect gizmo = GameManager.Instance.Gizmos.GetGroup(DungeonGizmo.GroupName.Tile).Get<DungeonGizmo.Rect>(tile.index);
                if (null == gizmo)
                {
                    gizmo = new DungeonGizmo.Rect($"Tile_{tile.index}", Color.white, 1.0f, 1.0f);
                    gizmo.position = new Vector3(tile.rect.x, tile.rect.y);
                    GameManager.Instance.Gizmos.GetGroup(DungeonGizmo.GroupName.Tile).Add(tile.index, gizmo);
                }

                gizmo.color = Color.white;
            };

            foreach (var corridor in corridors)
            {
                float interval = GameManager.Instance.tickTime / corridor.tiles.Count;
                foreach (Tile tile in corridor.tiles)
                {
                    DungeonGizmo.Rect gizmo = GameManager.Instance.Gizmos.GetGroup(DungeonGizmo.GroupName.Tile).Get<DungeonGizmo.Rect>(tile.index);
                    if (null == gizmo)
                    {
                        yield break;
                    }

                    gizmo.color = Color.red;

                    int x = (int)tile.rect.x;
                    int y = (int)tile.rect.y;

                    IfWallChangeColor(x - 1, y - 1);
                    IfWallChangeColor(x - 1, y);
                    IfWallChangeColor(x - 1, y + 1);
                    IfWallChangeColor(x, y - 1);
                    IfWallChangeColor(x, y + 1);
                    IfWallChangeColor(x + 1, y - 1);
                    IfWallChangeColor(x + 1, y);
                    IfWallChangeColor(x + 1, y + 1);

                    yield return new WaitForSeconds(interval);
                }
            }
        }
    }
}