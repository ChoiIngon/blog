using System.Collections;
using UnityEngine;

namespace NDungeonEvent.NGizmo
{
    public class DestroyGizmo : DungeonEvent
    {
        private string groupName;
        private int index;

        public DestroyGizmo(string groupName, int index = 0)
        {
            this.groupName = groupName;
            this.index = index;
        }

        public IEnumerator OnEvent()
        {
            if (0 == index)
            {
                GameManager.Instance.Gizmos.DestroyGroup(groupName);
            }
            else
            {
                GameManager.Instance.Gizmos.GetGroup(groupName).Remove(index);
            }

            yield return new WaitForSeconds(GameManager.Instance.tickTime);
        }
    }
}