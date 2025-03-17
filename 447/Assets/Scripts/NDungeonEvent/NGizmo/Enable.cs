using System.Collections;
using UnityEngine;

namespace NDungeonEvent.NGizmo
{
    public class Enable : DungeonEvent
    {
        private string name;
        private bool enable;

        public Enable(string name, bool enable)
        {
            this.name = name;
            this.enable = enable;
        }

        public IEnumerator OnEvent()
        {
            GameManager.Instance.Gizmos.GetGroup(this.name).Enable(enable);
            yield break;
        }
    }
}