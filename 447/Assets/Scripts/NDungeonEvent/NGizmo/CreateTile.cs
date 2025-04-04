using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NDungeonEvent.NGizmo
{
	public class CreateTile : DungeonEvent
    {
        private struct Snapshot
        {
			public int index;
			public Vector3 position;
			public float width;
			public float height;
		}

        private string groupName;
        private List<Snapshot> snapshots = new List<Snapshot>();
        private Color color;
        private int sortingOrder;

        public CreateTile(string groupName, Tile tile, Color color, int sortingOrder)
        {
            this.groupName = groupName;
            Snapshot snapshot = new Snapshot() { index = tile.index, position = new Vector3(tile.rect.x, tile.rect.y), width = tile.rect.width, height = tile.rect.height };
			this.snapshots.Add(snapshot);
			this.color = color;
            this.sortingOrder = sortingOrder;
        }

		public CreateTile(string groupName, List<Tile> tiles, Color color, int sortingOrder)
		{
			this.groupName = groupName;
			foreach (var tile in tiles)
            {
                Snapshot snapshot = new Snapshot() { index = tile.index, position = new Vector3(tile.rect.x, tile.rect.y), width = tile.rect.width, height = tile.rect.height };
                snapshots.Add(snapshot);
            }
			this.color = color;
			this.sortingOrder = sortingOrder;
		}

		public IEnumerator OnEvent()
        {
            float interval = GameManager.Instance.tickTime / snapshots.Count;

			foreach (var snapshot in snapshots)
            {
                DungeonGizmo.Rect gizmo = new DungeonGizmo.Rect($"Tile_{snapshot.index}", color, snapshot.width, snapshot.height);
                gizmo.position = snapshot.position;
                gizmo.sortingOrder = sortingOrder;
                GameManager.Instance.Gizmos.GetGroup(groupName).Add(snapshot.index, gizmo);

				yield return new WaitForSeconds(interval);
			}
            
        }
    }
}