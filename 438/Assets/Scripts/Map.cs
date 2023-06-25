using UnityEngine;

public class Map : MonoBehaviour
{
    public int width;
    public int height;

    private Tile[] tiles;

    public void Init()
    {
        tiles = new Tile[width * height];
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                GameObject obj = Instantiate(GameManager.Instance.tilePrefab, new Vector3(x, y, 0), Quaternion.identity);
                Tile tile = obj.GetComponent<Tile>();
                tile.Init(this.transform, x, y);
                tiles[y * width + x] = tile;
            }
        }
    }

    public Tile GetTile(int x, int y)
    {
        if (0 > x || x >= width || 0 > y || y >= height)
        {
            return null;
        }

        return tiles[y * width + x];
    }

    public struct ScanDirection
    {
        public int horizontalX; // x 축이 가로로 사용 될 때. 1, 2, 5, 6 분면에서 사용.
        public int verticalY;   // y 축이 세로로 사용 될 때. 1, 2, 5, 6 분면에서 사용.
        public int horizontalY; // y 축이 가로로 사용 될 때. 3, 4, 7, 8 분면에서 사용.
        public int verticalX;   // x 축이 세로로 사용 될 때. 3, 4, 7, 8 분면에서 사용.
    };

    public readonly static ScanDirection[] scanDirections =
    {
        new ScanDirection { horizontalX = 1, verticalY = 1, horizontalY = 0, verticalX = 0 },
        new ScanDirection { horizontalX =-1, verticalY = 1, horizontalY = 0, verticalX = 0 },
        new ScanDirection { horizontalX = 0, verticalY = 0, horizontalY = 1, verticalX = 1 },
        new ScanDirection { horizontalX = 0, verticalY = 0, horizontalY =-1, verticalX = 1 },
        new ScanDirection { horizontalX =-1, verticalY =-1, horizontalY = 0, verticalX = 0 },
        new ScanDirection { horizontalX = 1, verticalY =-1, horizontalY = 0, verticalX = 0 },
        new ScanDirection { horizontalX = 0, verticalY = 0, horizontalY = 1, verticalX =-1 },
        new ScanDirection { horizontalX = 0, verticalY = 0, horizontalY =-1, verticalX =-1 },
    };

    public void InitSight(int x, int y, int radius)
    {
		foreach (ScanDirection scanDirection in scanDirections)
		{
			InitSightOctant(x, y, radius, scanDirection);
		}
	}

	private void InitSightOctant(int x, int y, int radius, ScanDirection scanDirection)
	{
		int radiusSquare = radius * radius;

		for (int dy = 1; dy <= radius; dy++)
		{
			// 기울기 = 가로 / 세로
			for (int dx = dy; dx >= 0; dx--)
			{
				int tileX = x + (-dx * scanDirection.horizontalX + dy * scanDirection.horizontalY);
				int tileY = y + (-dx * scanDirection.verticalX + dy * scanDirection.verticalY);

				Tile tile = GetTile(tileX, tileY);
				if (null == tile)
				{
					continue;
				}

				if (dx * dx + dy * dy < radiusSquare)
				{
					tile.SetVisible(false);
				}
			}
		}
	}

	public void CastLight(int x, int y, int radius)
    {
        foreach (ScanDirection scanDirection in scanDirections)
        {
            CastLightOctant(x, y, 1, radius, 1.0f, 0.0f, scanDirection);
        }
    }

    private void CastLightOctant(int x, int y, int row, int radius, float startSlope, float endSlope, ScanDirection scanDirection)
    {
        if (startSlope <= endSlope)  // 탐색 끝
        {
            return;
        }

        int radiusSquare = radius * radius;
        float nextStartSlope = startSlope;

        for(int dy=row; dy<=radius; dy++)
        {
            bool blocked = false;
            // 기울기 = 가로 / 세로
            for (int dx = (int)(nextStartSlope * (float)dy); dx >= 0; dx--)
            {
                float leftSlope = (float)(dx + 0.5f) / (float)(dy - 0.5f);
                float rightSlope = (float)(dx - 0.5f) / (float)(dy + 0.5f);

                if (startSlope < rightSlope)
                {
                    continue;
                }
                else if (endSlope > leftSlope)
                {
                    break;
                }

                int tileX = x + (-dx * scanDirection.horizontalX + dy * scanDirection.horizontalY);
                int tileY = y + (-dx * scanDirection.verticalX + dy * scanDirection.verticalY);

                Tile tile = GetTile(tileX, tileY);
                if (null == tile)
                {
                    continue;
                }

                if (dx * dx + dy * dy < radiusSquare)
                {
                    tile.SetVisible(true);
                }

                if (true == blocked)
                {
                    if (null != tile.block)
                    {
						nextStartSlope = rightSlope;
                        continue;
                    }
                    else
                    {
                        blocked = false;
                        startSlope = nextStartSlope;
                    }
                }
                else if (null != tile.block)
                {
					blocked = true;
                    nextStartSlope = rightSlope;
					CastLightOctant(x, y, dy + 1, radius, startSlope, leftSlope, scanDirection);
                }
            }

            if (true == blocked)
            {
                return;
            }
        }
    }
}
