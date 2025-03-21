using System.Collections.Generic;
using UnityEngine;

public class ShadowCast
{
    public struct ScanDirection
    {
        public int horizontalX; // x ���� ���η� ��� �� ��. 1, 2, 5, 6 �и鿡�� ���.
        public int verticalY;   // y ���� ���η� ��� �� ��. 1, 2, 5, 6 �и鿡�� ���.
        public int horizontalY; // y ���� ���η� ��� �� ��. 3, 4, 7, 8 �и鿡�� ���.
        public int verticalX;   // x ���� ���η� ��� �� ��. 3, 4, 7, 8 �и鿡�� ���.
    }

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

    private TileMap tileMap;
    public List<Tile> tiles = new List<Tile>();

    public ShadowCast(TileMap tileMap)
    {
        this.tileMap = tileMap;
    }

    public void CastLight(int x, int y, int radius)
    {
        if (0 >= radius)
        {
            return;
        }

        var tile = tileMap.GetTile(x, y);
        tiles.Add(tile);

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

        int radiusSquare = radius * radius;

        float nextStartSlope = startSlope; // ���ŷ ������ ���� �Ǿ��� �� ������ ���� �Ǵ� ���� ����

        // dx(��Ÿ x), dy(��Ÿ y) : ���������� ���� ��ĵ �Ǵ� Ÿ���� ����
        for (int dy = row; dy <= radius; dy++)
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

                var tile = tileMap.GetTile(tileX, tileY);
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

                    if (Tile.Type.Wall == tile.type || (null != tile.dungeonObject && true == tile.dungeonObject.blockLightCast) || (startSlope >= centerSlope && centerSlope >= endSlope))
                    {
                        this.tiles.Add(tile);
                    }
                }

                if (true == blocked)
                {
                    if (Tile.Type.Wall == tile.type || (null != tile.dungeonObject && true == tile.dungeonObject.blockLightCast))
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
                else if (Tile.Type.Wall == tile.type || (null != tile.dungeonObject && true == tile.dungeonObject.blockLightCast))            // ���ŷ ���� ����. ���ο� ���� ���⸦ �����ϰ� ���� �� �Ǵ� ������ ����� ��ĵ ����
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
