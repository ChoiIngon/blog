using System.ComponentModel;
using UnityEngine;
using UnityEngine.UIElements;

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

    void Update()
    {
        if (null == GameManager.Instance)
        {
            return;
        }

        if (true == Input.GetMouseButtonDown(0))
        {
            Vector3 mousePosition = Input.mousePosition;
            Vector3 worldPosition = Camera.main.ScreenToWorldPoint(mousePosition);

            RaycastHit2D hit = Physics2D.Raycast(worldPosition, transform.forward, 30.0f);
            Debug.DrawRay(worldPosition, transform.forward * 10f, Color.red, 1f);
            if (true == hit && hit.transform.gameObject.tag == "Tile")
            {
                Tile tile = hit.transform.GetComponent<Tile>();
                if (null != tile.block)
                {
                    GameObject block = tile.block;
                    block.transform.SetParent(null);
                    GameObject.Destroy(block);
                }
                else
                {
					tile.CreateBlock();
				}
			}
        }
    }

    public struct ScanDirection
    {
        public int horizontalX; // x ���� ���η� ��� �� ��. 1, 2, 5, 6 �и鿡�� ���.
        public int verticalY;   // y ���� ���η� ��� �� ��. 1, 2, 5, 6 �и鿡�� ���.
        public int horizontalY; // y ���� ���η� ��� �� ��. 3, 4, 7, 8 �и鿡�� ���.
        public int verticalX;   // x ���� ���η� ��� �� ��. 3, 4, 7, 8 �и鿡�� ���.
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
			InitSigntOctant(x, y, radius, scanDirection);
		}
	}

	private void InitSigntOctant(int x, int y, int radius, ScanDirection scanDirection)
	{
		int radiusSquare = radius * radius;

		for (int dy = 1; dy <= radius; dy++)
		{
			// ���� = ���� / ����
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
        if (startSlope <= endSlope)  // Ž�� ��
        {
            return;
        }

        int radiusSquare = radius * radius;
        float nextStartSlope = startSlope;

        for(int dy=row; dy<=radius; dy++)
        {
            bool blockedSection = false;
            // ���� = ���� / ����
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

                if (true == blockedSection)
                {
                    if (null != tile.block)
                    {
                		//GameManager.Instance.CreateSlopeLine(Color.green, new Vector3(sourceX, sourceY, 0), new Vector3(dx + 0.5f * + 0, tileY - 0.5f, 0));
						nextStartSlope = rightSlope;
                        continue;
                    }
                    else
                    {
                        blockedSection = false;
                        startSlope = nextStartSlope;
                    }
                }
                else if (null != tile.block)
                {
					//GameManager.Instance.CreateSlopeLine(Color.green, new Vector3(sourceX, sourceY, 0), new Vector3(tileX - 0.5f, tileY - 0.5f, 0));
					blockedSection = true;
                    nextStartSlope = rightSlope;
					CastLightOctant(x, y, dy + 1, radius, startSlope, leftSlope, scanDirection);
                }
            }

            if (true == blockedSection)
            {
                break;
            }
        }
    }
    public void CrearOctantFov(int sourceX, int sourceY, int radius, ScanDirection scanDirection)
    {
        int radiusSquare = radius * radius;

        for (int dy = 1; dy <= radius; dy++)
        {
            for (int dx = (int)(-1 * (float)dy); dx <= 0; dx++)
            {
                if (dx * dx + dy * dy > radiusSquare)
                {
                    continue;
                }

                int tileX = sourceX + (dx * scanDirection.horizontalX + dy * scanDirection.horizontalY);
                int tileY = sourceY + (dx * scanDirection.verticalX + dy * scanDirection.verticalY);

                Tile tile = GetTile(tileX, tileY);
                if (null == tile)
                {
                    continue;
                }

                tile.CreateBlock();
            }
        }
    }
}
