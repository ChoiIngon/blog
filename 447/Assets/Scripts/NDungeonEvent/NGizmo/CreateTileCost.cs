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

            GameObject gizmoRoot = null;
            if (false == GameManager.Instance.gizmos.TryGetValue(GameManager.EventName.TileCostGizmo, out gizmoRoot))
            {
                gizmoRoot = new GameObject(GameManager.EventName.TileCostGizmo);
                gizmoRoot.transform.parent = GameManager.Instance.transform;

                GameManager.Instance.gizmos.Add(GameManager.EventName.TileCostGizmo, gizmoRoot);
            }

            float interval = GameManager.Instance.tickTime / tiles.Count;
            for (int i = 0; i < tiles.Count; i++)
            {
                Tile tile = tiles[i];

                DungeonGizmo.Rect gizmo = new DungeonGizmo.Rect($"TileCost_{tile.index}", Color.white, tile.rect.width, tile.rect.height);
                gizmo.parent = gizmoRoot.transform;
                gizmo.sortingOrder = GameManager.SortingOrder.Corridor;
                gizmo.position = new Vector3(tile.rect.x, tile.rect.y);
                yield return new WaitForSeconds(interval);
            }
        }
    }
}