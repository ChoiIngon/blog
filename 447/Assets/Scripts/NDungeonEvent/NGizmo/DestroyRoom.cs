using System.Collections;
using UnityEngine;

namespace NDungeonEvent.NGizmo
{
    public class DestroyRoom : DungeonEvent
    {
        private int index;

        public DestroyRoom(int index)
        {
            this.index = index;
        }

        public IEnumerator OnEvent()
        {
            DungeonGizmo.Block gizmo;
            if (false == GameManager.Instance.roomGizmos.TryGetValue(index, out gizmo))
            {
                yield break;
            }

            gizmo.parent = null;
            DungeonGizmo.Destroy(gizmo);
            GameManager.Instance.roomGizmos.Remove(index);
            yield return new WaitForSeconds(GameManager.Instance.tickTime);
        }
    }
}