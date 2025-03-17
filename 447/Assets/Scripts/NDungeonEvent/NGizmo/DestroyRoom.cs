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
            GameManager.Instance.Gizmos.GetGroup(GameManager.Gizmo.GroupName.Room).Remove(index);
            yield return new WaitForSeconds(GameManager.Instance.tickTime);
        }
    }
}