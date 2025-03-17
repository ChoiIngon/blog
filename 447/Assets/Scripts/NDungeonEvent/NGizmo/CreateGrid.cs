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
            DungeonGizmo.Grid gizmo = new DungeonGizmo.Grid(name, (int)rect.width, (int)rect.height);
            GameManager.Instance.Gizmos.GetGroup(name).Add(gizmo);
            yield break;
        }
    }
}