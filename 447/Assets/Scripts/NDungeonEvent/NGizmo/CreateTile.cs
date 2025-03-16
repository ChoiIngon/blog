using System.Collections;
using UnityEngine;

namespace NDungeonEvent.NGizmo
{
    public class CreateTile : DungeonEvent
    {
        private Tile tile;
        private Vector3 position;
        private Color color;
        private float width;
        private float height;
        private int sortingOrder;

        public CreateTile(Tile tile, Color color, int sortingOrder)
        {
            this.tile = tile;
            this.position = new Vector3(tile.rect.x, tile.rect.y);
            this.color = color;
            this.width = tile.rect.width;
            this.height = tile.rect.height;
            this.sortingOrder = sortingOrder;
        }

        public IEnumerator OnEvent()
        {
            GameObject gizmoRoot = null;
            if (false == GameManager.Instance.gizmos.TryGetValue(GameManager.EventName.TileGizmo, out gizmoRoot))
            {
                gizmoRoot = new GameObject(GameManager.EventName.TileGizmo);
                gizmoRoot.transform.parent = GameManager.Instance.transform;

                GameManager.Instance.gizmos.Add(GameManager.EventName.TileGizmo, gizmoRoot);
            }

            DungeonGizmo.Rect gizmo = null;
            if (false == GameManager.Instance.tileGizmos.TryGetValue(tile.index, out gizmo))
            {
                gizmo = new DungeonGizmo.Rect($"Tile_{tile.index}", color, width, height);
                GameManager.Instance.tileGizmos.Add(tile.index, gizmo);
            }

            gizmo.parent = gizmoRoot.transform;
            gizmo.position = position;
            gizmo.color = color;
            gizmo.sortingOrder = sortingOrder;

            yield return new WaitForSeconds(GameManager.Instance.tickTime / 10);
        }
    }
}