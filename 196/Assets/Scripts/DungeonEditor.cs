using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Dungeon))]
public class DungeonEditor : Editor
{
	public override void OnInspectorGUI()
	{
		base.OnInspectorGUI();
        Dungeon dungeon = (Dungeon)target;
		if (true == GUILayout.Button("Create Rooms"))
		{
			dungeon.CreateRooms();
		}
		if (true == GUILayout.Button("Delaunay Triangulation"))
		{
			dungeon.DelaunayTriangulation();
		}
        if (true == GUILayout.Button("MinimumSpanningTree"))
        {
            dungeon.MinimumSpanningTree();
        }
        if (true == GUILayout.Button("Astar Path Finding"))
		{
			dungeon.AstarPathFinding();
		}
        if (true == GUILayout.Button("Build Wall"))
        {
            dungeon.BuildWall();
        }
        if (true == GUILayout.Button("ClearAll"))
		{
			dungeon.Clear();
		}

		if (true == GUI.changed)
		{
			dungeon.EnableGizmo();
		}
    }
}
