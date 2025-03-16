using System.Collections;
using UnityEngine;

namespace NDungeonEvent.NGizmo
{
    public class ChangeRoomColor : DungeonEvent
    {
        public Room room;
        public Color color;

        public ChangeRoomColor(Room room, Color color)
        {
            this.room = room;
            this.color = color;
        }

        public IEnumerator OnEvent()
        {
            DungeonGizmo.Block gizmo;
            if (false == GameManager.Instance.roomGizmos.TryGetValue(room.index, out gizmo))
            {
                yield break;
            }

            gizmo.color = color;
            yield return new WaitForSeconds(GameManager.Instance.tickTime);
        }
    }
}