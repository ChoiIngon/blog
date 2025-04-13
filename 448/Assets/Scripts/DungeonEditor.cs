#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(NDungeon.Dungeon))]
public class DungeonEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        if (true == GUILayout.Button("Generate Dungeon"))
        {
            NDungeon.Dungeon dungeon = (NDungeon.Dungeon)this.target;
            dungeon.Generate();
        }
    }
}

#endif