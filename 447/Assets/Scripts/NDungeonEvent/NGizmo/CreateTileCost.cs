using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NDungeonEvent.NGizmo
{
    public class CreateTileCost : DungeonEvent
    {
        private List<Tile> tiles;

        public CreateTileCost(List<Tile> tiles)
        {
            this.tiles = tiles;
        }

        public IEnumerator OnEvent()
        {
            if (null == tiles)
            {
                yield break;
            }

            float interval = GameManager.Instance.tickTime / tiles.Count;
            for (int i = 0; i < tiles.Count; i++)
            {
                Tile tile = tiles[i];

                DungeonGizmo.Rect gizmo = new DungeonGizmo.Rect($"TileCost_{tile.index}", Color.white, tile.rect.width, tile.rect.height);
                gizmo.sortingOrder = GameManager.Gizmo.SortingOrder.TileCost;
                gizmo.position = new Vector3(tile.rect.x, tile.rect.y);

                GameManager.Instance.Gizmos.GetGroup(GameManager.Gizmo.GroupName.TileCost).Add(gizmo);
                yield return new WaitForSeconds(interval);
            }
        }
    }
}