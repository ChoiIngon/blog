using System.Collections;
using Unity.Collections.LowLevel.Unsafe;
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

            var gizmo = new DungeonGizmo.Block($"Room_{room.index}", color, room.rect.width, room.rect.height);
            gizmo.position = new Vector3(position.x, position.y, 0.0f);
            gizmo.sortingOrder = GameManager.SortingOrder.Room;

            GameManager.Instance.Gizmos.GetGroup(GameManager.Gizmo.GroupName.Room).Add(room.index, gizmo);

            GameManager.AdjustOrthographicCamera(cameraBoundary);

            DungeonLog.Write($"The room {room.index} has been created");
        }
    }
}