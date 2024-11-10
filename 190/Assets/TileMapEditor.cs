using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(TileMap))]
public class TileMapEditor : Editor
{
	public override void OnInspectorGUI()
	{
		base.OnInspectorGUI();

		if (true == GUILayout.Button("Generate Tile Map"))
		{
			TileMap.GetInstance().CreateTiles();
        }

		if (true == GUILayout.Button("Set Wall"))
		{
			Tile tile = TileMap.GetInstance().select;
			if (null == tile)
			{
				return;
			}

			if (Tile.TileType.Wall != tile.type)
			{
				tile.Init(tile.index, Tile.TileType.Wall);
			}
			else
			{
				tile.Init(tile.index, Tile.TileType.Floor);
			}
			TileMap.GetInstance().select = null;
        }

		if (true == GUILayout.Button("Set \'from\'"))
		{
            if (null == TileMap.GetInstance().select)
            {
                return;
            }

			TileMap.GetInstance().Clear();

            Tile from = TileMap.GetInstance().from;
			if (null != from)
			{
				from.SetColor(Tile.ColorType.Floor);
			}

            TileMap.GetInstance().from = TileMap.GetInstance().select;
            TileMap.GetInstance().from.SetColor(Tile.ColorType.From);
            TileMap.GetInstance().select = null;
		}

		if (true == GUILayout.Button("Set \'to\'"))
		{
            if (null == TileMap.GetInstance().select)
            {
                return;
            }

            TileMap.GetInstance().Clear();

            Tile to = TileMap.GetInstance().to;
			if (null != to)
			{
				to.SetColor(Tile.ColorType.Floor);
			}

            TileMap.GetInstance().to = TileMap.GetInstance().select;
            TileMap.GetInstance().to.SetColor(Tile.ColorType.To);
            TileMap.GetInstance().select = null;
        }

		if (null != TileMap.GetInstance().from && null != TileMap.GetInstance().to)
		{
			if (true == GUILayout.Button("Find Path"))
			{
				TileMap.GetInstance().FindPath();
			}

			if (null != TileMap.GetInstance().recorder)
			{
				if (true == GUILayout.Button("Simulate Path Finding"))
				{
					if (null == TileMap.GetInstance().recorder)
					{
						return;
					}

					TileMap.GetInstance().recorder.Next();
				}
			}
		}
    }
}
