using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace NDungeonEvent.NGizmo
{
    public class CreatedRoomWall : DungeonEvent
    {
        private List<Room> rooms;
        public CreatedRoomWall(List<Room> rooms)
        {
            this.rooms = rooms;
        }

        public IEnumerator OnEvent()
        {
            int tileCount = 0;
            foreach (Room room in rooms)
            {
                tileCount += (int)room.rect.width * 2 + (int)room.rect.height * 2;
            }

            float interval = GameManager.Instance.tickTime / tileCount;
            foreach (Room room in rooms)
            {
                // 방을 벽들로 막아 버림
                for (int x = (int)room.rect.xMin; x < (int)room.rect.xMax; x++)
                {
                    BuildWallOnTile(x, (int)room.rect.yMax - 1);
                    yield return new WaitForSeconds(interval);
                }

                for (int y = (int)room.rect.yMax - 2; y >= (int)room.rect.yMin + 1; y--)
                {
                    BuildWallOnTile((int)room.rect.xMax - 1, y);
                    yield return new WaitForSeconds(interval);
                }

                for (int x = (int)room.rect.xMax - 1; x >= (int)room.rect.xMin; x--)
                {
                    BuildWallOnTile(x, (int)room.rect.yMin);
                    yield return new WaitForSeconds(interval);
                }

                for (int y = (int)room.rect.yMin + 1; y < (int)room.rect.yMax - 1; y++)
                {
                    BuildWallOnTile((int)room.rect.xMin, y);
                    yield return new WaitForSeconds(interval);
                }
            }
        }

        private void BuildWallOnTile(int x, int y)
        {
            var tile = GameManager.Instance.tileMap.GetTile(x, y);
            if (null == tile)
            {
                return;
            }

            if (Tile.Type.Wall == tile.type)
            {
                DungeonGizmo.Rect gizmo = GameManager.Instance.Gizmos.GetGroup(GameManager.Gizmo.GroupName.Tile).Get<DungeonGizmo.Rect>(tile.index);
                if (null == gizmo)
                {
                    gizmo = new DungeonGizmo.Rect($"Tile_{tile.index}", Color.white, 1.0f, 1.0f);
                    gizmo.sortingOrder = GameManager.Gizmo.SortingOrder.Wall;
                    gizmo.position = new Vector3(x, y);
                    
                    GameManager.Instance.Gizmos.GetGroup(GameManager.Gizmo.GroupName.Tile).Add(tile.index, gizmo);
                }

                gizmo.color = Color.white;
            }
        }
    }
}