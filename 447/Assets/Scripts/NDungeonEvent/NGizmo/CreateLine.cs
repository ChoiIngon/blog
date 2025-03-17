using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NDungeonEvent.NGizmo
{
    public class CreateLine : DungeonEvent
    {
        public struct Line
        {
            public Vector3 start;
            public Vector3 end;
        }

        private string name;
        private List<Line> lines;
        private Color color;
        private int sortingOrder;
        private float width;

        public CreateLine(string name, List<Line> lines, Color color, int sortingOrder, float width)
        {
            this.name = name;
            this.lines = lines;
            this.color = color;
            this.sortingOrder = sortingOrder;
            this.width = width;
        }

        public IEnumerator OnEvent()
        {
            if (null == lines)
            {
                yield break;
            }

            float interval = GameManager.Instance.tickTime / lines.Count;
            for (int i = 0; i < lines.Count; i++)
            {
                Line line = lines[i];
                string gizmoName = $"{name}_{i}_({line.start.x},{line.start.y}) -> ({line.end.x},{line.end.y})";

                DungeonGizmo.Line gizmo = new DungeonGizmo.Line(gizmoName, color, line.start, line.end, width);
                gizmo.sortingOrder = sortingOrder;

                GameManager.Instance.Gizmos.GetGroup(name).Add(gizmo);
                
                yield return new WaitForSeconds(interval);
            }
        }
    }
}