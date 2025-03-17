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
            DungeonGizmo.Rect gizmo = new DungeonGizmo.Rect($"Tile_{tile.index}", color, width, height);
            gizmo.position = position;
            gizmo.sortingOrder = sortingOrder;

            GameManager.Instance.Gizmos.GetGroup(GameManager.Gizmo.GroupName.Tile).Add(tile.index, gizmo);
            yield return new WaitForSeconds(GameManager.Instance.tickTime / 10);
        }
    }
}