using UnityEngine;

public class Map : MonoBehaviour
{
    private int width;
    private int height;
    private Tile[] tiles;

    public void Init(int width, int height)
    {
        this.width = width;
        this.height = height;

        tiles = new Tile[width * height];

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                Tile tile = Instantiate(GameManager.Instance.tilePrefab, new Vector3(x, y, 0), Quaternion.identity);
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

    public void CastLight(int x, int y, int radius)
    {
        if (0 >= radius)
        {
            return;
        }

        Tile tile = GetTile(x, y);
        tile.SetVisible(true); // Ž�� ������. visible ���·� ����

        // �� 8�и� Ž��
        foreach (ScanDirection scanDirection in scanDirections)
        {
            CastLightOctant(x, y, 1, radius, 1.0f, 0.0f, scanDirection);
        }
    }

    // �� �и鿡 ���� Shadow Casting
    private void CastLightOctant(int x, int y, int row, int radius, float startSlope, float endSlope, ScanDirection scanDirection)
    {
        if (startSlope < endSlope)  // ���� ���Ⱑ ���� ���� ���� ���� ���� Ž�� ����
        {
            return;
        }
/*
#if UNITY_EDITOR
        {   // ������� ���� �ð����� ���� ���� �ʿ��ؼ� �ۼ��� �ڵ�.
            // �ٽ� �� ���� ������ �;� ���� �������� �׷� ���� ������ ����. �����ϵ��� ����.
            Vector3 start = new Vector3(x, y, 0.0f);
            {
                Vector3 vEndSlope = new Vector3(-scanDirection.horizontalX * startSlope * y, scanDirection.verticalY * y, 0.0f);
                vEndSlope.Normalize();
                Vector3 end = start + vEndSlope * radius;
                GameManager.Instance.CreateSlopeLine(new Color(1.0f, startSlope, endSlope), start, end);
            }

            {
                Vector3 vEndSlope = new Vector3(-scanDirection.horizontalX * endSlope * y, scanDirection.verticalY * y, 0.0f);
                vEndSlope.Normalize();
                Vector3 end = start + vEndSlope * radius;
                GameManager.Instance.CreateSlopeLine(new Color(1.0f, startSlope, endSlope), start, end);
            }
        }
#endif
*/
        int radiusSquare = radius * radius;

        float nextStartSlope = startSlope; // ���ŷ ������ ���� �Ǿ��� �� ������ ���� �Ǵ� ���� ����

        // dx(��Ÿ x), dy(��Ÿ y) : ���������� ���� ��ĵ �Ǵ� Ÿ���� ����
        for(int dy=row; dy<=radius; dy++)
        {
            bool blocked = false;

            for (int dx = Mathf.CeilToInt(nextStartSlope * (float)dy); dx >= 0; dx--)
            {
                float leftSlope = (dx + 0.5f) / (dy - 0.5f);
                float rightSlope = (dx - 0.5f) / (dy + 0.5f);

                if (rightSlope > startSlope) // ������ ���� ���Ⱑ ���� ���� ���� ũ��? => ���� ��ĵ ������ ���� �ʾҴ�
                {
                    continue;
                }
                else if (endSlope > leftSlope) // ���� ���Ⱑ ���� ���� ���� ���� �۴�? => �̹� ��ĵ ������ ������.
                {
                    break;
                }

                // �������� ��� ��ǥ���� ���� ���� ��ǥ(����Ƽ ��ǥ)�� ��ȯ
                int tileX = x + (-dx * scanDirection.horizontalX + dy * scanDirection.horizontalY);
                int tileY = y + (-dx * scanDirection.verticalX + dy * scanDirection.verticalY);

                Tile tile = GetTile(tileX, tileY);
                if (null == tile)
                {
                    continue;
                }

                // ��ĵ �Ǵ� Ÿ���� �þ� ���� ���� �ִ��� �Ǵ�
                // ������ ���� ���ϱ� ���� https://kukuta.tistory.com/152
                // sqrt ������ ���̱� ���� radiusSquare �� �̸� radius�� ������ ���� ���Ҵ�
                if (dx * dx + dy * dy < radiusSquare)
                {
					float centerSlope = (float)dx / (float)dy;

					if (startSlope >= centerSlope && centerSlope >= endSlope)
                    {
                        tile.SetVisible(true);
                    }
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
                        startSlope = nextStartSlope;    // ���ŷ ������ ������ ��� ���ο� ���� ���� ����
                    }
                }
                else if (null != tile.block)            // ���ŷ ���� ����. ���ο� ���� ���⸦ �����ϰ� ���� �� �Ǵ� ������ ����� ��ĵ ����
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

    // �þ� �ʱ�ȭ
    public void InitSight(int x, int y, int radius)
    {
        if (0 >= radius)
        {
            return;
        }

        Tile tile = GetTile(x, y);
        tile.SetVisible(false);

        foreach (ScanDirection scanDirection in scanDirections)
        {
            InitSightOctant(x, y, radius, scanDirection);
        }
    }

    // 8�и鿡 ���� �þ� �ʱ�ȭ
    private void InitSightOctant(int x, int y, int radius, ScanDirection scanDirection)
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
}
