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

    public void CastLight(int x, int y, int radius)
    {
        if (0 >= radius)
        {
            return;
        }

        Tile tile = GetTile(x, y);
        tile.SetVisible(true); // 탐색 시작점. visible 상태로 셋팅

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
/*
#if UNITY_EDITOR
        {   // 디버깅을 위해 시각적인 기울기 선이 필요해서 작성한 코드.
            // 다시 쓸 일이 있을까 싶어 남겨 놓았지만 그럴 일은 없을것 같다. 무시하도록 하자.
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

        float nextStartSlope = startSlope; // 블록킹 섹션이 종료 되었을 때 새로이 설정 되는 시작 기울기

        // dx(델타 x), dy(델타 y) : 시작점으로 부터 스캔 되는 타일의 변위
        for(int dy=row; dy<=radius; dy++)
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

                Tile tile = GetTile(tileX, tileY);
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
                        startSlope = nextStartSlope;    // 블록킹 섹션이 끝나는 경우 새로운 시작 기울기 설정
                    }
                }
                else if (null != tile.block)            // 블록킹 섹션 시작. 새로운 종료 기울기를 설정하고 다음 행 또는 열에서 재귀적 스캔 시작
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

    // 시야 초기화
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

    // 8분면에 대한 시야 초기화
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
}
