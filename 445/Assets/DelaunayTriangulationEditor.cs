using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(DelaunayTriangulation))]
public class DelaunayTriangulationEditor : Editor
{
    public bool showCircle = true;

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        DelaunayTriangulation delaunay = (DelaunayTriangulation)this.target;
        if (true == GUILayout.Button("Reset"))
        {
            delaunay.Init(10, 10);
        }

        if (true == GUILayout.Button("Toggle Circle"))
        {
            showCircle = !showCircle;
            delaunay.ActivateCircle(showCircle);
        }

        if (true == GUILayout.Button("RemoveSuperTriangle"))
        {
            delaunay.RemoveSuperTriangle();
        }
    }
}