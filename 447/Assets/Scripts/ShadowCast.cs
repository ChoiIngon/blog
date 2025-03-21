using System.Collections.Generic;
using UnityEngine;

public class ShadowCast
{
    public struct ScanDirection
    {
        public int horizontalX; // x 축이 가로로 사용 될 때. 1, 2, 5, 6 분면에서 사용.
        public int verticalY;   // y 축이 세로로 사용 될 때. 1, 2, 5, 6 분면에서 사용.
        public int horizontalY; // y 축이 가로로 사용 될 때. 3, 4, 7, 8 분면에서 사용.
        public int verticalX;   // x 축이 세로로 사용 될 때. 3, 4, 7, 8 분면에서 사용.
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

        // 각 8분면 탐색
        foreach (ScanDirection scanDirection in scanDirections)
        {
            CastLightOctant(x, y, 1, radius, 1.0f, 0.0f, scanDirection);
        }
    }

    // 각 분면에 대한 Shadow Casting
    private void CastLightOctant(int x, int y, int row, int radius, float startSlope, float endSlope, ScanDirection scanDirection)
    {
        if (startSlope < endSlope)  // 시작 기울기가 종료 기울기 보다 작은 경우는 탐색 종료
        {
            return;
        }

        int radiusSquare = radius * radius;

        float nextStartSlope = startSlope; // 블록킹 섹션이 종료 되었을 때 새로이 설정 되는 시작 기울기

        // dx(델타 x), dy(델타 y) : 시작점으로 부터 스캔 되는 타일의 변위
        for (int dy = row; dy <= radius; dy++)
        {
            bool blocked = false;

            for (int dx = Mathf.CeilToInt(nextStartSlope * (float)dy); dx >= 0; dx--)
            {
                float leftSlope = (dx + 0.5f) / (dy - 0.5f);
                float rightSlope = (dx - 0.5f) / (dy + 0.5f);

                if (rightSlope > startSlope) // 오른쪽 접점 기울기가 시작 기울기 보다 크다? => 아직 스캔 구역에 들어가지 않았다
                {
                    continue;
                }
                else if (endSlope > leftSlope) // 종료 기울기가 왼쪽 접점 기울기 보다 작다? => 이미 스캔 구역을 지났다.
                {
                    break;
                }

                // 시작점의 상대 좌표에서 실제 맵의 좌표(유니티 좌표)로 변환
                int tileX = x + (-dx * scanDirection.horizontalX + dy * scanDirection.horizontalY);
                int tileY = y + (-dx * scanDirection.verticalX + dy * scanDirection.verticalY);

                var tile = tileMap.GetTile(tileX, tileY);
                if (null == tile)
                {
                    continue;
                }

                // 스캔 되는 타일이 시야 범위 내에 있는지 판단
                // 벡터의 길이 구하기 참고 https://kukuta.tistory.com/152
                // sqrt 연산을 줄이기 위해 radiusSquare 는 미리 radius에 제곱을 구해 놓았다
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
                        startSlope = nextStartSlope;    // 블록킹 섹션이 끝나는 경우 새로운 시작 기울기 설정
                    }
                }
                else if (Tile.Type.Wall == tile.type || (null != tile.dungeonObject && true == tile.dungeonObject.blockLightCast))            // 블록킹 섹션 시작. 새로운 종료 기울기를 설정하고 다음 행 또는 열에서 재귀적 스캔 시작
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
