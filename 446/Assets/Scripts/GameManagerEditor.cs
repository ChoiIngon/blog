#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GameManager))]
public class GameManagerEditor : Editor
{
	public override void OnInspectorGUI()
	{
		base.OnInspectorGUI();
		if (true == GUILayout.Button("Create Dungeon"))
		{
            GameManager.Instance.CreateDungeon();
        }

        if (true == GUILayout.Button("ClearAll"))
		{
            GameManager.Instance.Clear();
		}

        if (true == GUI.changed)
        {
            if (null != GameManager.Instance.dungeon)
            {
                GameManager.Instance.dungeon.EnableGizmo();
            }
        }
    }
}

#endif