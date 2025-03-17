using System.Collections;
using UnityEngine;

namespace NDungeonEvent.NGizmo
{
    public class CreateTriangle : DungeonEvent
    {
        private DelaunayTriangulation triangulation;
        private DelaunayTriangulation.Circle biggestCircle;

        public CreateTriangle(DelaunayTriangulation triangulation, DelaunayTriangulation.Circle biggestCircle)
        {
            this.triangulation = triangulation;
            this.biggestCircle = biggestCircle;
        }

        public IEnumerator OnEvent()
        {
            var group = GameManager.Instance.Gizmos.GetGroup(GameManager.Gizmo.GroupName.Triangle);

            int index = 1;
            float interval = GameManager.Instance.tickTime / triangulation.triangles.Count;
            foreach (var triangle in triangulation.triangles)
            {
                DungeonGizmo.Line line_ab = new DungeonGizmo.Line($"Triangle_{index}_ab", Color.green, triangle.a, triangle.b, 0.1f);
                line_ab.sortingOrder = GameManager.SortingOrder.TriangleLine;
                group.Add(line_ab);

                DungeonGizmo.Line line_ac = new DungeonGizmo.Line($"Triangle_{index}_ac", Color.green, triangle.a, triangle.c, 0.1f);
                line_ac.sortingOrder = GameManager.SortingOrder.TriangleLine;
                group.Add(line_ac);

                DungeonGizmo.Line line_bc = new DungeonGizmo.Line($"Triangle_{index}_bc", Color.green, triangle.b, triangle.c, 0.1f);
                line_bc.sortingOrder = GameManager.SortingOrder.TriangleLine;
                group.Add(line_bc);

                DungeonGizmo.Circle innerCircle = new DungeonGizmo.Circle($"Triangle_{index}_InnerCircle", Color.green, triangle.innerCircle.radius, 0.1f);
                innerCircle.position = triangle.innerCircle.center;
                innerCircle.sortingOrder = GameManager.SortingOrder.TriangleInnerCircle;
                group.Add(innerCircle);

                yield return new WaitForSeconds(interval);
            }

            DungeonGizmo.Circle circle = new DungeonGizmo.Circle($"Biggest_{index}_InnerCircle", Color.red, biggestCircle.radius, 0.5f);
            circle.position = biggestCircle.center;
            circle.sortingOrder = GameManager.SortingOrder.BiggestCircle;
            group.Add(circle);
            yield return new WaitForSeconds(GameManager.Instance.tickTime);
            group.Clear();
        }
    }
}