using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NDungeonEvent.NGizmo
{
    public class RepositionRoom : DungeonEvent
    {
        private struct Snapshot
        {
            public int index;
            public Vector3 position;
        }

        private List<Snapshot> snapshots = new List<Snapshot>();
        private Rect cameraBoundary;

        public RepositionRoom(List<Room> rooms)
        {
            this.snapshots = new List<Snapshot>();
            foreach (Room room in rooms)
            {
                this.snapshots.Add(new Snapshot() { index = room.index, position = room.position });
            }

            this.cameraBoundary = DungeonTileMapGenerator.GetBoundaryRect(rooms);
        }

        public IEnumerator OnEvent()
        {
            GameObject gizmoRoot = null;
            if (false == GameManager.Instance.gizmos.TryGetValue(GameManager.EventName.RoomGizmo, out gizmoRoot))
            {
                yield break;
            }

            foreach (Snapshot data in this.snapshots)
            {
                DungeonGizmo.Block gizmo;
                if (false == GameManager.Instance.roomGizmos.TryGetValue(data.index, out gizmo))
                {
                    continue;
                }

                if (gizmo.position == data.position)
                {
                    continue;
                }

                float interpolation = 0.0f;
                Vector3 start = gizmo.position;
                while (1.0f > interpolation)
                {
                    interpolation += Time.deltaTime / GameManager.Instance.tickTime;
                    gizmo.position = Vector3.Lerp(start, data.position, interpolation);
                    yield return null;
                }

                gizmo.position = data.position;
            }

            GameManager.AdjustOrthographicCamera(cameraBoundary);
            Camera.main.transform.position = new Vector3(cameraBoundary.center.x, cameraBoundary.center.y, Camera.main.transform.position.z);
        }
    }
}