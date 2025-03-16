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
            GameObject gizmoRoot = null;
            if (false == GameManager.Instance.gizmos.TryGetValue(name, out gizmoRoot))
            {
                yield break;
            }

            gizmoRoot.SetActive(enable);
        }
    }
}