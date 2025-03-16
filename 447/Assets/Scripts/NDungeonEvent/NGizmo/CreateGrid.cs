using System.Collections;
using UnityEngine;

namespace NDungeonEvent.NGizmo
{
    public class CreateGrid : DungeonEvent
    {
        private string name;
        private Rect rect;

        public CreateGrid(string name, Rect rect)
        {
            this.name = name;
            this.rect = rect;
        }

        public IEnumerator OnEvent()
        {
            GameObject gizmoRoot = new GameObject(name);
            gizmoRoot.transform.parent = GameManager.Instance.transform;
            GameManager.Instance.gizmos.Add(name, gizmoRoot);

            DungeonGizmo.Grid gizmo = new DungeonGizmo.Grid(name, (int)rect.width, (int)rect.height);
            gizmo.gameObject.transform.parent = gizmoRoot.transform;
            yield break;
        }
    }
}