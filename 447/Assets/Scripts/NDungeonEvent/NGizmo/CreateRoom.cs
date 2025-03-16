using System.Collections;
using UnityEngine;

namespace NDungeonEvent.NGizmo
{
    public class CreateRoom : DungeonEvent
    {
        public Room room;
        public Vector3 position;
        public Rect cameraBoundary;
        public Color color;

        public CreateRoom(Room room, Rect cameraBoundary, Color color)
        {
            this.room = room;
            this.position = room.position;
            this.cameraBoundary = cameraBoundary;
            this.color = color;
        }

        public IEnumerator OnEvent()
        {
            if (0 >= room.index)
            {
                yield break;
            }

            GameObject gizmoRoot = null;
            if (false == GameManager.Instance.gizmos.TryGetValue(GameManager.EventName.RoomGizmo, out gizmoRoot))
            {
                gizmoRoot = new GameObject(GameManager.EventName.RoomGizmo);
                gizmoRoot.transform.parent = GameManager.Instance.transform;

                GameManager.Instance.gizmos.Add(GameManager.EventName.RoomGizmo, gizmoRoot);
            }

            var roomGizmo = new DungeonGizmo.Block($"{GameManager.EventName.RoomGizmo}_{room.index}", color, room.rect.width, room.rect.height);
            roomGizmo.parent = gizmoRoot.transform;
            roomGizmo.position = new Vector3(position.x, position.y, 0.0f);
            roomGizmo.sortingOrder = GameManager.SortingOrder.Room;

            GameManager.Instance.roomGizmos.Add(room.index, roomGizmo);

            GameManager.AdjustOrthographicCamera(cameraBoundary);
            Camera.main.transform.position = new Vector3(cameraBoundary.center.x, cameraBoundary.center.y, Camera.main.transform.position.z);

            DungeonLog.Write($"The room {room.index} has been created");
        }
    }
}