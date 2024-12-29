using System.Collections.Generic;
using UnityEngine;

namespace Data
{
    public class TileMap
    {
        private Tile[] tiles;
        public Rect rect;

        public int width
        {
            get { return (int)rect.width; }
        }

        public int height
        {
            get { return (int)rect.height; }
        }

        public TileMap(List<Block> blocks)
        {
            rect = new Rect();
            rect.xMin = int.MaxValue;
            rect.xMax = int.MinValue;
            rect.yMin = int.MaxValue;
            rect.yMax = int.MinValue;
            foreach (Block block in blocks)
            {
                rect.xMin = Mathf.Min(rect.xMin, block.rect.xMin);
                rect.xMax = Mathf.Max(rect.xMax, block.rect.xMax);
                rect.yMin = Mathf.Min(rect.yMin, block.rect.yMin);
                rect.yMax = Mathf.Max(rect.yMax, block.rect.yMax);
            }

            tiles = new Tile[width * height];
            // ��ü Ÿ�� �ʱ�ȭ
            for (int i = 0; i < width * height; i++)
            {
                Tile tile = new Tile();
                tile.index = i;
                tile.rect = new Rect(i % width, i / width, 1, 1);
                tile.type = Tile.Type.None;
                tile.cost = Tile.PathCost.Default;
                tiles[i] = tile;
            }

            foreach (Block block in blocks)
            {
                // ��ϵ��� (0, 0) �������� �ű�
                block.rect.x -= rect.xMin;
                block.rect.y -= rect.yMin;

                { // ��� Ÿ���� cost�� ���� ��� ���ַ� ���� ���鵵�� ��
                    for (int y = (int)block.rect.yMin; y < (int)block.rect.yMax; y++)
                    {
                        for (int x = (int)block.rect.xMin; x < (int)block.rect.xMax; x++)
                        {
                            Tile tile = GetTile(x, y);
                            tile.cost = Tile.PathCost.Floor;
                        }
                    }
                }

                { // ����� ���� cost�� ���� ���� ���� ���� �ȵ��� ����
                    for (int x = (int)block.rect.xMin; x < (int)block.rect.xMax; x++)
                    {
                        Tile outofTop = GetTile(x, (int)block.rect.yMax);
                        if (null != outofTop)
                        {
                            outofTop.cost = Tile.PathCost.Wall;
                        }

                        Tile top = GetTile(x, (int)block.rect.yMax - 1);
                        top.cost = Tile.PathCost.Wall;

                        Tile bottom = GetTile(x, (int)block.rect.yMin);
                        bottom.cost = Tile.PathCost.Wall;

                        Tile outofBottom = GetTile(x, (int)block.rect.yMin -1);
                        if (null != outofBottom)
                        {
                            outofBottom.cost = Tile.PathCost.Wall;
                        }
                    }

                    for (int y = (int)block.rect.yMin; y < (int)block.rect.yMax; y++)
                    {
                        Tile outOfLeft = GetTile((int)block.rect.xMin - 1, y);
                        if (null != outOfLeft)
                        {
                            outOfLeft.cost = Tile.PathCost.Wall;
                        }

                        Tile left = GetTile((int)block.rect.xMin, y);
                        left.cost = Tile.PathCost.Wall;

                        Tile right = GetTile((int)block.rect.xMax - 1, y);
                        right.cost = Tile.PathCost.Wall;

                        Tile outOfRight = GetTile((int)block.rect.xMax, y);
                        if (null != outOfRight)
                        {
                            outOfRight.cost = Tile.PathCost.Wall;
                        }
                    }
                }

                if(Block.Type.Corridor == block.type)
                { // ����� ����� cost�� ���� ����� ����� ���� ������ ����
                    for (int x = (int)block.rect.xMin + 1; x < (int)block.rect.xMax - 1; x++)
                    {
                        Tile tile = GetTile(x, (int)block.rect.center.y);
                        tile.cost = Tile.PathCost.Corridor;
                    }

                    for (int y = (int)block.rect.yMin + 1; y < (int)block.rect.yMax - 1; y++)
                    {
                        Tile tile = GetTile((int)block.rect.center.x, y);
                        tile.cost = Tile.PathCost.Corridor;
                    }
                }

                // �� ����� ��� �濡 ���� �ʱ�ȭ
                if (Block.Type.Room == block.type)
                {
                    for (int y = (int)block.rect.yMin; y < (int)block.rect.yMax; y++)
                    {
                        for (int x = (int)block.rect.xMin; x < (int)block.rect.xMax; x++)
                        {
                            Tile tile = GetTile(x, y);
                            tile.type = Tile.Type.Floor;
                        }
                    }

                    // ���� �𼭸��� ���� ����� ���� �����ϱ� ���� ��� �� ��տ� ���� �̸� ����
                    int xMin = (int)block.rect.xMin;
                    int yMin = (int)block.rect.yMin;
                    int xMax = (int)block.rect.xMax;
                    int yMax = (int)block.rect.yMax;

                    // left bottom
                    GetTile(xMin, yMin + 1).type = Tile.Type.Wall;
                    GetTile(xMin, yMin).type = Tile.Type.Wall;
                    GetTile(xMin + 1, yMin).type = Tile.Type.Wall;

                    GetTile(xMax - 1, yMin + 1).type = Tile.Type.Wall;
                    GetTile(xMax - 1, yMin).type = Tile.Type.Wall;
                    GetTile(xMax - 2, yMin).type = Tile.Type.Wall;

                    GetTile(xMin, yMax - 2).type = Tile.Type.Wall;
                    GetTile(xMin, yMax - 1).type = Tile.Type.Wall;
                    GetTile(xMin + 1, yMax - 1).type = Tile.Type.Wall;

                    GetTile(xMax - 1, yMax - 2).type = Tile.Type.Wall;
                    GetTile(xMax - 1, yMax - 1).type = Tile.Type.Wall;
                    GetTile(xMax - 2, yMax - 1).type = Tile.Type.Wall;
                }
            }

            rect.x = 0;
            rect.y = 0;
        }

        public Tile GetTile(int index)
        {
            if (0 > index || index >= tiles.Length)
            {
                return null;
            }

            return tiles[index];
        }

        public Tile GetTile(int x, int y)
        {
            if (0 > x || x >= rect.width)
            {
                return null;
            }

            if (0 > y || y >= height)
            {
                return null;
            }

            return tiles[y * width + x];
        }
    }
}