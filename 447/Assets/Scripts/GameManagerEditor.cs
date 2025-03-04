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
            GameManager gameManger = (GameManager)this.target;
            gameManger.CreateDungeon();
        }

        if (true == GUI.changed)
        {
            if (null == GameManager.Instance.tileMap)
            {
                return;
            }

        }
    }
}

#endif